namespace djack.RogueSurvivor.Data.Items
{
    public class ItemLightModel : ItemModel
    {
        int m_MaxBatteries;
        int m_FovBonus;
        string m_OutOfBatteriesImageID;

        public int MaxBatteries
        {
            get { return m_MaxBatteries; }
        }

        public int FovBonus
        {
            get { return m_FovBonus; }
        }

        public string OutOfBatteriesImageID
        {
            get { return m_OutOfBatteriesImageID; }
        }

        public ItemLightModel(string aName, string theNames, string imageID, int fovBonus, int maxBatteries, string outOfBatteriesImageID)
            : base(aName, theNames, imageID)
        {
            m_FovBonus = fovBonus;
            m_MaxBatteries = maxBatteries;
            m_OutOfBatteriesImageID = outOfBatteriesImageID;
            this.DontAutoEquip = true;
        }
    }
}
