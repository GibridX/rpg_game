using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Services.Game
{
    public class GameStateService
    {
        private readonly GameConfig _config;

        public GameStateService(GameConfig config)
        {
            _config = config;
        }

        public void DisplayGameHeader(int currentDepth)
        {
            ConsoleHelper.WriteColor(@"
╔══════════════════════════════════════════════════════════════╗
║                   ТЕКСТОВАЯ RPG - ПОДЗЕМЕЛЬЕ                 ║
║                 Древний Склеп - Глубина: " + $"{currentDepth,2}" + @"                  ║
╚══════════════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
        }

        public void DisplayCompactStatusBar(Player player, int currentDepth, Dungeon currentDungeon)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════╗");

            double healthPercent = (double)player.Health / player.MaxHealth;
            int barWidth = 20;
            int filledWidth = (int)(barWidth * healthPercent);
            string healthBar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);

            ConsoleHelper.WriteColorInline($"║ HP: ", ConsoleColor.Red);
            ConsoleHelper.WriteColorInline($"{player.Health,3}/{player.MaxHealth,3} ", ConsoleColor.White);
            ConsoleHelper.WriteColorInline($"[{healthBar}] ", ConsoleColor.Red);

            ConsoleHelper.WriteColorInline($"АТК: {player.Attack,2} ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"УР: {player.Level,2} ", ConsoleColor.Cyan);
            ConsoleHelper.WriteColorInline($"Золото: {player.Gold,3} ", ConsoleColor.Yellow);
            Console.WriteLine("║");

            double expPercent = (double)player.Experience / player.ExperienceToNextLevel;
            filledWidth = (int)(barWidth * expPercent);
            string expBar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);

            ConsoleHelper.WriteColorInline($"║ Опыт: {player.Experience,3}/{player.ExperienceToNextLevel,3} ", ConsoleColor.Blue);
            ConsoleHelper.WriteColorInline($"[{expBar}]", ConsoleColor.Blue);

            if (player.TurnsSinceLastRest == -1)
            {
                ConsoleHelper.WriteColorInline($" Отдых: готов", ConsoleColor.DarkGreen);
            }
            else if (player.TurnsSinceLastRest < _config.RestRoomCooldown)
            {
                int turnsRemaining = _config.RestRoomCooldown - player.TurnsSinceLastRest;
                ConsoleHelper.WriteColorInline($" Отдых: {turnsRemaining}", ConsoleColor.DarkGreen);
            }
            else
            {
                ConsoleHelper.WriteColorInline($" Отдых: готов", ConsoleColor.DarkGreen);
            }
            Console.WriteLine("                      ║");

            Console.WriteLine("╚══════════════════════════════════════════════════╝");

            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom != null)
            {
                Console.WriteLine();
                ConsoleHelper.WriteColorInline($"{GetRoomDescription(currentRoom)} ", GetRoomTitleColor(currentRoom.Type));
                ConsoleHelper.WriteColorInline($"({player.X}, {player.Y})", ConsoleColor.Gray);
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        
        private string GetRoomDescription(Room room)
        {
            return room.Description;
        }

        private ConsoleColor GetRoomTitleColor(RoomType type)
        {
            return type switch
            {
                RoomType.Start => ConsoleColor.Blue,
                RoomType.Enemy => ConsoleColor.Red,
                RoomType.Treasure => ConsoleColor.Magenta,
                RoomType.Boss => ConsoleColor.DarkRed,
                RoomType.Empty => ConsoleColor.Gray,
                _ => ConsoleColor.White
            };
        }
    }
}