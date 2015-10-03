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
    partial class MainWindow : Window, IView
    {
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

        MenuItem recentMenu;

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

            foreach (Widget w in widgets) {
                if(w.Name == "FileAction")
                {
                    var m = (Menu)((MenuItem)w).Submenu;
                    foreach (Widget w2 in m.Children) 
                        if (w2.Name == "OpenRecentAction") 
                            recentMenu = (MenuItem)w2;
                }
            }

            //This is always returning false, and solves a bug
            if (projectview1 == null || propertiesview1 == null)
                return;

            projectview1.Initalize (this, propertiesview1);

            if (Assembly.GetEntryAssembly ().FullName.Contains ("Pipeline"))
                BuildMenu ();
            else {
                menubar1.Hide ();
                vbox2.Remove (menubar1);
            }

            propertiesview1.Initalize (this);
            this.textview2.SizeAllocated += AutoScroll;
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

        public void ReloadRecentList(List<string> paths)
        {
            recentMenu.Submenu = null;
            var m = new Menu ();

            int nop = 0;

            foreach (var project in paths)
            {
                nop++;
                var recentItem = new MenuItem(project);

                // We need a local to make the delegate work correctly.
                var localProject = project;
                recentItem.Activated += delegate
                    {
                        _controller.OpenProject(localProject);
                    };

                m.Insert (recentItem, 0);
            }

            if (nop > 0)
            {
                m.Add(new SeparatorMenuItem());
                var item = new MenuItem("Clear");
                item.Activated += delegate
                    {
                        _controller.ClearRecentList();
                    };
                m.Add(item);
            }
            else
            {
                var item = new MenuItem("No History");
                item.Sensitive = false;
                m.Add(item);
            }

            recentMenu.Submenu = m;
            m.ShowAll();

            menubar1.ShowAll ();
        }

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
            }
            UpdateMenus ();
        }

        public void AddTreeItem (IProjectItem item)
        {
            projectview1.AddItem (projectview1.GetBaseIter(), item.OriginalPath, item.Exists, false, _controller.GetFullPath(item.OriginalPath));
        }

        public void AddTreeFolder (string folder)
        {
            projectview1.AddItem (projectview1.GetBaseIter(), folder, true, true, _controller.GetFullPath(folder));
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

        public void AutoScroll(object sender, SizeAllocatedArgs e)
        {
            textview2.ScrollToIter(textview2.Buffer.EndIter, 0, false, 0, 0);
        }

        public void OutputAppend (string text)
        {
            if (text == null)
                return;

            Application.Invoke (delegate { 
                try {
                    lock(textview2.Buffer) {
                        textview2.Buffer.Text += text + "\r\n";
                        UpdateMenus();
                        System.Threading.Thread.Sleep(1);
                    }
                }
                catch {
                }
            });
        }

        public void OutputClear ()
        {
            Application.Invoke (delegate { 
                try {
                    lock(textview2.Buffer) {
                        textview2.Buffer.Text = "";
                        UpdateMenus();
                    }
                }
                catch {
                }
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

        public bool ChooseItemTemplate(out ContentItemTemplate template, out string name)
        {
            var dialog = new NewTemplateDialog(this, _controller.Templates.GetEnumerator ());

            if (dialog.Run() == (int)ResponseType.Ok)
            {
                template = dialog.templateFile;
                name = dialog.name;

                return true;
            }

            template = null;
            name = null;

            return false;
        }

        public bool ChooseName(string title, string text, string oldname, bool docheck, out string newname)
        {
            var ted = new TextEditorDialog(this, title, text, oldname, docheck);

            if (ted.Run() == (int)ResponseType.Ok)
            {
                newname = ted.text;
                return true;
            }

            newname = "";
            return false;
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
            projectview1.RefreshItem(projectview1.GetBaseIter(), item.OriginalPath, item.Exists, _controller.GetFullPath(item.OriginalPath));
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

        public void ExpandPath(string path)
        {
            projectview1.ExpandItem(projectview1.GetBaseIter(), path);
        }

        public bool GetSelection(out FileType type, out string path, out string location)
        {
            List<TreeIter> iters;
            List<string> ids;
            string[] paths = projectview1.GetSelectedTreePath (out iters, out ids);

            if (paths.Length != 1)
            {
                type = FileType.Base;
                path = "";
                location = "";
                return false;
            }

            path = paths[0];
            GetInfo(ids[0], paths[0], out type, out location);

            return true;
        }

        public bool GetSelection(out FileType[] type, out string[] path, out string[] location)
        {
            var types = new List<FileType>();
            var locations = new List<string>();

            List<TreeIter> iters;
            List<string> ids;
            path = projectview1.GetSelectedTreePath (out iters, out ids);

            for (int i = 0; i < path.Length; i++)
            {
                FileType tmp_type;
                string tmp_loc;

                GetInfo(ids[i], path[i], out tmp_type, out tmp_loc);

                types.Add(tmp_type);
                locations.Add(tmp_loc);
            }

            type = types.ToArray();
            location = locations.ToArray();

            return (path.Length > 0);
        }

        public List<ContentItem> GetChildItems(string path)
        {
            var items = new List<ContentItem>();
            TreeIter iter;

            if (!projectview1.GetIter(projectview1.GetBaseIter(), path, out iter))
                return items;

            var paths = projectview1.GetAllPaths(iter);
            foreach (string p in paths)
            {
                var item = _controller.GetItem(p) as ContentItem;

                if (item != null)
                    items.Add(item);
            }

            return items;
        }
#endregion

        private void GetInfo(string id, string path, out FileType type, out string location)
        {
            if (id == projectview1.ID_FOLDER)
            {
                type = FileType.Folder;
                location = path;
            }
            else if (id == projectview1.ID_BASE)
            {
                type = FileType.Base;
                location = "";
            }
            else
            {
                type = FileType.File;
                location = System.IO.Path.GetDirectoryName(path);
            }
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
            UpdateMenus();
        }

        protected void OnRedoActionActivated (object sender, EventArgs e)
        {
            _controller.Redo ();
            UpdateMenus();
        }

        public void OnNewItemActionActivated (object sender, EventArgs e)
        {
            _controller.NewItem();
            UpdateMenus();
        }

        public void OnAddItemActionActivated (object sender, EventArgs e)
        {
            _controller.Include();
            UpdateMenus();
        }

        public void OnNewFolderActionActivated(object sender, EventArgs e)
        {
            _controller.NewFolder();
            UpdateMenus();
        }

        public void OnAddFolderActionActivated(object sender, EventArgs e)
        {
            _controller.IncludeFolder();
            UpdateMenus();
        }

        public void OnRenameActionActivated (object sender, EventArgs e)
        {
            _controller.Rename ();
            UpdateMenus();
        }
        
        public void OnDeleteActionActivated (object sender, EventArgs e)
        {
            _controller.Delete ();
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

        protected void OnCancelBuildActionActivated (object sender, EventArgs e)
        {
            _controller.CancelBuild();
            UpdateMenus();
        }

        public void UpdateMenus()
        {
            var ms = _controller.GetMenuSensitivityInfo();

            NewAction.Sensitive = ms.New;
            OpenAction.Sensitive = ms.Open;
            ImportAction.Sensitive = ms.Import;
            SaveAction.Sensitive = ms.Save;
            SaveAsAction.Sensitive = ms.SaveAs;
            CloseAction.Sensitive = ms.Close;
            ExitAction.Sensitive = ms.Exit;
            AddAction.Sensitive = ms.Add;
            RenameAction.Sensitive = ms.Rename;
            DeleteAction.Sensitive = ms.Delete;
            BuildAction.Sensitive = ms.BuildMenu;
            BuildAction1.Sensitive = ms.Build;
            RebuildAction.Sensitive = ms.Rebuild;
            CleanAction.Sensitive = ms.Clean;
            CancelBuildAction.Sensitive = CancelBuildAction.Visible = ms.Cancel;

            #if GTK3
            if(Global.UseHeaderBar)
            {
                new_button.Sensitive = ms.New;
                open_button.Sensitive = ms.Open;
                save_button.Sensitive = ms.Save;
                build_button.Sensitive = ms.Build;
            }
            #endif

            DebugModeAction.Sensitive = ms.Debug;

            UpdateUndoRedo(_controller.CanUndo, _controller.CanRedo);
        }

        void UpdateUndoRedo(bool canUndo, bool canRedo)
        {
            UndoAction.Sensitive = canUndo;
            RedoAction.Sensitive = canRedo;
        }
    }
}

