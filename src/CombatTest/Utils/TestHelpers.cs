using System;
using System.Collections.Generic;

namespace CombatTest.Utils
{
    public static class TestHelpers
    {
        private static readonly Random Random = new Random();

        internal static Dictionary<Team, List<Unit>> WithRandomUnit(this Dictionary<Team, List<Unit>> teams, Team team)
        {
            if (!teams.TryGetValue(team, out var teamUnits))
            {
                teams[team] = teamUnits = new List<Unit>();
            }

            var unit = UnitFactory.CreateRandomUnit(team);
            teamUnits.Add(unit);

            // Each unit can have between zero and two abilities.
            // The abilities are "spells" that affect any stat instantly or over time, and that can be permanent or temporary.
            unit.WithRandomAbilities();

            return teams;
        }

        internal static Dictionary<Team, List<Unit>> WithRandomDeadUnit(this Dictionary<Team, List<Unit>> teams, Team team)
        {
            if (!teams.TryGetValue(team, out var teamUnits))
            {
                teams[team] = teamUnits = new List<Unit>();
            }

            var unit = UnitFactory.CreateDead(team);
            teamUnits.Add(unit);

            return teams;
        }

        internal static Unit WithRandomAbilities(this Unit unit)
        {
            // Each unit can have between zero and two abilities.
            // The abilities are "spells" that affect any stat instantly or over time, and that can be permanent or temporary.
            var abilitiesCount = Random.Next(3);
            for (var i = 0; i < abilitiesCount; i++)
            {
                unit.WithRandomAbility();
            }

            return unit;
        }

        internal static Unit WithRandomAbility(this Unit unit)
        {
            unit.AddOwnAbility(AbilityFactory.CreateRandomAbility());
            return unit;
        }
    }
}