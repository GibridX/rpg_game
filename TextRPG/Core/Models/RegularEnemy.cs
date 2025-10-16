using TextRPG.Config;

namespace TextRPG.Core.Models
{
    public class RegularEnemy : Enemy
    {
        private readonly Random _random;
        private readonly GameConfig _config;

        public RegularEnemy(string name, int x, int y, int level, GameConfig config)
            : base(name, x, y, level)
        {
            _config = config;
            _random = new Random();
            InitializeStats();
        }

        private void InitializeStats()
        {
            MaxHealth = Math.Min(
                _config.MinEnemyHealth + (Level * _config.EnemyHealthPerLevel),
                _config.MaxEnemyHealth
            );
            Health = MaxHealth;
            Attack = _config.BaseEnemyAttack + (Level * _config.EnemyAttackPerLevel);
            GoldReward = _config.BaseEnemyGold + (Level * _config.EnemyGoldPerLevel);
            ExperienceReward = 15 + (Level * 3);
            IsBoss = false;
        }

        public override int CalculateDamage()
        {
            int variance = _random.Next(-2, 3);
            return Math.Max(1, Attack + variance);
        }

        public override string ToString()
        {
            return $"{Name} (Ур. {Level}) - HP: {Health}/{MaxHealth} ATK: {Attack}";
        }
    }
}