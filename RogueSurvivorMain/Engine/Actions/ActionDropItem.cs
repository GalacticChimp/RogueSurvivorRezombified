using System;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionDropItem : ActorAction
    {
        Item m_Item;

        public ActionDropItem(Actor actor, Item it)
            : base(actor)
        {
            if (it == null)
                throw new ArgumentNullException("item");

            m_Item = it;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorDropItem(m_Item, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoDropItem(m_Item);
        }
    }
}
