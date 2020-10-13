using System.Drawing;

namespace djack.RogueSurvivor.Common
{
    public static class PointExtensions
    {
        public static Point Add(this Point pt, int x, int y)
        {
            return new Point(pt.X + x, pt.Y + y);
        }

        public static Point Add(this Point pt, Point other)
        {
            return new Point(pt.X + other.X, pt.Y + other.Y);
        }
    }
}
