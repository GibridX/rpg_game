public class PlayerSettings
{
    public int BaseHealth { get; set; } = 100;
    public int BaseAttack { get; set; } = 10;
    public int DamageVariance { get; set; } = 3;
    public int ExpForNextLevelMultiplier { get; set; } = 2;
    public int RestRoomCooldown { get; set; } = 5;
    public int MaxInventorySize { get; set; } = 20;
    public int MaxLevel { get; set; } = 15;
    public int DodgePerLevel { get; set; } = 2;

    public static PlayerSettings Default => new PlayerSettings();
}