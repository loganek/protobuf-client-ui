using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace protobufclientui
{
    public interface IUI
    {
        void LoadMessageType(IMessage message);

        void AddIncommingMessage(IMessage message);

        void SetMessageTypes(List<Type> types);

        void LogError(string title, string message);

        void SetMessageTunel(Dictionary<string, Type> options);

        void SetTunnelState(bool started);
    }

}
