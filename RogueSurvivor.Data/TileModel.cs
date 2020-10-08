using System.Drawing;

namespace djack.RogueSurvivor.Data
{
    public class TileModel
    {
        public static readonly TileModel UNDEF = new TileModel("", Color.Pink, false, true);

        int m_ID;
        string m_ImageID;
        bool m_IsWalkable;
        bool m_IsTransparent;
        Color m_MinimapColor;

        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        public string ImageID
        {
            get { return m_ImageID; }
        }

        public bool IsWalkable
        {
            get { return m_IsWalkable; }
        }

        public bool IsTransparent
        {
            get { return m_IsTransparent; }
        }

        public Color MinimapColor
        {
            get { return m_MinimapColor; }
        }

        public bool IsWater
        {
            get;
            set;
        }

        public string WaterCoverImageID
        {
            get;
            set;
        }

        public TileModel(string imageID, Color minimapColor, bool IsWalkable, bool IsTransparent)
        {
            m_ImageID = imageID;
            m_IsWalkable = IsWalkable;
            m_IsTransparent = IsTransparent;
            m_MinimapColor = minimapColor;
        }
    }
}
