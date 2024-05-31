using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BaseEnemy;

public class SlowingTower : BaseTower
{
    public float slowFactor = 0.5f;
    public float slowDuration = 2.0f;
    void Awake()
    {
        damage = 10f;
        attackRange = 2f;
        penetrationType = PenetrationType.Low;
        attackCooldown = 1.0f;
        targetingMode = TargetingMode.Weakest;
    }

    public override void Attack(Vector2 direction)
    {
        GameObject slowEffect = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 270f;
        slowEffect.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        slowEffect.GetComponent<Rigidbody2D>().AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
        shootAudioSource.Play();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, attackRange, enemyLayerMask);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            GameObject enemy = hit.collider.gameObject;
            ApplySlowToEnemy(enemy, slowFactor, slowDuration);
        }
    }

    public void ApplySlowToEnemy(GameObject enemy, float slowFactor, float slowDuration)
    {
        BaseEnemy enemyScript = enemy.GetComponent<BaseEnemy>();
        if (enemyScript != null)
        {
            if (!enemyScript.isSlowed)
            {
               enemyScript.moveSpeed *= slowFactor;
               enemyScript.isSlowed = true;
               enemyScript.tempSlowDuration = slowDuration;
            }
            enemyScript.slowedStartTime = Time.time;
        }
    }
}
