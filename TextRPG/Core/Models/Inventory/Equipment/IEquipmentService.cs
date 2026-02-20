namespace TextRPG.Core.Models.Inventory.Equipment
{
    public interface IEquipmentService
    {
        bool EquipItem(string itemKey);
        void UnequipItem(Item item, bool silent = false);
        void UnequipWeapon();
        void UnequipArmor();
    }
}