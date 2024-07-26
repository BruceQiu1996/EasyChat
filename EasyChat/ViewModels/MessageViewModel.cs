using CommunityToolkit.Mvvm.ComponentModel;

namespace EasyChat.ViewModels
{
    public class MessageViewModel : ObservableObject
    {
        public string MessageId { get; set; }
        public DateTime SendTime { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool FromSelf { get; set; }

        private bool isSending = false;
        public bool IsSending 
        {
            get => isSending;
            set => SetProperty(ref isSending, value);
        }

        private bool isError = false;
        public bool IsError
        {
            get => isError;
            set => SetProperty(ref isError, value);
        }

        private string errrorMessage;
        public string ErrrorMessage
        {
            get => errrorMessage;
            set => SetProperty(ref errrorMessage, value);
        }

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
