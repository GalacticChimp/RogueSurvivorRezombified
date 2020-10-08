using System;

namespace djack.RogueSurvivor.Data.Items
{
    public class ItemSprayPaintModel : ItemModel
    {
        int m_MaxPaintQuantity;
        string m_TagImageID;

        public int MaxPaintQuantity
        {
            get { return m_MaxPaintQuantity; }
        }

        public string TagImageID
        {
            get { return m_TagImageID; }
        }

        public ItemSprayPaintModel(string aName, string theNames, string imageID, int paintQuantity, string tagImageID)
            : base(aName, theNames, imageID)
        {
            if (tagImageID == null)
                throw new ArgumentNullException("tagImageID");

            m_MaxPaintQuantity = paintQuantity;
            m_TagImageID = tagImageID;
        }
    }
}
