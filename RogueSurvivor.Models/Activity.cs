using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public enum Activity
    {
        IDLE,
        CHASING,
        FIGHTING,
        TRACKING,
        FLEEING,
        FOLLOWING,
        SLEEPING,
        FOLLOWING_ORDER,
        /// <summary>
        /// Fleeing from a primed explosive.
        /// </summary>
        FLEEING_FROM_EXPLOSIVE
    }
}
