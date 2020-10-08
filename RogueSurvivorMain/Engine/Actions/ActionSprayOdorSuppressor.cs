using System;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Data.Items;

namespace djack.RogueSurvivor.Engine.Actions
{
    // alpha10
    class ActionSprayOdorSuppressor : ActorAction
    {
        readonly ItemSprayScent m_Spray;
        readonly Actor m_SprayOn;

        public ActionSprayOdorSuppressor(Actor actor, ItemSprayScent spray, Actor sprayOn)
            : base(actor)
        {
            if (sprayOn == null)
                throw new ArgumentNullException("target");

            m_Spray = spray;
            m_SprayOn = sprayOn;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorSprayOdorSuppressor(m_Spray, m_SprayOn, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoSprayOdorSuppressor(m_Spray, m_SprayOn);
        }
    }
}
