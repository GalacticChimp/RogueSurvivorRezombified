namespace djack.RogueSurvivor.Data
{
    public abstract class FactionDB
    {
        public abstract Faction this[int id]
        {
            get;
        }
    }
}
