using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionPush : ActorAction
    {
        readonly MapObject m_Object;
        readonly Direction m_Direction;
        readonly Point m_To;

        public Direction Direction { get { return m_Direction; } }
        public Point To { get { return m_To; } }

        public ActionPush(Actor actor, MapObject pushObj, Direction pushDir)
            : base(actor)
        {
            if (pushObj == null)
                throw new ArgumentNullException("pushObj");

            m_Object = pushObj;
            m_Direction = pushDir;
            m_To = pushObj.Location.Position + pushDir;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorPush(m_Object, out m_FailReason) && m_Object.CanPushObjectTo(m_To, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoPush(m_Object, m_To);
        }

    }
}
