The design team is asking to start working on the early combat prototype. The initial specs are the following ones:

# Gameplay Specs
The combat system has two teams. Each team has a certain number of units (between 1 - 5) that they are going to fight in a turn-based fashion.

When a team has zero units alive, the other team wins.

The teams will fight automatically.

## Unit Specs
### Stats

Each Unit has the following stats that comes from a configuration:
- **MaxHealth**: The health points that it needs to be subtracted to kill this unit. By now there isn't any resurrection feature, so it dies permanently.
- **Attack**: The amount of damage that a physical hit does.
- **Defence**: The amount of damage that is reduced when receiving a physical attack.
- **Speed**: How often the unit plays a turn (the turn order will be explained later)

### Abilities

Each unit can have between zero and two abilities. The abilities are "spells" that affect any stat instantly or over time, and that can be permanent or temporary.

The abilities have the following configuration:
- **TargetTeam**: To which team this ability can be cast. It has two options: *Ally* team or *Enemy* team
- **TargetNum**: To whom this ability can be cast. It has three options: *Self* (the more restrictive one. This ability just can be cast to the unit that is casting it), *Single* (just one target. `Self` is a subgroup on this target), and *All* (all the units that are in the TargetTeam)
- **Amount**: The amount of stat that is modified.
- **StatAffected**: The stat affected by the ability. 
- **IsPermanent**: 
    * If *true*, the stat modification will be applied in every turn of the unit that is receiving the ability, and won't be restored when the ability is consumed.
    * If *false*, the stat modification will temporary, so it will be applied instantly, and will be restored when the ability is consumed.
- TurnsApplied: The number of turns that this buff/debuff is going to be applied. If the number of turns is 0, it will be applied *instantly*. These turns that the buff/debuff is applied are counted from the Unit that is receiving the spell, and they are applied at the begin of the unit turn.

Note: All the **instant** abilities must be **Permanent**.

There aren't animations and effects yet, but the abilities will be applied after an animation is triggered. The class AnimationTrigger should be use to simulate it.

## Turn Order
The turn order will be given by the speed of each unit. Similar to FFVII, each unit will have a bar that has to be filled to be able to attack. In our case, this bar will contain 100 units.
The unit's *Speed* says how fast this bar will be filled. The design team doesn't know if the update of the turn should be discrete or not. It is open to the developer to chose.

When more than one unit happens to have the turn in the same moment, the turn order will be given following the next algorithm.  
First, a team is chosen randomly to go first and the other second.  
When we have the team order, the turn is given to each team, so a random unit of that team picks the turn.  
For example: If we have the units A1, A2, B1, B2 from teams A and B. Randomly, we chose team A. The between A1 and A2, A2 is randomly picked. Then we pick randomly from team B, so it gets picked B1. Then A1 and the B2. So the order would be `A2-B1-A1-B2`  
If we have the units A1, B1, B2, and the team that goes first is A, the order would be `A1-B1-B2` or `A1-B2-B1`

The design team has doubts about this heuristic to solve the problem of choosing which unit should attack first when they have a tie. They might change the heuristic in the future.

## Ability and Target selection

The design team is asking the unit's ability&target selection to be random:

* First the unit will select randomly if it does a physical attack or if it cast a random ability.
* Then, the target selection will be random too. 

Note: The possible target of a physical attack is a Enemy-Single unit.