using System.Diagnostics;

namespace AmeisenUtilities
{
    public class WoWExe
    {
        #region Public Fields

        public string characterName;
        public Process process;

        #endregion Public Fields

        #region Public Methods

        public override string ToString()
        {
            return process.Id.ToString() + " - " + characterName;
        }

        #endregion Public Methods
    }
}