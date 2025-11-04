using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Utils;
using System.Text;

namespace TextRPG.Core.Services
{
    public class CombatService : ICombatService
    {
        private readonly Random _random;
        private readonly GameConfig _config;
        private readonly StringBuilder _combatBuilder;
        private readonly string[] _actionDescriptions;
        private const int CombatWidth = 60;

        public CombatService(GameConfig config)
        {
            _random = new Random();
            _config = config;
            _combatBuilder = new StringBuilder(512);
            _actionDescriptions = new[]
            {
                "1. Атаковать",
                "2. Защищаться (уменьшает урон на 50%)", 
                "3. Использовать предмет",
                "4. Попытаться сбежать"
            };
        }

        public CombatResult StartCombat(Player player, Enemy enemy)
        {
            DisplayCombatHeader(enemy, isBoss: false);
            return ExecuteCombat(player, enemy, isBoss: false);
        }

        public CombatResult StartBossFight(Player player, Boss boss)
        {
            DisplayCombatHeader(boss, isBoss: true);
            return ExecuteCombat(player, boss, isBoss: true);
        }

        private CombatResult ExecuteCombat(Player player, Enemy enemy, bool isBoss)
        {
            bool isDefending = false;
            int turnNumber = 1;

            while (enemy.Health > 0 && player.Health > 0)
            {
                DisplayTurnHeader(turnNumber++);
                DisplayCombatStatus(player, enemy);
                
                var action = GetPlayerAction(player);
                isDefending = action == CombatAction.Defend;
                
                if (!ProcessPlayerAction(player, enemy, action))
                    continue;

                if (enemy.Health <= 0) break;

                if (isBoss)
                    ProcessBossTurn(player, (Boss)enemy, isDefending);
                else
                    ProcessEnemyTurn(player, enemy, isDefending);

                Console.WriteLine();
                DisplayHealthBars(player, enemy);
                Console.WriteLine();
            }

            return CreateCombatResult(player, enemy);
        }

        private void DisplayCombatHeader(Enemy enemy, bool isBoss)
        {
            var borderColor = isBoss ? ConsoleColor.DarkRed : ConsoleColor.Red;
            var titleColor = isBoss ? ConsoleColor.Red : ConsoleColor.Yellow;
            var enemyColor = isBoss ? ConsoleColor.Magenta : ConsoleColor.DarkYellow;

            DisplayBorder("╔══════════════════════════════════════════════════════════╗", borderColor);
            
            if (isBoss)
            {
                DisplayCenteredText("=== БОСС БИТВА ===", titleColor);
                DisplayBorder("╠══════════════════════════════════════════════════════════╣", borderColor);
            }
            
            DisplayCenteredText($"Встреча с: {enemy.Name}", enemyColor);
            DisplayBorder("╚══════════════════════════════════════════════════════════╝", borderColor);
            
            Console.WriteLine();
            DisplayEnemyInfo(enemy, isBoss);
            Console.WriteLine();
        }

        private void DisplayTurnHeader(int turnNumber)
        {
            ConsoleHelper.WriteColor($"╔══════════════════════════════════════════════════════════╗", ConsoleColor.DarkGray);
            ConsoleHelper.WriteColor($"║                      ХОД {turnNumber, -2}                              ║", ConsoleColor.Gray);
            ConsoleHelper.WriteColor($"╚══════════════════════════════════════════════════════════╝", ConsoleColor.DarkGray);
            Console.WriteLine();
        }

