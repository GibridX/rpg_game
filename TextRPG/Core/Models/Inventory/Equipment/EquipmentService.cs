using System;
using TextRPG.Core.Enums;

namespace TextRPG.Core.Models.Inventory.Equipment
{
    public class EquipmentService
    {
        private readonly Player _player;

        public EquipmentService(Player player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public EquipmentResult EquipItem(string itemKey)
        {

            if (string.IsNullOrEmpty(itemKey))
                return new EquipmentResult(false, "Неверный ключ предмета!");

            if (!_player.Inventory.Items.TryGetValue(itemKey, out var item))
                return new EquipmentResult(false, "Предмет не найден в инвентаре!");
            
            Item? oldEquipment = null;
            bool equipSuccess = false;

            switch (item.Type)
            {
                case ItemType.Weapon when item.EquipmentType == EquipmentType.Weapon:
                    oldEquipment = _player.Inventory.EquippedWeapon;
                    _player.Inventory.EquippedWeapon = item;
                    _player.Attack = _player.BaseAttack + item.Value;
                    equipSuccess = true;
                    break;

                case ItemType.Treasure when item.EquipmentType == EquipmentType.Armor:
                    oldEquipment = _player.Inventory.EquippedArmor;
                    _player.Inventory.EquippedArmor = item;
                    double healthPercent = (double)_player.Health / _player.MaxHealth;
                    _player.MaxHealth = _player.BaseMaxHealth + item.Value;
                    _player.Health = (int)(_player.MaxHealth * healthPercent);
                    if (_player.Health <= 0) _player.Health = 1;
                    equipSuccess = true;
                    break;

                default:
                    return new EquipmentResult(false, "Этот предмет нельзя экипировать.");
            }

            if (equipSuccess)
            {

                _player.Inventory.Items.Remove(itemKey);

                if (oldEquipment != null)
                {
                    UnequipItem(oldEquipment, silent: true);
                    _player.Inventory.AddItem(oldEquipment);
                }

                return new EquipmentResult(true, $"Вы экипировали {item.Name}!");
            }

            return new EquipmentResult(false, "Не удалось экипировать предмет.");
        }

        public EquipmentResult UnequipItem(Item item, bool silent = false)
        {
            if (item == null)
            {
                return new EquipmentResult(false, "Предмет не может быть null.");
            }
            
            switch (item.Type)
            {
                case ItemType.Weapon when item == _player.Inventory.EquippedWeapon:
                    _player.Inventory.EquippedWeapon = null;
                    _player.Attack = _player.BaseAttack;
                    if (!silent)
                        return new EquipmentResult(true, $"Вы сняли {item.Name}");
                    break;

                case ItemType.Treasure when item == _player.Inventory.EquippedArmor:
                    _player.Inventory.EquippedArmor = null;
                    double healthPercent = (double)_player.Health / _player.MaxHealth;
                    _player.MaxHealth = _player.BaseMaxHealth;
                    _player.Health = (int)(_player.MaxHealth * healthPercent);
                    _player.Health = Math.Max(1, Math.Min(_player.Health, _player.MaxHealth));
                    if (!silent)
                        return new EquipmentResult(true, $"Вы сняли {item.Name}");
                    break;

                default:
                    return new EquipmentResult(false, "Этот предмет не экипирован.");
            }

            return new EquipmentResult(true, string.Empty);
        }

        public EquipmentResult UnequipWeapon()
        {
            if (_player.Inventory.EquippedWeapon != null)
            {
                var weapon = _player.Inventory.EquippedWeapon;
                var result = UnequipItem(weapon);
                if (result.Success)
                {
                    _player.Inventory.AddItem(weapon);
                }
                return result;
            }
            return new EquipmentResult(false, "Оружие не экипировано!");
        }
        
        public EquipmentResult UnequipArmor()
        {
            if (_player.Inventory.EquippedArmor != null)
            {
                var armor = _player.Inventory.EquippedArmor;
                var result = UnequipItem(armor);
                if (result.Success)
                {
                    _player.Inventory.AddItem(armor);
                }
                return result;
            }
            return new EquipmentResult(false, "Броня не экипирована!");
        }
    }
}