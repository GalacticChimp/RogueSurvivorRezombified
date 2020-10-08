using System;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionCloseDoor: ActorAction
    {
        DoorWindow m_Door;

        public ActionCloseDoor(Actor actor, DoorWindow door)
            : base(actor)
        {
            if (door == null)
                throw new ArgumentNullException("door");

            m_Door = door;
        }

        public override bool IsLegal()
        {
            return m_Actor.IsClosableFor(m_Door, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoCloseDoor(m_Door);
        }
    }
}
