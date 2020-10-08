using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemFood : Item
    {
        public int Nutrition { get; private set; }
        public bool IsPerishable { get; private set; }
        public WorldTime BestBefore { get; private set; }

        /// <summary>
        /// Not perishable.
        /// </summary>
        /// <param name="model"></param>
        public ItemFood(ItemModel model)
            : base(model)
        {
            if (!(model is ItemFoodModel))
                throw new ArgumentException("model is not a FoodModel");

            this.Nutrition = (model as ItemFoodModel).Nutrition;
            this.IsPerishable = false;

        }

        /// <summary>
        /// Perishable food.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="bestBefore"></param>
        public ItemFood(ItemModel model, int bestBefore)
            : base(model)
        {
            if (!(model is ItemFoodModel))
                throw new ArgumentException("model is not a FoodModel");

            this.Nutrition = (model as ItemFoodModel).Nutrition;
            this.BestBefore = new WorldTime(bestBefore);
            this.IsPerishable = true;
        }
    }
}
