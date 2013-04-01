using System;

namespace ChatClient.Main
{
    public class MessageRecivedEventArgs : EventArgs
    {
        public string MessageType { get; private set; }
        public string MessageText { get; private set; }

        public MessageRecivedEventArgs(string type, string text)
        {
            MessageType = type;
            MessageText = text;
        }
    }
}