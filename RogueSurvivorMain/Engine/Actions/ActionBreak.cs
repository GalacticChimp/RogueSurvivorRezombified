using System;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionBreak : ActorAction
    {
        MapObject m_Obj;

        // alpha10.1 needed by RogueGame to ask player if he really wants to break
        public MapObject MapObject { get { return m_Obj; } }

        public ActionBreak(Actor actor, MapObject obj)
            : base(actor)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            m_Obj = obj;
        }

        public override bool IsLegal()
        {
            return m_Actor.IsBreakableFor(m_Obj, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoBreak(m_Obj);
        }
    }
}

