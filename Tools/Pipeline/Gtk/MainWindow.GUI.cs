using Gtk;
using GLib;

namespace MonoGame.Tools.Pipeline
{
	internal partial class MainWindow
	{
        #if GTK3
        HeaderBar hbar;

        [Builder.ObjectAttribute] Button new_button;
        [Builder.ObjectAttribute] Button save_button;
        [Builder.ObjectAttribute] Button build_button;
        [Builder.ObjectAttribute] Button rebuild_button;
        [Builder.ObjectAttribute] Button cancel_button;
        [Builder.ObjectAttribute] Separator separator1;
        [Builder.ObjectAttribute] ToggleButton filteroutput_button;
        [Builder.ObjectAttribute] Button saveas_button;
        [Builder.ObjectAttribute] Button undo_button;
        [Builder.ObjectAttribute] Button redo_button;
        [Builder.ObjectAttribute] Button close_button;
        [Builder.ObjectAttribute] Button clean_button;

        MenuButton open_button, gear_button;
        ModalButton debugmode_button;
        #endif

		private global::Gtk.UIManager UIManager;
		
		private global::Gtk.Action FileAction;
		
		private global::Gtk.Action NewAction;
		
		private global::Gtk.Action OpenAction;
		
		private global::Gtk.Action OpenRecentAction;
		
		private global::Gtk.Action CloseAction;
		
		private global::Gtk.Action ImportAction;
		
		private global::Gtk.Action SaveAction;
		
		private global::Gtk.Action SaveAsAction;
		
		private global::Gtk.Action ExitAction;
		
		private global::Gtk.Action EditAction;
		
		private global::Gtk.Action UndoAction;
		
		private global::Gtk.Action RedoAction;

        Action RenameAction;

        private global::Gtk.Action ExcludeAction;
		
		private global::Gtk.Action DeleteAction;
		
		private global::Gtk.Action BuildAction;
		
		private global::Gtk.Action BuildAction1;
		
		private global::Gtk.Action RebuildAction;
		
		private global::Gtk.Action CleanAction;
		
		private global::Gtk.ToggleAction DebugModeAction, FilterOutputAction;
		
		private global::Gtk.Action HelpAction;
		
		private global::Gtk.Action ViewHelpAction;
		
		private global::Gtk.Action AboutAction;
		
		private global::Gtk.Action CancelBuildAction;
		
		private global::Gtk.Action AddAction;
		
		private global::Gtk.Action NewItemAction;
		
		private global::Gtk.Action NewFolderAction;
		
		private global::Gtk.Action ExistingItemAction;
		
		private global::Gtk.Action ExistingFolderAction;
		
		private global::Gtk.VBox vbox2;
		
		private global::Gtk.MenuBar menubar1;
		
		private global::Gtk.HPaned hpaned1;
		
		private global::Gtk.VPaned vpaned2;
		
		private global::MonoGame.Tools.Pipeline.ProjectView projectview1;
		
		private global::MonoGame.Tools.Pipeline.PropertiesView propertiesview1;
		
        BuildOutput buildOutput1;
        TreeView treeview1;