        private void DisplayEnemyInfo(Enemy enemy, bool isBoss)
        {
            var statsColor = isBoss ? ConsoleColor.Magenta : ConsoleColor.DarkYellow;
            
            ConsoleHelper.WriteColorInline("Урон: ", ConsoleColor.Gray);
            ConsoleHelper.WriteColorInline($"{enemy.DamageMin}-{enemy.DamageMax}", statsColor);
            
            ConsoleHelper.WriteColorInline("  | Здоровье: ", ConsoleColor.Gray);
            ConsoleHelper.WriteColorInline($"{enemy.Health}/{enemy.MaxHealth}", GetHealthColor(enemy.Health, enemy.MaxHealth));
            
            if (isBoss)
            {
                var boss = (Boss)enemy;
                ConsoleHelper.WriteColorInline("  | Шанс способности: ", ConsoleColor.Gray);
                ConsoleHelper.WriteColorInline($"{boss.SpecialAbilityChance}%", ConsoleColor.Magenta);
            }
        }

        private void DisplayCombatStatus(Player player, Enemy enemy)
        {
            DisplayHealthBars(player, enemy);
            Console.WriteLine();
        }

        private void DisplayHealthBars(Player player, Enemy enemy)
        {
            // Полоса здоровья игрока
            DisplayHealthBar("ИГРОК", player.Health, player.MaxHealth, ConsoleColor.Green);
            
            // Полоса здоровья врага  
            DisplayHealthBar(enemy.Name.ToUpper(), enemy.Health, enemy.MaxHealth, ConsoleColor.Red);
        }

        private void DisplayHealthBar(string name, int currentHealth, int maxHealth, ConsoleColor color)
        {
            const int barWidth = 30;
            double percent = (double)currentHealth / maxHealth;
            int filledWidth = (int)(barWidth * percent);
            string bar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);

            var barColor = percent switch
            {
                > 0.6 => ConsoleColor.Green,
                > 0.3 => ConsoleColor.Yellow,
                _ => ConsoleColor.Red
            };

            ConsoleHelper.WriteColorInline($"{name,-15}: ", ConsoleColor.White);
            ConsoleHelper.WriteColorInline($"{currentHealth,3}/{maxHealth,3} ", ConsoleColor.Gray);
            ConsoleHelper.WriteColorInline($"[{bar}] ", barColor);
            ConsoleHelper.WriteColorInline($"({percent * 100:0}%)", ConsoleColor.DarkGray);
            Console.WriteLine();
        }

        private CombatAction GetPlayerAction(Player player)
        {
            ConsoleHelper.WriteColor("Выберите действие:", ConsoleColor.Cyan);
            
            // Отображение действий
            foreach (var action in _actionDescriptions)
            {
                ConsoleHelper.WriteColor($"  {action}", ConsoleColor.White);
            }

            // Показ доступных зелий
            var potions = player.Inventory.GetItemsByType(ItemType.Potion);
            if (potions.Any())
            {
                ConsoleHelper.WriteColor("Доступные зелья:", ConsoleColor.DarkMagenta);
                for (int i = 0; i < potions.Count; i++)
                {
                    ConsoleHelper.WriteColor($"    {potions[i].Name} (+{potions[i].Value} HP)", ConsoleColor.Magenta);
                }
            }

            while (true)
            {
                ConsoleHelper.WriteColorInline("\nВаш выбор: ", ConsoleColor.Cyan);
                var input = Console.ReadLine();
                
                switch (input)
                {
                    case "1": return CombatAction.Attack;
                    case "2": return CombatAction.Defend;
                    case "3": return CombatAction.UseItem;
                    case "4": return CombatAction.Escape;
                    default:
                        ConsoleHelper.WriteColor("Неверный выбор! Введите цифру от 1 до 4:", ConsoleColor.Red);
                        break;
                }
            }
        }

        private bool ProcessPlayerAction(Player player, Enemy enemy, CombatAction action)
        {
            Console.WriteLine();
            
            switch (action)
            {
                case CombatAction.Attack:
                    return ProcessAttack(player, enemy);
                    
                case CombatAction.Defend:
                    ConsoleHelper.WriteColor("Вы принимаете защитную стойку...", ConsoleColor.Blue);
                    return true;

                case CombatAction.UseItem:
                    return UseItemInCombat(player);

                case CombatAction.Escape:
                    return AttemptEscape(player, enemy);

                default:
                    return false;
            }
        }

