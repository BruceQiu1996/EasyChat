using EasyTcp4Net;

namespace EasyChat.Server
{
    /// <summary>
    /// 用户对象
    /// 如果需要做单用户多端，需要用userid或者username对应多个sessionID以及ClientSession
    /// </summary>
    public class Account
    {
        public string UserName { get; set; }
        public string SessionId => Session.SessionId;
        public ClientSession Session { get; set; }
    }
}
