using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;

namespace TextRPG.Core.Factory
{
    public class EnemyFactory : IEnemyFactory
    {
        private readonly GameConfig _config = new();
        private readonly Random _random = new();

        private static readonly string[] RegularEnemyNames =
        {
            "Гоблин", "Скелет", "Орк", "Зомби", "Волк",
            "Тролль", "Гигантский паук", "Призрак", "Падший рыцарь"
        };

        private static readonly string[] BossName =
        {
            "Огненый дракон",
            "Король лич",
            "Тёмный рыцарь",
            "Каменный голем",
            "Вождь гоблинов"
        };

        public Enemy CreateRegularEnemy(int x, int y, int playerLevel, int depth)
        {
            string name = RegularEnemyNames[_random.Next(RegularEnemyNames.Length)];
            int level = CalculateEnemyLevel(playerLevel, depth);

            return new RegularEnemy(name, x, y, level, _config);
        }

        public Boss CreateBoss(int x, int y, int playerLevel, int depth)
        {
            string name = BossName[_random.Next(BossName.Length)];
            int level = CalculateBossLevel(playerLevel, depth);

            return new Boss(name, x, y, level, _config);
        }

        public Enemy CreateEnemyByRoomType(RoomType roomType, int x, int y, int playerLevel, int depth)
        {
            return roomType switch
            {
                RoomType.Boss => CreateBoss(x, y, playerLevel, depth),
                RoomType.Enemy => CreateRegularEnemy(x, y, playerLevel, depth),
                _ => throw new ArgumentException($"Комната не поддерживает создание врагов: {roomType}")
            };
        }

        private int CalculateEnemyLevel(int playerLevel, int depth)
        {
            int baseLevel = Math.Max(1, playerLevel - 1 + depth);
            return Math.Max(1, baseLevel);
        }

        private int CalculateBossLevel(int playerLevel, int depth)
        {
            return Math.Max(playerLevel, depth + 1);
        }
    }
}