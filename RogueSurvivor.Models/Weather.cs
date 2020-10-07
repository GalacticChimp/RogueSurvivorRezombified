using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Models
{
    [Serializable]
    enum Weather
    {
        _FIRST,

        CLEAR = _FIRST,
        CLOUDY,
        RAIN,
        HEAVY_RAIN,

        _COUNT
    }
}
