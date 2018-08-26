using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Net;

namespace AmeisenServer
{
    internal class AmeisenServer
    {
        #region Private Fields

        private static ArrayList activeBots;
        private static int botCount;

        #endregion Private Fields

        #region Private Methods

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

        private static void Main(string[] args)
        {
            Console.Title = "AmeisenServer";

            activeBots = new ArrayList();
            botCount = 0;

            WebServer webServer = new WebServer(SendResponse, "http://+:16200/");
            string serverRunning = "#+        Starting AmeisenServer [0.0.0.0:16200]        +#";
            string fancyBar = "";

            foreach (char c in serverRunning)
                fancyBar += "-";

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

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("+- Webserver started...");
            Console.ResetColor();

            string cmd = "";
            while (!cmd.ToLower().Equals("stop"))
            {
                cmd = Console.ReadLine();
                if (!cmd.ToLower().Equals("stop"))
                    Console.WriteLine("unknown: " + cmd);
            }
        }

        /// <summary>
        /// Read the body of the request we just received
        /// </summary>
        /// <param name="request">request to read the body of</param>
        /// <returns>body of the request as string</returns>
        private static string ReadBody(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
                return null;

            using (Stream body = request.InputStream)
            using (StreamReader reader = new StreamReader(body, request.ContentEncoding))
                return reader.ReadToEnd();
        }

        private static string SendResponse(HttpListenerRequest request)
        {
            FancyCW(request.RemoteEndPoint.Address.ToString(), request.HttpMethod.ToString(), request.Url.ToString());

            if (request.HttpMethod == "GET")
            {
                if (request.Url.ToString().Contains("activeBots"))
                    return JsonConvert.SerializeObject(activeBots);
                else
                    return string.Format("Go to the ./web/ folder and open index.html for a webview lmao, this kind of \"shit\" will never get through here >:)");
            }
            else if (request.HttpMethod == "POST")
            {
                if (request.Url.ToString().Contains("botRegister"))
                {
                    string bodyContent = ReadBody(request);

                    MeSendable meSendable = JsonConvert.DeserializeObject<MeSendable>(bodyContent);

                    Bot botToAdd = new Bot
                    {
                        id = botCount,
                        me = meSendable,
                        ip = request.RemoteEndPoint.Address.ToString()
                    };
                    botToAdd.name = botToAdd.me.Name;

                    activeBots.Add(botToAdd);
                    FancyCW(request.RemoteEndPoint.Address.ToString(), request.HttpMethod.ToString(), "ACTIVEBOTS: " + JsonConvert.SerializeObject(activeBots));

                    botCount++;
                    return "addedBot:[" + (botCount - 1) + "]";
                }
                else if (request.Url.ToString().Contains("botUnregister"))
                {
                    string bodyContent = ReadBody(request);

                    int idToRemove = Convert.ToInt32(bodyContent.Split('[')[1].Replace("]", ""));

                    foreach (Bot b in activeBots)
                        if (b.id == idToRemove)
                        {
                            activeBots.Remove(b);
                            break;
                        }

                    return "removedBot:[" + idToRemove + "]";
                }
                else if (request.Url.ToString().Contains("botUpdate"))
                {
                    string bodyContent = ReadBody(request);

                    int idToUpdate = Convert.ToInt32(bodyContent.Split(']')[0].Replace("]", ""));
                    string botContent = bodyContent.Split(']')[1];

                    MeSendable meSendable = JsonConvert.DeserializeObject<MeSendable>(botContent);

                    Bot botToUpdate = new Bot
                    {
                        id = idToUpdate,
                        me = meSendable,
                        ip = request.RemoteEndPoint.Address.ToString()
                    };
                    botToUpdate.name = botToUpdate.me.Name;

                    activeBots[idToUpdate] = botToUpdate;

                    return "updatedBot:[" + idToUpdate + "]";
                }
                else
                    return "unknown command";
            }
            else
                return "AyyLMAO";
        }

        #endregion Private Methods
    }
}