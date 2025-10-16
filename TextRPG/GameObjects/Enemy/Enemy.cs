using TextRPG.Core.Interfaces;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Models
{
    public abstract class Enemy : GameObject, ICombatant, IEquatable<Enemy>
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Level { get; set; }
        public int GoldReward { get; set; }
        public int ExperienceReward { get; set; }
        public bool IsBoss { get; protected set; }
        public bool IsAlive => Health > 0;

        protected Enemy(string name, int x, int y, int level) : base(name, x, y)
        {
            Level = level;
        }

        public virtual void TakeDamage(int damage)
        {
            int actualDamage = Math.Max(0, damage);
            Health = Math.Max(0, Health - actualDamage);

            ConsoleHelper.WriteColor($"{Name} получает {actualDamage} урона! Осталось HP: {Health}/{MaxHealth}", GetDamageColor(actualDamage));
        }

        public virtual void Heal(int amount)
        {
            int actualHeal = Math.Min(MaxHealth - Health, amount);
            Health += actualHeal;

            if (actualHeal > 0)
            {
                ConsoleHelper.WriteColor($"{Name} восстанавливает {actualHeal} HP. Теперь HP: {Health}/{MaxHealth}", ConsoleColor.Green);
            }
        }

        private ConsoleColor GetDamageColor(int damage)
        {
            return damage switch
            {
                > 20 => ConsoleColor.DarkRed,
                > 10 => ConsoleColor.Red,
                > 5 => ConsoleColor.Yellow,
                _ => ConsoleColor.Gray
            };
        }

        public abstract int CalculateDamage();

        public override bool Equals(object? obj) => Equals(obj as Enemy);

        public bool Equals(Enemy? other) =>
            base.Equals(other) &&
            Health == other.Health &&
            MaxHealth == other.MaxHealth &&
            Attack == other.Attack &&
            Level == other.Level;

        public override int GetHashCode() =>
            HashCode.Combine(base.GetHashCode(), Health, MaxHealth, Attack, Level);

        public override string ToString() =>
            $"{Name} (Ур. {Level}) - HP: {Health}/{MaxHealth} ATK: {Attack}";
    }
}