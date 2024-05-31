public class TankEnemy : BaseEnemy
{
    public TankEnemy()
    {
        maxHealth = 150f;
        moveSpeed = 0.3f;
        armorType = ArmorType.High;
    }
}