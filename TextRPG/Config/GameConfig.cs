namespace TextRPG.Config
{
    public class GameConfig
    {
        // Игрок
        public int BasePlayerHealth { get; set; } = 100;
        public int BasePlayerAttack { get; set; } = 10;
        public int PlayerDamageVariance { get; set; } = 3;
        public int ExpForNextLevelMultiplier { get; set; } = 2;
        public int RestRoomCooldown { get; set; } = 5;
        public int MaxInventorySize { get; set; } = 20;
        public int MaxPlayerLevel { get; set; } = 10;

        // Враг
        public int MinEnemyHealth { get; set; } = 30;
        public int MaxEnemyHealth { get; set; } = 200;
        public int EnemyHealthPerLevel { get; set; } = 6;
        public int EnemyAttackPerLevel { get; set; } = 2;
        public int BaseEnemyAttack { get; set; } = 8;
        public int BaseEnemyGold { get; set; } = 10;
        public int EnemyGoldPerLevel { get; set; } = 4;

        // Игровые механики
        public int EscapeChance { get; set; } = 50;
        public int DodgePerLevel { get; set; } = 2;

        // Генерация комнат
        public int BaseEmptyRoomChance { get; set; } = 25;
        public int BaseEnemyRoomChance { get; set; } = 20;
        public int BaseTreasureRoomChance { get; set; } = 10;
        public int BaseMerchantRoomChance { get; set; } = 5;
        public int BaseRestRoomChance { get; set; } = 5;

        public int MaxDugeonGenerationAttempts { get; set; } = 10;
        public int EnemyChance { get; set; } = 20;
        public int TreasureChance { get; set; } = 10;

        // Босс
        public int BossHealthMuliplier { get; set; } = 10;
        public int BossAttackMultiplier { get; set; } = 3;
    }
}