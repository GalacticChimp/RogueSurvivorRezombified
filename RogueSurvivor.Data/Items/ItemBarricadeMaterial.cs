using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemBarricadeMaterial : Item
    {
        public ItemBarricadeMaterial(ItemModel model) : base(model)
        {
            if (!(model is ItemBarricadeMaterialModel))
                throw new ArgumentException("model is not BarricadeMaterialModel");
        }
    }
}
