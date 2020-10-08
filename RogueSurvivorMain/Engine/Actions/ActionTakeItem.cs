using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionTakeItem : ActorAction
    {
        Point m_Position;
        Item m_Item;

        public ActionTakeItem(Actor actor, Point position, Item it)
            : base(actor)
        {
            if (it == null)
                throw new ArgumentNullException("item");

            m_Position = position;
            m_Item = it;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorGetItem(m_Item, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoTakeItem(m_Position, m_Item);
        }
    }
}