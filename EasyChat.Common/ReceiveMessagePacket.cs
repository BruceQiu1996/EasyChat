using System.Net.Sockets;

namespace EasyChat.Common
{
    /// <summary>
    /// 消息回复
    /// </summary>
    public class ReceiveMessagePacket : Packet
    {
        public bool Success { get; set; } = true;
        public string Reason { get; set; }
        public bool FromSelf { get; set; }
        public string MessageId { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public virtual ReceiveMessagePacket FromMessage(SendMessagePacket packet, bool fromSelf, bool success = true, string reason = null)
        {
            var message = new ReceiveTextMessagePacket();
            From = packet.From;
            To = packet.To;
            SendTime = packet.SendTime;
            Reason = reason;
            Success = success;
            MessageId = packet.MessageId;
            FromSelf = fromSelf;

            return this;
        }
    }
}
