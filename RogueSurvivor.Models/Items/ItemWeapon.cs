using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemWeapon : Item
    {
        public ItemWeapon(ItemModel model)
            : base(model)
        {
            if (!(model is ItemWeaponModel))
                throw new ArgumentException("model is not a WeaponModel");
        }
    }
}
