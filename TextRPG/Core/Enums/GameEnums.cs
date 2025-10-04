namespace TextRPG.Core.Enums
{
    public enum RoomType
    {
        Empty,
        Enemy,
        Treasure,
        Boss,
        Start,
        Exit,
        Merchant,
        Rest
    }

    public enum ItemType
    {
        Weapon,
        Potion,
        Treasure,
        Scroll
    }

    public enum EquipmentType { Weapon, Armor, Other }

    public enum CombatAction
    {
        Attack,
        Defend,
        UseItem,
        Escape
    }
}