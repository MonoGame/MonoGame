using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xwt;
using Xwt.Drawing;

namespace MonoGame.Tools.Pipeline
{
	class XwtView : Window, IView
	{
		/// <summary>
		/// The project which will be opened as soon as a controller is attached.
		/// Is used when PipelineTool is launched to open a project, provided by the command line.
		/// </summary>
		public string OpenProjectPath;

		IController _controller;

		FileDialogFilter MonoGameContentProjectFileFilter;
		FileDialogFilter XnaContentProjectFileFilter;
        FileDialogFilter AllFilesFilter;

		public XwtView()
		{
			GenerateUI();

			// DialogFilters
			MonoGameContentProjectFileFilter = new FileDialogFilter("MonoGame Content Build Projects (*.mgcb)", "*.mgcb");
			XnaContentProjectFileFilter = new FileDialogFilter("XNA Content Projects (*.contentproj)", "*.contentproj");
			AllFilesFilter = new FileDialogFilter("All Files (*.*)", "*.*");
		}

		protected override bool OnCloseRequested ()
		{
			if (!_controller.Exit())
				return false;

			return base.OnCloseRequested();
		}

		protected override void OnClosed ()
		{
			base.OnClosed();
			Application.Exit();
		}

		protected override void OnShown ()
		{
			base.OnShown();

			if (!String.IsNullOrEmpty(OpenProjectPath)) {
				_controller.OpenProject(OpenProjectPath);
				OpenProjectPath = null;
			}
		}

		#region IView implements

		public void Attach (IController controller)
		{
			_controller = controller;

			_controller.OnBuildStarted += UpdateMenus;
			_controller.OnBuildFinished += UpdateMenus;
			_controller.OnProjectLoading += UpdateMenus;
			_controller.OnProjectLoaded += UpdateMenus;

			_controller.OnCanUndoRedoChanged += UpdateUndoRedo;
//			throw new NotImplementedException ();
		}

		public AskResult AskSaveOrCancel ()
		{
			var result = MessageDialog.AskQuestion("Do you want to save the project first?", Command.Yes, Command.No, Command.Cancel);

			if (result == Command.Yes)
				return AskResult.Yes;

			if (result == Command.No)
				return AskResult.No;

			return AskResult.Cancel;
		}

		public bool AskSaveName (ref string filePath, string title)
		{
			SaveFileDialog save = new SaveFileDialog() {
				Title = title,
				CurrentFolder = Path.GetDirectoryName(filePath),
				InitialFileName = Path.GetFileName(filePath)
			};
			save.Filters.Add(MonoGameContentProjectFileFilter);
            save.Filters.Add(AllFilesFilter);
            var result = save.Run();

			filePath = save.FileName;

			return result;
		}


		public bool AskOpenProject (out string projectFilePath)
		{
			OpenFileDialog open = new OpenFileDialog()
                {
                    Title = "Open MGCB Project"
                };
			open.Multiselect = false;
			open.Filters.Add(MonoGameContentProjectFileFilter);
            open.Filters.Add(AllFilesFilter);

			var result = open.Run();
			projectFilePath = open.FileName;

			return result;
		}

		public bool AskImportProject (out string projectFilePath)
		{
			OpenFileDialog open = new OpenFileDialog()
                {
                    Title = "Import XNA Content Project"
                };
			open.Multiselect = false;
			open.Filters.Add(XnaContentProjectFileFilter);
            open.Filters.Add(AllFilesFilter);

			var result = open.Run();
			projectFilePath = open.FileName;

			return result;
		}

		public void ShowError (string title, string message)
		{
			MessageDialog.ShowError(title, message);
		}

		public void ShowMessage (string message)
		{
			MessageDialog.ShowMessage(message);
		}

		public void BeginTreeUpdate ()
		{
			Debug.Assert(_treeUpdating == false, "Must finish previous tree update!");
			_treeUpdating = true;
			_treeSort = false;
		}

		public void SetTreeRoot (IProjectItem item)
		{
			Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            _root = null;
            _store.Clear();

            var project = item as PipelineProject;
            if (project == null)
                return;

            _root = _store.AddNode().SetValue(nameCol, item.Name).SetValue(imgCol, _projectIcon).SetValue(tag, new PipelineProjectProxy(project));
		}

		public void AddTreeItem (IProjectItem item)
		{
			Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");
			_treeSort = true;

            var path = item.Location;
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var curPath = string.Empty;
            var parent = _root;
            foreach (var folder in folders)
            {
                curPath = Path.Combine(curPath, folder);
                bool valid = parent.MoveToFirst();
                bool found = false;
                while (valid && parent.GetValue(nameCol) != folder)
                    valid = parent.MoveNext();
                if (!found)
                {
                    parent = _store.AddNode(parent.CurrentPosition).SetValue(nameCol, folder).SetValue(imgCol, _folderClosedIcon).SetValue(tag, new FolderItem(curPath));
                }
                else
                {
                    parent.MoveToChild();
                }
            }
            _store.AddNode(parent.CurrentPosition).SetValue(nameCol, item.Name).SetValue(imgCol, _contentIcon).SetValue(tag, item);
        }

