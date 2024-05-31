using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static TurnManager;
using static UnityEngine.EventSystems.EventTrigger;


public class EnemyPlacementManager : MonoBehaviourPunCallbacks
{
    public GameObject[] enemyPrefabs;
    public Transform[] path1Waypoints;
    public Transform[] path2Waypoints;
    public Transform[] path3Waypoints;
    public Transform spawnPosition;

    public bool isTankEnemySpawned = false;
    public bool isBaseEnemySpawned = false;
    public bool isFastEnemySpawned = false;
    public bool isSplitterEnemySpawned = false;
    public bool isFlyerEnemySpawned = false;

    public string pathTaken;
    private GameObject selectedEnemy;

    public List<BaseEnemy> activeEnemies = new List<BaseEnemy>();

    public new PhotonView photonView;


    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void SelectPath1()
    {
        if (selectedEnemy != null)
        {
            pathTaken = "Left";
            AssignPath(path1Waypoints);
        }
    }
    public void SelectPath2()
    {
        if (selectedEnemy != null)
        {
            pathTaken = "Middle";
            AssignPath(path2Waypoints);
            Debug.LogError("ASSIGNINGPATH");
        }
    }
    public void SelectPath3()
    {
        if (selectedEnemy != null)
        {            pathTaken = "Right";
            AssignPath(path3Waypoints);
        }
    }
    void AssignPath(Transform[] path)
    {
        photonView.RPC("PlaceEnemy", RpcTarget.All, selectedEnemy.name, pathTaken);
        GameObject turnManager = GameObject.Find("TurnManager");
        if (selectedEnemy == enemyPrefabs[0])
        {
            isBaseEnemySpawned = true;
            Transform buttonToDisable = turnManager.GetComponent<TurnManager>().attackerButtons.transform.Find("BaseEnemy");
            buttonToDisable.gameObject.SetActive(false);
        }
        else if (selectedEnemy == enemyPrefabs[1])
        {
            isTankEnemySpawned = true;
            Transform buttonToDisable = turnManager.GetComponent<TurnManager>().attackerButtons.transform.Find("TankEnemy");
            buttonToDisable.gameObject.SetActive(false);

        }
        else if (selectedEnemy == enemyPrefabs[2])
        {
            isFastEnemySpawned = true;
            Transform buttonToDisable = turnManager.GetComponent<TurnManager>().attackerButtons.transform.Find("FastEnemy");
            buttonToDisable.gameObject.SetActive(false);

        }
        else if (selectedEnemy == enemyPrefabs[3])
        {
            isSplitterEnemySpawned = true;
            Transform buttonToDisable = turnManager.GetComponent<TurnManager>().attackerButtons.transform.Find("SplitterEnemy");
            buttonToDisable.gameObject.SetActive(false);

        }
        else if (selectedEnemy == enemyPrefabs[4])
        {
            isFlyerEnemySpawned = true;
            Transform buttonToDisable = turnManager.GetComponent<TurnManager>().attackerButtons.transform.Find("FlyerEnemy");
            buttonToDisable.gameObject.SetActive(false);
        }
        selectedEnemy = null;
    }

    public void SelectBaseEnemy()
    {
        if (!isBaseEnemySpawned)
        {
            selectedEnemy = enemyPrefabs[0];
        }
    }
    public void SelectTankEnemy()
    {
        if (!isTankEnemySpawned)
        {
            selectedEnemy = enemyPrefabs[1];
        }
    }
    public void SelectFastEnemy()
    {
        if (!isFastEnemySpawned)
        {
            selectedEnemy = enemyPrefabs[2];
        }
    }
    public void SelectSplitterEnemy()
    {
        if (!isSplitterEnemySpawned)
        {
            selectedEnemy = enemyPrefabs[3];
        }
    }
    public void SelectFlyerEnemy()
    {
        if (!isFlyerEnemySpawned)
        {
            selectedEnemy = enemyPrefabs[4];
        }
    }

    [PunRPC]
    void AlertTurnManager(string enemyName, string pathTaken)
    {
        TurnManager turnManager = GameObject.Find("TurnManager").GetComponent<TurnManager>();

        TurnManager.UnitLogInfo logEntry = new TurnManager.UnitLogInfo
        {
            unitType = enemyName,
            path = pathTaken
        };

        turnManager.unitPlacementLog.Add(logEntry);
        Debug.LogError("ALLERTETURNMANAGER");
        if (PhotonNetwork.IsMasterClient)
        {
            FindObjectOfType<TurnManager>().EndAttackerTurn();
        }
    }

    [PunRPC]
    void PlaceEnemy(string enemyName, string pathTaken)
    {
        GameObject selectedEnemy;
        switch (enemyName)
        {
            case "BaseEnemy": selectedEnemy = enemyPrefabs[0]; break;
            case "TankEnemy": selectedEnemy = enemyPrefabs[1]; break;
            case "FastEnemy": selectedEnemy = enemyPrefabs[2]; break;
            case "SplitterEnemy": selectedEnemy = enemyPrefabs[3]; break;
            case "FlyerEnemy": selectedEnemy = enemyPrefabs[4]; break;
            default:
                Debug.LogError("Unknown enemy name: " + enemyName);
                return;
        }


        GameObject enemyInstance = Instantiate(selectedEnemy, spawnPosition.position, spawnPosition.rotation);
        BaseEnemy enemyComponent = enemyInstance.GetComponent<BaseEnemy>();
        if (pathTaken == "Left")
        {
            GameObject pathObject = GameObject.Find("Path1");
            Transform[] pathWaypoints = pathObject.GetComponentsInChildren<Transform>();
            Transform[] adjustedWaypoints = new Transform[pathWaypoints.Length - 1];
            Array.Copy(pathWaypoints, 1, adjustedWaypoints, 0, adjustedWaypoints.Length);
            enemyComponent.pathWaypoints = adjustedWaypoints;
        }
        if (pathTaken == "Middle")
        {
            GameObject pathObject = GameObject.Find("Path2");
            Transform[] pathWaypoints = pathObject.GetComponentsInChildren<Transform>();
            Transform[] adjustedWaypoints = new Transform[pathWaypoints.Length - 1];
            Array.Copy(pathWaypoints, 1, adjustedWaypoints, 0, adjustedWaypoints.Length);
            enemyComponent.pathWaypoints = adjustedWaypoints;
        }
        if (pathTaken == "Right")
        {
            GameObject pathObject = GameObject.Find("Path3");
            Transform[] pathWaypoints = pathObject.GetComponentsInChildren<Transform>();
            Transform[] adjustedWaypoints = new Transform[pathWaypoints.Length - 1];
            Array.Copy(pathWaypoints, 1, adjustedWaypoints, 0, adjustedWaypoints.Length);
            enemyComponent.pathWaypoints = adjustedWaypoints;
        }
        enemyComponent.currentWaypointIndex = 0;
        Debug.LogError(enemyComponent.pathWaypoints.Length);
        TurnManager turnManager = GameObject.Find("TurnManager").GetComponent<TurnManager>();
        if (turnManager.isMyTurn)
        {
            photonView.RPC("AlertTurnManager", RpcTarget.All, enemyName, pathTaken);
        }
    }
}