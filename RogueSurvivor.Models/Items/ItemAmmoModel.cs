namespace djack.RogueSurvivor.Data.Items
{
    public enum AmmoType
    {
        _FIRST = 0,

        LIGHT_PISTOL = _FIRST,
        HEAVY_PISTOL,
        SHOTGUN,
        LIGHT_RIFLE,
        HEAVY_RIFLE,
        BOLT,

        _COUNT
    }

    public class ItemAmmoModel : ItemModel
    {
        AmmoType m_AmmoType;

        public AmmoType AmmoType
        {
            get { return m_AmmoType; }
        }

        public int MaxQuantity
        {
            get { return this.StackingLimit; }
        }

        public ItemAmmoModel(string aName, string theNames, string imageID, AmmoType ammoType, int maxQuantity)
            : base(aName, theNames, imageID)
        {
            m_AmmoType = ammoType;
            this.IsStackable = true;
            this.StackingLimit = maxQuantity;
        }
    }
}
