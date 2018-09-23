using AmeisenBotLogger;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenBotCore
{
    public class AmeisenEventHook
    {
        private const string LUA_FRAME = "abotEventFrame";
        private const string LUA_TABLE = "abotEventTable";
        private const string LUA_REGISTER = "abotRegisterEvent";
        private const string LUA_UNREGISTER = "abotUnregisterEvent";
        private const string LUA_INFO = "abotGetInfo";
        private const string LUA_EVENTRECEIVED = "abotOnReceivedEvent";
        private const string LUA_EVENTCOUNT = "abotGetEventCount";
        private const string LUA_EVENTREMOVE = "abotRemoveEvent";
        private const string LUA_EVENTNAME = "abotGetEventName";

        public bool IsActive { get; private set; }
        private List<AmeisenEvent> SubscribedEvents { get; set; }
        private Thread EventReader { get; set; }

        public AmeisenEventHook()
        {
            EventReader = new Thread(new ThreadStart(ReadEvents));
        }

        public void Init()
        {
            //StringBuilder luaStuff = new StringBuilder();
            AmeisenCore.LuaDoString($"{LUA_FRAME} = CreateFrame('Frame','{LUA_FRAME}');{LUA_FRAME}:SetScript('OnEvent',{LUA_EVENTRECEIVED});{LUA_TABLE}={"{}"};");
            AmeisenCore.LuaDoString($"function {LUA_REGISTER}(e){LUA_FRAME}:RegisterEvent(e);end");
            AmeisenCore.LuaDoString($"function {LUA_UNREGISTER}(e){LUA_FRAME}:UnregisterEvent(e);end");
            AmeisenCore.LuaDoString($"function {LUA_INFO}(e,d)table.insert({LUA_TABLE}, {"{e,time(),d}"});end");
            AmeisenCore.LuaDoString($"function {LUA_EVENTRECEIVED}(s,e,...){LUA_INFO}(e,{"{...}"});end");
            AmeisenCore.LuaDoString($"function {LUA_EVENTCOUNT}()return {LUA_TABLE}.count;end");
            AmeisenCore.LuaDoString($"function {LUA_EVENTREMOVE}()table.wipe({LUA_TABLE});end");
            AmeisenCore.LuaDoString($"function {LUA_EVENTNAME}(i)local ret;ret={LUA_TABLE}[i];{LUA_TABLE}.remove(i);return ret;end");
            //AmeisenCore.LuaDoString(luaStuff.ToString());

            IsActive = true;
            EventReader.Start();
        }

        public void Stop()
        {
            if (IsActive)
            {
                IsActive = false;
                EventReader.Join();
            }
        }

        public void Subscribe(string eventName)
        {
            AmeisenCore.LuaDoString($"{LUA_REGISTER}('{eventName}')");
        }

        public void Unsubscribe(string eventName)
        {
            AmeisenCore.LuaDoString($"{LUA_UNREGISTER}('{eventName}')");
        }

        private void ReadEvents()
        {
            while (IsActive)
            {
                string eventString = AmeisenCore.GetLocalizedText($"ameisenbotEvent = {LUA_EVENTNAME}(1)", "ameisenbotEvent");
                if (eventString != "")
                {
                    AmeisenLogger.Instance.Log(LogLevel.DEBUG, "LUA Event Fired: " + eventString, this);
                }

                Thread.Sleep(500);
            }
        }
    }

    public class AmeisenEvent
    {
        private string WowEventName { get; set; }

        private delegate void OnWowEventReceived();
    }
}