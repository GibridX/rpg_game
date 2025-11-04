using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Services.Game
{
    public class MerchantService
    {
        private readonly GameConfig _config;

        public MerchantService(GameConfig config)
        {
            _config = config;
        }

        public List<Item> CreateMerchantItems()
        {
            return new List<Item>
            {
                new Item("Большое зелье здоровья", 0, 0, 50, ItemType.Potion, EquipmentType.Other, ConsoleColor.Green),
                new Item("Стальной меч", 0, 0, 100, ItemType.Weapon, EquipmentType.Weapon, ConsoleColor.Yellow),
                new Item("Кожаный доспех", 0, 0, 120, ItemType.Treasure, EquipmentType.Armor, ConsoleColor.Magenta)
            };
        }

        public void DisplayMerchantUI(Player player, List<Item> merchantItems)
        {
            Console.WriteLine($"\nВаше золото: {player.Gold}");
            ConsoleHelper.WriteColor("\nТовары торговца:", ConsoleColor.Yellow);

            for (int i = 0; i < merchantItems.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {merchantItems[i].Name} - {merchantItems[i].Value} золота");
            }

            DisplayPlayerSellableItems(player);
        }

        private void DisplayPlayerSellableItems(Player player)
        {
            ConsoleHelper.WriteColor("\nВаш инвентарь для продажи:", ConsoleColor.Cyan);

            var itemsForSale = player.Inventory.GetItemsForSale();

            if (itemsForSale.Any())
            {
                for (int i = 0; i < itemsForSale.Count; i++)
                {
                    var item = itemsForSale[i].Value;
                    int sellPrice = item.Value / 2;
                    Console.WriteLine($"{i + 5}. {item.Name} - {sellPrice} золота (продажа)");
                }
            }
            else
            {
                Console.WriteLine("Пусто");
            }
        }

        public bool ProcessMerchantInput(string input, Player player, List<Item> merchantItems)
        {
            if (input == "0") return false;

            if (int.TryParse(input, out int choice))
            {
                if (choice >= 1 && choice <= merchantItems.Count)
                {
                    BuyItem(player, merchantItems[choice - 1]);
                    return true;
                }
                else if (choice >= 5 && choice <= 9)
                {
                    SellItem(player, choice - 5);
                    return true;
                }
                else
                {
                    ConsoleHelper.WriteColor("Неверный выбор!", ConsoleColor.Red);
                    return true;
                }
            }
            else
            {
                ConsoleHelper.WriteColor("Неверный ввод!", ConsoleColor.Red);
                return true;
            }
        }

        private void BuyItem(Player player, Item itemToBuy)
        {
            if (player.Gold >= itemToBuy.Value)
            {
                player.Gold -= itemToBuy.Value;

                var boughtItem = new Item(
                    itemToBuy.Name,
                    -1, -1,
                    itemToBuy.Value,
                    itemToBuy.Type,
                    itemToBuy.EquipmentType,
                    itemToBuy.Color
                );

                player.AddItem(boughtItem);
                ConsoleHelper.WriteColor($"Вы купили {itemToBuy.Name}!", ConsoleColor.Green);
            }
            else
            {
                ConsoleHelper.WriteColor("Недостаточно золота!", ConsoleColor.Red);
            }
        }

        private void SellItem(Player player, int itemIndex)
        {
            var itemsForSale = player.Inventory.GetItemsForSale();

            if (itemIndex < itemsForSale.Count && itemIndex >= 0)
            {
                var itemToSell = itemsForSale[itemIndex];

                int sellPrice = itemToSell.Value.Value / 2;

                player.Inventory.RemoveItem(itemToSell.Key);
                player.Gold += sellPrice;
                ConsoleHelper.WriteColor($"Вы продали {itemToSell.Value.Name} за {sellPrice} золота!", ConsoleColor.Yellow);
            }
            else
            {
                ConsoleHelper.WriteColor("Неверный выбор предмета!", ConsoleColor.Red);
            }
        }
    }
}