namespace EasyChat.Common
{
    public class RegisterResultPacket : Packet
    {
        public bool Success { get; set; }
        public string Reason { get; set; }
    }
}
