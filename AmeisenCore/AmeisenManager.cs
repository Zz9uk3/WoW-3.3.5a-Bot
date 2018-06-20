using AmeisenCore.Objects;
using Binarysharp.MemoryManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Me RefreshMe()
        {
            if (isAttached)
            {
                me = AmeisenCore.GetMe();
                return me;
            }
            else
                throw new Exception("Manager is not attached to any WoW...");
        }
    }
}
