public class RoomSettings
{
    public int EmptyRoomChance { get; set; } = 25;
    public int EnemyRoomChance { get; set; } = 20;
    public int TreasureRoomChance { get; set; } = 10;
    public int MerchantRoomChance { get; set; } = 5;
    public int RestRoomChance { get; set; } = 5;
    public int EnemyChance { get; set; } = 20;
    public int TreasureChance { get; set; } = 10;

    public static RoomSettings Default => new RoomSettings();
}