using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CombatTest.Utils;
using NUnit.Framework;

namespace CombatTest
{
    [TestFixture]
    public class CombatControllerTest
    {
        [Test]
        public async Task Unit_PerformHealAbility_ShouldNotHealDeadUnit()
        {
            var deadUnit = UnitFactory.CreateDead(Team.Blue);
            var expectedDeadUnitHealth = deadUnit.CurrentHealth;
            var expectedDeadUnitIsAlive = deadUnit.IsAlive;

            var healAbility = AbilityFactory.CreateHeal();
            await deadUnit.PerformAbility(healAbility);

            Assert.AreEqual(expectedDeadUnitHealth, deadUnit.CurrentHealth);
            Assert.AreEqual(expectedDeadUnitIsAlive, deadUnit.IsAlive);
        }

        [Test]
        public async Task Unit_PerformHealAbility_ShouldClapMaxHealth([Values(0, 1, 2)] int turnsApplied)
        {
            const int unitHealth = 5;
            const int abilityAmount = 5;

            var unit = UnitFactory.CreateUnit(Team.Blue, unitHealth, 0, 0, 0);

            var anyAbility = AbilityFactory.CreateAbility(TargetTeam.Ally, TargetNum.Self, abilityAmount, Stat.Health,
                false, turnsApplied);
            await unit.PerformAbility(anyAbility);

            while (unit.AppliedAbilities.Count > 0)
            {
                await unit.PerformAppliedAbilities();
            }

            Assert.AreEqual(unitHealth, unit.CurrentHealth);
        }

        [Test]
        public async Task Unit_PerformShieldAbility_ShouldNotDieIfAttackEqualsToItsHealth()
        {
            var unit = UnitFactory.CreateRandomUnit(Team.Blue);
            var oldUnitDefense = unit.Defence;
            var oldUnitCurrentHealth = unit.CurrentHealth;

            var shieldAbility = AbilityFactory.CreateShield();
            await unit.PerformAbility(shieldAbility);

            var newUnitDefense = unit.Defence;
            var newUnitCurrentHealth = unit.CurrentHealth;

            Assert.AreNotEqual(oldUnitDefense, newUnitDefense);
            Assert.AreEqual(oldUnitCurrentHealth, newUnitCurrentHealth);

            var enemyAttack = oldUnitCurrentHealth + oldUnitDefense;
            unit.PerformPhysicalDamage(enemyAttack);

            Assert.IsTrue(unit.IsAlive);
        }

        [Test]
        public async Task Unit_PerformPoisonAbility_ShouldDieOvertimeNoMatterItsHealth([Values(1, 50, 100)] int health)
        {
            var unit = UnitFactory.CreateUnit(Team.Blue, health, 0, 0, 0);

            var poisonAbility = AbilityFactory.CreatePoison();
            do
            {
                var oldUnitHealth = unit.CurrentHealth;
                await unit.PerformAbility(poisonAbility);
                var newUnitHealth = unit.CurrentHealth;

                Assert.AreNotEqual(oldUnitHealth, newUnitHealth);
            } while (unit.IsAlive);

            Assert.IsFalse(unit.IsAlive);
        }

        [Test]
        public async Task Unit_PerformAnyAbilityForTurns_ShouldChangeUnitStatWhileTurnsAreApplied([Values(0, 1, 2)] int turnsApplied)
        {
            const int unitHealth = 50;
            const int abilityAmount = 5;

            var unit = UnitFactory.CreateUnit(Team.Blue, unitHealth, 0, 0, 0);
            unit.PerformPhysicalDamage(45);
            var unitHealthAfterDamage = unit.CurrentHealth;

            var anyAbility = AbilityFactory.CreateAbility(TargetTeam.Ally, TargetNum.Self, abilityAmount, Stat.Health, false, turnsApplied);
            await unit.PerformAbility(anyAbility);

            while (unit.AppliedAbilities.Count > 0)
            {
                await unit.PerformAppliedAbilities();
            }

            var expectedHealth = unitHealthAfterDamage + abilityAmount * (turnsApplied + 1);
            Assert.AreEqual(expectedHealth, unit.CurrentHealth);
        }

