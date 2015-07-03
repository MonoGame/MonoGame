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

        public static IController _controller;
        private ContentIcons _treeIcons;

        private bool _treeUpdating;
        private bool _treeSort;

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

            _treeIcons = new ContentIcons();
            
            _treeView.ImageList = _treeIcons.Icons;
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

            var cmv = _controller.GetContextMenuVisibilityInfo();

            _treeOpenFileMenuItem.Visible = cmv.Open;
            _treeAddMenu.Visible = cmv.Add;
            _treeSeparator1.Visible = cmv.Open || cmv.Add;
            _treeRebuildMenuItem.Visible = cmv.Rebuild;
            _treeOpenFileLocationMenuItem.Visible = cmv.OpenFileLocation;
            _treeRenameMenuItem.Visible = cmv.Rename;
            _treeDeleteMenuItem.Visible = cmv.Delete;

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

            _propertyGrid.SelectedObject = null;

            if(item == null)
            {
                _treeView.Nodes.Clear();
                return;
            }

            var project = item as PipelineProject;
            if (project == null)
                return;

            TreeNode root;

            if (_treeView.Nodes.Count == 0)
                root = _treeView.Nodes.Add(string.Empty, item.Name, -1);
            else
                root = _treeView.Nodes[0];

            root.Tag = new PipelineProjectProxy(project);
            root.SelectedImageIndex = ContentIcons.ProjectIcon;
            root.ImageIndex = ContentIcons.ProjectIcon;
            root.Text = item.Name;

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
                    folderNode.ImageIndex = ContentIcons.FolderClosedIcon;
                    folderNode.SelectedImageIndex = ContentIcons.FolderClosedIcon;

                    var idx = path.IndexOf(folder);
                    var curPath = path.Substring(0, idx + folder.Length);
                    folderNode.Tag = new FolderItem(curPath);
                    
                    parent = folderNode.Nodes;
                }
                else
                    parent = found[0].Nodes;
            }

            string fullPath = ((PipelineController)_controller).GetFullPath(item.OriginalPath);
            int iconIdx = _treeIcons.GetIcon(item.Exists, fullPath);

            var node = parent.Add(string.Empty, item.Name, -1);
            node.Tag = item;
            node.ImageIndex = iconIdx;
            node.SelectedImageIndex = iconIdx;

            _treeView.SelectedNode = node;
        }

        public void RemoveTreeItem(ContentItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            var node = _treeView.AllNodes().Find(f => f.Tag == item);
            if (node == null)
                return;

            var parent = node.Parent;
            node.Remove();

            var obj = _propertyGrid.SelectedObject as ContentItem;
            if (obj != null && obj.OriginalPath == item.OriginalPath)
                _propertyGrid.SelectedObject = null;
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
            if (e.Node.ImageIndex == ContentIcons.FolderOpenIcon)
            {
                e.Node.ImageIndex = ContentIcons.FolderClosedIcon;
                e.Node.SelectedImageIndex = ContentIcons.FolderClosedIcon;
            }
        }

        private void TreeViewOnBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.ImageIndex == ContentIcons.FolderClosedIcon)
            {
                e.Node.ImageIndex = ContentIcons.FolderOpenIcon;
                e.Node.SelectedImageIndex = ContentIcons.FolderOpenIcon;
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
            var ms = _controller.GetMenuSensitivityInfo();

            _newProjectMenuItem.Enabled = ms.New;
            _openProjectMenuItem.Enabled = ms.Open;
            _importProjectMenuItem.Enabled = ms.Import;
            _saveMenuItem.Enabled = ms.Save;
            _saveAsMenuItem.Enabled = ms.SaveAs;
            _closeMenuItem.Enabled = ms.Close;
            _exitMenuItem.Enabled = ms.Exit;
            _addMenuItem.Enabled = ms.Add;
            _deleteMenuItem.Enabled = ms.Delete;
            _renameMenuItem.Enabled = ms.Rename;
            _buildMenuItem.Enabled = ms.Build;
            _rebuildMenuItem.Enabled = ms.Rebuild;
            _cleanMenuItem.Enabled = ms.Clean;
            _cancelBuildMenuItem.Enabled = _cancelBuildMenuItem.Visible = _cancelBuildSeparator.Visible = ms.Cancel;
      
            UpdateUndoRedo(_controller.CanUndo, _controller.CanRedo);
        }
        
        private void UpdateUndoRedo(bool canUndo, bool canRedo)
        {
            _undoMenuItem.Enabled = canUndo;
            _redoMenuItem.Enabled = canRedo;
        }

        private void OnDeleteItemClick(object sender, EventArgs e)
        {
            _controller.Delete();    
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
            _controller.Include();
        }

        private void OnNewItemClick(object sender, System.EventArgs e)
        {
            _controller.NewItem();
        }

        private void OnAddFolderClick(object sender, EventArgs e)
        {
            _controller.IncludeFolder();
        }

        private void OnNewFolderClick(object sender, EventArgs e)
        {
            _controller.NewFolder();
        }

        private void OnRedoClick(object sender, EventArgs e)
        {
            _controller.Redo();
        }

        private void OnUndoClick(object sender, EventArgs e)
        {
            _controller.Undo();
        }

        private void OnRenameItemClick(object sender, EventArgs e)
        {
            FileType type = FileType.Base;

            var item = (_treeView.SelectedNode.Tag as IProjectItem);
            string path = item.OriginalPath;

            if (_treeView.SelectedNode.Tag is ContentItem)
                type = FileType.File;
            else if (_treeView.SelectedNode.Tag is FolderItem)
                type = FileType.Folder;
            else
                path = item.Name;

            TextEditDialog dialog = new TextEditDialog("Rename", "New Name:", _treeView.SelectedNode.Text);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string newpath = System.IO.Path.GetDirectoryName(path) + System.IO.Path.DirectorySeparatorChar + dialog.text;
                _controller.Move(new [] { path }, new [] { newpath.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? newpath.Substring(1) : newpath }, new[] { type });
            }
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
                    if (parent[i].ImageIndex >= ContentIcons.MaxDefinedIconIndex || parent[i].ImageIndex == ContentIcons.ContentMissingIcon)
                    {
                        this.Invoke(new MethodInvoker(delegate()
                        {
                            string fullPath = ((PipelineController)_controller).GetFullPath(item.OriginalPath);
                            int iconIdx = _treeIcons.GetIcon(item.Exists, fullPath);

                            parent[i].ImageIndex = iconIdx;
                            parent[i].SelectedImageIndex = iconIdx;
                        }));
                    }
                }
            }
        }

        public bool CopyOrLinkFile(string file, bool exists, out CopyAction action, out bool applyforall)
        {
            AddFileDialog afd = new AddFileDialog(file, exists);
            if (afd.ShowDialog() == DialogResult.OK)
            {
                action = afd.responce;
                applyforall = afd.applyforall;
                return true;
            }

            action = CopyAction.Skip;
            applyforall = false;
            return false;
        }

        public void AddTreeFolder(string afolder)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");
            _treeSort = true;

            var path = afolder;
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var root = _treeView.Nodes[0];
            var parent = root.Nodes;
            foreach (var folder in folders)
            {
                var found = parent.Find(folder, false);
                if (found.Length == 0)
                {
                    var folderNode = parent.Add(folder, folder, -1);
                    folderNode.ImageIndex = ContentIcons.FolderClosedIcon;
                    folderNode.SelectedImageIndex = ContentIcons.FolderClosedIcon;

                    var idx = path.IndexOf(folder);
                    var curPath = path.Substring(0, idx + folder.Length);
                    folderNode.Tag = new FolderItem(curPath);

                    parent = folderNode.Nodes;
                }
                else
                    parent = found[0].Nodes;
            }
        }

        public void RemoveTreeFolder(string folder)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");
            _treeSort = true;

            var path = folder;
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var root = _treeView.Nodes[0];
            var parent = root.Nodes;

            for (int i = 0; i < folders.Length;i++)
            {
                var found = parent.Find(folders[i], false);

                if (found.Length == 0)
                    return;
                else if (i != folders.Length - 1)
                    parent = found[0].Nodes;
                else
                    parent.Remove(found[0]);
            }
        }

        public bool ChooseContentFolder(string initialDirectory, out string folder)
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = initialDirectory;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                folder = dialog.SelectedPath;
                return true;
            }

            folder = "";
            return false;
        }

        public bool CopyOrLinkFolder(string folder, bool exists, out CopyAction action, out bool applyforall)
        {
            var dialog = new AddFolderDialog(folder);
            applyforall = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                action = dialog.responce;
                return true;
            }

            action = CopyAction.Link;
            return false;
        }

        public bool ChooseItemTemplate(out ContentItemTemplate template, out string name)
        {
            var dlg = new NewContentDialog(_controller.Templates, EditorIcons.Templates);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                template = dlg.Selected;
                name = dlg.NameGiven;
                return true;
            }

            template = null;
            name = null;
            return false;
        }

        public bool ChooseName(string title, string text, string oldname, bool docheck, out string newname)
        {
            var dialog = new TextEditDialog(title, text, oldname);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                newname = dialog.text;
                return true;
            }

            newname = "";
            return false;
        }

        public void ReloadRecentList(List<string> paths)
        {
            _openRecentMenuItem.DropDownItems.Clear();

            foreach (var project in paths)
            {
                var recentItem = new ToolStripMenuItem(project);

                // We need a local to make the delegate work correctly.
                var localProject = project;
                recentItem.Click += (sender, args) => _controller.OpenProject(localProject);

                _openRecentMenuItem.DropDownItems.Insert(0, recentItem);
            }

            _openRecentMenuItem.Enabled = (_openRecentMenuItem.DropDownItems.Count >= 1);
        }

        public void ExpandPath(string path)
        {
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var root = _treeView.Nodes[0];
            var parent = root.Nodes;

            root.Expand();

            foreach (var folder in folders)
            {
                var found = parent.Find(folder, false);
                if (found.Length == 0)
                    return;

                parent = found[0].Nodes;
                found[0].Expand();
            }
        }

        public bool GetSelection(out FileType fileType, out string path, out string location)
        {
            var node = _treeView.SelectedNode ?? _treeView.Nodes[0];
            var item = (_treeView.SelectedNodes.Count() != 1) ? _treeView.Nodes[0].Tag : node.Tag;

            if (item is ContentItem)
                fileType = FileType.File;
            else if (item is FolderItem)
                fileType = FileType.Folder;
            else
                fileType = FileType.Base;

            path = (fileType == FileType.Base) ? "" : (item as IProjectItem).OriginalPath;
            location = (fileType == FileType.Base) ? "" : (item as IProjectItem).Location;

            return _treeView.SelectedNodes.Count() == 1;
        }

        public bool GetSelection(out FileType[] fileType, out string[] path, out string[] location)
        {
            var types = new List<FileType>();
            var paths = new List<string>();
            var locations = new List<string>();

            foreach (var node in _treeView.SelectedNodes)
            {
                var item = node.Tag;
                FileType tmp_type = FileType.Base;

                if (item is ContentItem)
                    tmp_type = FileType.File;
                else if (item is FolderItem)
                    tmp_type = FileType.Folder;
                    
                types.Add(tmp_type);
                paths.Add((tmp_type == FileType.Base) ? "" : (item as IProjectItem).OriginalPath);
                locations.Add((tmp_type == FileType.Base) ? "" : (item as IProjectItem).Location);
            }

            fileType = types.ToArray();
            path = paths.ToArray();
            location = locations.ToArray();

            return _treeView.SelectedNodes.Any();
        }

        public List<ContentItem> GetChildItems(string path)
        {
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var root = _treeView.Nodes[0];
            var parent = root;

            foreach (var folder in folders)
            {
                var found = parent.Nodes.Find(folder, false);
                if (found.Length == 0)
                    return new List<ContentItem>();

                parent = found[0];
            }

            return GetItems(parent);
        }

        private List<ContentItem> GetItems(TreeNode node)
        {
            var items = new List<ContentItem>();
            
            foreach (TreeNode n in node.Nodes)
                items.AddRange(GetItems(n));

            if (node.Tag is ContentItem)
                items.Add(node.Tag as ContentItem);

            return items;
        }

        #region drag & drop
        private void MainView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            string filename = GetDropFile(e.Data, ".mgcb");
            if (filename != null) 
                e.Effect = DragDropEffects.Copy;
        }

        private void MainView_DragDrop(object sender, DragEventArgs e)
        {
            string filename = GetDropFile(e.Data, ".mgcb");
            if (filename != null)
                _controller.OpenProject(filename);
        }

        private string GetDropFile(IDataObject dataObject, string extension)
        {
            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
                foreach (var filename in files)
                {
                    if (Path.GetExtension(filename).Equals(extension, StringComparison.OrdinalIgnoreCase))
                        return filename;
                }
            }
            return null;
        }
      
        #endregion
    }
}
