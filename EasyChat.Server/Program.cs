using EasyChat.Common;
using EasyTcp4Net;
using System.Buffers.Binary;
using System.Collections.Concurrent;

namespace EasyChat.Server
{
    internal class Program
    {
        private static readonly ushort _port = 50055;
        private static EasyTcpServer _easyTcpServer;
        private static readonly ConcurrentDictionary<string, Account> _accounts = new ConcurrentDictionary<string, Account>();
        private static readonly Semaphore _semaphore = new Semaphore(1, 1);
        static void Main(string[] args)
        {
            _easyTcpServer = new EasyTcpServer(_port);
            _easyTcpServer.StartListen();

            _easyTcpServer.SetReceiveFilter(new FixedHeaderPackageFilter(8, 0, 4, false));
            _easyTcpServer.OnReceivedData += async (obj, e) =>
            {
                await HandleMessageAsync(e.Data, e.Session);
            };
            _easyTcpServer.OnClientConnectionChanged += (obj, e) =>
            {
                if (e.Status == ConnectsionStatus.DisConnected)
                {
                    _accounts.TryRemove(e.ClientSession.SessionId, out var account);
                }
            };

            Console.ReadLine();
        }

        async static Task HandleMessageAsync(ReadOnlyMemory<byte> data, ClientSession clientSession)
        {
            var type = (MessageType)BinaryPrimitives.ReadInt32BigEndian(data.Slice(4, 4).Span);
            switch (type)
            {
                case MessageType.Register:
                    {
                        try
                        {
                            _semaphore.WaitOne();
                            var packet = Message<RegisterPacket>.FromBytes(data);
                            var exist = _accounts.FirstOrDefault(x => x.Value.UserName == packet.Body.UserName).Value;
                            if (exist != null)
                            {
                                await _easyTcpServer.SendAsync(clientSession, new Message<RegisterResultPacket>()
                                {
                                    MessageType = MessageType.RegisterResult,
                                    Body = new RegisterResultPacket()
                                    {
                                        Success = false,
                                        Reason = "已有同名用户在线"
                                    }
                                }.Serialize());
                            }
                            else
                            {
                                _accounts.AddOrUpdate(clientSession.SessionId, new Account()
                                {
                                    UserName = packet.Body.UserName,
                                    Session = clientSession
                                }, (x, y) =>
                                {
                                    return new Account()
                                    {
                                        UserName = packet.Body.UserName,
                                        Session = clientSession
                                    };
                                });

                                await _easyTcpServer.SendAsync(clientSession, new Message<RegisterResultPacket>()
                                {
                                    MessageType = MessageType.RegisterResult,
                                    Body = new RegisterResultPacket()
                                    {
                                        Success = true
                                    }
                                }.Serialize());

                                //告知在线的人，更新在线用户列表
                                //如果需要高并发的情况下1.这里的注册处理需要加信号量或者锁。2.发送用户列表的动作需要在队列中处理，防止后者覆盖前者
                                await UpdateOnlineAccountsToClientAsync();
                            }

                        }
                        catch (Exception ex)
                        {
                            await _easyTcpServer.SendAsync(clientSession, new Message<RegisterResultPacket>()
                            {
                                MessageType = MessageType.RegisterResult,
                                Body = new RegisterResultPacket()
                                {
                                    Success = false,
                                    Reason = "服务器异常"
                                }
                            }.Serialize());
                        }
                        finally 
                        {
                            _semaphore.Release();
                        }
                    }
                    break;
                case MessageType.SendTextMessage:
                    {
                        //await Task.Delay(2000);//测试消息延迟发送后发起者的消息加载状态
                        var packet = Message<SendTextMessagePacket>.FromBytes(data);
                        var to = _accounts.FirstOrDefault(x => x.Value.UserName == packet.Body.To);
                        var from = _accounts.FirstOrDefault(x => x.Value.UserName == packet.Body.From);
                        if (to.Key != null && from.Key != null)
                        {
                            await _easyTcpServer.SendAsync(clientSession, new Message<ReceiveTextMessagePacket>()
                            {
                                MessageType = MessageType.ReceiveTextMessage,
                                Body = new ReceiveTextMessagePacket().FromMessage(packet.Body,true)
                            }.Serialize());

                            if (to.Key != clientSession.SessionId) //不是自己发自己的
                            {
                                await _easyTcpServer.SendAsync(to.Key, new Message<ReceiveTextMessagePacket>()
                                {
                                    MessageType = MessageType.ReceiveTextMessage,
                                    Body = new ReceiveTextMessagePacket().FromMessage(packet.Body, false)
                                }.Serialize());
                            }
                        }
                        else if (to.Key == null) //对方可能下线
                        {
                            await _easyTcpServer.SendAsync(clientSession, new Message<ReceiveTextMessagePacket>()
                            {
                                MessageType = MessageType.ReceiveTextMessage,
                                Body = new ReceiveTextMessagePacket().FromMessage(packet.Body, true,false,"对方已下线")
                            }.Serialize());
                        }
                        else 
                        {
                            await _easyTcpServer.SendAsync(clientSession, new Message<ReceiveTextMessagePacket>()
                            {
                                MessageType = MessageType.ReceiveTextMessage,
                                Body = new ReceiveTextMessagePacket().FromMessage(packet.Body, true, false, "状态异常,请重新登录")
                            }.Serialize());
                        }
                    }
                    break;

                case MessageType.SendImageMessage:
                    {
                        var packet = Message<SendImageMessagePacket>.FromBytes(data);
                        var to = _accounts.FirstOrDefault(x => x.Value.UserName == packet.Body.To);
                        var from = _accounts.FirstOrDefault(x => x.Value.UserName == packet.Body.From);

                        if (to.Key != null && from.Key != null)
                        {
                            await _easyTcpServer.SendAsync(clientSession, new Message<ReceiveImageMessagePacket>()
                            {
                                MessageType = MessageType.ReceiveImageMessage,
                                Body = new ReceiveImageMessagePacket().FromMessage(packet.Body, true)
                            }.Serialize());

                            if (to.Key != clientSession.SessionId) //不是自己发自己的
                            {
                                await _easyTcpServer.SendAsync(to.Key, new Message<ReceiveImageMessagePacket>()
                                {
                                    MessageType = MessageType.ReceiveImageMessage,
                                    Body = new ReceiveImageMessagePacket().FromMessage(packet.Body, false)
                                }.Serialize());
                            }
                        }
                        else if (to.Key == null) //对方可能下线
                        {
                            await _easyTcpServer.SendAsync(clientSession, new Message<ReceiveImageMessagePacket>()
                            {
                                MessageType = MessageType.ReceiveImageMessage,
                                Body = new ReceiveImageMessagePacket().FromMessage(packet.Body, true, false, "对方已下线")
                            }.Serialize());
                        }
                        else 
                        {
                            await _easyTcpServer.SendAsync(clientSession, new Message<ReceiveImageMessagePacket>()
                            {
                                MessageType = MessageType.ReceiveImageMessage,
                                Body = new ReceiveImageMessagePacket().FromMessage(packet.Body, true, false, "状态异常,请重新登录")
                            }.Serialize());
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 通知客户端目前在线的用户列表
        /// </summary>
        /// <returns></returns>
        async static Task UpdateOnlineAccountsToClientAsync()
        {
            await Parallel.ForEachAsync(_accounts, async (x, y) =>
            {
                try
                {
                    await _easyTcpServer.SendAsync(x.Key, new Message<OnlineAccountListPacket>()
                    {
                        MessageType = MessageType.OnlineAccountsList,
                        Body = new OnlineAccountListPacket()
                        {
                            Accounts = _accounts
                            .ToDictionary(x => x.Key, x => x.Value.UserName)
                        }
                    }.Serialize());
                }
                catch
                {

                }
            });
        }
    }
}
