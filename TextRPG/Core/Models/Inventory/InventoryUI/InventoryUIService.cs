using System;
using System.Linq;
using TextRPG.Core.Models;
using TextRPG.Core.Enums;
using TextRPG.Core.Models.Inventory.Equipment;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Models.Inventory.UI
{
    public class InventoryUIService
    {
        public static void DisplayInventory(Player player)
        {
            Console.WriteLine("\n=== ИНВЕНТАРЬ ===");
            Console.WriteLine($"Оружие: {(player.Inventory.EquippedWeapon?.Name ?? "Нет")}");
            Console.WriteLine($"Броня: {(player.Inventory.EquippedArmor?.Name ?? "Нет")}");
            Console.WriteLine($"Золото: {player.Gold}");
            Console.WriteLine($"Свободно: {player.Inventory.Count}/{player.Inventory.Capacity}");
            Console.WriteLine();

            if (!player.Inventory.Items.Any())
            {
                Console.WriteLine("Ваш инвентарь пуст.");
            }
            else
            {
                var inventoryList = player.Inventory.Items.Values.ToList();

                for (int i = 0; i < inventoryList.Count; i++)
                {
                    string equippedMark = inventoryList[i] == player.Inventory.EquippedWeapon ||
                                        inventoryList[i] == player.Inventory.EquippedArmor ? " [Экипировано]" : "";
                    Console.WriteLine($"{i + 1}. {inventoryList[i]}{equippedMark}");
                }
            }
        }

        public static void DisplayInventoryManagement(Player player)
        {
            DisplayInventory(player);
            Console.WriteLine("\n--- Управление инвентарем ---");
            Console.WriteLine("Введите номер предмета для экипировки/использования");
            Console.WriteLine("Введите 'u1' для снятия оружия, 'u2' для снятия брони");
            Console.WriteLine("Введите 'd' + номер для удаления предмета");
            Console.WriteLine("Введите '0' для выхода");
        }

        public static bool IsValidInventoryChoice(string input, int inventoryCount)
        {
            if (input == "0") return true;
            
            if (input.StartsWith("u") || input.StartsWith("d"))
            {
                if (input == "u1" || input == "u2") return true;
                
                if (input.Length > 1 && int.TryParse(input.Substring(1), out int index))
                {
                    return index > 0 && index <= inventoryCount;
                }
                return false;
            }
            
            return int.TryParse(input, out int choice) && choice > 0 && choice <= inventoryCount;
        }

        public static void ProcessInventoryCommand(string input, Player player, EquipmentService equipmentService)
        {
            if (input == "0") return;

            var inventoryList = player.Inventory.Items.Values.ToList();

            if (input == "u1")
            {
                var result = equipmentService.UnequipWeapon();
                ConsoleHelper.WriteColor(result.Message, result.Success ? ConsoleColor.Green : ConsoleColor.Red);
            }
            else if (input == "u2")
            {
                var result = equipmentService.UnequipArmor();
                ConsoleHelper.WriteColor(result.Message, result.Success ? ConsoleColor.Green : ConsoleColor.Red);
            }
            else if (input.StartsWith("d"))
            {
                if (int.TryParse(input.Substring(1), out int deleteIndex) && deleteIndex > 0 && deleteIndex <= inventoryList.Count)
                {
                    var item = inventoryList[deleteIndex - 1];
                    var itemKey = player.Inventory.GetItemKey(item);
                    if (itemKey != null && player.Inventory.RemoveItem(itemKey))
                    {
                        ConsoleHelper.WriteColor($"Предмет {item.Name} удален из инвентаря.", ConsoleColor.Yellow);
                    }
                    else
                    {
                        ConsoleHelper.WriteColor("Не удалось удалить предмет!", ConsoleColor.Red);
                    }
                }
            }
            else if (int.TryParse(input, out int choice))
            {
                if (choice > 0 && choice <= inventoryList.Count)
                {
                    var item = inventoryList[choice - 1];
                    var itemKey = player.Inventory.GetItemKey(item);
                    
                    if (itemKey != null)
                    {
                        if (item.Type == ItemType.Potion)
                        {
                            UsePotion(player, item, itemKey);
                        }
                        else
                        {
                            var result = equipmentService.EquipItem(itemKey);
                            ConsoleHelper.WriteColor(result.Message, result.Success ? ConsoleColor.Green : ConsoleColor.Red);
                        }
                    }
                }
            }
        }

        private static void UsePotion(Player player, Item potion, string itemKey)
        {
            if (potion.Name.Contains("здоровья", StringComparison.OrdinalIgnoreCase))
            {
                int healAmount = potion.Value;
                player.Heal(healAmount);
                player.Inventory.RemoveItem(itemKey);
                ConsoleHelper.WriteColor($"Использовано {potion.Name}!", ConsoleColor.Green);
            }
            else
            {
                ConsoleHelper.WriteColor("Это зелье нельзя использовать напрямую.", ConsoleColor.Red);
            }
        }
    }
}