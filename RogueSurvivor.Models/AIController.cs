using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public abstract class AIController : ActorController
    {
        public abstract ActorOrder Order { get; }
        public abstract ActorDirective Directives { get; set; }

        public abstract void SetOrder(ActorOrder newOrder);
    }
}
