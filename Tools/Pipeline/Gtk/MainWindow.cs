using System.Collections.Generic;
using System.Diagnostics;
using System;
using Gtk;
using System.Reflection;
using System.Linq;

#if MONOMAC
using IgeMacIntegration;
#endif

namespace MonoGame.Tools.Pipeline
{
    partial class MainWindow : Window, IView
    {
        TreeStore recentListStore;

        const string basetitle = "MonoGame Pipeline";
        public static string LinuxNotAllowedCharacters = "/"; 
        public static string MacNotAllowedCharacters = ":";

        public static string NotAllowedCharacters
        {
            get
            {
                if (Global.DesktopEnvironment == "OSX")
                    return MacNotAllowedCharacters;
                else
                    return LinuxNotAllowedCharacters;
            }
        }

        public static bool CheckString(string s, string notallowedCharacters)
        {
            for (int i = 0; i < notallowedCharacters.Length; i++) 
                if (s.Contains (notallowedCharacters.Substring (i, 1)))
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
        bool expand = false, startup = true;

        public void ReloadTitle()
        {
            #if GTK3
            if(Global.UseHeaderBar)
            {
                if(string.IsNullOrEmpty(projectview1.openedProject))
                {
                    this.Title = "MonoGame Pipeline Tool";
                    hbar.Subtitle = "";
                }
                else
                {
                    this.Title = System.IO.Path.GetFileName(projectview1.openedProject);
                    hbar.Subtitle = System.IO.Path.GetDirectoryName(projectview1.openedProject);
                }
                return;
            }
            #endif

            if (projectview1.openedProject != "")
                this.Title = basetitle + " - " + System.IO.Path.GetFileName(projectview1.openedProject);
            else
                this.Title = basetitle;
        }

        public MainWindow () :
            base (WindowType.Toplevel)
        {
            Build();

            MonoGameContentProjectFileFilter = new FileFilter ();
            MonoGameContentProjectFileFilter.Name = "MonoGame Content Build Projects (*.mgcb)";
            MonoGameContentProjectFileFilter.AddPattern ("*.mgcb");

            XnaContentProjectFileFilter = new FileFilter ();
            XnaContentProjectFileFilter.Name = "XNA Content Projects (*.contentproj)";
            XnaContentProjectFileFilter.AddPattern ("*.contentproj");

            AllFilesFilter = new FileFilter ();
            AllFilesFilter.Name = "All Files (*.*)";
            AllFilesFilter.AddPattern ("*.*");

            #if GTK3
            Widget[] widgets = Global.UseHeaderBar ? menu2.Children : menubar1.Children;
            #else
            Widget[] widgets = menubar1.Children;
            #endif

            var column = new TreeViewColumn ();

            var textCell = new CellRendererText ();
            var dataCell = new CellRendererText ();

            dataCell.Visible = false;

            column.PackStart (textCell, false);
            column.PackStart (dataCell, false);

            treeview1.AppendColumn (column);

            column.AddAttribute (textCell, "markup", 0);
            column.AddAttribute (dataCell, "text", 1);

            recentListStore = new TreeStore (typeof (string), typeof (string));

            treeview1.Model = recentListStore;

            foreach (Widget w in widgets) {
                if(w.Name == "FileAction")
                {
                    var m = (Menu)((MenuItem)w).Submenu;
                    foreach (Widget w2 in m.Children)
                    {
                        if (w2.Name == "OpenRecentAction")
                        {
                            recentMenu = (MenuItem)w2;
                            break;
                        }
                    }
                    break;
                }
            }

            treerebuild = new MenuItem ("Rebuild");
            treerebuild.Activated += delegate {
                projectview1.Rebuild ();
            };

            //This is always returning false, and solves a bug
            if (projectview1 == null || propertiesview1 == null)
                return;

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
            PipelineSettings.Default.Load ();

            if (!String.IsNullOrEmpty(OpenProjectPath)) {
                _controller.OpenProject(OpenProjectPath);
                OpenProjectPath = null;
            }

            if(_controller.ProjectOpen)
                projectview1.ExpandBase();

            if (PipelineSettings.Default.Size.X != 0)
            {
                Resize(PipelineSettings.Default.Size.X, PipelineSettings.Default.Size.Y);

                this.vpaned2.Position = PipelineSettings.Default.VSeparator;
                this.hpaned1.Position = PipelineSettings.Default.HSeparator;

                _controller.LaunchDebugger = DebugModeAction.Active = PipelineSettings.Default.DebugMode;
                FilterOutputAction.Active = toolFilterOutput.Active = PipelineSettings.Default.FilterOutput;

#if GTK3
                if(Global.UseHeaderBar)
                    filteroutput_button.Active = FilterOutputAction.Active;
#endif
            }
        }

        private bool Maximized()
        {
#if GTK2
            return this.GdkWindow.State.HasFlag(Gdk.WindowState.Maximized);
#elif GTK3
            return this.Window.State.HasFlag(Gdk.WindowState.Maximized);
#endif
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a)
        {
            if (_controller.Exit ()) 
            {
                PipelineSettings.Default.FilterOutput = FilterOutputAction.Active;
                PipelineSettings.Default.DebugMode = DebugModeAction.Active;
                PipelineSettings.Default.Save();
                Application.Quit ();
            }
            else
                a.RetVal = true;
        }
        
        void MainWindow_SizeAllocated (object o, SizeAllocatedArgs args)
        {
            if (startup)
            {
                // headerbar can cause offset, this fixes it
                if (PipelineSettings.Default.Size.X != 0)
                {
                    this.vpaned2.Position = PipelineSettings.Default.VSeparator;
                    this.hpaned1.Position = PipelineSettings.Default.HSeparator;
                }

                if (PipelineSettings.Default.Maximized)
                    Maximize();

                startup = false;
            }

            if (!(PipelineSettings.Default.Maximized = Maximized()))
            {
                this.GetSize(out PipelineSettings.Default.Size.X, out PipelineSettings.Default.Size.Y);
                PipelineSettings.Default.VSeparator = vpaned2.Position;
                PipelineSettings.Default.HSeparator = hpaned1.Position;
            }
        }

#region IView implements

        public void Attach (IController controller)
        {
            _controller = controller;
            propertiesview1.controller = _controller;

            _controller.OnBuildStarted += delegate
            {
                buildOutput1.SetBaseFolder(_controller);
                UpdateMenus();
            };
            
            _controller.OnBuildFinished += UpdateMenus;
            _controller.OnProjectLoading += UpdateMenus;
            _controller.OnProjectLoaded += UpdateMenus;

            _controller.OnCanUndoRedoChanged += UpdateUndoRedo;
            UpdateMenus ();
        }

        public AskResult AskSaveOrCancel ()
        {
            var dialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Question, ButtonsType.None, "Do you want to save the project first?");
            dialog.Title = "Save";

            dialog.AddButton("No", (int)ResponseType.No);
            dialog.AddButton("Cancel", (int)ResponseType.Cancel);
            dialog.AddButton("Save", (int)ResponseType.Yes);

            var result = dialog.Run ();
            dialog.Destroy ();

            if (result == (int)ResponseType.Yes)
                return AskResult.Yes;
            else if (result == (int)ResponseType.No)
                return AskResult.No;

            return AskResult.Cancel;
        }

