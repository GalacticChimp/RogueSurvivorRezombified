﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using djack.RogueSurvivor.Data.Enums;
using djack.RogueSurvivor.Data.Helpers;

namespace djack.RogueSurvivor.Gameplay.AI
{
    /// <summary>
    /// alpa10 this unused for now
    /// </summary>
    [Serializable]
    class FeralDogAI : BaseAI
    {
        #region Constants
        const int FOLLOW_NPCLEADER_MAXDIST = 1;
        const int FOLLOW_PLAYERLEADER_MAXDIST = 1;
        const int RUN_TO_TARGET_DISTANCE = 3;  // dogs run to their target when close enough

        static string[] FIGHT_EMOTES = 
        {
            "waf",            // flee
            "waf!?",         // trapped
            "GRRRRR WAF WAF"  // fight
        };   
        #endregion

        #region Fields
        LOSSensor m_LOSSensor;
        SmellSensor m_LivingSmellSensor;
        #endregion

        #region BaseAI
        protected override void CreateSensors()
        {
            m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES);
            m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
        }

        protected override List<Percept> UpdateSensors(World world)
        {
            List<Percept> list = m_LOSSensor.Sense(world, m_Actor);
            list.AddRange(m_LivingSmellSensor.Sense(world, m_Actor));
            return list;
        }

        protected override ActorAction SelectAction(World world, List<Percept> percepts)
        {
            HashSet<Point> fov = m_LOSSensor.FOV;
            List<Percept> mapPercepts = FilterSameMap(percepts);

            //////////////////////////////////////////////////////////////
            // 1 defend our leader.
            // 2 attack or flee enemies.
            // 3 go eat food on floor if almost hungry
            // 4 go eat corpses if hungry
            // 5 rest or sleep
            // 6 follow leader
            // 7 wander
            /////////////////////////////////////////////////////////////

            // 1 defend our leader
            if (m_Actor.HasLeader)
            {
                Actor target = m_Actor.Leader.TargetActor;
                if(target != null && target.Location.Map == m_Actor.Location.Map)
                {
                    // emote: bark
                    m_Actor.DoSay(target, "GRRRRRR WAF WAF", Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);
                    // charge.
                    ActorAction chargeEnemy = BehaviorStupidBumpToward(target.Location.Position, true, false);
                    if (chargeEnemy != null)
                    {
                        RunToIfCloseTo(target.Location.Position, RUN_TO_TARGET_DISTANCE);
                        m_Actor.Activity = Activity.FIGHTING;
                        m_Actor.TargetActor = target;
                        return chargeEnemy;
                    }
                }
            }

            List<Percept> enemies = FilterEnemies(mapPercepts);
            bool isLeaderVisible = m_Actor.HasLeader && m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
            bool isLeaderFighting = m_Actor.HasLeader && IsAdjacentToEnemy(m_Actor.Leader);

            // 2 attack or flee enemies.
            #region
            if (enemies != null)
            {
                RouteFinder.SpecialActions allowedChargeActions = RouteFinder.SpecialActions.JUMP; // alpha10
                ActorAction ff = BehaviorFightOrFlee(enemies, isLeaderVisible, isLeaderFighting, Directives.Courage, FIGHT_EMOTES, allowedChargeActions);
                if (ff != null)
                {
                    // run to (or away if fleeing) if close.
                    if (m_Actor.TargetActor != null)
                        RunToIfCloseTo(m_Actor.TargetActor.Location.Position, RUN_TO_TARGET_DISTANCE);
                    return ff;
                }
            }
            #endregion

            // 3 go eat food on floor if almost hungry
            #region
            if (m_Actor.IsAlmostHungry())
            {
                List<Percept> itemsStack = FilterStacks(mapPercepts);
                if (itemsStack != null)
                {
                    ActorAction eatFood = BehaviorGoEatFoodOnGround(itemsStack);
                    if (eatFood != null)
                    {
                        RunIfPossible();
                        m_Actor.Activity = Activity.IDLE;
                        return eatFood;
                    }
                }
            }
            #endregion

            // 4 go eat corpses if hungry
            #region
            if (m_Actor.IsActorHungry())
            {
                List<Percept> corpses = FilterCorpses(mapPercepts);
                if (corpses != null)
                {
                    ActorAction eatCorpses = BehaviorGoEatCorpse(corpses);
                    if (eatCorpses != null)
                    {
                        RunIfPossible();
                        m_Actor.Activity = Activity.IDLE;
                        return eatCorpses;
                    }
                }
            }
            #endregion

            // 5 rest or sleep
            #region
            if (m_Actor.IsActorTired())
            {
                m_Actor.Activity = Activity.IDLE;
                return new ActionWait(m_Actor);
            }
            if (m_Actor.IsActorSleepy())
            {
                m_Actor.Activity = Activity.SLEEPING;
                return new ActionSleep(m_Actor);
            }
            #endregion

            // 6 follow leader
            #region
            if (m_Actor.HasLeader)
            {
                Point lastKnownLeaderPosition = m_Actor.Leader.Location.Position;
                int maxDist = m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
                ActorAction followAction = BehaviorFollowActor(m_Actor.Leader, lastKnownLeaderPosition, isLeaderVisible, maxDist);
                if (followAction != null)
                {
                    m_Actor.IsRunning = false;
                    m_Actor.Activity = Activity.FOLLOWING;
                    m_Actor.TargetActor = m_Actor.Leader;
                    return followAction;
                }
            }
            #endregion

            // 7 wander
            m_Actor.Activity = Activity.IDLE;
            return BehaviorWander(null);
        }

        private ActorAction BehaviorFightOrFlee(List<Percept> enemies, bool isLeaderVisible, bool isLeaderFighting, ActorCourage courage, string[] fIGHT_EMOTES)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Dogs specifics
        protected void RunToIfCloseTo(Point pos, int closeDistance)
        {
            if (DistanceHelpers.GridDistance(m_Actor.Location.Position, pos) <= closeDistance)
            {
                m_Actor.IsRunning = m_Actor.CanActorRun(out _);
            }
            else
            {
                m_Actor.IsRunning = false;
            }
        }
        #endregion
    }
}
