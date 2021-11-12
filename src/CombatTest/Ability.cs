namespace CombatTest
{
    public struct Ability
    {
        public TargetTeam TargetTeam { get; }
        public TargetNum TargetNum { get; }
        public int Amount { get; }
        public Stat StatAffected { get; }
        public bool IsPermanent { get; }
        public int TurnsApplied { get; private set; }

        public Ability(TargetTeam targetTeam, TargetNum targetNum, int amount, Stat statAffected, bool isPermanent,
            int turnsApplied)
        {
            TargetTeam = targetTeam;
            TargetNum = targetNum;
            Amount = amount;
            StatAffected = statAffected;
            IsPermanent = isPermanent;
            
            // If the number of turns is 0, it will be applied *instantly*.
            // *** so, I assume if TurnsApplied == 1, the "spell" will be applied instantly and in the next turn ***
            TurnsApplied = turnsApplied;
        }
        
        public void DecrementTurnsAppliedValue()
        {
            TurnsApplied--;
        }
    }
}