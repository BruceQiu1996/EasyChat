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
        static void Main(string[] args)
        {
            _easyTcpServer = new EasyTcpServer(_port);
            _easyTcpServer.StartListen();

            _easyTcpServer.SetReceiveFilter(new FixedHeaderPackageFilter(8, 0, 4, false));
            _easyTcpServer.OnReceivedData += async (obj, e) =>
            {
                await HandleMessageAsync(e.Data, e.Session);
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
                        var packet = Message<RegisterPacket>.FromBytes(data);
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

                        //告知在线的人，更新在线用户列表
                        //如果需要高并发的情况下1.这里的注册处理需要加信号量或者锁。2.发送用户列表的动作需要在队列中处理，防止后者覆盖前者
                        await UpdateOnlineAccountsToClientAsync();
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
