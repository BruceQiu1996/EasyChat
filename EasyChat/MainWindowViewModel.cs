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

        private string title;
        public string Title
        {
            get { return title; }
            set => SetProperty(ref title, value);
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

            Title = "EasyChat";
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
            var tempAccount = Account;
            if (string.IsNullOrEmpty(Message))
                return;

            if (tempAccount == null)
                return;

            var message = new Message<SendTextMessagePacket>()
            {
                MessageType = MessageType.SendTextMessage,
                Body = new SendTextMessagePacket()
                {
                    Text = Message,
                    To = tempAccount.UserName,
                    From = _profileDialogViewModel.UserName
                }
            };

            var vm = new TextMessageViewModel(message.Body.MessageId, message.Body.SendTime,
                null, tempAccount.UserName, true, Message);
            vm.IsSending = true; //消息发送中
            tempAccount.AddMessage(vm);
            await _easyTcpClient.SendAsync(message.Serialize());

            Message = null;
        }

        private async Task SendImagesAsync()
        {
            var tempAccount = Account;
            if (tempAccount == null)
                return;

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

                        var bytes = await File.ReadAllBytesAsync(file);
                        var message = new Message<SendImageMessagePacket>()
                        {
                            MessageType = MessageType.SendImageMessage,
                            Body = new SendImageMessagePacket()
                            {
                                To = tempAccount.UserName,
                                From = _profileDialogViewModel.UserName,
                                Data = bytes,
                                FileName = Path.GetFileName(file)
                            }
                        };

                        var vm = new ImageMessageViewModel(message.Body.MessageId, message.Body.SendTime, null, tempAccount.UserName, true, bytes, message.Body.FileName);
                        vm.IsSending = true; //消息发送中
                        tempAccount.AddMessage(vm);
                        await _easyTcpClient.SendAsync(message.Serialize());
                    }
                }
            }
        }

        /// <summary>
        /// 发送消息异常时
        /// </summary>
        private void SendMessageError(IMsssage<ReceiveMessagePacket> packet)
        {
            if (packet.GetBody().FromSelf)
            {
                var account = Accounts.FirstOrDefault(x => x.UserName == packet.GetBody().To);
                if (account != null)
                {
                    var message = account.Messages.FirstOrDefault(x => x.MessageId == packet.GetBody().MessageId);
                    if (message != null)
                    {
                        message.IsSending = false;
                        message.ErrrorMessage = packet.GetBody().Reason;
                        message.IsError = true;
                    }
                }
            }
        }

        /// <summary>
        /// 发送消息正常时
        /// </summary>
        private void SendMessageNormal(IMsssage<ReceiveMessagePacket> packet)
        {
            if (packet.GetBody().FromSelf)
            {
                var account = Accounts.FirstOrDefault(x => x.UserName == packet.GetBody().To);
                if (account != null)
                {
                    var message = account.Messages.FirstOrDefault(x => x.MessageId == packet.GetBody().MessageId);
                    if (message != null)
                    {
                        message.IsSending = false;
                    }
                }
            }
            else
            {
                var account = Accounts.FirstOrDefault(x => x.UserName == packet.GetBody().From);
                if (account != null)
                {
                    if (packet.GetBody() is ReceiveTextMessagePacket)
                    {
                        var temp = packet.GetBody() as ReceiveTextMessagePacket;
                        account.AddMessage(new TextMessageViewModel(temp.MessageId, temp.SendTime, temp.From, temp.To, temp.FromSelf, temp.Text));
                    }

                    if (packet.GetBody() is ReceiveImageMessagePacket)
                    {
                        var temp = packet.GetBody() as ReceiveImageMessagePacket;
                        account.AddMessage(new ImageMessageViewModel(temp.MessageId, temp.SendTime, temp.From, temp.To, temp.FromSelf, temp.Data, temp.FileName));
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
                case MessageType.RegisterResult:
                    {
                        var packet = Message<RegisterResultPacket>.FromBytes(data);
                        if (packet.Body.Success)
                        {
                            Title = $"EasyChat - {_profileDialogViewModel.UserName}";
                        }
                        else
                        {
                            MessageBox.Show($"登录注册失败:{packet.Body.Reason}", "异常", MessageBoxButton.OK, MessageBoxImage.Error);
                            await LoadAsync();
                        }
                    }
                    break;
                case MessageType.OnlineAccountsList:
                    {
                        var packet = Message<OnlineAccountListPacket>.FromBytes(data);
                        foreach (var item in packet.Body.Accounts)
                        {
                            if (Accounts.FirstOrDefault(x => x.UserName == item.Value) == null)
                            {
                                Accounts.Insert(0, new AccountViewModel(item.Value, item.Key));
                            }
                            else 
                            {
                                var account = Accounts.FirstOrDefault(x => x.UserName == item.Value);
                                account.SessionId = item.Key;
                            }
                        }
                    }
                    break;

                case MessageType.ReceiveTextMessage:
                    {
                        var packet = Message<ReceiveTextMessagePacket>.FromBytes(data);
                        if (packet.Body.Success)
                        {
                            SendMessageNormal(packet);
                        }
                        else
                        {
                            SendMessageError(packet);
                        }
                    }
                    break;

                case MessageType.ReceiveImageMessage:
                    {
                        var packet = Message<ReceiveImageMessagePacket>.FromBytes(data);
                        if (packet.Body.Success)
                        {
                            SendMessageNormal(packet);
                        }
                        else
                        {
                            SendMessageError(packet);
                        }
                    }
                    break;
            }
        }
    }
}
