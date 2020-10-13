using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Common;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    /// <summary>
    /// SkeletonAI : used by Skeleton branch.
    /// </summary>
    class SkeletonAI : BaseAI
    {
        #region
        const int IDLE_CHANCE = 80;
        #endregion

        #region Fields
        LOSSensor m_LOSSensor;
        #endregion

        #region BaseAI
        protected override void CreateSensors()
        {
            m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS);
        }

        protected override List<Percept> UpdateSensors(World world)
        {
            return m_LOSSensor.Sense(world, m_Actor);
        }

        protected override ActorAction SelectAction(World world, List<Percept> percepts)
        {
            List<Percept> mapPercepts = FilterSameMap(percepts);

            ////////////////////////////////////////////
            // 1 move in straight line to nearest enemy
            // 2 idle? % chance.
            // 3 wander
            ////////////////////////////////////////////

            // 1 move in straight line to nearest enemy
            Percept nearestEnemy = FilterNearest(FilterEnemies(mapPercepts));
            if (nearestEnemy != null)
            {
                ActorAction bumpAction = BehaviorStupidBumpToward(nearestEnemy.Location.Position, true, false);
                if (bumpAction != null)
                {
                    m_Actor.Activity = Activity.CHASING;
                    m_Actor.TargetActor = nearestEnemy.Percepted as Actor;
                    return bumpAction;
                }
            }

            // 2 idle? % chance.
            if (DiceRoller.RollChance(IDLE_CHANCE))
            {
                m_Actor.Activity = Activity.IDLE;
                return new ActionWait(m_Actor);
            }

            // 3 wander
            m_Actor.Activity = Activity.IDLE;
            return BehaviorWander(null);
        }
        #endregion
    }
}
