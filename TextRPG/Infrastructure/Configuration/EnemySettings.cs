public class EnemySettings
{
    public int MinHealth { get; set; } = 30;
    public int MaxHealth { get; set; } = 200;
    public int HealthPerLevel { get; set; } = 6;
    public int AttackPerLevel { get; set; } = 2;
    public int BaseAttack { get; set; } = 8;
    public int BaseGold { get; set; } = 10;
    public int GoldPerLevel { get; set; } = 4;

    public static EnemySettings Default => new EnemySettings();
}