        Toolbar toolBar1;
        ToolButton toolNew, toolOpen, toolSave, toolUndo, toolRedo, toolNewItem, toolNewFolder, toolAddItem, toolAddFolder, toolBuild, toolRebuild, toolClean, toolCancelBuild;
        ToggleToolButton toolFilterOutput;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoGame.Tools.Pipeline.MainWindow
			this.UIManager = new global::Gtk.UIManager ();
			global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
			this.FileAction = new global::Gtk.Action ("FileAction", global::Mono.Unix.Catalog.GetString ("File"), null, null);
			this.FileAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("File");
			w1.Add (this.FileAction, null);
            this.NewAction = new global::Gtk.Action ("NewAction", global::Mono.Unix.Catalog.GetString ("New..."), null, "gtk-new");
			this.NewAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("New...");
			w1.Add (this.NewAction, "<Control>n");
            this.OpenAction = new global::Gtk.Action ("OpenAction", global::Mono.Unix.Catalog.GetString ("Open..."), null, "gtk-open");
			this.OpenAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Open...");
			w1.Add (this.OpenAction, "<Control>o");
			this.OpenRecentAction = new global::Gtk.Action ("OpenRecentAction", global::Mono.Unix.Catalog.GetString ("Open Recent"), null, null);
			this.OpenRecentAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Open Recent");
			w1.Add (this.OpenRecentAction, null);
            this.CloseAction = new global::Gtk.Action ("CloseAction", global::Mono.Unix.Catalog.GetString ("Close"), null, "gtk-close");
			this.CloseAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Close");
			w1.Add (this.CloseAction, null);
			this.ImportAction = new global::Gtk.Action ("ImportAction", global::Mono.Unix.Catalog.GetString ("Import..."), null, null);
			this.ImportAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Import...");
			w1.Add (this.ImportAction, null);
            this.SaveAction = new global::Gtk.Action ("SaveAction", global::Mono.Unix.Catalog.GetString ("Save"), null, "gtk-save");
			this.SaveAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Save");
			w1.Add (this.SaveAction, "<Control>s");
            this.SaveAsAction = new global::Gtk.Action ("SaveAsAction", global::Mono.Unix.Catalog.GetString ("Save As..."), null, "gtk-save-as");
			this.SaveAsAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Save As...");
			w1.Add (this.SaveAsAction, null);
            this.ExitAction = new global::Gtk.Action ("ExitAction", global::Mono.Unix.Catalog.GetString ("Exit"), null, "gtk-quit");
			this.ExitAction.HideIfEmpty = false;
			this.ExitAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Exit");
			w1.Add (this.ExitAction, null);
			this.EditAction = new global::Gtk.Action ("EditAction", global::Mono.Unix.Catalog.GetString ("Edit"), null, null);
			this.EditAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Edit");
			w1.Add (this.EditAction, null);
            this.UndoAction = new global::Gtk.Action ("UndoAction", global::Mono.Unix.Catalog.GetString ("Undo"), null, "gtk-undo");
			this.UndoAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Undo");
			w1.Add (this.UndoAction, "<Control>z");
            this.RedoAction = new global::Gtk.Action ("RedoAction", global::Mono.Unix.Catalog.GetString ("Redo"), null, "gtk-redo");
			this.RedoAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Redo");
			w1.Add (this.RedoAction, "<Control>y");
			RenameAction = new Action ("RenameAction", global::Mono.Unix.Catalog.GetString ("Rename"), null, null);
			RenameAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Rename");
            w1.Add (RenameAction, null);
            this.ExcludeAction = new global::Gtk.Action ("ExcludeAction", global::Mono.Unix.Catalog.GetString ("Exclude from Project"), null, null);
            this.ExcludeAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Exclude");
            w1.Add (this.ExcludeAction, null);
            this.DeleteAction = new global::Gtk.Action ("DeleteAction", global::Mono.Unix.Catalog.GetString ("Delete"), null, "gtk-delete");
			this.DeleteAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Delete");
			w1.Add (this.DeleteAction, null);
			this.BuildAction = new global::Gtk.Action ("BuildAction", global::Mono.Unix.Catalog.GetString ("Build"), null, null);
			this.BuildAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Build");
			w1.Add (this.BuildAction, null);
            this.BuildAction1 = new global::Gtk.Action ("BuildAction1", global::Mono.Unix.Catalog.GetString ("Build"), null, "gtk-execute");
			this.BuildAction1.ShortLabel = global::Mono.Unix.Catalog.GetString ("Build");
			w1.Add (this.BuildAction1, "<Mod2>F6");
            this.RebuildAction = new global::Gtk.Action ("RebuildAction", global::Mono.Unix.Catalog.GetString ("Rebuild"), null, "gtk-execute");
			this.RebuildAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Rebuild");
			w1.Add (this.RebuildAction, null);
			this.CleanAction = new global::Gtk.Action ("CleanAction", global::Mono.Unix.Catalog.GetString ("Clean"), null, null);
			this.CleanAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Clean");
            w1.Add (this.CleanAction, null);
            this.DebugModeAction = new global::Gtk.ToggleAction ("DebugModeAction", global::Mono.Unix.Catalog.GetString ("Debug Mode"), null, null);
            this.DebugModeAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Debug Mode");
            w1.Add (this.DebugModeAction, null);
            this.FilterOutputAction = new global::Gtk.ToggleAction ("FilterOutputAction", global::Mono.Unix.Catalog.GetString ("Filter Output"), null, null);
            this.FilterOutputAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Filter Output");
            this.FilterOutputAction.Active = true;
            w1.Add (this.FilterOutputAction, null);
			this.HelpAction = new global::Gtk.Action ("HelpAction", global::Mono.Unix.Catalog.GetString ("Help"), null, null);
			this.HelpAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Help");
			w1.Add (this.HelpAction, null);
            this.ViewHelpAction = new global::Gtk.Action ("ViewHelpAction", global::Mono.Unix.Catalog.GetString ("View Help"), null, "gtk-help");
			this.ViewHelpAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("View Help");
			w1.Add (this.ViewHelpAction, "<Mod2>F1");
            this.AboutAction = new global::Gtk.Action ("AboutAction", global::Mono.Unix.Catalog.GetString ("About"), null, "gtk-about");
			this.AboutAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("About");
			w1.Add (this.AboutAction, null);
            this.CancelBuildAction = new global::Gtk.Action ("CancelBuildAction", global::Mono.Unix.Catalog.GetString ("Cancel Build"), null, "gtk-stop");
			this.CancelBuildAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Cancel Build");
			w1.Add (this.CancelBuildAction, null);
			this.AddAction = new global::Gtk.Action ("AddAction", global::Mono.Unix.Catalog.GetString ("Add"), null, null);
			this.AddAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Add");
			w1.Add (this.AddAction, null);
            this.NewItemAction = new global::Gtk.Action ("NewItemAction", global::Mono.Unix.Catalog.GetString ("New Item..."), null, "gtk-file");
			this.NewItemAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("New Item...");
			w1.Add (this.NewItemAction, null);
            this.NewFolderAction = new global::Gtk.Action ("NewFolderAction", global::Mono.Unix.Catalog.GetString ("New Folder..."), null, "gtk-directory");
			this.NewFolderAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("New Folder...");
			w1.Add (this.NewFolderAction, null);
			this.ExistingItemAction = new global::Gtk.Action ("ExistingItemAction", global::Mono.Unix.Catalog.GetString ("Existing Item..."), null, null);
			this.ExistingItemAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Add Existing Item...");
			w1.Add (this.ExistingItemAction, null);
			this.ExistingFolderAction = new global::Gtk.Action ("ExistingFolderAction", global::Mono.Unix.Catalog.GetString ("Existing Folder..."), null, null);
			this.ExistingFolderAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Add Existing Folder...");
			w1.Add (this.ExistingFolderAction, null);
			this.UIManager.InsertActionGroup (w1, 0);
			this.AddAccelGroup (this.UIManager.AccelGroup);
			this.Name = "MonoGame.Tools.Pipeline.MainWindow";
			this.Icon = global::Gdk.Pixbuf.LoadFromResource ("MonoGame.Tools.Pipeline.App.ico");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Container child MonoGame.Tools.Pipeline.MainWindow.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			// Container child vbox2.Gtk.Box+BoxChild
            this.UIManager.AddUiFromString ("<ui><menubar name='menubar1'><menu name='FileAction' action='FileAction'><menuitem name='NewAction' action='NewAction'/><menuitem name='OpenAction' action='OpenAction'/><menuitem name='OpenRecentAction' action='OpenRecentAction'/><menuitem name='CloseAction' action='CloseAction'/><separator/><menuitem name='ImportAction' action='ImportAction'/><separator/><menuitem name='SaveAction' action='SaveAction'/><menuitem name='SaveAsAction' action='SaveAsAction'/><separator/><menuitem name='ExitAction' action='ExitAction'/></menu><menu name='EditAction' action='EditAction'><menuitem name='UndoAction' action='UndoAction'/><menuitem name='RedoAction' action='RedoAction'/><separator/><menu name='AddAction' action='AddAction'><menuitem name='NewItemAction' action='NewItemAction'/><menuitem name='NewFolderAction' action='NewFolderAction'/><separator/><menuitem name='ExistingItemAction' action='ExistingItemAction'/><menuitem name='ExistingFolderAction' action='ExistingFolderAction'/></menu><separator/><menuitem name='ExcludeAction' action='ExcludeAction'/><separator/><menuitem name='RenameAction' action='RenameAction'/><menuitem name='DeleteAction' action='DeleteAction'/></menu><menu name='BuildAction' action='BuildAction'><menuitem name='BuildAction1' action='BuildAction1'/><menuitem name='RebuildAction' action='RebuildAction'/><menuitem name='CleanAction' action='CleanAction'/><menuitem name='CancelBuildAction' action='CancelBuildAction'/><separator name='sep1'/><menuitem name='DebugModeAction' action='DebugModeAction'/><menuitem name='FilterOutputAction' action='FilterOutputAction'/></menu><menu name='HelpAction' action='HelpAction'><menuitem name='ViewHelpAction' action='ViewHelpAction'/><separator/><menuitem name='AboutAction' action='AboutAction'/></menu></menubar></ui>");
			this.menubar1 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/menubar1")));
			this.menubar1.Name = "menubar1";
			this.vbox2.Add (this.menubar1);

