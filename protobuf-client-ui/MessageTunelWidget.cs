using System;
using System.Collections.Generic;
using Xwt;
using System.Linq;

namespace protobufclientui
{
    public delegate void MessageTunelSelected(object sender, string tunel);

    class MessageTunelOptionsWidget : Table
    {
        readonly Dictionary<string, Func<object>> _getters = new Dictionary<string, Func<object>>();

        public MessageTunelOptionsWidget(Dictionary<string, Type> tunelOptions)
        {
            int i = 0;
            foreach (var option in tunelOptions)
            {
                Add(new Label(option.Key), 0, i);
                Widget edit;
                if (option.Value == typeof(int) || option.Value == typeof(string))
                {
                    edit = new TextEntry();
                    _getters.Add(option.Key, () => (edit as TextEntry).Text);
                }
                else
                {
                    edit = new Label("Unknown type " + option.Value.Name);
                }

                Add(edit, 1, i);
                i++;
            }
        }

        public Dictionary<string, object> GetParameters()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach (var o in _getters)
            {
                data.Add(o.Key, o.Value());
            }
            return data;
        }
    }

    public class MessageTunelWidget : Frame
    {
        private MessageTunelOptionsWidget _optionsWidget;
        private readonly Button _startStopButton = new Button();

        public event EventHandler OnTunelStateChanged;
        public event MessageTunelSelected OnTunelSelected;

        public MessageTunelWidget(IEnumerable<string> tunels)
        {
            Label = "Message tunel options";

            var messageTunelSelector = new ComboBox();
            foreach (var tunel in tunels)
            {
                messageTunelSelector.Items.Add(tunel);
            }

            messageTunelSelector.SelectionChanged += (sender, e) => OnTunelSelected?.Invoke(this, messageTunelSelector.SelectedText);

            var box = new HBox();
            Content = box;
            box.PackStart(messageTunelSelector);

            _startStopButton.Clicked += (sender, e) => OnTunelStateChanged?.Invoke(this, e);
            box.PackStart(_startStopButton);
            messageTunelSelector.SelectedIndex = 0;
        }

        public Dictionary<string, object> GetParameters()
        {
            if (_optionsWidget != null)
            {
                return _optionsWidget.GetParameters();
            }

            return new Dictionary<string, object>();
        }

        public void LoadTunelOptions(Dictionary<string, Type> tunelOptions)
        {
            var optionsWidget = new MessageTunelOptionsWidget(tunelOptions);

            if (_optionsWidget != null)
            {
                (Content as HBox).Placements.First((w) => _optionsWidget == w.Child).Child = optionsWidget;
            }
            else
            {
                (Content as HBox).PackStart(optionsWidget);
            }
            _optionsWidget = optionsWidget;
        }

        internal void SetStarted(bool started)
        {
            _startStopButton.Label = started ? "Stop" : "Start";
        }
    }
}
