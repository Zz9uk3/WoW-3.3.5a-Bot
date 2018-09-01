using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenUtilities
{
    public enum CombatLogicStatement
    {
        GREATER,
        GREATER_OR_EQUAL,
        EQUAL,
        LESS_OR_EQUAL,
        LESS,
        HAS_BUFF,
        HAS_BUFF_MYSELF,
        NOT_HAS_BUFF,
        NOT_HAS_BUFF_MYSELF
    }
}
