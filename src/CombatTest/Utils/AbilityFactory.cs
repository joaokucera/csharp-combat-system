using System;

namespace CombatTest.Utils
{
    public static class AbilityFactory
    {
        private static readonly Random Random = new();

        public static Ability CreateRandomAbility()
        {
            return Random.Next(3) switch
            {
                0 => CreateShield(),
                1 => CreateHeal(),
                2 => CreatePoison(),
                _ => throw new NotSupportedException()
            };
        }

        public static Ability CreateShield()
        {
            return CreateAbility(TargetTeam.Ally, TargetNum.Self, 10, Stat.Defense, false, 1);
        }

        public static Ability CreateHeal()
        {
            return CreateAbility(TargetTeam.Ally, TargetNum.Single, 20, Stat.Health, false, 0);
        }

        public static Ability CreatePoison()
        {
            return CreateAbility(TargetTeam.Enemy, TargetNum.All, -5, Stat.Health, true, 0);
        }

        public static Ability CreateAbility(TargetTeam targetTeam, TargetNum targetNum, int amount, Stat stat,
            bool isPermanent, int turnsApplied)
        {
            return new(targetTeam, targetNum, amount, stat, isPermanent, turnsApplied);
        }
    }
}