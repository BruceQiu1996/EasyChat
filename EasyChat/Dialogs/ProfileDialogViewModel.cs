using CommunityToolkit.Mvvm.ComponentModel;

namespace EasyChat.Dialogs
{
    public class ProfileDialogViewModel : ObservableObject
    {
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set => SetProperty(ref _userName, value);
        }
    }
}
