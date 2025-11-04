using TextRPG.Core.Models;

namespace TextRPG.Core.Services.Generation
{
    public interface ILevelGenerator
    {
        Dungeon GenerateDungeon(string name, int maxRooms, int depth = 1);
    }
}