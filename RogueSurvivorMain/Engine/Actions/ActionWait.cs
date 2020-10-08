using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionWait : ActorAction
    {
        public ActionWait(Actor actor)
            : base(actor)
        {
        }

        public override bool IsLegal()
        {
            return true;
        }

        public override void Perform()
        {
            m_Actor.DoWait();
        }
    }
}
