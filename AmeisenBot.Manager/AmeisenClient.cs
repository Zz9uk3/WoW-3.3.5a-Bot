using AmeisenBotData;
using AmeisenBotUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Timers;

namespace AmeisenBotManager
{
    public class AmeisenClient
    {
        private static readonly HttpClient client = new HttpClient();
        private Thread botListUpdateThread;
        private System.Timers.Timer botListUpdateTimer;
        private Thread botUpdateThread;
        private System.Timers.Timer botUpdateTimer;
        public int BotID { get; private set; }
        public List<NetworkBot> BotList { get; private set; }
        public IPAddress IPAddress { get; private set; }
        public bool IsRegistered { get; private set; }
        public int Port { get; private set; }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }

        public AmeisenClient(AmeisenDataHolder ameisenDataHolder)
        {
            AmeisenDataHolder = ameisenDataHolder;

            botUpdateTimer = new System.Timers.Timer(1000);
            botUpdateTimer.Elapsed += UpdateBot;
            AmeisenDataHolder = ameisenDataHolder;

            botListUpdateTimer = new System.Timers.Timer(1000);
            botListUpdateTimer.Elapsed += UpdateBotList;
        }

        private Me Me
        {
            get { return AmeisenDataHolder.Me; }
            set { AmeisenDataHolder.Me = value; }
        }

        public async void Register(Me me, IPAddress ip, int port = 16200)
        {
            try
            {
                IPAddress = ip;
                Port = port;

                string base64Image = "";
                if (AmeisenDataHolder.Settings.picturePath.Length > 0)
                    base64Image = Convert.ToBase64String(
                            Utils.ImageToByte(
                                new Bitmap(AmeisenDataHolder.Settings.picturePath)
                                )
                            );

                SendableMe meSendable = new SendableMe().ConvertFromMe(me);
                string content = JsonConvert.SerializeObject(new NetworkRegData(base64Image, meSendable));

                HttpContent contentToSend = new StringContent(content);
                HttpResponseMessage response = await client.PostAsync($"http://{ip}:{port}/botRegister/", contentToSend);
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
            HttpContent contentToSend = new StringContent($"unregisterBot[{BotID}]");
            HttpResponseMessage response = await client.PostAsync($"http://{IPAddress}:{Port}/botUnregister/", contentToSend);
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
                string jsonData = JsonConvert.SerializeObject(
                    new SendableMe().ConvertFromMe(Me)
                    );

                HttpContent contentToSend = new StringContent($"{BotID}]{jsonData}");
                HttpResponseMessage response = await client.PostAsync($"http://{IPAddress}:{Port}/botUpdate/{BotID}/", contentToSend);
                string responseString = await response.Content.ReadAsStringAsync();
            }
            catch { }
        }

        private async void UpdateBotList(object source, ElapsedEventArgs e)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://{IPAddress}:{Port}/activeBots/");
                string responseString = await response.Content.ReadAsStringAsync();
                BotList = JsonConvert.DeserializeObject<List<NetworkBot>>(responseString);
                AmeisenDataHolder.ActiveNetworkBots = BotList;
            }
            catch { }
        }
    }
}