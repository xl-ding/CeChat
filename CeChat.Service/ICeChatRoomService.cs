using System.Collections.Generic;
using CeChat.Model;

namespace CeChat.Service
{
    public interface ICeChatRoomService
    {
        List<string> UserList { get; set; }

        void Join(string userName);

        void Leave(string userName);

        void SendMessage(MessageInfo messageInfo);

        IEnumerable<MessageInfo> GetMessages();
    }
}
