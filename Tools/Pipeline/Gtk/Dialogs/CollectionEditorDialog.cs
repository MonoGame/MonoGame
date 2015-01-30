using System;

namespace MonoGame.Tools.Pipeline
{
    public partial class CollectionEditorDialog : Gtk.Dialog
    {
        public string text;

        public CollectionEditorDialog (string text)
        {
            Build();
            textview2.Buffer.Text = text;
        }

        protected void OnResponse(object sender, EventArgs e)
        {
            Destroy();
        }

        protected void OnButtonOkClicked (object sender, EventArgs e)
        {
            text = textview2.Buffer.Text;
            Respond(Gtk.ResponseType.Ok);
        }
    }
}

