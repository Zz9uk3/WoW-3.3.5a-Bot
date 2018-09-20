using AmeisenBotUtilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;

namespace AmeisenBotServer
{
    internal class AmeisenServer
    {
        private static ArrayList activeBots;
        private static int botID;
        private static Timer timeoutTimer;

        private static NetworkBot ConvertRegisterDataToBot(HttpListenerRequest request)
        {
            string bodyContent = ReadBody(request);
            NetworkRegData registerData = JsonConvert.DeserializeObject<NetworkRegData>(bodyContent);

            NetworkBot convertedBot = new NetworkBot
            {
                id = botID,
                me = registerData.Me,
                ip = request.RemoteEndPoint.Address.ToString(),
                base64Image = registerData.Base64Image,
                lastUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            convertedBot.name = convertedBot.me.Name;

            return convertedBot;
        }

        /// <summary>
        /// THIS THING IS THE SHIT...
        /// </summary>
        /// <param name="ip">IP-Address</param>
        /// <param name="httpMethod">GET, POST, PUT ...</param>
        /// <param name="url">URL</param>
        private static void FancyCW(string ip, string httpMethod, string url)
        {
            Console.Write("[");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(ip);
            Console.ResetColor();

            Console.Write("]{");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(httpMethod);
            Console.ResetColor();

            Console.Write("}: ");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(url + "\n");
            Console.ResetColor();
        }

        private static int GetBotPositionByID(int botID)
        {
            int count = 0;
            foreach (NetworkBot b in activeBots)
            {
                if (b.id == botID)
                {
                    return count;
                }

                count++;
            }
            return -1;
        }

        private static void Main(string[] args)
        {
            Console.Title = "AmeisenServer";

            activeBots = new ArrayList();
            botID = 0;

            WebServer webServer = new WebServer(SendResponse, "http://+:16200/");
            string serverRunning = "#+        Starting AmeisenServer [0.0.0.0:16200]        +#";
            StringBuilder fancyBar = new StringBuilder();

            foreach (char c in serverRunning)
            {
                fancyBar.Append("-");
            }

            Console.WindowWidth = fancyBar.Length + 1;
            Console.BufferWidth = fancyBar.Length + 1;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(fancyBar);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(serverRunning);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(fancyBar);
            Console.ResetColor();

            webServer.Run();

            timeoutTimer = new Timer(1000);
            timeoutTimer.Elapsed += TimeoutCheck;
            timeoutTimer.Start();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("+- Webserver started...");
            Console.ResetColor();

            string cmd = "";
            while (!cmd.ToLower().Equals("stop"))
            {
                cmd = Console.ReadLine();
                if (!cmd.ToLower().Equals("stop"))
                {
                    Console.WriteLine($"unknown: {cmd}");
                }
            }
            timeoutTimer.Stop();
        }

        /// <summary>
        /// Read the body of the request we just received
        /// </summary>
        /// <param name="request">request to read the body of</param>
        /// <returns>body of the request as string</returns>
        private static string ReadBody(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return "";
            }

            string body = "";
            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                body = reader.ReadToEnd();
            }
            return body;
        }

        private static int RemoveBot(HttpListenerRequest request)
        {
            string bodyContent = ReadBody(request);
            int idToRemove = Convert.ToInt32(bodyContent.Split('[')[1].Replace("]", ""));
            RemoveBotByID(idToRemove);
            return idToRemove;
        }

        private static void RemoveBotByID(int idToRemove)
        {
            foreach (NetworkBot b in activeBots)
            {
                if (b.id == idToRemove)
                {
                    activeBots.Remove(b);
                    break;
                }
            }
        }

        private static string SendResponse(HttpListenerRequest request)
        {
            // Bot GET ACTIVE BOTS
            if (request.HttpMethod == "GET")
            {
                if (request.Url.ToString().Contains("activeBots"))
                {
                    ArrayList copyOfBotList = (ArrayList)activeBots.Clone();
                    return JsonConvert.SerializeObject(copyOfBotList);
                }
                else
                {
                    return string.Format("Go to the ./web/ folder and open index.html for a webview lmao, this \"shit\" will never get through here >:)");
                }
            }
            // Bot REGISTER
            else if (request.HttpMethod == "POST")
            {
                if (request.Url.ToString().Contains("botRegister"))
                {
                    activeBots.Add(ConvertRegisterDataToBot(request));
                    botID++;
                    return "addedBot:[" + (botID - 1) + "]";
                }
                // Bot UNREGISTER
                else if (request.Url.ToString().Contains("botUnregister"))
                {
                    return "removedBot:[" + RemoveBot(request) + "]";
                }
                // Bot UPDATE
                else if (request.Url.ToString().Contains("botUpdate"))
                {
                    return "updatedBot:[" + UpdateBot(request) + "]";
                }
                // UNKNOWN
                else
                {
                    return "unknown command";
                }
            }
            else
            {
                return "AyyLMAO";
            }
        }

        private static void TimeoutCheck(object sender, ElapsedEventArgs e)
        {
            foreach (NetworkBot bot in activeBots)
            {
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - bot.lastUpdate > 5000)
                {
                    RemoveBotByID(bot.id);
                }
            }
        }

        private static int UpdateBot(HttpListenerRequest request)
        {
            string bodyContent = ReadBody(request);
            int idToUpdate = Convert.ToInt32(bodyContent.Split(']')[0].Replace("]", ""));
            string botContent = bodyContent.Split(']')[1];

            SendableMe meSendable = JsonConvert.DeserializeObject<SendableMe>(botContent);

            NetworkBot convertedBot = new NetworkBot
            {
                id = idToUpdate,
                me = meSendable,
                ip = request.RemoteEndPoint.Address.ToString(),
                base64Image = ((NetworkBot)activeBots[GetBotPositionByID(idToUpdate)]).base64Image,
                lastUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };

            convertedBot.name = convertedBot.me.Name;
            activeBots[GetBotPositionByID(idToUpdate)] = convertedBot;

            return idToUpdate;
        }
    }
}