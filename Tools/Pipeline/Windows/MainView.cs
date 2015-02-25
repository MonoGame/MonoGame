// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{    
    partial class MainView : Form, IView, IProjectObserver
    {
        // The project which will be opened as soon as a controller is attached.
        // Is used when PipelineTool is launched to open a project, provided by the command line.
        public string OpenProjectPath;

        private IController _controller;
        private ImageList _treeIcons;

        private bool _treeUpdating;
        private bool _treeSort;

        private const int ContentItemIcon = 0;
        private const int ContentMissingIcon = 1;
        private const int FolderOpenIcon = 2;
        private const int FolderClosedIcon = 3;
        private const int ProjectIcon = 4;        

        private const string MonoGameContentProjectFileFilter = "MonoGame Content Build Files (*.mgcb)|*.mgcb";
        private const string XnaContentProjectFileFilter = "XNA Content Projects (*.contentproj)|*.contentproj";

        public static MainView Form { get; private set; }

        public MainView()
        {            
            InitializeComponent();

            // Set the application icon this form.
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // Find an appropriate font for console like output.
            var faces = new [] { "Consolas", "Lucida Console", "Courier New" };
            for (var f=0; f < faces.Length; f++)
            {
                _outputWindow.Font = new System.Drawing.Font(faces[f], 9F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
                if (_outputWindow.Font.Name == faces[f])
                    break;               
            }

            _outputWindow.SelectionHangingIndent = TextRenderer.MeasureText(" ", _outputWindow.Font).Width;            

            _treeIcons = new ImageList();
            var asm = Assembly.GetExecutingAssembly();
            _treeIcons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.blueprint.png")));
            _treeIcons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.missing.png")));
            _treeIcons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_open.png")));
            _treeIcons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_closed.png")));
            _treeIcons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.settings.png")));
            
            _treeView.ImageList = _treeIcons;
            _treeView.BeforeExpand += TreeViewOnBeforeExpand;
            _treeView.BeforeCollapse += TreeViewOnBeforeCollapse;
            _treeView.NodeMouseClick += TreeViewOnNodeMouseClick;
            _treeView.NodeMouseDoubleClick += TreeViewOnNodeMouseDoubleClick;

            _propertyGrid.PropertyValueChanged += OnPropertyGridPropertyValueChanged;

            InitOutputWindowContextMenu();

            Form = this;
        }

        public void Attach(IController controller)
        {
            _controller = controller;

            var updateMenus = new Action(UpdateMenus);
            var invokeUpdateMenus = new Action(() => Invoke(updateMenus));

            _controller.OnBuildStarted += invokeUpdateMenus;
            _controller.OnBuildFinished += invokeUpdateMenus;
            _controller.OnProjectLoading += invokeUpdateMenus;
            _controller.OnProjectLoaded += invokeUpdateMenus;

            var updateUndoRedo = new CanUndoRedoChanged(UpdateUndoRedo);
            var invokeUpdateUndoRedo = new CanUndoRedoChanged((u, r) => Invoke(updateUndoRedo, u, r));

            _controller.OnCanUndoRedoChanged += invokeUpdateUndoRedo;
            _controller.Selection.Modified += OnSelectionModified;
        }

        private void InitOutputWindowContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();

            MenuItem miCopy = new MenuItem("&Copy");
            miCopy.Click += (o, a) =>
            {
                if (!string.IsNullOrEmpty(_outputWindow.SelectedText))
                    Clipboard.SetText(_outputWindow.SelectedText);
            };

            MenuItem miSelectAll = new MenuItem("&Select all");
            miSelectAll.Click += (o, a) => _outputWindow.SelectAll();

            contextMenu.MenuItems.Add(miCopy);
            contextMenu.MenuItems.Add(miSelectAll);

            _outputWindow.ContextMenu = contextMenu;
        }

        public void OnTemplateDefined(ContentItemTemplate template)
        {
            // Load icon
            try
            {
                var iconPath = Path.Combine(Path.GetDirectoryName(template.TemplateFile), template.Icon);                
                var iconName = Path.GetFileNameWithoutExtension(iconPath);

                if (!EditorIcons.Templates.Images.ContainsKey(iconName))
                {
                    var iconImage = Image.FromFile(iconPath);
                    EditorIcons.Templates.Images.Add(iconName, iconImage);
                }

                template.Icon = iconName;
            }
            catch (Exception)
            {
                template.Icon = "Default";
            }
        }

        private void OnSelectionModified(Selection selection, object sender)
        {
            if (sender == this)
                return;

            _treeView.SelectedNodes = _controller.Selection.Select(FindNode);
        }

        private TreeNode FindNode(IProjectItem projectItem)
        {
            foreach (var n in _treeView.AllNodes())
            {
                var i = n.Tag as IProjectItem;
                if (i.OriginalPath == projectItem.OriginalPath)
                    return n;                
            }

            return null;
        }

        private void OnPropertyGridPropertyValueChanged(object s, PropertyValueChangedEventArgs args)
        {
            if (args.ChangedItem.Label == "References")
                _controller.OnReferencesModified();

            // TODO: This is the multi-select case which needs to be handled somehow to support undo.
            if (args.OldValue == null)
                return;

            var obj = _propertyGrid.SelectedObject;

            if (obj is ContentItem)
            {
                var item = obj as ContentItem;
                var action = new UpdateContentItemAction(this, _controller, item, args.ChangedItem.PropertyDescriptor, args.OldValue);
                _controller.AddAction(action);                
                _controller.OnProjectModified();
            }
            else
            {
                var item = (PipelineProject)_controller.GetItem((obj as PipelineProjectProxy).OriginalPath);
                var action = new UpdateProjectAction(this, _controller, item, args.ChangedItem.PropertyDescriptor, args.OldValue);
                _controller.AddAction(action);

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
                    TreeViewShowContextMenu(node, p);
                }
            }
        }

        private void TreeViewShowContextMenu(TreeNode node, Point contextMenuLocation)
        {
            if (!_treeView.SelectedNodes.Contains(node))
            {
                _treeView.SelectedNode = node;
            }

            if (node.Tag is ContentItem)
            {
                _treeAddItemMenuItem.Visible = false;
                _treeNewItemMenuItem.Visible = false;
            }
            else
            {
                _treeAddItemMenuItem.Visible = true;
                _treeNewItemMenuItem.Visible = true;
            }

            if (node.Tag is FolderItem)
            {
                _treeOpenFileMenuItem.Visible = false;
            }
            else
            {
                _treeOpenFileMenuItem.Visible = true;
            }

            _treeContextMenu.Show(_treeView, contextMenuLocation);
        }

        private void TreeViewOnNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs args)
        {
            // Even though we support 'Open File' as an action on the root (PipelineProject)
            // double clicking on it toggles whether it is expanded. 
            // So if you want to open it just use the menu.
            if (!(args.Node.Tag is ContentItem))
                return;

            ContextMenu_OpenFile_Click(sender, args);            
        }

        public void UpdateRecentProjectList()
        {
            _openRecentMenuItem.DropDownItems.Clear();

            foreach (var project in History.Default.ProjectHistory)
            {
                var recentItem = new ToolStripMenuItem(project);

                // We need a local to make the delegate work correctly.
                var localProject = project;
                recentItem.Click += (sender, args) => _controller.OpenProject(localProject);

                _openRecentMenuItem.DropDownItems.Insert(0, recentItem);
            }

            _openRecentMenuItem.Enabled = (_openRecentMenuItem.DropDownItems.Count >= 1);
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
            node.ImageIndex = item.Exists ? ContentItemIcon : ContentMissingIcon;
            node.SelectedImageIndex = item.Exists ? ContentItemIcon : ContentMissingIcon;

            _treeView.SelectedNode = node;

            root.Expand();
        }

        public void RemoveTreeItem(ContentItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            var node = _treeView.AllNodes().Find(f => f.Tag == item);
            if (node == null)
                return;

            var parent = node.Parent;
            node.Remove();

            {
                var obj = _propertyGrid.SelectedObject as ContentItem;
                if (obj != null && obj.OriginalPath == item.OriginalPath)
                    _propertyGrid.SelectedObject = null;
            }

            // Clean up the parent nodes without children
            // and be sure not to delete the root node.
            while (parent != null && parent.Parent != null && parent.Nodes.Count == 0)
            {
                var parentParent = parent.Parent;

                parent.Remove();

                {
                    var obj = _propertyGrid.SelectedObject as ContentItem;
                    if (obj != null && obj.OriginalPath == item.OriginalPath)
                        _propertyGrid.SelectedObject = null;
                }

                parent = parentParent;
            }            
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
            foreach (var obj in _controller.Selection)
            {
                if (obj.OriginalPath.Equals(item.OriginalPath, StringComparison.OrdinalIgnoreCase))
                {
                    _propertyGrid.Refresh();
                    _propertyGrid.ExpandAllGridItems();
                    break;
                }
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

        public Process CreateProcess(string exe, string commands)
        {
            var _buildProcess = new Process();
            _buildProcess.StartInfo.FileName = exe;
            _buildProcess.StartInfo.Arguments = commands;
            return _buildProcess;
        }

        private void ExitMenuItemClick(object sender, System.EventArgs e)
        {
            if (_controller.Exit())
                Application.Exit();
        }

        private void MainView_Load(object sender, EventArgs e)
        {            
            // We only load the History.StartupProject if there was not
            // already a project specified via command line.
            if (string.IsNullOrEmpty(OpenProjectPath))
            {
                var startupProject = History.Default.StartupProject;
                if (!string.IsNullOrEmpty(startupProject) && File.Exists(startupProject))                
                    OpenProjectPath = startupProject;                
            }

            History.Default.StartupProject = null;
            
            if (!string.IsNullOrEmpty(OpenProjectPath))
            {
                _controller.OpenProject(OpenProjectPath);
                OpenProjectPath = null;
            }
        }

        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (!_controller.Exit())
                    e.Cancel = true;
            }
        }        

        private void OnNewProjectClick(object sender, EventArgs e)
        {
            _controller.NewProject();
        }

        private void OnImportProjectClick(object sender, EventArgs e)
        {
            _controller.ImportProject();
        }

        private void OnOpenProjectClick(object sender, EventArgs e)
        {
            _controller.OpenProject();
        }

        private void OnCloseProjectClick(object sender, EventArgs e)
        {
            _controller.CloseProject();
        }

        private void OnSaveProjectClick(object sender, System.EventArgs e)
        {
            _controller.SaveProject(false);
        }

        private void OnSaveAsProjectClick(object sender, System.EventArgs e)
        {
            _controller.SaveProject(true);
        }

        private void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {            
            _controller.Selection.Clear(this);
            _propertyGrid.SelectedObject = null;

            foreach (var node in _treeView.SelectedNodes)
            {
                _controller.Selection.Add(node.Tag as IProjectItem, this);
            }

            _propertyGrid.SelectedObjects = _controller.Selection.ToArray();
            _propertyGrid.ExpandAllGridItems();
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
            _controller.LaunchDebugger = _debuggerMenuItem.Checked;
            _controller.Build(false);
        }

        private void RebuildMenuItemClick(object sender, EventArgs e)
        {
            _controller.LaunchDebugger = _debuggerMenuItem.Checked;
            _controller.Build(true);
        }

        private void RebuildItemsMenuItemClick(object sender, EventArgs e)
        {
            _controller.LaunchDebugger = _debuggerMenuItem.Checked;
            _controller.RebuildItems(_treeView.GetSelectedContentItems());
        }

        private void CleanMenuItemClick(object sender, EventArgs e)
        {
            _controller.LaunchDebugger = _debuggerMenuItem.Checked;
            _controller.Clean();
        }

        private void CancelBuildMenuItemClick(object sender, EventArgs e)
        {
            _controller.CancelBuild();
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

        private void TreeViewOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (_treeView.SelectedNode != null)
                {
                    Point nodeCoords = _treeView.PointToScreen(_treeView.SelectedNode.Bounds.Location);
                    TreeViewShowContextMenu(_treeView.SelectedNode, nodeCoords);
                }
            }
        }

        private void MainMenuMenuActivate(object sender, EventArgs e)
        {
            UpdateMenus();
        }

        private void UpdateMenus()
        {
            var notBuilding = !_controller.ProjectBuilding;
            var projectOpen = _controller.ProjectOpen;
            var projectOpenAndNotBuilding = projectOpen && notBuilding;

            // Update the state of all menu items.

            _newProjectMenuItem.Enabled = notBuilding;
            _openProjectMenuItem.Enabled = notBuilding;
            _importProjectMenuItem.Enabled = notBuilding;

            _saveMenuItem.Enabled = projectOpenAndNotBuilding && _controller.ProjectDirty;
            _saveAsMenuItem.Enabled = projectOpenAndNotBuilding;
            _closeMenuItem.Enabled = projectOpenAndNotBuilding;

            _exitMenuItem.Enabled = notBuilding;

            _newItemMenuItem.Enabled = projectOpen;
            _addItemMenuItem.Enabled = projectOpen;
            _deleteMenuItem.Enabled = projectOpen;

            _buildMenuItem.Enabled = projectOpenAndNotBuilding;

            _treeRebuildMenuItem.Enabled = _rebuildMenuItem.Enabled = projectOpenAndNotBuilding;
            _rebuildMenuItem.Enabled = _treeRebuildMenuItem.Enabled;

            _cleanMenuItem.Enabled = projectOpenAndNotBuilding;
            _cancelBuildSeparator.Visible = !notBuilding;
            _cancelBuildMenuItem.Enabled = !notBuilding;
            _cancelBuildMenuItem.Visible = !notBuilding;
      
            UpdateUndoRedo(_controller.CanUndo, _controller.CanRedo);
            UpdateRecentProjectList();
        }
        
        private void UpdateUndoRedo(bool canUndo, bool canRedo)
        {
            _undoMenuItem.Enabled = canUndo;
            _redoMenuItem.Enabled = canRedo;
        }

        private void OnDeleteItemClick(object sender, EventArgs e)
        {
            var items = new List<ContentItem>();
            var nodes = _treeView.SelectedNodesRecursive;

            foreach (var node in nodes)
            {
                var item = node.Tag as ContentItem;
                if (item != null && !items.Contains(item))
                    items.Add(item);                    
            }

            _controller.Exclude(items);      
        }

        private void ViewHelpMenuItemClick(object sender, EventArgs e)
        {
            Process.Start("http://www.monogame.net/documentation/?page=Pipeline");
        }

        private void AboutMenuItemClick(object sender, EventArgs e)
        {
            var about = new AboutDialog();
            about.Show();
        }

        private void OnAddItemClick(object sender, EventArgs e)
        {
            var node = _treeView.SelectedNode ?? _treeView.Nodes[0];
            var item = node.Tag as IProjectItem;
            _controller.Include(item.Location);
        }

        private void OnNewItemClick(object sender, System.EventArgs e)
        {
            var dlg = new NewContentDialog(_controller.Templates, EditorIcons.Templates);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                var template = dlg.Selected;
                var location = ((_treeView.SelectedNode ?? _treeView.Nodes[0]).Tag as IProjectItem).Location;

                // Ensure name is unique among files at this location?
                _controller.NewItem(dlg.NameGiven, location, template);
            }
        }

        private void OnRedoClick(object sender, EventArgs e)
        {
            _controller.Redo();
        }

        private void OnUndoClick(object sender, EventArgs e)
        {
            _controller.Undo();
        }

        private void ContextMenu_OpenFile_Click(object sender, EventArgs e)
        {
            var filePath = (_treeView.SelectedNode.Tag as IProjectItem).OriginalPath;
            filePath = _controller.GetFullPath(filePath);

            if (File.Exists(filePath))
            {
                Process.Start(filePath);
            }
        }

        private void ContextMenu_OpenFileLocation_Click(object sender, EventArgs e)
        {
            var filePath = (_treeView.SelectedNode.Tag as IProjectItem).OriginalPath;
            filePath = _controller.GetFullPath(filePath);

            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                Process.Start("explorer.exe", "/select, " + filePath);

            }
        }

        // http://stackoverflow.com/a/3955553/168235
        #region Custom Word-Wrapping (Output Window)
        
        const uint EM_SETWORDBREAKPROC = 0x00D0;

        [DllImport("user32.dll")]
        extern static IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

        delegate int EditWordBreakProc(IntPtr text, int pos_in_text, int bCharSet, int action);

        event EditWordBreakProc WordWrapCallbackEvent;
        
        private int WordWrapCallback(IntPtr text, int pos_in_text, int bCharSet, int action)
        {
            return 0;
        }
        #endregion

        private void MainView_Shown(object sender, EventArgs e)
        {
            WordWrapCallbackEvent = new EditWordBreakProc(WordWrapCallback);

            IntPtr ptr_func = Marshal.GetFunctionPointerForDelegate(WordWrapCallbackEvent);

            SendMessage(_outputWindow.Handle, EM_SETWORDBREAKPROC, IntPtr.Zero, ptr_func);
        }

        public void ItemExistanceChanged(IProjectItem item)
        {
            var path = item.Location;
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var root = _treeView.Nodes[0];
            var parent = root.Nodes;

            foreach (var folder in folders)
            {
                var found = parent.Find(folder, false);
                if (found.Length == 0)
                    return;

                parent = found[0].Nodes;
            }

            for (int i = 0; i < parent.Count; i++)
            {
                if (parent[i].Text == item.Name)
                {
                    if (parent[i].ImageIndex == ContentItemIcon || parent[i].ImageIndex == ContentMissingIcon)
                    {
                        this.Invoke(new MethodInvoker(delegate()
                        {
                            parent[i].ImageIndex = item.Exists ? ContentItemIcon : ContentMissingIcon;
                            parent[i].SelectedImageIndex = item.Exists ? ContentItemIcon : ContentMissingIcon;
                        }));
                    }
                }
            }
        }
    }
}
