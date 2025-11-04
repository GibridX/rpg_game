using TextRPG.Core.Models;
using TextRPG.Core.Enums;

namespace TextRPG
{
    public class Item : GameObject, IEquatable<Item>
    {
        public ItemType Type { get; private set; } 
        public EquipmentType EquipmentType { get; set; }
        public int Value { get; private set; }
        public ConsoleColor Color { get; set; }

        public Item(string name, int x, int y, int value, ItemType type,
                   EquipmentType equipType = EquipmentType.Other,
                   ConsoleColor color = ConsoleColor.White) : base(name, x, y)
        {
            Value = Math.Max(0, value);
            Type = type;
            EquipmentType = equipType;
            Color = color;
        }


        public Item CreateCopy(int x, int y)
        {
            return new Item(this.Name, x, y, this.Value, this.Type, this.EquipmentType, this.Color);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Item);
        }

        public bool Equals(Item? other)
        {
            return base.Equals(other) && Value == other.Value
            && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Value, Type);
        }

        public override string ToString()
        {
            return $"{Name} ({Type}) - Ценность: {Value}";
        }

        public void DisplayWithColor()
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = Color;
            Console.Write($"{Name} ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"({Type}) - Ценность: {Value}");
            Console.ForegroundColor = originalColor;
        }
    }
}