        private bool ProcessAttack(Player player, Enemy enemy)
        {
            int baseDamage = player.Attack;
            int variance = _random.Next(-_config.PlayerDamageVariance, _config.PlayerDamageVariance + 1);
            int playerDamage = Math.Max(1, baseDamage + variance);

            bool isCritical = false;
            bool isBoss = enemy is Boss;

            // Критический удар
            if (isBoss && _random.Next(100) < 15)
            {
                playerDamage = (int)(playerDamage * 1.5);
                isCritical = true;
            }

            enemy.TakeDamage(playerDamage);
            
            if (isCritical)
            {
                ConsoleHelper.WriteColor($"КРИТИЧЕСКИЙ УДАР! Вы нанесли {playerDamage} урона {enemy.Name}!", ConsoleColor.Magenta);
            }
            else
            {
                ConsoleHelper.WriteColor($"Вы нанесли {playerDamage} урона {enemy.Name}!", ConsoleColor.Yellow);
            }

            return true;
        }

        private void ProcessEnemyTurn(Player player, Enemy enemy, bool isPlayerDefending)
        {
            if (CheckDodge(player, enemy, isBoss: false))
                return;

            int enemyDamage = CalculateEnemyDamage(enemy, isPlayerDefending);
            player.TakeDamage(enemyDamage);
            
            ConsoleHelper.WriteColor($"{enemy.Name} наносит вам {enemyDamage} урона!", ConsoleColor.Red);
        }

        private void ProcessBossTurn(Player player, Boss boss, bool isPlayerDefending)
        {
            if (CheckDodge(player, boss, isBoss: true))
                return;

            int bossDamage = CalculateBossDamage(boss, isPlayerDefending);
            string attackDescription = $"{boss.Name} атакует";
            ConsoleColor messageColor = ConsoleColor.DarkRed;

            // Специальная способность босса
            if (_random.Next(100) < boss.SpecialAbilityChance)
            {
                var ability = boss.UseSpecialAbility();
                attackDescription = $"{boss.Name} использует '{ability}'";
                bossDamage = (int)(bossDamage * 1.3);
                messageColor = ConsoleColor.Magenta;
            }

            player.TakeDamage(bossDamage);
            ConsoleHelper.WriteColor($"{attackDescription} и наносит {bossDamage} урона!", messageColor);
        }

        private bool CheckDodge(Player player, Enemy enemy, bool isBoss)
        {
            int dodgeChance = player.Level * (isBoss ? _config.DodgePerLevel / 2 : _config.DodgePerLevel);
            
            if (_random.Next(100) < dodgeChance)
            {
                ConsoleHelper.WriteColor($"Вы ловко уклоняетесь от атаки {enemy.Name}!", ConsoleColor.Cyan);
                return true;
            }
            
            return false;
        }

        private int CalculateEnemyDamage(Enemy enemy, bool isPlayerDefending)
        {
            int damage = enemy.CalculateDamage();
            return isPlayerDefending ? damage / 2 : damage;
        }

        private int CalculateBossDamage(Boss boss, bool isPlayerDefending)
        {
            int damage = boss.CalculateDamage();
            return isPlayerDefending ? damage / 2 : damage;
        }

