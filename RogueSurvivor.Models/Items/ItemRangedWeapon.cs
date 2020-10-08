using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemRangedWeapon : ItemWeapon
    {
        int m_Ammo;
        AmmoType m_AmmoType;

        public int Ammo
        {
            get { return m_Ammo; }
            set { m_Ammo = value; }
        }

        public AmmoType AmmoType
        {
            get { return m_AmmoType; }
        }

        public ItemRangedWeapon(ItemModel model)
            : base(model)
        {
            if(!(model is ItemRangedWeaponModel))
                throw new ArgumentException("model is not RangedWeaponModel");

            ItemRangedWeaponModel m = model as ItemRangedWeaponModel;

            m_Ammo = m.MaxAmmo;
            m_AmmoType = m.AmmoType;
        }
    }
}
