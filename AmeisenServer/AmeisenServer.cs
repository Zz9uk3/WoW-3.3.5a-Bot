using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;

namespace AmeisenServer
{
    class AmeisenServer
    {
        static void Main(string[] args)
        {
            Console.Title = "AmeisenServer";

            WebServer webServer = new WebServer(SendResponse, "http://127.0.0.1:80/");
            string serverRunning = "#+ Starting AmeisenServer [0.0.0.0:80] +#";
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

        private static string SendResponse(HttpListenerRequest request)
        {
            return string.Format("<HTML><BODY>AmeisenBot Web Interface</BODY></HTML>");
        }
    }
}
