using TextRPG.Core.Models;
using TextRPG.Core.Enums;
using TextRPG.Config;
using TextRPG.Core.Utils;

namespace TextRPG
{
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
        public int TurnsSinceLastRest { get; set; } = -1;
        public Dictionary<string, Item> Inventory { get; private set; }
        public Item? EquippedWeapon { get; private set; }
        public Item? EquippedArmor { get; private set; }
        private GameConfig _config;

        public Player(string name, int x, int y) : base(name, x, y)
        {
            _config = new GameConfig();
            BaseMaxHealth = 100;
            MaxHealth = BaseMaxHealth;
            Health = MaxHealth;
            BaseAttack = 10;
            Attack = BaseAttack;
            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 50;
            Gold = 0;
            Inventory = new Dictionary<string, Item>();
        }

        public void IncrementTurnCounter()
        {
            if (TurnsSinceLastRest >= 0)
            {
                TurnsSinceLastRest++;
            }
        }

        public void ActivateRestCooldown()
        {
            TurnsSinceLastRest = 0;
        }

        public static class InventoryHelper
        {
            public static void DisplayInventory(Player player)
            {
                Console.WriteLine("\n=== Инвентарь ===");
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
                        string equippedMark = inventoryList[i] == player.EquippedWeapon ||
                                            inventoryList[i] == player.EquippedArmor ? " [Экипировано]" : "";
                        Console.WriteLine($"{i + 1}. {inventoryList[i]}{equippedMark}");
                    }
                }
            }

            public static bool IsValidInventoryChoice(string input, int inventoryCount)
            {
                if (input == "0" || input == "u") return true;
                return int.TryParse(input, out int choice) && choice > 0 && choice <= inventoryCount;
            }
        }

        public void EquipItem(string itemKey)
        {

            if (!Inventory.ContainsKey(itemKey))
            {
                Console.WriteLine("Предмет не найден в инвентаре!");
                return;
            }

            var item = Inventory[itemKey];
            Item? oldEquipment = null;
            bool equipSuccess = false;

            switch (item.Type)
            {
                case ItemType.Weapon when item.EquipmentType == EquipmentType.Weapon:
                    oldEquipment = EquippedWeapon;
                    EquippedWeapon = item;
                    Attack = BaseAttack + item.Value;
                    Console.WriteLine($"Вы экипировали {item.Name} (+{item.Value} к атаке)");
                    equipSuccess = true;
                    break;
                case ItemType.Treasure when item.EquipmentType == EquipmentType.Armor:
                    oldEquipment = EquippedArmor;
                    EquippedArmor = item;
                    double healthPercent = (double)Health / MaxHealth;
                    MaxHealth = BaseMaxHealth + item.Value;
                    Health = (int)(MaxHealth * healthPercent);
                    if (Health <= 0) Health = 1;
                    Console.WriteLine($"Вы экипировали {item.Name} (+{item.Value} к максимальному HP)");
                    equipSuccess = true;
                    break;
                default:
                    Console.WriteLine("Этот предмет нельзя экипировать.");
                    return;
            }

            if (equipSuccess)
            {
                Inventory.Remove(itemKey);

                if (oldEquipment != null)
                {
                    AddItem(oldEquipment);
                    UnequipItem(oldEquipment, silent: true);
                }
            }
        }

        public void UnequipItem(Item item, bool silent = false)
        {
            switch (item.Type)
            {
                case ItemType.Weapon when item == EquippedWeapon:
                    EquippedWeapon = null;
                    Attack = BaseAttack;
                    if (!silent)
                    {
                        Console.WriteLine($"Вы сняли {item.Name}");
                    }
                    break;
                case ItemType.Treasure when item == EquippedArmor:
                    EquippedArmor = null;
                    double healthPercent = (double)Health / MaxHealth;
                    MaxHealth = BaseMaxHealth;
                    Health = (int)(MaxHealth * healthPercent);
                    Health = Math.Max(1, Math.Min(Health, MaxHealth));
                    if (!silent)
                    {
                        Console.WriteLine($"Вы сняли {item.Name}");
                    }
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
            if (Level >= _config.MaxPlayerLevel)
            {
                ConsoleHelper.WriteColor("Вы достигли максимального уровня!", ConsoleColor.Yellow);
                return;
            }
            Level++;
            Experience -= ExperienceToNextLevel;
            ExperienceToNextLevel = (int)(ExperienceToNextLevel * 2);

            BaseAttack += 3;
            BaseMaxHealth += 20;

            Attack = BaseAttack + (EquippedWeapon?.Value ?? 0);
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
            ConsoleHelper.WriteColor($"║          УРОВЕНЬ ПОВЫШЕН! {Level}           ║", ConsoleColor.Yellow);
            ConsoleHelper.WriteColor($"║  HP: +20  АТК: +3  Макс.Опыт: {ExperienceToNextLevel} ║", ConsoleColor.Yellow);
            Console.WriteLine($"╚══════════════════════════════════════╝", ConsoleColor.Yellow);
        }

        public void TakeDamage(int damage)
        {
            int actualDamage = damage;

            if (random.Next(100) < (Level * _config.DodgePerLevel))
            {
                actualDamage = damage / 2;
                ConsoleHelper.WriteColor(" Уклонение! Урон уменьшен вдвое.", ConsoleColor.Cyan);
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
            ConsoleHelper.WriteColor($" Восстановлено {actualHeal} HP. Теперь HP: {Health}/{MaxHealth}", ConsoleColor.Green);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            ConsoleHelper.WriteColor($" Найдено {amount} золота! Всего: {Gold}", ConsoleColor.Yellow);
        }

        public void AddItem(Item item)
        {
            if (Inventory.Count >= _config.MaxInventorySize)
            {
                ConsoleHelper.WriteColor("Инвентарь полон! Вы не можете поднять этот предмет.", ConsoleColor.Red);
                return;
            }


            string itemKey = GenerateItemKey(item);

            if (Inventory.ContainsKey(itemKey))
            {
                if (item.Type == ItemType.Potion)
                {
                    var existingItem = Inventory[itemKey];
                    Inventory[itemKey] = item;
                    ConsoleHelper.WriteColor($"Зелье обновлено: {item.Name}", ConsoleColor.Green);
                }
                else
                {
                    itemKey = GenerateUniqueItemKey(item);
                    Inventory[itemKey] = item;
                    ConsoleHelper.WriteColor($"Предмет добавлен: {item.Name}", ConsoleColor.Green);
                }
            }
            else
            {
                Inventory[itemKey] = item;
                ConsoleHelper.WriteColor($"Предмет добавлен: {item.Name}", ConsoleColor.Green);
            }
        }

        private string GenerateItemKey(Item item)
        {
            if (item.Type == ItemType.Potion)
            {
                return $"{item.Type}_{item.Name}";
            }

            return $"{item.Type}_{item.Name}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
        }

        private string GenerateUniqueItemKey(Item item)
        {
            return $"{item.Type}_{item.Name}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }

        public bool RemoveItem(string itemKey)
        {
            if (Inventory.ContainsKey(itemKey))
            {
                var item = Inventory[itemKey];
                if (item == EquippedWeapon || item == EquippedArmor)
                {
                    ConsoleHelper.WriteColor("Нельзя удалить экипированный предмет!", ConsoleColor.Red);
                    return false;
                }

                Inventory.Remove(itemKey);
                return true;
            }
            return false;
        }

        public Item? GetItem(string itemKey)
        {
            return Inventory.ContainsKey(itemKey) ? Inventory[itemKey] : null;
        }

        public bool HasItem(string itemName)
        {
            return Inventory.Values.Any(item => item.Name == itemName);
        }

        private List<Item> GetItemsByType(ItemType type)
        {
            return Inventory.Values.Where(item => item.Type == type).ToList();
        }

        public int GetItemCount(string itemName)
        {
            return Inventory.Values.Count(item => item.Name == itemName);
        }

        public string? GetItemKey(Item item)
        {
            return Inventory.FirstOrDefault(kvp => kvp.Value == item).Key;
        }

        private Random random = new Random();

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
}