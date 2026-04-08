using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Enums
{
    [Flags]
    public enum SectionType
    {
        None = 0,
        General = 1,
        Detailed = 2,
        Distance = 4
    }
}
