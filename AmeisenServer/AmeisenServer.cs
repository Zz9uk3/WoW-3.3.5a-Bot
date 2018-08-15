using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace AmeisenServer
{
    class AmeisenServer
    {
        private static TcpListener listener;
        private static List<AmeisenServerThread> threads = new List<AmeisenServerThread>();

        static void Main(string[] args)
        {
            Console.Title = "AmeisenServer";
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 16200);

            string serverRunning = "#+ Starting AmeisenServer [" + endPoint.ToString() + "] +#";
            string fancyBar = "";

            foreach (char c in serverRunning)
                fancyBar += "-";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(fancyBar);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(serverRunning);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(fancyBar);
            Console.ResetColor();

            listener = new TcpListener(endPoint);
            listener.Start();

            Thread mainThread = new Thread(new ThreadStart(Run));
            mainThread.Start();

            string cmd = "";
            while (!cmd.ToLower().Equals("stop"))
            {
                cmd = Console.ReadLine();
                if (!cmd.ToLower().Equals("stop"))
                    Console.WriteLine("unknown: " + cmd);
            }

            mainThread.Abort();

            foreach (AmeisenServerThread thread in threads)
            {
                thread.Stop = true;
                while (thread.Running)
                    Thread.Sleep(100);
            }

            listener.Stop();
        }

        public static void Run()
        {
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Handling new Client: " + client.Client.RemoteEndPoint.ToString());
                threads.Add(new AmeisenServerThread(client));
            }
        }
    }
}