            toolBar1 = new Toolbar();

            toolNew = Global.GetToolButton(NewAction, "Toolbar.New.png");
            toolBar1.Add(toolNew);

            toolOpen = Global.GetToolButton(OpenAction, "Toolbar.Open.png");
            toolBar1.Add(toolOpen);

            toolSave = Global.GetToolButton(SaveAction, "Toolbar.Save.png");
            toolBar1.Add(toolSave);

            toolBar1.Add(new SeparatorToolItem());

            toolUndo = Global.GetToolButton(UndoAction, "Toolbar.Undo.png");
            toolBar1.Add(toolUndo);

            toolRedo = Global.GetToolButton(RedoAction, "Toolbar.Redo.png");
            toolBar1.Add(toolRedo);

            toolBar1.Add(new SeparatorToolItem());

            toolNewItem = Global.GetToolButton(NewItemAction, "Toolbar.NewItem.png");
            toolBar1.Add(toolNewItem);

            toolAddItem = Global.GetToolButton(ExistingItemAction, "Toolbar.ExistingItem.png");
            toolBar1.Add(toolAddItem);

            toolNewFolder = Global.GetToolButton(NewFolderAction, "Toolbar.NewFolder.png");
            toolBar1.Add(toolNewFolder);

            toolAddFolder = Global.GetToolButton(ExistingFolderAction, "Toolbar.ExistingFolder.png");
            toolBar1.Add(toolAddFolder);

