using System;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionSwitchPowerGenerator : ActorAction
    {
        PowerGenerator m_PowGen;

        public ActionSwitchPowerGenerator(Actor actor, PowerGenerator powGen)
            : base(actor)
        {
            if (powGen == null)
                throw new ArgumentNullException("powGen");

            m_PowGen = powGen;
        }

        public override bool IsLegal()
        {
            return m_Actor.IsSwitchableFor(m_PowGen, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoSwitchPowerGenerator(m_PowGen);
        }
    }
}
