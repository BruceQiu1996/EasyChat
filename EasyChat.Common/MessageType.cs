namespace EasyChat.Common
{
    public enum MessageType : int
    {
        Register = 1,
        RegisterResult = 99,
        OnlineAccountsList = 2,
        SendTextMessage = 3,
        ReceiveTextMessage = 4,
        SendImageMessage = 5,
        ReceiveImageMessage = 6
    }
}
