using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemSprayScent : Item
    {
        public int SprayQuantity { get; set; }
        public Odor Odor { get { return (this.Model as ItemSprayScentModel).Odor; } } // alpha10
        public int Strength { get { return (this.Model as ItemSprayScentModel).Strength; } } // alpha10

        public ItemSprayScent(ItemModel model)
            : base(model)
        {
            if (!(model is ItemSprayScentModel))
                throw new ArgumentException("model is not a ItemScentSprayModel");

            this.SprayQuantity = (model as ItemSprayScentModel).MaxSprayQuantity;
        }
    }
}
