namespace djack.RogueSurvivor.Data.Items
{
    public class ItemGrenadeModel : ItemExplosiveModel
    {
        int m_MaxThrowDistance;

        public int MaxThrowDistance { get { return m_MaxThrowDistance; } }

        public ItemGrenadeModel(string aName, string theNames, string imageID, int fuseDelay, BlastAttack attack, string blastImageID, int maxThrowDistance) 
            : base(aName, theNames, imageID, fuseDelay, attack, blastImageID)
        {
            m_MaxThrowDistance = maxThrowDistance;
        }
    }
}
