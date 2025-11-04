namespace TextRPG.Core.Interfaces
{
    public interface IGameObject : IEquatable<IGameObject>
    {
        string Name { get; }
        int X { get; set; }
        int Y { get; set; }
    }

    public interface ICombatant
    {
        int Health { get; set; }
        int MaxHealth { get; set; }
        int Attack { get; set; }
        void TakeDamage(int damage);
        void Heal(int amount);
    }
}