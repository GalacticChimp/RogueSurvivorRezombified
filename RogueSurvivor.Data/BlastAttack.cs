using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public struct BlastAttack
    {
        public int Radius { get; private set; }
        public int[] Damage { get; private set; }
        public bool CanDamageObjects { get; private set; }
        public bool CanDestroyWalls { get; private set; }

        public BlastAttack(int radius, int[] damage, bool canDamageObjects, bool canDestroyWalls)
            : this()
        {
            if (damage.Length != radius + 1)
                throw new ArgumentException("damage.Length != radius + 1");

            this.Radius = radius;
            this.Damage = damage;
            this.CanDamageObjects = canDamageObjects;
            this.CanDestroyWalls = canDestroyWalls;
        }

        public int BlastDamage(int distance, BlastAttack attack)
        {
            if (distance < 0 || distance > attack.Radius)
                throw new ArgumentOutOfRangeException(String.Format("blast distance {0} out of range", distance));

            return attack.Damage[distance];
        }
    }
}
