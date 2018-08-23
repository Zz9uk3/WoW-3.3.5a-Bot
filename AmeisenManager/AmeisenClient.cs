using AmeisenData;
using AmeisenUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Timers;

namespace AmeisenManager
{
    public class AmeisenClient
    {
        private static AmeisenClient instance;
        private static readonly object padlock = new object();

        private static readonly HttpClient client = new HttpClient();

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        public List<Bot> BotList { get; private set; }

        public bool IsRegistered { get; private set; }
        public int BotID { get; private set; }
        public IPAddress IPAddress { get; private set; }
        public int Port { get; private set; }

        private System.Timers.Timer botUpdateTimer;
        private System.Timers.Timer botListUpdateTimer;

        private Thread botUpdateThread;
        private Thread botListUpdateThread;

        private AmeisenClient()
        {
            botUpdateTimer = new System.Timers.Timer(1000);
            botUpdateTimer.Elapsed += UpdateBot;

            botListUpdateTimer = new System.Timers.Timer(1000);
            botListUpdateTimer.Elapsed += UpdateBotList;
        }

        public static AmeisenClient Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new AmeisenClient();
                    return instance;
                }
            }
        }

        public async void Register(Me me, IPAddress ip, int port = 16200)
        {
            try
            {
                IPAddress = ip;
                Port = port;

                HttpContent contentToSend = new StringContent(JsonConvert.SerializeObject(new MeSendable().ConvertFromMe(me)));
                HttpResponseMessage response = await client.PostAsync("http://" + ip + ":" + port + "/botRegister/", contentToSend);
                string responseString = await response.Content.ReadAsStringAsync();

                if (responseString.Contains("addedBot"))
                {
                    IsRegistered = true;
                    BotID = Convert.ToInt32(responseString.Split('[')[1].Replace("]", ""));

                    botUpdateThread = new Thread(new ThreadStart(() => { botUpdateTimer.Start(); }));
                    botUpdateThread.Start();
                    botListUpdateThread = new Thread(new ThreadStart(() => { botListUpdateTimer.Start(); }));
                    botListUpdateThread.Start();
                }
                else
                    IsRegistered = false;
            }
            catch { IsRegistered = false; }
        }

        public async void Unregister()
        {
            HttpContent contentToSend = new StringContent("unregisterBot[" + BotID + "]");
            HttpResponseMessage response = await client.PostAsync("http://" + IPAddress + ":" + Port + "/botUnregister/", contentToSend);
            string responseString = await response.Content.ReadAsStringAsync();

            if (responseString.Contains("removedBot"))
            {
                botUpdateTimer.Stop();
                botListUpdateTimer.Stop();

                botUpdateThread.Join();
                botListUpdateThread.Join();

                IsRegistered = false;
            }
        }

        private async void UpdateBot(object source, ElapsedEventArgs e)
        {
            try
            {
                HttpContent contentToSend = new StringContent(BotID + "]" + JsonConvert.SerializeObject(new MeSendable().ConvertFromMe(Me)));
                HttpResponseMessage response = await client.PostAsync("http://" + IPAddress + ":" + Port + "/botUpdate/" + BotID + "/", contentToSend);
                string responseString = await response.Content.ReadAsStringAsync();
            }
            catch { }
        }

        private async void UpdateBotList(object source, ElapsedEventArgs e)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("http://" + IPAddress + ":" + Port + "/activeBots/");
                string responseString = await response.Content.ReadAsStringAsync();
                BotList = JsonConvert.DeserializeObject<List<Bot>>(responseString);
            }
            catch { }
        }
    }
}