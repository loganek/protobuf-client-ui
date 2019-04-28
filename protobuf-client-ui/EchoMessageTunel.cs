using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace protobufclientui
{
    public class EchoMessageTunel : IMessageTunel
    {
        private bool _started;
        public event MessageReceivedHandler OnMessageReceived;

        public Dictionary<string, Type> GetParameterOptions()
        {
            return new Dictionary<string, Type>();
        }

        public void SendMessage(IMessage message)
        {
            if (!_started)
            {
                return;
            }
            OnMessageReceived?.Invoke(message);
        }

        public void SetIncommingMessageType(Type message_type)
        {
        }

        public void Start(Dictionary<string, object> parameters)
        {
            _started = true;
        }

        public void Stop()
        {
            _started = false;
        }
    }
}
