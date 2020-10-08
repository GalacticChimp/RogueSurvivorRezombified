using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Data.Items;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionThrowGrenade : ActorAction
    {
        Point m_ThrowPos;

        public ActionThrowGrenade(Actor actor, Point throwPos)
            : base(actor)
        {
            m_ThrowPos = throwPos;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorThrowTo(m_ThrowPos, null, out m_FailReason);
        }

        public override void Perform()
        {
            Item grenade = m_Actor.GetEquippedWeapon();

            if (grenade is ItemPrimedExplosive)
                m_Actor.DoThrowGrenadePrimed(m_ThrowPos);
            else
                m_Actor.DoThrowGrenadeUnprimed(m_ThrowPos);
        }
    }
}
