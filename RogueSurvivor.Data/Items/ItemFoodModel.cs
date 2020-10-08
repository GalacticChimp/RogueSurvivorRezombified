namespace djack.RogueSurvivor.Data.Items
{
    public class ItemFoodModel : ItemModel
    {
        int m_Nutrition;
        bool m_IsPerishable;
        int m_BestBeforeDays;

        public int Nutrition
        {
            get { return m_Nutrition; }
        }

        public bool IsPerishable
        {
            get { return m_IsPerishable; }
        }

        public int BestBeforeDays
        {
            get { return m_BestBeforeDays; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aName"></param>
        /// <param name="theNames"></param>
        /// <param name="imageID"></param>
        /// <param name="nutrition"></param>
        /// <param name="bestBeforeDays">-1 for non perishable food.</param>
        public ItemFoodModel(string aName, string theNames, string imageID, int nutrition, int bestBeforeDays)
            : base(aName, theNames, imageID)
        {
            m_Nutrition = nutrition;
            if (bestBeforeDays < 0)
                m_IsPerishable = false;
            else
            {
                m_IsPerishable = true;
                m_BestBeforeDays = bestBeforeDays;
            }
        }
    }
}