            toolBar1.Add(new SeparatorToolItem());

            toolBuild = Global.GetToolButton(BuildAction1, "Toolbar.Build.png");
            toolBar1.Add(toolBuild);

            toolRebuild = Global.GetToolButton(RebuildAction, "Toolbar.Rebuild.png");
            toolBar1.Add(toolRebuild);

            toolClean = Global.GetToolButton(CleanAction, "Toolbar.Clean.png");
            toolBar1.Add(toolClean);

            toolCancelBuild = Global.GetToolButton(CancelBuildAction, "Toolbar.CancelBuild.png");
            toolBar1.Add(toolCancelBuild);

            toolBar1.Add(new SeparatorToolItem());

            toolFilterOutput = Global.GetToggleToolButton(FilterOutputAction, "Toolbar.FilterOutput.png");
            toolBar1.Add(toolFilterOutput);

            if (!Global.UseHeaderBar)
                this.vbox2.PackStart(toolBar1, false, true, 0);

			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.menubar1]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hpaned1 = new global::Gtk.HPaned ();
			this.hpaned1.CanFocus = true;
			this.hpaned1.Name = "hpaned1";
			this.hpaned1.Position = 179;
			// Container child hpaned1.Gtk.Paned+PanedChild
			this.vpaned2 = new global::Gtk.VPaned ();
			this.vpaned2.CanFocus = true;
			this.vpaned2.Name = "vpaned2";
			this.vpaned2.Position = 247;
			// Container child vpaned2.Gtk.Paned+PanedChild
			this.projectview1 = new global::MonoGame.Tools.Pipeline.ProjectView ();
			this.projectview1.Events = ((global::Gdk.EventMask)(256));
			this.projectview1.Name = "projectview1";
			this.vpaned2.Add (this.projectview1);
			global::Gtk.Paned.PanedChild w3 = ((global::Gtk.Paned.PanedChild)(this.vpaned2 [this.projectview1]));
			w3.Resize = false;
			// Container child vpaned2.Gtk.Paned+PanedChild
			this.propertiesview1 = new global::MonoGame.Tools.Pipeline.PropertiesView ();
			this.propertiesview1.Events = ((global::Gdk.EventMask)(256));
			this.propertiesview1.Name = "propertiesview1";
			this.vpaned2.Add (this.propertiesview1);
			global::Gtk.Paned.PanedChild w4 = ((global::Gtk.Paned.PanedChild)(this.vpaned2 [this.propertiesview1]));
			w4.Resize = false;
			this.hpaned1.Add (this.vpaned2);
			global::Gtk.Paned.PanedChild w5 = ((global::Gtk.Paned.PanedChild)(this.hpaned1 [this.vpaned2]));
			w5.Resize = false;
			// Container child hpaned1.Gtk.Paned+PanedChild
            buildOutput1 = new BuildOutput();
            this.hpaned1.Add (this.buildOutput1);
			this.vbox2.Add (this.hpaned1);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hpaned1]));
			w8.Position = 2;
			this.Add (this.vbox2);

            treeview1 = new TreeView();

