using System;
using System.Collections.Generic;

namespace CombatTest.Utils
{
    public static class TeamExtensions
    {
        private static readonly Random Random = new Random();
        
        // Auxiliary lists used to check and sort teams
        private static readonly HashSet<int> UnitSpeeds = new HashSet<int>();
        private static readonly Dictionary<Team, List<Unit>> UnitsByTeam = new Dictionary<Team, List<Unit>>();

        public static IEnumerable<Unit> TryGetAllAllyUnits(this Dictionary<Team, List<Unit>> teams, Team team)
        {
            // Could just return the code below, however, I'd like to prevent misconfiguration
            // return _teams[team];

            foreach (var entry in teams)
            {
                // Preventing misconfiguration null ref
                if (entry.Key == team && entry.Value?.Count > 0)
                {
                    foreach (var unit in entry.Value)
                    {
                        yield return unit;
                    }
                }
            }
        }

        public static bool TryGetRandomAllyUnit(this Dictionary<Team, List<Unit>> teams, Team team, out Unit unit)
        {
            // Preventing misconfiguration null ref
            if (teams.ContainsKey(team) && teams[team]?.Count > 0)
            {
                unit = teams[team].GetRandomElement(Random);
                return true;
            }

            unit = null;
            return false;
        }

        public static IEnumerable<Unit> TryGetAllEnemyUnits(this Dictionary<Team, List<Unit>> teams, Team team)
        {
            foreach (var entry in teams)
            {
                // Preventing misconfiguration null ref
                if (entry.Key != team && entry.Value?.Count > 0)
                {
                    foreach (var unit in entry.Value)
                    {
                        yield return unit;
                    }
                }
            }
        }

        public static bool TryGetRandomEnemyUnit(this Dictionary<Team, List<Unit>> teams, Team team, out Unit unit)
        {
            foreach (var entry in teams)
            {
                // Preventing misconfiguration null ref
                if (entry.Key != team && entry.Value?.Count > 0)
                {
                    unit = entry.Value.GetRandomElement(Random);
                    return true;
                }
            }

            unit = null;
            return false;
        }

        public static bool HasUnitsWithSameSpeed(this List<Unit> units)
        {
            UnitSpeeds.Clear();

            foreach (var unit in units)
            {
                UnitSpeeds.Add(unit.Speed);
            }

            return units.Count != UnitSpeeds.Count;
        }

        public static List<Unit> SortRandomlyAttackingUnits(this List<Unit> units, IList<Team> teams)
        {
            UnitsByTeam.Clear();
            
            foreach (var unit in units)
            {
                if (!UnitsByTeam.TryGetValue(unit.Team, out var teamUnits))
                {
                    UnitsByTeam[unit.Team] = teamUnits = new List<Unit>();
                }
                
                teamUnits.Add(unit);
            }
            
            var selectedTeam = teams.GetRandomElement(Random);
            var pickFromSelectedTeamFlag = true;
            var sortedAttackingUnits = new List<Unit>();

            void AddUnitToSortedListAndRemoveFromDictionary(Unit unit)
            {
                sortedAttackingUnits.Add(unit);
                UnitsByTeam[unit.Team].Remove(unit);
            }

            do
            {
                if (pickFromSelectedTeamFlag)
                {
                    if (TryGetRandomAllyUnit(UnitsByTeam, selectedTeam, out var selectedUnit))
                    {
                        AddUnitToSortedListAndRemoveFromDictionary(selectedUnit);
                    }
                }
                else
                {
                    if (TryGetRandomEnemyUnit(UnitsByTeam, selectedTeam, out var selectedUnit))
                    {
                        AddUnitToSortedListAndRemoveFromDictionary(selectedUnit);
                    }
                }
                
                pickFromSelectedTeamFlag = !pickFromSelectedTeamFlag;
                
            } while (sortedAttackingUnits.Count < units.Count);

            return sortedAttackingUnits;
        }
    }
}