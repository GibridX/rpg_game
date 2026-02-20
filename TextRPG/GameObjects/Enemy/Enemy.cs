using TextRPG.Core.Interfaces;
using TextRPG.Core.Utils;
using System.Text;

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
        public int DamageMin { get; protected set; }
        public int DamageMax { get; protected set; }

        protected Enemy(string name, int x, int y, int level) : base(name, x, y)
        {
            Level = level;
        }

        public virtual void TakeDamage(int damage)
        {
            int actualDamage = Math.Max(0, damage);
            int previousHealth = Health;
            Health = Math.Max(0, Health - actualDamage);

            DisplayDamageMessage(actualDamage, previousHealth);
        }

        private void DisplayDamageMessage(int damage, int previousHealth)
        {
            var damageColor = GetDamageColor(damage);
            double healthPercent = (double)Health / MaxHealth;

            var message = new StringBuilder();
            message.Append($"{Name} получает {damage} урона! ");

            // Добавляем визуальный индикатор здоровья
            message.Append($"[{GetHealthBar(healthPercent)}] ");
            message.Append($"HP: {Health}/{MaxHealth}");

            ConsoleHelper.WriteColor(message.ToString(), damageColor);

            // Дополнительное сообщение при низком здоровье
            if (healthPercent <= 0.25 && Health > 0)
            {
                ConsoleHelper.WriteColor($"{Name} выглядит сильно раненым!", ConsoleColor.DarkRed);
            }
        }

        private string GetHealthBar(double percent)
        {
            const int barWidth = 10;
            int filledWidth = (int)(barWidth * percent);
            return new string('█', filledWidth) + new string('░', barWidth - filledWidth);
        }

        private ConsoleColor GetDamageColor(int damage)
        {
            double damagePercent = (double)damage / MaxHealth;

            return damagePercent switch
            {
                > 0.3 => ConsoleColor.DarkRed,    // Очень сильный урон
                > 0.15 => ConsoleColor.Red,       // Сильный урон
                > 0.05 => ConsoleColor.Yellow,    // Средний урон
                _ => ConsoleColor.Gray            // Слабый урон
            };
        }

        public virtual void Heal(int amount)
        {
            int actualHeal = Math.Min(MaxHealth - Health, amount);
            int previousHealth = Health;
            Health += actualHeal;

            if (actualHeal > 0)
            {
                var message = $"{Name} восстанавливает {actualHeal} HP. ";
                message += $"[{GetHealthBar((double)Health / MaxHealth)}] ";
                message += $"Теперь HP: {Health}/{MaxHealth}";

                ConsoleHelper.WriteColor(message, ConsoleColor.Green);
            }
        }

        public abstract int CalculateDamage();

        // Новая оптимизированная версия расчета урона
        public virtual int CalculateDamageWithVariance(int variancePercent = 20)
        {
            int baseDamage = CalculateDamage();
            int variance = (int)(baseDamage * variancePercent / 100.0);
            return Math.Max(1, baseDamage + Random.Shared.Next(-variance, variance + 1));
        }

        // Метод для получения информации о враге в бою
        public virtual string GetCombatInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Name} (Уровень {Level})");
            sb.AppendLine($"Здоровье: {Health}/{MaxHealth}");
            sb.AppendLine($"Атака: {Attack} ({DamageMin}-{DamageMax} урона)");
            sb.AppendLine($"Награда: {GoldReward} золота, {ExperienceReward} опыта");
            return sb.ToString();
        }

        // Метод для краткого отображения в интерфейсе
        public virtual string GetShortInfo()
        {
            double healthPercent = (double)Health / MaxHealth;
            var healthStatus = healthPercent switch
            {
                > 0.75 => "Здоров",
                > 0.5 => "Ранен",
                > 0.25 => "Сильно ранен",
                _ => "При смерти"
            };

            return $"{Name} Ур.{Level} [{healthStatus}]";
        }

        // Метод для отображения в списке противников
        public virtual string GetBattleDisplay()
        {
            const int barWidth = 15;
            double percent = (double)Health / MaxHealth;
            int filledWidth = (int)(barWidth * percent);
            string healthBar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);

            var healthColor = percent switch
            {
                > 0.6 => ConsoleColor.Green,
                > 0.3 => ConsoleColor.Yellow,
                _ => ConsoleColor.Red
            };

            return $"{Name,-20} [{healthBar}] {Health,3}/{MaxHealth,3}";
        }

        public override bool Equals(object? obj) => Equals(obj as Enemy);

        public bool Equals(Enemy? other) =>
            other != null &&
            base.Equals(other) &&
            Health == other.Health &&
            MaxHealth == other.MaxHealth &&
            Attack == other.Attack &&
            Level == other.Level &&
            GoldReward == other.GoldReward &&
            ExperienceReward == other.ExperienceReward;

        public override int GetHashCode() =>
            HashCode.Combine(base.GetHashCode(), Health, MaxHealth, Attack, Level, GoldReward, ExperienceReward);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"╔═══════════════════════════════════╗");
            sb.AppendLine($"║             {Name,-15}           ║");
            sb.AppendLine($"╠═══════════════════════════════════╣");
            sb.AppendLine($"║ Уровень: {Level,-24} ║");
            sb.AppendLine($"║ Здоровье: {Health,-3}/{MaxHealth,-3} {"[" + GetHealthBar((double)Health / MaxHealth) + "]",-15} ║");
            sb.AppendLine($"║ Атака: {Attack,-26} ║");
            sb.AppendLine($"║ Урон: {DamageMin}-{DamageMax,-21} ║");
            sb.AppendLine($"║ Награда: {GoldReward} зол. {ExperienceReward} оп. ║");
            sb.AppendLine($"╚═══════════════════════════════════╝");
            return sb.ToString();
        }

        // Новые методы для улучшенного взаимодействия
        public virtual bool CanUseSpecialAbility() => false;

        public virtual string UseSpecialAbility() => string.Empty;

        public virtual int GetSpecialAbilityDamage() => 0;

        // Метод для проверки возможности побега от этого врага
        public virtual int GetEscapeDifficulty() => Level * 10;

        // Метод для получения модификатора урона based на состоянии
        public virtual double GetDamageModifier() =>
            Health <= MaxHealth * 0.25 ? 1.2 : 1.0; // Усиление урона при низком здоровье
    }
}