using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyChat.Common;
using EasyChat.Dialogs;
using EasyChat.ViewModels;
using EasyTcp4Net;
using Microsoft.Win32;
using ModernWpf.Controls;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.IO;
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

        private string message;
        public string Message
        {
            get { return message; }
            set => SetProperty(ref message, value);
        }

        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand SendMessageCommandAsync { get; set; }
        public AsyncRelayCommand SendImagesCommandAsync { get; set; }

        private readonly EasyTcpClient _easyTcpClient;
        private readonly ProfileDialog _profileDialog;
        private readonly ProfileDialogViewModel _profileDialogViewModel;
        public MainWindowViewModel(ProfileDialog profileDialog, ProfileDialogViewModel profileDialogViewModel)
        {
            _profileDialog = profileDialog;
            _profileDialogViewModel = profileDialogViewModel;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            SendMessageCommandAsync = new AsyncRelayCommand(SendMessageAsync);
            SendImagesCommandAsync = new AsyncRelayCommand(SendImagesAsync);
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

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <returns></returns>
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(Message))
                return;

            if (Account == null)
                return;

            await _easyTcpClient.SendAsync(new Message<SendTextMessagePacket>()
            {
                MessageType = MessageType.SendTextMessage,
                Body = new SendTextMessagePacket()
                {
                    Text = Message,
                    To = Account.SessionId,
                }
            }.Serialize());

            Message = null;
        }

        private async Task SendImagesAsync() 
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "图片文件 (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif" // 设置文件过滤器  
            };

            if (openFileDialog.ShowDialog() == true)
            {
                List<string> selectedFiles = new List<string>(openFileDialog.FileNames);

                if (selectedFiles.Count > 3)
                {
                    MessageBox.Show("请选择不超过3张图片！");
                }
                else
                {
                    foreach (var file in selectedFiles)
                    {
                        await _easyTcpClient.SendAsync(new Message<SendImageMessagePacket>()
                        {
                            MessageType = MessageType.SendImageMessage,
                            Body = new SendImageMessagePacket() 
                            {
                                To = Account.SessionId,
                                Data = await File.ReadAllBytesAsync(file),
                                FileName = file
                            }
                        }.Serialize());
                    }
                }
            }
        }

        /// <summary>
        /// 处理收到的信息数据
        /// </summary>
        /// <param name="data">收到的信息</param>
        /// <returns></returns>
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

                case MessageType.ReceiveTextMessage:
                    {
                        var packet = Message<ReceiveTextMessagePacket>.FromBytes(data);
                        if (packet.Body.FromSelf)
                        {
                            var account = Accounts.FirstOrDefault(x => x.SessionId == packet.Body.To);
                            if (account != null)
                            {
                                account.AddMessage(new TextMessageViewModel(packet.Body.MessageId,
                                    packet.Body.SendTime, packet.Body.From, packet.Body.To, packet.Body.FromSelf,
                                    packet.Body.Text));
                            }
                        }
                        else 
                        {
                            var account = Accounts.FirstOrDefault(x => x.SessionId == packet.Body.From);
                            if (account != null)
                            {
                                account.AddMessage(new TextMessageViewModel(packet.Body.MessageId,
                                    packet.Body.SendTime, packet.Body.From, packet.Body.To, packet.Body.FromSelf,
                                    packet.Body.Text));
                            }
                        }
                    }
                    break;

                case MessageType.ReceiveImageMessage:
                    {
                        var packet = Message<ReceiveImageMessagePacket>.FromBytes(data);
                        if (packet.Body.FromSelf)
                        {
                            var account = Accounts.FirstOrDefault(x => x.SessionId == packet.Body.To);
                            if (account != null)
                            {
                                account.AddMessage(new ImageMessageViewModel(packet.Body.MessageId,
                                    packet.Body.SendTime, packet.Body.From, packet.Body.To, packet.Body.FromSelf,
                                    packet.Body.Data,packet.Body.FileName));
                            }
                        }
                        else
                        {
                            var account = Accounts.FirstOrDefault(x => x.SessionId == packet.Body.From);
                            if (account != null)
                            {
                                account.AddMessage(new ImageMessageViewModel(packet.Body.MessageId,
                                    packet.Body.SendTime, packet.Body.From, packet.Body.To, packet.Body.FromSelf,
                                    packet.Body.Data, packet.Body.FileName));
                            }
                        }
                    }
                    break;
            }
        }
    }
}
