namespace djack.RogueSurvivor.Data.Items
{
    public class ItemWeaponModel : ItemModel
    {
        Attack m_Attack;

        public Attack Attack
        {
            get { return m_Attack; }
        }

        public ItemWeaponModel(string aName, string theNames, string imageID, Attack attack)
            : base(aName, theNames, imageID)
        {
            m_Attack = attack;
        }
    }
}
