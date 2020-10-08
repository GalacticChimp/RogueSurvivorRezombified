using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public class StateMapObject : MapObject
    {
        int m_State;

        public int State
        {
            get { return m_State; }
        }

        public StateMapObject(string name, string hiddenImageID)
            : base(name, hiddenImageID)
        {
        }

        public StateMapObject(string name, string hiddenImageID, Break breakable, Fire burnable, int hitPoints)
            : base(name, hiddenImageID, breakable, burnable, hitPoints)
        {
        }

        public virtual void SetState(int newState)
        {
            m_State = newState;
        }
    }
}
