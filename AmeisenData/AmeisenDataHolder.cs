using AmeisenUtilities;
using System.Collections.Generic;

namespace AmeisenData
{
    public class AmeisenDataHolder
    {
        #region Private Fields

        private static readonly object padlock = new object();

        private static AmeisenDataHolder instance = null;

        #endregion Private Fields

        #region Private Constructors

        private AmeisenDataHolder()
        {
        }

        #endregion Private Constructors

        #region Public Properties

        public static AmeisenDataHolder Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new AmeisenDataHolder();
                    return instance;
                }
            }
        }

        public List<WoWObject> ActiveWoWObjects { get; set; }

        // Data to hold
        public Me Me { get; set; }

        public Unit Target { get; set; }

        #endregion Public Properties
    }
}