		public void RemoveTreeItem (ContentItem contentItem)
		{
			//throw new NotImplementedException();
		}

		public void UpdateTreeItem (IProjectItem item)
		{
			//throw new NotImplementedException();
		}

		public void EndTreeUpdate ()
		{
			if (_treeSort)
            {
				// Sort tree
			}
			_treeSort = false;

			_treeUpdating = false;
		}

		public void UpdateProperties (IProjectItem item)
		{
			//throw new NotImplementedException();
		}

		public void OutputAppend (string text)
		{
			if (text == null)
				return;

            Application.Invoke(() => _outputWindow.Append(text));
        }

		public void OutputClear ()
		{
			Application.Invoke(() => _outputWindow.Clear());
		}

		public bool ChooseContentFile (string initialDirectory, out List<string> files)
		{
			var dlg = new OpenFileDialog() {
				Title = "Add Content Files",
				Multiselect = true,
				CurrentFolder = initialDirectory
			};
			dlg.Filters.Add(AllFilesFilter);

			bool result = dlg.Run();
			files = new List<string>(dlg.FileNames);
			return result;
		}

        public void OnTemplateDefined(ContentItemTemplate item)
        {
        }

        public Process CreateProcess(string exe, string commands)
        {
            var _buildProcess = new Process();
            if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
            {
                _buildProcess.StartInfo.FileName = exe;
                _buildProcess.StartInfo.Arguments = commands;
            }
            else
            {
                _buildProcess.StartInfo.FileName = "mono";
                _buildProcess.StartInfo.Arguments = string.Format("\"{0}\" {1}", exe, commands);
            }
            return _buildProcess;
        }

		#endregion


		Image _folderClosedIcon;
        //Image _folderOpenIcon;
		Image _projectIcon;
        Image _contentIcon;
		DataField<Image> imgCol = new DataField<Image>();
		DataField<string> nameCol = new DataField<string>();
		DataField<object> tag = new DataField<object>();

		TreeView _treeView;
        TreeStore _store;
        TreeNavigator _root;
		private bool _treeUpdating;
		private bool _treeSort;

		OutputTextView _outputWindow;

		/// <summary>
		/// Generates the UI
		/// </summary>
		/// <remarks>>
		/// As XWT doesn't have a UI designer yet, it needs to be setted up by brute force
		/// </remarks>
		void GenerateUI ()
		{
			CreateMenu();
			CreateContent();
		}

		#region Menu Bar

		// Under File Menu
		MenuItem _newMenuItem;
		MenuItem _openMenuItem;
		MenuItem _closeMenuItem;
		MenuItem _importMenuItem;
		MenuItem _saveMenuItem;
		MenuItem _saveAsMenuItem;
		MenuItem _exitMenuItem;

		// Under Edit Menu
		MenuItem _undoMenuItem;
		MenuItem _redoMenuItem;
		MenuItem _newItemMenuItem;
		MenuItem _addItemMenuItem;
		MenuItem _deleteMenuItem;

		// Under Build Menu
		MenuItem _buildMenu;
		MenuItem _buildMenuItem;
		MenuItem _rebuildMenuItem;
		MenuItem _cleanMenuItem;
		SeparatorMenuItem _cancelSeparatorMenuItem;
		MenuItem _cancelBuildMenuItem;
		CheckBoxMenuItem _debugMenuItem;

