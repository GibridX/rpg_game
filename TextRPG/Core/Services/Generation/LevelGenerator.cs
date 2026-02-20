using TextRPG.Config;
using TextRPG.Core.Models;
using TextRPG.Core.Services.Generation;

namespace TextRPG.Core.Services.Generation
{
    public class LevelGenerator : ILevelGenerator
    {
        private readonly IDungeonGenerationService _dungeonService;
        private readonly int _width;
        private readonly int _height;

        public LevelGenerator(int width, int height, int seed = 0)
        {
            _width = width;
            _height = height;
            var config = new GameConfig();
            var restrictionService = new RoomRestrictionService();
            _dungeonService = new DungeonGenerationService(config, restrictionService, seed);
        }

        public LevelGenerator(IDungeonGenerationService dungeonService, int width, int height)
        {
            _dungeonService = dungeonService;
            _width = width;
            _height = height;
        }

        public Dungeon GenerateDungeon(string name, int maxRooms, int depth = 1)
        {
            return _dungeonService.GenerateDungeon(name, _width, _height, maxRooms, depth);
        }
    }
}