        public bool AskSaveName (ref string filePath, string title)
        {
            var filechooser =
                new FileChooserDialog("Save MGCB Project As",
                    this,
                    FileChooserAction.Save,
                    "Cancel", ResponseType.Cancel,
                    "Save", ResponseType.Accept);

            filechooser.AddFilter (MonoGameContentProjectFileFilter);
            filechooser.AddFilter (AllFilesFilter);

            if (title != null)
                filechooser.Title = title;

            var result = filechooser.Run() == (int)ResponseType.Accept;
            filePath = filechooser.Filename;

            if (filechooser.Filter == MonoGameContentProjectFileFilter && result && !filePath.EndsWith(".mgcb"))
                filePath += ".mgcb";

            filechooser.Destroy ();
            return result;
        }

        public bool AskOpenProject (out string projectFilePath)
        {
            var filechooser =
                new FileChooserDialog("Open MGCB Project",
                    this,
                    FileChooserAction.Open,
                    "Cancel", ResponseType.Cancel,
                    "Open", ResponseType.Accept);

            filechooser.AddFilter (MonoGameContentProjectFileFilter);
            filechooser.AddFilter (AllFilesFilter);

            var result = filechooser.Run() == (int)ResponseType.Accept;
            projectFilePath = filechooser.Filename;
            filechooser.Destroy ();

            return result;
        }

