using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;


namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionRangedAttack : ActorAction
    {
        Actor m_Target;
        List<Point> m_LoF = new List<Point>();
        FireMode m_Mode;

        public ActionRangedAttack(Actor actor, Actor target, FireMode mode)
            : base(actor)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
            m_Mode = mode;
        }

        public ActionRangedAttack(Actor actor, Actor target)
            : this(actor, target, FireMode.DEFAULT)
        {
        }

        public override bool IsLegal()
        {
            m_LoF.Clear();
            return m_Actor.CanActorFireAt(m_Target, m_LoF, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoRangedAttack(m_Target, m_LoF, m_Mode);
        }
    }
}
