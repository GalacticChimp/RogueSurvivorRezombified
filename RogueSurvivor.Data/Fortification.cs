using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public class Fortification : MapObject
    {
        public const int SMALL_BASE_HITPOINTS = DoorWindow.BASE_HITPOINTS / 2;
        public const int LARGE_BASE_HITPOINTS = DoorWindow.BASE_HITPOINTS;

        public Fortification(string name, string imageID, int hitPoints)
            : base(name, imageID, Break.BREAKABLE, Fire.BURNABLE, hitPoints)
        {
        }
    }
}
