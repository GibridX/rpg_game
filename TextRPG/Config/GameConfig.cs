namespace TextRPG.Config
{
    public static class GameConfig
    {
        // Игрок
        public static int BasePlayerHealth = 100;
        public static int BasePlayerAttack = 10;
        public static int PlayerDamageVariance = 3;
        public static int ExpForNextLevelMultiplier = 2;
        public static int RestRoomCooldown = 5;
        public static int MaxInventorySize = 20;
        public static int MaxPlayerLevel = 25;

        // Враг
        public static int MinEnemyHealth = 30;
        public static int MaxEnemyHealth = 200;
        public static int EnemyHealthPerLevel = 6;
        public static int EnemyAttackPerLevel = 2;
        public static int BaseEnemyAttack = 8;
        public static int BaseEnemyGold = 10;
        public static int EnemyGoldPerLevel = 4;

        // Игровые механики
        public static int EscapeChance = 50;
        public static int DodgePerLevel = 2;

        // Генерация комнат
        public static int BaseEmptyRoomChance = 25;
        public static int BaseEnemyRoomChance = 20;
        public static int BaseTreasureRoomChance = 10;
        public static int BaseMerchantRoomChance = 5;
        public static int BaseRestRoomChance = 5;

        public static int MaxDugeonGenerationAttempts = 10;
        public static int EnemyChance = 20;
        public static int TreasureChance = 10;

        // Босс
        public static int BossHealthMuliplier = 10;
        public static int BossAttackMultiplier = 3;
    }
}