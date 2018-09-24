using AmeisenBotData;
using AmeisenBotUtilities;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Timers;

namespace AmeisenBotManager
{
    public class AmeisenClient : IDisposable
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private System.Timers.Timer botListUpdateTimer;
        private System.Timers.Timer botUpdateTimer;
        public int BotID { get; private set; }
        public List<NetworkBot> Bots { get; private set; }
        public bool IsRegistered { get; private set; }
        public IPAddress ConnectedIP { get; private set; }
        public int ConnectedPort { get; private set; }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Me; }
            set { AmeisenDataHolder.Me = value; }
        }

        public AmeisenClient(AmeisenDataHolder ameisenDataHolder)
        {
            AmeisenDataHolder = ameisenDataHolder;

            botUpdateTimer = new System.Timers.Timer(1000);
            botUpdateTimer.Elapsed += UpdateBot;
            AmeisenDataHolder = ameisenDataHolder;

            botListUpdateTimer = new System.Timers.Timer(1000);
            botListUpdateTimer.Elapsed += UpdateBotList;
        }

        public void Register(Me me, IPAddress ip, int port = 16200)
        {
            SendRequest(me, ip, port, HttpMethod.Post);
        }

        private IRestResponse SendRequest(Me me, IPAddress ip, int port, HttpMethod post)
        {
            SendableMe meSendable = new SendableMe().ConvertFromMe(me);
            string meSendableJSON = JsonConvert.SerializeObject(meSendable);

            string base64Image = "";
            if (AmeisenDataHolder.Settings.picturePath.Length > 0)
            {
                base64Image = Convert.ToBase64String(
                        Utils.ImageToByte(
                            new Bitmap(AmeisenDataHolder.Settings.picturePath)));
            }

            NetworkBot networkBot = new NetworkBot
            {
                id = 0,
                ip = "0.0.0.0",
                lastUpdate = Environment.TickCount,
                name = AmeisenDataHolder.Settings.ameisenServerName,
                me = meSendableJSON,
                picture = base64Image
            };

            RestClient client = new RestClient($"http://{ip}:{port}");
            // client.Authenticator = new HttpBasicAuthenticator(username, password);

            RestRequest request = new RestRequest("bot/{name}", Method.POST);
            request.AddUrlSegment("name", AmeisenDataHolder.Settings.ameisenServerName);
            request.AddObject(networkBot);
            request.AddParameter("name", "value");

            return client.Execute(request);
        }

        public void Unregister(Me me, IPAddress ip, int port = 16200)
        {
            IRestResponse response = SendRequest(me, ip, port, HttpMethod.Delete);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                IsRegistered = false;
                NetworkBot responseBot = JsonConvert.DeserializeObject<NetworkBot>(response.Content);
                BotID = responseBot.id;

                botUpdateTimer.Close();
                botListUpdateTimer.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable)botListUpdateTimer).Dispose();
                ((IDisposable)botUpdateTimer).Dispose();
                httpClient.Dispose();
            }
        }

        private void UpdateBot(object source, ElapsedEventArgs e)
        {
            IRestResponse response = SendRequest(AmeisenDataHolder.Me, ConnectedIP, ConnectedPort, HttpMethod.Put);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                NetworkBot responseBot = JsonConvert.DeserializeObject<NetworkBot>(response.Content);
                BotID = responseBot.id;
            }
        }

        private void UpdateBotList(object source, ElapsedEventArgs e)
        {
            IRestResponse response = GetAllBots(ConnectedIP, ConnectedPort, HttpMethod.Get);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Bots = JsonConvert.DeserializeObject<List<NetworkBot>>(response.Content);
            }
        }

        private IRestResponse GetAllBots(IPAddress ip, int port, HttpMethod get)
        {
            RestClient client = new RestClient($"http://{ip}:{port}");
            // client.Authenticator = new HttpBasicAuthenticator(username, password);
            RestRequest request = new RestRequest("bot/{name}", Method.POST);
            return client.Execute(request);
        }
    }
}