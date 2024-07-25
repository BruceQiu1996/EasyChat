namespace EasyChat.Common
{
    public class Packet
    {
        public string PacketId { get; } = Guid.NewGuid().ToString();
        public DateTime SendTime { get; } = DateTime.Now;

        public Packet()
        {
        }
    }
}
