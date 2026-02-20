using TextRPG.Config;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Models
{
    public class Boss : Enemy
    {
        private readonly Random _random;
        private readonly GameConfig _config;

        public List<string> SpecialAbilities { get; private set; }
        public int SpecialAbilityChance { get; private set; } = 25;

        public Boss(string name, int x, int y, int level, GameConfig config)
            : base(name, x, y, level)
        {
            _config = config;
            _random = new Random();
            SpecialAbilities = new List<string>();
            IsBoss = true;
            InitializeStats();
            InitializeAbilities();
        }

        private void InitializeStats()
        {
            MaxHealth = 80 + (Level * 8);
            Health = MaxHealth;
            Attack = 12 + (Level * 2);
            GoldReward = 100 + (Level * 25);
            ExperienceReward = 50 + (Level * 15);
        }

        private void InitializeAbilities()
        {
            SpecialAbilities.AddRange(
            [
                "Огненное дыхание",
                "Сокрушительный удар",
                "Землетрясение",
                "Магическией щит"
            ]);
        }

        public override int CalculateDamage()
        {
            int variance = _random.Next(-3, 6);

            if (_random.Next(100) < SpecialAbilityChance)
            {
                ConsoleHelper.WriteColor($"{Name} использует {UseSpecialAbility()}!", ConsoleColor.DarkMagenta);
                return (int)((Attack + variance) * 1.1);
            }

            return Math.Max(1, Attack + variance);
        }

        public override string UseSpecialAbility()
        {
            if (SpecialAbilities.Count == 0)
                return "Мощная атака";

            var ability = SpecialAbilities[_random.Next(SpecialAbilities.Count)];
            return ability;
        }

        public override void TakeDamage(int damage)
        {
            if (_random.Next(100) < 15)
            {
                damage = (int)(damage * 0.8);
                ConsoleHelper.WriteColor($"{Name} сопротивляется урону!", ConsoleColor.DarkGray);
            }

            base.TakeDamage(damage);
        }

        public override string ToString()
        {
            return $"{Name} (Босс Ур. {Level}) - HP: {Health}/{MaxHealth} ATK: {Attack}";
        }
    }
}