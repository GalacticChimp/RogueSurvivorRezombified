using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    /// <summary>
    /// "Marker" class to tell the game the actor is controller by the human player. The class does nothing as the game itself handles all the work.
    /// </summary>
    public class PlayerController : ActorController
    {

        public override ActorAction GetAction(World world)
        {
            // shouldn't get here
            throw new InvalidOperationException("do not call PlayerController.GetAction()");
        }
    }
}
