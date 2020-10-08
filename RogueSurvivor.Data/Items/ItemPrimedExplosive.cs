using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemPrimedExplosive : ItemExplosive
    {
        public int FuseTimeLeft { get; set; }

        public ItemPrimedExplosive(ItemModel model)
            : base(model, model)
        {
            if (!(model is ItemExplosiveModel))
                throw new ArgumentException("model is not ItemExplosiveModel");

            this.FuseTimeLeft = (model as ItemExplosiveModel).FuseDelay;
        }
    }
}
