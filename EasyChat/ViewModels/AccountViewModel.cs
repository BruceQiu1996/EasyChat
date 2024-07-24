using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace EasyChat.ViewModels
{
    /// <summary>
    /// 好友
    /// </summary>
    public class AccountViewModel : ObservableObject
    {
        public string UserName { get; private set; }
        public string SessionId { get; private set; }
        private Brush avatarColor;
        public Brush AvatarColor
        {
            get => avatarColor;
            set => SetProperty(ref avatarColor, value);
        }
        public string AvatarText => UserName.Substring(0, 1);

        public AccountViewModel(string userName, string sessionid)
        {
            UserName = userName;
            SessionId = sessionid;
            AvatarColor = GetRandBrush(125);
            Messages = new ObservableCollection<MessageViewModel>();
            LastChatTime = DateTime.Now.ToString("HH:mm");

            AddMessage(new TextMessageViewModel()
            {
                Text = "123213132132132133123今晚吃饭了吗？？？？",
                FromSelf = true,
                SendTime = DateTime.Now,
            });

            AddMessage(new TextMessageViewModel()
            {
                Text = "没有吃捏?",
                FromSelf = false,
                SendTime = DateTime.Now
            });
        }

        private string _lastChatTime;
        public string LastChatTime
        {
            get { return _lastChatTime; }
            set => SetProperty(ref _lastChatTime, value);
        }

        private ObservableCollection<MessageViewModel> messages;
        public ObservableCollection<MessageViewModel> Messages
        {
            get => messages;
            set => SetProperty(ref messages, value);
        }

        private Color GetRandColor(int start = 0, int end = 256)
        {
            Random random = new Random();
            byte red = Convert.ToByte(random.Next(start, end));
            byte green = Convert.ToByte(random.Next(start, end));
            byte blue = Convert.ToByte(random.Next(start, end));
            Color color = Color.FromArgb(255, red, green, blue);

            return color;

        }

        private Brush GetRandBrush(int start = 0, int end = 256)
        {
            return new SolidColorBrush(GetRandColor(start, end));
        }

        private void AddMessage(MessageViewModel message) 
        {
            Messages.Add(message);
            LastChatTime = message.SendTime.ToString("HH:mm");
        }
    }
}
