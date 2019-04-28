using System;
using Xwt;

namespace protobufclientui
{
    public class SetupWidget : Table
    {
        public event EventHandler LoadRequested;

        public string ProtoFileLocation { get { return _protoFileSelector.FileName; } }
        public string GeneratorLocation { get { return _generatorFileSelector.FileName; } }

        readonly FileSelector _protoFileSelector = new FileSelector ();
        readonly FileSelector _generatorFileSelector = new FileSelector ();

        public SetupWidget()
        {
            _protoFileSelector.FileSelectionMode = FileSelectionMode.Open;
            _generatorFileSelector.FileSelectionMode = FileSelectionMode.Open;

            Add (new Label ("Proto file location:"), 0, 0);
            Add (_protoFileSelector, 1, 0, 1, 1, true);
            Add (new Label ("protoc generator location:"), 0, 1);
            Add (_generatorFileSelector, 1, 1, 1, 1, true);
            
            var loadButton = new Button ("Load");
            loadButton.Clicked += (sender, args) => {
                LoadRequested?.Invoke (this, null);
            };
            Add (loadButton, 0, 2, 1, 2);

            LoadDefaultCompilerPath ();
        }

        void LoadDefaultCompilerPath ()
        {
            foreach (var path in Environment.GetEnvironmentVariable ("PATH").Split (new char [] { ':' })) {
                string location = System.IO.Path.Combine (path, "protoc");
                if (System.IO.File.Exists (location)) {
                    _generatorFileSelector.FileName = location;
                    return;
                }
            }
        }
    }
}


