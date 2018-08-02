using System;
using System.Diagnostics;
using System.Threading;
using AmeisenCore.Objects;
using Magic;

namespace AmeisenCore
{
    /// <summary>
    /// Singleton class to hold important things like
    /// - Proccess: WoW.exe we're attached to
    /// - BlackMagic: instance that is attached to WoW.exe
    ///   - get the state by isAttached boolean
    /// - AmeisenHook: instance that is hookes to WoW.exe's EndScene
    ///   - get the state by isHooked boolean
    /// - Me: all character information
    /// </summary>
    public class AmeisenManager
    {
        private static AmeisenManager i;

        private bool isAttached, isHooked;

        private Process wowProcess;
        private BlackMagic blackmagic;
        private AmeisenHook ameisenHook;

        private Me me;

        private AmeisenManager()
        {
            isAttached = false;
            isHooked = false;
        }

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

            // Attach to Proccess
            blackmagic = new BlackMagic(p.Id);
            isAttached = true;

            // Hook EndScene LMAO
            // TODO: Fix this piece of garbage
            // ameisenHook = new AmeisenHook();
            // isHooked = true;

            RefreshMeAsync();
        }

        /// <summary>
        /// Get current MemorySharp instance
        /// </summary>
        /// <returns>memorysharp</returns>
        public BlackMagic GetBlackMagic()
        {
            if (isAttached)
                return blackmagic;
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        /// <summary>
        /// Get current AmeisenHook instance
        /// </summary>
        /// <returns>memorysharp</returns>
        public AmeisenHook GetAmeisenHook()
        {
            if (isHooked)
                return ameisenHook;
            else
                throw new Exception("Manager is not hooked to any WoW's EndScene...");
        }

        /// <summary>
        /// Get our char's stats, group members, target...
        /// </summary>
        /// <returns>char's stats, group members, target</returns>
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

        /// <summary>
        /// Get our WoW-Process
        /// </summary>
        public Process GetProcess()
        {
            return wowProcess;
        }

        private void RefreshMeAsync()
        {
            me = AmeisenCore.ReadMe();
        }
    }
}
