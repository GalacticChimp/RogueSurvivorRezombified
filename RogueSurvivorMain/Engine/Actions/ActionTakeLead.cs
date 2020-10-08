using System;
using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionTakeLead : ActorAction
    {
        readonly Actor m_Target;

        public ActionTakeLead(Actor actor, Actor target)
            : base(actor)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorTakeLead(m_Target, out m_FailReason);
        }

        public override void Perform()
        {
            // alpha10.1 steal lead vs take lead
            if (m_Target.HasLeader)
                m_Actor.DoStealLead(m_Target);
            else
                m_Actor.DoTakeLead(m_Target);

        }
    }
}
