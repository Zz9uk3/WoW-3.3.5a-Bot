using Newtonsoft.Json;

namespace AmeisenBotUtilities
{
    /// <summary>
    /// A Bot received from the server
    /// </summary>
    public struct NetworkBot
    {
        public string picture;
        public int id;
        public string ip;
        public long lastUpdate;
        public string me;
        public string name;

        public SendableMe GetMe() { return JsonConvert.DeserializeObject<SendableMe>(me); }
    }
}