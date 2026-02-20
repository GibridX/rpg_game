using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Services.Game
{
    public class MerchantService
    {
        private readonly GameConfig _config;

        private const int BoxWidth = 38;
        private const string HorizontalLine = "╠══════════════════════════════════════╣";
        private const string TopBorder = "╔══════════════════════════════════════╗";
        private const string BottomBorder = "╚══════════════════════════════════════╝";

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
            // Console.WriteLine($"\nВаше золото: {player.Gold}");
            // ConsoleHelper.WriteColor("\nТовары торговца:", ConsoleColor.Yellow);

            // for (int i = 0; i < merchantItems.Count; i++)
            // {
            //     Console.WriteLine($"{i + 1}. {merchantItems[i].Name} - {merchantItems[i].Value} золота");
            // }

            // DisplayPlayerSellableItems(player);

            // Новая реализация
            Console.Clear();

            DisplayBoxedHeader("Торговец", ConsoleColor.DarkYellow);

            ConsoleHelper.WriteColor($"║ Золото: {player.Gold, -28} ║", ConsoleColor.Yellow);
            ConsoleHelper.WriteColor(HorizontalLine, ConsoleColor.DarkYellow);

            Console.WriteLine();

            DisplayItemsBox("Товары торговца:", merchantItems, ConsoleColor.Green, startNumber: 1);
            Console.WriteLine();

            DisplayPlayerSellableItems(player);

            Console.WriteLine();

            var actions = new[]
            {
                "0 - Закончить торговлю"
            };
            DisplayBoxedMenu("Действия", actions, ConsoleColor.Cyan);

            ConsoleHelper.WriteColor("\nВыберите предмет для покупки/продажи: ", ConsoleColor.Green);
        }

        private void DisplayItemsBox(string title, List<Item> items, ConsoleColor color, int startNumber = 1)
        {
            ConsoleHelper.WriteColor(TopBorder, color);
            ConsoleHelper.WriteColor(CenterText(title, BoxWidth + 2), ConsoleColor.Yellow);
            ConsoleHelper.WriteColor(HorizontalLine, color);

            for (int i = 0; i < items.Count; i++)
            {
                string itemText = $"{startNumber + i}. {items[i].Name} - {items[i].Value} золота";
                ConsoleHelper.WriteColor($"║ {itemText.PadRight(BoxWidth - 2)} ║", ConsoleColor.White);
            }

            ConsoleHelper.WriteColor(BottomBorder, color);
        }

        private void DisplayPlayerSellableItems(Player player)
        {
            var itemsForSale = player.Inventory.GetItemsForSale();

            ConsoleHelper.WriteColor(TopBorder, ConsoleColor.Cyan);
            ConsoleHelper.WriteColor(CenterText("Ваши предметы для продажи", BoxWidth + 2), ConsoleColor.Yellow);
            ConsoleHelper.WriteColor(HorizontalLine, ConsoleColor.Cyan);

            if (itemsForSale.Any())
            {
                for (int i = 0; i < itemsForSale.Count; i++)
                {
                    var item = itemsForSale[i].Value;
                    int sellPrice = item.Value / 2;
                    string itemText = $"{i + 5}. {item.Name} - {sellPrice} золота";
                    ConsoleHelper.WriteColor($"║ {itemText.PadRight(BoxWidth - 2)} ║", ConsoleColor.White);
                }
            }
            else
            {
                string emptyText = "   Нет предметов для продажи";
                ConsoleHelper.WriteColor($"║ {emptyText.PadRight(BoxWidth - 2)} ║", ConsoleColor.DarkGray);
            }

            ConsoleHelper.WriteColor(BottomBorder, ConsoleColor.Cyan);
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
                else
                {
                    var itemsForSale = player.Inventory.GetItemsForSale();
                    int sellIndex = choice - 5;

                    if (sellIndex >= 0 && sellIndex < itemsForSale.Count)
                    {
                        SellItem(player, sellIndex);
                        return true;
                    }
                    else
                    {
                        ShowSimpleMessage("Ошибка", "Неверный выбор!", ConsoleColor.Red);
                        return true;
                    }
                }
            }
            else
            {
                ShowSimpleMessage("Ошибка", "Неверный ввод!", ConsoleColor.Red);
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
                ShowSimpleMessage("Успешная покупка", $"Вы купили {itemToBuy.Name}!", ConsoleColor.Green);
            }
            else
            {
                ShowSimpleMessage("Ошибка", "Недостаточно золота!", ConsoleColor.Red);
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

                ShowSimpleMessage("Успешная продажа", $"Вы продали {itemToSell.Value.Name} за {sellPrice} золота!", ConsoleColor.Yellow);
            }
            else
            {
                ShowSimpleMessage("Ошибка", "Неверный выбор предмета!", ConsoleColor.Red);
            }
        }

        #region Вспомогательные методы отображения

        private void DisplayBoxedHeader(string title, ConsoleColor color)
        {
            ConsoleHelper.WriteColor(TopBorder, color);
            ConsoleHelper.WriteColor(CenterText(title, BoxWidth + 2), color);
            ConsoleHelper.WriteColor(HorizontalLine, color);
        }

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

        private void DisplayBoxedContent(string[] content, ConsoleColor borderColor)
        {
            ConsoleHelper.WriteColor(TopBorder, borderColor);

            foreach (var line in content)
            {
                if (line.Contains("Товары торговца:") || line.Contains("Ваши предметы") || line.Contains("Действия"))
                {
                    ConsoleHelper.WriteColor(CenterText(line, BoxWidth + 2), ConsoleColor.Yellow);
                    ConsoleHelper.WriteColor(HorizontalLine, borderColor);
                }
                else
                {
                    ConsoleHelper.WriteColor($"║ {line.PadRight(BoxWidth - 2)} ║", ConsoleColor.White);
                }
            }

            ConsoleHelper.WriteColor(BottomBorder, borderColor);
        }

        private void ShowSimpleMessage(string title, string message, ConsoleColor color)
        {
            Console.Clear();
            DisplayBoxedHeader(title, color);
            ConsoleHelper.WriteColor($"║ {message.PadRight(BoxWidth - 2)} ║", ConsoleColor.White);
            ConsoleHelper.WriteColor(BottomBorder, color);
            ShowContinuePrompt();
            Console.ReadKey(true);
        }


        private string CenterText(string text, int width)
        {
            if (text.Length >= width - 4)
                return $"║ {text} ║";

            int padding = (width - text.Length - 4) / 2;
            return $"║ {new string(' ', padding)}{text}{new string(' ', padding + (text.Length % 2 == 0 ? 0 : 1))} ║";
        }

        private void ShowContinuePrompt()
        {
            ConsoleHelper.WriteColor("\nНажмите любую клавишу чтобы продолжить...", ConsoleColor.Gray);
        }

        #endregion
    }
}