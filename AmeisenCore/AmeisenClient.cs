using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore
{
    class AmeisenClient
    {
        private static AmeisenClient i;

        private AmeisenClient() { }

        public static AmeisenClient GetInstance()
        {
            if (i == null)
                i = new AmeisenClient();
            return i;
        }
    }
}
