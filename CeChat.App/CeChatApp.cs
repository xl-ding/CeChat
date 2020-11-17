using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CeChat.Model;
using CeChat.Service;
using Microsoft.Extensions.DependencyInjection;

namespace CeChat.App
{
    public partial class CeChatApp : Form
    {
        public ICeChatRoomService CeChatRoomService { get; }

        /// <summary>
        /// 异步加入
        /// </summary>
        /// <param name="userName">用户名</param>
        public delegate void AsyncJoin(string userName);

        public CeChatApp(ICeChatRoomService ceChatRoomService)
        {
            InitializeComponent();
            this.CeChatRoomService = ceChatRoomService;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string userName = this.txtUserName.Text.Trim();
            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("请输入用户名！", "提示", MessageBoxButtons.OK);
                return;
            }

            AsyncJoin asyncJoin = new AsyncJoin(this.CeChatRoomService.Join);
            await Task.Factory.FromAsync(asyncJoin.BeginInvoke(userName, null, null), asyncJoin.EndInvoke);
            //this.CeChatRoomService.Join(userName);
            //FrmChatting frmChatting = new FrmChatting(this.CeChatRoomService, userName);
            //this.Visible = false;
            //frmChatting.ShowDialog();
            //frmChatting.Show();

            IServiceProvider serviceProvider = ConfigureServices(new ServiceCollection());
            this.Visible = false;
            serviceProvider.GetRequiredService<FrmChatting>().Show();
            //this.Dispose();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            string userName = this.txtUserName.Text.Trim();
            services.AddTransient<FrmChatting>();
            services.AddTransient<ICeChatRoomService>(sp =>
            {
                return this.CeChatRoomService;
            });

            services.AddTransient<User>(sp =>
            {
                return new User() { UserName = userName };
            });

            return services.BuildServiceProvider();
        }

        private void TxtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.BtnLogin_Click(sender, e);
            }
        }
    }
}