#if GTK3
            if(Global.UseHeaderBar)
            {
                vbox2.Remove (menubar1);

                var builder = new Builder(null, "MonoGame.Tools.Pipeline.Gtk.MainWindow.HeaderBar.glade", null);

                hbar = new HeaderBar(builder.GetObject("headerbar").Handle);
                open_button = new MenuButton(builder.GetObject("open_button").Handle);
                gear_button = new MenuButton(builder.GetObject("gear_button").Handle);
                debugmode_button = new ModalButton(builder.GetObject("debugmode_button").Handle);
                builder.Autoconnect(this);

                hbar.AttachToWindow(this);
                hbar.ShowCloseButton = true;
                hbar.Show();

                var gearpopover = new PopoverMenu(builder.GetObject("popovermenu1").Handle);
                gearpopover.WidthRequest = 200;

                gear_button.Popover = gearpopover;

                new_button.UseActionAppearance = false;
                new_button.RelatedAction = NewAction;
                save_button.UseActionAppearance = false;
                save_button.RelatedAction = SaveAction;
                build_button.UseActionAppearance = false;
                build_button.RelatedAction = BuildAction1;
                rebuild_button.UseActionAppearance = false;
                rebuild_button.RelatedAction = RebuildAction;
                cancel_button.UseActionAppearance = false;
                cancel_button.RelatedAction = CancelBuildAction;
                saveas_button.RelatedAction = SaveAsAction;
                undo_button.RelatedAction = UndoAction;
                redo_button.RelatedAction = RedoAction;
                close_button.RelatedAction = CloseAction;
                clean_button.RelatedAction = CleanAction;
                debugmode_button.RelatedAction = DebugModeAction;
                filteroutput_button.UseActionAppearance = false;
                filteroutput_button.RelatedAction = FilterOutputAction;

                var popover = new Popover(open_button);

                var vbox = new VBox();
                vbox.WidthRequest = 350;
                vbox.HeightRequest = 300;

                Gtk3Wrapper.gtk_tree_view_set_activate_on_single_click(treeview1.Handle, true);
                treeview1.HeadersVisible = false;
                treeview1.EnableGridLines = TreeViewGridLines.Horizontal;
                treeview1.HoverSelection = true;
                treeview1.RowActivated += delegate(object o, RowActivatedArgs args) {
                    popover.Hide();

                    TreeIter iter;
                    if(!recentListStore.GetIter(out iter, args.Path))
                        return;

                    OpenProject(recentListStore.GetValue(iter, 1).ToString());
                };

                var scroll1 = new ScrolledWindow();
                scroll1.WidthRequest = 350;
                scroll1.HeightRequest = 300;
                scroll1.Add(treeview1);

                vbox.PackStart(scroll1, true, true, 0);

                var hbox = new HBox();

                var openButton = new Button("Open Other...");
                openButton.Clicked += delegate(object sender, System.EventArgs e) {
                    popover.Hide();
                    OnOpenActionActivated(sender, e);
                };
                hbox.PackStart(openButton, true, true, 0);

                var importButton = new Button("Import");
                importButton.Clicked += delegate(object sender, System.EventArgs e) {
                    popover.Hide();
                    OnImportActionActivated(sender, e);
                };
                hbox.PackStart(importButton, false, true, 1);

                vbox.PackStart(hbox, false, true, 1);

                vbox.ShowAll();

                popover.Add(vbox);
                open_button.Popover = popover;
            }
