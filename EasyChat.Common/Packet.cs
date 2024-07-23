namespace EasyChat.Common
{
    public class Packet
    {
        public string MessageId { get; } = Guid.NewGuid().ToString();
        public DateTime SendTime { get; } = DateTime.Now;

        public Packet()
        {
        }
    }
}
