using System;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Data.Helpers
{
    public static class DistanceHelpers
    {
        public static bool IsAdjacent(this Location a, Location b)
        {
            if (a.Map != b.Map) return false;
            return IsAdjacent(a.Position, b.Position);
        }

        public static bool IsAdjacent(this Point pA, Point pB)
        {
            return Math.Abs(pA.X - pB.X) < 2 && Math.Abs(pA.Y - pB.Y) < 2;
        }

        public static int GridDistance(Point pA, int bX, int bY)
        {
            return Math.Max(Math.Abs(pA.X - bX), Math.Abs(pA.Y - bY));
        }

        public static int GridDistance(Point pA, Point pB)
        {
            return Math.Max(Math.Abs(pA.X - pB.X), Math.Abs(pA.Y - pB.Y));
        }

        /// <summary>
        /// Standard distance formula: square root of summed squares.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float StdDistance(Point from, Point to)
        {
            int dX = to.X - from.X;
            int dY = to.Y - from.Y;
            int square = dX * dX + dY * dY;
            return (float)Math.Sqrt(square);
        }

        public static float StdDistance(Point v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }
    }
}
