using Xwt;

namespace protobufclientui
{
    public static partial class App
    {
        static Controller Controller;

        static App ()
        {
            ToolkitWindows = ToolkitType.Wpf;
            ToolkitLinux = ToolkitType.Gtk;
            ToolkitMacOS = ToolkitType.XamMac;
        }

        static void OnRun (string[] args)
        {
            Controller = new Controller();
        }

        static void OnExit ()
        {
            if (Controller != null)
                Controller.Dispose ();
        }
    }
}


