using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public abstract class TimedTask
    {
        public int TurnsLeft
        {
            get;
            set;
        }

        public bool IsCompleted
        {
            get { return this.TurnsLeft <= 0; }
        }

        protected TimedTask(int turnsLeft)
        {
            this.TurnsLeft = turnsLeft;
        }

        public void Tick(Map m)
        {
            if (--this.TurnsLeft <= 0)
                Trigger(m);
        }

        public abstract void Trigger(Map m);
    }
}
