// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
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

        private bool _treeUpdating;
        private bool _treeSort;

        private const int ContentItemIcon = 0;
        private const int FolderOpenIcon = 1;
        private const int FolderClosedIcon = 2;
        private const int ProjectIcon = 3;

        private const string MonoGameContentProjectFileFilter = "MonoGame Content Build Files (*.mgcb)|*.mgcb";
        private const string XnaContentProjectFileFilter = "XNA Content Projects (*.contentproj)|*.contentproj";

        public static MainView Form { get; private set; }

        public MainView()
        {            
            InitializeComponent();

            // Find an appropriate font for console like output.
            var faces = new [] { "Consolas", "Lucida Console", "Courier New" };
            for (var f=0; f < faces.Length; f++)
            {
                _outputWindow.Font = new System.Drawing.Font(faces[f], 9F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
                if (_outputWindow.Font.Name == faces[f])
                    break;               
            }

            _treeIcons = new ImageList();
            _treeIcons.Images.Add(Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.blueprint.png")));
            _treeIcons.Images.Add(Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_open.png")));
            _treeIcons.Images.Add(Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_closed.png")));
            _treeIcons.Images.Add(Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.settings.png")));
            
            _treeView.ImageList = _treeIcons;
            _treeView.BeforeExpand += TreeViewOnBeforeExpand;
            _treeView.BeforeCollapse += TreeViewOnBeforeCollapse;
            _treeView.NodeMouseClick += TreeViewOnNodeMouseClick;

            _propertyGrid.PropertyValueChanged += OnPropertyGridPropertyValueChanged;

            Form = this;
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

                    if (node.Tag is ContentItem)
                        _itemContextMenu.Show(_treeView, p);
                    else
                        _folderContextMenu.Show(_treeView, p);    
                }
            }
        }

        //public event SelectionChanged OnSelectionChanged;

        public void Attach(IController controller)
        {
            _controller = controller;

            // Make sure build and project trigger updates to all the menu items.
            Action activate = delegate { this.Invoke(new MethodInvoker(UpdateMenus)); };
            _controller.OnBuildStarted += activate;
            _controller.OnBuildFinished += activate;
            _controller.OnProjectLoading += activate;
            _controller.OnProjectLoaded += activate;
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

        public bool AskSaveName(ref string filePath, string title)
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
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

        public void ShowMessage(string message)
        {
            MessageBox.Show(this, message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void BeginTreeUpdate()
        {
            Debug.Assert(_treeUpdating == false, "Must finish previous tree update!");
            _treeUpdating = true;
            _treeSort = false;
            _treeView.BeginUpdate();
        }

        public void SetTreeRoot(IProjectItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            _treeView.Nodes.Clear();
            _propertyGrid.SelectedObject = null;

            var project = item as PipelineProject;
            if (project == null)
                return;

            var root = _treeView.Nodes.Add(string.Empty, item.Name, -1);
            root.Tag = new PipelineProjectProxy(project);
            root.SelectedImageIndex = ProjectIcon;
            root.ImageIndex = ProjectIcon;

            _propertyGrid.SelectedObject = root.Tag;
        }

        public void AddTreeItem(IProjectItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");
            _treeSort = true;

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
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            var node = _treeView.AllNodes().Find(f => f.Tag == item);
            if (node != null)
                _treeView.Nodes.Remove(node);
        }

        public void SelectTreeItem(IProjectItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            var node = _treeView.AllNodes().Find(e => e.Tag == item);
            if (node != null)
                _treeView.SelectedNode = node;
        }

        public void UpdateTreeItem(IProjectItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

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

        public void EndTreeUpdate()
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            if (_treeSort)
            {
                var node = _treeView.SelectedNode;
                _treeView.Sort();
                _treeView.SelectedNode = node;
            }
            _treeSort = false;
            _treeView.EndUpdate();

            _treeUpdating = false;
        }

        public void ShowProperties(IProjectItem item)
        {
            _propertyGrid.SelectedObject = item;
            _propertyGrid.ExpandAllGridItems();
        }

        public void UpdateProperties(IProjectItem item)
        {
            if (_propertyGrid.SelectedObject == item)
            {
                _propertyGrid.Refresh();
                _propertyGrid.ExpandAllGridItems();
            }
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

        public bool ChooseContentFile(string initialDirectory, out List<string> files)
        {
            var dlg = new OpenFileDialog()
            {
                RestoreDirectory = true,
                AddExtension = true,
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = "All Files (*.*)|*.*",
                InitialDirectory = initialDirectory,
                Multiselect = true,
                
            };

            var result = dlg.ShowDialog(this);
            files = new List<string>();

            if (result != DialogResult.OK)
                return false;

            files.AddRange(dlg.FileNames);

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

        private void RebuildMenuItemClick(object sender, EventArgs e)
        {
            _controller.Build(true);
        }

        private void CleanMenuItemClick(object sender, EventArgs e)
        {
            _controller.Clean();
        }

        private void ItemRebuildMenuItemClick(object sender, EventArgs e)
        {
            var item = _treeView.GetSelectedContentItem();
            _controller.RebuildItem(item);
        }

        private void CancelBuildMenuItemClick(object sender, EventArgs e)
        {
            _controller.CancelBuild();
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

        private void _mainMenu_MenuActivate(object sender, EventArgs e)
        {
            UpdateMenus();
        }

        private void UpdateMenus()
        {
            var notBuilding = !_controller.ProjectBuilding;
            var projectOpen = _controller.ProjectOpen;
            var projectOpenAndNotBuilding = projectOpen && notBuilding;

            // Update the state of all menu items.

            _newMenuItem.Enabled = notBuilding;
            _openMenuItem.Enabled = notBuilding;
            _importMenuItem.Enabled = notBuilding;

            _saveMenuItem.Enabled = projectOpenAndNotBuilding && _controller.ProjectDiry;
            _saveAsMenuItem.Enabled = projectOpenAndNotBuilding;
            _closeMenuItem.Enabled = projectOpenAndNotBuilding;

            _exitMenuItem.Enabled = notBuilding;

            _newItemMenuItem.Enabled = projectOpen;
            _addItemMenuItem.Enabled = projectOpen;
            _deleteMenuItem.Enabled = projectOpen;

            _buildMenuItem.Enabled = projectOpenAndNotBuilding;
            _itemRebuildMenuItem.Enabled = _rebuildMenuItem.Enabled = projectOpenAndNotBuilding;
            _cleanMenuItem.Enabled = projectOpenAndNotBuilding;
            _cancelBuildSeparator.Visible = !notBuilding;
            _cancelBuildMenuItem.Enabled = !notBuilding;
            _cancelBuildMenuItem.Visible = !notBuilding;
        }

        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (_treeView.SelectedNode == null)
                return;

            var item = _treeView.SelectedNode.Tag as ContentItem;
            if (item == null)
                return;

            _controller.Exclude(item);
        }

        private void ViewHelpMenuItemClick(object sender, EventArgs e)
        {
            Process.Start("http://www.monogame.net/documentation/");
        }

        private void AboutMenuItemClick(object sender, EventArgs e)
        {
            Process.Start("http://www.monogame.net/about/");
        }

        private void AddMenuItemClick(object sender, EventArgs e)
        {
            var node = _treeView.SelectedNode ?? _treeView.Nodes[0];
            var item = node.Tag as IProjectItem;
            _controller.Include(item.Location);
        }
    }
}
