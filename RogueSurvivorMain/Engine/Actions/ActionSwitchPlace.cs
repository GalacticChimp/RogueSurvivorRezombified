using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionSwitchPlace : ActorAction
    {
        readonly Actor m_Target;

        public ActionSwitchPlace(Actor actor, Actor target)
            : base(actor)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorSwitchPlaceWith(m_Target, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoSwitchPlace(m_Target);
        }
    }
}
