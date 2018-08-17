using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using AmeisenCore.Objects;
using AmeisenUtilities;
using Newtonsoft.Json;

namespace AmeisenServer
{
    class AmeisenServerThread
    {
        public bool Stop { get; set; }
        public bool Running { get; set; }
        private readonly TcpClient client;

        public AmeisenServerThread(TcpClient client)
        {
            this.client = client;
            new Thread(new ThreadStart(Run)).Start();
        }

        private void Run()
        {
            Running = true;

            StreamReader inStream = new StreamReader(client.GetStream());
            string clientName = inStream.ReadLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("New Client: " + client.Client.RemoteEndPoint.ToString() + " " + clientName);
            Console.ResetColor();

            int botID = AmeisenServerManager.GetInstance().AddBot(clientName, client.Client.RemoteEndPoint.ToString());

            while (!Stop && client.Connected)
            {
                string data = inStream.ReadLine();

                string cmd = data.Split(']')[0].Replace("]", "");
                data = data.Split(']')[1];

                Console.WriteLine(clientName + " [" + cmd + "]: " + data);

                switch (cmd)
                {
                    case "0":
                        if (data.Contains("SHUTDOWN"))
                            Stop = true;
                        break;

                    case "1":
                        try { AmeisenServerManager.GetInstance().UpdateBot(botID, JsonConvert.DeserializeObject<MeSendable>(data)); } catch { }
                        break;

                    case "2":
                        string bots = JsonConvert.SerializeObject(AmeisenServerManager.GetInstance().GetBots());
                        StreamWriter outStream = new StreamWriter(client.GetStream());
                        outStream.WriteLine(bots);
                        break;

                    default:
                        break;
                }

                Thread.Sleep(100);
            }

            AmeisenServerManager.GetInstance().RemoveBot(botID);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Client disconnected: " + client.Client.RemoteEndPoint.ToString() + " " + clientName);
            Console.ResetColor();

            Running = false;
        }
    }
}
