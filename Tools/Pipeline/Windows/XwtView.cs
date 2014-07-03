using System;
using System.Collections.Generic;
using Xwt;
using System.IO;
using System.Windows.Forms;

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

		public XwtView ()
		{
			generateUI ();

			// DialogFilters
			MonoGameContentProjectFileFilter = new FileDialogFilter("MonoGame Content Build Files (*.mgcb)", "*.mgcb");
			XnaContentProjectFileFilter = new FileDialogFilter ("XNA Content Projects (*.contentproj)", "*.contentproj");
		}

		protected override bool OnCloseRequested ()
		{
			if (!_controller.Exit ())
				return false;

			return base.OnCloseRequested ();
		}

		protected override void OnShown ()
		{
			base.OnShown ();

			if (!String.IsNullOrEmpty (OpenProjectPath)) {
				_controller.OpenProject (OpenProjectPath);
				OpenProjectPath = null;
			}
		}


		public void Attach (IController controller)
		{
			_controller = controller;
			throw new NotImplementedException ();
		}

		public AskResult AskSaveOrCancel ()
		{
			var result = MessageDialog.AskQuestion ("Do you want to save the project first?", Command.Yes, Command.No, Command.Cancel);

			if (result == Command.Yes)
				return AskResult.Yes;

			if (result == Command.No)
				return AskResult.No;

			return AskResult.Cancel;
		}

		public bool AskSaveName (ref string filePath, string title)
		{
			SaveFileDialog save = new SaveFileDialog () {
				Title = title,
				CurrentFolder = Path.GetDirectoryName (filePath),
				InitialFileName = Path.GetFileName(filePath),
				ActiveFilter = new FileDialogFilter("MonoGame Content Build Files", "*.mgcb"),
			};
			var result = save.Run ();

			filePath = save.FileName;

			return result;
		}
			

		public bool AskOpenProject (out string projectFilePath)
		{
			OpenFileDialog open = new OpenFileDialog ();
			open.Multiselect = false;
			open.Filters.Add (MonoGameContentProjectFileFilter);

			var result = open.Run ();
			projectFilePath = open.CurrentFolder;

			return result;
		}

		public bool AskImportProject (out string projectFilePath)
		{
			OpenFileDialog open = new OpenFileDialog ();
			open.Multiselect = false;
			open.Filters.Add (XnaContentProjectFileFilter);

			var result = open.Run ();
			projectFilePath = open.CurrentFolder;

			return result;
		}

		public void ShowError (string title, string message)
		{
			MessageDialog.ShowError (title, message);
		}

		public void ShowMessage (string message)
		{
			MessageDialog.ShowMessage (message);
		}

		public void BeginTreeUpdate ()
		{
			throw new NotImplementedException ();
		}

		public void SetTreeRoot (IProjectItem item)
		{
			throw new NotImplementedException ();
		}

		public void AddTreeItem (IProjectItem item)
		{
			throw new NotImplementedException ();
		}

		public void RemoveTreeItem (ContentItem contentItem)
		{
			throw new NotImplementedException ();
		}

		public void UpdateTreeItem (IProjectItem item)
		{
			throw new NotImplementedException ();
		}

		public void EndTreeUpdate ()
		{
			throw new NotImplementedException ();
		}

		public void UpdateProperties (IProjectItem item)
		{
			throw new NotImplementedException ();
		}

		public void OutputAppend (string text)
		{
			throw new NotImplementedException ();
		}

		public void OutputClear ()
		{
			throw new NotImplementedException ();
		}

		public bool ChooseContentFile (string initialDirectory, out List<string> files)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Generates the UI
		/// </summary>
		/// <remarks>>
		/// As XWT doesn't have a UI designer yet, it needs to be setted up by brute force
		/// </remarks>
		void generateUI ()
		{
			createMenu ();
		}

		void createMenu ()
		{
			// TODO: Implement click event handlers
			MainMenu = new Menu ();
			var separator = new SeparatorMenuItem ();

			var fileMenu = new MenuItem () {
				Label = "_File",
				SubMenu = new Menu ()
			};
					
			var newMenu = new MenuItem ("New...");
			var openMenu = new MenuItem ("Open...");
			var closeMenu = new MenuItem ("Close");
			var importMenu = new MenuItem ("Import...");
			var saveMenu = new MenuItem ("Save");
			var saveAsMenu = new MenuItem ("Save as...");
			var exitMenu = new MenuItem ("Exit");

			fileMenu.SubMenu.Items.Add (newMenu);
			fileMenu.SubMenu.Items.Add (openMenu);
			fileMenu.SubMenu.Items.Add (closeMenu);
			fileMenu.SubMenu.Items.Add (separator);
			fileMenu.SubMenu.Items.Add (importMenu);
			fileMenu.SubMenu.Items.Add (separator);
			fileMenu.SubMenu.Items.Add (saveMenu);
			fileMenu.SubMenu.Items.Add (saveAsMenu);
			fileMenu.SubMenu.Items.Add (separator);
			fileMenu.SubMenu.Items.Add (exitMenu);

			MainMenu.Items.Add (fileMenu);

			var editMenu = new MenuItem () {
				Label = "_Edit",
				SubMenu = new Menu ()
			};

			var undoMenu = new MenuItem ("Undo");
			var redoMenu = new MenuItem ("Redo");
			var newItemMenu = new MenuItem ("_New Item");
			var addItemMenu = new MenuItem ("Add Item");
			var delMenu = new MenuItem ("_Delete");

			editMenu.SubMenu.Items.Add (undoMenu);
			editMenu.SubMenu.Items.Add (redoMenu);
			editMenu.SubMenu.Items.Add (separator);
			editMenu.SubMenu.Items.Add (newItemMenu);
			editMenu.SubMenu.Items.Add (addItemMenu);
			editMenu.SubMenu.Items.Add (separator);
			editMenu.SubMenu.Items.Add (delMenu);

			MainMenu.Items.Add (editMenu);

			var buildMenu = new MenuItem() {
				Label = "_Build",
				SubMenu = new Menu()
			};

			var buildMenuItem = new MenuItem("_Build");
			var rebuildMenu = new MenuItem("_Rebuild");
			var cleanMenu = new MenuItem("_Clean");
			var debugMenu = new CheckBoxMenuItem ("Debug Mode");

			buildMenu.SubMenu.Items.Add (buildMenuItem);
			buildMenu.SubMenu.Items.Add (rebuildMenu);
			buildMenu.SubMenu.Items.Add (cleanMenu);
			buildMenu.SubMenu.Items.Add (separator);
			buildMenu.SubMenu.Items.Add (debugMenu);

			MainMenu.Items.Add (buildMenu);

			var helpMenu = new MenuItem () {
				Label = "_Help",
				SubMenu = new Menu ()
			};

			var viewHelpMenu = new MenuItem ("View Help");
			var aboutMenu = new MenuItem ("_About");

			helpMenu.SubMenu.Items.Add (viewHelpMenu);
			helpMenu.SubMenu.Items.Add (separator);
			helpMenu.SubMenu.Items.Add (aboutMenu);

			MainMenu.Items.Add (helpMenu);
		}
	}
}

