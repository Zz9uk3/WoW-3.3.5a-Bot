using AmeisenBotLogger;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AmeisenBotCore
{
    public class AmeisenEventHook
    {
        private const string LUA_FRAME = "ameisenbotEventFrame";
        private const string LUA_TABLE = "ameisenbotEventTable";
        private const string LUA_REGISTER = "ameisenbotRegisterEvent";
        private const string LUA_UNREGISTER = "ameisenbotUnregisterEvent";
        private const string LUA_INFO = "ameisenbotGetInfo";
        private const string LUA_EVENTRECEIVED = "ameisenbotOnReceivedEvent";
        private const string LUA_EVENTCOUNT = "ameisenbotGetEventCount";
        private const string LUA_EVENTREMOVE = "ameisenbotRemoveEvent";
        private const string LUA_EVENTNAME = "ameisenbotGetEventName";
        public bool IsActive { get; private set; }
        private List<AmeisenEvent> SubscribedEvents { get; set; }
        private Thread EventReader { get; set; }

        public AmeisenEventHook()
        {
            EventReader = new Thread(new ThreadStart(ReadEvents));
        }

        public void Init()
        {
            StringBuilder luaStuff = new StringBuilder();
            luaStuff.Append($"{LUA_FRAME} = CreateFrame('Frame','{LUA_FRAME}');{LUA_FRAME}:SetScript('OnEvent',{LUA_EVENTRECEIVED});{LUA_TABLE}={{}};");
            luaStuff.Append($"function {LUA_REGISTER}(e){LUA_FRAME}:RegisterEvent(e);end");
            luaStuff.Append($"function {LUA_UNREGISTER}(e){LUA_FRAME}:UnregisterEvent(e);end");
            luaStuff.Append($"function {LUA_INFO}(e,d)table.insert({LUA_TABLE}, {{e,time(),d}});end");
            luaStuff.Append($"function {LUA_EVENTRECEIVED}(s,e,...){LUA_INFO}(e,{{...}});end");
            luaStuff.Append($"function {LUA_EVENTCOUNT}()return {LUA_TABLE}.count;end");
            luaStuff.Append($"function {LUA_EVENTREMOVE}()table.wipe({LUA_TABLE});end");
            luaStuff.Append($"function {LUA_EVENTNAME}(i)local ret;ret={LUA_TABLE}[i];{LUA_TABLE}.remove(i);return ret;end");
            AmeisenCore.LuaDoString(luaStuff.ToString());

            IsActive = true;
            EventReader.Start();
        }

        public void Stop()
        {
            IsActive = false;
            EventReader.Join();
        }

        public void Subscribe()
        {
        }

        public void Unsubscribe()
        {
        }

        private void ReadEvents()
        {
            while (!IsActive)
            {
                string eventString = AmeisenCore.GetLocalizedText(LUA_EVENTNAME + "(1)", "lua.lua");
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "LUA Event Fired: " + eventString, this);

                Thread.Sleep(100);
            }
        }
    }

    public class AmeisenEvent
    {
        private string WowEventName { get; set; }

        private delegate void OnWowEventReceived();
    }
}