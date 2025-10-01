public class CombatSettins
{
    public int EscapeChance { get; set; } = 50;
    public int BossHealthMuliplier { get; set; } = 10;
    public int BossAttackMultiplier { get; set; } = 3;

    public static CombatSettins Default => new CombatSettins();
}