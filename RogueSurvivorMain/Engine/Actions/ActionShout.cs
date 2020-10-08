using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionShout : ActorAction
    {
        string m_Text;

        public ActionShout(Actor actor)
            : this(actor, null)
        {
        }

        public ActionShout(Actor actor, string text)
            : base(actor)
        {
            m_Text = text;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorShout(out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoShout(m_Text);
        }
    }
}
