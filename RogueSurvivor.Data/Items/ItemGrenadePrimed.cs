using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemGrenadePrimed : ItemPrimedExplosive
    {
        public ItemGrenadePrimed(ItemModel model)
            : base(model)
        {
            if (!(model is ItemGrenadePrimedModel))
                throw new ArgumentException("model is not ItemGrenadePrimedModel");
        }
    }
}
