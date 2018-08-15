using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using AmeisenCore.Objects;
using AmeisenLogging;
using AmeisenUtilities;
using Magic;

namespace AmeisenCore
{
    /// <summary>
    /// Singleton class to hold important things like
    /// - Proccess: WoW.exe we're attached to
    /// - BlackMagic: instance that is attached to WoW.exe
    ///   - get the state by isAttached boolean
    /// - AmeisenHook: instance that is hooked to WoW.exe's EndScene
    ///   - get the state by isHooked boolean
    /// - Me: all character information
    /// </summary>
    public class AmeisenManager
    {
        private static AmeisenManager i;

        private bool isAttached;
        private bool isHooked;

        private bool stopClientThread;

        private Process wowProcess;
        private BlackMagic blackmagic;
        private AmeisenHook ameisenHook;

        private TcpClient ameisenServerClient;
        private Thread ameisenServerClientThread;

        private Me me;

        private List<WoWObject> activeWoWObjects;
        // To determine if we need to refresh some things
        private DateTime timestampObjects;

        private bool isAllowedToMove;

        public bool IsSupposedToAttack { get; set; }
        public bool IsSupposedToTank { get; set; }
        public bool IsSupposedToHeal { get; set; }

        DispatcherTimer objectUpdateTimer;

        private AmeisenManager()
        {
            isAttached = false;
            isHooked = false;
            isAllowedToMove = true;

            objectUpdateTimer = new DispatcherTimer();
            objectUpdateTimer.Tick += new EventHandler(ObjectUpdateTimer_Tick);
            objectUpdateTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
        }

        private void ObjectUpdateTimer_Tick(object sender, EventArgs e)
        {
            RefreshObjects();
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
            isAttached = blackmagic.IsProcessOpen;

            // Hook EndScene LMAO
            // TODO: Fix this piece of garbage
            ameisenHook = new AmeisenHook();
            isHooked = ameisenHook.isHooked;

            // Update our ObjectList
            objectUpdateTimer.Start();
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
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Getting Me", this);
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
        public void RefreshMeAsync()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Refresh Me Async", this);
            if (isAttached)
            {
                new Thread(new ThreadStart(RefreshMe)).Start();
            }
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        /// <summary>
        /// Get our WoWObjects in the memory
        /// </summary>
        /// <returns>WoWObjects in the memory</returns>
        public List<WoWObject> GetObjects()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Getting Objects", this);
            if (isAttached)
            {
                //bool needToRefresh = (DateTime.Now - timestampObjects).TotalSeconds > 5;

                if (activeWoWObjects == null)
                    RefreshObjectsAsync();

                // need to do this only for specific objects, saving cpu usage
                //if (needToRefresh)
                //RefreshObjectsAsync();
                return activeWoWObjects;
            }
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        /// <summary>
        /// Refresh our bot's objectlist, you can get the stats by calling GetObjects().
        /// 
        /// This runs Async.
        /// </summary>
        private void RefreshObjectsAsync()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Refreshing Objects Async", this);
            timestampObjects = DateTime.Now;

            if (isAttached)
            {
                new Thread(new ThreadStart(RefreshObjects)).Start();
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

        public void RefreshMe() { me = AmeisenCore.ReadMe(); }

        private void RefreshObjects()
        {
            activeWoWObjects = AmeisenCore.RefreshAllWoWObjects();
        }
        
        public void ConnectToAmeisenServer(IPEndPoint endPoint)
        {
            ameisenServerClientThread = new Thread(new ThreadStart(() => AmeisenServerClientThreadRun(endPoint)));
            ameisenServerClientThread.Start();
        }

        public void DisconnectFromAmeisenServer()
        {
            stopClientThread = true;
        }

        private void AmeisenServerClientThreadRun(IPEndPoint endPoint)
        {
            ameisenServerClient = new TcpClient();
            ameisenServerClient.Connect(endPoint);

            Stream outStream = ameisenServerClient.GetStream();
            Byte[] sendBytes = Encoding.ASCII.GetBytes(AmeisenSettings.GetInstance().settings.ameisenServerName + "\r\n");
            outStream.Write(sendBytes, 0, sendBytes.Length);
            outStream.Flush();

            while (!stopClientThread && ameisenServerClient.Connected)
            {
                Byte[] sendMeBytes = Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(me) + "\r\n");
                outStream.Write(sendMeBytes, 0, sendMeBytes.Length);
                outStream.Flush();
                Thread.Sleep(1000);
            }

            sendBytes = Encoding.ASCII.GetBytes("SHUTDOWN" + "\r\n");
            outStream.Write(sendBytes, 0, sendBytes.Length);
            outStream.Flush();

            ameisenServerClient.Client.Disconnect(false);
            ameisenServerClient.Close();
        }

        /// <summary>
        /// Lock bot movement
        /// </summary>
        public void LockMovement() { isAllowedToMove = false; }

        /// <summary>
        /// Unlock bot movement
        /// </summary>
        public void UnlockMovement() { isAllowedToMove = true; }

        /// <summary>
        /// Is the bot allowed to move right now?
        /// </summary>
        public bool IsAllowedToMove() { return isAllowedToMove; }
    }
}
