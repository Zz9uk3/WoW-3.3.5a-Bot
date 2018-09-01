namespace AmeisenUtilities
{
    /// <summary>
    /// A Bot received from the server
    /// </summary>
    public struct NetworkBot
    {
        public string base64Image;
        public int id;
        public string ip;
        public long lastUpdate;
        public SendableMe me;
        public string name;
    }
}