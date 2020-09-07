using Play.Enum;

namespace Play.Hubs
{
    public class Message
    {
        public MessageType MessageType { get; set; }
        public object Data { get; set; }
        public Message(MessageType type, object data)
        {
            MessageType = type;
            Data = data;
        }
    }
}
