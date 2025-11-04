using TextRPG.Core.Models;

namespace TextRPG.Core.Services.Generation
{
    public interface IRoomService
    {
        void ProcessRoom(Room room, Player player, Dungeon dungeon, ref bool bossDefeated, Action<int> onDepthIncrease);
    }
}