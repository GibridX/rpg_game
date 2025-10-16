using TextRPG.Core.Utils;

namespace TextRPG.Core.Services.UI
{
    public class GameScreenService
    {
        public void ShowVictoryScreen(Player player, int currentDepth)
        {
            Console.Clear();
            ConsoleHelper.WriteColor(@"
        ╔══════════════════════════════════════════╗
        ║                ПОБЕДА!                  ║
        ║                                          ║
        ║   Вы нашли выход из подземелья!         ║
        ║   Ваши достижения:                       ║
        ║                                          ║", ConsoleColor.Yellow);

            ConsoleHelper.WriteColorInline($"   Уровень персонажа: {player.Level} ", ConsoleColor.Cyan);
            ConsoleHelper.WriteColorInline($"Золото: {player.Gold} ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"Глубина: {currentDepth}", ConsoleColor.Green);
            Console.WriteLine();

            ConsoleHelper.WriteColorInline($"   Здоровье: {player.Health}/{player.MaxHealth} ", ConsoleColor.Red);
            ConsoleHelper.WriteColorInline($"Атака: {player.Attack} ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"Опыт: {player.Experience}/{player.ExperienceToNextLevel}", ConsoleColor.Blue);
            Console.WriteLine();

            ConsoleHelper.WriteColor(@"
        ║                                          ║
        ║   Хотите спуститься глубже?             ║
        ║   1 - Да, продолжить приключение        ║
        ║   2 - Нет, выйти из игры                ║
        ║                                          ║
        ╚══════════════════════════════════════════╝", ConsoleColor.Yellow);
        }

        public void ShowGameOverScreen(Player player, int currentDepth)
        {
            Console.Clear();
            ConsoleHelper.WriteColor(@"
╔══════════════════════════════════════════╗
║              ИГРА ОКОНЧЕНА              ║
║                                          ║
║        Вы потерпели поражение           ║
║                                          ║
║         Ваши достижения:                 ║", ConsoleColor.Red);

            ConsoleHelper.WriteColorInline($"   Уровень: {player.Level} ", ConsoleColor.Cyan);
            ConsoleHelper.WriteColorInline($"Золото: {player.Gold} ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"Глубина: {currentDepth}", ConsoleColor.Green);
            Console.WriteLine();

            ConsoleHelper.WriteColor(@"
║                                          ║
║         Попробуйте еще раз!              ║
║                                          ║
╚══════════════════════════════════════════╝", ConsoleColor.Red);
        }

        public bool GetVictoryChoice()
        {
            var choice = Console.ReadLine();
            return choice == "1";
        }
    }
}