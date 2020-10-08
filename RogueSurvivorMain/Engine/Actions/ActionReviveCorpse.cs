using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionReviveCorpse : ActorAction
    {
        readonly Corpse m_Target;

        public ActionReviveCorpse(Actor actor, Corpse target)
            : base(actor)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorReviveCorpse(m_Target, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoReviveCorpse(m_Target);
        }
    }
}
