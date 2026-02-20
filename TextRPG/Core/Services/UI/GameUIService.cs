using System;
using System.Linq;
using TextRPG.Core.Models;
using TextRPG.Core.Enums;
using TextRPG.Core.Models.Inventory.Equipment;
using TextRPG.Core.Models.Inventory.UI;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Services.UI
{
    public class GameUIService
    {
        private readonly EquipmentService _equipmentService;

        public GameUIService(EquipmentService equipmentService)
        {
            _equipmentService = equipmentService;
        }

        // Константы для форматирования
        private const int BoxWidth = 38;
        private const string HorizontalLine = "╠══════════════════════════════════════╣";
        private const string TopBorder = "╔══════════════════════════════════════╗";
        private const string BottomBorder = "╚══════════════════════════════════════╝";

        #region Основные меню

        public void ShowMainMenu()
        {
            var menuItems = new[]
            {
                "I - Инвентарь",
                "H - Использовать зелье",
                "C - Информация о персонаже", 
                "E - Статистика экипировки",
                "M - Перемещение",
                "ESC - Выйти из игры"
            };

            DisplayBoxedMenu("ГЛАВНОЕ МЕНЮ", menuItems, ConsoleColor.Cyan);
            ConsoleHelper.WriteColor("\nВыберите действие: ", ConsoleColor.Green);
        }

        public void ShowMovementMenu()
        {
            var menuItems = new[]
            {
                "W - Вверх",
                "S - Вниз", 
                "A - Влево",
                "D - Вправо",
                "H - Использовать зелье",
                "ESC - Назад"
            };

            DisplayBoxedMenu("ПЕРЕМЕЩЕНИЕ", menuItems, ConsoleColor.Cyan);
            ConsoleHelper.WriteColor("\nВыберите направление: ", ConsoleColor.Green);
        }

        #endregion

        #region Информационные экраны

        public void ShowCharacterInfo(Player player, int currentDepth)
        {
            Console.Clear();
            
            var characterInfo = new[]
            {
                $"Имя: {player.Name}",
                $"Уровень: {player.Level}",
                $"Здоровье: {player.Health}/{player.MaxHealth}",
                $"Атака: {player.Attack}",
                $"Золото: {player.Gold}",
                $"Опыт: {player.Experience}/{player.ExperienceToNextLevel}",
                $"Глубина подземелья: {currentDepth}",
                $"Размер инвентаря: {player.Inventory.Count}/{player.Inventory.Capacity}"
            };

            var equipmentInfo = new[]
            {
                $"Оружие: {player.Inventory.EquippedWeapon?.Name ?? "Нет"}",
                $"Броня: {player.Inventory.EquippedArmor?.Name ?? "Нет"}"
            };

            DisplayBoxedContent("ИНФОРМАЦИЯ О ПЕРСОНАЖЕ", characterInfo, ConsoleColor.Cyan);
            DisplayBoxedContent("ЭКИПИРОВКА", equipmentInfo, ConsoleColor.Yellow);
            
            ShowContinuePrompt();
            Console.ReadKey(true);
        }

        public void ShowEquipmentStats(Player player)
        {
            bool hasEquipment = player.Inventory.EquippedWeapon != null || 
                       player.Inventory.EquippedArmor != null;

            if (!hasEquipment)
            {
                ShowErrorMessage("У вас нет экипировки!");
                ShowContinuePrompt();
                Console.ReadKey(true);
                return;
            }
            
            var weaponStats = player.Inventory.EquippedWeapon != null 
                ? $"{player.Inventory.EquippedWeapon.Name} (+{player.Inventory.EquippedWeapon.Value} к атаке)"
                : "Нет (Базовая атака: " + player.BaseAttack + ")";

            var armorStats = player.Inventory.EquippedArmor != null
                ? $"{player.Inventory.EquippedArmor.Name} (+{player.Inventory.EquippedArmor.Value} к HP)"
                : "Нет (Базовое HP: " + player.BaseMaxHealth + ")";

            var stats = new[]
            {
                $"Оружие: {weaponStats}",
                $"Броня: {armorStats}",
                $"Итоговая атака: {player.Attack}",
                $"Итоговое HP: {player.MaxHealth}"
            };

            DisplayBoxedContent("СТАТИСТИКА ЭКИПИРОВКИ", stats, ConsoleColor.Cyan);
        }

        public void ShowInventorySummary(Player player)
        {
            var summary = new[]
            {
                $"Предметов: {player.Inventory.Count}",
                $"Оружие: {player.Inventory.EquippedWeapon?.Name ?? "Нет"}",
                $"Броня: {player.Inventory.EquippedArmor?.Name ?? "Нет"}",
                $"Золото: {player.Gold}"
            };

            DisplayBoxedContent("ИТОГИ ИНВЕНТАРЯ", summary, ConsoleColor.DarkRed);
        }

        #endregion

        #region Управление инвентарем

        public void ShowInventoryManagement(Player player)
        {
            if (_equipmentService == null)
            {
                ShowErrorMessage("Ошибка: сервис экипировки не инициализирован!");
                return;
            }

            bool inInventory = true;

            while (inInventory && player.Health > 0)
            {
                Console.Clear();
                InventoryUIService.DisplayInventoryManagement(player);

                var input = Console.ReadLine()?.ToLower();

                if (input == "0")
                {
                    inInventory = false;
                }
                else if (!string.IsNullOrEmpty(input))
                {
                    InventoryUIService.ProcessInventoryCommand(input, player, _equipmentService);

                    if (player.Inventory.Count == 0)
                    {
                        ShowInfoMessage("\nИнвентарь пуст. Возврат в главное меню...");
                        inInventory = false;
                    }
                    
                    if (inInventory)
                    {
                        ShowContinuePrompt();
                        Console.ReadKey(true);
                    }
                }
            }

            Console.Clear();
        }

        public void ShowQuickPotionMenu(Player player)
        {
            var potions = player.Inventory.GetItemsByType(ItemType.Potion);
            
            if (!potions.Any())
            {
                ShowErrorMessage("У вас нет зелий!");
                ShowContinuePrompt();
                Console.ReadKey(true);
                return;
            }

            var potionOptions = potions
                .Select((potion, index) => $"{index + 1}. {potion.Name} (+{potion.Value} HP)")
                .Concat(new[] { "0. Отмена" })
                .ToArray();

            DisplayBoxedContent("БЫСТРОЕ ИСПОЛЬЗОВАНИЕ ЗЕЛИЙ", potionOptions, ConsoleColor.Cyan);

            var input = Console.ReadLine();
            if (int.TryParse(input, out int choice) && choice > 0 && choice <= potions.Count)
            {
                var potion = potions[choice - 1];
                var potionKey = player.Inventory.GetItemKey(potion);
                
                if (potionKey != null)
                {
                    player.Heal(potion.Value);
                    player.Inventory.RemoveItem(potionKey);
                    ShowSuccessMessage($"Использовано {potion.Name}!");
                }
            }
        }

        #endregion

        #region Вспомогательные методы отображения

        private void DisplayBoxedMenu(string title, string[] menuItems, ConsoleColor color)
        {
            ConsoleHelper.WriteColor(TopBorder, color);
            ConsoleHelper.WriteColor(CenterText(title, BoxWidth + 2), color);
            ConsoleHelper.WriteColor(HorizontalLine, color);
            
            foreach (var item in menuItems)
            {
                ConsoleHelper.WriteColor($"║ {item.PadRight(BoxWidth - 2)} ║", ConsoleColor.White);
            }
            
            ConsoleHelper.WriteColor(BottomBorder, color);
        }

        private void DisplayBoxedContent(string title, string[] content, ConsoleColor color)
        {
            ConsoleHelper.WriteColor(TopBorder, color);
            ConsoleHelper.WriteColor(CenterText(title, BoxWidth + 2), color);
            ConsoleHelper.WriteColor(HorizontalLine, color);
            
            foreach (var line in content)
            {
                ConsoleHelper.WriteColor($"║ {line.PadRight(BoxWidth - 2)} ║", ConsoleColor.White);
            }
            
            ConsoleHelper.WriteColor(BottomBorder, color);
        }

        private string CenterText(string text, int width)
        {
            if (text.Length >= width - 4) 
                return $"║ {text} ║";
                
            int padding = (width - text.Length - 4) / 2;
            return $"║ {new string(' ', padding)}{text}{new string(' ', padding + (text.Length % 2 == 0 ? 0 : 1))} ║";
        }

        #endregion

        #region Утилиты сообщений

        public void ShowActionFeedback(string action)
        {
            var feedbackMap = new Dictionary<string, (string message, ConsoleColor color)>
            {
                ["inventory"] = ("Открываю инвентарь...", ConsoleColor.Yellow),
                ["potion"] = ("Использование зелья...", ConsoleColor.Yellow),
                ["character"] = ("Информация о персонаже...", ConsoleColor.Yellow),
                ["equipment"] = ("Статистика экипировки...", ConsoleColor.Yellow),
                ["movement"] = ("Перемещение...", ConsoleColor.Yellow),
                ["exit"] = ("Выход из игры...", ConsoleColor.Red),
                ["debug"] = ("Отладочная информация...", ConsoleColor.Magenta)
            };

            if (feedbackMap.TryGetValue(action.ToLower(), out var feedback))
            {
                ConsoleHelper.WriteColor(feedback.message, feedback.color);
            }
        }

        public void ShowErrorMessage(string message) => ConsoleHelper.WriteColor(message, ConsoleColor.Red);
        public void ShowWarningMessage(string message) => ConsoleHelper.WriteColor(message, ConsoleColor.Yellow);
        public void ShowSuccessMessage(string message) => ConsoleHelper.WriteColor(message, ConsoleColor.Green);
        public void ShowInfoMessage(string message) => ConsoleHelper.WriteColor(message, ConsoleColor.Cyan);
        public void ShowContinuePrompt() => ConsoleHelper.WriteColor("Нажмите любую клавишу чтобы продолжить...", ConsoleColor.Gray);

        #endregion
    }
}