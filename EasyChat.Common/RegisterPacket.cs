namespace EasyChat.Common
{
    /// <summary>
    /// 向服务端注册用户名
    /// </summary>
    public class RegisterPacket : Packet
    {
        public string UserName { get; set; }
    }
}
