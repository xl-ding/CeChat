using System;
using System.Threading.Tasks;
using System.Windows.Forms;
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

            IServiceProvider serviceProvider = ConfigureServices(new ServiceCollection());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(serviceProvider.GetRequiredService<FrmChatting>());

            this.Visible = false;

            //frmChatting.ShowDialog();
            //frmChatting.Show();
            //this.Dispose();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            string userName = this.txtUserName.Text.Trim();
            services.AddTransient<FrmChatting>();
            services.AddTransient<ICeChatRoomService,string>(sp =>
            {
                return new { this.CeChatRoomService, userName };
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
