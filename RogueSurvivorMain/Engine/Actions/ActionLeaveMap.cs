using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionLeaveMap : ActorAction
    {
        Point m_ExitPoint;

        public Point ExitPoint
        {
            get { return m_ExitPoint; }
        }

        public ActionLeaveMap(Actor actor, Point exitPoint)
            : base(actor)
        {
            m_ExitPoint = exitPoint;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorLeaveMap(out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoLeaveMap(m_ExitPoint, true);
        }
    }
}
