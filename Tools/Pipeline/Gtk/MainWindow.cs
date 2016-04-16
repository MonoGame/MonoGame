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

        public string OpenProjectPath;
        public IController _controller;

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

            Init();

            var widgets = Global.UseHeaderBar ? new Widget[0] : menubar1.Children;

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
            PipelineSettings.Default.Load();

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
                FilterOutputAction.Active = PipelineSettings.Default.FilterOutput;
            }

            UpdateRecentProjectList();
        }

        private bool Maximized()
        {
#if GTK2
            if (this.GdkWindow == null)
                return false;

            return this.GdkWindow.State.HasFlag(Gdk.WindowState.Maximized);
#elif GTK3
            if (this.Window == null)
                return false;
            
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

        public void ItemExistanceChanged(IProjectItem item)
        {
            Application.Invoke(
                delegate {
                    projectview1.RefreshItem(projectview1.GetBaseIter(), item.OriginalPath, item.Exists, _controller.GetFullPath(item.OriginalPath));
                }
            );
        }

        public bool GetSelection(out FileType type, out string path, out string location)
        {
            List<TreeIter> iters;
            List<string> ids;
            string[] paths = projectview1.GetSelectedTreePath(out iters, out ids);

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
            path = projectview1.GetSelectedTreePath(out iters, out ids);

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
            _controller.NewItem();
            UpdateMenus();
            expand = false;
        }

        public void OnAddItemActionActivated (object sender, EventArgs e)
        {
            expand = true;
            _controller.Include();
            UpdateMenus();
            expand = false;
        }

        public void OnNewFolderActionActivated(object sender, EventArgs e)
        {
            expand = true;
            _controller.NewFolder();
            UpdateMenus();
            expand = false;
        }

        public void OnAddFolderActionActivated(object sender, EventArgs e)
        {
            expand = true;
            _controller.IncludeFolder ();
            UpdateMenus();
            expand = false;
        }

        public void OnExcludeActionActivated (object sender, EventArgs e)
        {
            projectview1.Remove (false);
            UpdateMenus();
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
            projectview1.Remove (true);
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
            adialog.Run();
        }

        protected void OnDebugModeActionActivated (object sender, EventArgs e)
        {
            _controller.LaunchDebugger = DebugModeAction.Active;

#if GTK3
            if(Global.UseHeaderBar)
                debugmode_button.Active = DebugModeAction.Active;
#endif
        }

        protected void OnFilterOutputActionActivated (object sender, EventArgs e)
        {
            buildOutput1.CurrentPage = FilterOutputAction.Active ? 0 : 1;
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
                    NewAction.Sensitive = notBuilding;
                    OpenAction.Sensitive = notBuilding;
                    ImportAction.Sensitive = notBuilding;

                    SaveAction.Sensitive = projectOpenAndNotBuilding && _controller.ProjectDirty;
                    SaveAsAction.Sensitive = projectOpenAndNotBuilding;
                    CloseAction.Sensitive = projectOpenAndNotBuilding;

                    ExitAction.Sensitive = notBuilding;

                    AddAction.Sensitive = toolAddItem.Sensitive = toolAddFolder.Sensitive = 
                        toolNewItem.Sensitive = toolNewFolder.Sensitive = projectOpen;
                    
                    ExcludeAction.Sensitive = somethingSelected;

                    RenameAction.Sensitive = paths.Length == 1;
                    DeleteAction.Sensitive = somethingSelected;

                    BuildAction.Sensitive = projectOpen;
                    BuildAction1.Sensitive = projectOpenAndNotBuilding;

                    treerebuild.Sensitive = RebuildAction.Sensitive = projectOpenAndNotBuilding;
                    RebuildAction.Sensitive = treerebuild.Sensitive;

                    CleanAction.Sensitive = toolClean.Sensitive = projectOpenAndNotBuilding;
                    CancelBuildAction.Sensitive = !notBuilding;

                    toolBuild.Visible = notBuilding;
                    toolRebuild.Visible = notBuilding;
                    toolClean.Visible = notBuilding;
                    toolCancelBuild.Visible = !notBuilding;

                    #if GTK3
                    if (Global.UseHeaderBar)
                    {
                        open_button.Sensitive = OpenAction.Sensitive;
                        build_button.Visible = BuildAction1.Sensitive;
                        rebuild_button.Visible = RebuildAction.Sensitive;
                        cancel_button.Visible = CancelBuildAction.Sensitive;
                        separator1.Visible = build_button.Visible || cancel_button.Visible;
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

            if (Global.UseHeaderBar)
                recentListStore.Clear();
            else
                recentMenu.Submenu = null;

            var projectList = PipelineSettings.Default.ProjectHistory.ToList();

            foreach (var project in projectList)
            {
                if (Global.UseHeaderBar)
                    recentListStore.InsertWithValues(0, "<b>" + System.IO.Path.GetFileName(project) + "</b>" + Environment.NewLine + System.IO.Path.GetDirectoryName(project), project);
                else
                {
                    var recentItem = new MenuItem(project);
                    recentItem.Activated += (sender, e) => OpenProject(project);
                    m.Insert(recentItem, 0);
                }
            }
            
            if (!Global.UseHeaderBar && projectList.Count > 0)
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

