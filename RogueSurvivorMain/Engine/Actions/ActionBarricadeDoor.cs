using System;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionBarricadeDoor : ActorAction
    {
        DoorWindow m_Door;

        public ActionBarricadeDoor(Actor actor, DoorWindow door)
            : base(actor)
        {
            if (door == null)
                throw new ArgumentNullException("door");

            m_Door = door;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorBarricadeDoor(m_Door, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoBarricadeDoor(m_Door);
        }
    }
}
