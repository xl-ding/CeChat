using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CeChat.Model;
using CeChat.Service;

namespace CeChat.App
{
    public partial class FrmChatting : Form
    {
        /// <summary>
        /// 异步刷新
        /// </summary>
        /// <returns>消息列表</returns>
        public delegate IEnumerable<MessageInfo> AsyncRefresh();

        /// <summary>
        /// 异步离开聊天室
        /// </summary>
        /// <param name="userName">用户名</param>
        public delegate void AsyncLeave(string userName);

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 聊天室
        /// </summary>
        public ICeChatRoomService ChatRoomService { get; set; }

        public FrmChatting(ICeChatRoomService chatRoomService, string userName)
        {
            InitializeComponent();
            this.ChatRoomService = chatRoomService;
            this.UserName = userName;
            this.FormClosed += FrmChatting_FormClosed;
            this.RefreshTimer.Start();
        }

        private async void FrmChatting_FormClosed(object sender, FormClosedEventArgs e)
        {
            AsyncLeave asyncLeave = new AsyncLeave(this.ChatRoomService.Leave);
            await Task.Factory.FromAsync(asyncLeave.BeginInvoke(this.UserName, null, null), asyncLeave.EndInvoke);
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

        private async void BtnSend_Click(object sender, EventArgs e)
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
            Action<MessageInfo> sendMessage = (MessageInfo info) =>
            {
                this.ChatRoomService.ReceivingMessage(info);
            };

            await Task.Factory.FromAsync(sendMessage.BeginInvoke(messageInfo, null, null), sendMessage.EndInvoke);
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
