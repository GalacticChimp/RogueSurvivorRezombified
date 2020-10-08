namespace djack.RogueSurvivor.Data
{
    public abstract class ItemModelDB
    {
        public abstract ItemModel this[int id]
        {
            get;
        }
    }
}
