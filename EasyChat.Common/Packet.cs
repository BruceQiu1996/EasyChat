namespace EasyChat.Common
{
    public class Packet
    {
        public string PacketId { get; } = Guid.NewGuid().ToString();
        public DateTime SendTime { get; set; } = DateTime.Now;

        public Packet()
        {
        }
    }
}
