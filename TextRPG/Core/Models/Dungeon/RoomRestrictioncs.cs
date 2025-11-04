using System.Collections.Generic;
using System.Linq;
using TextRPG.Core.Enums;

namespace TextRPG
{
    public class RoomRestrictions
    {
        public bool IsNearStart { get; set; }
        public bool HasMerchantNeighbor { get; set; }
        public bool HasRestNeighbor { get; set; }
        public bool HasTreasureNeighbor { get; set; }
        public int DistanceToStart { get; set; }
        public int DistanceToBoss { get; set; }
        public int Depth { get; set; }

        public bool IsRoomTypeAllowed(RoomType roomType)
        {
            if (IsNearStart)
            {
                var forbiddenNearStart = new[] { RoomType.Treasure, RoomType.Merchant, RoomType.Rest };
                if (forbiddenNearStart.Contains(roomType))
                {
                    return false;
                }
            }

            return roomType switch
            {
                RoomType.Merchant when HasMerchantNeighbor => false,
                RoomType.Rest when HasRestNeighbor => false,
                RoomType.Treasure when HasTreasureNeighbor => false,
                _ => true
            };
        }

        public int GetRoomTypePriority(RoomType roomType, int currentCount, int targetCount)
        {
            int distributionPriority = (targetCount - currentCount) * 10;

            int localPriority = roomType switch
            {
                RoomType.Enemy when DistanceToBoss <= 3 => 7,
                RoomType.Treasure when DistanceToBoss <= 2 => 6,
                RoomType.Rest when DistanceToStart >= 3 && Depth == 1 => 6,
                RoomType.Merchant when DistanceToStart <= 3 => 4,
                _ => 1
            };

            if (roomType == RoomType.Enemy)
            {
                localPriority += Depth * 2;
            }

            return distributionPriority + localPriority;
        }
    }
}