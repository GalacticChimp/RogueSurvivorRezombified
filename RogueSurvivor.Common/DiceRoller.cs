using System;

namespace djack.RogueSurvivor.Common
{
    [Serializable]
    public static class DiceRoller
    {
        static Random m_Rng;

        static DiceRoller()
        {
            // TODO check the seed
            int seed = (int)DateTime.UtcNow.Ticks;
            m_Rng = new Random(seed);
        }

        /// <summary>
        /// Roll in range [min, max[.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Roll(int min, int max)
        {
            // sanity check, fixes crashes.
            if (max <= min) return min;

            // roll it baby.
            int r;
            lock (m_Rng) // thread safe, Random is supposed to be thread safe but apparently not...
            {
                r = m_Rng.Next(min, max);
            }
            // FIX awfull bug, in some very rare cases .NET Random returns max instead of max-1 (wtf?!)
            if (r >= max) r = max - 1;

            return r;
        }

        public static float RollFloat()
        {
            float r;
            lock (m_Rng) // thread safe, Random is supposed to be thread safe but apparently not...
            {
                r = (float)m_Rng.NextDouble();
            }
            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chance">chance as a percentage [0..100]</param>
        /// <returns></returns>
        public static bool RollChance(int chance)
        {
            return Roll(0, 100) < chance;
        }

        /// <summary>
        /// [0,skillValue]
        /// </summary>
        /// <param name="skillValue"></param>
        /// <returns></returns>
        public static int RollSkill(int skillValue)
        {
            if (skillValue <= 0)
                return 0;
            // bell curve results = less extremes => favor higher skill vs lower skill
            return (Roll(0, skillValue + 1) + Roll(0, skillValue + 1)) / 2;
        }
    }
}
