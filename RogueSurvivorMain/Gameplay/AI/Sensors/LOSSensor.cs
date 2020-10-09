using System;
using System.Collections.Generic;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;

namespace djack.RogueSurvivor.Gameplay.AI.Sensors
{
    [Serializable]
    class LOSSensor : Sensor
    {
        #region Types
        [Flags]
        public enum SensingFilter
        {
            /// <summary>
            /// Actors.
            /// </summary>
            ACTORS = (1<<0),

            /// <summary>
            /// Stacks of items (Inventory).
            /// </summary>
            ITEMS = (1<<1),

            /// <summary>
            /// Stacks of corpses (List of Corpse>
            /// </summary>
            CORPSES = (1<<2)
        }
        #endregion

        #region Fields
        HashSet<Point> m_FOV;
        SensingFilter m_Filters;
        #endregion

        #region Properties
        public HashSet<Point> FOV
        {
            get { return m_FOV; }
        }

        public SensingFilter Filters
        {
            get { return m_Filters; }
            set { m_Filters = value; }
        }
        #endregion

        #region Init
        public LOSSensor(SensingFilter filters)
        {
            m_Filters = filters;
        }
        #endregion

        public override List<Percept> Sense(World world, Actor actor)
        {
            // compute FOV
            m_FOV = LOS.ComputeFOVFor(actor, actor.Location.Map.LocalTime, world.Weather);
            int maxRange = actor.ActorFOV(actor.Location.Map.LocalTime, world.Weather);

            // compute percepts.
            List<Percept> list = new List<Percept>();

            if ((m_Filters & SensingFilter.ACTORS) != 0)
            {
                // roughly estimate time for two sensing methods.
                int searchFovMethodTime = maxRange * maxRange;
                int searchActorsListMethodTime = actor.Location.Map.CountActors;

                // choose method which seems less costly in time.
                if (searchFovMethodTime < searchActorsListMethodTime)
                {
                    // FOV check : bad when few actors, good when many actors.
                    foreach (Point p in m_FOV)
                    {
                        Actor other = actor.Location.Map.GetActorAt(p.X, p.Y);
                        if (other != null && other != actor)
                            list.Add(new Percept(other, actor.Location.Map.LocalTime.TurnCounter, other.Location));
                    }
                }
                else
                {
                    // Actors list check : good when few actors, bad when many actors.
                    foreach (Actor other in actor.Location.Map.Actors)
                    {
                        if (other == actor)
                            continue;

                        if (LOS.LOSDistance(actor.Location.Position, other.Location.Position) > maxRange)
                            continue;

                        if (m_FOV.Contains(other.Location.Position))
                            list.Add(new Percept(other, actor.Location.Map.LocalTime.TurnCounter, other.Location));
                    }
                }
            }

            if ((m_Filters & SensingFilter.ITEMS) != 0)
            {
                foreach (Point p in m_FOV)
                {
                    Inventory inv = actor.Location.Map.GetItemsAt(p);
                    if (inv == null || inv.IsEmpty)
                        continue;
                    list.Add(new Percept(inv, actor.Location.Map.LocalTime.TurnCounter, new Location(actor.Location.Map, p)));
                }
            }

            if ((m_Filters & SensingFilter.CORPSES) != 0)
            {
                foreach (Point p in m_FOV)
                {
                    List<Corpse> corpses = actor.Location.Map.GetCorpsesAt(p.X, p.Y);
                    if (corpses != null)
                        list.Add(new Percept(corpses, actor.Location.Map.LocalTime.TurnCounter, new Location(actor.Location.Map, p)));
                }
            }

            // done.
            return list;
        }
    }
}
