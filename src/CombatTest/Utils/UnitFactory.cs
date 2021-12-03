using System;

namespace CombatTest.Utils
{
    public static class UnitFactory
    {
        private static readonly Random Random = new();
        
        public static Unit CreateRandomUnit(Team team)
        {
            return Random.Next(3) switch
            {
                0 => CreateTank(team),
                1 => CreateHealer(team),
                2 => CreateAssassin(team),
                _ => throw new NotSupportedException()
            };
        }

        public static Unit CreateTank(Team team)
        {
            return CreateUnit(team, 100, 25, 20, 15);
        }

        public static Unit CreateHealer(Team team)
        {
            return CreateUnit(team, 50, 10, 0, 10);
        }

        public static Unit CreateAssassin(Team team)
        {
            return CreateUnit(team, 50, 40, 5, 20);
        }
        
        public static Unit CreateDead(Team team)
        {
            return CreateUnit(team, 0, 0, 0, 0);
        }
        
        public static Unit CreateUnit(Team team, int health, int attack, int defence, int speed)
        {
            return new(team, health, attack, defence, speed);
        }
    }
}