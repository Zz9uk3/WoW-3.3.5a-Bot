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

        public void AttachManager(Process p)
        {
            wowProcess = p;
            memorySharp = new MemorySharp(p);
            isAttached = true;


            Console.WriteLine("Attached to WoW...");
            me = AmeisenCore.GetMe();
        }

        public MemorySharp GetMemorySharp()
        {
            if (isAttached)
                return memorySharp;
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        public Me GetMe()
        {
            if (isAttached)
                return me;
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

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
