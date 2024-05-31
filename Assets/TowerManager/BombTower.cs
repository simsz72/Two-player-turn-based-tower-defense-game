using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BaseEnemy;

public class BombTower : BaseTower
{
    void Awake()
    {
        damage = 10f;
        attackRange = 3f;
        penetrationType = PenetrationType.None;
        attackCooldown = 1.0f;
        targetingMode = TargetingMode.Weakest;
    }
    public float explosionRadius = 2f;

    public override void Attack(Vector2 direction)
    {
        GameObject explosionEffect = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 270f;
        explosionEffect.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        explosionEffect.GetComponent<Rigidbody2D>().AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
        shootAudioSource.Play();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, attackRange, enemyLayerMask);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            GameObject bomb = hit.collider.gameObject;
            DetonateBomb(explosionEffect);
            DetonateBomb(bomb);
        }
    }

    void DetonateBomb(GameObject explosionCenter)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionCenter.transform.position, explosionRadius, enemyLayerMask);
        foreach (Collider2D hit in hitColliders)
        {
            BaseEnemy enemy = hit.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, BaseEnemy.PenetrationType.None);
            }
        }
    }
}
