using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Models
{
    abstract class TileModelDB 
    {
        public abstract TileModel this[int id]
        {
            get;
        }
    }
}
