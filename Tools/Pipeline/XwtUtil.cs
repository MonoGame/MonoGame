using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public static class XwtUtil
    {
        public static Eto.Forms.Control ToEto(this Xwt.Widget value)
        {
            var nativeWidget = Xwt.Toolkit.CurrentEngine.GetNativeWidget(value);

#if WINDOWS
            var widget = nativeWidget as System.Windows.Controls.Control;
#else
            var widget = nativeWidget as Gtk.Widget;
#endif

            return widget.ToEto();
        }
    }
}