        private bool UseItemInCombat(Player player)
        {
            var potions = player.Inventory.GetItemsByType(ItemType.Potion);

            if (!potions.Any())
            {
                ConsoleHelper.WriteColor("У вас нет зелий для использования!", ConsoleColor.Red);
                return false;
            }

            ConsoleHelper.WriteColor("Выберите зелье:", ConsoleColor.DarkMagenta);
            
            for (int i = 0; i < potions.Count; i++)
            {
                ConsoleHelper.WriteColorInline($"  [{i + 1}] ", ConsoleColor.Yellow);
                ConsoleHelper.WriteColor($"{potions[i].Name} (+{potions[i].Value} HP)", ConsoleColor.Magenta);
            }
            ConsoleHelper.WriteColor("  [0] Отмена", ConsoleColor.Gray);

            while (true)
            {
                ConsoleHelper.WriteColorInline("\nВаш выбор: ", ConsoleColor.Cyan);
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice == 0) return false;
                    if (choice > 0 && choice <= potions.Count)
                    {
                        var selectedPotion = potions[choice - 1];
                        var potionKey = player.Inventory.GetItemKey(selectedPotion);

                        if (potionKey != null)
                        {
                            int healAmount = selectedPotion.Value;
                            player.Heal(healAmount);
                            player.Inventory.RemoveItem(potionKey);
                            
                            ConsoleHelper.WriteColor($"Вы использовали {selectedPotion.Name} и восстановили {healAmount} HP!", ConsoleColor.Green);
                            return true;
                        }
                    }
                }
                ConsoleHelper.WriteColor("Неверный выбор! Попробуйте снова:", ConsoleColor.Red);
            }
        }

        private bool AttemptEscape(Player player, Enemy enemy)
        {
            if (_random.Next(100) < _config.EscapeChance)
            {
                ConsoleHelper.WriteColor("Вы успешно сбежали из боя!", ConsoleColor.Green);
                return false;
            }
            else
            {
                ConsoleHelper.WriteColor("Побег не удался! Враг слишком близко!", ConsoleColor.Red);
                return true;
            }
        }

        private CombatResult CreateCombatResult(Player player, Enemy enemy)
        {
            bool playerWon = enemy.Health <= 0;

            Console.WriteLine();
            
            if (playerWon)
            {
                DisplayVictoryScreen(enemy);
                return new CombatResult(true, enemy.ExperienceReward, enemy.GoldReward);
            }
            else
            {
                DisplayDefeatScreen();
                return new CombatResult(false, 0, 0);
            }
        }

        private void DisplayVictoryScreen(Enemy enemy)
        {
            ConsoleHelper.WriteColor("╔══════════════════════════════════════════════════════════╗", ConsoleColor.Green);
            ConsoleHelper.WriteColor("║                        ПОБЕДА!                          ║", ConsoleColor.Green);
            ConsoleHelper.WriteColor("╚══════════════════════════════════════════════════════════╝", ConsoleColor.Green);
            
            ConsoleHelper.WriteColor($"{enemy.Name} побежден!", ConsoleColor.Green);
            ConsoleHelper.WriteColor($"Получено опыта: {enemy.ExperienceReward}", ConsoleColor.Blue);
            ConsoleHelper.WriteColor($"Получено золота: {enemy.GoldReward}", ConsoleColor.Yellow);
        }

        private void DisplayDefeatScreen()
        {
            ConsoleHelper.WriteColor("╔══════════════════════════════════════════════════════════╗", ConsoleColor.Red);
            ConsoleHelper.WriteColor("║                       ПОРАЖЕНИЕ                         ║", ConsoleColor.Red);
            ConsoleHelper.WriteColor("╚══════════════════════════════════════════════════════════╝", ConsoleColor.Red);
            ConsoleHelper.WriteColor("Вы пали в бою...", ConsoleColor.Red);
        }

        // Вспомогательные методы для отрисовки
        private void DisplayBorder(string border, ConsoleColor color)
        {
            ConsoleHelper.WriteColor(border, color);
        }

        private void DisplayCenteredText(string text, ConsoleColor color)
        {
            int padding = (CombatWidth - text.Length) / 2;
            string paddedText = $"║{new string(' ', padding)}{text}{new string(' ', CombatWidth - text.Length - padding - 2)}║";
            ConsoleHelper.WriteColor(paddedText, color);
        }

        private ConsoleColor GetHealthColor(int currentHealth, int maxHealth)
        {
            double percent = (double)currentHealth / maxHealth;
            return percent switch
            {
                > 0.6 => ConsoleColor.Green,
                > 0.3 => ConsoleColor.Yellow,
                _ => ConsoleColor.Red
            };
        }
    }
}