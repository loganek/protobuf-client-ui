using System;
using Google.Protobuf;
using Xwt;
using System.Collections.Generic;
using Google.Protobuf.Reflection;

namespace protobufclientui
{
    class MessageWidget : VBox, IValueWidget
    {
        readonly IMessage _message;
        readonly Dictionary<string, Frame> _oneOfBoxes = new Dictionary<string, Frame>();
        readonly Dictionary<string, IValueWidget> _widgets = new Dictionary<string, IValueWidget>();
        readonly Controller _controller;

        class OneOfObject
        {
            public FieldDescriptor Descriptor { get; set; }

            public override string ToString()
            {
                return Descriptor.Name;
            }
        }

        public MessageWidget(IMessage message, Controller controller)
        {
            _message = message;
            _controller = controller;

            foreach (var oneof in _message.Descriptor.Oneofs)
            {
                var box = new HBox();
                box.PackStart(new Label(oneof.Name));
                var cbbox = new ComboBox();
                foreach (var f in oneof.Fields)
                {
                    cbbox.Items.Add(new OneOfObject { Descriptor = f });
                }
                cbbox.SelectionChanged += Cbbox_SelectionChanged;

                box.PackStart(cbbox);
                _oneOfBoxes[oneof.Name] = new Frame();
                PackStart(box);
                PackStart(_oneOfBoxes[oneof.Name]);
            }

            foreach (var field in _message.Descriptor.Fields.InDeclarationOrder())
            {
                if (field.ContainingOneof != null)
                {
                    continue;
                }

                Widget w;
                try
                {
                    w = MakeWidgetForField(field);
                }
                catch (Exception ex)
                {
                    w = new Label("Failed to create field: " + ex.Message);
                }

                Widget parentWidget;
                var caption = string.Format("{0} ({1})", field.Name, field.FieldType);
                if (w is MessageWidget)
                {
                    var frame = new Frame
                    {
                        Label = caption,
                        Content = w
                    };

                    parentWidget = frame;
                }
                else
                {
                    var box = new HBox();
                    box.PackStart(new Label(caption));
                    box.PackStart(w);
                    parentWidget = box;
                }
                PackStart(parentWidget);
                _widgets[field.Name] = w as IValueWidget;
            }
        }

        void Cbbox_SelectionChanged(object sender, EventArgs e)
        {
            var field = (sender as ComboBox).SelectedItem as OneOfObject;
            var newWidget = MakeWidgetForField(field.Descriptor);
            _oneOfBoxes[field.Descriptor.ContainingOneof.Name].Content = newWidget;
            field.Descriptor.Accessor.SetValue(_message, (newWidget as IValueWidget).GetValue());
        }

        public object GetValue()
        {
            UpdateValue();

            return _message;
        }

        public void UpdateValue()
        {
            foreach (var field in _message.Descriptor.Fields.InDeclarationOrder())
            {
                if (field.ContainingOneof != null)
                {
                    continue;
                }
                field.Accessor.SetValue(_message, _widgets[field.Name].GetValue());
            }

            foreach (var oneof in _message.Descriptor.Oneofs)
            {
                var widget = _oneOfBoxes[oneof.Name].Content as IValueWidget;
                oneof.Accessor.GetCaseFieldDescriptor(_message).Accessor.SetValue(_message, widget.GetValue());
            }
        }

        Widget MakeWidgetForField(FieldDescriptor field)
        {
            if (field.IsRepeated)
            {
                return new NotImplementedWidget();
            }

            switch (field.FieldType)
            {
                case FieldType.Double:
                case FieldType.Float:
                case FieldType.Int64:
                case FieldType.UInt64:
                case FieldType.Int32:
                case FieldType.Fixed64:
                case FieldType.Fixed32:
                case FieldType.Bool:
                case FieldType.String:
                case FieldType.UInt32:
                case FieldType.SFixed32:
                case FieldType.SFixed64:
                case FieldType.SInt32:
                case FieldType.SInt64:
                    return new TextWidget(field);
                case FieldType.Group:
                    return null;
                case FieldType.Message:
                    return new MessageWidget(_controller.CreateMessageFromTypeName(field.MessageType.Name), _controller);
                case FieldType.Enum:
                    return new EnumWidget(_controller.GetTypeFromName(field.EnumType.Name), field);
                case FieldType.Bytes:
                    return new BytesWidget();
            }
            return new Label(field.FieldType.ToString());
        }
    }
}

