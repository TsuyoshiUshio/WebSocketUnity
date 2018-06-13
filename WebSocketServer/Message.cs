using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocketServer
{
    public abstract class Message
    {
        public string MessageType { get; set; }
    }

    public class JoinMessage : Message
    {
        public const string MessageTypeKeyword = "JoinMessage";
        public string UserName { get; set; }
    }

    public class ChatMessage
    {
        public string UserName { get; set; }
        public ArraySegment<byte> Message { get; set; }
        public DateTimeOffset RecieveTime { get; set; }
    }
}
