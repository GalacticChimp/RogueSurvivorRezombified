using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public class Faction
    {
        int m_ID;
        string m_Name;
        string m_MemberName;
        List<Faction> m_Enemies = new List<Faction>(1);

        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        public string Name
        {
            get { return m_Name; }
        }

        public string MemberName
        {
            get { return m_MemberName; }
        }

        public bool LeadOnlyBySameFaction
        {
            get;
            set;
        }

        public IEnumerable<Faction> Enemies
        {
            get { return m_Enemies; }
        }

        public Faction(string name, string memberName)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (memberName == null)
                throw new ArgumentNullException("memberName");

            m_Name = name;
            m_MemberName = memberName;
        }

        public void AddEnemy(Faction other)
        {
            m_Enemies.Add(other);
        }

        public virtual bool IsEnemyOf(Faction other)
        {
            return other != this && m_Enemies.Contains(other);
        }
    }
}
