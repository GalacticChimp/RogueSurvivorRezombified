using djack.RogueSurvivor.Common;

namespace djack.RogueSurvivor.Data.Helpers
{
    public  static class DirectionHelpers
    {
        public static Direction RollDirection()
        {
            return Direction.COMPASS[DiceRoller.Roll(0, 8)];
        }
    }
}
