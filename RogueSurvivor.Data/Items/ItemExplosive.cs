using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemExplosive : Item
    {
        public int PrimedModelID { get; private set; }

        public ItemExplosive(ItemModel model, ItemModel primedModel)
            : base(model)
        {
            if (!(model is ItemExplosiveModel))
                throw new ArgumentException("model is not ItemExplosiveModel");

            this.PrimedModelID = primedModel.ID;
        }
    }
}
