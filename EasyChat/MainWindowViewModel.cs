using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyChat.Common;
using EasyChat.Dialogs;
using EasyChat.ViewModels;
using EasyTcp4Net;
using ModernWpf.Controls;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Windows;

namespace EasyChat
{
    public class MainWindowViewModel : ObservableObject
    {
        private ObservableCollection<AccountViewModel> accounts;
        public ObservableCollection<AccountViewModel> Accounts
        {
            get { return accounts; }
            set => SetProperty(ref accounts, value);
        }

        private AccountViewModel account;
        public AccountViewModel Account
        {
            get { return account; }
            set => SetProperty(ref account, value);
        }

        public AsyncRelayCommand LoadCommandAsync { get; set; }

        private readonly EasyTcpClient _easyTcpClient;
        private readonly ProfileDialog _profileDialog;
        private readonly ProfileDialogViewModel _profileDialogViewModel;
        public MainWindowViewModel(ProfileDialog profileDialog, ProfileDialogViewModel profileDialogViewModel)
        {
            _profileDialog = profileDialog;
            _profileDialogViewModel = profileDialogViewModel;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            Accounts = new ObservableCollection<AccountViewModel>();
            _easyTcpClient = new EasyTcpClient("127.0.0.1", 50055, new EasyTcpClientOptions()
            {
                ConnectRetryTimes = 3,
                BufferSize = 8 * 1024,
                MaxPipeBufferSize = int.MaxValue
            });
            _easyTcpClient.SetReceiveFilter(new FixedHeaderPackageFilter(8, 0, 4, false));
            _easyTcpClient.OnReceivedData += async (obj, e) =>
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await HandleMessageAsync(e.Data);
                });
            };
        }

        private async Task LoadAsync()
        {
            try
            {
                await _easyTcpClient.ConnectAsync();
                var result = await _profileDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    //注册名字和session绑定
                    await _easyTcpClient.SendAsync(new Message<RegisterPacket>()
                    {
                        MessageType = MessageType.Register,
                        Body = new RegisterPacket()
                        {
                            UserName = _profileDialogViewModel.UserName
                        }
                    }.Serialize());
                }
            }
            catch (Exception ex)
            {
                //初始化失败
            }
        }

        private async Task HandleMessageAsync(ReadOnlyMemory<byte> data)
        {
            var type = (MessageType)BinaryPrimitives.ReadInt32BigEndian(data.Slice(4, 4).Span);
            switch (type)
            {
                case MessageType.OnlineAccountsList:
                    {
                        var packet = Message<OnlineAccountListPacket>.FromBytes(data);
                        foreach (var item in packet.Body.Accounts)
                        {
                            if (Accounts.FirstOrDefault(x => x.SessionId == item.Key) == null)
                            {
                                Accounts.Insert(0, new AccountViewModel(item.Value, item.Key));
                            }
                        }
                    }
                    break;
            }
        }
    }
}
