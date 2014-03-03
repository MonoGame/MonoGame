// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class MainView : Form, IView, IProjectObserver
    {
        IController _controller;

        public MainView()
        {
            InitializeComponent();
        }

        public event SelectionChanged OnSelectionChanged;

        public void Attach(IController controller)
        {
            _controller = controller;
        }

        public AskResult AskSaveOrCancel()
        {
            var result = MessageBox.Show(
                this,
                "Do you want to save the project first?",
                "Save Project",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button3);

            if (result == DialogResult.Yes)
                return AskResult.Yes;
            if (result == DialogResult.No)
                return AskResult.No;

            return AskResult.Cancel;
        }

        public bool AskSaveName(ref string filePath)
        {
            var dialog = new SaveFileDialog
            {
                RestoreDirectory = true,
                InitialDirectory = Path.GetDirectoryName(filePath),
                FileName = Path.GetFileName(filePath),
                AddExtension = true,
                CheckPathExists = true,
                Filter = "Pipeline Project (*.pipline)|*.pipeline"
            };
            var result = dialog.ShowDialog(this);
            filePath = dialog.FileName;
            return result != DialogResult.Cancel;
        }

        public bool AskOpenProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog()
            {
                RestoreDirectory = true,
                AddExtension = true,
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = "Pipeline Project (*.pipline)|*.pipeline"
            };
            var result = dialog.ShowDialog(this);
            projectFilePath = dialog.FileName;
            return result != DialogResult.Cancel;
        }

        public void ShowError(string title, string message)
        {
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }

        public void ClearTree()
        {
            _treeView.Nodes.Clear();
        }

        public void AddTreeItem(IProjectItem item)
        {
            var path = item.Path;
            var folders = path.Split(new [] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            var root = _treeView.Nodes;
            foreach (var folder in folders)
            {
                var found = root.Find(folder, false);
                if (found.Length == 0)
                    root = root.Add(folder, folder, -1).Nodes;
                else
                    root = found[0].Nodes;
            }

            root.Add(string.Empty, item.Label, -1).Tag = item;
        }

        private void NewMenuItemClick(object sender, System.EventArgs e)
        {
            _controller.NewProject();
        }

        private void ExitMenuItemClick(object sender, System.EventArgs e)
        {
            if (_controller.Exit())
                Application.Exit();
        }

        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (!_controller.Exit())
                    e.Cancel = true;
            }
        }

        private void SaveMenuItemClick(object sender, System.EventArgs e)
        {
            _controller.SaveProject(false);
        }

        private void SaveAsMenuItemClick(object sender, System.EventArgs e)
        {
            _controller.SaveProject(true);
        }

        private void OpenMenuItemClick(object sender, System.EventArgs e)
        {
            _controller.OpenProject();
        }
    }
}
