﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;   // Point

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using djack.RogueSurvivor.Common;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    /// <summary>
    /// Soldier AI
    /// </summary>
    class SoldierAI : OrderableAI
    {
        #region Constants
        const int LOS_MEMORY = 10;
        const int FOLLOW_LEADER_MIN_DIST = 1;
        const int FOLLOW_LEADER_MAX_DIST = 2;

        const int EXPLORATION_LOCATIONS = 30;
        const int EXPLORATION_ZONES = 3;

        const int BUILD_SMALL_FORT_CHANCE = 20;
        const int BUILD_LARGE_FORT_CHANCE = 50;
        const int START_FORT_LINE_CHANCE = 1;

        const int DONT_LEAVE_BEHIND_EMOTE_CHANCE = 50;

        static string[] FIGHT_EMOTES = 
        {
            "Damn",
            "Fuck I'm cornered",
            "Die"
        };  
        #endregion

        #region Fields
        LOSSensor m_LOSSensor;
        MemorizedSensor m_MemLOSSensor;

        ExplorationData m_Exploration;
        #endregion
        
        #region BaseAI
        public override void TakeControl(Actor actor)
        {
            base.TakeControl(actor);

            m_Exploration = new ExplorationData(EXPLORATION_LOCATIONS, EXPLORATION_ZONES);
        }

        protected override void CreateSensors()
        {
            m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS);
            m_MemLOSSensor = new MemorizedSensor(m_LOSSensor, LOS_MEMORY);
        }

        protected override List<Percept> UpdateSensors(World world)
        {
            return m_MemLOSSensor.Sense(world, m_Actor);
        }

        protected override ActorAction SelectAction(World world, List<Percept> percepts)
        {
            List<Percept> mapPercepts = FilterSameMap(percepts);

            // alpha10
            // don't run by default.
            m_Actor.IsRunning = false;

            // 0. Equip best item
            ActorAction bestEquip = BehaviorEquipBestItems(false, true);
            if (bestEquip != null)
            {
                return bestEquip;
            }
            // end alpha10

            // 1. Follow order
            #region
            if (this.Order != null)
            {
                ActorAction orderAction = ExecuteOrder(world, this.Order, mapPercepts, m_Exploration);
                if (orderAction == null)
                    SetOrder(null);
                else
                {
                    m_Actor.Activity = Activity.FOLLOWING_ORDER;
                    return orderAction;
                }
            }
            #endregion

            /////////////////////////////////////
            // 0 run away from primed explosives.
            // 1 throw grenades at enemies.
            // alpha10 OBSOLETE 2 equip weapon/armor.
            // 3 shout, fire/hit at nearest enemy.
            // 4 rest if tired
            // alpha10 obsolete and redundant with rule 3! 5 charge enemy.
            // 6 use med.
            // 7 sleep.
            // 8 chase old enemy.
            // 9 build fortification.
            // 10 hang around leader.            
            // 11 (leader) don't leave followers behind.
            // 12 explore.
            // 13 wander.
            ////////////////////////////////////

            // get data.
            List<Percept> allEnemies = FilterEnemies(mapPercepts);
            List<Percept> currentEnemies = FilterCurrent(allEnemies);
            bool checkOurLeader = m_Actor.HasLeader && !DontFollowLeader;
            bool hasCurrentEnemies = (currentEnemies != null);
            bool hasAnyEnemies = (allEnemies != null);

            // exploration.
            m_Exploration.Update(m_Actor.Location);

            // 0 run away from primed explosives.
            #region
            ActorAction runFromExplosives = BehaviorFleeFromExplosives(FilterStacks(mapPercepts));
            if (runFromExplosives != null)
            {
                m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
                return runFromExplosives;
            }
            #endregion

            // 1 throw grenades at enemies.
            #region
            if (hasCurrentEnemies)
            {
                ActorAction throwAction = BehaviorThrowGrenade(m_LOSSensor.FOV, currentEnemies);
                if (throwAction != null)
                {
                    return throwAction;
                }
            }
            #endregion

            // alpha10 obsolete
            // 2 equip weapon/armor
            //#region
            //ActorAction equipWpnAction = BehaviorEquipWeapon(game);
            //if (equipWpnAction != null)
            //{
            //    m_Actor.Activity = Activity.IDLE;
            //    return equipWpnAction;
            //}
            //ActorAction equipArmAction = BehaviorEquipBestBodyArmor(game);
            //if (equipArmAction != null)
            //{
            //    m_Actor.Activity = Activity.IDLE;
            //    return equipArmAction;
            //}
            //#endregion

            // 3 shout, fire/hit at nearest enemy.
            #region
            if (hasCurrentEnemies)
            {
                // shout?
                if (DiceRoller.RollChance(50))
                {
                    List<Percept> friends = FilterNonEnemies(mapPercepts);
                    if (friends != null)
                    {
                        ActorAction shoutAction = BehaviorWarnFriends(friends, FilterNearest(currentEnemies).Percepted as Actor);
                        if (shoutAction != null)
                        {
                            m_Actor.Activity = Activity.IDLE;
                            return shoutAction;
                        }
                    }
                }

                // fire?
                List<Percept> fireTargets = FilterFireTargets(currentEnemies);
                if (fireTargets != null)
                {
                    Percept nearestTarget = FilterNearest(fireTargets);
                    ActorAction fireAction = BehaviorRangedAttack(nearestTarget);
                    if (fireAction != null)
                    {
                        m_Actor.Activity = Activity.FIGHTING;
                        m_Actor.TargetActor = nearestTarget.Percepted as Actor;
                        return fireAction;
                    }
                }

                // fight or flee?
                RouteFinder.SpecialActions allowedChargeActions = RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS; // alpha10
                ActorAction fightOrFlee = BehaviorFightOrFlee(currentEnemies, true, true, ActorCourage.COURAGEOUS, FIGHT_EMOTES, allowedChargeActions);
                if (fightOrFlee != null)
                {
                    return fightOrFlee;
                }
            }
            #endregion

            // 4 rest if tired
            #region
            ActorAction restAction = BehaviorRestIfTired();
            if (restAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return restAction;
            }
            #endregion

            // alpha10 obsolete and redundant with rule 3!
            //// 5 charge enemy
            //#region
            //if (hasAnyEnemies)
            //{
            //    Percept nearestEnemy = FilterNearest(game, allEnemies);
            //    ActorAction chargeAction = BehaviorChargeEnemy(game, nearestEnemy, false, false);
            //    if (chargeAction != null)
            //    {
            //        m_Actor.Activity = Activity.FIGHTING;
            //        m_Actor.TargetActor = nearestEnemy.Percepted as Actor;
            //        return chargeAction;
            //    }
            //}
            //#endregion

            // 6 use medicine
            #region
            ActorAction useMedAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
            if (useMedAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return useMedAction;
            }
            #endregion

            // 7 sleep.
            #region
            if ( !hasAnyEnemies && WouldLikeToSleep(m_Actor) && IsInside(m_Actor) && m_Actor.CanActorSleep(out string reason))
            {               
                // secure sleep?
                ActorAction secureSleepAction = BehaviorSecurePerimeter(m_LOSSensor.FOV);
                if (secureSleepAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return secureSleepAction;
                }

                // sleep.
                ActorAction sleepAction = BehaviorSleep(m_LOSSensor.FOV);
                if (sleepAction != null)
                {
                    if (sleepAction is ActionSleep)
                        m_Actor.Activity = Activity.SLEEPING;
                    return sleepAction;
                }
            }
            #endregion

            // 8 chase old enemy
            #region
            List<Percept> oldEnemies = Filter(allEnemies, (p) => p.Turn != m_Actor.Location.Map.LocalTime.TurnCounter);
            if (oldEnemies != null)
            {
                Percept chasePercept = FilterNearest(oldEnemies);

                // cheat a bit for good chasing behavior.
                if (m_Actor.Location == chasePercept.Location)
                {
                    // memorized location reached, chase now the actor directly (cheat so they appear more intelligent)
                    Actor chasedActor = chasePercept.Percepted as Actor;
                    chasePercept = new Percept(chasedActor, m_Actor.Location.Map.LocalTime.TurnCounter, chasedActor.Location);
                }

                // chase.
                ActorAction chargeAction = BehaviorChargeEnemy(chasePercept, false, false);
                if (chargeAction != null)
                {
                    m_Actor.Activity = Activity.FIGHTING;
                    m_Actor.TargetActor = chasePercept.Percepted as Actor;
                    return chargeAction;
                }
            }
            #endregion

            // 9 build fortification
            #region
            // large fortification.
            if (DiceRoller.RollChance(BUILD_LARGE_FORT_CHANCE))
            {
                ActorAction buildAction = BehaviorBuildLargeFortification(START_FORT_LINE_CHANCE);
                if (buildAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return buildAction;
                }
            }
            // small fortification.
            if (DiceRoller.RollChance(BUILD_SMALL_FORT_CHANCE))
            {
                ActorAction buildAction = BehaviorBuildSmallFortification();
                if (buildAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return buildAction;
                }
            }
            #endregion

            // 10 hang around leader.
            #region
            if (checkOurLeader)
            {
                Point lastKnownLeaderPosition = m_Actor.Leader.Location.Position;
                ActorAction followAction = BehaviorHangAroundActor(m_Actor.Leader, lastKnownLeaderPosition, FOLLOW_LEADER_MIN_DIST, FOLLOW_LEADER_MAX_DIST);
                if (followAction != null)
                {
                    m_Actor.Activity = Activity.FOLLOWING;
                    m_Actor.TargetActor = m_Actor.Leader;
                    return followAction;
                }
            }
            #endregion

            // 11 (leader) don't leave followers behind.
            #region
            if (m_Actor.CountFollowers > 0)
            {
                Actor target;
                ActorAction stickTogether = BehaviorDontLeaveFollowersBehind(4, out target);
                if (stickTogether != null)
                {
                    // emote?
                    if (DiceRoller.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
                    {
                        if (target.IsSleeping)
                            m_Actor.DoEmote(String.Format("patiently waits for {0} to wake up.", target.Name));
                        else
                        {
                            if (m_LOSSensor.FOV.Contains(target.Location.Position))
                                m_Actor.DoEmote(String.Format("{0}! Don't lag behind!", target.Name));
                            else
                                m_Actor.DoEmote(String.Format("Where the hell is {0}?", target.Name));
                        }
                    }

                    // go!
                    m_Actor.Activity = Activity.IDLE;
                    return stickTogether;
                }
            }
            #endregion

            // 12 explore
            #region
            ActorAction exploreAction = BehaviorExplore(m_Exploration);
            if (exploreAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return exploreAction;
            }
            #endregion

            // 13 wander
            #region
            m_Actor.Activity = Activity.IDLE;
            return BehaviorWander(m_Exploration);
            #endregion
        }
        #endregion
    }
}
