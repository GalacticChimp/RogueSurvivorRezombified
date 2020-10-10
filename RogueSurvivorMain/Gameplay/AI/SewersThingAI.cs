using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Data.Helpers;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    /// <summary>
    /// Sewers Thing AI, used by Unique Sewers Thing.
    /// Based on a much simplified Zombie AI.
    /// </summary>
    class SewersThingAI : BaseAI
    {
        #region Constants
        const int LOS_MEMORY = 20;
        #endregion

        #region Fields
        MemorizedSensor m_LOSSensor;
        SmellSensor m_LivingSmellSensor;
        SmellSensor m_MasterSmellSensor;
        #endregion

        #region BaseAI
        protected override void CreateSensors()
        {
            m_LOSSensor = new MemorizedSensor(new LOSSensor(LOSSensor.SensingFilter.ACTORS), LOS_MEMORY);
            m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
            m_MasterSmellSensor = new SmellSensor(Odor.UNDEAD_MASTER);
        }

        protected override List<Percept> UpdateSensors(World world)
        {
            List<Percept> list = m_LOSSensor.Sense(world, m_Actor);
            list.AddRange(m_LivingSmellSensor.Sense(world, m_Actor));
            list.AddRange(m_MasterSmellSensor.Sense(world, m_Actor));
            return list;
        }

        protected override ActorAction SelectAction(World world, List<Percept> percepts)
        {
            HashSet<Point> fov = (m_LOSSensor.Sensor as LOSSensor).FOV;
            List<Percept> mapPercepts = FilterSameMap(percepts);

            //////////////////////////////////////////////////////////////
            // 1 move closer to an enemy, nearest & visible enemies first
            // 2 move to highest living scent
            // 3 wander
            //////////////////////////////////////////////////////////////

            // 1 move closer to an enemy, nearest & visible enemies first
            #region
            List<Percept> enemies = FilterEnemies(mapPercepts);
            if (enemies != null)
            {
                // try visible enemies first, the closer the best.
                List<Percept> visibleEnemies = Filter(enemies, (p) => p.Turn == m_Actor.Location.Map.LocalTime.TurnCounter);
                if (visibleEnemies != null)
                {
                    Percept bestEnemyPercept = null;
                    ActorAction bestBumpAction = null;
                    float closest = int.MaxValue;

                    foreach (Percept enemyP in visibleEnemies)
                    {
                        float distance = DistanceHelpers.GridDistance(m_Actor.Location.Position, enemyP.Location.Position);
                        if (distance < closest)
                        {
                            ActorAction bumpAction = BehaviorStupidBumpToward(enemyP.Location.Position, true, true);
                            if (bumpAction != null)
                            {
                                closest = distance;
                                bestEnemyPercept = enemyP;
                                bestBumpAction = bumpAction;
                            }
                        }
                    }

                    if (bestBumpAction != null)
                    {
                        m_Actor.Activity = Activity.CHASING;
                        m_Actor.TargetActor = bestEnemyPercept.Percepted as Actor;
                        return bestBumpAction;
                    }
                }

                // then try rest, the closer the best.
                List<Percept> oldEnemies = Filter(enemies, (p) => p.Turn != m_Actor.Location.Map.LocalTime.TurnCounter);
                if (oldEnemies != null)
                {
                    Percept bestEnemyPercept = null;
                    ActorAction bestBumpAction = null;
                    float closest = int.MaxValue;

                    foreach (Percept enemyP in oldEnemies)
                    {
                        float distance = DistanceHelpers.GridDistance(m_Actor.Location.Position, enemyP.Location.Position);
                        if (distance < closest)
                        {
                            ActorAction bumpAction = BehaviorStupidBumpToward(enemyP.Location.Position, true, true);
                            if (bumpAction != null)
                            {
                                closest = distance;
                                bestEnemyPercept = enemyP;
                                bestBumpAction = bumpAction;
                            }
                        }
                    }

                    if (bestBumpAction != null)
                    {
                        m_Actor.Activity = Activity.CHASING;
                        m_Actor.TargetActor = bestEnemyPercept.Percepted as Actor;
                        return bestBumpAction;
                    }
                }
            }
            #endregion

            // 2 move to highest living scent
            ActorAction trackLivingAction = BehaviorTrackScent(m_LivingSmellSensor.Scents);
            if (trackLivingAction != null)
            {
                m_Actor.Activity = Activity.TRACKING;
                return trackLivingAction;
            }

            // 3 wander
            m_Actor.Activity = Activity.IDLE;
            return BehaviorWander(null);
        }
        #endregion
    }
}
