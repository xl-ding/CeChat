using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;
using CeChat.Model;
using CeChat.Service;
using Microsoft.Extensions.DependencyInjection;

namespace CeChat.App
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            IServiceProvider serviceProvider = ConfigureServices(new ServiceCollection());

            // 注册信道
            TcpChannel tcpChannel = new TcpChannel(0);
            ChannelServices.RegisterChannel(tcpChannel, false);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(serviceProvider.GetRequiredService<CeChatApp>());
        }

        /// <summary>
        /// 获取服务对象
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务对象集合</returns>
        public static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // 添加CeChatApp对象
            services.AddTransient<CeChatApp>();

            // 添加FrmChatting对象
            services.AddTransient<FrmChatting>();

            // 添加远程服务对象
            services.AddSingleton<ICeChatRoomService>(sp =>
            {
                // todo 创建远程对象
                //RemotingConfiguration.Configure("CeChat.App.exe.config", false);
                object RemoteObj = Activator.GetObject(typeof(ICeChatRoomService), "tcp://localhost:8989/CeChatRoomService");
                return (ICeChatRoomService)RemoteObj;
            });

            // 添加User对象
            services.AddSingleton<User>();

            return services.BuildServiceProvider();
        }
    }
}
