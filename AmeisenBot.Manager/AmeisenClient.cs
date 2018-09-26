using AmeisenBotData;
using AmeisenBotUtilities;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Timers;

namespace AmeisenBotManager
{
    public class AmeisenClient : IDisposable
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private Timer botListUpdateTimer;
        private Timer botUpdateTimer;
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
            AmeisenDataHolder = ameisenDataHolder;

            botUpdateTimer = new System.Timers.Timer(1000);
            botUpdateTimer.Elapsed += UpdateBot;

            botListUpdateTimer = new System.Timers.Timer(1000);
            botListUpdateTimer.Elapsed += UpdateBotList;
        }

        public void Register(Me me, IPAddress ip, int port = 16200)
        {
            IRestResponse response = SendRequest(me, ip, port, HttpMethod.Post, true);
            if (response.StatusCode == HttpStatusCode.Created
                || response.StatusCode == HttpStatusCode.OK)
            {
                IsRegistered = true;
                NetworkBot responseBot = JsonConvert.DeserializeObject<NetworkBot>(response.Content);
                BotID = responseBot.id;

                ConnectedIP = ip;
                ConnectedPort = port;

                botUpdateTimer.Start();
                botListUpdateTimer.Start();
            }
        }

        private IRestResponse SendRequest(Me me, IPAddress ip, int port, HttpMethod post, bool sendPicture = false)
        {
            SendableMe meSendable = new SendableMe().ConvertFromMe(me);
            string meSendableJSON = JsonConvert.SerializeObject(meSendable);

            string base64Image = "";
            if (sendPicture && AmeisenDataHolder.Settings.picturePath.Length > 0)
            {
                base64Image = Convert.ToBase64String(
                        Utils.ImageToByte(
                            new Bitmap(AmeisenDataHolder.Settings.picturePath)));
            }

            RestClient client = new RestClient($"http://{ip}:{port}");
            // client.Authenticator = new HttpBasicAuthenticator(username, password);

            RestRequest request = new RestRequest("bot/{name}", Method.PUT)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddUrlSegment("name", AmeisenDataHolder.Settings.ameisenServerName);
            request.AddParameter("id", 0);
            request.AddParameter("ip", "0.0.0.0");
            request.AddParameter("lastActive", Environment.TickCount);
            request.AddParameter("name", AmeisenDataHolder.Settings.ameisenServerName);
            request.AddParameter("me", meSendableJSON);
            request.AddParameter("picture", base64Image);

            return client.Execute(request);
        }

        public void Unregister(Me me, IPAddress ip, int port = 5000)
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
            IRestResponse response = GetAllBots(ConnectedIP, ConnectedPort);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Bots = JsonConvert.DeserializeObject<List<NetworkBot>>(response.Content);
            }
        }

        private IRestResponse GetAllBots(IPAddress ip, int port)
        {
            RestClient client = new RestClient($"http://{ip}:{port}");
            // client.Authenticator = new HttpBasicAuthenticator(username, password);
            RestRequest request = new RestRequest("allbots/", Method.GET);
            return client.Execute(request);
        }
    }
}