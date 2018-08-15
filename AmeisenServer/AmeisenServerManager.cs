using AmeisenCore.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenServer
{
    struct Bot
    {
        public int id;
        public Me me;
        public string name;
        public string ip;
    }

    class AmeisenServerManager
    {
        private static AmeisenServerManager i;

        private List<Bot> bots;
        private int count;

        private AmeisenServerManager() { bots = new List<Bot>(); }

        public static AmeisenServerManager GetInstance()
        {
            if (i == null)
                i = new AmeisenServerManager();
            return i;
        }

        public int AddBot(string name, string ip)
        {
            Bot bot = new Bot() { id = count, me = null, name = name, ip = ip };
            bots.Add(bot);
            count++;
            return bot.id;
        }

        public void UpdateBot(int id, Me me)
        {
            Bot bot = bots[id];
            bot.me = me;
            bots[id] = bot;
        }

        public void RemoveBot(int id) { bots.RemoveAt(id); }

        public List<Bot> GetBots() { return bots; }
    }
}
