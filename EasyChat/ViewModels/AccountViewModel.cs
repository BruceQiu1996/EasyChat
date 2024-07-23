using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace EasyChat.ViewModels
{
    /// <summary>
    /// 好友
    /// </summary>
    public class AccountViewModel : ObservableObject
    {
        public string UserName { get; private set; }
        public string SessionId { get; private set; }

        public AccountViewModel(string userName, string sessionid)
        {
            UserName = userName;
            SessionId = sessionid;
        }

        private string _lastChatTime;
        public string LastChatTime
        {
            get { return _lastChatTime; }
            set => SetProperty(ref _lastChatTime, value);
        }

        public ObservableCollection<MessageViewModel> Messages { get; set; } = new ObservableCollection<MessageViewModel>();
    }
}
