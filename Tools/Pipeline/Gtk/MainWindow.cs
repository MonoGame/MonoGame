using System.Collections.Generic;
using System.Diagnostics;
using System;
using Gtk;
using System.Reflection;
#if MONOMAC
using IgeMacIntegration;
#endif

namespace MonoGame.Tools.Pipeline
{
	partial class MainWindow : Gtk.Window, IView
	{
		public static string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 _.";

		public static bool CheckString(string s, string allowedCharacters)
		{
			for (int i = 0; i < s.Length; i++) 
				if (!allowedCharacters.Contains (s.Substring (i, 1)))
					return false;

			return true;
		}

		public string OpenProjectPath;
		public IController _controller;

		FileFilter MonoGameContentProjectFileFilter;
		FileFilter XnaContentProjectFileFilter;
		FileFilter AllFilesFilter;

		MenuItem treerebuild;
		MenuItem recentMenu;

		public MainWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			MonoGameContentProjectFileFilter = new FileFilter ();
			MonoGameContentProjectFileFilter.Name = "MonoGame Content Build Projects (*.mgcb)";
			MonoGameContentProjectFileFilter.AddPattern ("*.mgcb");

			XnaContentProjectFileFilter = new FileFilter ();
			XnaContentProjectFileFilter.Name = "XNA Content Projects (*.contentproj)";
			XnaContentProjectFileFilter.AddPattern ("*.contentproj");

			AllFilesFilter = new FileFilter ();
			AllFilesFilter.Name = "All Files (*.*)";
			AllFilesFilter.AddPattern ("*.*");

			Widget[] widgets = menubar1.Children;
			foreach (Widget w in widgets) {
				if(((MenuItem)w).Name == "FileAction")
				{
					Menu m = (Menu)((MenuItem)w).Submenu;
					foreach (Widget w2 in m.Children) 
						if (((MenuItem)w2).Name == "OpenRecentAction") 
							recentMenu = (MenuItem)w2;
				}
			}

			treerebuild = new MenuItem ("Rebuild");
			treerebuild.Activated += delegate {
				projectview1.Rebuild ();
			};
			projectview1.Initalize (this, treerebuild, propertiesview1);

			if (Assembly.GetEntryAssembly ().FullName.Contains ("Pipeline"))
				BuildMenu ();
			else {
				menubar1.Hide ();
				vbox2.Remove (menubar1);
			}

			propertiesview1.Initalize (this);
		}
			
		void BuildMenu() {

#if MONOMAC
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				IgeMacMenu.GlobalKeyHandlerEnabled = true;

				//Tell the IGE library to use your GTK menu as the Mac main menu
				IgeMacMenu.MenuBar = this.menubar1;

				//tell IGE which menu item should be used for the app menu's quit item
				//IgeMacMenu.QuitMenuItem = yourQuitMenuItem;

				//add a new group to the app menu, and add some items to it
				var appGroup = IgeMacMenu.AddAppMenuGroup ();
				appGroup.AddMenuItem (new MenuItem(), "About Pipeline...");

				//hide the menu bar so it no longer displays within the window
				menubar1.Hide ();
				vbox2.Remove (menubar1);

			}
#endif
		}

		public void OnShowEvent()
		{
			if (string.IsNullOrEmpty(OpenProjectPath))
			{
				var startupProject = History.Default.StartupProject;
				if (!string.IsNullOrEmpty(startupProject) && System.IO.File.Exists(startupProject))                
					OpenProjectPath = startupProject;                
			}

			History.Default.StartupProject = null;

			if (!String.IsNullOrEmpty(OpenProjectPath)) {
				_controller.OpenProject(OpenProjectPath);
				OpenProjectPath = null;
			}
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			if (_controller.Exit ()) 
				Application.Quit ();
			else
				a.RetVal = true;
		}

