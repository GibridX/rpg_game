using System;
using System.Collections.Generic;
using System.Linq;
using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;

namespace TextRPG
{

    public static class ConsoleHelper
    {
        public static void WriteColor(string text, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteColorInline(string text, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }
    }

    public static class SimpleHotkeyHandler
    {
        public static bool CheckForHotkeys()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);

                if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.D:
                            HandleCtrlD();
                            return true;
                    }
                }
            }
            return false;
        }

        private static void HandleCtrlD()
        {
            Console.WriteLine();
            ConsoleHelper.WriteColor("[Ctrl+D] Отладочная информация - функция в разработке", ConsoleColor.Yellow);
            ConsoleHelper.WriteColor("Нажмите любую клавишу чтобы продолжить...", ConsoleColor.Gray);
            Console.ReadKey(true);
        }
    }

    public class Game
    {
        private Player player = null!;
        private Dungeon currentDungeon = null!;
        private LevelGenerator generator;
        private Random random;
        private bool needsClear = true;
        private int currentDepth = 1;
        private bool bossDefeated = false;

        public Game()
        {
            generator = new LevelGenerator(12, 12);
            random = new Random();
            InitializeGame();
        }

        public bool IsExitActive()
        {
            return bossDefeated;
        }

        private void InitializeGame(bool isNewGame = true)
        {
            if (isNewGame)
            {
                player = new Player("Герой", 0, 0);
                currentDepth = 1;
                bossDefeated = false;
            }
            else
            {
                ConsoleHelper.WriteColor($"\nВы спускаетесь на новые грубины подземелья {currentDepth}...", ConsoleColor.Cyan);
                bossDefeated = false;
            }

            currentDungeon = generator.GenerateDungeon($"Древний склеп - Уровень {currentDepth}", 20, currentDepth);

            var startRoom = currentDungeon.Rooms.FirstOrDefault(r => r.Type == RoomType.Start);
            if (startRoom == null)
            {
                startRoom = currentDungeon.Rooms.FirstOrDefault();
                if (startRoom == null)
                {
                    startRoom = new Room(0, 0, RoomType.Start);
                    currentDungeon.AddRoom(startRoom);
                }
                else
                {
                    startRoom.Type = RoomType.Start;
                }
            }
            player.X = startRoom.X;
            player.Y = startRoom.Y;
            startRoom.IsExplored = true;

            if (!isNewGame)
            {
                ConsoleHelper.WriteColor($"Вы вошли в подземелье уровня {currentDepth}. Будьте осторожны!", ConsoleColor.Yellow);
            }
        }

        private void ClearIfNeeded()
        {
            if (needsClear)
            {
                Console.Clear();
                DisplayGameHeader();
                needsClear = false;
            }
        }

        private void DisplayGameHeader()
        {
            ConsoleHelper.WriteColor(@"
╔══════════════════════════════════════════════════════════════╗
║                   ТЕКСТОВАЯ RPG - ПОДЗЕМЕЛЬЕ                 ║
║                 Древний Склеп - Глубина: " + $"{currentDepth,2}" + @"                  ║
╚══════════════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
        }

        private void MarkForClear()
        {
            needsClear = true;
        }

        private void DisplayCompactStatusBar()
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
            else if (player.TurnsSinceLastRest < GameConfig.RestRoomCooldown)
            {
                int turnsRemaining = GameConfig.RestRoomCooldown - player.TurnsSinceLastRest;
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

        public void Start()
        {
            try
            {
                DisplayGameHeader();
                ConsoleHelper.WriteColor("Нажмите любую клавишу чтобы начать...", ConsoleColor.Green);
                Console.ReadKey(true);

                InitializeGame(isNewGame: true);

                while (player.Health > 0)
                {
                    ClearIfNeeded();
                    DisplayCompactStatusBar();
                    currentDungeon.DisplayMiniMap(player, bossDefeated);

                    if (!ProcessPlayerInput())
                        break;

                    if (player.Health <= 0)
                    {
                        ShowGameOverScreen();
                        return;
                    }

                    MarkForClear();
                }

                if (player.Health <= 0)
                {
                    ShowGameOverScreen();
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteColor($"Критическая ошибка: {ex.Message}", ConsoleColor.Red);
                Console.ReadKey();
            }
        }

        private bool ProcessPlayerInput()
        {
            if (SimpleHotkeyHandler.CheckForHotkeys())
            {
                MarkForClear();
                return true;
            }

            ShowMainMenu();
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    MovePlayer();
                    break;
                case "2":
                    ShowInventory();
                    break;
                case "3":
                    ShowCharacterInfo();
                    break;
                case "4":
                    return false;
                default:
                    ConsoleHelper.WriteColor("Неверный выбор! Нажмите любую клавишу...", ConsoleColor.Red);
                    Console.ReadKey(true);
                    MarkForClear();
                    break;
            }

            return true;
        }

        private void ShowMainMenu()
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

        private void Rest()
        {
            ConsoleHelper.WriteColor("\n Комната отдыха", ConsoleColor.DarkGreen);

            if (player.TurnsSinceLastRest >= 0 && player.TurnsSinceLastRest < GameConfig.RestRoomCooldown)
            {
                int turnsRemaining = GameConfig.RestRoomCooldown - player.TurnsSinceLastRest;
                ConsoleHelper.WriteColor($"Комната наполняется исцеляющей магией... Вернитесь через {turnsRemaining} ходов.", ConsoleColor.Yellow);
                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
                return;
            }

            ConsoleHelper.WriteColor("Здесь царит спокойная атмосфера. Вы чувствуете, как силы возвращаются к вам.", ConsoleColor.Green);

            int healAmount = player.MaxHealth - player.Health;
            if (healAmount > 0)
            {
                player.Heal(healAmount);
                ConsoleHelper.WriteColor($"Вы полностью восстановили здоровье! +{healAmount} HP", ConsoleColor.Green);
            }
            else
            {
                ConsoleHelper.WriteColor("Ваше здоровье уже на максимуме.", ConsoleColor.Gray);
            }

            player.ActivateRestCooldown();

            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom != null)
            {
                currentRoom.Type = RoomType.Empty;
                currentRoom.Description = "Пустая комната отдыха. Исцеляющая магия иссякла.";
            }

            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
        }

        private void VisitMerchant()
        {
            ConsoleHelper.WriteColor("\nВстреча с торговцем", ConsoleColor.DarkYellow);
            ConsoleHelper.WriteColor("'Приветствую, путник! Хочешь взглянуть на мой товар?'", ConsoleColor.Yellow);

            var merchantItems = CreateMerchantItems();

            bool trading = true;
            while (trading && player.Health > 0)
            {
                DisplayMerchantUI(merchantItems);
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    ConsoleHelper.WriteColor("Неверный ввод!", ConsoleColor.Red);
                    continue;
                }

                trading = ProcessMerchantInput(input, merchantItems);

                if (trading)
                {
                    Console.WriteLine("\nНажмите любую клавишу чтобы продолжить...");
                    Console.ReadKey(true);
                }
            }

            if (player.Health > 0)
            {
                ConsoleHelper.WriteColor("'Возвращайся, если понадобятся припасы!'", ConsoleColor.Yellow);
            }
        }

        private List<Item> CreateMerchantItems()
        {
            return new List<Item>
            {
                new Item("Большое зелье здоровья", 0, 0, 50, ItemType.Potion, EquipmentType.Other, ConsoleColor.Green),
                new Item("Стальной меч", 0, 0, 100, ItemType.Weapon, EquipmentType.Weapon, ConsoleColor.Yellow),
                new Item("Кожаный доспех", 0, 0, 120, ItemType.Treasure, EquipmentType.Armor, ConsoleColor.Magenta)
            };
        }

        private void DisplayPlayerSellableItems()
        {
            ConsoleHelper.WriteColor("\nВаш инвентарь для продажи:", ConsoleColor.Cyan);

            var itemsForSale = player.Inventory
                .Where(kvp => kvp.Value != player.EquippedWeapon && kvp.Value != player.EquippedArmor)
                .ToList();

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

        private void BuyItem(Item itemToBuy)
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

        private void SellItem(int itemIndex)
        {
            var itemsForSale = player.Inventory
                .Where(kvp => kvp.Value != player.EquippedWeapon && kvp.Value != player.EquippedArmor)
                .ToList();

            if (itemIndex < itemsForSale.Count && itemIndex >= 0)
            {
                var itemToSell = itemsForSale[itemIndex];

                if (itemToSell.Value == player.EquippedWeapon || itemToSell.Value == player.EquippedArmor)
                {
                    ConsoleHelper.WriteColor("Нельзя продать экипированный предмет! Сначала снимите его.", ConsoleColor.Red);
                    return;
                }

                int sellPrice = itemToSell.Value.Value / 2;

                player.Inventory.Remove(itemToSell.Key);
                player.Gold += sellPrice;
                ConsoleHelper.WriteColor($"Вы продали {itemToSell.Value.Name} за {sellPrice} золота!", ConsoleColor.Yellow);
            }
            else
            {
                ConsoleHelper.WriteColor("Неверный выбор предмета!", ConsoleColor.Red);
            }
        }

        private void DisplayMerchantUI(List<Item> merchantItems)
        {
            Console.WriteLine($"\nВаше золото: {player.Gold}");
            ConsoleHelper.WriteColor("\nТовары торговца:", ConsoleColor.Yellow);

            for (int i = 0; i < merchantItems.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {merchantItems[i].Name} - {merchantItems[i].Value} золота");
            }

            DisplayPlayerSellableItems();
        }

        private bool ProcessMerchantInput(string? input, List<Item> merchantItems)
        {
            if (input == "0")
            {
                return false;
            }

            if (int.TryParse(input, out int choice))
            {
                if (choice >= 1 && choice <= merchantItems.Count)
                {
                    BuyItem(merchantItems[choice - 1]);
                }
                else if (choice >= 5 && choice <= 9)
                {
                    var itemsForSale = player.Inventory
                    .Where(kvp => kvp.Value != player.EquippedWeapon && kvp.Value != player.EquippedArmor)
                    .ToList();

                    if (!itemsForSale.Any())
                    {
                        ConsoleHelper.WriteColor("Нет предметов для продажи!", ConsoleColor.Red);
                        return true;
                    }

                    int itemIndex = choice - 5;
                    if (itemIndex >= 0 && itemIndex < itemsForSale.Count)
                    {
                        SellItem(itemIndex);
                    }
                    else
                    {
                        ConsoleHelper.WriteColor("Неверный выбор предмета!", ConsoleColor.Red);
                    }
                }
                else
                {
                    ConsoleHelper.WriteColor("Неверный выбор!", ConsoleColor.Red);
                }
            }
            else
            {
                ConsoleHelper.WriteColor("Неверный ввод!", ConsoleColor.Red);
            }

            return true;
        }

        private void ShowInventory()
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
                var input = Console.ReadLine()?.ToLower();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Неверный ввод!");
                    return;
                }

                if (input == "0") return;

                if (input == "u")
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
                else if (int.TryParse(input, out int choice))
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
                else if (input != "u" && input != "0")
                {
                    Console.WriteLine("Неверный ввод!");
                }
            }
            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
            MarkForClear();
        }

        private void LootRoom(Room room)
        {
            if (room.Items.Any())
            {
                foreach (var item in room.Items.ToList())
                {
                    var inventoryItem = item.CreateCopy(-1, -1);
                    player.AddItem(inventoryItem);
                    room.Items.Remove(item);
                    Console.WriteLine($"Вы нашли {item.Name}!");

                    if (item.Type == ItemType.Potion)
                    {
                        player.Heal(item.Value);
                        Console.WriteLine($"Зелье восстановило {item.Value} здоровья!");
                    }
                    else if (item.Type == ItemType.Weapon)
                    {
                        player.Attack += item.Value / 5;
                        Console.WriteLine($"Оружие увеличило вашу атаку!");
                    }
                }
                room.Type = RoomType.Empty;

                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
            }
            else
            {
                Console.WriteLine("Сокровищница пуста.");
                room.Type = RoomType.Empty;

                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
            }
        }

        private void BossFight()
        {
            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom == null) return;

            int bossHealth = 80 + (player.Level * GameConfig.BossHealthMuliplier);
            int bossAttack = 10 + (player.Level * GameConfig.BossAttackMultiplier);

            Console.WriteLine($"\nБитва с Боссом! Здоровье: {bossHealth}, Атака: {bossAttack}");

            while (bossHealth > 0 && player.Health > 0)
            {
                Console.WriteLine($"Дракон HP: {bossHealth}, Ваше HP: {player.Health}");
                Console.WriteLine("1. Атаковать\n2. Использовать зелье\n3. Защищаться");

                var choice = Console.ReadLine();
                int damageTaken = bossAttack;

                switch (choice)
                {
                    case "1":
                        bossHealth -= player.Attack;
                        Console.WriteLine($"Вы нанесли {player.Attack} урона!");
                        break;
                    case "2":
                        var potions = player.Inventory.Values
                            .Where(item => item.Type == ItemType.Potion)
                            .ToList();

                        if (potions.Any())
                        {
                            var potion = potions.First();
                            var potionKey = player.Inventory.FirstOrDefault(kvp => kvp.Value == potion).Key;
                            player.Heal(potion.Value);
                            player.Inventory.Remove(potionKey);
                            Console.WriteLine($"Вы восстановили {potion.Value} здоровья!");
                            damageTaken = 0;
                        }
                        else
                        {
                            Console.WriteLine("У вас нет зелий!");
                            continue;
                        }
                        break;
                    case "3":
                        damageTaken = bossAttack / 2;
                        Console.WriteLine("Вы защищаетесь! Урон уменьшен.");
                        break;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        continue;
                }
                if (bossHealth > 0 && damageTaken > 0)
                {
                    player.TakeDamage(damageTaken);
                    Console.WriteLine($"Дракон наносит {damageTaken} урона!");
                }
            }

            if (bossHealth <= 0)
            {
                Console.WriteLine("Вы победили дракона! Вы настоящий герой!");

                bossDefeated = true;

                if (currentRoom.Items.Any())
                {
                    foreach (var item in currentRoom.Items.ToList())
                    {
                        player.AddItem(item);
                        currentRoom.Items.Remove(item);
                        Console.WriteLine($"Вы нашли {item.Name}!");
                    }
                }

                currentRoom.Type = RoomType.Exit;

                ConsoleHelper.WriteColor("Портал выхода активирован! Теперь вы можете покинуть подземелье.", ConsoleColor.Yellow);

                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);

                if (ShowVictoryScreen())
                {
                    currentDepth++;
                    InitializeGame(isNewGame: false);
                    MarkForClear();
                }
                else
                {
                    ConsoleHelper.WriteColor("Спасибо за игру! До новых встреч!", ConsoleColor.Green);
                    return;
                }
            }
            else
            {
                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
            }
        }

        private void MovePlayer()
        {
            ConsoleHelper.WriteColor("\n Куда двигаемся?", ConsoleColor.Cyan);
            ConsoleHelper.WriteColor("W - вверх, S - вниз, A - влево, D - вправо, 0 - отмена", ConsoleColor.White);

            var direction = Console.ReadLine()?.ToLower();

            if (string.IsNullOrEmpty(direction))
            {
                ConsoleHelper.WriteColor("Неверное направление!", ConsoleColor.Red);
                return;
            }

            int newX = player.X, newY = player.Y;

            switch (direction)
            {
                case "w": newY--; break;
                case "s": newY++; break;
                case "a": newX--; break;
                case "d": newX++; break;
                case "0":
                    ConsoleHelper.WriteColor("Перемещение отменено.", ConsoleColor.Yellow);
                    Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                    Console.ReadKey(true);
                    return;
                default:
                    ConsoleHelper.WriteColor("Неверное направление!", ConsoleColor.Red);
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey(true);
                    return;
            }

            if (newX < 0 || newX >= currentDungeon.Width || newY < 0 || newY >= currentDungeon.Height)
            {
                ConsoleHelper.WriteColor(" Вы достигли границы подземелья! Дальше бога нет.", ConsoleColor.Red);
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }

            var newRoom = currentDungeon.GetRoom(newX, newY);
            if (newRoom != null)
            {
                player.X = newX;
                player.Y = newY;

                player.IncrementTurnCounter();

                if (!newRoom.IsExplored)
                {
                    newRoom.IsExplored = true;
                    ConsoleHelper.WriteColor(" Вы обнаружили новую комнату!", ConsoleColor.Green);
                }
                else
                {
                    ConsoleHelper.WriteColor($" Вы переместились в комнату ({newX}, {newY})", ConsoleColor.Gray);
                }

                ProcessCurrentRoom();
            }
            else
            {
                ConsoleHelper.WriteColor(" Туда нельзя двигаться! Это стена.", ConsoleColor.Red);
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey(true);
            }
        }

        private void ProcessCurrentRoom()
        {
            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom == null)
            {
                ConsoleHelper.WriteColor("Ошибка: комната не найдена!", ConsoleColor.Red);

                try
                {
                    var startRoom = currentDungeon.Rooms.FirstOrDefault(r => r.Type == RoomType.Start);
                    if (startRoom != null)
                    {
                        player.X = startRoom.X;
                        player.Y = startRoom.Y;
                        ConsoleHelper.WriteColor("Вы возвращены в стартовую комнату.", ConsoleColor.Yellow);
                    }
                    else
                    {
                        var anyRoom = currentDungeon.Rooms.FirstOrDefault();
                        if (anyRoom != null)
                        {
                            player.X = anyRoom.X;
                            player.Y = anyRoom.Y;
                            ConsoleHelper.WriteColor("Вы перемещены в случайную комнату.", ConsoleColor.Yellow);
                        }
                        else
                        {
                            ConsoleHelper.WriteColor("Критическая ошибка: в подземелье нет комнат!", ConsoleColor.Red);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteColor($"Ошибка при восстановлении позиции: {ex.Message}", ConsoleColor.Red);
                }
                return;
            }

            if (currentRoom.Type != RoomType.Rest)
            {
                player.IncrementTurnCounter();
            }

            switch (currentRoom.Type)
            {
                case RoomType.Enemy:
                    Combat();
                    break;
                case RoomType.Boss:
                    BossFight();
                    break;
                case RoomType.Treasure:
                    LootRoom(currentRoom);
                    break;
                case RoomType.Merchant:
                    VisitMerchant();
                    break;
                case RoomType.Rest:
                    Rest();
                    break;
                case RoomType.Exit:
                    if (ShowVictoryScreen())
                    {
                        currentDepth++;
                        InitializeGame(isNewGame: false);
                        MarkForClear();
                    }
                    else
                    {
                        ConsoleHelper.WriteColor("Спасибо за игру! До новых встреч!", ConsoleColor.Green);
                        return;
                    }
                    break;
            }
        }

        private void ShowCharacterInfo()
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
            MarkForClear();
        }

        private void Combat()
        {
            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom == null) return;

            int enemyLevel = Math.Max(1, player.Level - 1 + currentDepth);
            int enemyHealth = Math.Min(
                GameConfig.MinEnemyHealth + (enemyLevel * GameConfig.EnemyHealthPerLevel),
                GameConfig.MaxEnemyHealth + (currentDepth * 20));
            int enemyAttack = GameConfig.BaseEnemyAttack + (enemyLevel * GameConfig.EnemyAttackPerLevel);
            int enemyGold = GameConfig.BaseEnemyGold + (enemyLevel * GameConfig.EnemyGoldPerLevel);

            ConsoleHelper.WriteColor($"\n Вы столкнулись с врагом (Ур. {player.Level})!", ConsoleColor.Red);
            ConsoleHelper.WriteColor($" Здоровье врага: {enemyHealth}, Атака: {enemyAttack}", ConsoleColor.DarkRed);

            while (enemyHealth > 0 && player.Health > 0)
            {
                ConsoleHelper.WriteColor($"\n Ваше HP: {player.Health}", ConsoleColor.Green);
                ConsoleHelper.WriteColor($" Враг HP: {enemyHealth}", ConsoleColor.Red);
                ConsoleHelper.WriteColor("1. Атаковать\n2. Бежать (50% шанс)\n3. Использовать предмет", ConsoleColor.White);

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        int baseDamage = player.Attack;
                        int variance = random.Next(0, GameConfig.PlayerDamageVariance * 2 + 1) - GameConfig.PlayerDamageVariance;
                        int damage = Math.Max(1, baseDamage + variance);
                        enemyHealth -= damage;
                        ConsoleHelper.WriteColor($" Вы нанесли {damage} урона!", ConsoleColor.Yellow);

                        if (enemyHealth > 0)
                        {
                            player.TakeDamage(enemyAttack);
                            ConsoleHelper.WriteColor($" Враг нанес {enemyAttack} урона!", ConsoleColor.Red);
                        }
                        break;
                    case "2":
                        if (random.Next(100) < GameConfig.EscapeChance)
                        {
                            ConsoleHelper.WriteColor(" Вы успешно сбежали!", ConsoleColor.Green);
                            return;
                        }
                        else
                        {
                            ConsoleHelper.WriteColor(" Побег не удался! Враг атакует!", ConsoleColor.Red);
                            player.TakeDamage(enemyAttack);
                            ConsoleHelper.WriteColor($" Враг нанес {enemyAttack} урона!", ConsoleColor.Red);
                        }
                        break;
                    case "3":
                        if (UseItemInCombat())
                        {
                            if (enemyHealth > 0)
                            {
                                player.TakeDamage(enemyAttack);
                                ConsoleHelper.WriteColor($" Враг нанес {enemyAttack} урона!", ConsoleColor.Red);
                            }
                        }
                        else
                        {
                            player.TakeDamage(enemyAttack);
                            ConsoleHelper.WriteColor($" Враг нанес {enemyAttack} урона!", ConsoleColor.Red);
                        }
                        break;

                    default:
                        ConsoleHelper.WriteColor("Неверный выбор! Пропускаете ход.", ConsoleColor.Red);
                        player.TakeDamage(enemyAttack);
                        ConsoleHelper.WriteColor($" Враг нанес {enemyAttack} урона!", ConsoleColor.Red);
                        break;
                }
            }

            if (enemyHealth <= 0)
            {
                int expGain = 15 + (player.Level * 3) + (currentDepth * 4);
                player.AddExperience(expGain);
                player.AddGold(enemyGold);

                currentRoom.Type = RoomType.Empty;

                if (currentRoom.Items.Any())
                {
                    foreach (var item in currentRoom.Items.ToList())
                    {
                        player.AddItem(item);
                        currentRoom.Items.Remove(item);
                        ConsoleHelper.WriteColor($" Вы нашли {item.Name}!", GetItemColor(item.Type));
                    }
                }

                ConsoleHelper.WriteColor(" Враг побежден!", ConsoleColor.Green);
            }
            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
        }

        private bool UseItemInCombat()
        {
            var potions = player.Inventory.Values
                .Where(item => item.Type == ItemType.Potion)
                .ToList();

            if (!potions.Any())
            {
                ConsoleHelper.WriteColor("У вас нет зелий для использования!", ConsoleColor.Red);
                return false;
            }

            ConsoleHelper.WriteColor("\nВыберите зелье для использования:", ConsoleColor.Cyan);
            for (int i = 0; i < potions.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {potions[i].Name} (+{potions[i].Value} HP)");
            }
            Console.WriteLine("0. Отмена");

            var input = Console.ReadLine();
            if (int.TryParse(input, out int choice) && choice > 0 && choice <= potions.Count)
            {
                var selectedPotion = potions[choice - 1];
                var potionKey = player.Inventory.FirstOrDefault(kvp => kvp.Value == selectedPotion).Key;

                if (!string.IsNullOrEmpty(potionKey))
                {
                    player.Heal(selectedPotion.Value);
                    player.Inventory.Remove(potionKey);
                    ConsoleHelper.WriteColor($"Вы использовали {selectedPotion.Name}!", ConsoleColor.Green);
                    return true;
                }
            }

            ConsoleHelper.WriteColor("Неверный выбор!", ConsoleColor.Red);
            return false;
        }

        private ConsoleColor GetItemColor(ItemType type)
        {
            return type switch
            {
                ItemType.Weapon => ConsoleColor.Yellow,
                ItemType.Potion => ConsoleColor.Green,
                ItemType.Treasure => ConsoleColor.Magenta,
                ItemType.Scroll => ConsoleColor.Blue,
                _ => ConsoleColor.White
            };
        }

        private bool ShowVictoryScreen()
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

            var choice = Console.ReadLine();
            return choice == "1";
        }

        private void ShowGameOverScreen()
        {
            Console.Clear();
            ConsoleHelper.WriteColor(@"
╔══════════════════════════════════════════╗
║              ИГРА ОКОНЧЕНА          ║
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
    }

    public class LevelGenerator
    {
        private Random random;
        private int width;
        private int height;

        public LevelGenerator(int width, int height, int seed = 0)
        {
            this.width = width;
            this.height = height;
            this.random = seed == 0 ? new Random() : new Random(seed);
        }

        public Dungeon GenerateDungeon(string name, int maxRooms, int depth = 1)
        {
            Dungeon dungeon;
            bool isConnected;
            int attempts = 0;

            do
            {
                dungeon = GenerateDungeonAttempt(name, maxRooms, depth);
                var startRoom = dungeon.Rooms.FirstOrDefault(r => r.Type == RoomType.Start);
                var bossRoom = dungeon.Rooms.FirstOrDefault(r => r.Type == RoomType.Boss);

                if (startRoom == null || bossRoom == null)
                {
                    isConnected = false;
                    continue;
                }

                isConnected = IsDungeonConnected(dungeon, startRoom, bossRoom);
                attempts++;

                if (attempts >= GameConfig.MaxDugeonGenerationAttempts - 1 && !isConnected)
                {
                    return CreateFallbackDungeon(name, maxRooms, depth);
                }

            } while (!isConnected && attempts < GameConfig.MaxDugeonGenerationAttempts);

            if (!isConnected)
            {
                ConsoleHelper.WriteColor($"Внимание: не удалось создать идеальное подземелье после {attempts} попыток", ConsoleColor.Yellow);
            }
            return dungeon;
        }

        private Dungeon CreateFallbackDungeon(string name, int maxRooms, int depth)
        {
            var dungeon = new Dungeon(name + " (Fallback)", width, height, depth);

            var startRoom = new Room(0, 0, RoomType.Start);
            dungeon.AddRoom(startRoom);

            var bossRoom = new Room(width - 1, height - 1, RoomType.Boss);
            dungeon.AddRoom(bossRoom);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((x == 0 && y == 0) || (x == width - 1 && y == height - 1)) continue;

                    var roomType = RoomType.Empty;
                    if (random.Next(100) < 30) roomType = RoomType.Enemy;
                    else if (random.Next(100) < 10) roomType = RoomType.Treasure;

                    var room = new Room(x, y, roomType);
                    dungeon.AddRoom(room);
                }
            }

            return dungeon;
        }

        private bool IsDungeonConnected(Dungeon dungeon, Room startRoom, Room exitRoom)
        {
            var visited = new HashSet<Room>();
            var queue = new Queue<Room>();

            queue.Enqueue(startRoom);
            visited.Add(startRoom);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == exitRoom) return true;

                var neighbors = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
                foreach (var (dx, dy) in neighbors)
                {
                    var neighbor = dungeon.GetRoom(current.X + dx, current.Y + dy);
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }

        public Dungeon GenerateDungeonAttempt(string name, int maxRooms, int depth = 1)
        {
            var dungeon = new Dungeon(name, width, height, depth);
            var visited = new bool[width, height];

            int x = width / 2;
            int y = height / 2;

            var rooms = new List<Room>();
            var startRoom = new Room(x, y, RoomType.Empty);
            dungeon.AddRoom(startRoom);
            rooms.Add(startRoom);
            visited[x, y] = true;

            int roomCreated = 1;
            int attemptsWithoutProgress = 0;
            const int maxAttemptsWithoutProgress = 100;

            while (roomCreated < maxRooms && attemptsWithoutProgress < maxAttemptsWithoutProgress)
            {
                int direction = random.Next(4);
                int newX = x, newY = y;

                switch (direction)
                {
                    case 0: newX++; break;
                    case 1: newX--; break;
                    case 2: newY++; break;
                    case 3: newY--; break;
                }

                if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                {
                    if (!visited[newX, newY])
                    {
                        var room = new Room(newX, newY, RoomType.Empty);
                        dungeon.AddRoom(room);
                        rooms.Add(room);
                        visited[newX, newY] = true;
                        roomCreated++;
                        x = newX;
                        y = newY;
                        attemptsWithoutProgress = 0;
                    }
                    else
                    {
                        attemptsWithoutProgress++;
                    }
                }
                else
                {
                    attemptsWithoutProgress++;
                }
            }

            startRoom.Type = RoomType.Start;

            var bossRoom = rooms.Where(r => r != startRoom).OrderByDescending(r => Math.Abs(r.X - startRoom.X) + Math.Abs(r.Y - startRoom.Y)).FirstOrDefault();
            if (bossRoom == null)
            {
                bossRoom = rooms.FirstOrDefault(r => r != startRoom);
            }

            if (bossRoom == null)
            {
                int bossX = (x + 1) % width;
                int bossY = (y + 1) % height;
                bossRoom = new Room(bossX, bossY, RoomType.Boss);
                dungeon.AddRoom(bossRoom);
                rooms.Add(bossRoom);
            }
            else
            {
                bossRoom.Type = RoomType.Boss;
            }

            var deadEnds = rooms.Where(r => r.Type == RoomType.Empty && CountNeighbors(dungeon, r) == 1).ToList();
            foreach (var room in deadEnds.Take(Math.Min(3, deadEnds.Count)))
            {
                room.Type = RoomType.Treasure;
            }

            foreach (var room in rooms.Where(r => r.Type == RoomType.Empty))
            {
                room.Type = GetRandomRoomType(depth, room, startRoom, bossRoom);
            }

            foreach (var room in rooms)
            {
                PopulateRoom(room, depth);
            }

            return dungeon;
        }

        private int CountNeighbors(Dungeon dungeon, Room room)
        {
            int count = 0;
            var neighbors = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };

            foreach (var (dx, dy) in neighbors)
            {
                var neighbor = dungeon.GetRoom(room.X + dx, room.Y + dy);
                if (neighbor != null) count++;
            }
            return count;
        }

        private RoomType GetRandomRoomType(int depth, Room room, Room startRoom, Room bossRoom)
        {
            int distanceToBoss = Math.Abs(room.X - bossRoom.X) + Math.Abs(room.Y - bossRoom.Y);
            int roll = random.Next(100);

            int enemyChance = GameConfig.BaseEmptyRoomChance + (depth * 5);
            int treasureChance = GameConfig.BaseTreasureRoomChance + (depth * 3);
            int merchantChance = GameConfig.BaseMerchantRoomChance;
            int restChance = GameConfig.BaseRestRoomChance;

            if (distanceToBoss <= 2)
            {
                if (roll < enemyChance) return RoomType.Enemy;
                if (roll < enemyChance + treasureChance) return RoomType.Treasure;
                if (roll < enemyChance + treasureChance + merchantChance) return RoomType.Merchant;
                if (roll < enemyChance + treasureChance + merchantChance + restChance) return RoomType.Rest;
                return RoomType.Empty;
            }
            else
            {
                int emptyChance = GameConfig.BaseEmptyRoomChance;
                if (roll < emptyChance) return RoomType.Empty;
                if (roll < emptyChance + enemyChance) return RoomType.Enemy;
                if (roll < emptyChance + enemyChance + treasureChance) return RoomType.Treasure;
                if (roll < emptyChance + enemyChance + treasureChance + merchantChance) return RoomType.Merchant;
                if (roll < emptyChance + enemyChance + treasureChance + merchantChance + restChance) return RoomType.Rest;
                return RoomType.Empty;
            }
        }

        private void PopulateRoom(Room room, int depth)
        {
            switch (room.Type)
            {
                case RoomType.Treasure:
                    string[] treasureNames = { "Золотой нагрудник", "Магический амулет", "Шлем воина" };
                    string treasureName = treasureNames[random.Next(treasureNames.Length)];

                    var treasure = new Item(treasureName, room.X, room.Y,
                        30 + (depth * 10), ItemType.Treasure, EquipmentType.Armor, ConsoleColor.Magenta);
                    room.AddItem(treasure);
                    break;

                case RoomType.Enemy:
                    if (random.Next(100) < 70)
                    {
                        var itemType = random.Next(4) switch
                        {
                            0 => ItemType.Weapon,
                            1 => ItemType.Potion,
                            2 => ItemType.Scroll,
                            _ => ItemType.Treasure
                        };

                        var item = itemType switch
                        {
                            ItemType.Weapon => new Item("Стальной меч", room.X, room.Y,
                                20 + (depth * 5), itemType, EquipmentType.Weapon, ConsoleColor.Yellow),
                            ItemType.Potion => new Item("Большое зелье", room.X, room.Y,
                                30 + (depth * 8), itemType, EquipmentType.Other, ConsoleColor.Green),
                            ItemType.Scroll => new Item("Свиток телепортации", room.X, room.Y,
                                15, itemType, EquipmentType.Other, ConsoleColor.Blue),
                            _ => new Item("Драгоценный камень", room.X, room.Y,
                                40 + (depth * 10), itemType, EquipmentType.Other, ConsoleColor.Magenta)
                        };
                        room.AddItem(item);
                    }
                    break;

                case RoomType.Boss:
                    var potion = new Item("Эликсир здоровья", room.X, room.Y,
                        60 + (depth * 15), ItemType.Potion, EquipmentType.Other, ConsoleColor.Green);
                    room.AddItem(potion);

                    var weapon = new Item("Легендарный меч", room.X, room.Y,
                        50 + (depth * 20), ItemType.Weapon, EquipmentType.Weapon, ConsoleColor.Yellow);
                    room.AddItem(weapon);
                    break;

                case RoomType.Start:
                    var startPotion = new Item("Малое зелье здоровья", room.X, room.Y,
                        25, ItemType.Potion, EquipmentType.Other, ConsoleColor.Green);
                    room.AddItem(startPotion);
                    break;
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Console.Title = "Текстовая RPG - Подземелье";
            Console.CursorVisible = true;

            int attempts = 0;
            const int maxAttempts = 3;
            bool gameCompletedSuccessfully = false;

            while (attempts < maxAttempts && !gameCompletedSuccessfully)
            {
                try
                {
                    var game = new Game();
                    game.Start();
                    gameCompletedSuccessfully = true;
                }
                catch (Exception ex)
                {
                    attempts++;
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Произошла ошибка в игре (попытка {attempts}/{maxAttempts})");
                    Console.WriteLine(ex.Message);

#if DEBUG
                    Console.WriteLine("\nStack Trace:");
                    Console.WriteLine(ex.StackTrace);
#endif

                    Console.ResetColor();

                    if (attempts < maxAttempts)
                    {
                        Console.WriteLine($"\nПерезапуск игры через 3 секунды...");
                        for (int i = 3; i > 0; i--)
                        {
                            Console.WriteLine($"{i}...");
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nИгра завершена из-за критических ошибок.");
                        Console.WriteLine("Нажмите любую клавишу для выхода...");
                        Console.ReadKey();
                    }
                }
            }

            if (gameCompletedSuccessfully)
            {
                Console.WriteLine("\nИгра завершена успешно! Спасибо за игру!");
            }
        }
    }
}