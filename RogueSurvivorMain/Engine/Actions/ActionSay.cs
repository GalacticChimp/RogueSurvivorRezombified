using System;

using djack.RogueSurvivor.Data.Enums;
using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionSay : ActorAction
    {
        Actor m_Target;
        string m_Text;
        Sayflags m_Flags;

        public ActionSay(Actor actor, Actor target, string text, Sayflags flags)
            : base(actor)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
            m_Text = text;
            m_Flags = flags;
        }

        public override bool IsLegal()
        {
            return true;
        }

        public override void Perform()
        {
            m_Actor.DoSay(m_Target, m_Text, m_Flags);
        }
    }
}
