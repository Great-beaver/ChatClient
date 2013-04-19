using System;
using Chat.Main;

namespace ChatClient.Main
{
    public class MessageRecivedEventArgs : EventArgs
    {

        public MessageType MessageType { get; private set; }
        public string MessageText { get; private set; }
        public byte Sender { get; private set; }

        public MessageRecivedEventArgs(MessageType type, string text, byte sender)
        {
            MessageType = type;
            MessageText = text;
            Sender = sender;
        }
    }
}