using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public class Board : MapObject
    {
        public string[] Text
        {
            get;
            set;
        }

        public Board(string name, string imageID, string[] text)
            : base(name, imageID)
        {
            this.Text = text;
        }
    }
}
