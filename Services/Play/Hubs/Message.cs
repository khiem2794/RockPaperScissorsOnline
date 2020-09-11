using Play.Enum;
using Play.Models;

namespace Play.Hubs
{
    public class Message
    {
        public MessageType MessageType { get; set; }
        public GameUpdate Data { get; set; }
        public Message(MessageType type, GameUpdate data)
        {
            MessageType = type;
            Data = data;
        }
    }
}
