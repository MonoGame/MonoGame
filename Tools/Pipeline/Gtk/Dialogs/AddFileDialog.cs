using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class AddFileDialog : Dialog
    {
        public bool applyforall;
        public int responce;

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
            responce = radiobuttonCopy.Active ? 1 : 2;
            applyforall = checkbutton1.Active;

            Respond(ResponseType.Ok);
        }
    }
}

