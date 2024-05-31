using UnityEngine;
using System.Collections;
using Photon.Pun;

public class BaseEnemy : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public float moveSpeed = 2f;
    private float originalSpeed = 0f;

    public bool isSlowed = false;
    public bool isPoisoned = false;
    public bool isMarked = false;

    public float slowedStartTime;
    public float tempSlowDuration;

    public float poisonStartTime;
    public float tempPoisonDuration;
    public float tempDamagePerSecond;
    private float poisonTick = 0f;

    public string enemyName = "HAHA";

    public Canvas canvas;
    public GameObject damageTextPrefab;

    public Transform[] pathWaypoints = null;
    private SpriteRenderer spriteRenderer;
    public int currentWaypointIndex = 0;

    public bool isMoving = false;

    public new PhotonView photonView;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSpeed = moveSpeed;
        moveSpeed = 0f;
        currentHealth = maxHealth;
        canvas = GameObject.FindObjectOfType<Canvas>();
        enemyName = gameObject.name.Replace("(Clone)", "");
        GameObject displayManager = GameObject.Find("DisplayManager");
        if (displayManager)
        {
            displayManager.GetComponent<DisplayManager>().DisplayStats(transform, enemyName);
        }
        EnemyPlacementManager enemyPlacementManager = GameObject.Find("GameManager").GetComponent<EnemyPlacementManager>();
        photonView = GetComponent<PhotonView>();
        enemyPlacementManager.activeEnemies.Add(this);
    }

    protected void Update()
    {
        if (!isSlowed && isMoving)
        {
            moveSpeed = originalSpeed;
        }
        if (pathWaypoints != null && currentWaypointIndex < pathWaypoints.Length)
        {
            Vector3 targetWaypoint = pathWaypoints[currentWaypointIndex].position;
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, moveSpeed * Time.deltaTime);

            Vector3 movementDirection = (targetWaypoint - transform.position).normalized;
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
                if (sr.gameObject.name == "sword")
                {
                    sr.flipY = movementDirection.x > 0;
                }
                else
                {
                    sr.flipX = movementDirection.x > 0;
                }
            }

            if (Vector3.Distance(transform.position, targetWaypoint) < 0.1f)
            {
                currentWaypointIndex++;
            }
        }
        if (isSlowed)
        {
            if (Time.time - slowedStartTime >= tempSlowDuration)
            {
                ResetSpeed();
            }
        }
        if (isPoisoned)
        {
            if (Time.time - poisonStartTime <= tempPoisonDuration)
            {
                if (Time.time - poisonTick >= 1f)
                {
                    TakeDamage(tempDamagePerSecond, PenetrationType.High);
                    poisonTick = Time.time;
                }
            }
            else
            {
                isPoisoned = false;
            }
        }
        if (currentWaypointIndex >= pathWaypoints.Length)
        {
            EnemyReachedEnd();
        }
    }

    private void ResetSpeed()
    {
        moveSpeed = originalSpeed;
        isSlowed = false;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            float bulletDamage = 10f;
            TakeDamage(bulletDamage, PenetrationType.Low);
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("Bomb"))
        {
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("Mark"))
        {
            Destroy(other.gameObject);
        }
    }

    virtual public void TakeDamage(float damage, PenetrationType penetrationType)
    {
        if (isMarked)
        {
            damage *= 2f;
        }
        switch (penetrationType)
        {
            case PenetrationType.None:
                switch (armorType)
                {
                    case ArmorType.None:
                        damage *= 1f;
                        break;
                    case ArmorType.Low:
                        damage *= 0.8f;
                        break;
                    case ArmorType.Medium:
                        damage *= 0.5f;
                        break;
                    case ArmorType.High:
                        damage *= 0.3f;
                        break;
                }
                break;

            case PenetrationType.Low:
                switch (armorType)
                {
                    case ArmorType.None:
                        damage *= 1f;
                        break;
                    case ArmorType.Low:
                        damage *= 1f;
                        break;
                    case ArmorType.Medium:
                        damage *= 0.7f;
                        break;
                    case ArmorType.High:
                        damage *= 0.5f;
                        break;
                }
                break;

            case PenetrationType.Medium:
                if (armorType != ArmorType.High)
                {
                    damage *= 1f;
                }
                else
                {
                    damage *= 0.7f;
                }
                break;

            case PenetrationType.High:
                damage *= 1f;
                break;
        }
        if (damageTextPrefab != null)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
            GameObject damageText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, canvas.transform);
            damageText.GetComponent<FloatingDamageText>().SetText("-" + damage.ToString());
        }
        currentHealth -= damage;
        GameObject displayManager = GameObject.Find("DisplayManager");
        displayManager.GetComponent<DisplayManager>().UpdateEnemyHealth(gameObject, currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            EnemyPlacementManager enemyPlacementManager = GameObject.Find("GameManager").GetComponent<EnemyPlacementManager>();
            enemyPlacementManager.activeEnemies.Remove(this);
            if (displayManager)
            {
                displayManager.GetComponent<DisplayManager>().DestroyNameDisplay(gameObject);
            }
            Destroy(gameObject);
        }
    }
    private void EnemyReachedEnd()
    {
        GameObject displayManager = GameObject.Find("DisplayManager");
        if (displayManager)
        {
            displayManager.GetComponent<DisplayManager>().EnemyReachedEnd();
        }
        EnemyPlacementManager enemyPlacementManager = GameObject.Find("GameManager").GetComponent<EnemyPlacementManager>();
        enemyPlacementManager.activeEnemies.Remove(this);
        Debug.Log(enemyPlacementManager.activeEnemies.Count);
        Destroy(gameObject);
    }

    public enum ArmorType { None, Low, Medium, High }
    public ArmorType armorType;

    public enum PenetrationType { None, Low, Medium, High }
    public PenetrationType penetrationType;
}
