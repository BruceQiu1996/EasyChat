namespace EasyChat.ViewModels
{
    public class MessageViewModel
    {
        public string MessageId { get; set; }
        public DateTime SendTime { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool FromSelf { get; set; }

        public MessageViewModel(string id, DateTime sendTime, string from, string to, bool fromSelf)
        {
            MessageId = id;
            SendTime = sendTime;
            From = from;
            To = to;
            FromSelf = fromSelf;
        }

        public virtual string GetDesc()
        {
            return "一条消息";
        }
    }
}
