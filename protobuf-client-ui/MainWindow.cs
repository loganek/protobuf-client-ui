using Xwt;
using System;
using Google.Protobuf;
using System.Collections.Generic;
using System.Linq;

namespace protobufclientui
{
    public class MainWindow : Window, IUI
    {
        readonly ComboBox _outcommingTypeSelector = new ComboBox ();
        readonly ComboBox _incommingTypeSelector = new ComboBox ();
        readonly RichTextView _outputView = new RichTextView ();
        readonly Frame _messageEditorFrame = new Frame ();
        readonly MessageTunelWidget _messageTunelWidget;
        readonly Controller _controller;

        public MainWindow (Controller controller)
        {
            _controller = controller;

            Title = "protobuf-client-ui";
            Width = 400;
            Height = 400;

            var mainLayout = new VBox ();
            Content = mainLayout;

            var setupWidget = new SetupWidget ();
            setupWidget.LoadRequested += (sender, args) => Init (sender as SetupWidget);
            mainLayout.PackStart (setupWidget);

            _messageTunelWidget = new MessageTunelWidget (_controller.MessageTunels);
            _messageTunelWidget.OnTunelSelected += (sender, tunel) => _controller.SelectMessageTunel (tunel);
            _messageTunelWidget.OnTunelStateChanged += (sender, e) => _controller.ToggleTunelState (_messageTunelWidget.GetParameters ());
            mainLayout.PackStart (_messageTunelWidget);

            _outcommingTypeSelector.SelectionChanged += (sender, e) => _controller.SetOutcommingMessageType (_outcommingTypeSelector.SelectedItem as string);
            var outBox = new VBox ();
            outBox.PackStart (new Label ("outcomming message editor"));
            outBox.PackStart (_outcommingTypeSelector);
            var generate = new Button ("Generate");
            generate.Clicked += Generate_Clicked;
            outBox.PackStart (generate);
            outBox.PackStart (_messageEditorFrame);

            _incommingTypeSelector.SelectionChanged += (sender, e) => _controller.SetIncommingMessageType (_incommingTypeSelector.SelectedItem as string);
            var inBox = new VBox ();
            inBox.PackStart (new Label ("incomming messages"));
            inBox.PackStart (_incommingTypeSelector);
            inBox.PackStart (_outputView);

            var mainPanel = new HBox (); // TODO use Pane?
            mainPanel.PackStart (outBox, true, true);
            mainPanel.PackStart (inBox, true, true);
            mainLayout.PackStart (mainPanel);
        }

        public void SetMessageTunel (Dictionary<string, Type> options)
        {
            _messageTunelWidget.LoadTunelOptions (options);
        }

        public void AddIncommingMessage (IMessage message)
        {
            Application.Invoke (() => _outputView.LoadText (new JsonFormatter (JsonFormatter.Settings.Default).Format (message), new Xwt.Formats.PlainTextFormat ()));
        }

        public void LoadMessageType (IMessage message)
        {
            _messageEditorFrame.Content = new MessageWidget (message, _controller);
        }

        public void LogError (string title, string message)
        {
            Application.Invoke (() => MessageDialog.ShowError (title, message));
        }

        public void SetMessageTypes (List<Type> types)
        {
            _outcommingTypeSelector.Items.Clear ();
            _incommingTypeSelector.Items.Clear ();

            foreach (var type in types) {
                _outcommingTypeSelector.Items.Add (type.Name);
                _incommingTypeSelector.Items.Add (type.Name);
            }

            if (types.Any ()) {
                _outcommingTypeSelector.SelectedIndex = 0;
                _incommingTypeSelector.SelectedIndex = 0;
            }
        }

        void Init (SetupWidget setupWidget)
        {
            _controller.LoadFile (setupWidget.ProtoFileLocation, setupWidget.GeneratorLocation);
        }

        void Generate_Clicked (object sender, EventArgs e)
        {
            try {
                if (_messageEditorFrame.Content is MessageWidget messageWidget) {
                    messageWidget.UpdateValue ();
                    _controller.SendMessage ();
                } else {
                    LogError ("Failed to generate message", "Message type was not selected");
                }
            } catch (Exception ex) {
                MessageDialog.ShowError ("Failed to send message: " + ex.Message);
            }
        }

        public void SetTunnelState (bool started)
        {
            _messageTunelWidget.SetStarted (started);
        }

        protected override bool OnCloseRequested ()
        {
            Application.Exit ();
            return true;
        }
    }
}

