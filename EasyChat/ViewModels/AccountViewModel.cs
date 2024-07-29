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
        public string SessionId { get; set; }
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
            AvatarColor = GetRandBrush(0, 130);
            Messages = new ObservableCollection<MessageViewModel>();
            LastChatTime = DateTime.Now.ToString("HH:mm");
            Desc = "暂无消息";
        }

        private string _lastChatTime;
        public string LastChatTime
        {
            get { return _lastChatTime; }
            set => SetProperty(ref _lastChatTime, value);
        }

        private int _unReadCounts;
        public int UnReadCounts
        {
            get { return _unReadCounts; }
            set => SetProperty(ref _unReadCounts, value);
        }

        private string? _unReadCountText;
        public string? UnReadCountText
        {
            get { return _unReadCountText; }
            set => SetProperty(ref _unReadCountText, value);
        }


        private DateTime _lastChatDateTime = default;

        private string _desc;
        public string Desc
        {
            get { return _desc; }
            set => SetProperty(ref _desc, value);
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

        /// <summary>
        /// 增加消息
        /// </summary>
        /// <param name="message"></param>
        public void AddMessage(MessageViewModel message, bool isCurrent)
        {
            //第一条消息加上时间
            if (Messages.Count <= 0)
            {
                Messages.Add(new TimeMessageViewModel(null, default, null, null, default, DateTime.Now.ToString("HH:mm")));
            }

            //隔两分钟的消息加上时间
            if (_lastChatDateTime != default && DateTime.Now.AddMinutes(-2) >= _lastChatDateTime)
            {
                Messages.Add(new TimeMessageViewModel(null, default, null, null, default, DateTime.Now.ToString("HH:mm")));
            }

            Messages.Add(message);
            _lastChatDateTime = message.SendTime;
            LastChatTime = message.SendTime.ToString("HH:mm");
            Desc = message.GetDesc();
            if (!isCurrent)
            {
                UnReadCounts++;
            }

            UnReadCountText = UnReadCounts == 0 ? null : UnReadCounts > 99 ? "99+" : UnReadCounts.ToString();
        }
    }
}
