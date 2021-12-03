using System.Threading.Tasks;

namespace CombatTest
{
    public interface ICombatController
    {
        bool CanRun(CombatState combatState, int turnsToRun, int barIncrementsPerTurn);
        Task Run(CombatState combatState, int turnsToRun, int barIncrementsPerTurn);
    }

    public class CombatController : ICombatController
    {
        private const int FixedAmountOfTeamsToRun = 2;

        public bool CanRun(CombatState combatState, int turnsToRun, int barIncrementsPerTurn)
        {
            return combatState.TeamsWithOriginalUnits.Count == FixedAmountOfTeamsToRun &&
                   turnsToRun > 0 &&
                   barIncrementsPerTurn > 0;
        }

        public async Task Run(CombatState combatState, int turnsToRun, int barIncrementsPerTurn)
        {
            if (!CanRun(combatState, turnsToRun, barIncrementsPerTurn))
            {
                return;
            }

            combatState.CurrentCombatState = CombatStateEnum.Started;

            for (var i = 0; i < turnsToRun; i++)
            {
                combatState.CurrentCombatState = CombatStateEnum.Running;

                for (var j = 0; j < barIncrementsPerTurn; j++)
                {
                    combatState.IncrementUnitBarValues();
                }

                await combatState.PerformAppliedAbilities();

                foreach (var unit in combatState.GetOrderedUnitsToAttack())
                {
                    await combatState.PerformActionByUnit(unit);
                }

                combatState.UpdateAliveUnits();

                if (!combatState.HasAnyTeamAllUnitsDead())
                {
                    continue;
                }

                combatState.CurrentCombatState = CombatStateEnum.EndedWithWinningTeam;
                return;
            }

            combatState.CurrentCombatState = CombatStateEnum.EndedWithoutWinningTeam;
        }
    }
}