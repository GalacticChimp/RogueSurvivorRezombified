using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemEntertainment : Item
    {
        // alpha10 boring items moved out of Actor
        List<Actor> m_BoringFor = null;

        public ItemEntertainmentModel EntertainmentModel { get { return this.Model as ItemEntertainmentModel; } }

        public ItemEntertainment(ItemModel model)
            : base(model)
        {
            if (!(model is ItemEntertainmentModel))
                throw new ArgumentException("model is not a EntertainmentModel");
        }

        // alpha10 boring items moved out of Actor
        public void AddBoringFor(Actor a)
        {
            if (m_BoringFor == null) m_BoringFor = new List<Actor>(1);
            if (m_BoringFor.Contains(a)) return;
            m_BoringFor.Add(a);
        }

        public bool IsBoringFor(Actor a)
        {
            if (m_BoringFor == null) return false;
            return m_BoringFor.Contains(a);
        }

        // alpha10
        public override void OptimizeBeforeSaving()
        {
            base.OptimizeBeforeSaving();

            // clean up dead actors refs
            // side effect: revived actors will forget about boring items
            if (m_BoringFor != null)
            {
                for (int i = 0; i < m_BoringFor.Count; )
                {
                    if (m_BoringFor[i].IsDead)
                        m_BoringFor.RemoveAt(i);
                    else
                        i++;
                }
                if (m_BoringFor.Count == 0)
                    m_BoringFor = null;
            }
        }
    }
}
