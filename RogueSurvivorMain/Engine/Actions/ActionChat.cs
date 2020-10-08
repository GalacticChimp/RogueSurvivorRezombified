using System;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionChat : ActorAction
    {
        readonly Actor m_Target;

        public Actor Target
        {
            get { return m_Target; }
        }

        public ActionChat(Actor actor, Actor target)
            : base(actor)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
        }

        public override bool IsLegal()
        {
            return true;
        }

        public override void Perform()
        {
            m_Actor.DoChat(m_Target);
        }
    }
}
