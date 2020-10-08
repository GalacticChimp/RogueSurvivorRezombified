using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public enum ActorTasks
    {
        BARRICADE_ONE,
        BARRICADE_MAX,
        GUARD,
        PATROL,
        DROP_ALL_ITEMS,
        BUILD_SMALL_FORTIFICATION,
        BUILD_LARGE_FORTIFICATION,
        REPORT_EVENTS,
        SLEEP_NOW,
        FOLLOW_TOGGLE,
        WHERE_ARE_YOU
    }

    [Serializable]
    public class ActorOrder
    {
        ActorTasks m_Task;
        Location m_Location;

        public ActorTasks Task { get { return m_Task; } }
        public Location Location { get { return m_Location; } }

        public ActorOrder(ActorTasks task, Location location)
        {
            m_Task = task;
            m_Location = location;
        }

        public override string ToString()
        {
            switch (m_Task)
            {
                case ActorTasks.BARRICADE_ONE:
                    return String.Format("barricade one ({0},{1})", m_Location.Position.X, m_Location.Position.Y);
                case ActorTasks.BARRICADE_MAX:
                    return String.Format("barricade max ({0},{1})", m_Location.Position.X, m_Location.Position.Y);
                case ActorTasks.BUILD_LARGE_FORTIFICATION:
                    return String.Format("build large fortification ({0},{1})", m_Location.Position.X, m_Location.Position.Y);
                case ActorTasks.BUILD_SMALL_FORTIFICATION:
                    return String.Format("build small fortification ({0},{1})", m_Location.Position.X, m_Location.Position.Y);
                case ActorTasks.DROP_ALL_ITEMS:
                    return "drop all items";
                case ActorTasks.GUARD:
                    return String.Format("guard ({0},{1})", m_Location.Position.X, m_Location.Position.Y);
                case ActorTasks.PATROL:
                    return String.Format("patrol ({0},{1})", m_Location.Position.X, m_Location.Position.Y);
                case ActorTasks.REPORT_EVENTS:
                    return "reporting events to leader";
                case ActorTasks.SLEEP_NOW:
                    return "sleep there";
                case ActorTasks.FOLLOW_TOGGLE:
                    return "stop/start following";
                case ActorTasks.WHERE_ARE_YOU:
                    return "reporting position";
                default:
                    throw new NotImplementedException("unhandled task");
            }
        }
    }
}
