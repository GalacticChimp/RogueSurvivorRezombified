using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public struct Defence
    {
        [NonSerialized]
        public static readonly Defence BLANK = new Defence(0, 0, 0);

        public int Value { get; private set; }
        public int Protection_Hit { get; private set; }
        public int Protection_Shot { get; private set; }

        public Defence(int value, int protection_hit, int protection_shot)
            : this()
        {
            this.Value = value;
            this.Protection_Hit = protection_hit;
            this.Protection_Shot = protection_shot;
        }

        public static Defence operator+(Defence lhs, Defence rhs)
        {
            return new Defence(lhs.Value + rhs.Value, lhs.Protection_Hit + rhs.Protection_Hit, lhs.Protection_Shot + rhs.Protection_Shot);
        }

        public static Defence operator-(Defence lhs, Defence rhs)
        {
            return new Defence(lhs.Value - rhs.Value, lhs.Protection_Hit - rhs.Protection_Hit, lhs.Protection_Shot - rhs.Protection_Shot);
        }
    }
}
