using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CombatTest.Utils;

namespace CombatTest
{
    public class Unit : IComparable<Unit>
    {
        private const int BarValueToAttack = 100;

        private int _currentBarValue;
        private readonly Random _random = new Random();
        private readonly IAnimationTrigger _animationTrigger = new AnimationTrigger();

        public Team Team { get; }
        public int MaxHealth { get; }
        public int Attack { get; private set; }
        public int Defence { get; private set; }
        public int Speed { get; private set; }

        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;
        public bool CanAttack => _currentBarValue >= BarValueToAttack;
        public List<Ability> OwnAbilities { get; } = new List<Ability>();
        public List<Ability> AppliedAbilities { get; private set; } = new List<Ability>();

        public Unit(Team team, int health = 0, int attack = 0, int defence = 0, int speed = 0)
        {
            Team = team;
            MaxHealth = health;
            Attack = attack;
            Defence = defence;
            Speed = speed;

            CurrentHealth = MaxHealth;
        }

        public void AddOwnAbility(Ability ability)
        {
            OwnAbilities.Add(ability);
        }

        public bool TryGetRandomAbility(out Ability ability)
        {
            if (OwnAbilities.Count > 0)
            {
                ability = OwnAbilities.GetRandomElement(_random);
                return true;
            }

            ability = default;
            return false;
        }

        public void IncrementBarValue()
        {
            if (_currentBarValue > BarValueToAttack)
            {
                _currentBarValue -= BarValueToAttack;
            }

            _currentBarValue += Speed;
        }

        public UnitAction GetRandomUnitAction()
        {
            if (OwnAbilities.Count == 0)
            {
                // If the unit has no abilities, it will always perform a PhysicalAttack
                return UnitAction.PhysicalAttack;
            }

            return _random.Next(1) switch
            {
                0 => UnitAction.PhysicalAttack,
                1 => UnitAction.CastRandomAbility,
                _ => throw new NotSupportedException()
            };
        }

        public void PerformPhysicalDamage(int enemyAttack)
        {
            CurrentHealth -= enemyAttack - Defence;
        }

        public async Task PerformAbility(Ability ability, bool canAddAbility = true)
        {
            // The class AnimationTrigger should be use to simulate it.
            await _animationTrigger.PlayAnimation();
            
            // Instant ability action - it will only count down if the ability is applied for more than 1 round
            switch (ability.StatAffected)
            {
                case Stat.Health:
                    PerformHealthAbility(ability.Amount);
                    break;
                case Stat.Attack:
                    PerformAttackAbility(ability.Amount);
                    break;
                case Stat.Defense:
                    PerformDefenseAbility(ability.Amount);
                    break;
                case Stat.Speed:
                    PerformSpeedAbility(ability.Amount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // If the ability is permanent or it is applied for more than 1 turn, add it to the applied abilities list
            if (canAddAbility && (ability.IsPermanent || ability.TurnsApplied > 0))
            {
                AppliedAbilities.Add(ability);
            }
        }

        public async Task PerformAppliedAbilities()
        {
            var abilitiesToKeep = new List<Ability>();
            
            foreach (var ability in AppliedAbilities)
            {
                if (ability.IsPermanent || ability.TurnsApplied > 0)
                {
                    await PerformAbility(ability, false);
                    // Decrement the amount of turns applied (does not interfere with the permanent flag)
                    ability.DecrementTurnsAppliedValue();
                }

                if (ability.IsPermanent || ability.TurnsApplied > 0)
                {
                    abilitiesToKeep.Add(ability);
                }
            }
            
            AppliedAbilities = abilitiesToKeep;
        }

        private void PerformHealthAbility(int healthAmount)
        {
            if (!IsAlive)
            {
                return;
            }

            CurrentHealth += healthAmount;

            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }

        private void PerformAttackAbility(int attackAmount)
        {
            Attack += attackAmount;
        }

        private void PerformDefenseAbility(int defenseAmount)
        {
            Defence += defenseAmount;
        }

        private void PerformSpeedAbility(int speedAmount)
        {
            Speed += speedAmount;
        }

        public int CompareTo(Unit other)
        {
            return Speed.CompareTo(other.Speed);
        }
    }
}