using TextRPG.Core.Enums;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Services.UI
{
    public class GameUIService
    {
        public void ShowMainMenu()
        {
            ConsoleHelper.WriteColor("\nДоступные действия:", ConsoleColor.Cyan);
            ConsoleHelper.WriteColorInline("1. ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline("Перемещение", ConsoleColor.White);
            ConsoleHelper.WriteColorInline("   2. ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline("Инвентарь", ConsoleColor.White);
            Console.WriteLine();
            ConsoleHelper.WriteColorInline("3. ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline("Информация о персонаже", ConsoleColor.White);
            ConsoleHelper.WriteColorInline("   4. ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline("Выйти из игры", ConsoleColor.White);
            Console.WriteLine();
            ConsoleHelper.WriteColorInline("\nВыберите действие: ", ConsoleColor.Green);
        }

        public void ShowCharacterInfo(Player player, int currentDepth)
        {
            Console.Clear();
            ConsoleHelper.WriteColor("╔══════════════════════════════════════════╗", ConsoleColor.Cyan);
            ConsoleHelper.WriteColor("║           ИНФОРМАЦИЯ О ПЕРСОНАЖЕ        ║", ConsoleColor.Cyan);
            ConsoleHelper.WriteColor("╚══════════════════════════════════════════╝", ConsoleColor.Cyan);

            ConsoleHelper.WriteColorInline($" Имя: ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"{player.Name}\n", ConsoleColor.White);

            ConsoleHelper.WriteColorInline($" Уровень: ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"{player.Level}\n", ConsoleColor.White);

            ConsoleHelper.WriteColorInline($" Здоровье: ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"{player.Health}/{player.MaxHealth}\n", ConsoleColor.Red);

            ConsoleHelper.WriteColorInline($" Атака: ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"{player.Attack}\n", ConsoleColor.White);

            ConsoleHelper.WriteColorInline($" Золото: ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"{player.Gold}\n", ConsoleColor.Yellow);

            ConsoleHelper.WriteColorInline($" Опыт: ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"{player.Experience}/{player.ExperienceToNextLevel}\n", ConsoleColor.Blue);

            ConsoleHelper.WriteColorInline($" Глубина подземелья: ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"{currentDepth}\n", ConsoleColor.White);

            ConsoleHelper.WriteColorInline($" Размер инвентаря: ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"{player.Inventory.Count}\n", ConsoleColor.White);

            ConsoleHelper.WriteColor("\nЭкипировка:", ConsoleColor.Cyan);
            ConsoleHelper.WriteColorInline($" Оружие: ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"{(player.EquippedWeapon?.Name ?? "Нет")}\n", ConsoleColor.White);

            ConsoleHelper.WriteColorInline($" Броня: ", ConsoleColor.Yellow);
            ConsoleHelper.WriteColorInline($"{(player.EquippedArmor?.Name ?? "Нет")}\n", ConsoleColor.White);

            ConsoleHelper.WriteColor("\nНажмите любую клавишу чтобы продолжить...", ConsoleColor.Green);
            Console.ReadKey(true);
        }

        public void ShowInventory(Player player)
        {
            Console.Clear();
            Console.WriteLine("\n=== Инвентарь ===");

            Console.WriteLine("Экипировано:");
            Console.WriteLine($"Оружие: {(player.EquippedWeapon?.Name ?? "Нет")}");
            Console.WriteLine($"Броня: {(player.EquippedArmor?.Name ?? "Нет")}");
            Console.WriteLine();

            if (!player.Inventory.Any())
            {
                Console.WriteLine("Ваш инвентарь пуст.");
            }
            else
            {
                var inventoryList = player.Inventory.Values.ToList();

                for (int i = 0; i < inventoryList.Count; i++)
                {
                    string equippedMark = "";
                    if (inventoryList[i] == player.EquippedWeapon || inventoryList[i] == player.EquippedArmor)
                    {
                        equippedMark = " [Экипировано]";
                    }
                    Console.WriteLine($"{i + 1}. {inventoryList[i]}{equippedMark}");
                }

                Console.WriteLine("\nВыберите действие:");
                if (player.Inventory.Count <= 9)
                {
                    Console.WriteLine($"1-{player.Inventory.Count} - Использовать/Экипировать предмет");
                }
                else
                {
                    Console.WriteLine($"1-9 - Использовать/Экипировать предмет");
                    Console.WriteLine($"Или введите номер предмета (1-{player.Inventory.Count})");
                }
                Console.WriteLine("U - Снять экипировку");
                Console.WriteLine("0 - Отмена");
                
                ProcessInventoryInput(player, inventoryList);
            }
            
            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
        }

        private void ProcessInventoryInput(Player player, List<Item> inventoryList)
        {
            var input = Console.ReadLine()?.ToLower();
            if (string.IsNullOrEmpty(input)) return;

            if (input == "0") return;

            if (input == "u")
            {
                UnequipItem(player);
            }
            else if (int.TryParse(input, out int choice))
            {
                UseOrEquipItem(player, inventoryList, choice);
            }
        }

        private void UnequipItem(Player player)
        {
            Console.WriteLine("Что снять?");
            Console.WriteLine("1 - Оружие");
            Console.WriteLine("2 - Броня");
            var unequipChoice = Console.ReadLine();
            
            if (unequipChoice == "1")
            {
                if (player.EquippedWeapon != null)
                    player.UnequipItem(player.EquippedWeapon);
                else
                    Console.WriteLine("Оружие не экипировано!");
            }
            else if (unequipChoice == "2")
            {
                if (player.EquippedArmor != null)
                    player.UnequipItem(player.EquippedArmor);
                else
                    Console.WriteLine("Броня не экипирована!");
            }
        }

        private void UseOrEquipItem(Player player, List<Item> inventoryList, int choice)
        {
            if (choice > 0 && choice <= inventoryList.Count)
            {
                var selectedItem = inventoryList[choice - 1];
                var itemKey = player.GetItemKey(selectedItem);

                if (string.IsNullOrEmpty(itemKey))
                {
                    Console.WriteLine("Ошибка: не удалось найти ключ предмета!");
                    return;
                }

                switch (selectedItem.Type)
                {
                    case ItemType.Potion:
                        int healValue = selectedItem.Value;
                        player.Heal(healValue);
                        player.Inventory.Remove(itemKey);
                        Console.WriteLine($"Вы использовали {selectedItem.Name}!");
                        break;

                    case ItemType.Weapon:
                    case ItemType.Treasure:
                        if (selectedItem.EquipmentType == EquipmentType.Weapon ||
                            selectedItem.EquipmentType == EquipmentType.Armor)
                            player.EquipItem(itemKey);
                        else
                            Console.WriteLine("Этот предмет нельзя экипировать.");
                        break;

                    default:
                        Console.WriteLine("Этот предмет нельзя использовать.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Неверный номер предмета!");
            }
        }
    }
}