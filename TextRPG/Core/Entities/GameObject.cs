public abstract class GameObject : IEquatable<GameObject>
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as GameObject);
        }

        public bool Equals(GameObject? other)
        {
            return other != null && Name == other.Name
            && X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, X, Y);
        }

        public override string ToString()
        {
            return $"{Name} на ({X}, {Y})";
        }
    }