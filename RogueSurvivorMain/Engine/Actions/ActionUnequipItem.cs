using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionUnequipItem : ActorAction
    {
        Item m_Item;

        public ActionUnequipItem(Actor actor, Item it)
            : base(actor)
        {
            if (it == null)
                throw new ArgumentNullException("item");

            m_Item = it;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorUnequipItem(m_Item, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoUnequipItem(m_Item);
        }
    }
}
