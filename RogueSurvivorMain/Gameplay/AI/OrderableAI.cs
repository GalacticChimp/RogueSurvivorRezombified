using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Data.Enums;
using djack.RogueSurvivor.Data.Helpers;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay.AI.Sensors;

namespace djack.RogueSurvivor.Gameplay.AI
{
    /// <summary>
    /// Base class for AIs that can follow orders and is notified of raid events.
    /// </summary>
    [Serializable]
    abstract class OrderableAI : BaseAI
    {
        #region Fields
        protected Percept m_LastEnemySaw;
        protected Percept m_LastItemsSaw;
        protected Percept m_LastSoldierSaw;
        protected Percept m_LastRaidHeard;

        protected bool m_ReachedPatrolPoint;
        protected int m_ReportStage;
        #endregion

        #region Properties
        public bool DontFollowLeader { get; set; }
        #endregion

        #region Orders
        public override void SetOrder(ActorOrder newOrder)
        {
            base.SetOrder(newOrder);

            // reset order states.
            m_ReachedPatrolPoint = false;
            m_ReportStage = 0;
        }

        protected ActorAction ExecuteOrder(World world, ActorOrder order, List<Percept> percepts, ExplorationData exploration)
        {
            // cancel if leader is dead!
            if (m_Actor.Leader == null || m_Actor.Leader.IsDead)
                return null;

            // execute task.
            switch (order.Task)
            {
                case ActorTasks.BARRICADE_ONE:
                    return ExecuteBarricading(order.Location, false);
                case ActorTasks.BARRICADE_MAX:
                    return ExecuteBarricading(order.Location, true);
                case ActorTasks.BUILD_SMALL_FORTIFICATION:
                    return ExecuteBuildFortification(order.Location, false);
                case ActorTasks.BUILD_LARGE_FORTIFICATION:
                    return ExecuteBuildFortification(order.Location, true);
                case ActorTasks.DROP_ALL_ITEMS:
                    return ExecuteDropAllItems();
                case ActorTasks.GUARD:
                    return ExecuteGuard(order.Location, percepts);
                case ActorTasks.PATROL:
                    return ExecutePatrol(order.Location, percepts, exploration);
                case ActorTasks.REPORT_EVENTS:
                    return ExecuteReport(world, percepts);
                case ActorTasks.SLEEP_NOW:
                    return ExecuteSleepNow(percepts);
                case ActorTasks.FOLLOW_TOGGLE:
                    return ExecuteToggleFollow();
                case ActorTasks.WHERE_ARE_YOU:
                    return ExecuteReportPosition();

                default:
                    throw new NotImplementedException("order task not handled");
            }
        }

        #region Barricading
        ActorAction ExecuteBarricading(Location location, bool toTheMax)
        {
            /////////////////////
            // 1. Check validity.
            // 2. Perform.
            /////////////////////

            // 1. Check validity.
            if (m_Actor.Location.Map != location.Map)
                return null;
            DoorWindow door = location.Map.GetMapObjectAt(location.Position) as DoorWindow;
            if (door == null)
                return null;
            if (!m_Actor.CanActorBarricadeDoor(door,out string reason))
                return null;

            // 2. Perform.
            // 2.1 If adjacent, barricade.
            // 2.2 Move closer.

            // 2.1 If adjacent, barricade.
            if (m_Actor.Location.Position.IsAdjacent(location.Position))
            {
                ActorAction barricadeAction = new ActionBarricadeDoor(m_Actor, door);
                if (barricadeAction.IsLegal())
                {
                    if (!toTheMax)
                    {
                        SetOrder(null);
                    }
                    return barricadeAction;
                }
                else
                    return null;
            }

            // 2.2 Move closer.
            ActorAction moveAction = BehaviorIntelligentBumpToward(location.Position, false, false);
            if (moveAction != null)
            {
                RunIfPossible();
                return moveAction;
            }
            else
                return null;
        }
        #endregion

