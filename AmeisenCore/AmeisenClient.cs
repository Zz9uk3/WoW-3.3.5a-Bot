using AmeisenCore.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore
{
    public class AmeisenClient
    {
        private static AmeisenClient i;

        private TcpClient TcpClient { get; set; }
        public bool IsConnected { get; private set; }

        private AmeisenClient() { TcpClient = new TcpClient(); }

        /// <summary>
        /// Connect to a server
        /// </summary>
        /// <param name="port">default port 16200</param>
        public void Connect(IPAddress ip, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(ip, port);
            TcpClient.Connect(endPoint);

            IsConnected = TcpClient.Connected;
            SendMessage("HELLO");
        }

        public void Disconnect()
        {
            SendMessage("SHUTDOWN");
            TcpClient.Close();
        }

        public void SendMe(Me me)
        {
            if (IsConnected)
                SendMessage(Newtonsoft.Json.JsonConvert.SerializeObject(me));
            else
                throw new Exception("You need to be connected to a server");
        }

        private void SendMessage(string s)
        {
            NetworkStream stream = TcpClient.GetStream();
            StreamWriter streamWriter = new StreamWriter(stream);

            streamWriter.AutoFlush = true;
            streamWriter.WriteLine(s + "\n\r");
        }

        public static AmeisenClient GetInstance()
        {
            if (i == null)
                i = new AmeisenClient();
            return i;
        }
    }
}
