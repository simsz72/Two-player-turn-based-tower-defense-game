using System.Collections;
using UnityEngine;
using static BaseEnemy;

public class PoisonTower : BaseTower
{
    public float damagePerSecond = 5f;
    public float poisonDuration = 3.0f;


    void Awake()

    {
        damage = 5f;
        attackRange = 2f;
        penetrationType = PenetrationType.None;
        attackCooldown = 2.0f;
        targetingMode = TargetingMode.Strongest;
    }
    public override void Attack(Vector2 direction)
    {
        GameObject poisonEffect = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 270f;
        poisonEffect.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        poisonEffect.GetComponent<Rigidbody2D>().AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
        shootAudioSource.Play();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, attackRange, enemyLayerMask);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            GameObject enemy = hit.collider.gameObject;
            ApplyPoisonToEnemy(enemy, damagePerSecond, poisonDuration);
        }
    }

    public void ApplyPoisonToEnemy(GameObject enemy, float damagePerSecond, float poisonDuration)
    {
        BaseEnemy enemyScript = enemy.GetComponent<BaseEnemy>();
        if (enemyScript != null)
        {
            if (!enemyScript.isPoisoned)
            {
                enemyScript.tempPoisonDuration = poisonDuration;
                enemyScript.tempDamagePerSecond = damagePerSecond;
                enemyScript.isPoisoned = true;
            }
            enemyScript.poisonStartTime = Time.time;
        }
    }
}
