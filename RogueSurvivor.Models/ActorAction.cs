using System;

namespace djack.RogueSurvivor.Data
{
    public abstract class ActorAction
    {
        protected readonly RogueGame m_Game;
        protected readonly Actor m_Actor;
        protected string m_FailReason;

        public string FailReason
        {
            get { return m_FailReason; }
            set { m_FailReason = value; }
        }

        protected ActorAction(Actor actor, RogueGame game)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            //if (game == null)
            //    throw new ArgumentNullException("game");

            m_Actor = actor;
            //m_Game = game;
        }

        public abstract bool IsLegal();
        public abstract void Perform();
    }
}
