using TextRPG.Core.Interfaces;

namespace TextRPG.Core.Models
{
    public abstract class GameObject : IGameObject
    {
        public string Name { get; protected set; }
        public int X { get; set; }
        public int Y { get; set; }

        protected GameObject(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
        }

        public override bool Equals(object? obj) => Equals(obj as GameObject);
        
        public bool Equals(IGameObject? other) => 
            other != null && Name == other.Name && X == other.X && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(Name, X, Y);

        public override string ToString() => $"{Name} на ({X}, {Y})";
    }
}