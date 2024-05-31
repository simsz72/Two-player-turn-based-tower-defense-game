using UnityEngine;

public class Flyer : BaseEnemy
{
    public Flyer()
    {
        maxHealth = 50f;
        moveSpeed = 0.8f;
        armorType = ArmorType.None;
    }
}