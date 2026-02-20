using System;
using System.Collections;
using System.Linq;
using TextRPG.Core.Models;
using TextRPG.Core.Enums;

namespace TextRPG.Core.Models.Inventory
{
    public class Inventory
    {
        public Dictionary<string, Item> Items { get; private set; }
        private Dictionary<string, List<string>> _nameToKeys;
        public Item? EquippedWeapon { get; set; }
        public Item? EquippedArmor { get; set; }
        public int Capacity { get; set; }
        public int Count => Items.Count;
        public bool IsFull => Count >= Capacity;

        public Inventory(int capacity)
        {
            Items = new Dictionary<string, Item>();
            _nameToKeys = new Dictionary<string, List<string>>();
            Capacity = capacity;
        }

        public bool AddItem(Item item)
        {
            if (IsFull) return false;

            string itemKey = GenerateItemKey(item);
            Items[itemKey] = item;

            string lowerName = item.Name.ToLower();
            if (!_nameToKeys.ContainsKey(lowerName))
                _nameToKeys[lowerName] = new List<string>();
            _nameToKeys[lowerName].Add(itemKey);
            return true;
        }

        public bool RemoveItem(string itemKey)
        {
            if (Items.TryGetValue(itemKey, out var item))
            {
                if (item == EquippedWeapon || item == EquippedArmor)
                {
                    return false;
                }

                bool removed = Items.Remove(itemKey);

                if (removed)
                {
                    string lowerName = item.Name.ToLower();
                    if (_nameToKeys.ContainsKey(lowerName))
                    {
                        _nameToKeys[lowerName].Remove(itemKey);
                        if (_nameToKeys[lowerName].Count == 0)
                        {
                            _nameToKeys.Remove(lowerName);
                        }
                    }
                }

                return removed;
            }
            return false;
        }

        public Item? GetItem(string itemKey)
        {
            return Items.TryGetValue(itemKey, out var item) ? item : null;
        }

        public bool HasItem(string itemName)
        {
            return _nameToKeys.ContainsKey(itemName.ToLower());
        }

        public int GetItemCount(string itemName)
        {
            string lowerName = itemName.ToLower();
            return _nameToKeys.ContainsKey(lowerName) ? _nameToKeys[lowerName].Count : 0;
        }

        public string? GetItemKey(Item item)
        {
            string lowerName = item.Name.ToLower();
            if (_nameToKeys.TryGetValue(lowerName, out var keys))
            {
                return keys.FirstOrDefault(key => Items[key] == item);
            }
            return null;
        }

        public List<Item> GetItemsByType(ItemType type)
        {
            return Items.Values.Where(item => item.Type == type).ToList();
        }

        public List<KeyValuePair<string, Item>> GetItemsForSale()
        {
            return Items.Where(kvp =>
            kvp.Value != EquippedWeapon &&
            kvp.Value != EquippedArmor).ToList();
        }

        public List<KeyValuePair<string, Item>> GetEquippableItems()
        {
            return Items.Where(kvp => (kvp.Value.Type == ItemType.Weapon && kvp.Value.EquipmentType == EquipmentType.Weapon) ||
            (kvp.Value.Type == ItemType.Treasure && kvp.Value.EquipmentType == EquipmentType.Armor)).ToList();
        }

        public void Clear()
        {
            Items.Clear();
            _nameToKeys.Clear();
            EquippedWeapon = null;
            EquippedArmor = null;
        }

        private string GenerateItemKey(Item item)
        {
            string baseKey = $"{item.Type}_{item.Name}";

            if (item.Type == ItemType.Potion && !Items.ContainsKey(baseKey))
            {
                return baseKey;
            }

            return $"{baseKey}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }
    }
}