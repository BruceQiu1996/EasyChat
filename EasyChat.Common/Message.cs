using EasyChat.Common.Extensions;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace EasyChat.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public class Message<TBody> where TBody : Packet
    {
        //数据包包体长度 4字节
        public int BodyLength { get; private set; }
        //消息类型 4字节
        public MessageType MessageType { get; set; }
        public TBody? Body { get; set; }
        private Message<TBody> Deserialize(byte[] bodyData)
        {
            var bodyStr = System.Text.Encoding.Default.GetString(bodyData);
            Body = JsonSerializer.Deserialize<TBody>(bodyStr);

            return this;
        }

        public static Message<TBody> FromBytes(ReadOnlyMemory<byte> data)
        {
            Message<TBody> packet = new Message<TBody>();
            packet.BodyLength = BinaryPrimitives.ReadInt32BigEndian(data.Slice(0, 4).Span);
            packet.MessageType = (MessageType)BinaryPrimitives.ReadInt32BigEndian(data.Slice(4, 4).Span);
            packet.Deserialize(data.Slice(8, packet.BodyLength).Span.ToArray());

            return packet;
        }

        public byte[] Serialize()
        {
            var Length = 4 + 4;
            var bodyArray = System.Text.Encoding.Default.GetBytes(JsonSerializer.Serialize(Body));
            BodyLength = bodyArray.Length;
            Length += bodyArray.Length;
            byte[] result = new byte[Length];
            result.AddInt32(0, bodyArray.Length);
            result.AddInt32(4, (int)MessageType);
            Buffer.BlockCopy(bodyArray, 0, result, 8, bodyArray.Length);

            return result;
        }
    }
}
