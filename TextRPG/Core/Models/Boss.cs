using TextRPG.Config;

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
            MaxHealth = (100 + (Level * _config.BossHealthMuliplier)) * 2;
            Health = MaxHealth;
            Attack = (10 + (Level * _config.BossAttackMultiplier)) * 2;
            GoldReward = 200 + (Level * 50);
            ExperienceReward = 100 + (Level * 25);
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
            int variance = _random.Next(-5, 10);

            if (_random.Next(100) < SpecialAbilityChance)
            {
                return (int)((Attack + variance) * 1.5);
            }

            return Math.Max(1, Attack + variance);
        }

        public string UseSpecialAbility()
        {
            if (SpecialAbilities.Count == 0)
                return "Мощная атака";

            var ability = SpecialAbilities[_random.Next(SpecialAbilities.Count)];
            return ability;
        }

        public override void TakeDamage(int damage)
        {
            if (_random.Next(100) < 20)
            {
                damage = (int)(damage * 0.7);
            }

            base.TakeDamage(damage);
        }

        public override string ToString()
        {
            return $"{Name} (Босс Ур. {Level}) - HP: {Health}/{MaxHealth} ATK: {Attack}";
        }
    }
}