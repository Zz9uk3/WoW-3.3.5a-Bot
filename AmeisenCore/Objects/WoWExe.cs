using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AmeisenCore.Objects
{
    public class WoWExe
    {
        public Process process;
        public string characterName;

        public override string ToString()
        {
            return process.Id.ToString() + " - " + characterName;
        }
    }
}
