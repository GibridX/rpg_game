using TextRPG.Core.Utils;

namespace TextRPG.Core.Services.UI
{
    public class GameScreenService
    {
        public bool ShowVictoryScreen(Player player, bool isImmediateBossVictory = false)
        {
            Console.Clear();
            
            if (isImmediateBossVictory)
            {
                ConsoleHelper.WriteColor(@"
                ╔══════════════════════════════════════════╗
                ║              ПОБЕДА НАД БОССОМ!         ║
                ║                                          ║
                ║   Вы одолели могущественного врага!     ║
                ║   Ваши достижения:                       ║
                ║                                          ║", ConsoleColor.Yellow);
            }
            else
            {
                ConsoleHelper.WriteColor(@"
                ╔══════════════════════════════════════════╗
                ║                ПОБЕДА!                  ║
                ║                                          ║
                ║   Вы нашли выход из подземелья!         ║
                ║   Ваши достижения:                       ║
                ║                                          ║", ConsoleColor.Yellow);
            }

            ConsoleHelper.WriteColorInline($"   Уровень персонажа: {player.Level} ", ConsoleColor.Cyan);
            ConsoleHelper.WriteColorInline($"Золото: {player.Gold} ", ConsoleColor.Yellow);
            Console.WriteLine();

            ConsoleHelper.WriteColorInline($"   Здоровье: {player.Health}/{player.MaxHealth} ", ConsoleColor.Red);
            ConsoleHelper.WriteColorInline($"Атака: {player.Attack} ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"Опыт: {player.Experience}/{player.ExperienceToNextLevel}", ConsoleColor.Blue);
            Console.WriteLine();

            if (isImmediateBossVictory)
            {
                ConsoleHelper.WriteColor(@"
                ║                                          ║
                ║    Выберите действие:                   ║
                ║    1 - Немедленно перейти на след. уровень ║
                ║    2 - Продолжить исследование уровня   ║
                ║                                          ║
                ║    ★ Выход останется доступен ★        ║
                ║                                          ║
                ╚══════════════════════════════════════════╝", ConsoleColor.Yellow);
            }
            else
            {
                ConsoleHelper.WriteColor(@"
                ║                                          ║
                ║   Хотите спуститься глубже?             ║
                ║   1 - Да, продолжить приключение        ║
                ║    2 - Вернуться к исследованию уровня  ║
                ║                                          ║
                ╚══════════════════════════════════════════╝", ConsoleColor.Yellow);
            }

            while (true)
            {
                var choice = Console.ReadLine();
                
                if (isImmediateBossVictory)
                {
                    switch (choice)
                    {
                        case "1":
                            return true;
                        case "2":
                            return false;
                        default:
                            ConsoleHelper.WriteColor("Неверный выбор! Введите 1 или 2:", ConsoleColor.Red);
                            break;
                    }
                }
                else
                {
                    switch (choice)
                    {
                        case "1": 
                            return true;  // Перейти на след. уровень
                        case "2": 
                            return false; // Вернуться к исследованию
                        default:
                            ConsoleHelper.WriteColor("Неверный выбор! Введите 1 или 2:", ConsoleColor.Red);
                            break;
                    }
                }
            }
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