using System.Runtime.InteropServices;

public class GameSettings
{
    public PlayerSettings Player { get; set; } = new();
    public EnemySettings Enemy { get; set; } = new();
    public DungeonSettings Dungeon { get; set; } = new();
    public RoomSettings Room { get; set; } = new();
    public CombatSettins Combat { get; set; } = new();
    public EconomySettings Economy { get; set; } = new();

    public static GameSettings CreateDefault()
    {
        return new GameSettings
        {
            
        };
    }
}