using UnityEngine;
using static BaseEnemy;

public class BouncingBulletTower : BaseTower
{
    void Awake()
    {
        damage = 10f;
        attackRange = 2f;
        penetrationType = PenetrationType.Medium;
        targetingMode = TargetingMode.Closest;
        attackCooldown = 1f;
    }

    public override void Attack(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 270f;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        bullet.GetComponent<Rigidbody2D>().AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
        shootAudioSource.Play();
    }
}