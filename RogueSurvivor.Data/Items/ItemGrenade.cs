using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemGrenade : ItemExplosive
    {
        public ItemGrenade(ItemModel model, ItemModel primedModel)
            : base(model, primedModel)
        {
            if (!(model is ItemGrenadeModel))
                throw new ArgumentException("model is not ItemGrenadeModel");
        }
    }
}
