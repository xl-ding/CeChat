# 聊天室APP学习总结

## Remoting

### RPC

参考链接：[RPC](https://mp.weixin.qq.com/s/UYL8yD9lusl3ELPSiIDk_A)

RPC（Remote Procedure Call）：远程过程调用，它是一种通过网络从远程计算机程序上请求服务，而不需要了解底层网络技术的思想。也就是说客户端在请求服务端的数据或操作时，并不关心它的底层实现逻辑，只需要从远程服务对象上调用相应的属性或方法从而实现友好的通讯。

实现RPC主要有三个步骤：注册远程服务对象、数据处理、数据传输。客户端需要注册远程服务对象，将数据处理为服务端所能识别的二进制流（序列化），通过远程服务对象将处理后的数据传输给服务端。

RPC应用场景：主要针对一些业务量多的应用，通过将一些公共的业务逻辑抽离出来组成服务应用，共多个应用程序访问，从而实现数据友好共享的作用。

实现RPC的方式有很多，如Remoting，WebService，MQ等

![RPC框架](img\RPC框架.png)

### Remoting

参考链接：[Remoting](https://docs.microsoft.com/zh-cn/previous-versions/dotnet/netframework-4.0/kwdt6w2k(v%3Dvs.100))

Remoting为实现RPC的一种方式。其实现逻辑主要为在服务端创建一个公共的服务对象共享出去，共多个客户端访问和修改。

在服务端创建一个继承MarshalByRefObject的类对象用于构建远程服务对象，客户端通过注册该远程服务对象的实例从而访问和修改服务端数据。

服务端：

```C#
class Program
{
        static void Main(string[] args)
        {
            Console.WriteLine("This is sever!");

            // 注册信道
            TcpChannel tcpChannel = new TcpChannel(8989);
            ChannelServices.RegisterChannel(tcpChannel, false);

            // 注册服务端对象
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(CeChatRoomService), nameof(CeChatRoomService), WellKnownObjectMode.Singleton);

            // 配置文件模式
            //RemotingConfiguration.Configure("CeChat.Host.exe.config", false);
            Console.ReadKey();
        }
}
```

若要让其他应用程序域中的对象能够远程创建此对象的实例，必须生成宿主或侦听器应用程序来执行下列操作：

（1）选择并注册信道。信道是代表您处理网络协议和序列化格式的对象。

（2）在 .NET 远程处理系统中注册类型，以使它能够使用您的信道侦听类型请求。



客户端：

```C#
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
                object RemoteObj = Activator.GetObject(typeof(ICeChatRoomService), "tcp://localhost:8989/CeChatRoomService");
                return (ICeChatRoomService)RemoteObj;
            });

            // 添加User对象
            services.AddSingleton<User>();

            return services.BuildServiceProvider();
        }
}
```

客户端通过Activator.GetObject()调用远程服务，通过接口（ICeChatRoomService）反射远程服务对象（CeChatRoomService）

远程服务对象：

```C#
public class CeChatRoomService : MarshalByRefObject, ICeChatRoomService
{
        /// <summary>
        /// 消息列表
        /// </summary>
        private readonly List<MessageInfo> _messageList = new List<MessageInfo>();

        /// <summary>
        /// 用户列表
        /// </summary>
        public List<string> UserList
        {
            get;
            set;
        }

        /// <summary>
        /// 用户加入
        /// </summary>
        /// <param name="userName"></param>
        public void Join(string userName)
        {
            if (this.UserList == null)
            {
                this.UserList = new List<string>();
            }

            this.UserList.Add(userName);
        }

        /// <summary>
        /// 用户离开
        /// </summary>
        /// <param name="userName"></param>
        public void Leave(string userName)
        {
            this.UserList.Remove(userName);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public void ReceivingMessage(MessageInfo message)
        {
            this._messageList.Add(message);
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MessageInfo> GetMessages()
        {
            //Thread.Sleep(2000);
            return this._messageList;
        }
}
```





### IOC

参考链接：[IOC](https://mp.weixin.qq.com/s/W9L8jHFMV-yLUaRBKn4Adg)

控制反转（IoC）是面向对象编程中的一种设计原则，可以用来减低代码之间的耦合度。依赖注入（DI）是实现控制反转一种常见的方式。控制反转实质上就是把程序中用的所有对象统一放入容器中管理，调用对象实例的时候直接从容器中获取，使对象的创建都依赖于容器而非对象，从而减少对象与对象之间的耦合性。

最常见的依赖注入为构造器注入，即将服务对象的创建交给容器，构造器参数对象统一由构造器注入。

实现依赖注入主要有三步，注册服务，创建容器，创建对象。完成这几个步骤主要依赖于下述几个类对象：

IServiceCollection用于存储程序所用的服务对象和参数对象，即注册类对象；IServiceProvider就是容器，IServiceCollection注册好服务对象后，创建IServiceProvider，从而将服务对象统一放入容器中。

通过容器创建的出来的对象，根据不同的注入方式有三种生命周期：

Singleton(单例) ：整个容器的生命周期内是同一个对象；通过 services.AddSingleton()方法进行注册；

Scoped(作用域) ：在容器或子容器的生命周期内，对象保持一致，如果容器释放掉，那就意味着对象也会释放掉；通过 services.AddScoped()方法进行注册；

Transient(瞬时) ： 每次使用都会创建新的实例；通过 services.AddTransient()方法进行注册；

注：services 是  IServiceCollection services ；

具体代码实现，如下图示例：

![创建对象](img\创建容器.png)

![注册服务对象](img\注册服务对象.png)

![容器应用](img\容器应用.png)