#endif

            this.Title = basetitle;

			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 751;
            this.DefaultHeight = 557;

            #if GTK3
            Gdk.Geometry geom = new Gdk.Geometry();
            geom.MinWidth = 400;
            geom.MinHeight = 300;
            this.SetGeometryHints(this, geom, Gdk.WindowHints.MinSize);
            #endif

			this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
            this.NewAction.Activated += new global::System.EventHandler (this.OnNewActionActivated);
            this.OpenAction.Activated += new global::System.EventHandler (this.OnOpenActionActivated);
			this.CloseAction.Activated += new global::System.EventHandler (this.OnCloseActionActivated);
			this.ImportAction.Activated += new global::System.EventHandler (this.OnImportActionActivated);
            this.SaveAction.Activated += new global::System.EventHandler (this.OnSaveActionActivated);
			this.SaveAsAction.Activated += new global::System.EventHandler (this.OnSaveAsActionActivated);
            this.ExitAction.Activated += new global::System.EventHandler (this.OnExitActionActivated);
			this.UndoAction.Activated += new global::System.EventHandler (this.OnUndoActionActivated);
            this.RedoAction.Activated += new global::System.EventHandler (this.OnRedoActionActivated);
            ExcludeAction.Activated += this.OnExcludeActionActivated;
            RenameAction.Activated += this.OnRenameActionActivated;
			this.DeleteAction.Activated += new global::System.EventHandler (this.OnDeleteActionActivated);
			this.BuildAction1.Activated += new global::System.EventHandler (this.OnBuildAction1Activated);
			this.RebuildAction.Activated += new global::System.EventHandler (this.OnRebuildActionActivated);
			this.CleanAction.Activated += new global::System.EventHandler (this.OnCleanActionActivated);
			this.ViewHelpAction.Activated += new global::System.EventHandler (this.OnViewHelpActionActivated);
			this.AboutAction.Activated += new global::System.EventHandler (this.OnAboutActionActivated);
            this.NewItemAction.Activated += new global::System.EventHandler (this.OnNewItemActionActivated);
            this.NewFolderAction.Activated += new global::System.EventHandler (this.OnNewFolderActionActivated);
			this.ExistingItemAction.Activated += new global::System.EventHandler (this.OnAddItemActionActivated);
			this.ExistingFolderAction.Activated += new global::System.EventHandler (this.OnAddFolderActionActivated);
			this.DebugModeAction.Activated += new global::System.EventHandler (this.OnDebugModeActionActivated); 
            this.FilterOutputAction.Activated += OnFilterOutputActionActivated;
			this.CancelBuildAction.Activated += new global::System.EventHandler (this.OnCancelBuildActionActivated);
			this.SizeAllocated += MainWindow_SizeAllocated;
		}
	}
}
