namespace EasyChat.Common
{
    public class ReceiveTextMessagePacket : ReceiveMessagePacket
    {
        public string Text { get; set; }

        public override ReceiveTextMessagePacket FromMessage(SendMessagePacket packet, bool fromSelf, bool success = true, string reason = null)
        {
            base.FromMessage(packet, fromSelf, success, reason);

            Text = (packet as SendTextMessagePacket)!.Text;

            return this;
        }
    }
}
