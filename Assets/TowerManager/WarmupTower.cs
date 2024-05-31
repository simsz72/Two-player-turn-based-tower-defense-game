using UnityEngine;
using static BaseEnemy;

public class WarmupTower : BaseTower
{
    public float maxAttackSpeedMultiplier = 1f;
    public float speedIncreaseRate = 0.1f;

    private float baseAttackCooldown;

    void Awake()
    {
        damage = 10f;
        attackRange = 3f;
        penetrationType = PenetrationType.Low;
        targetingMode = TargetingMode.Closest;
        baseAttackCooldown = 2.0f;
    }

    new void Update()
    {
        base.Update();
        if (targetEnemy != null)
        {
            ReduceAttackCooldown();
        }
        else
        {
            ResetAttackCooldown();
        }
    }

    void ReduceAttackCooldown()
    {
        attackCooldown = Mathf.Clamp(attackCooldown - (speedIncreaseRate * Time.deltaTime), baseAttackCooldown / maxAttackSpeedMultiplier, baseAttackCooldown);
    }

    void ResetAttackCooldown()
    {
        attackCooldown = baseAttackCooldown;
    }
}