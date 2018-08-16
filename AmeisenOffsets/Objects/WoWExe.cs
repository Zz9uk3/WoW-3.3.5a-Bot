using System.Diagnostics;

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
