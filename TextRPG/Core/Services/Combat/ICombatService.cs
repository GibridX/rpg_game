using TextRPG.Core.Models;

namespace TextRPG.Core.Services
{
    public interface ICombatService
    {
        CombatResult StartCombat(Player player, Enemy enemy);
        CombatResult StartBossFight(Player player, Boss boss);
    }

    public record CombatResult(bool PlayerWon, int ExperienceGained, int GoldGained);
}