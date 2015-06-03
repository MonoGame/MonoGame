using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class AddFileDialog : Dialog
    {
        public bool applyforall;
        public CopyAction responce;

        public AddFileDialog(Window parrent, string fileloc, bool exists) : base(Global.GetNewDialog(parrent.Handle))
        {
            Build();

            this.Title = Mono.Unix.Catalog.GetString ("Add File Action");

            this.AddButton("Ok", ResponseType.Ok);
            this.AddButton("Cancel", ResponseType.Cancel);
            this.DefaultResponse = ResponseType.Ok;

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
            if (radiobuttonCopy.Active)
                responce = CopyAction.Copy;
            else if (radiobuttonLink.Active)
                responce = CopyAction.Link;
            else
                responce = CopyAction.Skip;

            applyforall = checkbutton1.Active;

            Destroy ();
        }
    }
}

