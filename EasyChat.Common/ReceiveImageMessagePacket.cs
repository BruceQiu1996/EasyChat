namespace EasyChat.Common
{
    public class ReceiveImageMessagePacket : ReceiveMessagePacket
    {
        public byte[] Data { get; set; }
        public string FileName { get; set; }
    }
}
