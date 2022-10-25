using System;

namespace GamblePyon {
    public class MessageEventArgs : EventArgs {
        public string Message { get; private set; }
        public MessageType MessageType { get; private set; }

        public MessageEventArgs(string message, MessageType messageType) {
            Message = message;
            MessageType = messageType;
        }
    }

    public enum MessageType {
        Normal, BlackjackRoll
    }
}
