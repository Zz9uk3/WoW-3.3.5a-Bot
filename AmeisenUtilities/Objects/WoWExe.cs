using System.Diagnostics;

namespace AmeisenUtilities
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
