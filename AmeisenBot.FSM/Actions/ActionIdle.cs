using AmeisenBotCore;
using AmeisenBotData;
using AmeisenBotFSM.Interfaces;
using System;
using static AmeisenBotFSM.Objects.Delegates;

namespace AmeisenBotFSM.Actions
{
    public class ActionIdle : IAction
    {
        public Start StartAction { get { return Start; } }
        public DoThings StartDoThings { get { return DoThings; } }
        public Exit StartExit { get { return Stop; } }
        private long TickCountToExecuteRandomEmote { get; set; }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }

        private string[] randomEmoteList = {
            "dance",
            "shrug",
            "laugh",
            "train",
            "joke",
            "fart",
            "bravo",
            "chicken"
        };

        public ActionIdle(AmeisenDataHolder ameisenDataHolder)
        {
            AmeisenDataHolder = ameisenDataHolder;
        }

        public void DoThings()
        {
            if (AmeisenDataHolder.IsAllowedToDoRandomEmotes)
            {
                DoRandomEmote();
            }
        }

        public void Start()
        {
            TickCountToExecuteRandomEmote = new Random().Next(60000, 600000);
        }

        public void Stop() { }

        private void DoRandomEmote()
        {
            if (Environment.TickCount >= TickCountToExecuteRandomEmote)
            {
                AmeisenCore.LuaDoString($"DoEmote(\"{randomEmoteList[new Random().Next(randomEmoteList.Length)]}\");");
                TickCountToExecuteRandomEmote = new Random().Next(60000, 600000);
            }
        }
    }
}