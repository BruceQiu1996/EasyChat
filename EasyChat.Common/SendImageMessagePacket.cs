namespace EasyChat.Common
{
    /// <summary>
    /// 发送图片
    /// 本质就是发送文件
    /// 后期拓展发送文件功能可以复用
    /// 发送文件和图片不一样，主要是下载逻辑
    /// 文件消息和文件本身应该分开处理，图片则可以一起发送
    /// </summary>
    public class SendImageMessagePacket : SendMessagePacket
    {
        public byte[] Data { get; set; }
        public string FileName { get; set; }
    }
}
