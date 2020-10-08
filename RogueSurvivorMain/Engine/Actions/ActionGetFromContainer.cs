using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionGetFromContainer : ActorAction
    {
        Point m_Position;

        /// <summary>
        /// Gets item that will be taken : top item from container position.
        /// </summary>
        public Item Item
        {
            get
            {
                Map map = m_Actor.Location.Map;
                return map.GetItemsAt(m_Position).TopItem;
            }
        }

        public ActionGetFromContainer(Actor actor, Point position)
            : base(actor)
        {
            m_Position = position;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorGetItemFromContainer(m_Position, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoTakeFromContainer(m_Position);
        }
    }
}
