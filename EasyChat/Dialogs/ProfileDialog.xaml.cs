using ModernWpf.Controls;

namespace EasyChat.Dialogs
{
    /// <summary>
    /// Interaction logic for ProfileDialog.xaml
    /// </summary>
    public partial class ProfileDialog : ContentDialog
    {
        public ProfileDialog(ProfileDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
