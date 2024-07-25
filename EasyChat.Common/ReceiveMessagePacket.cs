namespace EasyChat.Common
{
    public class ReceiveMessagePacket : Packet
    {
        public bool Success { get; set; }
        public string Reason { get; set; }
        public bool FromSelf { get; set; }
        public string MessageId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
