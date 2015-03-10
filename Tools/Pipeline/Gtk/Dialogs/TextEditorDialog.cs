using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class TextEditorDialog : Dialog
    {
        public string text;

        public TextEditorDialog(string title, string label, string text)
        {
            Build();

            Title = title;
            label2.Text = label;
            entry1.Text = text;
        }

        protected void OnResponse(object sender, EventArgs e)
        {
            Destroy ();
        }

        protected void OnButtonOkClicked(object sender, EventArgs e)
        {
            text = entry1.Text;
            Respond(ResponseType.Ok);
        }
    }
}

