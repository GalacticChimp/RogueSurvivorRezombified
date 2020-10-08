using System;

namespace djack.RogueSurvivor.Data.Enums
{
    [Flags]
    public enum Sayflags
    {
        NONE = 0,
        /// <summary>
        /// If told to the player and visible will highlight pause the game.
        /// </summary>
        IS_IMPORTANT = (1 << 0),

        /// <summary>
        /// Does not cost action points (emote).
        /// </summary>
        IS_FREE_ACTION = (1 << 1),

        // alpha10
        /// <summary>
        /// A warning or menace, should be highlighted.
        /// </summary>
        IS_DANGER = (1 << 2)
    }
}
