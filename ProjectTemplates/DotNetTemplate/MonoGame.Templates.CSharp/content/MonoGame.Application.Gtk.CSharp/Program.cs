using System;
using Gtk;
using Microsoft.Xna.Framework;

namespace MGNamespace
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            var win = new MainWindow();
            win.ShowAll();

            Application.Run();
        }
    }
}
