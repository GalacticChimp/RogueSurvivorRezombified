using System;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionBashDoor : ActorAction
    {
        DoorWindow m_Door;

        public ActionBashDoor(Actor actor, DoorWindow door)
            : base(actor)
        {
            if (door == null)
                throw new ArgumentNullException("door");

            m_Door = door;
        }

        public override bool IsLegal()
        {
            return m_Actor.IsBashableFor(m_Door, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoBreak(m_Door);
        }
    }
}
