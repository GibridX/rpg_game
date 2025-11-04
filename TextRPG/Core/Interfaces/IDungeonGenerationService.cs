using TextRPG.Core.Models;

namespace TextRPG.Core.Services.Generation
{
    public interface IDungeonGenerationService
    {
        Dungeon GenerateDungeon(string name, int width, int height, int maxRooms, int depth = 1);
    }
}