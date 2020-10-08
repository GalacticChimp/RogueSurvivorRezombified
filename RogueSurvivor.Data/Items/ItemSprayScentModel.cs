namespace djack.RogueSurvivor.Data.Items
{
    public class ItemSprayScentModel : ItemModel
    {
        int m_MaxSprayQuantity;
        Odor m_Odor;
        int m_Strength;

        public int MaxSprayQuantity
        {
            get { return m_MaxSprayQuantity;}
        }

        public int Strength
        {
            get { return m_Strength; }
        }

        public Odor Odor
        {
            get { return m_Odor;}
        }

        public ItemSprayScentModel(string aName, string theNames, string imageID, int sprayQuantity, Odor odor, int strength)
            : base(aName, theNames, imageID)
        {
            m_MaxSprayQuantity = sprayQuantity;
            m_Odor = odor;
            m_Strength = strength;
        }
    }
}