#region IView implements

		public void Attach (IController controller)
		{
			_controller = controller;
			propertiesview1.controller = _controller;

			_controller.OnBuildStarted += UpdateMenus;
			_controller.OnBuildFinished += UpdateMenus;
			_controller.OnProjectLoading += UpdateMenus;
			_controller.OnProjectLoaded += UpdateMenus;

			_controller.OnCanUndoRedoChanged += UpdateUndoRedo;
			UpdateMenus ();
		}

		public AskResult AskSaveOrCancel ()
		{
			Pipeline.YesNoCancelDialog dialog = new Pipeline.YesNoCancelDialog ("Question", "Do you want to save the project first?");
			dialog.TransientFor = this;
			var result = dialog.Run ();
			dialog.Destroy ();

			if (result == (int)ResponseType.Yes)
				return AskResult.Yes;

			if (result == (int)ResponseType.No)
				return AskResult.No;

			return AskResult.Cancel;
		}

		public bool AskSaveName (ref string filePath, string title)
		{
			Gtk.FileChooserDialog filechooser =
				new Gtk.FileChooserDialog(title,
					this,
					FileChooserAction.Save,
					"Cancel", ResponseType.Cancel,
					"Save", ResponseType.Accept);

			filechooser.AddFilter (MonoGameContentProjectFileFilter);
			filechooser.AddFilter (AllFilesFilter);

			var result = (filechooser.Run() == (int)ResponseType.Accept) ? true : false;
			filePath = filechooser.Filename;

            if (filechooser.Filter == MonoGameContentProjectFileFilter)
                filePath += ".mgcb";

			filechooser.Destroy ();
			return result;
		}

		public bool AskOpenProject (out string projectFilePath)
		{
			Gtk.FileChooserDialog filechooser =
				new Gtk.FileChooserDialog("Open MGCB Project",
					this,
					FileChooserAction.Open,
					"Cancel", ResponseType.Cancel,
					"Open", ResponseType.Accept);

			filechooser.AddFilter (MonoGameContentProjectFileFilter);
			filechooser.AddFilter (AllFilesFilter);

			var result = (filechooser.Run() == (int)ResponseType.Accept) ? true : false;
			projectFilePath = filechooser.Filename;
			filechooser.Destroy ();

			return result;
		}

		public bool AskImportProject (out string projectFilePath)
		{
			Gtk.FileChooserDialog filechooser =
				new Gtk.FileChooserDialog("Import XNA Content Project",
					this,
					FileChooserAction.Open,
					"Cancel", ResponseType.Cancel,
					"Open", ResponseType.Accept);

			filechooser.AddFilter (XnaContentProjectFileFilter);
			filechooser.AddFilter (AllFilesFilter);

			var result = (filechooser.Run() == (int)ResponseType.Accept) ? true : false;
			projectFilePath = filechooser.Filename;
			filechooser.Destroy ();

			return result;
		}

		public void ShowError (string title, string message)
		{
			MessageDialog dialog = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, message);
			dialog.Title = title;
			dialog.Run();
			dialog.Destroy ();
		}

		public void ShowMessage (string message)
		{
			MessageDialog dialog = new MessageDialog (this, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok, message);
			dialog.Title = "Info";
			dialog.Run();
			dialog.Destroy ();
		}

		public void BeginTreeUpdate ()
		{

		}

		public void SetTreeRoot (IProjectItem item)
		{
			if (item != null) {
				projectview1.openedProject = item.OriginalPath;
				projectview1.SetBaseIter (System.IO.Path.GetFileNameWithoutExtension (item.OriginalPath));
			}
			else {
				projectview1.SetBaseIter ("");
				projectview1.Close ();
				UpdateMenus ();
			}
		}

		public void AddTreeItem (IProjectItem item)
		{
            projectview1.AddItem (projectview1.GetBaseIter(), item.OriginalPath, item.Exists);
		}

		public void RemoveTreeItem (ContentItem contentItem)
		{
            projectview1.RemoveItem (projectview1.GetBaseIter (), contentItem.OriginalPath);
		}

		public void UpdateTreeItem (IProjectItem item)
        {

		}

		public void EndTreeUpdate ()
		{

		}

		public void UpdateProperties (IProjectItem item)
		{
            UpdateMenus ();
		}

		public void OutputAppend (string text)
		{
			if (text == null)
				return;

			Gtk.Application.Invoke (delegate { 
				textview2.Buffer.Text += text + "\r\n";
                UpdateMenus();
			});
		}

		public void OutputClear ()
		{
			Gtk.Application.Invoke (delegate { 
                textview2.Buffer.Text = "";
                UpdateMenus();
			});
		}

		public bool ChooseContentFile (string initialDirectory, out List<string> files)
		{
			Gtk.FileChooserDialog filechooser =
				new Gtk.FileChooserDialog("Add Content Files",
					this,
					FileChooserAction.Open,
					"Cancel", ResponseType.Cancel,
					"Open", ResponseType.Accept);
			filechooser.SelectMultiple = true;

			filechooser.AddFilter (AllFilesFilter);
			filechooser.SetCurrentFolder (initialDirectory);

			bool result = (filechooser.Run() == (int)ResponseType.Accept) ? true : false;

			files = new List<string>();
			files.AddRange (filechooser.Filenames);
			filechooser.Destroy ();

			return result;
		}

		public void OnTemplateDefined(ContentItemTemplate item)
		{

		}

		public void ItemExistanceChanged(IProjectItem item)
		{
			projectview1.RefreshItem(projectview1.GetBaseIter(), item.OriginalPath, item.Exists);
		}

		public Process CreateProcess(string exe, string commands)
		{
			var _buildProcess = new Process();
#if WINDOWS
			_buildProcess.StartInfo.FileName = exe;
			_buildProcess.StartInfo.Arguments = commands;
#endif
#if MONOMAC || LINUX
			_buildProcess.StartInfo.FileName = "mono";
			_buildProcess.StartInfo.Arguments = string.Format("\"{0}\" {1}", exe, commands);
#endif

			return _buildProcess;
		}
