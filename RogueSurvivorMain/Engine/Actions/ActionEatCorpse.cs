using System;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionEatCorpse : ActorAction
    {
        readonly Corpse m_Target;

        public ActionEatCorpse(Actor actor, Corpse target)
            : base(actor)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
        }

        public override bool IsLegal()
        {
            return m_Actor.CanActorEatCorpse(m_Target, out m_FailReason);
        }

        public override void Perform()
        {
            m_Actor.DoEatCorpse(m_Target);
        }
    }
}
