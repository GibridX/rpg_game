using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Utils;


namespace TextRPG.Core.Services
{
    public class CombatService : ICombatService
    {
        private readonly Random _random;
        private readonly GameConfig _config;

        public CombatService(GameConfig config)
        {
            _random = new Random();
            _config = config;
        }

        public CombatResult StartCombat(Player player, Enemy enemy)
        {
            ConsoleHelper.WriteColor($"Вы столкнулись с {enemy.Name}!", ConsoleColor.Red);
            ConsoleHelper.WriteColor($"{enemy}", ConsoleColor.DarkRed);

            while (enemy.Health > 0 && player.Health > 0)
            {
                DisplayCombatStatus(player, enemy);
                var action = GetPlayerAction(player);

                if (!ProcessPlayerAction(player, enemy, action))
                    continue;

                if (enemy.Health <= 0) break;

                ProcessEnemyTurn(player, enemy);
            }
            return CreateCombatResult(player, enemy);
        }

        public CombatResult StartBossFight(Player player, Boss boss)
        {
            ConsoleHelper.WriteColor($"\nВСТРЕЧА С БОССОМ: {boss.Name}!", ConsoleColor.DarkRed);
            ConsoleHelper.WriteColor($"{boss}", ConsoleColor.Red);
            ConsoleHelper.WriteColor("Приготовьтесь к тяжелой битве!", ConsoleColor.Yellow);

            while (boss.Health > 0 && player.Health > 0)
            {
                DisplayCombatStatus(player, boss);
                var action = GetPlayerAction(player);

                if (!ProcessPlayerAction(player, boss, action))
                    continue;

                if (boss.Health <= 0) break;

                ProcessBossTurn(player, boss);
            }

            return CreateCombatResult(player, boss);
        }

        private CombatAction GetPlayerAction(Player player)
        {
            ConsoleHelper.WriteColor("\nВыберите действие:", ConsoleColor.Cyan);
            ConsoleHelper.WriteColor("1. Атаковать", ConsoleColor.White);
            ConsoleHelper.WriteColor("2. Защищаться (уменьшает урон на 50%)", ConsoleColor.White);
            ConsoleHelper.WriteColor("3. Использовать предмет", ConsoleColor.White);
            ConsoleHelper.WriteColor("4. Попытаться сбежать", ConsoleColor.White);

            while (true)
            {
                var input = Console.ReadLine();
                switch (input)
                {
                    case "1": return CombatAction.Attack;
                    case "2": return CombatAction.Defend;
                    case "3": return CombatAction.UseItem;
                    case "4": return CombatAction.Escape;
                    default:
                        ConsoleHelper.WriteColor("Неверный выбор! Попробуйте снова.", ConsoleColor.Red);
                        break;
                }
            }
        }

        private bool ProcessPlayerAction(Player player, Enemy enemy, CombatAction action)
        {
            switch (action)
            {
                case CombatAction.Attack:
                    int playerDamage = player.Attack + _random.Next(-_config.PlayerDamageVariance, _config.PlayerDamageVariance + 1);
                    playerDamage = Math.Max(1, playerDamage);

                    enemy.TakeDamage(playerDamage);
                    ConsoleHelper.WriteColor($"Вы нанесли {playerDamage} урона {enemy.Name}!", ConsoleColor.Yellow);
                    return true;

                case CombatAction.Defend:
                    ConsoleHelper.WriteColor("Вы готовитесь к защите...", ConsoleColor.Blue);
                    return true;

                case CombatAction.UseItem:
                    return UseItemInCombat(player);

                case CombatAction.Escape:
                    return AttemptEscape(player, enemy);

                default:
                    return false;
            }
        }

        private void ProcessEnemyTurn(Player player, Enemy enemy)
        {
            int enemyDamage = enemy.CalculateDamage();

            if (_random.Next(100) < (player.Level * _config.DodgePerLevel))
            {
                ConsoleHelper.WriteColor("Вы уклонились от атаки!", ConsoleColor.Cyan);
                return;
            }

            player.TakeDamage(enemyDamage);
            ConsoleHelper.WriteColor($"{enemy.Name} наносит вам {enemyDamage} урона!", ConsoleColor.Red);
        }

        private void ProcessBossTurn(Player player, Boss boss)
        {
            int bossDamage = boss.CalculateDamage();
            string attackDescription = $"{boss.Name} атакует";

            if (_random.Next(100) < boss.SpecialAbilityChance)
            {
                var ability = boss.UseSpecialAbility();
                attackDescription = $"{boss.Name} использует '{ability}'";
                bossDamage = (int)(bossDamage * 1.3); // +30 урона от спобности
            }

            if (_random.Next(100) < (player.Level * (_config.DodgePerLevel / 2)))
            {
                ConsoleHelper.WriteColor($"Вы уклонились от атаки {boss.Name}!", ConsoleColor.Cyan);
                return;
            }

            player.TakeDamage(bossDamage);
            ConsoleHelper.WriteColor($"{attackDescription} и наносит {bossDamage} урона!", ConsoleColor.DarkRed);
        }

        private bool UseItemInCombat(Player player)
        {
            var potions = player.Inventory.Values
                .Where(item => item.Type == ItemType.Potion)
                .ToList();

            if (!potions.Any())
            {
                ConsoleHelper.WriteColor("У вас нет зелий для использования!", ConsoleColor.Red);
                return false;
            }

            ConsoleHelper.WriteColor("\nВыберите зелье:", ConsoleColor.Cyan);
            for (int i = 0; i < potions.Count; i++)
            {
                ConsoleHelper.WriteColor($"{i + 1}. {potions[i].Name} (+{potions[i].Value} HP)", ConsoleColor.White);
            }
            ConsoleHelper.WriteColor("0. Отмена", ConsoleColor.Gray);

            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= potions.Count)
            {
                var selectedPotion = potions[choice - 1];
                var potionKey = player.Inventory.First(kvp => kvp.Value == selectedPotion).Key;

                player.Heal(selectedPotion.Value);
                player.Inventory.Remove(potionKey);
                ConsoleHelper.WriteColor($"Вы использовали {selectedPotion.Name}!", ConsoleColor.Green);
                return true;
            }
            return false;
        }

        private bool AttemptEscape(Player player, Enemy enemy)
        {
            if (_random.Next(100) < _config.EscapeChance)
            {
                ConsoleHelper.WriteColor("Вы успешно сбежали!", ConsoleColor.Green);
                return false;
            }
            else
            {
                ConsoleHelper.WriteColor("Побег не удался!", ConsoleColor.Red);
                return true;
            }
        }

        private void DisplayCombatStatus(Player player, Enemy enemy)
        {
            ConsoleHelper.WriteColor($"\nВаше HP: {player.Health}/{player.MaxHealth}", ConsoleColor.Green);
            ConsoleHelper.WriteColor($"{enemy.Name} HP: {enemy.Health}/{enemy.MaxHealth}", ConsoleColor.Red);
        }
        
        private CombatResult CreateCombatResult(Player player, Enemy enemy)
        {
            bool playerWon = enemy.Health <= 0;

            if (playerWon)
            {
                ConsoleHelper.WriteColor($"\n{enemy.Name} побежден!", ConsoleColor.Green);
                return new CombatResult(true, enemy.ExperienceReward, enemy.GoldReward);
            }
            else
            {
                ConsoleHelper.WriteColor($"\nВы погибли...", ConsoleColor.Red);
                return new CombatResult(false, 0, 0);
            }
        }
    }
}