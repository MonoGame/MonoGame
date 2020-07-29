using System.Threading;
using Eto;
using Eto.Forms;
using MonoDevelop.Ide.Gui;
using MonoGame.Tools.Pipeline;

namespace MonoGame.IDE.VisualStudioForMac
{
    public class MGPad : PadContent
    {
        private static Application _application;

        private MainWindow _window;
        private Gtk.GtkNSViewHost _control;

        static MGPad()
        {
            _application = new Application(Platforms.XamMac2);
            _application.Attach();
        }

        public MGPad()
        {
            _window = new MainWindow();
            PipelineController.Create(_window);

            _control = new Gtk.GtkNSViewHost(_window.ToNative(true));
            _control.ShowAll();
        }

        public override MonoDevelop.Components.Control Control => _control;
    }
}
