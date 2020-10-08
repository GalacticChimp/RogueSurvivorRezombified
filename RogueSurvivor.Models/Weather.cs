using System;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public enum Weather
    {
        _FIRST,

        CLEAR = _FIRST,
        CLOUDY,
        RAIN,
        HEAVY_RAIN,

        _COUNT
    }
}
