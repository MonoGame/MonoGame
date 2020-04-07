using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class LogWindow : Form
    {
        public static Button ButtonCopy;
        private Clipboard _clipboard;
        
        public LogWindow()
        {
            InitializeComponent();

            ButtonCopy = _buttonCopy;
            Style = "LogWindow";
            _clipboard = new Clipboard();
        }

        public string LogText
        {
            get { return _textAreaLog.Text; }
            set { _textAreaLog.Text = value; }
        }

        private void LogWindow_Closed(object sender, EventArgs e)
        {
            PipelineSettings.Default.ErrorMessage = string.Empty;
            PipelineSettings.Default.Save();
            Application.Instance.Quit();
        }

        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            _clipboard.Clear();
            _clipboard.Text = _textAreaLog.Text;
        }
    }
}
