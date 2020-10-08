namespace djack.RogueSurvivor.Data.Items
{
    public class ItemMeleeWeaponModel : ItemWeaponModel
    {
        public bool IsFragile { get; set; }

        // alpha10
        public int ToolBashDamageBonus { get; set; }
        public float ToolBuildBonus { get; set; }
        public bool IsTool { get { return ToolBashDamageBonus != 0 || ToolBuildBonus != 0; } }

        public ItemMeleeWeaponModel(string aName, string theNames, string imageID, Attack attack)
            : base(aName, theNames, imageID, attack)
        {
        }
    }
}