        #region Building fortification
        ActorAction ExecuteBuildFortification(Location location, bool isLarge)
        {
            /////////////////////
            // 1. Check validity.
            // 2. Perform.
            /////////////////////

            // 1. Check validity.
            if (m_Actor.Location.Map != location.Map)
                return null;
            if (!m_Actor.CanActorBuildFortification(location.Position, isLarge, out string reason))
                return null;

            // 2. Perform.
            // 2.1 If adjacent, build.
            // 2.2 Move closer.

            // 2.1 If adjacent, build.
            if (m_Actor.Location.Position.IsAdjacent(location.Position))
            {
                ActorAction buildAction = new ActionBuildFortification(m_Actor, location.Position, isLarge);
                if (buildAction.IsLegal())
                {
                    SetOrder(null);
                    return buildAction;
                }
                else
                    return null;
            }

            // 2.2 Move closer.
            ActorAction moveAction = BehaviorIntelligentBumpToward(location.Position, false, false);
            if (moveAction != null)
            {
                RunIfPossible();
                return moveAction;
            }
            else
                return null;
        }
        #endregion

        #region Guarding
        ActorAction ExecuteGuard(Location location, List<Percept> percepts)
        {
            ////////////////////////////////
            // Interrupt guard:
            // 1. See enemy => raise alarm.
            ////////////////////////////////

            // 1. See enemy => raise alarm.
            List<Percept> enemies = FilterEnemies(percepts);
            if (enemies != null)
            {
                // ALAAAARRMM!!
                SetOrder(null);
                Actor nearestEnemy = FilterNearest(enemies).Percepted as Actor;
                if (nearestEnemy == null)
                    throw new InvalidOperationException("null nearest enemy");
                return new ActionShout(m_Actor, String.Format("{0} sighted!!", nearestEnemy.Name));
            }

            //////////////////////////////
            // Guard duty actions:
            // 1. Mimick leader cell phone usage.
            // 2. Move to guard position.
            // 3. Eat if hungry.
            // 4. Heal if need to.
            // 5. Wait.
            /////////////////////////////

            // 1. Mimick leader cell phone usage.
            ActorAction phoneAction = BehaviorEquipCellPhone();
            if (phoneAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return phoneAction;
            }
            phoneAction = BehaviorUnequipCellPhoneIfLeaderHasNot();
            if (phoneAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return phoneAction;
            }

            // 2. Move to guard position.
            if (m_Actor.Location.Position != location.Position)
            {
                ActorAction bumpAction = BehaviorIntelligentBumpToward(location.Position, false, false);
                if (bumpAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return bumpAction;
                }
            }

            // 3. Eat if hungry.
            if (m_Actor.IsActorHungry())
            {
                ActorAction eatAction = BehaviorEat();
                if (eatAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return eatAction;
                }
            }

            // 4. Heal if need to.
            ActorAction useMedAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
            if (useMedAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return useMedAction;
            }

            // 5. Wait.
            m_Actor.Activity = Activity.IDLE;
            return new ActionWait(m_Actor);
        }
        #endregion

