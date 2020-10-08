using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemSprayPaint : Item
    {
        public int PaintQuantity { get; set; }

        public ItemSprayPaint(ItemModel model)
            : base(model)
        {
            if (!(model is ItemSprayPaintModel))
                throw new ArgumentException("model is not a SprayPaintModel");

            this.PaintQuantity = (model as ItemSprayPaintModel).MaxPaintQuantity;
        }
    }
}
