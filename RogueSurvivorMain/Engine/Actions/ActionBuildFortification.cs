using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionBuildFortification : ActorAction
    {
        Point m_BuildPos;
        bool m_IsLarge;

        public ActionBuildFortification(Actor actor, Point buildPos, bool isLarge)
            : base(actor)
        {
            m_BuildPos = buildPos;
            m_IsLarge = isLarge;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorBuildFortification(m_BuildPos, m_IsLarge, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoBuildFortification(m_BuildPos, m_IsLarge);
        }
    }
}
