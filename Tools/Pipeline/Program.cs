using System;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var view = new MainView();
            var model = new PipelineModel();
            var controller = new PipelineController(view, model);          
            Application.Run(view);
        }
    }
}
