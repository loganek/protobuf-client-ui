using System;
using Xwt;
using Google.Protobuf.Reflection;
using Google.Protobuf;

namespace protobufclientui
{
    public interface IValueWidget
    {
        object GetValue();
    }

    public class BoxedWidget<T> : Frame
        where T : Widget
    {

        public BoxedWidget(T widget)
        {
            Content = widget;
        }

        protected T Widget => Content as T;
    }

    class TextWidget : BoxedWidget<TextEntry>, IValueWidget
    {
        readonly TypeCode _typeCode;

        public TextWidget(FieldDescriptor field) : base(new TextEntry())
        {
            _typeCode = FieldTypeToTypeCode(field.FieldType);
            object defaultValue;
            switch (_typeCode)
            {
                case TypeCode.String:
                    defaultValue = "";
                    break;
                default:
                    defaultValue = Activator.CreateInstance(System.Type.GetType("System." + Enum.GetName(typeof(TypeCode), _typeCode)));
                    break;
            }
            Widget.Text = defaultValue.ToString();
        }

        public object GetValue()
        {
            return Convert.ChangeType(Widget.Text, _typeCode);
        }

        static TypeCode FieldTypeToTypeCode(FieldType type)
        {
            switch (type)
            {
                case FieldType.Double:
                    return TypeCode.Double;
                case FieldType.Float:
                    return TypeCode.Single;
                case FieldType.Int64:
                case FieldType.Fixed64:
                case FieldType.SFixed64:
                case FieldType.SInt64:
                    return TypeCode.Int64;
                case FieldType.UInt64:
                    return TypeCode.UInt64;
                case FieldType.Int32:
                case FieldType.SInt32:
                case FieldType.Fixed32:
                case FieldType.SFixed32:
                    return TypeCode.Int32;
                case FieldType.Bool:
                    return TypeCode.Boolean;
                case FieldType.String:
                    return TypeCode.String;
                case FieldType.UInt32:
                    return TypeCode.UInt32;
                default:
                    throw new Exception(string.Format("Unsupported type {0}", type));
            }
        }
    }

    class EnumWidget : BoxedWidget<ComboBox>, IValueWidget
    {
        readonly Type _type;

        class EnumValueDescriptorWrapper
        {
            public EnumValueDescriptor Descriptor { get; private set; }

            public EnumValueDescriptorWrapper(EnumValueDescriptor descriptor)
            {
                Descriptor = descriptor;
            }

            public override string ToString()
            {
                return Descriptor.Name;
            }
        }

        public EnumWidget(Type type, FieldDescriptor field) : base(new ComboBox())
        {
            _type = type;
            foreach (var value in field.EnumType.Values)
            {
                Widget.Items.Add(new EnumValueDescriptorWrapper(value));
            }
            Widget.SelectedIndex = 0;
        }

        public object GetValue()
        {
            return Enum.ToObject(_type, (Widget.SelectedItem as EnumValueDescriptorWrapper).Descriptor.Number);
        }
    }

    class BytesWidget : BoxedWidget<TextEntry>, IValueWidget
    {
        public BytesWidget() : base(new TextEntry())
        {
        }

        public object GetValue()
        {
            string[] strValues = Widget.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (strValues.Length == 0)
            {
                return ByteString.Empty;
            }

            var values = new byte[strValues.Length];

            for (int i = 0; i < strValues.Length; i++)
            {
                values[i] = Convert.ToByte(strValues[i], 16);
            }

            return ByteString.CopyFrom(values, 0, values.Length);
        }
    }

    class NotImplementedWidget : BoxedWidget<Label>, IValueWidget
    {
        public NotImplementedWidget() : base(new Label("Not implemented"))
        {
        }

        public object GetValue()
        {
            return null;
        }
    }
}

