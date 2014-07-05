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

		public XwtView ()
		{
			generateUI ();

			// DialogFilters
			MonoGameContentProjectFileFilter = new FileDialogFilter("MonoGame Content Build Files (*.mgcb)", "*.mgcb");
			XnaContentProjectFileFilter = new FileDialogFilter ("XNA Content Projects (*.contentproj)", "*.contentproj");
		}

		protected override bool OnCloseRequested ()
		{
            //if (!_controller.Exit ())
            //    return false;

			return base.OnCloseRequested ();
		}

		protected override void OnClosed ()
		{
			base.OnClosed ();
			Application.Exit ();
		}

		protected override void OnShown ()
		{
			base.OnShown ();

			if (!String.IsNullOrEmpty (OpenProjectPath)) {
				_controller.OpenProject (OpenProjectPath);
				OpenProjectPath = null;
			}
		}

		#region IView implements
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
            Debug.Assert(_treeUpdating == false, "Must finish previous tree update!");
            _treeUpdating = true;
            _treeSort = false;
			throw new NotImplementedException ();
		}

		public void SetTreeRoot (IProjectItem item)
		{
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            var store = _treeView.DataSource as TreeStore;
            store.Clear();

            var project = item as PipelineProject;
            if (project == null)
                return;

            store.AddNode().SetValue(nameCol, item.Name).SetValue(imgCol, settingsIcon).SetValue(tag, item);
			//throw new NotImplementedException ();
		}

		public void AddTreeItem (IProjectItem item)
		{
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");
            _treeSort = true;

            var path = item.Location;
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var store = _treeView.DataSource as TreeStore;
            TreeNavigator root = store.GetFirstNode();

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
            if (_treeSort)
            {
                // Sort tree
            }
            _treeSort = false;

            _treeUpdating = false;
		}

		public void UpdateProperties (IProjectItem item)
		{
			throw new NotImplementedException ();
		}

		public void OutputAppend (string text)
		{
            if (text == null)
                return;

            _outputWindow.Append(text);
		}

		public void OutputClear ()
		{
            _outputWindow.Clear();
		}

		public bool ChooseContentFile (string initialDirectory, out List<string> files)
		{
            var dlg = new OpenFileDialog()
            {
                Title = "Open",
                Multiselect = true,
                CurrentFolder = initialDirectory
            };
            dlg.Filters.Add(new FileDialogFilter("All Files (*.*)", "*.*"));

            bool result = dlg.Run();
            files = new List<string>(dlg.FileNames);
            return result;
			throw new NotImplementedException ();
		}
		#endregion


        Image folderClosedIcon;
        Image settingsIcon;
        DataField<Image> imgCol = new DataField<Image>();
        DataField<string> nameCol = new DataField<string>();
        DataField<object> tag = new DataField<object>();

        TreeView _treeView;
        private bool _treeUpdating;
        private bool _treeSort;

        OutputTextView _outputWindow;

		/// <summary>
		/// Generates the UI
		/// </summary>
		/// <remarks>>
		/// As XWT doesn't have a UI designer yet, it needs to be setted up by brute force
		/// </remarks>
		void generateUI ()
		{
			createMenu ();

			var mainPaned = new HPaned ();

            var leftPane = new VPaned();

            _treeView = new TreeView()
            {
                HeadersVisible = false
            };

            folderClosedIcon = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_closed.png"));
            settingsIcon = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.settings.png"));
            TreeStore store = new TreeStore(imgCol, nameCol);
            _treeView.DataSource = store;

            leftPane.Panel1.Content = _treeView;
            //leftPane.Panel2.Content = _propertyGrid;

            mainPaned.Panel1.Content = leftPane;

            _outputWindow = new OutputTextView();

            // Find an appropriate font for console like output.
            var faces = new[] { "Consolas", "Lucida Console", "Courier New" };
            for (var f = 0; f < faces.Length; f++)
            {
                _outputWindow.Font = Font.FromName(faces[f]).WithScaledSize(0.9).WithStyle(FontStyle.Normal);
                if (_outputWindow.Font.Family == faces[f])
                    break;
            }

            mainPaned.Panel2.Content = _outputWindow;

            Content = mainPaned;
		}

		void createMenu ()
		{
			// TODO: Implement click event handlers
			MainMenu = new Menu ();

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
			fileMenu.SubMenu.Items.Add (new SeparatorMenuItem ());
			fileMenu.SubMenu.Items.Add (importMenu);
			fileMenu.SubMenu.Items.Add (new SeparatorMenuItem ());
			fileMenu.SubMenu.Items.Add (saveMenu);
			fileMenu.SubMenu.Items.Add (saveAsMenu);
			fileMenu.SubMenu.Items.Add (new SeparatorMenuItem ());
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
			editMenu.SubMenu.Items.Add (new SeparatorMenuItem ());
			editMenu.SubMenu.Items.Add (newItemMenu);
			editMenu.SubMenu.Items.Add (addItemMenu);
			editMenu.SubMenu.Items.Add (new SeparatorMenuItem ());
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
			buildMenu.SubMenu.Items.Add (new SeparatorMenuItem ());
			buildMenu.SubMenu.Items.Add (debugMenu);

			MainMenu.Items.Add (buildMenu);

			var helpMenu = new MenuItem () {
				Label = "_Help",
				SubMenu = new Menu ()
			};

			var viewHelpMenu = new MenuItem ("View Help");
			var aboutMenu = new MenuItem ("_About");

			helpMenu.SubMenu.Items.Add (viewHelpMenu);
			helpMenu.SubMenu.Items.Add (new SeparatorMenuItem ());
			helpMenu.SubMenu.Items.Add (aboutMenu);

			MainMenu.Items.Add (helpMenu);
		}
    }
}

