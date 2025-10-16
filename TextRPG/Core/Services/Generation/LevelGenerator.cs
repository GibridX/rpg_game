using TextRPG.Config;
using TextRPG.Core.Models;

namespace TextRPG.Core.Services.Generation
{
    public class LevelGenerator
    {
        private readonly DungeonGenerationService _dungeonService;
        private readonly int _width;
        private readonly int _heigth;

        public LevelGenerator(int width, int height, int seed = 0)
        {
            _width = width;
            _heigth = height;
            _dungeonService = new DungeonGenerationService(new GameConfig(), seed);
        }

        public Dungeon GenerateDungeon(string name, int maxRooms, int depth = 1)
        {
            return _dungeonService.GenerateDungeon(name, _width, _heigth, maxRooms, depth);
        }
    }
}