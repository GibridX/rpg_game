using TextRPG.Core.Enums;
using TextRPG.Core.Models;

namespace TextRPG.Core.Factory
{
    public interface IEnemyFactory
    {
        Enemy CreateRegularEnemy(int x, int y, int playerLevel, int depth);
        Boss CreateBoss(int x, int y, int playerLevel, int depth);
        Enemy CreateEnemyByRoomType(RoomType roomType, int x, int y, int playerLevel, int depth);
    }
}