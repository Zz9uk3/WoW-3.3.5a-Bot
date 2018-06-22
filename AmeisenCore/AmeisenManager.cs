using AmeisenCore.Objects;
using Binarysharp.MemoryManagement;
using System;
using System.Diagnostics;
using System.Threading;

namespace AmeisenCore
{
    public class AmeisenManager
    {
        private static AmeisenManager i;

        private bool isAttached;

        private Process wowProcess;
        private MemorySharp memorySharp;

        private Me me;

        private AmeisenManager() { isAttached = false; }

        public static AmeisenManager GetInstance()
        {
            if (i == null)
                i = new AmeisenManager();
            return i;
        }

        /// <summary>
        /// Attach the manager to the given WoW Process to be able to read and write memory etc.
        /// </summary>
        /// <param name="p">wow process object</param>
        public void AttachManager(Process p)
        {
            wowProcess = p;
            memorySharp = new MemorySharp(p);
            isAttached = true;
            
            me = AmeisenCore.GetMe();
        }

        /// <summary>
        /// Get current MemorySharp
        /// </summary>
        /// <returns>memorysharp</returns>
        public MemorySharp GetMemorySharp()
        {
            if (isAttached)
                return memorySharp;
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        /// <summary>
        /// Get our char's stats
        /// </summary>
        /// <returns>char's stats</returns>
        public Me GetMe()
        {
            if (isAttached)
                return me;
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        /// <summary>
        /// Refresh our bot's stats, you can get the stats by calling GetMe().
        /// 
        /// This runs Async.
        /// </summary>
        public void RefreshMe()
        {
            if (isAttached)
            {
                new Thread(new ThreadStart(RefreshMeAsync)).Start();
            }
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        private void RefreshMeAsync()
        {
            me = AmeisenCore.GetMe();
        }
    }
}
