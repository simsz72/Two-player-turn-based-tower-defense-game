using UnityEngine;
using System.Collections;

public class BouncingBullet : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    public int maxBounces = 3;
    private int currentBounces = 0;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            currentBounces++;
            BaseEnemy enemy = collision.gameObject.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, BaseEnemy.PenetrationType.Low);
            }

            GetComponent<Collider2D>().enabled = false;
            StartCoroutine(ReenableCollision());

            if (currentBounces < maxBounces)
            {
                FindNewTargetAndBounce(collision.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    void FindNewTargetAndBounce(GameObject excludeTarget)
    {
        Collider2D closestEnemy = FindClosestEnemy(excludeTarget.GetComponent<Collider2D>());

        if (closestEnemy != null)
        {
            Vector2 direction = (closestEnemy.transform.position - transform.position).normalized;
            rb.velocity = direction * speed;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    Collider2D FindClosestEnemy(Collider2D excludeTarget)
    {
        float searchRadius = 5f;
        int enemyLayerMask = LayerMask.GetMask("Enemy");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayerMask);

        Collider2D closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D collider in hits)
        {
            if (collider != excludeTarget)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = collider;
                }
            }
        }

        return closestEnemy;
    }

    IEnumerator ReenableCollision()
    {
        yield return new WaitForSeconds(0.1f);
        if (gameObject != null)
        {
            GetComponent<Collider2D>().enabled = true;
        }
    }
}
