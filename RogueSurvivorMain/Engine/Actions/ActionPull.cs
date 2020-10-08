using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionPull : ActorAction
    {
        readonly MapObject m_Object;
        readonly Direction m_MoveActorDir;
        readonly Point m_MoveActorTo;

        public Direction MoveActorDirection { get { return m_MoveActorDir; } }
        public Point MoveActorTo { get { return m_MoveActorTo; } }

        public ActionPull(Actor actor, MapObject pullObj, Direction moveActorDir)
            : base(actor)
        {
            if (pullObj == null)
                throw new ArgumentNullException("pushObj");

            m_Object = pullObj;
            m_MoveActorDir = moveActorDir;
            m_MoveActorTo = m_Actor.Location.Position + moveActorDir;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanPullObject(m_Object, m_MoveActorTo, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoPull(m_Object, m_MoveActorTo);
        }
    }
}