        #region Patrolling
        ActorAction ExecutePatrol(Location location, List<Percept> percepts, ExplorationData exploration)
        {
            ////////////////////////////////
            // Interrupt patrol
            // 1. See enemy => raise alarm.
            ////////////////////////////////

            // 1. See enemy => raise alarm.
            List<Percept> enemies = FilterEnemies(percepts);
            if (enemies != null)
            {
                // ALAAAARRMM!!
                SetOrder(null);
                Actor nearestEnemy = FilterNearest(enemies).Percepted as Actor;
                if (nearestEnemy == null)
                    throw new InvalidOperationException("null nearest enemy");
                return new ActionShout(m_Actor, String.Format("{0} sighted!!", nearestEnemy.Name));
            }

            //////////////////////////////
            // Patrol duty actions:
            // Check if patrol position reached.
            // 1. Mimick leader cell phone usage.
            // 2. Move to patrol position.
            // 3. Eat if hungry.
            // 4. Heal if need to.
            // 5. Wander in patrol zone.
            /////////////////////////////

            // Check patrol position reached.
            if (!m_ReachedPatrolPoint)
            {
                m_ReachedPatrolPoint = m_Actor.Location.Position == location.Position;
            }

            // 1. Mimick leader cell phone usage.
            ActorAction phoneAction = BehaviorEquipCellPhone();
            if (phoneAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return phoneAction;
            }
            phoneAction = BehaviorUnequipCellPhoneIfLeaderHasNot();
            if (phoneAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return phoneAction;
            }

            // 2. Move to patrol position.
            if (!m_ReachedPatrolPoint)
            {
                ActorAction bumpAction = BehaviorIntelligentBumpToward(location.Position, false, false);
                if (bumpAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return bumpAction;
                }
            }

            // 3. Eat if hungry.
            if (m_Actor.IsActorHungry())
            {
                ActorAction eatAction = BehaviorEat();
                if (eatAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return eatAction;
                }
            }

            // 4. Heal if need to.
            ActorAction useMedAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
            if (useMedAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return useMedAction;
            }

            // 5. Wander in patrol zones.
            List<Zone> patrolZones = location.Map.GetZonesAt(Order.Location.Position.X, Order.Location.Position.Y);
            return BehaviorWander((loc) =>
            {
                List<Zone> zonesHere = loc.Map.GetZonesAt(loc.Position.X, loc.Position.Y);
                if (zonesHere == null)
                    return false;
                foreach (Zone zHere in zonesHere)
                    foreach (Zone zPatrol in patrolZones)
                        if (zHere == zPatrol)
                            return true;
                return false;
            }, exploration
            );
        }
        #endregion

        #region Dropping all items
        ActorAction ExecuteDropAllItems()
        {
            // if no more items, done.
            if (m_Actor.Inventory.IsEmpty)
                return null;

            // alpha10.1 bugfix followers drop all was looping
            // use drop item behaviour on the first item it can.
            for (int i = 0; i < m_Actor.Inventory.CountItems; i++)
            {
                ActorAction dropAction = BehaviorDropItem(m_Actor.Inventory[i]);
                if (dropAction != null)
                    return dropAction;
            }

            // we still have at least one item but cannot drop it for some reason,
            // consider the order done.
            return null;

            /* bugged code, did mark item to be dropped as taboo (causing loop bug) and could illegaly drop an item
            // drop next item.
            Item dropIt = m_Actor.Inventory[0];
            if (dropIt.IsEquipped)
                return new ActionUnequipItem(m_Actor, game, dropIt);
            else
                return new ActionDropItem(m_Actor, game, dropIt);
            */
        }
        #endregion

        #region Reporting
        ActorAction ExecuteReport(World world, List<Percept> percepts)
        {
            ////////////////////////////////
            // Interrupt report
            // 1. See enemy => raise alarm.
            ////////////////////////////////

            // 1. See enemy => raise alarm.
            List<Percept> enemies = FilterEnemies(percepts);
            if (enemies != null)
            {
                // ALAAAARRMM!!
                SetOrder(null);
                Actor nearestEnemy = FilterNearest(enemies).Percepted as Actor;
                if (nearestEnemy == null)
                    throw new InvalidOperationException("null nearest enemy");
                return new ActionShout(m_Actor, String.Format("{0} sighted!!", nearestEnemy.Name));
            }

            //////////////////////
            // Continue reporting.
            //////////////////////
            ActorAction reportAction = null;
            bool isReportDone = false;

            switch (m_ReportStage)
            {
                case 0: // events.
                    if (m_LastRaidHeard != null)
                        reportAction = BehaviorTellFriendAboutPercept(world, m_LastRaidHeard);
                    else
                        reportAction = new ActionSay(m_Actor, m_Actor.Leader, "No raids heard.", Sayflags.NONE);

                    ++m_ReportStage;
                    break;

                case 1: // enemies.
                    if (m_LastEnemySaw != null)
                        reportAction = BehaviorTellFriendAboutPercept(world, m_LastEnemySaw);
                    else
                        reportAction = new ActionSay(m_Actor, m_Actor.Leader, "No enemies sighted.", Sayflags.NONE);

                    ++m_ReportStage;
                    break;

                case 2: // items.
                    if (m_LastItemsSaw != null)
                        reportAction = BehaviorTellFriendAboutPercept(world, m_LastItemsSaw);
                    else
                        reportAction = new ActionSay(m_Actor, m_Actor.Leader, "No items sighted.", Sayflags.NONE);

                    ++m_ReportStage;
                    break;

                case 3: // soldiers.
                    if (m_LastSoldierSaw != null)
                        reportAction = BehaviorTellFriendAboutPercept(world, m_LastSoldierSaw);
                    else
                        reportAction = new ActionSay(m_Actor, m_Actor.Leader, "No soldiers sighted.", Sayflags.NONE);

                    ++m_ReportStage;
                    break;

                case 4: // end of report.
                    isReportDone = true;
                    reportAction = new ActionSay(m_Actor, m_Actor.Leader, "That's it.", Sayflags.NONE);
                    break;

            }

            if (isReportDone)
                SetOrder(null);
            if (reportAction != null)
                return reportAction;
            else
                return new ActionSay(m_Actor, m_Actor.Leader, "Let me think...", Sayflags.NONE);
        }
        #endregion

