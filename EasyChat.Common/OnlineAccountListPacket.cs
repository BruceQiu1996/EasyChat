namespace EasyChat.Common
{
    public class OnlineAccountListPacket : Packet
    {
        public Dictionary<string, string> Accounts { get; set; }
    }
}
