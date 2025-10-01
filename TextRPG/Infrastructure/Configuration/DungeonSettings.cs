public class DungeonSettings
{
    public int MaxGenerationAttempts { get; set; } = 10;
    public int Width { get; set; } = 12;
    public int Height { get; set; } = 12;
    public int MaxRooms { get; set; } = 20;

    public static DungeonSettings Default => new DungeonSettings();
}