using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaomaStorytellers
{
    public enum RoomDecaySetting
    {
        Absolute, // use the probability (later in settings) to randomly select cells
        Adjacent, 
        Augmented
    }
}
