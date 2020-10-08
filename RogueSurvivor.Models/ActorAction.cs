using System;

namespace djack.RogueSurvivor.Data
{
    public abstract class ActorAction
    {
        protected readonly Actor m_Actor;
        protected string m_FailReason;

        public string FailReason
        {
            get { return m_FailReason; }
            set { m_FailReason = value; }
        }

        protected ActorAction(Actor actor)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            m_Actor = actor;
        }

        public abstract bool IsLegal();
        public abstract void Perform();
    }
}
