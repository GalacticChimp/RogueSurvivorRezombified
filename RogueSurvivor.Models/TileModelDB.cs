namespace djack.RogueSurvivor.Data
{
    public abstract class TileModelDB 
    {
        public abstract TileModel this[int id]
        {
            get;
        }
    }
}