        public bool AskImportProject (out string projectFilePath)
        {
            var filechooser =
                new FileChooserDialog("Import XNA Content Project",
                    this,
                    FileChooserAction.Open,
                    "Cancel", ResponseType.Cancel,
                    "Open", ResponseType.Accept);

            filechooser.AddFilter (XnaContentProjectFileFilter);
            filechooser.AddFilter (AllFilesFilter);

            var result = filechooser.Run() == (int)ResponseType.Accept;
            projectFilePath = filechooser.Filename;
            filechooser.Destroy ();

            return result;
        }

        public void ShowError (string title, string message)
        {
            var dialog = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, message);
            dialog.Title = title;
            dialog.Run();
            dialog.Destroy ();
        }

        public void ShowMessage (string message)
        {
            var dialog = new MessageDialog (this, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok, message);
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
                projectview1.openedProject = "";
                projectview1.SetBaseIter ("");
                projectview1.Close ();
                UpdateMenus ();
            }
        }

        public void AddTreeItem (IProjectItem item)
        {
            projectview1.AddItem (projectview1.GetBaseIter(), item.OriginalPath, item.Exists, false,  expand, _controller.GetFullPath(item.OriginalPath));
        }

        public void AddTreeFolder (string folder)
        {
            projectview1.AddItem (projectview1.GetBaseIter(), folder, true, true,  expand, _controller.GetFullPath(folder));
        }

        public void RemoveTreeItem (ContentItem contentItem)
        {
            projectview1.RemoveItem (projectview1.GetBaseIter (), contentItem.OriginalPath);
        }

        public void RemoveTreeFolder (string folder)
        {
            projectview1.RemoveItem (projectview1.GetBaseIter (), folder);
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

            Application.Invoke(delegate
                { 
                    buildOutput1.OutputAppend(text);
                    UpdateMenus();
                });
        }

        public void OutputClear ()
        {
            Application.Invoke(delegate
                { 
                    buildOutput1.ClearOutput();
                    UpdateMenus();
                });
        }

        public bool ChooseContentFile (string initialDirectory, out List<string> files)
        {
            var filechooser =
                new FileChooserDialog("Add Content Files",
                    this,
                    FileChooserAction.Open,
                    "Cancel", ResponseType.Cancel,
                    "Open", ResponseType.Accept);
            filechooser.SelectMultiple = true;

            filechooser.AddFilter (AllFilesFilter);
            filechooser.SetCurrentFolder (initialDirectory);

            bool result = filechooser.Run() == (int)ResponseType.Accept;

            files = new List<string>();
            files.AddRange (filechooser.Filenames);
            filechooser.Destroy ();

            return result;
        }

        public bool ChooseContentFolder (string initialDirectory, out string folder)
        {
            var folderchooser =
                new FileChooserDialog("Add Content Folder",
                    this,
                    FileChooserAction.SelectFolder,
                    "Cancel", ResponseType.Cancel,
                    "Open", ResponseType.Accept);

            folderchooser.SetCurrentFolder (initialDirectory);
            bool result = folderchooser.Run() == (int)ResponseType.Accept;

            folder = folderchooser.Filename;
            folderchooser.Destroy ();

            return result;
        }

        public bool CopyOrLinkFile(string file, bool exists, out CopyAction action, out bool applyforall)
        {
            var afd = new AddItemDialog(this, file, exists, FileType.File);

            if (afd.Run() == (int)ResponseType.Ok)
            {
                action = afd.responce;
                applyforall = afd.applyforall;
                return true;
            }

            action = CopyAction.Link;
            applyforall = false;
            return false;
        }

        public bool CopyOrLinkFolder(string folder, bool exists, out CopyAction action, out bool applyforall)
        {
            var afd = new AddItemDialog(this, folder, exists, FileType.Folder);
            applyforall = false;

            if (afd.Run() == (int)ResponseType.Ok)
            {
                action = afd.responce;
                return true;
            }

            action = CopyAction.Link;
            return false;
        }

        public void OnTemplateDefined(ContentItemTemplate item)
        {

        }

        public void ItemExistanceChanged(IProjectItem item)
        {
            Application.Invoke(
                delegate {
                    projectview1.RefreshItem(projectview1.GetBaseIter(), item.OriginalPath, item.Exists, _controller.GetFullPath(item.OriginalPath));
                }
            );
        }


