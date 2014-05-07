﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class MainView : Form, IView, IProjectObserver
    {
        private IController _controller;
        private ImageList _treeIcons;
        private ContextMenuStrip _contextMenu;

        private const int ContentItemIcon = 0;
        private const int FolderOpenIcon = 1;
        private const int FolderClosedIcon = 2;
        private const int ProjectIcon = 3;
        private const string ContextMenuInclude = "Add";
        private const string ContextMenuExclude = "Remove";
        private const string ContextMenuOpenFile = "Open File";
        private const string ContextMenuOpenFileLocation = "Open File Location";

        private const string MonoGameContentProjectFileFilter = "MonoGame Content Build Files (*.mgcb)|*.mgcb";
        private const string XnaContentProjectFileFilter = "XNA Content Projects (*.contentproj)|*.contentproj";

        public MainView()
        {            
            InitializeComponent();

            _treeIcons = new ImageList();
            _treeIcons.Images.Add(Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.blueprint.png")));
            _treeIcons.Images.Add(Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_open.png")));
            _treeIcons.Images.Add(Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_closed.png")));
            _treeIcons.Images.Add(Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.settings.png")));
            
            _treeView.ImageList = _treeIcons;
            _treeView.BeforeExpand += TreeViewOnBeforeExpand;
            _treeView.BeforeCollapse += TreeViewOnBeforeCollapse;
            _treeView.NodeMouseClick += TreeViewOnNodeMouseClick;

            _contextMenu = new ContextMenuStrip();
            _contextMenu.ItemClicked += OnContextMenuItemClicked;

            _propertyGrid.PropertyValueChanged += OnPropertyGridPropertyValueChanged;            
        }

        private void OnPropertyGridPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "References")
                _controller.OnReferencesModified();
            else
            {
                if (_propertyGrid.SelectedObject is ContentItem)
                    _controller.OnItemModified(_propertyGrid.SelectedObject as ContentItem);
                else
                    _controller.OnProjectModified();
            }
        }

        private void OnContextMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case ContextMenuInclude:
                    {
                        _controller.Include((e.ClickedItem.Tag as IProjectItem).Location);                        
                    } break;
                case ContextMenuExclude:
                    {
                        _controller.Exclude(e.ClickedItem.Tag as ContentItem);                        
                    } break;
                case ContextMenuOpenFile:
                    {
                        var filePath = (e.ClickedItem.Tag as IProjectItem).OriginalPath;
                        filePath = _controller.GetFullPath(filePath);

                        if (File.Exists(filePath))
                        {
                            Process.Start(filePath);                            
                        }
                    } break;
                case ContextMenuOpenFileLocation:
                    {
                        var filePath = (e.ClickedItem.Tag as IProjectItem).OriginalPath;
                        filePath = _controller.GetFullPath(filePath);

                        if (File.Exists(filePath) || Directory.Exists(filePath))
                        {
                            Process.Start("explorer.exe", "/select, " + filePath);
                        }
                    } break;
                default:
                    throw new Exception(string.Format("Unhandled menu item text={0}", e.ClickedItem.Text));
            }
        }

        private void TreeViewOnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button == MouseButtons.Right)
            {
                // Point where the mouse is clicked.
                var p = new Point(e.X, e.Y);

                // Get the node that the user has clicked.
                var node = _treeView.GetNodeAt(p);
                if (node != null)
                {
                    // Select the node the user has clicked.
                    _treeView.SelectedNode = node;

                    _contextMenu.Items.Clear();

                    if (node.Tag is ContentItem)                    
                        _contextMenu.Items.Add(ContextMenuExclude).Tag = node.Tag;                    
                    else                    
                        _contextMenu.Items.Add(ContextMenuInclude).Tag = node.Tag;

                    var filePath = (node.Tag as IProjectItem).OriginalPath;
                    filePath = _controller.GetFullPath(filePath);

                    if (File.Exists(filePath))
                        _contextMenu.Items.Add(ContextMenuOpenFile).Tag = node.Tag;

                    if (File.Exists(filePath) || Directory.Exists(filePath))                    
                        _contextMenu.Items.Add(ContextMenuOpenFileLocation).Tag = node.Tag;

                    _contextMenu.Show(_treeView, p);
                }
            }
        }

        //public event SelectionChanged OnSelectionChanged;

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
                Filter = MonoGameContentProjectFileFilter,
                FilterIndex = 2,                
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
                Filter = MonoGameContentProjectFileFilter,
                FilterIndex = 2,
            };
            var result = dialog.ShowDialog(this);
            projectFilePath = dialog.FileName;
            return result != DialogResult.Cancel;
        }

        public bool AskImportProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog()
            {
                RestoreDirectory = true,
                AddExtension = true,
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = XnaContentProjectFileFilter,
                FilterIndex = 2,
            };
            var result = dialog.ShowDialog(this);
            projectFilePath = dialog.FileName;
            return result != DialogResult.Cancel;
        }

        public void ShowError(string title, string message)
        {
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }

        public void SetTreeRoot(IProjectItem item)
        {
            _treeView.Nodes.Clear();

            if (item != null)
            {
                var root = _treeView.Nodes.Add(string.Empty, item.Name, -1);
                root.Tag = item;
                root.SelectedImageIndex = ProjectIcon;
                root.ImageIndex = ProjectIcon;
            }

            _propertyGrid.SelectedObject = item;
        }

        public void AddTreeItem(IProjectItem item)
        {            
            var path = item.Location;
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);

            var root = _treeView.Nodes[0];
            var parent = root.Nodes;
            foreach (var folder in folders)
            {
                var found = parent.Find(folder, false);
                if (found.Length == 0)
                {
                    var folderNode = parent.Add(folder, folder, -1);
                    folderNode.ImageIndex = FolderClosedIcon;
                    folderNode.SelectedImageIndex = FolderClosedIcon;

                    var idx = path.IndexOf(folder);
                    var curPath = path.Substring(0, idx + folder.Length);
                    folderNode.Tag = new FolderItem(curPath);
                    
                    parent = folderNode.Nodes;
                }
                else
                    parent = found[0].Nodes;
            }

            var node = parent.Add(string.Empty, item.Name, -1);
            node.Tag = item;
            node.ImageIndex = ContentItemIcon;
            node.SelectedImageIndex = ContentItemIcon;

            root.Expand();
        }

        public void RemoveTreeItem(ContentItem item)
        {
            var node = _treeView.AllNodes().Find(f => f.Tag == item);
            if (node != null)
                _treeView.Nodes.Remove(node);
        }

        public void SelectTreeItem(IProjectItem item)
        {
            var node = _treeView.AllNodes().Find(e => e.Tag == item);
            if (node != null)
                _treeView.SelectedNode = node;
        }

        public void UpdateTreeItem(IProjectItem item)
        {
            var node = _treeView.AllNodes().Find(e => e.Tag == item);
            if (node != null)
			{
				// Do something useful, eg...
				/* 
				if (!node.IsValid)
				{
	                node.ForeColor = Color.Red;
				}
				else
				{
					node.ForeColor = Color.Black;
				}*/
			}
        }

        public void ShowProperties(IProjectItem item)
        {
            _propertyGrid.SelectedObject = item;
        }

        public void UpdateProperties(IProjectItem item)
        {
            if (_propertyGrid.SelectedObject == item)
                _propertyGrid.Refresh();
        }

        public void OutputAppend(string text)
        {
            if (text == null)
                return;

            // We need to append newlines.
            var line = string.Concat(text, Environment.NewLine);

            // Write the output... safely if needed.
            if (InvokeRequired)
                _outputWindow.Invoke(new Action<string>(_outputWindow.AppendText), new object[] { line });
            else
                _outputWindow.AppendText(line);
        }

        public bool ChooseContentFile(string initialDirectory, out string file)
        {
            var dlg = new OpenFileDialog()
            {
                RestoreDirectory = true,
                AddExtension = true,
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = "All Files (*.*)|*.*",
                InitialDirectory = initialDirectory,
                Multiselect = false,
                
            };
            var result = dlg.ShowDialog(this);
            
            file = dlg.FileName;

            if (result != DialogResult.OK)
                return false;

            return true;
        }

        public void OutputClear()
        {
            _outputWindow.Clear();
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

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            _controller.CloseProject();
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

        private void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            _controller.OnTreeSelect(e.Node.Tag as IProjectItem);
        }

        private void TreeViewMouseUp(object sender, MouseEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button != MouseButtons.Right)
                return;

            // Point where the mouse is clicked.
            var p = new Point(e.X, e.Y);

            // Get the node that the user has clicked.
            var node = _treeView.GetNodeAt(p);
            if (node == null) 
                return;

            // Select the node the user has clicked.
            _treeView.SelectedNode = node;

            // TODO: Show context menu!
        }

        private void BuildMenuItemClick(object sender, EventArgs e)
        {
            _controller.Build(false);
        }

        private void RebuilMenuItemClick(object sender, EventArgs e)
        {
            _controller.Build(true);
        }

        private void CleanMenuItemClick(object sender, EventArgs e)
        {
            _controller.Clean();
        }

        private void ImportMenuItem_Click(object sender, EventArgs e)
        {
            _controller.ImportProject();
        }

        private void TreeViewOnBeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.ImageIndex == FolderOpenIcon)
            {
                e.Node.ImageIndex = FolderClosedIcon;
                e.Node.SelectedImageIndex = FolderClosedIcon;
            }
        }

        private void TreeViewOnBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.ImageIndex == FolderClosedIcon)
            {
                e.Node.ImageIndex = FolderOpenIcon;
                e.Node.SelectedImageIndex = FolderOpenIcon;
            }
        }

        private void FileMenu_Click(object sender, EventArgs e)
        {
            // Update the enabled state for menu items.

            _newMenuItem.Enabled = true;
            _openMenuItem.Enabled = true;
            _importMenuItem.Enabled = true;

            _saveMenuItem.Enabled = _controller.ProjectOpen && _controller.ProjectDiry;
            _saveAsMenuItem.Enabled = _controller.ProjectOpen;
            _closeMenuItem.Enabled = _controller.ProjectOpen;
        }

        private void BuildMenu_Click(object sender, EventArgs e)
        {
            // Update the enabled state for menu items.

            _buildMenuItem.Enabled = _controller.ProjectOpen;
            _cleanMenuItem.Enabled = _controller.ProjectOpen;
            _rebuilMenuItem.Enabled = _controller.ProjectOpen;
        }
    }
}
