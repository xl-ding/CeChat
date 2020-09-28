using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeChat.Model
{
    [Serializable]
    public class MessageInfo
    {
        public string UserName { get; set; }

        public DateTime MessageTime { get; set; }

        public string MsgContent { get; set; }
    }
}