        #region Sleeping now
        ActorAction ExecuteSleepNow(List<Percept> percepts)
        {
            // interrupt if seeing an enemy.
            List<Percept> enemies = FilterEnemies(percepts);
            if (enemies != null)
            {
                // ALAAAARRMM!!
                SetOrder(null);
                Actor nearestEnemy = FilterNearest(enemies).Percepted as Actor;
                if (nearestEnemy == null)
                    throw new InvalidOperationException("null nearest enemy");
                return new ActionShout(m_Actor, String.Format("{0} sighted!!", nearestEnemy.Name));
            }

            // try to sleep.
            string reason;
            if (m_Actor.CanActorSleep(out reason))
            {
                // start sleeping only one even turns so the player has at least one turn to cancel the order...
                if (m_Actor.Location.Map.LocalTime.TurnCounter % 2 == 0)
                    return new ActionSleep(m_Actor);
                else
                    return new ActionWait(m_Actor);
            }
            else
            {
                SetOrder(null);
                m_Actor.DoEmote(String.Format("I can't sleep now : {0}.", reason));
                return new ActionWait(m_Actor);
            }
        }
        #endregion

        #region Toggle following
        ActorAction ExecuteToggleFollow()
        {
            // consider it done.
            SetOrder(null);

            // toggle.
            DontFollowLeader = !DontFollowLeader;

            // emote.
            m_Actor.DoEmote(DontFollowLeader ? "OK I'll do my stuff, see you soon!" : "I'm ready!");
            return new ActionWait(m_Actor);
        }
        #endregion

        #region Where are you?
        ActorAction ExecuteReportPosition()
        {
            // consider it done. 
            SetOrder(null);

            // do it.
            string reportTxt = String.Format("I'm in {0} at {1},{2}.", m_Actor.Location.Map.Name, m_Actor.Location.Position.X, m_Actor.Location.Position.Y);
            return new ActionSay(m_Actor, m_Actor.Leader, reportTxt, Sayflags.NONE);
        }
        #endregion

        #endregion

        #region Raid notification.
        public void OnRaid(RaidType raid, Location location, int turn)
        {
            if (m_Actor.IsSleeping)
                return;

            string raidDesc;
            switch (raid)
            {
                case RaidType.ARMY_SUPLLIES: raidDesc = "a chopper hovering"; break;
                case RaidType.BIKERS: raidDesc = "motorcycles coming"; break;
                case RaidType.BLACKOPS: raidDesc = "a chopper hovering"; break;
                case RaidType.GANGSTA: raidDesc = "cars coming"; break;
                case RaidType.NATGUARD: raidDesc = "the army coming"; break;
                case RaidType.SURVIVORS: raidDesc = "honking coming"; break;
                default:
                    throw new ArgumentOutOfRangeException(String.Format("unhandled raidtype {0}", raid.ToString()));
            }

            m_LastRaidHeard = new Percept(raidDesc, turn, location);
        }
        #endregion
    }
}
