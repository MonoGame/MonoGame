using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class AddFileDialog : Dialog
    {
        public bool applyforall;
        public CopyAction responce;

        public AddFileDialog(string fileloc, bool exists)
        {
            Build();

            label1.Text = label1.Text.Replace("%file", fileloc);
            label1.Markup = label1.Text;

            if (exists)
            {
                radiobuttonCopy.Sensitive = false;
                radiobuttonLink.Active = true;
            }
        }

        protected void OnResponse(object sender, EventArgs e)
        {
            Destroy ();
        }

        protected void OnButtonOkClicked(object sender, EventArgs e)
        {
            if (radiobuttonCopy.Active)
                responce = CopyAction.Copy;
            else if (radiobuttonLink.Active)
                responce = CopyAction.Link;
            else
                responce = CopyAction.Skip;

            applyforall = checkbutton1.Active;
            Respond(ResponseType.Ok);
        }
    }
}

