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
            //TcpChannel tcpChannel = new TcpChannel(8989);
            //ChannelServices.RegisterChannel(tcpChannel, false);

            //RemotingConfiguration.RegisterWellKnownServiceType(typeof(CeChatRoomService), nameof(CeChatRoomService), WellKnownObjectMode.Singleton);

            // 配置文件模式
            RemotingConfiguration.Configure("CeChat.Host.exe.config", false);
            Console.ReadKey();
        }
    }
}
