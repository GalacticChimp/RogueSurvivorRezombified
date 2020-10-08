namespace djack.RogueSurvivor.Data.Items
{
    public class ItemEntertainmentModel : ItemModel
    {
        int m_Value;
        int m_BoreChance;

        public int Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public int BoreChance
        {
            get { return m_BoreChance; }
            set { m_BoreChance = value; }
        }

        public ItemEntertainmentModel(string aName, string theNames, string imageID, int value, int boreChance)
            : base(aName, theNames, imageID)
        {
            m_Value = value;
            m_BoreChance = boreChance;
        }
    }
}
