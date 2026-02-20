using TextRPG.Core.Models;
using TextRPG.Core.Enums;
using TextRPG.Config;
using TextRPG.Core.Utils;
using TextRPG.Core.Models.Inventory;
using TextRPG.Core.Models.Inventory.Equipment;

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

        public Inventory Inventory { get; private set; }
        public EquipmentService EquipmentService { get; private set; }

        private GameConfig _config;
        private Random random = new Random();

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

            Inventory = new Inventory(_config.MaxInventorySize);
            
            EquipmentService = new EquipmentService(this);
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

            Attack = BaseAttack + (Inventory.EquippedWeapon?.Value ?? 0);
            int oldMaxHealth = MaxHealth;
            MaxHealth = BaseMaxHealth + (Inventory.EquippedArmor?.Value ?? 0);

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
            if (!Inventory.AddItem(item))
            {
                ConsoleHelper.WriteColor("Инвентарь полон! Вы не можете поднять этот предмет.", ConsoleColor.Red);
                return;
            }
            ConsoleHelper.WriteColor($"Предмет добавлен: {item.Name}", ConsoleColor.Green);
        }

        public bool RemoveItem(string itemKey)
        {
            return Inventory.RemoveItem(itemKey);
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
}