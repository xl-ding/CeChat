using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace CeChat.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This is sever!");

            // 注册信道
            TcpChannel tcpChannel = new TcpChannel(8989);
            ChannelServices.RegisterChannel(tcpChannel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(CeChatRoomService), nameof(CeChatRoomService), WellKnownObjectMode.Singleton);

            Console.ReadKey();
        }
    }
}
