using System.Diagnostics;

namespace AmeisenUtilities
{
    public class WowExe
    {
        public string characterName;
        public Process process;

        public override string ToString()
        {
            return $"{process.Id.ToString()} - {characterName}";
        }
    }
}