namespace djack.RogueSurvivor.Data
{
    public abstract class ActorModelDB
    {
        public abstract ActorModel this[int id]
        {
            get;
        }
    }
}
