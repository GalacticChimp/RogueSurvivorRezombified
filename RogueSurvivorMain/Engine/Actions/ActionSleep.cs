using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionSleep : ActorAction
    {
        public ActionSleep(Actor actor)
            : base(actor)
        {
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorSleep(out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoStartSleeping();
        }
    }
}
