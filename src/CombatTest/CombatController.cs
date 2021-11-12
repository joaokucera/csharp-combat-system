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
            // Can run if:
            // combat system has two teams;
            // more than 0 turns defined;
            // more than 0 bar increments defined;
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
                    // The turn order will be given by the speed of each unit.
                    // Similar to FFVII, each unit will have a bar that has to be filled to be able to attack.
                    // In our case, this bar will contain 100 units.
                    combatState.IncrementUnitBarValues();
                }

                // **IsPermanent**: 
                // * If *true*, the stat modification will be applied in every turn of the unit that is receiving the ability, and won't be restored when the ability is consumed.
                // * If *false*, the stat modification will temporary, so it will be applied instantly, and will be restored when the ability is consumed.
                // - TurnsApplied: The number of turns that this buff/debuff is going to be applied. If the number of turns is 0, it will be applied *instantly*.
                // These turns that the buff/debuff is applied are counted from the Unit that is receiving the spell, and they are applied at the begin of the unit turn.
                await combatState.PerformAppliedAbilities();

                // The unit's *Speed* says how fast this bar will be filled.
                // The design team doesn't know if the update of the turn should be discrete or not.
                // It is open to the developer to chose.
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