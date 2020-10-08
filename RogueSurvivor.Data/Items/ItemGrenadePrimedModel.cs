using System;

namespace djack.RogueSurvivor.Data.Items
{
    public class ItemGrenadePrimedModel : ItemExplosiveModel
    {
        public ItemGrenadeModel GrenadeModel { get; private set; }

        public ItemGrenadePrimedModel(string aName, string theNames, string imageID, ItemGrenadeModel grenadeModel)
            : base(aName, theNames, imageID, grenadeModel.FuseDelay, grenadeModel.BlastAttack, grenadeModel.BlastImage)
        {
            if (grenadeModel == null)
                throw new ArgumentNullException("grenadeModel");

            this.GrenadeModel = grenadeModel;
        }
    }
}
