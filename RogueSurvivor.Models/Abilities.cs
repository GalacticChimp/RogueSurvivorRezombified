using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public class Abilities
    {
        [NonSerialized]
        public static readonly Abilities NONE = new Abilities();

        public bool IsUndead { get; set; }

        public bool IsUndeadMaster { get; set; }

        /// <summary>
        /// Killing living actors will turn them into zombies; actor also regen its HP from damage inflicted on livings.
        /// </summary>
        public bool CanZombifyKilled { get; set; }

        /// <summary>
        /// Can loose stamina points and get tired.
        /// </summary>
        public bool CanTire { get; set; }

        /// <summary>
        /// Loose food points as time passes and needs to eat.
        /// </summary>
        public bool HasToEat { get; set; }

        /// <summary>
        /// Loose sleep points as time passes and needs to sleep.
        /// </summary>
        public bool HasToSleep { get; set; }

        /// <summary>
        /// Sanity gauge.
        /// </summary>
        public bool HasSanity { get; set; }

        /// <summary>
        /// Can run (move at x2 speed at the cost of STA).
        /// </summary>
        public bool CanRun { get; set; }

        public bool CanTalk { get; set; }

        public bool CanUseMapObjects { get; set; }

        /// <summary>
        /// Can bash doors & windows and prefer to break through stuff rather than going around.
        /// </summary>
        public bool CanBashDoors { get; set; }

        /// <summary>
        /// Can attack or bump into objects to break them.
        /// </summary>
        public bool CanBreakObjects { get; set; }

        public bool CanJump { get; set; }

        /// <summary>
        /// Small actors are blocked only by closed doors.
        /// </summary>
        public bool IsSmall { get; set; }

        /// <summary>
        /// Has an inventory (carry items around).
        /// </summary>
        public bool HasInventory { get; set; }

        public bool CanUseItems { get; set; }

        public bool CanTrade { get; set; }

        public bool CanBarricade { get; set; }

        /// <summary>
        /// Can push/pull movable map objects.
        /// </summary>
        public bool CanPush { get; set; }

        /// <summary>
        /// Has a chance to stumble when jumping.
        /// </summary>
        public bool CanJumpStumble { get; set; }

        /// <summary>
        /// A law enforcer can attack murderers that are not law enforces with impunity.
        /// AIs will make use of this.
        /// </summary>
        public bool IsLawEnforcer { get; set; }

        /// <summary>
        /// Can do intelligent assesments like avoiding traps.
        /// </summary>
        public bool IsIntelligent { get; set; }

        /// <summary>
        /// Is slowly rotting and needs to eat flesh.
        /// </summary>
        public bool IsRotting { get; set; }

        // alpha10
        public bool CanDisarm { get; set; } = true;

        /// <summary>
        /// AI flag : tell some AIs t can use Exits with flag Exit.IsAnAIExit.
        /// </summary>
        public bool AI_CanUseAIExits { get; set; }

        /// <summary>
        /// AI flag : ranged weapons are not interesting items.
        /// </summary>
        public bool AI_NotInterestedInRangedWeapons { get; set; }

        // alpha10 obsolete, was unused in alpha9
        ///// <summary>
        ///// AI flag : tell some AIs to use the assault barricades behavior.
        ///// </summary>
        //public bool ZombieAI_AssaultBreakables { get; set; }

        /// <summary>
        /// AI flag : tell some AIs to use the explore behavior.
        /// </summary>
        public bool ZombieAI_Explore { get; set; }
    }
}
