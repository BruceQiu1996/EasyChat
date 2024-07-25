using ModernWpf.Controls;

namespace EasyChat.Dialogs
{
    /// <summary>
    /// Interaction logic for ProfileDialog.xaml
    /// </summary>
    public partial class ProfileDialog : ContentDialog
    {
        private readonly ProfileDialogViewModel _profileDialogViewModel;
        public ProfileDialog(ProfileDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _profileDialogViewModel = viewModel;
        }
    }
}
