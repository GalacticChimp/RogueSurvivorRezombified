using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public enum DollPart
    {
        NONE = 0,

        _FIRST,

        RIGHT_HAND = _FIRST,
        LEFT_HAND,
        HEAD,
        TORSO,
        LEGS,
        FEET,
        SKIN,
        EYES,

        _COUNT
    }

    [Serializable]
    public class DollBody
    {
        [NonSerialized]
        public static readonly DollBody UNDEF = new DollBody(true, 0);

        readonly bool m_IsMale;
        readonly int m_Speed;

        public bool IsMale { get { return m_IsMale; } }
        public int Speed { get { return m_Speed; } }        

        public DollBody(bool isMale, int speed)
        {
            m_IsMale = isMale;
            m_Speed = speed;
        }
    }

    [Serializable]
    public class Doll
    {
        DollBody m_Body;
        List<string>[] m_Decorations;

        public DollBody Body
        {
            get { return m_Body; }
        }

        public Doll(DollBody body)
        {
            m_Body = body;
            m_Decorations = new List<string>[(int)DollPart._COUNT];
        }

        public List<string> GetDecorations(DollPart part)
        {
            return m_Decorations[(int)part];
        }

        public int CountDecorations(DollPart part)
        {
            List<string> partList = GetDecorations(part);
            return (partList == null ? 0 : partList.Count);
        }

        public void AddDecoration(DollPart part, string imageID)
        {
            List<string> partList = GetDecorations(part);
            if (partList == null)
            {
                partList = m_Decorations[(int)part] = new List<string>(1);
            }
            partList.Add(imageID);
        }

        public void RemoveDecoration(string imageID)
        {
            for (int iPart = 0; iPart < (int)DollPart._COUNT; iPart++)
            {
                List<string> partList = m_Decorations[iPart];
                if (partList == null)
                    continue;
                if (partList.Contains(imageID))
                {
                    partList.Remove(imageID);
                    if (partList.Count == 0)
                        m_Decorations[iPart] = null;
                    return;
                }
            }
        }

        public void RemoveDecoration(DollPart part)
        {
            m_Decorations[(int)part] = null;
        }

        public void RemoveAllDecorations()
        {
            for (int iPart = 0; iPart < (int)DollPart._COUNT; iPart++)
                m_Decorations[iPart] = null;
        }
    }
}
