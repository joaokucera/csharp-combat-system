using System;
using System.Collections.Generic;

namespace CombatTest.Utils
{
    public static class TestHelpers
    {
        private static readonly Random Random = new();

        internal static Dictionary<Team, List<Unit>> WithRandomUnit(this Dictionary<Team, List<Unit>> teams, Team team)
        {
            if (!teams.TryGetValue(team, out var teamUnits))
            {
                teams[team] = teamUnits = new List<Unit>();
            }

            var unit = UnitFactory.CreateRandomUnit(team);
            teamUnits.Add(unit);

            unit.WithRandomAbilities();

            return teams;
        }

        internal static void WithRandomDeadUnit(this Dictionary<Team, List<Unit>> teams, Team team)
        {
            if (!teams.TryGetValue(team, out var teamUnits))
            {
                teams[team] = teamUnits = new List<Unit>();
            }

            var unit = UnitFactory.CreateDead(team);
            teamUnits.Add(unit);
        }

        internal static void WithRandomAbilities(this Unit unit)
        {
            var abilitiesCount = Random.Next(3);
            for (var i = 0; i < abilitiesCount; i++)
            {
                unit.WithRandomAbility();
            }
        }

        internal static void WithRandomAbility(this Unit unit)
        {
            unit.AddOwnAbility(AbilityFactory.CreateRandomAbility());
        }
    }
}