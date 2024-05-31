using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public string player1Name;
    public string player2Name;
    public string currentDefender;
    public string opposingPlayer;
    public enum PlayerRole { Defender = 0, Attacker = 1 }
    public enum TurnState { AttackerTurn, DefenderTurn, Simulation, GameOver }

    public PlayerRole currentPlayer = PlayerRole.Attacker;
    public TurnState currentState = TurnState.AttackerTurn;
    public int turnsPerRound = 2;
    public int currentTurn = 1;

    public int currentRound = 1;

    public GameObject attackerButtons;
    public GameObject defenderButtons;

    private GameObject towerHubPanel;
    private GameObject enemyHubPanel;
    private GameObject turnPanel;

    private GameObject attackerRoleText;
    private GameObject defenderRoleText;

    public DisplayManager displayManager;

    Dictionary<string, int> playerMissedEnemies = new Dictionary<string, int>();

    public bool isMyTurn = false;
    public new PhotonView photonView;

    public int myPlayerId;
    public List<UnitLogInfo> unitPlacementLog = new List<UnitLogInfo>();
    public struct UnitLogInfo
    {
        public string unitType;
        public string path;
    }

    private bool coroutineStarted = false;

    void Start()
    {
        towerHubPanel = GameObject.Find("TowerHubPanel");
        enemyHubPanel = GameObject.Find("EnemyHubPanel");
        turnPanel = GameObject.Find("TurnPanel");
        attackerRoleText = GameObject.Find("AttackerText");
        defenderRoleText = GameObject.Find("DefenderText");

        towerHubPanel.SetActive(false);
        enemyHubPanel.SetActive(false);
        turnPanel.SetActive(false);
        displayManager = GameObject.Find("DisplayManager").GetComponent<DisplayManager>();
        displayManager.waitingForOpponentText.gameObject.SetActive(true);
        photonView = GetComponent<PhotonView>();
        myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;
        turnsPerRound = 3;
        currentTurn = 1;
        if (PhotonNetwork.IsConnected)
        {
            Debug.LogError(PhotonNetwork.LocalPlayer.ActorNumber);
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                currentPlayer = PlayerRole.Defender;
            }
            else
            {
                currentPlayer = PlayerRole.Attacker;
            }

        }
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Player1Name"))
        {
            player1Name = (string)PhotonNetwork.CurrentRoom.CustomProperties["Player1Name"];
            playerMissedEnemies.Add(player1Name, 0);
        }
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Player2Name"))
        {
            player2Name = (string)PhotonNetwork.CurrentRoom.CustomProperties["Player2Name"];
            playerMissedEnemies.Add(player2Name, 0);
        }
        if (PhotonNetwork.IsMasterClient)
        {
            currentDefender = player1Name;
            opposingPlayer = player2Name;
            photonView.RPC("SyncInitialRoles", RpcTarget.All, currentDefender, opposingPlayer);
            photonView.RPC("StartGameForEveryone", RpcTarget.All);
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case TurnState.AttackerTurn:
                if (currentPlayer == PlayerRole.Attacker && isMyTurn)
                {
                    towerHubPanel.SetActive(false);
                    enemyHubPanel.SetActive(true);
                }
                else
                {
                    towerHubPanel.SetActive(false);
                    enemyHubPanel.SetActive(false);
                }
                break;

            case TurnState.DefenderTurn:
                if (currentPlayer == PlayerRole.Defender && isMyTurn)
                {
                    towerHubPanel.SetActive(true);
                    enemyHubPanel.SetActive(false);
                }
                else
                {
                    towerHubPanel.SetActive(false);
                    enemyHubPanel.SetActive(false);
                }
                break;
            case TurnState.Simulation:
                turnPanel.SetActive(false);
                towerHubPanel.SetActive(false);
                enemyHubPanel.SetActive(false);
                displayManager.unitLogText.gameObject.SetActive(false);
                StartEnemyMovement();
                EnemyPlacementManager enemyPlacementManager = GameObject.Find("GameManager").GetComponent<EnemyPlacementManager>();
                if (currentState == TurnState.Simulation && enemyPlacementManager.activeEnemies.Count == 0 && !coroutineStarted)
                {
                    StartCoroutine(AfterRoundTime());
                    coroutineStarted = true;
                }
                break;
            case TurnState.GameOver:

                break;
        }
        if (currentTurn > turnsPerRound * 2)
        {
            currentState = TurnState.Simulation;
        }
        if (currentPlayer == PlayerRole.Attacker)
        {
            attackerRoleText.SetActive(true);
            defenderRoleText.SetActive(false);
        }
        else
        {
            defenderRoleText.SetActive(true);
            attackerRoleText.SetActive(false);
        }
    }

    public void EndAttackerTurn()
    {
        currentState = TurnState.DefenderTurn;
        photonView.RPC("SetTurnState", RpcTarget.All, TurnState.DefenderTurn);
    }
    public void EndDefenderTurn()
    {
        currentState = TurnState.AttackerTurn;
        photonView.RPC("SetTurnState", RpcTarget.All, TurnState.AttackerTurn);
    }

    void StartEnemyMovement()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObject in enemyObjects)
        {
            BaseEnemy enemy = enemyObject.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.isMoving = true;
            }
        }
    }
    void SwitchRoles()
    {
        if (currentPlayer == PlayerRole.Attacker)
        {
            currentPlayer = PlayerRole.Defender;
        }
        else
        {
            currentPlayer = PlayerRole.Attacker;
        }
    }

    void EndRound()
    {
        playerMissedEnemies[currentDefender] = displayManager.enemyReachedEnd;
        foreach (var entry in playerMissedEnemies)
        {
            Debug.LogError("Player: " + entry.Key + ", Missed Enemies: " + entry.Value);
        }
    }

    IEnumerator AfterRoundTime()
    {
        yield return new WaitForSeconds(3);
        EndRound();
        if (currentRound == 2)
        {
            GameOver();
        }
        else
        {
            SwitchRoles();
            StartNewRound();
        }
    }

    void GameOver()
    {
        if (playerMissedEnemies[player1Name] < playerMissedEnemies[player2Name])
        {
            displayManager.winnerText.text = player1Name + " Wins!!!";
        }
        else if (playerMissedEnemies[player1Name] > playerMissedEnemies[player2Name])
        {
            displayManager.winnerText.text = player2Name + " Wins!!!";
        }
        else
        {
            displayManager.winnerText.text = "It's a Tie!";
        }
        displayManager.winnerText.gameObject.SetActive(true);
        displayManager.quitGameButton.gameObject.SetActive(true);
    }


    void StartNewRound()
    {
        currentRound++;
        currentTurn = 1;
        isMyTurn = false;
        coroutineStarted = false;

        Debug.LogError("StartNewRoundTriggers!");

        EnemyPlacementManager enemyPlacementManager = GameObject.Find("GameManager").GetComponent<EnemyPlacementManager>();
        enemyPlacementManager.isBaseEnemySpawned = false;
        enemyPlacementManager.isFastEnemySpawned = false;
        enemyPlacementManager.isFlyerEnemySpawned = false;
        enemyPlacementManager.isSplitterEnemySpawned = false;
        enemyPlacementManager.isTankEnemySpawned = false;

        TowerPlacementManager towerPlacementManager = GameObject.Find("GameManager").GetComponent<TowerPlacementManager>();
        towerPlacementManager.isBaseTowerSpawned = false;
        towerPlacementManager.isBombTowerSpawned = false;
        towerPlacementManager.isBouncingTowerSpawned = false;
        towerPlacementManager.isMarkTowerSpawned = false;
        towerPlacementManager.isPoisonTowerSpawned = false;
        towerPlacementManager.isSlowingTowerSpawned = false;
        towerPlacementManager.isWarmupTowerSpawned = false;
        towerPlacementManager.occupiedPositions.Clear();

        DisplayManager displayManager = GameObject.Find("DisplayManager").GetComponent<DisplayManager>();
        displayManager.enemyReachedEnd = 0;
        displayManager.UpdateEnemyCountText();
        displayManager.currentPlayerTurn.text = "Attacker Turn";
        displayManager.currentPlayerTurn.color = Color.red;
        displayManager.turnCounterText.text = "Turn: " + Mathf.CeilToInt(currentTurn / 2f) + "/" + turnsPerRound;

        unitPlacementLog.Clear();
        photonView.RPC("UpdateLogText", RpcTarget.All);
        towerHubPanel.SetActive(true); 
        enemyHubPanel.SetActive(true);
        displayManager.unitLogText.gameObject.SetActive(true);

        foreach (Transform childTransform in attackerButtons.transform)
        {
            if (childTransform.gameObject.activeSelf == false)
            {
                childTransform.gameObject.SetActive(true);
            }
        }
        foreach (Transform childTransform in defenderButtons.transform)
        {
            if (childTransform.gameObject.activeSelf == false)
            {
                childTransform.gameObject.SetActive(true);
            }
        }

        GameObject[] placedTowers = GameObject.FindGameObjectsWithTag("Tower");

        foreach (GameObject tower in placedTowers)
        {
            Destroy(tower);
        }

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bomb");

        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }

        turnPanel.SetActive(true);
        currentState = TurnState.AttackerTurn;
        if (currentPlayer == PlayerRole.Attacker)
        {
            isMyTurn = true;
        }
        currentDefender = player2Name;

    }

    [PunRPC]
    void SetTurnState(TurnState newState)
    {
        currentTurn++;
        displayManager.turnCounterText.text = "Turn: " + Mathf.CeilToInt(currentTurn / 2f) + "/" + turnsPerRound;
        currentState = newState;

        isMyTurn = currentState == TurnState.AttackerTurn && currentPlayer == PlayerRole.Attacker || currentState == TurnState.DefenderTurn && currentPlayer == PlayerRole.Defender;

        string turnText = newState == TurnState.AttackerTurn ? "Attacker Turn" : "Defender Turn";
        Color turnColor = newState == TurnState.AttackerTurn ? Color.red : Color.green;

        displayManager.currentPlayerTurn.text = turnText;
        displayManager.currentPlayerTurn.color = turnColor;
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("UpdateLogText", RpcTarget.All);
        }
    }

    [PunRPC]
    void SyncInitialRoles(string initialDefender, string initialAttacker)
    {
        player1Name = initialDefender;
        player2Name = initialAttacker;

        foreach (var entry in playerMissedEnemies)
        {
            Debug.LogError("Player: " + entry.Key + ", Missed Enemies: " + entry.Value);
        }

        currentDefender = player1Name;

        Debug.LogError("SyncInitialRoles called. Defender: " + player1Name + ", Attacker: " + player2Name);
        Debug.LogError("SyncInitialRoles: After assignment, my role is: " + currentPlayer);

    }
    [PunRPC]
    void StartGameForEveryone()
    {
        isMyTurn = (currentPlayer == PlayerRole.Attacker);
        turnPanel.SetActive(true);
        displayManager.turnCounterText.text = "Turn: " + currentTurn + "/" + turnsPerRound;
        displayManager.waitingForOpponentText.gameObject.SetActive(false);
    }

    [PunRPC]
    void UpdateLogText()
    {
        string logText = "Assigned attacking units: \n";
        foreach (UnitLogInfo entry in unitPlacementLog)
        {
            logText += "Unit: " + entry.unitType + ",  Path: " + entry.path + "\n";
        }

        displayManager.unitLogText.text = logText;
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.InRoom)
        {
            if (otherPlayer != PhotonNetwork.LocalPlayer)
            {
                string message = "Opponent disconnected from the game";
                displayManager.DisplayOpponentLeftMessage(message);
            }
        }
    }
    public void QuitInMiddleGame()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        SceneManager.LoadScene("MainMenuScene");
    }
}