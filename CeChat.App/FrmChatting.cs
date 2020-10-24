using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;
using CeChat.Model;
using CeChat.Service;
using System.Threading.Tasks;

namespace CeChat.App
{
    public partial class FrmChatting : Form
    {
        public delegate IEnumerable<MessageInfo> AsyncRefresh();

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
                return;
            }

            MessageInfo messageInfo = new MessageInfo();
            messageInfo.UserName = this.UserName;
            messageInfo.MessageTime = DateTime.Now;
            messageInfo.MsgContent = message;
            this.ChatRoomService.ReceivingMessage(messageInfo);
            this.txtMessage.Text = string.Empty;
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            AsyncRefresh asyncrefresh = new AsyncRefresh(ChatRoomService.GetMessages); // 1/2

            //asyncrefresh.BeginInvoke(CallBack, null); // 1.AMP

            // 2.TAP
            IEnumerable<MessageInfo> messageInfos = await Task.Factory.FromAsync(asyncrefresh.BeginInvoke, asyncrefresh.EndInvoke, null);

            StringBuilder strText = new StringBuilder();
            foreach (var message in messageInfos)
            {
                strText.AppendLine(string.Format("{0}  {1} 说:", message.UserName, message.MessageTime));
                strText.AppendLine("    " + message.MsgContent);
                strText.AppendLine();
            }

            this.txtMessages.Text = strText.ToString();
        }

        public void CallBack(IAsyncResult result)
        {
            //AsyncRefresh asyncRefresh = result.AsyncState as AsyncRefresh;这一句和下面两句作用一致
            AsyncResult asyncResult = result as AsyncResult;
            AsyncRefresh asyncRefresh = asyncResult.AsyncDelegate as AsyncRefresh;

            var messageInfos = asyncRefresh.EndInvoke(result);

            StringBuilder strText = new StringBuilder();
            foreach (var message in messageInfos)
            {
                strText.AppendLine(string.Format("{0}  {1} 说:", message.UserName, message.MessageTime));
                strText.AppendLine("    " + message.MsgContent);
                strText.AppendLine();
            }

            Action action = () => { this.txtMessages.Text = strText.ToString(); };
            this.txtMessages.Invoke(action);
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.BtnSend_Click(sender, e);
            }
        }
    }
}
