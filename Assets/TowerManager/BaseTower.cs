using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using static BaseEnemy;
using TMPro;
using Photon.Pun;
using System;

public class BaseTower : MonoBehaviourPunCallbacks
{
    public int towerID;

    public float attackRange = 2f;
    public float damage = 10f;
    public PenetrationType penetrationType = PenetrationType.Low;
    public float attackCooldown = 1.0f;
    public float bulletSpeed = 10f;
    public TargetingMode targetingMode = TargetingMode.Strongest;

    public GameObject bulletPrefab;
    public LayerMask enemyLayerMask;

    protected GameObject targetEnemy;
    private float timeSinceLastAttack = 0.0f;
    private float timeSinceLastDetection = 0.0f;
    protected List<GameObject> enemiesInRange = new List<GameObject>();

    public TextMeshProUGUI towerNameText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI fireRateText;
    public TextMeshProUGUI penetrationText;
    public TextMeshProUGUI attackRangeText;
    public TMP_Dropdown targetingModeDropdown;
    public AudioSource shootAudioSource;
    public AudioClip shootSound;

    public new PhotonView photonView;

    void Start()
    {
        shootAudioSource.clip = shootSound;
        photonView = GetComponent<PhotonView>();
        enemyLayerMask = LayerMask.GetMask("Enemy");
        string towerName = gameObject.name.Replace("(Clone)", "");
        GameObject displayManager = GameObject.Find("DisplayManager");
        if (displayManager)
        {
            displayManager.GetComponent<DisplayManager>().DisplayStats(transform, towerName);
        }
    }

    protected void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        timeSinceLastDetection += Time.deltaTime;

        if (targetEnemy != null)
        {
            if (targetEnemy.GetComponent<Flyer>() == null || (targetEnemy.GetComponent<Flyer>() != null && targetingMode == TargetingMode.Air))
            {
                if (IsEnemyInRange(targetEnemy) && timeSinceLastAttack >= attackCooldown)
                {
                    Vector2 direction = (targetEnemy.transform.position - transform.position).normalized;
                    Attack(direction);
                    timeSinceLastAttack = 0.0f;
                }
            }
        }
        if (timeSinceLastDetection >= 0.1f)
        {
            DetectEnemy();
            timeSinceLastDetection = 0.0f;
        }
    }

    void OnMouseDown()
    {
        if (this == null) return;
        GameObject displayManager = GameObject.Find("DisplayManager");
        if (displayManager)
        {
            Debug.Log("ShowTowerStatsPanel called!");
            displayManager.GetComponent<DisplayManager>().ShowTowerStatsPanel(this);
        }
    }

    bool IsEnemyInRange(GameObject enemy)
    {
        float distance = Vector2.Distance(transform.position, enemy.transform.position);
        return distance <= attackRange;
    }

    public virtual void DetectEnemy()
    {
        List<GameObject> potentialTargets = new List<GameObject>();

        float angleStep = 15f;
        float currentAngle = 0f;
        int enemyLayerMask = LayerMask.GetMask("Enemy");

        while (currentAngle < 360f)
        {
            Vector2 direction = Quaternion.Euler(0, 0, currentAngle) * transform.right;
            Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y) + (direction * 0.5f);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, attackRange, enemyLayerMask);

            if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
            {
                potentialTargets.Add(hit.collider.gameObject);
            }

            currentAngle += angleStep;
        }
        if (potentialTargets.Count > 0)
        {
            targetEnemy = SelectPriorityTarget(potentialTargets);
        }
        else
        {
            targetEnemy = null;
        }
    }

    public void SetTargetingMode(TargetingMode mode)
    {
        targetingMode = mode;
    }

    protected GameObject SelectPriorityTarget(List<GameObject> targets)
    {
        if (targetingMode == TargetingMode.Closest || targetingMode == TargetingMode.Air)
        {
            float closestDistance = Mathf.Infinity;
            GameObject closestEnemy = null;

            foreach (GameObject enemy in targets)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }
        else if (targetingMode == TargetingMode.Strongest)
        {
            float highestHP = 0f;
            GameObject strongestEnemy = null;

            foreach (GameObject enemy in targets)
            {
                BaseEnemy enemyScript = enemy.GetComponent<BaseEnemy>();
                if (enemyScript != null)
                {
                    float enemyHP = enemyScript.currentHealth;
                    if (enemyHP > highestHP)
                    {
                        highestHP = enemyHP;
                        strongestEnemy = enemy;
                    }
                }
            }

            return strongestEnemy;
        }
        else if (targetingMode == TargetingMode.Weakest)
        {
            float lowestHP = Mathf.Infinity;
            GameObject weakestEnemy = null;

            foreach (GameObject enemy in targets)
            {
                BaseEnemy enemyScript = enemy.GetComponent<BaseEnemy>();
                if (enemyScript != null)
                {
                    float enemyHP = enemyScript.currentHealth;
                    if (enemyHP < lowestHP)
                    {
                        lowestHP = enemyHP;
                        weakestEnemy = enemy;
                    }
                }
            }

            return weakestEnemy;
        }
        else
        {
            Debug.LogError("Invalid TargetingMode!");
            return null;
        }
    }

    public enum TargetingMode { Closest, Strongest, Weakest, Air }

    virtual public void Attack(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 270f;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        bullet.GetComponent<Rigidbody2D>().AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
        shootAudioSource.Play();
    }
    void OnDestroy()
    {
        DisplayManager displayManager = GameObject.Find("DisplayManager").GetComponent<DisplayManager>();
        if (displayManager.towerLookup.ContainsKey(towerID))
        {
            displayManager.towerLookup.Remove(towerID);
        }
    }
}