		void CreateMenu ()
		{
            Title = "MonoGame Pipeline";
            Width = 800;
            Height = 600;
            
            // TODO: Implement click event handlers
			MainMenu = new Menu();

			var fileMenu = new MenuItem() {
				Label = "File",
				SubMenu = new Menu()
			};
					
			_newMenuItem = new MenuItem("New...");
			_newMenuItem.Clicked += OnNewMenuClicked;
			_openMenuItem = new MenuItem("Open...");
			_openMenuItem.Clicked += OnOpenMenuClicked;
			_closeMenuItem = new MenuItem("Close");
			_closeMenuItem.Sensitive = false;
			_closeMenuItem.Clicked += OnCloseMenuClicked;
			_importMenuItem = new MenuItem("Import...");
			_importMenuItem.Clicked += OnImportMenuClicked;
			_saveMenuItem = new MenuItem("Save");
			_saveMenuItem.Clicked += OnSaveMenuClicked;
			_saveAsMenuItem = new MenuItem("Save As...");
			_saveAsMenuItem.Clicked += OnSaveAsMenuClicked;
			_exitMenuItem = new MenuItem("Exit");
			_exitMenuItem.Clicked += OnExitMenuClicked;

			fileMenu.SubMenu.Items.Add(_newMenuItem);
			fileMenu.SubMenu.Items.Add(_openMenuItem);
			fileMenu.SubMenu.Items.Add(_closeMenuItem);
			fileMenu.SubMenu.Items.Add(new SeparatorMenuItem());
			fileMenu.SubMenu.Items.Add(_importMenuItem);
			fileMenu.SubMenu.Items.Add(new SeparatorMenuItem());
			fileMenu.SubMenu.Items.Add(_saveMenuItem);
			fileMenu.SubMenu.Items.Add(_saveAsMenuItem);
			fileMenu.SubMenu.Items.Add(new SeparatorMenuItem());
			fileMenu.SubMenu.Items.Add(_exitMenuItem);

			MainMenu.Items.Add(fileMenu);

			var editMenu = new MenuItem() {
				Label = "_Edit",
				SubMenu = new Menu()
			};
					
			_undoMenuItem = new MenuItem("Undo");
			_undoMenuItem.Clicked += OnUndoMenuClicked;
			_redoMenuItem = new MenuItem("Redo");
			_redoMenuItem.Clicked += OnRedoMenuClicked;
			_newItemMenuItem = new MenuItem("New Item");
			_addItemMenuItem = new MenuItem("Add Item");
			_deleteMenuItem = new MenuItem("Delete");

			editMenu.SubMenu.Items.Add(_undoMenuItem);
			editMenu.SubMenu.Items.Add(_redoMenuItem);
			editMenu.SubMenu.Items.Add(new SeparatorMenuItem());
			editMenu.SubMenu.Items.Add(_newItemMenuItem);
			editMenu.SubMenu.Items.Add(_addItemMenuItem);
			editMenu.SubMenu.Items.Add(new SeparatorMenuItem());
			editMenu.SubMenu.Items.Add(_deleteMenuItem);

			MainMenu.Items.Add(editMenu);

			_buildMenu = new MenuItem() {
				Label = "Build",
				SubMenu = new Menu()
			};

			_buildMenuItem = new MenuItem("Build");
			_buildMenuItem.Clicked += OnBuildMenuItemClicked;
			_rebuildMenuItem = new MenuItem("Rebuild");
			_rebuildMenuItem.Clicked += OnRebuildMenuClicked;
			_cleanMenuItem = new MenuItem("Clean");
			_cleanMenuItem.Clicked += OnCleanMenuClicked;
			_cancelSeparatorMenuItem = new SeparatorMenuItem();
			_cancelBuildMenuItem = new MenuItem() {
				Label = "Cancel",
				Visible = false
			};
			_cancelBuildMenuItem.Clicked += OnCancelMenuClicked;
			_debugMenuItem = new CheckBoxMenuItem("Debug Mode");

			_buildMenu.SubMenu.Items.Add(_buildMenuItem);
			_buildMenu.SubMenu.Items.Add(_rebuildMenuItem);
			_buildMenu.SubMenu.Items.Add(_cleanMenuItem);
			_buildMenu.SubMenu.Items.Add(_cancelBuildMenuItem);
			_buildMenu.SubMenu.Items.Add(new SeparatorMenuItem());
			_buildMenu.SubMenu.Items.Add(_debugMenuItem);

			MainMenu.Items.Add(_buildMenu);

			var helpMenu = new MenuItem() {
				Label = "Help",
				SubMenu = new Menu()
			};

			var viewHelpMenu = new MenuItem("View Help");
			viewHelpMenu.Clicked += OnViewHelpMenuClicked;
			var aboutMenu = new MenuItem("About");
			aboutMenu.Clicked += OnAboutMenuClicked;

			helpMenu.SubMenu.Items.Add(viewHelpMenu);
			helpMenu.SubMenu.Items.Add(new SeparatorMenuItem());
			helpMenu.SubMenu.Items.Add(aboutMenu);

			MainMenu.Items.Add(helpMenu);
		}

		void OnNewMenuClicked (object sender, EventArgs e)
		{
			_controller.NewProject();
		}

		void OnOpenMenuClicked (object sender, EventArgs e)
		{
			_controller.OpenProject();
		}

		void OnCloseMenuClicked (object sender, EventArgs e)
		{
			_controller.CloseProject();
		}

		void OnImportMenuClicked (object sender, EventArgs e)
		{
			_controller.ImportProject();
		}

		void OnSaveMenuClicked (object sender, EventArgs e)
		{
			_controller.SaveProject(false);
		}

