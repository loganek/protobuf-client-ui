using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Google.Protobuf;

namespace protobufclientui
{
    public class NetworkClient : IMessageTunel
    {
        readonly TcpClient _client;
        readonly Thread _th;
        Type _baseType;

        public event MessageReceivedHandler OnMessageReceived;

        public NetworkClient ()
        {
            _client = new TcpClient();
            _th = new Thread (new ThreadStart (Run));
        }

        public void SendMessage(IMessage message)
        {
            var size = (uint) message.CalculateSize ();
            var bytes = BitConverter.GetBytes (size);
            _client.GetStream ().Write (bytes, 0, bytes.Length);
            var msg = message.ToByteArray ();
            _client.GetStream ().Write (msg, 0, msg.Length);
        }

        void Run()
        {
            while (_client.Connected) {
                uint? size = ReadHeader ();
                if (size == null)
                {
                    Disconnect();
                    continue;
                }

                var buff = new byte[size.Value];
                var d = _client.GetStream ().Read (buff, 0, buff.Length); // TODO handle return value
                if (_baseType == null) {
                    continue;
                }
                var instance = Activator.CreateInstance (_baseType) as IMessage;
                instance.MergeFrom (buff);
                OnMessageReceived?.Invoke (instance);
            }
        }

        uint? ReadHeader ()
        {
            var buff = new byte[4];
            var size = _client.GetStream ().Read (buff, 0, buff.Length); // TODO properly handle return value
            if (size == 0)
            {
                return null;
            }

            return BitConverter.ToUInt32 (buff, 0);
        }

        public Dictionary<string, Type> GetParameterOptions()
        {
            return new Dictionary<string, Type> {
                { "hostname", typeof(string) },
                { "port", typeof(int)}
            };
        }

        public void Start(Dictionary<string, object> parameters)
        {
            if (_client.Connected)
            {
                return;
            }

            var hostname = parameters["hostname"] as string;
            int port = Convert.ToInt32(parameters["port"]);
            _client.Connect(hostname, port);
            _th.Start();
        }

        public void Stop()
        {
            Disconnect();

            _th.Join();
        }

        void Disconnect()
        {
            _client.GetStream().Close();
            _client.Close();
        }

        public void SetIncommingMessageType(Type message_type)
        {
            _baseType = message_type;
        }
    }
}

