using TextRPG.Core.Models;
using TextRPG.Core.Enums;
using TextRPG.Core.Utils;

namespace TextRPG
{
    public class Dungeon : GameObject, IEquatable<Dungeon>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<Room> Rooms { get; private set; }
        public int Depth { get; set; }

        public Dungeon(string name, int width, int height, int depth = 1) : base(name, 0, 0)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Rooms = new List<Room>();
        }

        public void AddRoom(Room room)
        {
            Rooms.Add(room);
        }

        public Room? GetRoom(int x, int y)
        {
            return Rooms.FirstOrDefault(r => r.X == x && r.Y == y);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Dungeon);
        }

        public bool Equals(Dungeon? other)
        {
            return base.Equals(other) && Width == other.Width
            && Height == other.Height && Rooms.SequenceEqual(other.Rooms);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Width);
            hash.Add(Height);
            foreach (var room in Rooms.OrderBy(r => r.X).ThenBy(r => r.Y))
            {
                hash.Add(room);
            }
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"{Name} - Размер: {Width}x{Height}, Комнат: {Rooms.Count}, Глубина: {Depth}";
        }

        public void DisplayMiniMap(Player player, bool isExitActive = false)
        {
            Console.WriteLine($"\n=== Миникарта {Name} (Глубина: {Depth}) ===");

            int startX = Math.Max(0, player.X - 3);
            int endX = Math.Min(Width - 1, player.X + 3);
            int startY = Math.Max(0, player.Y - 2);
            int endY = Math.Min(Height - 1, player.Y + 2);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    var room = GetRoom(x, y);
                    if (room != null)
                    {
                        if (player.X == x && player.Y == y)
                        {
                            ConsoleHelper.WriteColorInline("P ", ConsoleColor.Green);
                        }
                        else
                        {
                            char symbol = GetRoomSymbol(room);
                            ConsoleColor color = GetRoomColor(room, isExitActive);
                            ConsoleHelper.WriteColorInline(symbol + " ", color);
                        }
                    }
                    else
                    {
                        ConsoleHelper.WriteColorInline("# ", ConsoleColor.DarkGray);
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nЛегенда:");
            ConsoleHelper.WriteColorInline("P - Вы ", ConsoleColor.Green);
            ConsoleHelper.WriteColorInline("S - Старт ", ConsoleColor.Blue);
            ConsoleHelper.WriteColorInline("M - Враг ", ConsoleColor.Red);
            ConsoleHelper.WriteColorInline("T - Сокровище ", ConsoleColor.Magenta);
            ConsoleHelper.WriteColorInline("B - Босс ", ConsoleColor.DarkRed);
            ConsoleHelper.WriteColorInline("$ - Торговец ", ConsoleColor.DarkYellow);
            ConsoleHelper.WriteColorInline($"R - Отдых \n", ConsoleColor.DarkGreen);
            ConsoleHelper.WriteColorInline("+ - Пустая ", ConsoleColor.Gray);
            ConsoleHelper.WriteColorInline("# - Стена", ConsoleColor.DarkRed);
            Console.WriteLine();
        }

        public char GetRoomSymbol(Room room)
        {
            return room.Type switch
            {
                RoomType.Start => 'S',
                RoomType.Enemy => 'M',
                RoomType.Treasure => 'T',
                RoomType.Boss => 'B',
                RoomType.Merchant => '$',
                RoomType.Rest => 'R',
                RoomType.Empty => '+',
                _ => '?'
            };
        }

        public ConsoleColor GetRoomColor(Room room, bool isExitActive = false)
        {
            return room.Type switch
            {
                RoomType.Start => ConsoleColor.Blue,
                RoomType.Enemy => ConsoleColor.Red,
                RoomType.Treasure => ConsoleColor.Magenta,
                RoomType.Boss => ConsoleColor.DarkRed,
                RoomType.Merchant => ConsoleColor.DarkYellow,
                RoomType.Rest => ConsoleColor.DarkGreen,
                RoomType.Empty => ConsoleColor.Gray,
                _ => ConsoleColor.White
            };
        }
    }
}