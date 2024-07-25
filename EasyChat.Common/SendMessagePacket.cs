namespace EasyChat.Common
{
    public class SendMessagePacket : Packet
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string To { get; set; }
    }
}
