using System.Collections.Generic;
using System.Diagnostics;
using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
	partial class MainWindow : Gtk.Window, IView
	{
		public string OpenProjectPath;

		public IController _controller;

		FileFilter MonoGameContentProjectFileFilter;
		FileFilter XnaContentProjectFileFilter;
		FileFilter AllFilesFilter;

		MenuItem treerebuild;

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

			if (!String.IsNullOrEmpty(OpenProjectPath)) {
				_controller.OpenProject(OpenProjectPath);
				OpenProjectPath = null;
			}

			treerebuild = new MenuItem ("Rebuild");
			projectview1.Initalize (this, treerebuild);
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
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
			if (item != null)
				projectview1.SetBaseIter (System.IO.Path.GetFileNameWithoutExtension (item.OriginalPath));
			else {
				projectview1.SetBaseIter ("");
				projectview1.Close ();
				UpdateMenus ();
			}
		}

		public void AddTreeItem (IProjectItem item)
		{
			projectview1.AddItem (projectview1.GetBaseIter(), item.OriginalPath);
		}

		public void RemoveTreeItem (ContentItem contentItem)
		{
			projectview1.RemoveItem (projectview1.GetBaseIter (), contentItem.OriginalPath);
		}

		public void UpdateTreeItem (IProjectItem item)
		{
			Console.WriteLine (item.OriginalPath);
			//throw new NotImplementedException();
		}

		public void EndTreeUpdate ()
		{

		}

		public void UpdateProperties (IProjectItem item)
		{

		}

		public void OutputAppend (string text)
		{
			if (text == null)
				return;

			Gtk.Application.Invoke (delegate { 
				textview2.Buffer.Text += text;
			});
		}

		public void OutputClear ()
		{
			Gtk.Application.Invoke (delegate { 
				textview2.Buffer.Text += "";
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

			return result;
		}

		public void OnTemplateDefined(ContentItemTemplate item)
		{
		}

		private string ReplaceCharacters(string fileName)
		{
			return fileName.Replace (" ", "\\ ").Replace ("(", "\\(").Replace (")", "\\)");
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
				_buildProcess.StartInfo.FileName = "/bin/bash";
				_buildProcess.StartInfo.Arguments = "-c \"cd " + OpenProjectPath + " && mono " + ReplaceCharacters(AppDomain.CurrentDomain.BaseDirectory.ToString()) + "/MGCB.exe " + commands + "\"";

				_buildProcess.StartInfo.UseShellExecute = false; 
				_buildProcess.StartInfo.RedirectStandardOutput = true;
			}
			return _buildProcess;
		}

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
		}

		protected void OnSaveAsActionActivated (object sender, EventArgs e)
		{
			_controller.SaveProject(true);
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
			
		}

		public void OnAddItemActionActivated (object sender, EventArgs e)
		{

		}

		public void OnDeleteActionActivated (object sender, EventArgs e)
		{
			projectview1.Remove ();
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
			Process.Start("http://www.monogame.net/documentation/");
		}

		protected void OnAboutActionActivated (object sender, EventArgs e)
		{
			Process.Start("http://www.monogame.net/about/");
		}

		public void UpdateMenus()
		{
			var notBuilding = !_controller.ProjectBuilding;
			var projectOpen = _controller.ProjectOpen;
			var projectOpenAndNotBuilding = projectOpen && notBuilding;

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
			DeleteAction.Sensitive = projectOpen;

			BuildAction.Sensitive = projectOpenAndNotBuilding;

			treerebuild.Sensitive = RebuildAction.Sensitive = projectOpenAndNotBuilding;
			RebuildAction.Sensitive = treerebuild.Sensitive;

			CleanAction.Sensitive = projectOpenAndNotBuilding;
			CancelBuildAction.Sensitive = !notBuilding;
			CancelBuildAction.Visible = !notBuilding;

			UpdateUndoRedo(_controller.CanUndo, _controller.CanRedo);
			//UpdateRecentProjectList();
		}

		private void UpdateUndoRedo(bool canUndo, bool canRedo)
		{
			UndoAction.Sensitive = canUndo;
			RedoAction.Sensitive = canRedo;
		}

		protected void OnFileActionActivated (object sender, EventArgs e)
		{
			var notBuilding = !_controller.ProjectBuilding;
			var projectOpen = _controller.ProjectOpen;
			var projectOpenAndNotBuilding = projectOpen && notBuilding;
			SaveAction.Sensitive = projectOpenAndNotBuilding && _controller.ProjectDirty;
		}
		#endregion
	}
}

