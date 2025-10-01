public class EconomySettings
{
    public int MerchantPriceMultiplier { get; set; } = 1;
    public int SellPriceRatio { get; set; } = 2;

    public static EconomySettings Default => new EconomySettings();
}