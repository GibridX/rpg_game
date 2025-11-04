using System.Linq;
using TextRPG.Core.Enums;
using TextRPG.Core.Models;

namespace TextRPG.Core.Services
{
    public interface IRoomRestrictionService
    {
        RoomRestrictions GetRoomRestrictions(Room room, Dungeon dungeon, int depth);
        bool IsRoomTypeAllowed(RoomType roomType, RoomRestrictions restrictions);
        int GetRoomTypePriority(RoomType roomType, RoomRestrictions restrictions, int currentCount, int targetCount);
    }
}