using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public class PowerGenerator : StateMapObject
    {
        public const int STATE_OFF = 0;
        public const int STATE_ON = 1;

        string m_OffImageID;
        string m_OnImageID;

        public bool IsOn
        {
            get { return this.State == STATE_ON; }
        }

        public PowerGenerator(string name, string offImageID, string onImageID)
            : base(name, offImageID)
        {
            m_OffImageID = offImageID;
            m_OnImageID = onImageID;
        }

        public override void SetState(int newState)
        {
            base.SetState(newState);

            switch (newState)
            {
                case STATE_OFF:
                    this.ImageID = m_OffImageID;
                    break;

                case STATE_ON:
                    this.ImageID = m_OnImageID;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("unhandled state");
            }
        }

        public void TogglePower()
        {
            SetState(this.State == STATE_OFF ? STATE_ON : STATE_OFF);
        }
    }
}
