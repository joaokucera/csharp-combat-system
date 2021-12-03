using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CombatTest.Utils;

namespace CombatTest
{
    public class CombatState
    {
        private readonly List<Team> _teams = new();

        public CombatStateEnum CurrentCombatState { get; set; }
        public Dictionary<Team, List<Unit>> TeamsWithOriginalUnits { get; }
        public Dictionary<Team, List<Unit>> TeamsWithAliveUnits { get; }

        public CombatState(Dictionary<Team, List<Unit>> teams)
        {
            TeamsWithOriginalUnits = teams;
            TeamsWithAliveUnits = teams;

            foreach (var entry in TeamsWithOriginalUnits)
            {
                _teams.Add(entry.Key);
            }
        }

        public void IncrementUnitBarValues()
        {
            foreach (var team in TeamsWithAliveUnits)
            {
                foreach (var unit in team.Value)
                {
                    unit.IncrementBarValue();
                }
            }
        }

        public IEnumerable<Unit> GetOrderedUnitsToAttack()
        {
            var attackingUnits = new List<Unit>();

            foreach (var entry in TeamsWithAliveUnits)
            {
                foreach (var unit in entry.Value)
                {
                    if (unit.IsAlive && unit.CanAttack)
                    {
                        attackingUnits.Add(unit);
                    }
                }
            }

            if (attackingUnits.HasUnitsWithSameSpeed())
            {
                attackingUnits = attackingUnits.SortRandomlyAttackingUnits(_teams);
            }
            else
            {
                attackingUnits.Sort();
            }

            foreach (var unit in attackingUnits)
            {
                yield return unit;
            }
        }

        public async Task PerformActionByUnit(Unit unit)
        {
            if (unit.CanAttack)
            {
                return;
            }

            var unitAction = unit.GetRandomUnitAction();

            switch (unitAction)
            {
                case UnitAction.PhysicalAttack:
                    PerformPhysicalDamageByUnit(unit);
                    break;
                case UnitAction.CastRandomAbility:
                    await PerformAbilityByUnit(unit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateAliveUnits()
        {
            foreach (var teams in TeamsWithAliveUnits)
            {
                for (var i = teams.Value.Count - 1; i >= 0; i--)
                {
                    var unit = teams.Value[i];

                    if (!unit.IsAlive)
                    {
                        teams.Value.Remove(unit);
                    }
                }
            }
        }

        public bool HasAnyTeamAllUnitsDead()
        {
            foreach (var teams in TeamsWithAliveUnits)
            {
                if (teams.Value.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task PerformAppliedAbilities()
        {
            foreach (var teams in TeamsWithAliveUnits)
            {
                foreach (var unit in teams.Value)
                {
                    await unit.PerformAppliedAbilities();
                }
            }
        }

        private void PerformPhysicalDamageByUnit(Unit unit)
        {
            if (TeamsWithAliveUnits.TryGetRandomEnemyUnit(unit.Team, out var enemyToPerformPhysicalAttack))
            {
                enemyToPerformPhysicalAttack.PerformPhysicalDamage(unit.Attack);
            }
        }

        private async Task PerformAbilityByUnit(Unit unit)
        {
            if (unit.TryGetRandomAbility(out var ability))
            {
                switch (ability.TargetTeam)
                {
                    case TargetTeam.Ally:
                        switch (ability.TargetNum)
                        {
                            case TargetNum.Self:
                                await unit.PerformAbility(ability);
                                break;
                            case TargetNum.Single:
                                if (TeamsWithAliveUnits.TryGetRandomAllyUnit(unit.Team, out var allyToPerformAbility))
                                {
                                    await allyToPerformAbility.PerformAbility(ability);
                                }

                                break;
                            case TargetNum.All:
                                foreach (var ally in TeamsWithAliveUnits.TryGetAllAllyUnits(unit.Team))
                                {
                                    await ally.PerformAbility(ability);
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case TargetTeam.Enemy:
                        switch (ability.TargetNum)
                        {
                            case TargetNum.Self:
                                await unit.PerformAbility(ability);
                                break;
                            case TargetNum.Single:
                                if (TeamsWithAliveUnits.TryGetRandomEnemyUnit(unit.Team, out var enemyToPerformAbility))
                                {
                                    await enemyToPerformAbility.PerformAbility(ability);
                                }

                                break;
                            case TargetNum.All:
                                foreach (var enemyUnit in TeamsWithAliveUnits.TryGetAllEnemyUnits(unit.Team))
                                {
                                    await enemyUnit.PerformAbility(ability);
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                PerformPhysicalDamageByUnit(unit);
            }
        }
    }
}