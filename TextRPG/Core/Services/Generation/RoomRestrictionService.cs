using System.Linq;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;

namespace TextRPG.Core.Services
{
    public class RoomRestrictionService : IRoomRestrictionService
    {
        public RoomRestrictions GetRoomRestrictions(Room room, Dungeon dungeon, int depth)
        {
            var startRoom = dungeon.Rooms.First(r => r.Type == RoomType.Start);
            var bossRoom = dungeon.Rooms.First(r => r.Type == RoomType.Boss);
            var distanceToStart = Math.Abs(room.X - startRoom.X) + Math.Abs(room.Y - startRoom.Y);
            var distanceToBoss = Math.Abs(room.X - bossRoom.X) + Math.Abs(room.Y - bossRoom.Y);

            return new RoomRestrictions
            {
                IsNearStart = distanceToStart <= 2,
                DistanceToStart = distanceToStart,
                DistanceToBoss = distanceToBoss,
                Depth = depth,
                HasMerchantNeighbor = HasNeighborOfType(room, dungeon, RoomType.Merchant),
                HasRestNeighbor = HasNeighborOfType(room, dungeon, RoomType.Rest),
                HasTreasureNeighbor = HasNeighborOfType(room, dungeon, RoomType.Treasure)
            };
        }

        public bool IsRoomTypeAllowed(RoomType roomType, RoomRestrictions restrictions)
        {
            return restrictions.IsRoomTypeAllowed(roomType);
        }

        public int GetRoomTypePriority(RoomType roomType, RoomRestrictions restrictions, int currentCount, int targetCount)
        {
            return restrictions.GetRoomTypePriority(roomType, currentCount, targetCount);
        }

        private bool HasNeighborOfType(Room room, Dungeon dungeon, RoomType roomType)
        {
            var neighbors = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };

            foreach (var (dx, dy) in neighbors)
            {
                var neighbor = dungeon.GetRoom(room.X + dx, room.Y + dy);
                if (neighbor != null && neighbor.Type == roomType)
                {
                    return true;
                }
            }
            return false;
        }
    }
}