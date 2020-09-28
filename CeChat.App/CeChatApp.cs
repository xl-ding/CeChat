using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CeChat.Service;

namespace CeChat.App
{
    public partial class CeChatApp : Form
    {
        public ICeChatRoomService CeChatRoomService { get; }

        public CeChatApp(ICeChatRoomService ceChatRoomService)
        {
            InitializeComponent();
            this.CeChatRoomService = ceChatRoomService;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string userName = this.txtUserName.Text.Trim();
            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("请输入用户名！", "提示", MessageBoxButtons.OK);
                return;
            }

            this.CeChatRoomService.Join(userName);
            FrmChatting frmChatting = new FrmChatting(this.CeChatRoomService, userName);
            frmChatting.ShowDialog();
            this.Close();
        }
    }
}
