using System;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionRepairFortification : ActorAction
    {
        Fortification m_Fort;

        public ActionRepairFortification(Actor actor, Fortification fort)
            : base(actor)
        {
            if (fort == null)
                throw new ArgumentNullException("fort");

            m_Fort = fort;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorRepairFortification(m_Fort, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoRepairFortification(m_Fort);
        }
    }
}
