using System;

namespace djack.RogueSurvivor.Data.Items
{
    [Serializable]
    public class ItemMedicine : Item
    {
        public int Healing { get; private set; }
        public int StaminaBoost { get; private set; }
        public int SleepBoost { get; private set; }
        public int InfectionCure { get; private set; }
        public int SanityCure { get; private set; }

        public ItemMedicine(ItemModel model)
            : base(model)
        {
            if (!(model is ItemMedicineModel))
                throw new ArgumentException("model is not a MedecineModel");

            ItemMedicineModel m = model as ItemMedicineModel;
            this.Healing = m.Healing;
            this.StaminaBoost = m.StaminaBoost;
            this.SleepBoost = m.SleepBoost;
            this.InfectionCure = m.InfectionCure;
            this.SanityCure = m.SanityCure;
        }
    }
}
