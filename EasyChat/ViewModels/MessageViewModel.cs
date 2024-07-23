namespace EasyChat.ViewModels
{
    public class MessageViewModel
    {
        public string MessageId { get; set; }
        public DateTime SendTime { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public virtual string GetDesc() 
        {
            return "一条消息";
        }
    }
}
