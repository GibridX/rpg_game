using System;
using System.Collections;
using System.Linq;
using System.Reflection.Metadata;
using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Services.Generation
{
    public class DungeonGenerationService : IDungeonGenerationService
    {
        private readonly GameConfig _config;
        private readonly IRoomRestrictionService _restrictionService;
        private readonly Random _random;

        public DungeonGenerationService(GameConfig config, IRoomRestrictionService restrictionService, int seed = 0)
        {
            _config = config;
            _restrictionService = restrictionService;
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

            BalanceRoomDistribution(dungeon, depth);
            return dungeon;
        }

        private void BalanceRoomDistribution(Dungeon dungeon, int depth)
        {
            var rooms = dungeon.Rooms.Where(r => r.Type != RoomType.Start && r.Type != RoomType.Boss).ToList();
            if (rooms.Count == 0) return;

            var targetDistribution = new Dictionary<RoomType, int>
            {
                [RoomType.Enemy] = (int)(rooms.Count * GetEnemyRoomPercentage(depth)),
                [RoomType.Treasure] = (int)(rooms.Count * GetTreasureRoomPercentage(depth)),
                [RoomType.Merchant] = (int)(rooms.Count * GetMerchantRoomPercentage(depth)),
                [RoomType.Rest] = (int)(rooms.Count * GetRestRoomPercentage(depth)),
                [RoomType.Empty] = (int)(rooms.Count * GetEmptyRoomPercentage(depth))
            };

            var currentRooms = rooms.ToList();

            var enemyRooms = currentRooms
                .Where(r => r.Type == RoomType.Enemy)
                .Take(targetDistribution[RoomType.Enemy])
                .ToList();

            var otherRooms = currentRooms.Except(enemyRooms).ToList();

            foreach (var room in otherRooms)
            {
                room.Type = GetBalancedRoomType(room, dungeon, depth, targetDistribution);
            }

            EnsureMinimumRooms(dungeon, depth);

            foreach (var room in dungeon.Rooms)
            {
                room.Items.Clear();
                PopulateRoom(room, depth);
            }
        }


        private RoomType GetBalancedRoomType(Room room, Dungeon dungeon, int depth, Dictionary<RoomType, int> targetDistribution)
        {
            var restrictions = _restrictionService.GetRoomRestrictions(room, dungeon, depth);
            var currentCounts = dungeon.Rooms
                .Where(r => r.Type != RoomType.Start && r.Type != RoomType.Boss)
                .GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            var roomPriorities = new List<(RoomType Type, int Priority)>();

            foreach (var target in targetDistribution)
            {
                if (!_restrictionService.IsRoomTypeAllowed(target.Key, restrictions))
                    continue;

                var currentCount = currentCounts.GetValueOrDefault(target.Key, 0);
                var needed = target.Value - currentCount;

                if (needed > 0)
                {
                    var priority = _restrictionService.GetRoomTypePriority(target.Key, restrictions, currentCount, target.Value);
                    roomPriorities.Add((target.Key, priority));
                }
            }

            if (roomPriorities.Count == 0)
            {
                return GetWeightedRandomRoomType(depth, room, dungeon);
            }

            return roomPriorities
                .OrderByDescending(x => x.Priority)
                .First()
                .Type;
        }

        private RoomType GetWeightedRandomRoomType(int depth, Room room, Dungeon dungeon)
        {
            var restrictions = _restrictionService.GetRoomRestrictions(room, dungeon, depth);

            var weights = new Dictionary<RoomType, int>
            {
                [RoomType.Enemy] = GetEnemyRoomWeight(depth, restrictions.DistanceToBoss),
                [RoomType.Treasure] = _restrictionService.IsRoomTypeAllowed(RoomType.Treasure, restrictions) ?
                    GetTreasureRoomWeight(depth, restrictions.DistanceToBoss) : 0,
                [RoomType.Merchant] = _restrictionService.IsRoomTypeAllowed(RoomType.Merchant, restrictions) ?
                    GetMerchantRoomWeight(depth) : 0,
                [RoomType.Rest] = _restrictionService.IsRoomTypeAllowed(RoomType.Rest, restrictions) ?
                    GetRestRoomWeight(depth, restrictions.DistanceToBoss) : 0,
                [RoomType.Empty] = GetEmptyRoomWeight(depth, restrictions.DistanceToBoss)
            };

            if (depth >= 3)
            {
                weights[RoomType.Enemy] += 15;
            }
            else if (depth >= 2)
            {
                weights[RoomType.Enemy] += 10;
            }

            return GetRoomTypeByWeights(weights);
        }

        private float GetEnemyRoomPercentage(int depth)
        {
            float basePercentage = _config.BaseEnemyRoomChance / 100f;

            return depth switch
            {
                1 => Math.Min(0.3f, basePercentage),
                2 => Math.Min(0.35f, basePercentage + 0.05f),
                3 => Math.Min(0.4f, basePercentage),
                4 => Math.Min(0.55f, basePercentage + 0.05f),
                _ => Math.Min(0.6f, basePercentage + 0.1f)
            };
        }

        private float GetTreasureRoomPercentage(int depth)
            => Math.Min(0.20f, (_config.BaseTreasureRoomChance / 100f) + (depth * 0.015f));

        private float GetMerchantRoomPercentage(int depth)
            => Math.Min(0.1f, (_config.BaseMerchantRoomChance / 100f) + (depth * 0.01f));

        private float GetRestRoomPercentage(int depth)
            => Math.Min(0.15f, (_config.BaseRestRoomChance / 100f) + (depth * 0.01f));

        private float GetEmptyRoomPercentage(int depth)
        {
            float basePercentage = _config.BaseEmptyRoomChance / 100f;
            return depth switch
            {
                1 => Math.Max(0.2f, basePercentage + 0.1f),
                2 => Math.Max(0.15f, basePercentage + 0.05f),
                _ => Math.Max(0.1f, basePercentage)
            };
        }
        private int GetEnemyRoomWeight(int depth, int distanceToBoss)
        {
            int baseWeight = 20 + (depth * 4);
            return distanceToBoss <= 3 ? baseWeight + 5 : baseWeight;
        }

        private int GetTreasureRoomWeight(int depth, int distanceToBoss)
        {
            int baseWeight = 10 + (depth * 1);
            return distanceToBoss <= 2 ? baseWeight + 5 : baseWeight;
        }

        private int GetMerchantRoomWeight(int depth)
            => Math.Max(3, 8 - depth);

        private int GetRestRoomWeight(int depth, int distanceToBoss)
        {
            int baseWeight = 8 + depth;
            return distanceToBoss >= 4 ? baseWeight + 3 : baseWeight;
        }

        private int GetEmptyRoomWeight(int depth, int distanceToBoss)
            => Math.Max(5, 15 - (depth * 2));

        private void EnsureMinimumRooms(Dungeon dungeon, int depth)
        {
            var rooms = dungeon.Rooms.Where(r => r.Type != RoomType.Start && r.Type != RoomType.Boss).ToList();
            if (rooms.Count < 5) return;

            int minEnemyRooms = depth switch
            {
                1 => 1,
                2 => 2,
                3 => 3,
                _ => 4
            };

            var enemyRooms = rooms.Count(r => r.Type == RoomType.Enemy);
            if (enemyRooms < minEnemyRooms)
            {
                var emptyRooms = rooms.Where(r => r.Type == RoomType.Empty).ToList();
                foreach (var room in emptyRooms.Take(minEnemyRooms - enemyRooms))
                {
                    room.Type = RoomType.Enemy;
                }
            }

            var requiredTypes = new[] { RoomType.Treasure, RoomType.Rest };

            foreach (var requiredType in requiredTypes)
            {
                if (!rooms.Any(r => r.Type == requiredType))
                {
                    var candidate = rooms.FirstOrDefault(r =>
                        r.Type == RoomType.Empty &&
                        _restrictionService.IsRoomTypeAllowed(requiredType,
                            _restrictionService.GetRoomRestrictions(r, dungeon, depth)));

                    if (candidate != null)
                    {
                        candidate.Type = requiredType;
                    }
                }
            }

            if (depth >= 2 && !rooms.Any(r => r.Type == RoomType.Merchant))
            {
                var candidate = rooms.FirstOrDefault(r =>
                    r.Type == RoomType.Empty &&
                    _restrictionService.IsRoomTypeAllowed(RoomType.Merchant,
                        _restrictionService.GetRoomRestrictions(r, dungeon, depth)));

                if (candidate != null)
                {
                    candidate.Type = RoomType.Merchant;
                }
            }
        }

        private bool IsRoomNearStart(Room room, Dungeon dungeon)
        {
            var restrictions = _restrictionService.GetRoomRestrictions(room, dungeon, 1);
            return restrictions.IsNearStart;
        }

        private Dungeon CreateFallbackDungeon(string name, int width, int height, int depth)
        {
            var dungeon = new Dungeon(name + " (Fallback)", width, height, depth);

            var startRoom = new Room(0, 0, RoomType.Start);
            dungeon.AddRoom(startRoom);

            var bossRoom = new Room(width - 1, height - 1, RoomType.Boss);
            dungeon.AddRoom(bossRoom);

            int currentX = 0, currentY = 0;
            while (currentX < width - 1 || currentY < height - 1)
            {
                if (currentX < width - 1)
                {
                    currentX++;
                    var roomType = GetBalancedFallbackRoomType(depth, currentX, currentY, startRoom, dungeon);
                    var room = new Room(currentX, currentY, roomType);
                    dungeon.AddRoom(room);
                }

                if (currentY < height - 1)
                {
                    currentY++;
                    var roomType = GetBalancedFallbackRoomType(depth, currentX, currentY, startRoom, dungeon);
                    var room = new Room(currentX, currentY, roomType);
                    dungeon.AddRoom(room);
                }
            }

            var additionalRooms = Math.Min(8, width * height - dungeon.Rooms.Count);

            for (int i = 0; i < additionalRooms; i++)
            {
                int x = _random.Next(width);
                int y = _random.Next(height);

                if (dungeon.GetRoom(x, y) == null)
                {
                    var roomType = GetBalancedFallbackRoomType(depth, x, y, startRoom, dungeon);
                    var room = new Room(x, y, roomType);
                    dungeon.AddRoom(room);
                }
            }

            BalanceRoomDistribution(dungeon, depth);
            return dungeon;
        }

        private RoomType GetBalancedFallbackRoomType(int depth, int x, int y, Room startRoom, Dungeon dungeon)
        {
            var currentRoom = new Room(x, y, RoomType.Empty);
            var restrictions = _restrictionService.GetRoomRestrictions(currentRoom, dungeon, depth);

            if (restrictions.IsNearStart)
            {
                var nearbyWeights = new Dictionary<RoomType, int>
                {
                    [RoomType.Enemy] = 50,
                    [RoomType.Merchant] = _restrictionService.IsRoomTypeAllowed(RoomType.Merchant, restrictions) ? 20 : 0,
                    [RoomType.Empty] = 30
                };

                var totalWeight = nearbyWeights.Values.Sum();
                if (totalWeight > 0)
                {
                    var randomValue = _random.Next(totalWeight);
                    var currentWeight = 0;

                    foreach (var weight in nearbyWeights)
                    {
                        currentWeight += weight.Value;
                        if (randomValue < currentWeight)
                        {
                            return weight.Key;
                        }
                    }
                }
            }

            var weights = new Dictionary<RoomType, int>
            {
                [RoomType.Enemy] = 40 + (depth * 5),
                [RoomType.Treasure] = _restrictionService.IsRoomTypeAllowed(RoomType.Treasure, restrictions) ? 15 + (depth * 3) : 0,
                [RoomType.Merchant] = _restrictionService.IsRoomTypeAllowed(RoomType.Merchant, restrictions) ? 5 + depth : 0,
                [RoomType.Rest] = _restrictionService.IsRoomTypeAllowed(RoomType.Rest, restrictions) ? 10 + depth : 0,
                [RoomType.Empty] = 30 - (depth * 3)
            };

            return GetRoomTypeByWeights(weights);
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
            if (bossRoom == null && rooms.Count > 1)
            {
                bossRoom = rooms.First(r => r != startRoom);
            }

            else if (bossRoom == null)
            {
                int bossX = (startRoom.X + 2) % width;
                int bossY = (startRoom.Y + 2) % height;
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
                if (!IsRoomNearStart(room, dungeon))
                {
                    room.Type = RoomType.Treasure;
                }
            }

            foreach (var room in rooms.Where(r => r.Type == RoomType.Empty))
            {
                room.Type = GetRandomRoomType(depth, room, startRoom, bossRoom, dungeon);
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

        private RoomType GetRandomRoomType(int depth, Room room, Room startRoom, Room bossRoom, Dungeon dungeon)
        {
            var restrictions = _restrictionService.GetRoomRestrictions(room, dungeon, depth);

            if (restrictions.IsNearStart)
            {
                return GetRoomTypeNearStart(depth);
            }

            if (restrictions.DistanceToBoss <= 2)
            {
                return GetRoomTypeNearBoss(depth, room, dungeon);
            }
            else
            {
                return GetNormalRoomType(depth, room, dungeon);
            }
        }

        private RoomType GetRoomTypeNearStart(int depth)
        {
            var weights = new Dictionary<RoomType, int>
            {
                [RoomType.Enemy] = 60,
                [RoomType.Empty] = 25,
                [RoomType.Merchant] = 15
            };

            return GetRoomTypeByWeights(weights);
        }

        private RoomType GetRoomTypeNearBoss(int depth, Room room, Dungeon dungeon)
        {
            var restrictions = _restrictionService.GetRoomRestrictions(room, dungeon, depth);

            var weights = new Dictionary<RoomType, int>
            {
                [RoomType.Enemy] = 40 + (depth * 5),
                [RoomType.Treasure] = _restrictionService.IsRoomTypeAllowed(RoomType.Treasure, restrictions) ? 25 + (depth * 3) : 0,
                [RoomType.Rest] = _restrictionService.IsRoomTypeAllowed(RoomType.Rest, restrictions) ? 10 : 0,
                [RoomType.Merchant] = _restrictionService.IsRoomTypeAllowed(RoomType.Merchant, restrictions) ? 5 : 0,
                [RoomType.Empty] = Math.Max(0, 20 - (depth * 3))
            };

            return GetRoomTypeByWeights(weights);
        }

        private RoomType GetNormalRoomType(int depth, Room room, Dungeon dungeon)
        {
            var restrictions = _restrictionService.GetRoomRestrictions(room, dungeon, depth);

            var weights = new Dictionary<RoomType, int>
            {
                [RoomType.Enemy] = _config.BaseEnemyRoomChance + (depth * 3),
                [RoomType.Treasure] = _restrictionService.IsRoomTypeAllowed(RoomType.Treasure, restrictions) ?
                    _config.BaseTreasureRoomChance + (depth * 2) : 0,
                [RoomType.Rest] = _restrictionService.IsRoomTypeAllowed(RoomType.Rest, restrictions) ?
                    _config.BaseRestRoomChance : 0,
                [RoomType.Merchant] = _restrictionService.IsRoomTypeAllowed(RoomType.Merchant, restrictions) ?
                    _config.BaseMerchantRoomChance : 0,
                [RoomType.Empty] = Math.Max(0, _config.BaseEmptyRoomChance - (depth * 2))
            };

            return GetRoomTypeByWeights(weights);
        }

        private RoomType GetRoomTypeByWeights(Dictionary<RoomType, int> weights)
        {
            var totalWeight = weights.Values.Sum();
            if (totalWeight <= 0) return RoomType.Empty;

            var randomValue = _random.Next(totalWeight);
            var currentWeight = 0;

            foreach (var weight in weights)
            {
                currentWeight += weight.Value;
                if (randomValue < currentWeight)
                {
                    return weight.Key;
                }
            }

            return RoomType.Empty;
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