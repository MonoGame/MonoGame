// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class DeleteDialog : Dialog
    {
        public DeleteDialog(Window parrent, string[] items) : base(Global.GetNewDialog(parrent.Handle))
        {
            Build();

            this.AddButton("Ok", ResponseType.Ok);
            this.AddButton("Cancel", ResponseType.Cancel);
            this.DefaultResponse = ResponseType.Ok;

            foreach (var item in items)
                textView1.Buffer.Text += item + Environment.NewLine;
        }
    }
}

