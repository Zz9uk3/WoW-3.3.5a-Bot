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

            string serverRunning = "#+ Starting AmeisenServer [] +#";
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

            string cmd = "";
            while (!cmd.ToLower().Equals("stop"))
            {
                cmd = Console.ReadLine();
                if (!cmd.ToLower().Equals("stop"))
                    Console.WriteLine("unknown: " + cmd);
            }
        }
    }
}
