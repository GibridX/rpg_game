using System;
using System.Collections;
using System.Linq;
using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Services.Generation
{
    public class DungeonGenerationService
    {
        private readonly GameConfig _config;
        private readonly Random _random;

        public DungeonGenerationService(GameConfig config, int seed = 0)
        {
            _config = config;
            _random = seed == 0 ? new Random() : new Random(seed);
        }

        public Dungeon GenerateDungeon(string name, int width, int height, int maxRooms, int depth = 1)
        {
            Dungeon dungeon;
            bool isConnected;
            int attempts = 0;

            do
            {
                dungeon = GenerateDungeonAttempt(name, width, height, maxRooms, depth);
                var startRoom = dungeon.Rooms.FirstOrDefault(r => r.Type == RoomType.Start);
                var bossRoom = dungeon.Rooms.FirstOrDefault(r => r.Type == RoomType.Boss);

                if (startRoom == null || bossRoom == null)
                {
                    isConnected = false;
                    continue;
                }

                isConnected = IsDungeonConnected(dungeon, startRoom, bossRoom);
                attempts++;

                if (attempts >= _config.MaxDugeonGenerationAttempts - 1 && !isConnected)
                {
                    return CreateFallbackDungeon(name, width, height, depth);
                }
            } while (!isConnected && attempts < _config.MaxDugeonGenerationAttempts);

            if (!isConnected)
            {
                ConsoleHelper.WriteColor($"Внимание: не удалось создать идеальное подземелье после {attempts} попыток", ConsoleColor.Yellow);
            }
            return dungeon;
        }

        private Dungeon CreateFallbackDungeon(string name, int width, int height, int depth)
        {
            var dungeon = new Dungeon(name + " (Fallback)", width, height, depth);

            var startRoom = new Room(0, 0, RoomType.Start);
            dungeon.AddRoom(startRoom);

            var bossRoom = new Room(width - 1, height - 1, RoomType.Boss);
            dungeon.AddRoom(bossRoom);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((x == 0 && y == 0) || (x == width - 1 && y == height - 1)) continue;

                    var roomType = RoomType.Empty;
                    if (_random.Next(100) < 30) roomType = RoomType.Enemy;
                    else if (_random.Next(100) < 10) roomType = RoomType.Treasure;

                    var room = new Room(x, y, roomType);
                    dungeon.AddRoom(room);
                }
            }

            return dungeon;
        }

        private bool IsDungeonConnected(Dungeon dungeon, Room startRoom, Room exitRoom)
        {
            var visited = new HashSet<Room>();
            var queue = new Queue<Room>();

            queue.Enqueue(startRoom);
            visited.Add(startRoom);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == exitRoom) return true;

                var neighbors = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
                foreach (var (dx, dy) in neighbors)
                {
                    var neighbor = dungeon.GetRoom(current.X + dx, current.Y + dy);
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }

        private Dungeon GenerateDungeonAttempt(string name, int width, int height, int maxRooms, int depth = 1)
        {
            var dungeon = new Dungeon(name, width, height, depth);
            var visited = new bool[width, height];

            int x = width / 2;
            int y = height / 2;

            var rooms = new List<Room>();
            var startRoom = new Room(x, y, RoomType.Empty);
            dungeon.AddRoom(startRoom);
            rooms.Add(startRoom);
            visited[x, y] = true;

            int roomCreated = 1;
            int attemptsWithoutProgress = 0;
            const int maxAttemptsWithoutProgress = 100;

            while (roomCreated < maxRooms && attemptsWithoutProgress < maxAttemptsWithoutProgress)
            {
                int direction = _random.Next(4);
                int newX = x, newY = y;

                switch (direction)
                {
                    case 0: newX++; break;
                    case 1: newX--; break;
                    case 2: newY++; break;
                    case 3: newY--; break;
                }

                if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                {
                    if (!visited[newX, newY])
                    {
                        var room = new Room(newX, newY, RoomType.Empty);
                        dungeon.AddRoom(room);
                        rooms.Add(room);
                        visited[newX, newY] = true;
                        roomCreated++;
                        x = newX;
                        y = newY;
                        attemptsWithoutProgress = 0;
                    }
                    else
                    {
                        attemptsWithoutProgress++;
                    }
                }
                else
                {
                    attemptsWithoutProgress++;
                }
            }

            startRoom.Type = RoomType.Start;

            var bossRoom = rooms.Where(r => r != startRoom).OrderByDescending(r => Math.Abs(r.X - startRoom.X) + Math.Abs(r.Y - startRoom.Y)).FirstOrDefault();
            if (bossRoom == null)
            {
                bossRoom = rooms.FirstOrDefault(r => r != startRoom);
            }

            if (bossRoom == null)
            {
                int bossX = (x + 1) % width;
                int bossY = (y + 1) % height;
                bossRoom = new Room(bossX, bossY, RoomType.Boss);
                dungeon.AddRoom(bossRoom);
                rooms.Add(bossRoom);
            }
            else
            {
                bossRoom.Type = RoomType.Boss;
            }

            var deadEnds = rooms.Where(r => r.Type == RoomType.Empty && CountNeighbors(dungeon, r) == 1).ToList();
            foreach (var room in deadEnds.Take(Math.Min(3, deadEnds.Count)))
            {
                room.Type = RoomType.Treasure;
            }

            foreach (var room in rooms.Where(r => r.Type == RoomType.Empty))
            {
                room.Type = GetRandomRoomType(depth, room, startRoom, bossRoom);
            }

            foreach (var room in rooms)
            {
                PopulateRoom(room, depth);
            }

            return dungeon;
        }

        private int CountNeighbors(Dungeon dungeon, Room room)
        {
            int count = 0;
            var neighbors = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };

            foreach (var (dx, dy) in neighbors)
            {
                var neighbor = dungeon.GetRoom(room.X + dx, room.Y + dy);
                if (neighbor != null) count++;
            }
            return count;
        }

        private RoomType GetRandomRoomType(int depth, Room room, Room startRoom, Room bossRoom)
        {
            int distanceToBoss = Math.Abs(room.X - bossRoom.X) + Math.Abs(room.Y - bossRoom.Y);
            int roll = _random.Next(100);

            int enemyChance = _config.BaseEmptyRoomChance + (depth * 5);
            int treasureChance = _config.BaseTreasureRoomChance + (depth * 3);
            int merchantChance = _config.BaseMerchantRoomChance;
            int restChance = _config.BaseRestRoomChance;

            if (distanceToBoss <= 2)
            {
                if (roll < enemyChance) return RoomType.Enemy;
                if (roll < enemyChance + treasureChance) return RoomType.Treasure;
                if (roll < enemyChance + treasureChance + merchantChance) return RoomType.Merchant;
                if (roll < enemyChance + treasureChance + merchantChance + restChance) return RoomType.Rest;
                return RoomType.Empty;
            }
            else
            {
                int emptyChance = _config.BaseEmptyRoomChance;
                if (roll < emptyChance) return RoomType.Empty;
                if (roll < emptyChance + enemyChance) return RoomType.Enemy;
                if (roll < emptyChance + enemyChance + treasureChance) return RoomType.Treasure;
                if (roll < emptyChance + enemyChance + treasureChance + merchantChance) return RoomType.Merchant;
                if (roll < emptyChance + enemyChance + treasureChance + merchantChance + restChance) return RoomType.Rest;
                return RoomType.Empty;
            }
        }

        private void PopulateRoom(Room room, int depth)
        {
            switch (room.Type)
            {
                case RoomType.Treasure:
                    string[] treasureNames = { "Золотой нагрудник", "Магический амулет", "Шлем воина" };
                    string treasureName = treasureNames[_random.Next(treasureNames.Length)];

                    var treasure = new Item(treasureName, room.X, room.Y,
                        30 + (depth * 10), ItemType.Treasure, EquipmentType.Armor, ConsoleColor.Magenta);
                    room.AddItem(treasure);
                    break;

                case RoomType.Enemy:
                    if (_random.Next(100) < 70)
                    {
                        var itemType = _random.Next(4) switch
                        {
                            0 => ItemType.Weapon,
                            1 => ItemType.Potion,
                            2 => ItemType.Scroll,
                            _ => ItemType.Treasure
                        };

                        var item = itemType switch
                        {
                            ItemType.Weapon => new Item("Стальной меч", room.X, room.Y,
                                10 + (depth * 5), itemType, EquipmentType.Weapon, ConsoleColor.Yellow),
                            ItemType.Potion => new Item("Большое зелье", room.X, room.Y,
                                30 + (depth * 8), itemType, EquipmentType.Other, ConsoleColor.Green),
                            ItemType.Scroll => new Item("Свиток телепортации", room.X, room.Y,
                                15, itemType, EquipmentType.Other, ConsoleColor.Blue),
                            _ => new Item("Драгоценный камень", room.X, room.Y,
                                40 + (depth * 10), itemType, EquipmentType.Other, ConsoleColor.Magenta)
                        };
                        room.AddItem(item);
                    }
                    break;

                case RoomType.Boss:
                    var potion = new Item("Эликсир здоровья", room.X, room.Y,
                        60 + (depth * 15), ItemType.Potion, EquipmentType.Other, ConsoleColor.Green);
                    room.AddItem(potion);

                    var weapon = new Item("Легендарный меч", room.X, room.Y,
                        50 + (depth * 20), ItemType.Weapon, EquipmentType.Weapon, ConsoleColor.Yellow);
                    room.AddItem(weapon);
                    break;

                case RoomType.Start:
                    var startPotion = new Item("Малое зелье здоровья", room.X, room.Y,
                        25, ItemType.Potion, EquipmentType.Other, ConsoleColor.Green);
                    room.AddItem(startPotion);
                    break;
            }
        }
    }
}