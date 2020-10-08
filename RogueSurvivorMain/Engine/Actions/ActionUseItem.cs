using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionUseItem : ActorAction
    {
        Item m_Item;

        public ActionUseItem(Actor actor, Item it)
            : base(actor)
        {
            if (it == null)
                throw new ArgumentNullException("item");

            m_Item = it;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorUseItem(m_Item, out m_FailReason); 
        }

        public override void Perform()
        {
            m_Actor.DoUseItem(m_Item);
        }
    }
}
