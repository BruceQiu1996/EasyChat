namespace EasyChat.Common
{
    public class ReceiveImageMessagePacket : ReceiveMessagePacket
    {
        public byte[] Data { get; set; }
        public string FileName { get; set; }

        public override ReceiveImageMessagePacket FromMessage(SendMessagePacket packet, bool fromSelf, bool success = true, string reason = null)
        {
            base.FromMessage(packet, fromSelf, success, reason);

            Data = (packet as SendImageMessagePacket)!.Data;
            FileName = (packet as SendImageMessagePacket)!.FileName;

            return this;
        }
    }
}
