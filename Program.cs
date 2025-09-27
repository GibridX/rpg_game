using System;
using System.Collections.Generic;
using System.Linq;

namespace TextRPG
{
    public abstract class GameObject : IEquatable<GameObject>
    {
        public string Name { get; protected set; }
        public int X { get; set; }
        public int Y { get; set; }

        protected GameObject(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as GameObject);
        }

        public bool Equals(GameObject? other)
        {
            return other != null && Name == other.Name
            && X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, X, Y);
        }

        public override string ToString()
        {
            return $"{Name} на ({X}, {Y})";
        }
    }

    public enum RoomType
    {
        Empty,
        Enemy,
        Treasure,
        Boss,
        Start,
        Exit
    }

    public enum ItemType
    {
        Weapon,
        Potion,
        Treasure,
        Scroll
    }

    public enum EquipmentType { Weapon, Armor, Other }

    public class Player : GameObject, IEquatable<Player>
    {
        public int Health { get; set; }
        public int BaseAttack { get; private set; }
        public int MaxHealth { get; set; }
        public int BaseMaxHealth { get; private set; }
        public int Attack { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }
        public int Gold { get; set; }
        public List<Item> Inventory { get; private set; }
        public Item? EquippedWeapon { get; private set; }
        public Item? EquippedArmor { get; private set; }

        public Player(string name, int x, int y) : base(name, x, y)
        {
            BaseMaxHealth = 100;
            MaxHealth = BaseMaxHealth;
            Health = MaxHealth;
            BaseAttack = 10;
            Attack = BaseAttack;
            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 50;
            Gold = 0;
            Inventory = new List<Item>();
        }

        public void EquipItem(Item item)
        {
            if (!Inventory.Contains(item))
            {
                Console.WriteLine("Предмет не найден в инвентаре!");
                return;
            }
            switch (item.Type)
            {
                case ItemType.Weapon:
                    if (EquippedWeapon != null)
                        UnequipItem(EquippedWeapon);
                    EquippedWeapon = item;
                    Attack = BaseAttack + (item.Value / 2);
                    Console.WriteLine($"Вы экипировали {item.Name} (+{item.Value / 2} к атаке)");
                    break;
                case ItemType.Treasure when item.EquipmentType == EquipmentType.Armor:
                    if (EquippedArmor != null)
                        UnequipItem(EquippedArmor);
                    EquippedArmor = item;
                    MaxHealth = BaseMaxHealth + item.Value;
                    Console.WriteLine($"Вы экипировали {item.Name} (+{item.Value} к максимальному HP)");
                    break;
            }
        }

        public void UnequipItem(Item item)
        {
            switch (item.Type)
            {
                case ItemType.Weapon when item == EquippedWeapon:
                    EquippedWeapon = null;
                    Attack = BaseAttack;
                    Console.WriteLine($"Вы сняли {item.Name}");
                    break;
                case ItemType.Treasure when item == EquippedArmor:
                    EquippedArmor = null;
                    MaxHealth = BaseMaxHealth;
                    Health = Math.Min(Health, MaxHealth);
                    Console.WriteLine($"Вы сняли {item.Name}");
                    break;
            }
        }

        public void AddExperience(int exp)
        {
            Experience += exp;
            Console.WriteLine($"Получено {exp} опыта! Всего: {Experience}/{ExperienceToNextLevel}");
            
            while (Experience >= ExperienceToNextLevel)
            {
                LevelUp();
            }
        }

        public void LevelUp()
        {
            Level++;
            Experience -= ExperienceToNextLevel;
            ExperienceToNextLevel = (int)(ExperienceToNextLevel * 1.5);
            
            BaseAttack += 3;
            BaseMaxHealth += 20;

            Attack = BaseAttack + (EquippedWeapon?.Value / 2 ?? 0);
            int oldMaxHealth = MaxHealth;
            MaxHealth = BaseMaxHealth + (EquippedArmor?.Value ?? 0);

            if (oldMaxHealth > 0)
            {
                int healthDifference = MaxHealth - oldMaxHealth;
                Health += healthDifference;
                if (Health > MaxHealth) Health = MaxHealth;
                if (Health < 0) Health = 0;
            }
            
            Console.WriteLine($"╔══════════════════════════════════════╗", ConsoleColor.Yellow);
            WriteColor($"║          УРОВЕНЬ ПОВЫШЕН! {Level}           ║", ConsoleColor.Yellow);
            WriteColor($"║  HP: +20  АТК: +3  Макс.Опыт: {ExperienceToNextLevel} ║", ConsoleColor.Yellow);
            Console.WriteLine($"╚══════════════════════════════════════╝", ConsoleColor.Yellow);
        }

        public void TakeDamage(int damage)
        {
            int actualDamage = damage;
            // Шанс уклонения/блока
            if (random.Next(100) < (Level * 2))
            {
                actualDamage = damage / 2;
                WriteColor(" Уклонение! Урон уменьшен вдвое.", ConsoleColor.Cyan);
            }
            
            Health = Math.Max(0, Health - actualDamage);
        }

        public void Heal(int amount)
        {
            int newHealth = Health + amount;
            int actualHeal = amount;
            
            if (newHealth > MaxHealth)
            {
                actualHeal = MaxHealth - Health;
                Health = MaxHealth;
            }
            else
            {
                Health = newHealth;
            }
            WriteColor($" Восстановлено {actualHeal} HP. Теперь HP: {Health}/{MaxHealth}", ConsoleColor.Green);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            WriteColor($" Найдено {amount} золота! Всего: {Gold}", ConsoleColor.Yellow);
        }

        public void AddItem(Item item)
        {
            Inventory.Add(item);
        }

        private Random random = new Random();

        private void WriteColor(string text, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Player);
        }

        public bool Equals(Player? other)
        {
            return base.Equals(other) &&
                Health == other.Health &&
                MaxHealth == other.MaxHealth &&
                Attack == other.Attack &&
                Level == other.Level;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Health, MaxHealth, Attack, Level);
        }

        public override string ToString()
        {
            return $"{Name} (Ур. {Level}) - HP: {Health}/{MaxHealth} АТК: {Attack} Золото: {Gold}";
        }
    }

    public class Room : GameObject, IEquatable<Room>
    {
        public RoomType Type { get; set; }
        public bool IsExplored { get; set; }
        public List<Item> Items { get; private set; }
        public string Description { get; set; }

        public Room(int x, int y, RoomType type) : base($"Комната_{x}_{y}", x, y)
        {
            Type = type;
            IsExplored = false;
            Items = new List<Item>();
            Description = GetDefaultDescription(type);
        }

        private string GetDefaultDescription(RoomType type)
        {
            return type switch
            {
                RoomType.Start => "Вы стоите у входа в древнее подземелье. Стены покрыты мхом, а воздух пахнет пылью и тайнами.",
                RoomType.Exit => "Перед вами сияющий портал выхода! Свет исходящий от него обещает свободу и спасение.",
                RoomType.Enemy => "Комната заполнена костями предыдущих авантюриеров. В воздухе витает опасность...",
                RoomType.Treasure => "Блеск золота и драгоценностей слепит глаза. Сокровищница полна богатств!",
                RoomType.Boss => "Огромное логово с костями гигантских существ. Здесь обитает нечто ужасное...",
                RoomType.Empty => "Пустая каменная комната. Тишина нарушается лишь эхом ваших шагов.",
                _ => "Неизвестное место."
            };
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Room);
        }

        public bool Equals(Room? other)
        {
            return base.Equals(other) &&
            Type == other.Type &&
            IsExplored == other.IsExplored;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Type, IsExplored);
        }

        public override string ToString()
        {
            return $"{Type} комната на ({X}, {Y}) - Исследована: {IsExplored}";
        }
    }

    public class Item : GameObject, IEquatable<Item>
    {
        public int Value { get; private set; }
        public ItemType Type { get; private set; }
        public EquipmentType EquipmentType { get; set; }
        public ConsoleColor Color { get; set; }

        public Item(string name, int x, int y, int value, ItemType type, 
                   EquipmentType equipType = EquipmentType.Other, 
                   ConsoleColor color = ConsoleColor.White) : base(name, x, y)
        {
            Value = value;
            Type = type;
            EquipmentType = equipType;
            Color = color;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Item);
        }

        public bool Equals(Item? other)
        {
            return base.Equals(other) && Value == other.Value
            && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Value, Type);
        }

        public override string ToString()
        {
            return $"{Name} ({Type}) - Ценность: {Value}";
        }

        public void DisplayWithColor()
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = Color;
            Console.Write($"{Name} ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"({Type}) - Ценность: {Value}");
            Console.ForegroundColor = originalColor;
        }
    }

    public class Dungeon : GameObject, IEquatable<Dungeon>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<Room> Rooms { get; private set; }
        public int Depth { get; set; }

        public Dungeon(string name, int width, int height, int depth = 1) : base(name, 0, 0)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Rooms = new List<Room>();
        }

        public void AddRoom(Room room)
        {
            Rooms.Add(room);
        }

        public Room? GetRoom(int x, int y)
        {
            return Rooms.FirstOrDefault(r => r.X == x && r.Y == y);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Dungeon);
        }

        public bool Equals(Dungeon? other)
        {
            return base.Equals(other) && Width == other.Width
            && Height == other.Height && Rooms.SequenceEqual(other.Rooms);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Width);
            hash.Add(Height);
            foreach (var room in Rooms.OrderBy(r => r.X).ThenBy(r => r.Y))
            {
                hash.Add(room);
            }
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"{Name} - Размер: {Width}x{Height}, Комнат: {Rooms.Count}, Глубина: {Depth}";
        }

        public void DisplayMiniMap(Player player)
        {
            Console.WriteLine($"\n=== Миникарта {Name} (Глубина: {Depth}) ===");

            int startX = Math.Max(0, player.X - 3);
            int endX = Math.Min(Width - 1, player.X + 3);
            int startY = Math.Max(0, player.Y - 2);
            int endY = Math.Min(Height - 1, player.Y + 2);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    var room = GetRoom(x, y);
                    if (room != null)
                    {
                        if (player.X == x && player.Y == y)
                        {
                            WriteColorInline("P ", ConsoleColor.Green); // Игрок
                        }
                        else if (room.IsExplored)
                        {
                            char symbol = GetRoomSymbol(room);
                            ConsoleColor color = GetRoomColor(room);
                            WriteColorInline(symbol + " ", color);
                        }
                        else
                        {
                            WriteColorInline("? ", ConsoleColor.DarkGray);
                        }
                    }
                    else
                    {
                        WriteColorInline("# ", ConsoleColor.DarkRed); // Стена
                    }
                }
                Console.WriteLine();
            }

           Console.WriteLine("\nЛегенда:");
            WriteColorInline("P - Вы ", ConsoleColor.Green);
            WriteColorInline("S - Старт ", ConsoleColor.Blue);
            WriteColorInline("E - Выход ", ConsoleColor.Yellow);
            WriteColorInline("M - Враг ", ConsoleColor.Red);
            WriteColorInline("T - Сокровище ", ConsoleColor.Magenta);
            WriteColorInline("B - Босс ", ConsoleColor.DarkRed);
            WriteColorInline("? - Неизвестно", ConsoleColor.DarkGray);
            WriteColorInline("# - Стена", ConsoleColor.DarkRed);
            Console.WriteLine();
        }

        public char GetRoomSymbol(Room room)
        {
            return room.Type switch
            {
                RoomType.Start => 'S',
                RoomType.Exit => 'E',
                RoomType.Enemy => 'M',
                RoomType.Treasure => 'T',
                RoomType.Boss => 'B',
                RoomType.Empty => ' ',
                _ => '?'
            };
        }

        public ConsoleColor GetRoomColor(Room room)
        {
            return room.Type switch
            {
                RoomType.Start => ConsoleColor.Blue,
                RoomType.Exit => ConsoleColor.Yellow,
                RoomType.Enemy => ConsoleColor.Red,
                RoomType.Treasure => ConsoleColor.Magenta,
                RoomType.Boss => ConsoleColor.DarkRed,
                RoomType.Empty => ConsoleColor.Gray,
                _ => ConsoleColor.White
            };
        }

        private void WriteColorInline(string text, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
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

        public Game()
        {
            generator = new LevelGenerator(12, 12);
            random = new Random();
            InitializeGame();
        }

        private void InitializeGame()
        {
            player = new Player("Герой", 0, 0);
            currentDungeon = generator.GenerateDungeon("Древний склеп", 20, currentDepth);

            var startRoom = currentDungeon.Rooms.First(r => r.Type == RoomType.Start);
            player.X = startRoom.X;
            player.Y = startRoom.Y;
            startRoom.IsExplored = true;
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
            WriteColor(@"
╔══════════════════════════════════════════════════════════════╗
║                   ТЕКСТОВАЯ RPG - ПОДЗЕМЕЛЬЕ                ║
║                 Древний Склеп - Глубина: " + $"{currentDepth,2}" + @"                 ║
╚══════════════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
        }

        private void MarkForClear()
        {
            needsClear = true;
        }

        private void WriteColor(string text, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }

        private void WriteColorInline(string text, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }

        private void DisplayCompactStatusBar()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════╗");
            
            // HP бар
            double healthPercent = (double)player.Health / player.MaxHealth;
            int barWidth = 20;
            int filledWidth = (int)(barWidth * healthPercent);
            string healthBar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);
            
            WriteColorInline($"║ HP: ", ConsoleColor.Red);
            WriteColorInline($"{player.Health,3}/{player.MaxHealth,3} ", ConsoleColor.White);
            WriteColorInline($"[{healthBar}] ", ConsoleColor.Red);
            
            WriteColorInline($"АТК: {player.Attack,2} ", ConsoleColor.Yellow);
            WriteColorInline($"УР: {player.Level,2} ", ConsoleColor.Cyan);
            WriteColorInline($"💰: {player.Gold,3} ", ConsoleColor.Yellow);
            Console.WriteLine("║");
            
            // Опыт бар
            double expPercent = (double)player.Experience / player.ExperienceToNextLevel;
            filledWidth = (int)(barWidth * expPercent);
            string expBar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);
            
            WriteColorInline($"║ Опыт: {player.Experience,3}/{player.ExperienceToNextLevel,3} ", ConsoleColor.Blue);
            WriteColorInline($"[{expBar}]", ConsoleColor.Blue);
            Console.WriteLine("                      ║");
            
            Console.WriteLine("╚══════════════════════════════════════════════════╝");

            // Текущее местоположение
            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom != null)
            {
                Console.WriteLine();
                WriteColorInline(" 📍 ", ConsoleColor.Green);
                WriteColorInline($"{GetRoomDescription(currentRoom)} ", GetRoomTitleColor(currentRoom.Type));
                WriteColorInline($"({player.X}, {player.Y})", ConsoleColor.Gray);
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
                RoomType.Exit => ConsoleColor.Yellow,
                RoomType.Enemy => ConsoleColor.Red,
                RoomType.Treasure => ConsoleColor.Magenta,
                RoomType.Boss => ConsoleColor.DarkRed,
                RoomType.Empty => ConsoleColor.Gray,
                _ => ConsoleColor.White
            };
        }

        public void Start()
        {
            DisplayGameHeader();
            WriteColor("Нажмите любую клавишу чтобы начать...", ConsoleColor.Green);
            Console.ReadKey(true);

            while (player.Health > 0)
            {
                ClearIfNeeded();
                DisplayCompactStatusBar();
                currentDungeon.DisplayMiniMap(player);
                ShowMainMenu();

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        MovePlayer();
                        CheckRoomEvents(afterMove: true);
                        break;
                    case "2":
                        ShowInventory();
                        CheckRoomEvents(afterMove: false);
                        break;
                    case "3":
                        ShowCharacterInfo();
                        break;
                    case "4":
                        return;
                    default:
                        WriteColor("Неверный выбор! Нажмите любую клавишу...", ConsoleColor.Red);
                        Console.ReadKey(true);
                        MarkForClear();
                        continue;
                }

                if (currentDungeon.GetRoom(player.X, player.Y)?.Type == RoomType.Exit)
                {
                    if (ShowVictoryScreen())
                    {
                        // Переход на следующий уровень
                        currentDepth++;
                        InitializeGame();
                        MarkForClear();
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }
                MarkForClear();
            }

            ShowGameOverScreen();
        }

        private void ShowMainMenu()
        {
            WriteColor("\n🎮 Доступные действия:", ConsoleColor.Cyan);
            WriteColorInline("1. ", ConsoleColor.Yellow);
            WriteColorInline("👣 Перемещение", ConsoleColor.White);
            WriteColorInline("   2. ", ConsoleColor.Yellow);
            WriteColorInline("🎒 Инвентарь", ConsoleColor.White);
            Console.WriteLine();
            WriteColorInline("3. ", ConsoleColor.Yellow);
            WriteColorInline("📊 Информация о персонаже", ConsoleColor.White);
            WriteColorInline("   4. ", ConsoleColor.Yellow);
            WriteColorInline("🚪 Выйти из игры", ConsoleColor.White);
            Console.WriteLine();
            WriteColorInline("\nВыберите действие: ", ConsoleColor.Green);
        }

        private void CheckRoomEvents(bool afterMove = true)
        {
            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom == null) return;

            if (currentRoom.Type == RoomType.Empty && afterMove && random.Next(100) < 20)
            {
                RandomEvent();
                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
                return;
            }

            if (!afterMove) return;
            
            switch (currentRoom.Type)
            {
                case RoomType.Enemy:
                    Combat();
                    break;
                case RoomType.Treasure:
                    LootRoom(currentRoom);
                    break;
                case RoomType.Boss:
                    BossFight();
                    break;
            }
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
                for (int i = 0; i < player.Inventory.Count; i++)
                {
                    string equippedMark = "";
                    if (player.Inventory[i] == player.EquippedWeapon || player.Inventory[i] == player.EquippedArmor)
                    {
                        equippedMark = " [Экипировано]";
                    }
                    Console.WriteLine($"{i + 1}. {player.Inventory[i]}{equippedMark}");
                }

                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1-9 - Использовать/Экипировать предмет");
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
                    if (choice > 0 && choice <= player.Inventory.Count)
                    {
                        var selectedItem = player.Inventory[choice - 1];

                        switch (selectedItem.Type)
                        {
                            case ItemType.Potion:
                                int healValue = selectedItem.Value;
                                player.Inventory.Remove(selectedItem);
                                player.Heal(healValue);
                                Console.WriteLine($"Вы использовали {selectedItem.Name}!");
                                break;
                                
                            case ItemType.Weapon:
                            case ItemType.Treasure:
                                if (selectedItem.EquipmentType == EquipmentType.Armor)
                                    player.EquipItem(selectedItem);
                                else
                                    Console.WriteLine("Этот предмет нельзя экипировать.");
                                break;
                                
                            default:
                                Console.WriteLine("Этот предмет нельзя использовать.");
                                break;
                        }
                    }
                }
            }
            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
            MarkForClear();
        }

        private void RandomEvent()
        {
            int eventType = random.Next(3);
            switch (eventType)
            {
                case 0:
                    int healAmount = random.Next(10, 25);
                    player.Heal(healAmount);
                    Console.WriteLine($"Вы нашли источник здоровья! +{healAmount} HP");
                    break;
                case 1:
                    int damage = random.Next(5, 15);
                    player.TakeDamage(damage);
                    Console.WriteLine($"Вы попали на ловушку! -{damage} HP");
                    break;
                case 2:
                    var item = new Item("Случайный артефакт", player.X, player.Y, random.Next(20, 50), ItemType.Treasure);
                    player.AddItem(item);
                    Console.WriteLine($"Вы нашли {item.Name}!");
                    break;
            }
        }

        private void LootRoom(Room room)
        {
            if (room.Items.Any())
            {
                foreach (var item in room.Items.ToList())
                {
                    player.AddItem(item);
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
            }
            else
            {
                Console.WriteLine("Сокровищница пуста.");
                room.Type = RoomType.Empty;
            }
            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
        }

        private void BossFight()
        {
            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom == null) return;

            int bossHealth = 80 + (player.Level * 10);
            int bossAttack = 10 + (player.Level * 3);

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
                        var potion = player.Inventory.FirstOrDefault(i => i.Type == ItemType.Potion);
                        if (potion != null)
                        {
                            player.Heal(potion.Value);
                            player.Inventory.Remove(potion);
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

                currentRoom.Type = RoomType.Empty;

                if (currentRoom.Items.Any())
                {
                    foreach (var item in currentRoom.Items.ToList())
                    {
                        player.AddItem(item);
                        currentRoom.Items.Remove(item);
                        Console.WriteLine($"Вы нашли {item.Name}!");
                    }
                }
            }
            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
        }

        private void MovePlayer()
        {
            WriteColor("\n🗺️ Куда двигаемся?", ConsoleColor.Cyan);
            WriteColor("W - вверх, S - вниз, A - влево, D - вправо, 0 - отмена", ConsoleColor.White);

            var direction = Console.ReadLine()?.ToLower();

            int newX = player.X, newY = player.Y;

            switch (direction)
            {
                case "w": newY--; break;
                case "s": newY++; break;
                case "a": newX--; break;
                case "d": newX++; break;
                case "0":
                    WriteColor("Перемещение отменено.", ConsoleColor.Yellow);
                    Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                    Console.ReadKey(true);
                    return;
                default:
                    WriteColor("Неверное направление!", ConsoleColor.Red);
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey(true);
                    return;
            }

            var newRoom = currentDungeon.GetRoom(newX, newY);
            if (newRoom != null)
            {
                player.X = newX;
                player.Y = newY;

                if (!newRoom.IsExplored)
                {
                    newRoom.IsExplored = true;
                    WriteColor("🔍 Вы обнаружили новую комнату!", ConsoleColor.Green);
                    Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                    Console.ReadKey(true);
                }
                else
                {
                    WriteColor($"👣 Вы переместились в комнату ({newX}, {newY})", ConsoleColor.Gray);
                    Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                    Console.ReadKey(true);
                }
            }
            else
            {
                WriteColor("🚫 Туда нельзя двигаться! Это стена.", ConsoleColor.Red);
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey(true);
            }
        }

        private void ShowCharacterInfo()
        {
            Console.Clear();
            WriteColor("╔══════════════════════════════════════════╗", ConsoleColor.Cyan);
            WriteColor("║           ИНФОРМАЦИЯ О ПЕРСОНАЖЕ        ║", ConsoleColor.Cyan);
            WriteColor("╚══════════════════════════════════════════╝", ConsoleColor.Cyan);
            
            WriteColorInline($"🎯 Имя: ", ConsoleColor.Yellow);
            WriteColorInline($"{player.Name}\n", ConsoleColor.White);
            
            WriteColorInline($"⭐ Уровень: ", ConsoleColor.Yellow);
            WriteColorInline($"{player.Level}\n", ConsoleColor.White);
            
            WriteColorInline($"❤️ Здоровье: ", ConsoleColor.Yellow);
            WriteColorInline($"{player.Health}/{player.MaxHealth}\n", ConsoleColor.Red);
            
            WriteColorInline($"⚔️ Атака: ", ConsoleColor.Yellow);
            WriteColorInline($"{player.Attack}\n", ConsoleColor.White);
            
            WriteColorInline($"💰 Золото: ", ConsoleColor.Yellow);
            WriteColorInline($"{player.Gold}\n", ConsoleColor.Yellow);
            
            WriteColorInline($"📊 Опыт: ", ConsoleColor.Yellow);
            WriteColorInline($"{player.Experience}/{player.ExperienceToNextLevel}\n", ConsoleColor.Blue);
            
            WriteColorInline($"🏰 Глубина подземелья: ", ConsoleColor.Yellow);
            WriteColorInline($"{currentDepth}\n", ConsoleColor.White);
            
            WriteColorInline($"🎒 Размер инвентаря: ", ConsoleColor.Yellow);
            WriteColorInline($"{player.Inventory.Count}\n", ConsoleColor.White);

            WriteColor("\nЭкипировка:", ConsoleColor.Cyan);
            WriteColorInline($"🗡️ Оружие: ", ConsoleColor.Yellow);
            WriteColorInline($"{(player.EquippedWeapon?.Name ?? "Нет")}\n", ConsoleColor.White);
            
            WriteColorInline($"🛡️ Броня: ", ConsoleColor.Yellow);
            WriteColorInline($"{(player.EquippedArmor?.Name ?? "Нет")}\n", ConsoleColor.White);

            WriteColor("\nНажмите любую клавишу чтобы продолжить...", ConsoleColor.Green);
            Console.ReadKey(true);
            MarkForClear();
        }

        private void Combat()
        {
            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom == null) return;

            int enemyHealth = 30 + (player.Level * 8) + (currentDepth * 5);
            int enemyAttack = 8 + (player.Level * 3) + (currentDepth * 2);
            int enemyGold = 10 + (player.Level * 5) + (currentDepth * 3);

            WriteColor($"\n⚔️ Вы столкнулись с врагом (Ур. {player.Level + currentDepth})!", ConsoleColor.Red);
            WriteColor($"👹 Здоровье врага: {enemyHealth}, Атака: {enemyAttack}", ConsoleColor.DarkRed);

            while (enemyHealth > 0 && player.Health > 0)
            {
                WriteColor($"\n❤️ Ваше HP: {player.Health}", ConsoleColor.Green);
                WriteColor($"👹 Враг HP: {enemyHealth}", ConsoleColor.Red);
                WriteColor("1. ⚔️ Атаковать\n2. 🏃 Бежать (50% шанс)", ConsoleColor.White);

                var choice = Console.ReadLine();

                if (choice == "1")
                {
                    int damage = player.Attack + random.Next(-3, 4); // Небольшая вариативность урона
                    enemyHealth -= damage;
                    WriteColor($"💥 Вы нанесли {damage} урона!", ConsoleColor.Yellow);

                    if (enemyHealth > 0)
                    {
                        player.TakeDamage(enemyAttack);
                        WriteColor($"💢 Враг нанес {enemyAttack} урона!", ConsoleColor.Red);
                    }
                }
                else if (choice == "2")
                {
                    if (random.Next(100) < 50)
                    {
                        WriteColor("🏃 Вы успешно сбежали!", ConsoleColor.Green);
                        return;
                    }
                    else
                    {
                        WriteColor("💥 Побег не удался! Враг атакует!", ConsoleColor.Red);
                        player.TakeDamage(enemyAttack);
                        WriteColor($"💢 Враг нанес {enemyAttack} урона!", ConsoleColor.Red);
                    }
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
                        WriteColor($"🎁 Вы нашли {item.Name}!", GetItemColor(item.Type));
                    }
                }

                WriteColor("🎉 Враг побежден!", ConsoleColor.Green);
            }
            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
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
            WriteColor(@"
╔══════════════════════════════════════════╗
║               🎉 ПОБЕДА! 🎉             ║
║                                          ║
║   Вы нашли выход из подземелья!         ║
║   Ваши достижения:                       ║
║                                          ║", ConsoleColor.Yellow);
            
            WriteColorInline($"   Уровень: {player.Level} ", ConsoleColor.Cyan);
            WriteColorInline($"Золото: {player.Gold} ", ConsoleColor.Yellow);
            WriteColorInline($"Глубина: {currentDepth}", ConsoleColor.Green);
            Console.WriteLine();
            
            WriteColor(@"
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
            WriteColor(@"
╔══════════════════════════════════════════╗
║             💀 ИГРА ОКОНЧЕНА 💀         ║
║                                          ║
║        Вы потерпели поражение           ║
║                                          ║
║         Ваши достижения:                 ║", ConsoleColor.Red);
            
            WriteColorInline($"   Уровень: {player.Level} ", ConsoleColor.Cyan);
            WriteColorInline($"Золото: {player.Gold} ", ConsoleColor.Yellow);
            WriteColorInline($"Глубина: {currentDepth}", ConsoleColor.Green);
            Console.WriteLine();
            
            WriteColor(@"
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
            var dungeon = new Dungeon(name, width, height, depth);
            var visited = new bool[width, height];

            int x = width / 2;
            int y = height / 2;

            var rooms = new List<Room>();
            var startRoom = new Room(x, y, RoomType.Empty);
            dungeon.AddRoom(startRoom);
            visited[x, y] = true;

            int roomCreated = 1;

            while (roomCreated < maxRooms)
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
                    }
                }
            }

            startRoom.Type = RoomType.Start;

            var bossRoom = rooms.OrderByDescending(r => Math.Abs(r.X - startRoom.X) + Math.Abs(r.Y - startRoom.Y)).First();
            bossRoom.Type = RoomType.Boss;

            var exitCandidates = rooms.Where(r => Math.Abs(r.X - bossRoom.X) + Math.Abs(r.Y - bossRoom.Y) == 1 && r.Type == RoomType.Empty);
            var exitRoom = exitCandidates.FirstOrDefault() ?? bossRoom;
            exitRoom.Type = RoomType.Exit;

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
            if (dungeon.GetRoom(room.X + 1, room.Y) != null) count++;
            if (dungeon.GetRoom(room.X - 1, room.Y) != null) count++;
            if (dungeon.GetRoom(room.X, room.Y + 1) != null) count++;
            if (dungeon.GetRoom(room.X, room.Y - 1) != null) count++;
            return count;
        }

        private RoomType GetRandomRoomType(int depth, Room room, Room startRoom, Room bossRoom)
        {
            int distanceToBoss = Math.Abs(room.X - bossRoom.X) + Math.Abs(room.Y - bossRoom.Y);
            int roll = random.Next(100);

            // С увеличением глубины больше врагов и сокровищ
            int enemyChance = 40 + (depth * 5);
            int treasureChance = 20 + (depth * 3);

            if (distanceToBoss <= 2) 
            {
                if (roll < enemyChance) return RoomType.Enemy;
                if (roll < enemyChance + treasureChance) return RoomType.Treasure;
                return RoomType.Empty;
            }
            else 
            {
                if (roll < 30) return RoomType.Empty;
                if (roll < 30 + enemyChance) return RoomType.Enemy;
                if (roll < 30 + enemyChance + treasureChance) return RoomType.Treasure;
                return RoomType.Empty;
            }
        }

        private void PopulateRoom(Room room, int depth)
        {
            switch (room.Type)
            {
                case RoomType.Treasure:
                    var treasure = new Item("Золотой нагрудник", room.X, room.Y, 
                        30 + (depth * 10), ItemType.Treasure, EquipmentType.Armor, ConsoleColor.Magenta);
                    room.AddItem(treasure);
                    
                    // Добавляем золото в сокровищницу
                    if (random.Next(100) < 80)
                    {
                        var gold = new Item("Мешок золота", room.X, room.Y, 
                            50 + (depth * 20), ItemType.Treasure, EquipmentType.Other, ConsoleColor.Yellow);
                        room.AddItem(gold);
                    }
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
            
            try
            {
                var game = new Game();
                game.Start();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Произошла ошибка в игре:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }
    }
}