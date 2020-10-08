using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public abstract class ActorController
    {
        protected Actor m_Actor;

        public Actor ControlledActor { get { return m_Actor; } } // alpha10

        public virtual void TakeControl(Actor actor)
        {
            m_Actor = actor;
        }

        public virtual void LeaveControl()
        {
            m_Actor = null;
        }

        public abstract ActorAction GetAction();
    }
}
