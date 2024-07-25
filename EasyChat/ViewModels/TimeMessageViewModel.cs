namespace EasyChat.ViewModels
{
    public class TimeMessageViewModel : MessageViewModel
    {
        public string Time { get; set; }

        public TimeMessageViewModel(string id, DateTime sendTime, string from, string to, bool fromSelf, string time) : base(id, sendTime, from, to, fromSelf)
        {
            Time = time;
        }
    }
}
