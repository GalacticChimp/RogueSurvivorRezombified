using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionMeleeAttack : ActorAction
    {
        readonly Actor m_Target;

        public ActionMeleeAttack(Actor actor, Actor target)
            : base(actor)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
        }

        public override bool IsLegal()
        {
            return true;    // handled before in rules
        }

        public override void Perform()
        {
            m_Actor.DoMeleeAttack(m_Target);
        }
    }
}
