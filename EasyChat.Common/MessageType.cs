namespace EasyChat.Common
{
    public enum MessageType : int
    {
        Register = 1, 
        OnlineAccountsList = 2,
        SendTextMessage = 3,
        SendImageMessage = 4,

        MessageSendAck = 5,
        MessageReadAck = 6,
    }
}
