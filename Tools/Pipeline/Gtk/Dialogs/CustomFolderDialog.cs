// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class CustomFolderDialog : Dialog
    {
        private readonly string[] symbols = { "Platform", "Configuration", "Config", "Profile" };

        private MainWindow _window;
        private Uri _projectLocation;

        public string FileName { get; set; }

        public CustomFolderDialog(Window parrent, string startDirectory)
            : base(Global.GetNewDialog(parrent.Handle))
        {
            Build();

            _window = (MainWindow)parrent;

            string pl = ((PipelineController)_window._controller).ProjectLocation;
            if (!pl.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                pl += System.IO.Path.DirectorySeparatorChar;

            _projectLocation = new Uri(pl);

            entryPath.Text = startDirectory;
            entryPath.Position = entryPath.Text.Length;
        }

        protected void OnResponse(object sender, EventArgs e)
        {
            FileName = entryPath.Text;
        }

        protected void ButtonBrowse_Clicked(object sender, EventArgs e)
        {
            var dialog = new FileChooserDialog("Add Content Folder",
                             _window,
                             FileChooserAction.SelectFolder,
                             "Cancel", ResponseType.Cancel,
                             "Open", ResponseType.Accept);
            dialog.SetCurrentFolder(_window._controller.GetFullPath(entryPath.Text));

            var responseId = dialog.Run();
            var fileName = dialog.Filename;
            dialog.Destroy();

            if (responseId != (int)ResponseType.Accept)
                return;
            
            var pathUri = new Uri(fileName);
            entryPath.Text = Uri.UnescapeDataString(_projectLocation.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
            entryPath.Position = entryPath.Text.Length;
        }

        protected void ButtonSymbol_Clicked(object sender, EventArgs e)
        {
            var symbol = ((Button)sender).Label;
            var path = entryPath.Text;

            int start, end;
            entryPath.GetSelectionBounds(out start, out end);

            if (start != end)
                path = path.Remove(start, end - start);

            entryPath.Text = path.Insert(start, "$(" + symbol + ")");
            entryPath.GrabFocus();
            entryPath.Position = start + symbol.Length + 3;
        }
    }
}

