using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionTrade : ActorAction
    {
        readonly Actor m_Target;

        public ActionTrade(Actor actor, Actor target)
            : base(actor)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorInitiateTradeWith(m_Target, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoTrade(m_Target);
        }
    }
}
