using AmeisenCore.Objects;
using AmeisenUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AmeisenCore
{
    public class AmeisenClient
    {
        private static AmeisenClient i;

        private AmeisenClient()
        {

        }

        public static AmeisenClient GetInstance()
        {
            if (i == null)
                i = new AmeisenClient();
            return i;
        }
    }
}
