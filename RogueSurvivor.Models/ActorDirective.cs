using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public enum ActorCourage
    {
        COWARD,
        CAUTIOUS,
        COURAGEOUS
    }

    [Serializable]
    public class ActorDirective
    {
        public bool CanTakeItems { get; set; }
        public bool CanFireWeapons { get; set; }
        public bool CanThrowGrenades { get; set; }
        public bool CanSleep { get; set; }
        public bool CanTrade { get; set; }
        public ActorCourage Courage { get; set; }

        public ActorDirective()
        {
            Reset();
        }
        
        public void Reset()
        {
            this.CanTakeItems = true;
            this.CanFireWeapons = true;
            this.CanThrowGrenades = true;
            this.CanSleep = true;
            this.CanTrade = true;
            this.Courage = ActorCourage.CAUTIOUS;
        }

        public static string CourageString(ActorCourage c)
        {
            switch (c)
            {
                case ActorCourage.CAUTIOUS:
                    return "Cautious";
                case ActorCourage.COURAGEOUS:
                    return "Courageous";
                case ActorCourage.COWARD:
                    return "Coward";
                default:
                    throw new ArgumentOutOfRangeException("unhandled courage");
            }
        }
    }
}
