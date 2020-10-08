using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionMoveStep : ActorAction
    {
        Location m_NewLocation;

        public ActionMoveStep(Actor actor, Direction direction)
            : base(actor)
        {
            m_NewLocation = actor.Location + direction;
        }

        public ActionMoveStep(Actor actor, Point to)
            : base(actor)
        {
            m_NewLocation = new Location(actor.Location.Map, to);
        }

        public override bool IsLegal()
        {
            return m_Actor.IsWalkableFor(m_NewLocation, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoMoveActor(m_NewLocation);
        }
    }
}