		void OnSaveAsMenuClicked (object sender, EventArgs e)
		{
			_controller.SaveProject(true);
		}

		void OnExitMenuClicked (object sender, EventArgs e)
		{
			if (_controller.Exit())
				Application.Exit();
		}

		void OnUndoMenuClicked (object sender, EventArgs e)
		{
			_controller.Undo();
		}

		void OnRedoMenuClicked (object sender, EventArgs e)
		{
			_controller.Redo();
		}

		void OnBuildMenuItemClicked (object sender, EventArgs e)
		{
			_controller.LaunchDebugger = _debugMenuItem.Checked;
			_controller.Build(false);
		}

		void OnRebuildMenuClicked (object sender, EventArgs e)
		{
			_controller.LaunchDebugger = _debugMenuItem.Checked;
			_controller.Build(true);
		}

		void OnCancelMenuClicked (object sender, EventArgs e)
		{
			_controller.CancelBuild();
		}

		void OnCleanMenuClicked (object sender, EventArgs e)
		{
			_controller.LaunchDebugger = _debugMenuItem.Checked;
			_controller.Clean();
		}

		void OnViewHelpMenuClicked (object sender, EventArgs e)
		{
			Process.Start("http://www.monogame.net/documentation/");
		}

		void OnAboutMenuClicked (object sender, EventArgs e)
		{
			Process.Start("http://www.monogame.net/about/");
		}

		#endregion

		private void UpdateMenus()
		{
			var notBuilding = !_controller.ProjectBuilding;
			var projectOpen = _controller.ProjectOpen;
			var projectOpenAndNotBuilding = projectOpen && notBuilding;

			// Update the state of all menu items.
            Application.Invoke(() =>
                {
			        _newMenuItem.Sensitive = notBuilding;
			        _openMenuItem.Sensitive = notBuilding;
			        _importMenuItem.Sensitive = notBuilding;

			        _saveMenuItem.Sensitive = projectOpenAndNotBuilding && _controller.ProjectDirty;
			        _saveAsMenuItem.Sensitive = projectOpenAndNotBuilding;
			        _closeMenuItem.Sensitive = projectOpenAndNotBuilding;

			        _exitMenuItem.Sensitive = notBuilding;

			        _newItemMenuItem.Sensitive = projectOpen;
			        _addItemMenuItem.Sensitive = projectOpen;
			        _deleteMenuItem.Sensitive = projectOpen;

			        _buildMenuItem.Sensitive = projectOpenAndNotBuilding;

			        //_treeRebuildMenuItem.Enabled = _rebuildMenuItem.Enabled = projectOpenAndNotBuilding;
			        //_rebuildMenuItem.Sensitive = _treeRebuildMenuItem.Enabled;

			        _cleanMenuItem.Sensitive = projectOpenAndNotBuilding;
			        if (notBuilding) {
				        _buildMenu.SubMenu.Items.Remove(_cancelSeparatorMenuItem);
			        } else {
				        var items = _buildMenu.SubMenu.Items;
				        if (!items.Contains(_cancelSeparatorMenuItem)) {
					        int index = items.IndexOf(_cancelBuildMenuItem);
					        items.Insert(index, _cancelSeparatorMenuItem);
				        }
			        }
			        _cancelBuildMenuItem.Sensitive = !notBuilding;
			        _cancelBuildMenuItem.Visible = !notBuilding;
                });

			UpdateUndoRedo(_controller.CanUndo, _controller.CanRedo);
		}

		private void UpdateUndoRedo(bool canUndo, bool canRedo)
		{
            Application.Invoke(() =>
                {
                    _undoMenuItem.Sensitive = canUndo;
                    _redoMenuItem.Sensitive = canRedo;
                });
		}

		void CreateContent ()
		{
			var mainPaned = new HPaned();
            var leftPane = new VPaned();

            _store = new TreeStore(imgCol, nameCol, tag);
            _treeView = new TreeView(_store)
            {
				HeadersVisible = false,
			};
            _treeView.Columns.Add("Name", imgCol, nameCol);
            
            _contentIcon = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.blueprint.png"));
            _folderClosedIcon = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_closed.png"));
            //_folderOpenIcon = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_open.png"));
            _projectIcon = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.settings.png"));

            leftPane.Panel1.Content = _treeView;

            leftPane.Panel2.Content = new PropertyGrid();

			mainPaned.Panel1.Content = leftPane;
			_outputWindow = new OutputTextView();

			mainPaned.Panel2.Content = _outputWindow;
			Content = mainPaned;

            // Make the left pane 1/3 of the width by default
            mainPaned.Position = Width * 0.33;

            // No padding around the edge of the main window
            Padding = new WidgetSpacing();
		}
	}
}