        [Test]
        public async Task Unit_PerformAnyAbilityPermanently_ShouldChangeUnitStatForever([Values(0, 1, 2)] int turnsApplied)
        {
            const int unitHealth = 50;
            const int abilityAmount = 5;

            var unit = UnitFactory.CreateUnit(Team.Blue, unitHealth, 0, 0, 0);
            unit.PerformPhysicalDamage(45);

            var anyAbility = AbilityFactory.CreateAbility(TargetTeam.Ally, TargetNum.Self, abilityAmount, Stat.Health,
                true, turnsApplied);
            await unit.PerformAbility(anyAbility);

            var expectedAppliedAbilities = unit.AppliedAbilities.Count;
            var i = 0;

            while (unit.AppliedAbilities.Count > 0)
            {
                await unit.PerformAppliedAbilities();

                i++;
                if (i + 1 > turnsApplied)
                {
                    break;
                }
            }

            Assert.AreEqual(expectedAppliedAbilities, unit.AppliedAbilities.Count);
        }

        [Test]
        public async Task Combat_NoTeamsDefined_NoPerformed([Values(1, 5, 10)] int turnsToRun, [Values(5, 10, 20)] int barIncrementsPerTurn)
        {
            var teams = new Dictionary<Team, List<Unit>>();
            var combatState = new CombatState(teams);

            ICombatController combatController = new CombatController();
            await combatController.Run(combatState, turnsToRun, barIncrementsPerTurn);

            Assert.IsTrue(combatState.CurrentCombatState == CombatStateEnum.NotPerformed,
                "CurrentCombatState should be NotPerformed");
            Assert.AreEqual(combatState.TeamsWithOriginalUnits, combatState.TeamsWithAliveUnits);
        }

        [Test]
        public async Task Combat_OneTeamsDefined_NotPerformed([Values(1, 5, 10)] int turnsToRun, [Values(5, 10, 20)] int barIncrementsPerTurn)
        {
            var teams = new Dictionary<Team, List<Unit>>();
            teams
                .WithRandomUnit(Team.Blue);

            var combatState = new CombatState(teams);

            ICombatController combatController = new CombatController();
            await combatController.Run(combatState, turnsToRun, barIncrementsPerTurn);

            Assert.IsTrue(combatState.CurrentCombatState == CombatStateEnum.NotPerformed,
                "CurrentCombatState should be NotPerformed");
            Assert.IsFalse(combatState.TeamsWithOriginalUnits.Count == 2);
        }

        [Test]
        public async Task Combat_TwoTeamsDefinedButOneWithDeadUnits_CombatEndedWithWinningTeam([Values(1, 5, 10)] int turnsToRun, [Values(5, 10, 20)] int barIncrementsPerTurn)
        {
            var teams = new Dictionary<Team, List<Unit>>();
            teams
                .WithRandomUnit(Team.Blue)
                .WithRandomDeadUnit(Team.Red);

            var combatState = new CombatState(teams);

            ICombatController combatController = new CombatController();
            await combatController.Run(combatState, turnsToRun, barIncrementsPerTurn);

            Assert.IsTrue(combatState.CurrentCombatState == CombatStateEnum.EndedWithWinningTeam,
                "CurrentCombatState should be EndedWithWinningTeam");
            Assert.IsTrue(combatState.TeamsWithAliveUnits[Team.Blue].Count > 0);
            Assert.IsTrue(combatState.TeamsWithAliveUnits[Team.Red].Count == 0);
        }

        [Test]
        public async Task Combat_TwoTeamsDefined_CombatEnded([Values(25, 50, 100)] int turnsToRun, [Values(5, 10, 20)] int barIncrementsPerTurn)
        {
            var teams = new Dictionary<Team, List<Unit>>();
            teams
                .WithRandomUnit(Team.Blue)
                .WithRandomUnit(Team.Blue)
                .WithRandomUnit(Team.Blue)
                .WithRandomUnit(Team.Red)
                .WithRandomUnit(Team.Red)
                .WithRandomUnit(Team.Red);

            var combatState = new CombatState(teams);

            ICombatController combatController = new CombatController();
            await combatController.Run(combatState, turnsToRun, barIncrementsPerTurn);

            Assert.IsTrue(combatState.CurrentCombatState is CombatStateEnum.EndedWithWinningTeam or CombatStateEnum.EndedWithoutWinningTeam,
                "CurrentCombatState should be any Ended");
            Assert.IsFalse(combatState.TeamsWithAliveUnits.Any(t => t.Value.Any(u => !u.IsAlive)));
        }
    }
}