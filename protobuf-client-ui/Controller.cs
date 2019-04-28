using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf;

namespace protobufclientui
{
    public delegate void MessageReceivedHandler (IMessage message);

    public interface IMessageTunel
    {
        Dictionary<string, Type> GetParameterOptions ();

        void SendMessage (IMessage message);
        void SetIncommingMessageType (Type message_type);
        void Start (Dictionary<string, object> parameters);
        void Stop ();

        event MessageReceivedHandler OnMessageReceived;
    }

    public class Controller
    {
        Dictionary<string, Type> _types;
        readonly IUI _ui;
        IMessage _message;
        IMessageTunel _messageTunel;
        bool _messageTunelStarted;

        public Controller ()
        {
            _ui = new MainWindow (this);
            _ui.SetTunnelState (false);
            SelectMessageTunel (MessageTunels.First ());
            (_ui as MainWindow).Show ();
        }

        Dictionary<string, Type> GetMessageTunels ()
        {
            return new Dictionary<string, Type>
            {
                {"tcp", typeof(NetworkClient)},
                {"echo", typeof(EchoMessageTunel)}
            };
        }

        public IEnumerable<string> MessageTunels {
            get {
                return GetMessageTunels ().Keys;
            }
        }

        public void SelectMessageTunel (string name)
        {
            var tunels = GetMessageTunels ();

            if (_messageTunelStarted) {
                ToggleTunelState (null);
            }

            _messageTunel = Activator.CreateInstance (tunels [name]) as IMessageTunel;
            _messageTunel.OnMessageReceived += _ui.AddIncommingMessage;
            _ui.SetMessageTunel (_messageTunel.GetParameterOptions ());
        }

        void LoadTypes (Assembly assembly)
        {
            _types = new Dictionary<string, Type> ();
            foreach (var type in assembly.ExportedTypes) {
                _types [type.Name] = type;
            }
        }

        public void LoadFile (string protoFile, string generatorFile)
        {
            try {
                var assembly = new ProtobufCompiler (protoFile, generatorFile).GetAssembly ();
                LoadTypes (assembly);
                _ui.SetMessageTypes (MessageTypes);
            } catch (Exception ex) {
                _ui.LogError ("Failed to load proto file", ex.Message);
            }
        }

        public void SetOutcommingMessageType (string typeName)
        {
            _message = CreateMessageFromTypeName (typeName);
            _ui.LoadMessageType (_message);
        }

        internal void ToggleTunelState (Dictionary<string, object> parameters)
        {
            try {
                if (_messageTunelStarted) {
                    _messageTunel.Stop ();
                } else {
                    _messageTunel.Start (parameters);
                }
            } catch (Exception ex) {
                _ui.LogError ("Failed to change tunel state", ex.Message);
                return;
            }

            _messageTunelStarted = !_messageTunelStarted;
            _ui.SetTunnelState (_messageTunelStarted);
        }

        public void SetIncommingMessageType (string typeName)
        {
            _messageTunel.SetIncommingMessageType (_types [typeName]);
        }

        public IMessage CreateMessageFromTypeName (string typeName)
        {
            var message = Activator.CreateInstance (_types [typeName]) as IMessage;

            if (message == null) {
                _ui.LogError("Failed to create message instance", "Type " + typeName + " is not a valid Message type");
            }

            return message;
        }

        public Type GetTypeFromName (string typeName)
        {
            return _types [typeName];
        }

        List<Type> MessageTypes {
            get {
                var types = new List<Type> ();
                foreach (var type in _types) {
                    if (IsAssignableToGenericType (type.Value, typeof (IMessage<>))) {
                        types.Add (type.Value);
                    }
                }
                return types;
            }
        }

        public void SendMessage ()
        {
            if (!_messageTunelStarted) {
                _ui.LogError ("Failed to send message", "Message tunel was not started");
                return;
            }
            _messageTunel.SendMessage (_message);
        }

        private static bool IsAssignableToGenericType (Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces ();

            foreach (var it in interfaceTypes) {
                if (it.IsGenericType && it.GetGenericTypeDefinition () == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition () == genericType)
                return true;

            Type baseType = givenType.BaseType;
            return baseType != null && IsAssignableToGenericType (baseType, genericType);

        }

        internal void Dispose ()
        {
            (_ui as MainWindow).Dispose ();
        }
    }
}
