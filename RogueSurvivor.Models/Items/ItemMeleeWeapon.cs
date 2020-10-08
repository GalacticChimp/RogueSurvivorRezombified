using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemMeleeWeapon : ItemWeapon
    {
        public bool IsFragile
        {
            get { return (this.Model as ItemMeleeWeaponModel).IsFragile; }
        }

        // alpha10
        public int ToolBashDamageBonus
        {
            get { return (this.Model as ItemMeleeWeaponModel).ToolBashDamageBonus; }
        }

        public float ToolBuildBonus
        {
            get { return (this.Model as ItemMeleeWeaponModel).ToolBuildBonus; }
        }

        public bool IsTool
        {
            get { return (this.Model as ItemMeleeWeaponModel).IsTool; }
        }

        public ItemMeleeWeapon(ItemModel model)
            : base(model)
        {
            if (!(model is ItemMeleeWeaponModel))
                throw new ArgumentException("model is not a MeleeWeaponModel");
        }
    }
}
