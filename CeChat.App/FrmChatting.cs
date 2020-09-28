using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CeChat.Model;
using CeChat.Service;

namespace CeChat.App
{
    public partial class FrmChatting : Form
    {
        public string UserName { get; set; }

        public ICeChatRoomService ChatRoomService { get; set; }

        public FrmChatting(ICeChatRoomService chatRoomService, string userName)
        {
            InitializeComponent();
            this.ChatRoomService = chatRoomService;
            this.UserName = userName;
            this.RefreshTimer.Start();
        }

        public void Init()
        {
            var messageInfos = this.ChatRoomService.GetMessages();
            StringBuilder strText = new StringBuilder();
            foreach (var message in messageInfos)
            {
                strText.AppendLine(string.Format("{0}  {1} 说:", message.UserName, message.MessageTime));
                strText.AppendLine("    " + message.MsgContent);
                strText.AppendLine();
            }

            this.txtMessages.Text = strText.ToString();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string message = this.txtMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("发送内容不能为空!", "提示", MessageBoxButtons.OK);
            }

            MessageInfo messageInfo = new MessageInfo();
            messageInfo.UserName = this.UserName;
            messageInfo.MessageTime = DateTime.Now;
            messageInfo.MsgContent = message;
            this.ChatRoomService.ReceivingMessage(messageInfo);
            this.txtMessage.Text = string.Empty;
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            this.Init();
        }
    }
}
