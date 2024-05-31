using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using static BaseTower;
using UnityEngine.Events;
using static TurnManager;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class DisplayManager : MonoBehaviourPunCallbacks
{
    public Canvas canvas;

    public GameObject healthBarPrefab;
    private Dictionary<GameObject, HealthBarController> enemyHealthBars = new Dictionary<GameObject, HealthBarController>();

    public GameObject nameDisplayPrefab;
    private Dictionary<GameObject, NameDisplayController> nameDisplayTargets = new Dictionary<GameObject, NameDisplayController>();

    public TextMeshProUGUI enemyCountText;
    public int enemyReachedEnd = 0;

    public GameObject towerStatsPanel;

    public TextMeshProUGUI towerNameText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI fireRateText;
    public TextMeshProUGUI penetrationText;
    public TextMeshProUGUI attackRangeText;
    public TMP_Dropdown targetingModeDropdown;
    public TextMeshProUGUI currentPlayerTurn;
    public TextMeshProUGUI winnerText;
    public Button quitGameButton;
    public TextMeshProUGUI turnCounterText;
    public TextMeshProUGUI waitingForOpponentText;
    public TextMeshProUGUI opponentLeftText;

    public TextMeshProUGUI unitLogText;

    public new PhotonView photonView;

    public Dictionary<int, BaseTower> towerLookup = new Dictionary<int, BaseTower>();
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        UpdateNameDisplayPositions();
        UpdateHealthBarPositions();
    }

    public void DisplayStats(Transform target, string name)
    {
        GameObject nameDisplay = Instantiate(nameDisplayPrefab, target.position, Quaternion.identity);
        nameDisplay.transform.SetParent(canvas.transform);
        nameDisplay.transform.localPosition = Vector3.up * 1.0f;

        GameObject healthBar = Instantiate(healthBarPrefab, target.position, Quaternion.identity);
        healthBar.transform.SetParent(canvas.transform);
        healthBar.transform.localPosition = Vector3.up * 1.2f;

        nameDisplay.GetComponent<TextMeshProUGUI>().SetText(name);

        NameDisplayController nameDisplayController = nameDisplay.GetComponent<NameDisplayController>();
        if (nameDisplayController)
        {
            nameDisplayController.SetTarget(target);
            nameDisplayTargets[nameDisplay] = nameDisplayController;
        }

        HealthBarController healthBarController = healthBar.GetComponent<HealthBarController>();
        if (healthBarController)
        {
            healthBarController.SetTarget(target);
            enemyHealthBars[healthBar] = healthBarController;
        }
    }

    private void UpdateNameDisplayPositions()
    {
        foreach (KeyValuePair<GameObject, NameDisplayController> entry in nameDisplayTargets)
        {
            GameObject nameDisplay = entry.Key;
            Transform targetEnemy = entry.Value.GetTarget();

            if (nameDisplay != null && targetEnemy != null)
            {
                nameDisplay.transform.position = targetEnemy.position + Vector3.up * 1.2f;
            }
            else
            {
                Destroy(nameDisplay);
                nameDisplayTargets.Remove(nameDisplay);
            }
        }
    }

    private void UpdateHealthBarPositions()
    {
        foreach (KeyValuePair<GameObject, HealthBarController> entry in enemyHealthBars)
        {
            GameObject healthBar = entry.Key;
            Transform targetEnemy = entry.Value.GetTarget();

            if (healthBar != null && targetEnemy != null)
            {
                healthBar.transform.position = targetEnemy.position + Vector3.up * 1.2f;
            }
            else
            {
                Destroy(healthBar);
                enemyHealthBars.Remove(healthBar);
            }
        }
    }
    public void DestroyNameDisplay(GameObject enemy)
    {
        List<GameObject> nameDisplaysToRemove = new List<GameObject>();
        List<GameObject> healthBarsToRemove = new List<GameObject>();

        foreach (KeyValuePair<GameObject, NameDisplayController> entry in nameDisplayTargets)
        {
            if (entry.Value.GetTarget() == enemy)
            {
                nameDisplaysToRemove.Add(entry.Key);
            }
        }

        foreach (KeyValuePair<GameObject, HealthBarController> entry in enemyHealthBars)
        {
            if (entry.Value.GetTarget() == enemy)
            {
                healthBarsToRemove.Add(entry.Key);
            }
        }
        foreach (GameObject nameDisplay in nameDisplaysToRemove)
        {
            Destroy(nameDisplay);
            nameDisplayTargets.Remove(nameDisplay);
        }

        foreach (GameObject healthBar in healthBarsToRemove)
        {
            Destroy(healthBar);
            enemyHealthBars.Remove(healthBar);
        }
    }

    public void UpdateEnemyHealth(GameObject enemy, float currentHealth, float maxHealth)
    {
        foreach (KeyValuePair<GameObject, HealthBarController> entry in enemyHealthBars)
        {
            if (entry.Value.GetTarget().gameObject == enemy)
            {
                entry.Value.UpdateHealth(currentHealth, maxHealth);
                break;
            }
        }
    }
    public void EnemyReachedEnd()
    {
        enemyReachedEnd++;
        UpdateEnemyCountText();
    }

    public void UpdateEnemyCountText()
    {
        enemyCountText.text = "Enemies Missed: " + enemyReachedEnd;
    }
    public void DisplayOpponentLeftMessage(string message)
    {
        opponentLeftText.text = message;
        opponentLeftText.gameObject.SetActive(true);
    }
    public void ShowTowerStatsPanel(BaseTower tower)
    {
        targetingModeDropdown.onValueChanged.RemoveAllListeners();

        Vector3 towerPosition = tower.transform.position;
        Vector3 offset = Vector3.up * 1.0f + Vector3.right * 2.5f;
        towerStatsPanel.transform.position = towerPosition + offset;

        towerStatsPanel.SetActive(true);

        TurnManager turnManager = GameObject.Find("TurnManager").GetComponent<TurnManager>();
        if (turnManager.isMyTurn)
        {
            targetingModeDropdown.gameObject.SetActive(false);
        }
        tower.towerNameText = towerNameText;
        tower.damageText = damageText;
        tower.fireRateText = fireRateText;
        tower.penetrationText = penetrationText;
        tower.attackRangeText = attackRangeText;

        tower.towerNameText.text = tower.gameObject.name.Replace("(Clone)", "");
        tower.damageText.text = "Damage: " + tower.damage;
        tower.fireRateText.text = "Fire Rate: " + (1f / tower.attackCooldown).ToString("F1");
        tower.attackRangeText.text = "Attack Range: " + tower.attackRange;
        tower.penetrationText.text = "Penetration: " + tower.penetrationType.ToString();

        towerNameText.text = tower.towerNameText.text;
        damageText.text = tower.damageText.text;
        fireRateText.text = tower.fireRateText.text;
        attackRangeText.text = tower.attackRangeText.text;
        penetrationText.text = tower.penetrationText.text;

        tower.targetingModeDropdown.value = (int)tower.targetingMode;
        targetingModeDropdown.value = tower.targetingModeDropdown.value;

        if (!turnManager.isMyTurn)
        {
            targetingModeDropdown.onValueChanged.AddListener((int newIndex) =>
            {
                Debug.Log(tower);
                Debug.Log(tower.photonView);
                Debug.Log(tower.photonView.ViewID);
                photonView.RPC("ChangeTowerTargetingMode", RpcTarget.All, (TargetingMode)newIndex, tower.towerID);
                towerStatsPanel.SetActive(false);
            });
        }
        StartCoroutine(OffTime());
    }
    [PunRPC]
    void ChangeTowerTargetingMode(TargetingMode mode, int towerID)
    {
        BaseTower tower = towerLookup[towerID];
        if (tower != null)
        {
            tower.targetingMode = mode;
        }
    }

    IEnumerator OffTime()
    {
        yield return new WaitForSeconds(3);
        AfterCoroutine();
    }

    void AfterCoroutine()
    {
        targetingModeDropdown.gameObject.SetActive(true);
        towerStatsPanel.SetActive(false);
    }
    public void QuitToMainMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
}
