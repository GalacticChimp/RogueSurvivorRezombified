using System;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
    public class Message
    {
        string m_Text;
        Color m_Color;
        readonly int m_Turn;

        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }

        public Color Color
        {
            get { return m_Color; }
            set { m_Color = value; }
        }

        public int Turn
        {
            get { return m_Turn; }
        }

        public Message(string text, int turn, Color color)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            m_Text = text;
            m_Color = color;
            m_Turn = turn;
        }

        public Message(string text, int turn)
            : this(text, turn, Color.White)
        {
        }
    }
}
