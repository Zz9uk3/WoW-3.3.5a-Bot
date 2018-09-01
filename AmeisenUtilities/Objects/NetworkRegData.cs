namespace AmeisenUtilities
{
    public class NetworkRegData
    {
        public NetworkRegData(string base64Image, SendableMe me)
        {
            Me = me;
            Base64Image = base64Image;
        }

        public string Base64Image { get; set; }
        public SendableMe Me { get; set; }
    }
}