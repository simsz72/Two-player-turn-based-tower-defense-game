using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BaseEnemy;

public class MarkTower : BaseTower
{
    void Awake()
    {
        damage = 0f;
        attackRange = 2f;
        penetrationType = PenetrationType.None;
        attackCooldown = 1.0f;
        targetingMode = TargetingMode.Weakest;
    }

    public override void Attack(Vector2 direction)
    {
        GameObject markEffect = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 270f;
        markEffect.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        markEffect.GetComponent<Rigidbody2D>().AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
        shootAudioSource.Play();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, attackRange, enemyLayerMask);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            GameObject enemy = hit.collider.gameObject;
            BaseEnemy enemyScript = enemy.GetComponent<BaseEnemy>();

            if (enemyScript != null && !enemyScript.isMarked)
            {
                MarkEnemy(enemy);
            }
        }
    }

    void MarkEnemy(GameObject enemy)
    {
        BaseEnemy enemyScript = enemy.GetComponent<BaseEnemy>();
        if (enemyScript != null)
        {
            enemyScript.isMarked = true;
        }
    }

    public override void DetectEnemy()
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
                BaseEnemy enemy = hit.collider.GetComponent<BaseEnemy>();
                if (enemy != null && !enemy.isMarked)
                {
                    potentialTargets.Add(hit.collider.gameObject);
                }
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
}