#endregion

		protected void OnNewActionActivated (object sender, EventArgs e)
		{
			_controller.NewProject();
		}

		protected void OnOpenActionActivated (object sender, EventArgs e)
		{
			_controller.OpenProject();
		}

		protected void OnCloseActionActivated (object sender, EventArgs e)
		{
			_controller.CloseProject();
		}

		protected void OnImportActionActivated (object sender, EventArgs e)
		{
			_controller.ImportProject();
		}

		protected void OnSaveActionActivated (object sender, EventArgs e)
		{
            _controller.SaveProject(false);
            UpdateMenus();
		}

		protected void OnSaveAsActionActivated (object sender, EventArgs e)
		{
            _controller.SaveProject(true);
            UpdateMenus();
		}

		protected void OnExitActionActivated (object sender, EventArgs e)
		{
			if (_controller.Exit ())
				Application.Quit ();
		}

		protected void OnUndoActionActivated (object sender, EventArgs e)
		{
			_controller.Undo ();
		}

		protected void OnRedoActionActivated (object sender, EventArgs e)
		{
			_controller.Redo ();
		}

		public void OnNewItemActionActivated (object sender, EventArgs e)
		{
			NewTemplateDialog dialog = new NewTemplateDialog(_controller.Templates.GetEnumerator ());
			dialog.TransientFor = this;

			if (dialog.Run () == (int)ResponseType.Ok) {

				List<TreeIter> iters;
				List<Gdk.Pixbuf> icons;
				string[] paths = projectview1.GetSelectedTreePath (out iters, out icons);

				string location = "";

				if (paths.Length == 1) {
					if (icons [0] == projectview1.ICON_FOLDER)
						location = paths [0];
					else if (icons[0] == projectview1.ICON_BASE)
						location = _controller.GetFullPath ("");
					else
						location = System.IO.Path.GetDirectoryName (paths [0]);
				}
				else
					location = _controller.GetFullPath ("");

                _controller.NewItem(dialog.name, location, dialog.templateFile);
                UpdateMenus();
			}
		}

		public void OnAddItemActionActivated (object sender, EventArgs e)
		{
			List<TreeIter> iters;
			List<Gdk.Pixbuf> icons;
			string[] paths = projectview1.GetSelectedTreePath (out iters, out icons);

			if (paths.Length == 1) {
				if (icons [0] == projectview1.ICON_FOLDER)
					_controller.Include (paths [0]);
				else if (icons[0] == projectview1.ICON_BASE)
					_controller.Include (_controller.GetFullPath (""));
				else
					_controller.Include (System.IO.Path.GetDirectoryName (paths [0]));
			}
			else
                _controller.Include (_controller.GetFullPath (""));
            UpdateMenus();
		}

		public void OnDeleteActionActivated (object sender, EventArgs e)
		{
            projectview1.Remove ();
            UpdateMenus();
		}

		protected void OnBuildAction1Activated (object sender, EventArgs e)
		{
			_controller.Build(false);
		}

		protected void OnRebuildActionActivated (object sender, EventArgs e)
		{
			_controller.Build(true);
		}

		protected void OnCleanActionActivated (object sender, EventArgs e)
		{
			_controller.Clean();
		}

		protected void OnViewHelpActionActivated (object sender, EventArgs e)
		{
			Process.Start("http://www.monogame.net/documentation/?page=Pipeline");
		}

		protected void OnAboutActionActivated (object sender, EventArgs e)
		{
			Process.Start("http://www.monogame.net/about/");
			AboutDialog adialog = new AboutDialog ();
			adialog.TransientFor = this;
			adialog.Run ();
		}

		public void UpdateMenus()
		{
            List<TreeIter> iters;
            List<Gdk.Pixbuf> icons;
            string[] paths = projectview1.GetSelectedTreePath (out iters, out icons);

			var notBuilding = !_controller.ProjectBuilding;
			var projectOpen = _controller.ProjectOpen;
            var projectOpenAndNotBuilding = projectOpen && notBuilding;
            var somethingSelected = paths.Length > 0;

			// Update the state of all menu items.

			NewAction.Sensitive = notBuilding;
			OpenAction.Sensitive = notBuilding;
			ImportAction.Sensitive = notBuilding;

			SaveAction.Sensitive = projectOpenAndNotBuilding && _controller.ProjectDirty;
			SaveAsAction.Sensitive = projectOpenAndNotBuilding;
			CloseAction.Sensitive = projectOpenAndNotBuilding;

			ExitAction.Sensitive = notBuilding;

			NewItemAction.Sensitive = projectOpen;
			AddItemAction.Sensitive = projectOpen;
			DeleteAction.Sensitive = projectOpen && somethingSelected;

            BuildAction.Sensitive = projectOpen;
            BuildAction1.Sensitive = projectOpenAndNotBuilding;

			treerebuild.Sensitive = RebuildAction.Sensitive = projectOpenAndNotBuilding;
			RebuildAction.Sensitive = treerebuild.Sensitive;

			CleanAction.Sensitive = projectOpenAndNotBuilding;
			CancelBuildAction.Sensitive = !notBuilding;
			CancelBuildAction.Visible = !notBuilding;

			UpdateUndoRedo(_controller.CanUndo, _controller.CanRedo);
			UpdateRecentProjectList();
		}

		public void UpdateRecentProjectList()
		{
			History.Default.Load ();
			recentMenu.Submenu = null;
			Menu m = new Menu ();

			int nop = 0;

			foreach (var project in History.Default.ProjectHistory)
			{
				nop++;
				var recentItem = new MenuItem(project);

				// We need a local to make the delegate work correctly.
				var localProject = project;
				recentItem.Activated += (sender, args) => _controller.OpenProject(localProject);

				m.Insert (recentItem, 0);
			}
				
			if (nop > 0) {
				m.Add (new SeparatorMenuItem ());
				MenuItem item = new MenuItem ("Clear");
				item.Activated += delegate {
					History.Default.Clear ();
					UpdateRecentProjectList ();
				};
				m.Add (item);

				recentMenu.Submenu = m;
				m.ShowAll ();
			}

            recentMenu.Sensitive = nop > 0;
			menubar1.ShowAll ();
		}

		private void UpdateUndoRedo(bool canUndo, bool canRedo)
		{
			UndoAction.Sensitive = canUndo;
			RedoAction.Sensitive = canRedo;
		}

		protected void OnFileActionActivated (object sender, EventArgs e)
		{

		}

		protected void OnBuildActionActivated (object sender, EventArgs e)
		{

		}
	}
}

