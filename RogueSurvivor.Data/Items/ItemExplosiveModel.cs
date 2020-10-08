namespace djack.RogueSurvivor.Data.Items
{
    public class ItemExplosiveModel : ItemModel
    {
        int m_FuseDelay;
        BlastAttack m_Attack;
        string m_BlastImageID;

        public int FuseDelay { get { return m_FuseDelay; } }
        public BlastAttack BlastAttack { get { return m_Attack; } }
        public string BlastImage { get { return m_BlastImageID; } }

        public ItemExplosiveModel(string aName, string theNames, string imageID, int fuseDelay, BlastAttack attack, string blastImageID)
            : base(aName, theNames, imageID)
        {
            m_FuseDelay = fuseDelay;
            m_Attack = attack;
            m_BlastImageID = blastImageID;
        }
    }
}
