using UnityEngine;

public class SplitterEnemy : BaseEnemy
{
    public GameObject smallerEnemyPrefab;

    public SplitterEnemy()
    {
        maxHealth = 100f;
        moveSpeed = 0.7f;
        armorType = ArmorType.Medium;
    }
    public override void TakeDamage(float damage, PenetrationType penetrationType)
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
            Split();
            EnemyPlacementManager enemyPlacementManager = GameObject.Find("GameManager").GetComponent<EnemyPlacementManager>();
            enemyPlacementManager.activeEnemies.Remove(this);
            Debug.Log(enemyPlacementManager.activeEnemies.Count);
            if (displayManager)
            {
                displayManager.GetComponent<DisplayManager>().DestroyNameDisplay(gameObject);
            }
            Destroy(gameObject);
        }
    }
    public void Split()
    {
        Vector3[] offsets = new Vector3[] {
                new Vector3(0, 0, 0.5f),
                new Vector3(-0.25f, 0, -0.25f),
                new Vector3(0.25f, 0, -0.25f)
            };
        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = offsets[i];
            GameObject miniEnemy = Instantiate(smallerEnemyPrefab, transform.position + offset, Quaternion.identity);

            BaseEnemy miniEnemyComponent = miniEnemy.GetComponent<BaseEnemy>();
            miniEnemyComponent.pathWaypoints = pathWaypoints;
            miniEnemyComponent.currentWaypointIndex = currentWaypointIndex;
        }
    }
}