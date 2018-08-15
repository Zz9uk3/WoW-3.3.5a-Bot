using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using AmeisenCore.Objects;

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
                Console.WriteLine(clientName + ": " + data);
                AmeisenServerManager.GetInstance().UpdateBot(botID, Newtonsoft.Json.JsonConvert.DeserializeObject<Me>(data));

                if (data.Contains("SHUTDOWN"))
                    Stop = true;

                Thread.Sleep(1000);
            }

            AmeisenServerManager.GetInstance().RemoveBot(botID);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Client disconnected: " + client.Client.RemoteEndPoint.ToString() + " " + clientName);
            Console.ResetColor();

            Running = false;
        }
    }
}