#if MONOMAC || LINUX
        string FindSystemMono ()
        {
            // El Capitan Support. /usr/bin is now read only and /usr/local/bin is NOT included
            // in $PATH for apps... only the shell. So we need to manaully find the full path to 
            // the right location.
            string[] pathsToCheck = new string[] {
                "/usr/bin",
                "/usr/local/bin",
                "/Library/Frameworks/Mono.framework/Versions/Current/bin",
            };
            foreach (var path in pathsToCheck) {
                if (System.IO.File.Exists (System.IO.Path.Combine (path, "mono")))
                    return System.IO.Path.Combine (path, "mono");
            }
            OutputAppend("Cound not find mono. Please install the latest version from http://www.mono-project.com");
            return "mono";
        }
#endif

        public Process CreateProcess(string exe, string commands)
        {
            var _buildProcess = new Process();
#if WINDOWS
            _buildProcess.StartInfo.FileName = exe;
            _buildProcess.StartInfo.Arguments = commands;
#endif
#if MONOMAC || LINUX
            _buildProcess.StartInfo.FileName = FindSystemMono ();
            if (_controller.LaunchDebugger) {
                var port = Environment.GetEnvironmentVariable("MONO_DEBUGGER_PORT");
                port = !string.IsNullOrEmpty (port) ? port : "55555";
                var monodebugger = string.Format ("--debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:{0}",
                    port);
                _buildProcess.StartInfo.Arguments = string.Format("{0} \"{1}\" {2}", monodebugger, exe, commands);
                OutputAppend("************************************************");
                OutputAppend("RUNNING MGCB IN DEBUG MODE!!!");
                OutputAppend(string.Format ("Attach your Debugger to localhost:{0}", port));
                OutputAppend("************************************************");
            } else {
                _buildProcess.StartInfo.Arguments = string.Format("\"{0}\" {1}", exe, commands);
            }
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
            projectview1.ExpandBase();
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
            expand = true;
            var dialog = new NewTemplateDialog(this, _controller.Templates.GetEnumerator ());

            if (dialog.Run () == (int)ResponseType.Ok) {

                List<TreeIter> iters;
                List<string> ids;
                string[] paths = projectview1.GetSelectedTreePath (out iters, out ids);

                string location;

                if (paths.Length == 1) {
                    if (ids [0] == projectview1.ID_FOLDER)
                        location = paths [0];
                    else if (ids[0] == projectview1.ID_BASE)
                        location = _controller.GetFullPath ("");
                    else
                        location = System.IO.Path.GetDirectoryName (paths [0]);
                }
                else
                    location = _controller.GetFullPath ("");

                _controller.NewItem(dialog.name, location, dialog.templateFile);
                UpdateMenus();
            }
            expand = false;
        }

        public void OnAddItemActionActivated (object sender, EventArgs e)
        {
            expand = true;
            List<TreeIter> iters;
            List<string> ids;
            string[] paths = projectview1.GetSelectedTreePath (out iters, out ids);

            if (paths.Length == 1) {
                if (ids [0] == projectview1.ID_FOLDER)
                    _controller.Include (paths [0]);
                else if (ids[0] == projectview1.ID_BASE)
                    _controller.Include (_controller.GetFullPath (""));
                else
                    _controller.Include (System.IO.Path.GetDirectoryName (paths [0]));
            }
            else
                _controller.Include (_controller.GetFullPath (""));
            UpdateMenus();
            expand = false;
        }

        public void OnNewFolderActionActivated(object sender, EventArgs e)
        {
            var ted = new TextEditorDialog(this, "New Folder", "Folder Name:", "", true);
            if (ted.Run() != (int)ResponseType.Ok)
                return;
            var foldername = ted.text;

            expand = true;
            List<TreeIter> iters;
            List<string> ids;
            string[] paths = projectview1.GetSelectedTreePath (out iters, out ids);

            if (paths.Length == 1) {
                if (ids [0] == projectview1.ID_FOLDER)
                    _controller.NewFolder (foldername, paths [0]);
                else if (ids[0] == projectview1.ID_BASE)
                    _controller.NewFolder (foldername, _controller.GetFullPath (""));
                else
                    _controller.NewFolder (foldername, System.IO.Path.GetDirectoryName (paths [0]));
            }
            else
                _controller.NewFolder (foldername, _controller.GetFullPath (""));

            expand = false;
        }

        public void OnAddFolderActionActivated(object sender, EventArgs e)
        {
            expand = true;
            List<TreeIter> iters;
            List<string> ids;
            string[] paths = projectview1.GetSelectedTreePath (out iters, out ids);

            if (paths.Length == 1) {
                if (ids [0] == projectview1.ID_FOLDER)
                    _controller.IncludeFolder (paths [0]);
                else if (ids[0] == projectview1.ID_BASE)
                    _controller.IncludeFolder (_controller.GetFullPath (""));
                else
                    _controller.IncludeFolder (System.IO.Path.GetDirectoryName (paths [0]));
            }
            else
                _controller.IncludeFolder (_controller.GetFullPath (""));
            UpdateMenus();
            expand = false;
        }

        public void OnRenameActionActivated (object sender, EventArgs e)
        {
            expand = true;
            projectview1.Rename ();
            UpdateMenus();
            expand = false;
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
            var adialog = new AboutDialog ();
            adialog.TransientFor = this;
            adialog.Logo = new Gdk.Pixbuf(null, "MonoGame.Tools.Pipeline.App.ico");
            adialog.ProgramName = AssemblyAttributes.AssemblyProduct;
            adialog.Version = AssemblyAttributes.AssemblyVersion;
            adialog.Comments = AssemblyAttributes.AssemblyDescription;
            adialog.Copyright = AssemblyAttributes.AssemblyCopyright;
            adialog.Website = "http://www.monogame.net/";
            adialog.WebsiteLabel = "MonoGame Website";
            adialog.Run ();
            adialog.Destroy ();
        }

        protected void OnDebugModeActionActivated (object sender, EventArgs e)
        {
            _controller.LaunchDebugger = this.DebugModeAction.Active;
        }

        protected void OnFilterOutputActionActivated (object sender, EventArgs e)
        {
            buildOutput1.CurrentPage = FilterOutputAction.Active ? 0 : 1;
            toolFilterOutput.Active = FilterOutputAction.Active;

#if GTK3
            if(Global.UseHeaderBar)
                filteroutput_button.Active = FilterOutputAction.Active;
#endif
        }

        protected void ToggleFilterOutput (object sender, EventArgs e)
        {
            FilterOutputAction.Active = !FilterOutputAction.Active;
        }

        protected void OnCancelBuildActionActivated (object sender, EventArgs e)
        {
            _controller.CancelBuild();
        }

        public void UpdateMenus()
        {
            List<TreeIter> iters;
            List<string> ids;
            string[] paths = projectview1.GetSelectedTreePath (out iters, out ids);

            var notBuilding = !_controller.ProjectBuilding;
            var projectOpen = _controller.ProjectOpen;
            var projectOpenAndNotBuilding = projectOpen && notBuilding;
            var somethingSelected = paths.Length > 0;

            // Update the state of all menu items.


            Application.Invoke(delegate
                { 
                    NewAction.Sensitive = toolNew.Sensitive = notBuilding;
                    OpenAction.Sensitive = toolOpen.Sensitive = notBuilding;
                    ImportAction.Sensitive = notBuilding;

                    SaveAction.Sensitive = toolSave.Sensitive = projectOpenAndNotBuilding && _controller.ProjectDirty;
                    SaveAsAction.Sensitive = projectOpenAndNotBuilding;
                    CloseAction.Sensitive = projectOpenAndNotBuilding;

                    ExitAction.Sensitive = notBuilding;

                    AddAction.Sensitive = toolAddItem.Sensitive = toolAddFolder.Sensitive = 
                        toolNewItem.Sensitive = toolNewFolder.Sensitive = projectOpen;
            
                    RenameAction.Sensitive = paths.Length == 1;
                    DeleteAction.Sensitive = projectOpen && somethingSelected;

                    BuildAction.Sensitive = projectOpen;
                    BuildAction1.Sensitive = toolBuild.Sensitive = projectOpenAndNotBuilding;

                    treerebuild.Sensitive = RebuildAction.Sensitive = toolRebuild.Sensitive = projectOpenAndNotBuilding;
                    RebuildAction.Sensitive = treerebuild.Sensitive;

                    CleanAction.Sensitive = toolClean.Sensitive = projectOpenAndNotBuilding;
                    CancelBuildAction.Sensitive = !notBuilding;
                    CancelBuildAction.Visible = !notBuilding;

                    #if GTK3
                    if (Global.UseHeaderBar)
                    {
                        new_button.Sensitive = NewAction.Sensitive;
                        open_button.Sensitive = OpenAction.Sensitive;
                        save_button.Sensitive = SaveAction.Sensitive;
                        build_button.Visible = BuildAction1.Sensitive;
                        rebuild_button.Visible = RebuildAction.Sensitive;
                        cancel_button.Visible = CancelBuildAction.Sensitive;
                    }
                    #endif

                    DebugModeAction.Sensitive = notBuilding;
                    UpdateUndoRedo(_controller.CanUndo, _controller.CanRedo);
                });

            UpdateRecentProjectList();
        }

        public void OpenProject(string path)
        {
            _controller.OpenProject(path);
            projectview1.ExpandBase();
        }

        public void UpdateRecentProjectList()
        {
            var m = new Menu ();
            recentMenu.Submenu = null;

            if (Global.UseHeaderBar)
                recentListStore.Clear();

            var projectList = PipelineSettings.Default.ProjectHistory.ToList();

            foreach (var project in projectList)
            {
                if (Global.UseHeaderBar)
                    recentListStore.InsertWithValues(0, "<b>" + System.IO.Path.GetFileName(project) + "</b>" + Environment.NewLine + System.IO.Path.GetDirectoryName(project), project);

                var recentItem = new MenuItem(project);
                recentItem.Activated += (sender, e) => OpenProject(project);
                m.Insert (recentItem, 0);
            }
            
            if (projectList.Count > 0)
            {
                m.Add(new SeparatorMenuItem());
                var item = new MenuItem("Clear");
                item.Activated += delegate
                    {
                        PipelineSettings.Default.Clear();
                        UpdateRecentProjectList();
                    };
                m.Add(item);

                Application.Invoke(delegate
                    {
                        recentMenu.Submenu = m;
                        m.ShowAll();
                        recentMenu.Sensitive = projectList.Count > 0;
                        menubar1.ShowAll();
                    });
            }
        }

        void UpdateUndoRedo(bool canUndo, bool canRedo)
        {
            UndoAction.Sensitive = canUndo;
            RedoAction.Sensitive = canRedo;
        }
    }
}

