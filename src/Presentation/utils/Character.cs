public class Character
{
    public uint MaxStamina = 10;
    public uint MaxHp = 100;
    public uint DigPower = 1;
    public float StaminaRecoverySeconds = 20;
    public float HpRecoverySeconds = 5;
    public bool CanDig = true;
    public uint BagSlots = 4;

    public float EnemySpeedCoeff = 1f;

    public LootDefinition BagId;
    public LootDefinition WeaponId;
}