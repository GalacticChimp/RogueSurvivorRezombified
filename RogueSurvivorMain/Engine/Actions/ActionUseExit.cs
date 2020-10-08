using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionUseExit : ActorAction
    {
        Point m_ExitPoint;

        public ActionUseExit(Actor actor, Point exitPoint)
            : base(actor)
        {
            m_ExitPoint = exitPoint;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorUseExit(m_ExitPoint, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoUseExit(m_ExitPoint);
        }
    }
}
