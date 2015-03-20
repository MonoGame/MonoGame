using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class TextEditorDialog : Dialog
    {
        bool strictmode;

        public string text;

        public TextEditorDialog(string title, string label, string text, bool strictmode)
        {
            Build();

            this.strictmode = strictmode;
            buttonOk.Sensitive = !strictmode;

            Title = title;
            label2.Text = label;
            entry1.Text = text;
        }

        protected void OnResponse(object sender, EventArgs e)
        {
            Destroy ();
        }

        public void ButtonOkEnabled()
        {
            if (!strictmode)
                return;

            if (entry1.Text != "") {
                if (MainWindow.CheckString (entry1.Text, MainWindow.AllowedCharacters)) {
                    buttonOk.Sensitive = true;
                    label3.Visible = false;
                } else {
                    buttonOk.Sensitive = false;
                    label3.Visible = true;
                }
            } else {
                buttonOk.Sensitive = false;
                label3.Visible = false;
            }
        }

        protected void OnEntry1Changed(object sender, EventArgs e)
        {
            ButtonOkEnabled();
        }

        protected void OnButtonOkClicked(object sender, EventArgs e)
        {
            text = entry1.Text;
            Respond(ResponseType.Ok);
        }
    }
}

