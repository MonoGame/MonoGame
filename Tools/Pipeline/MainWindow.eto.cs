using System;
using Eto;
using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    partial class MainWindow
    {
        public Command cmdNew, cmdOpen, cmdClose, cmdImport, cmdSave, cmdSaveAs, cmdExit;
        public Command cmdUndo, cmdRedo, cmdAdd, cmdExclude, cmdRename, cmdDelete;
        public Command cmdNewItem, cmdNewFolder, cmdExistingItem, cmdExistingFolder;
        public Command cmdBuild, cmdRebuild, cmdClean, cmdCancelBuild;
        public CheckCommand cmdDebugMode, cmdFilterOutput;
        public Command cmdHelp, cmdAbout;
        public Command cmdOpenItem, cmdOpenItemWith, cmdOpenItemLocation, cmdRebuildItem;

        MenuBar menubar;
        ToolBar toolbar;
        ButtonMenuItem menuFile, menuRecent, menuEdit, menuAdd, menuBuild, menuHelp;
        ToolItem toolBuild, toolRebuild, toolClean, toolCancelBuild;
        MenuItem cmOpenItem, cmOpenItemWith, cmOpenItemLocation, cmRebuildItem, cmExclude, cmRename, cmDelete;
        ButtonMenuItem cmAdd;

        ProjectControl projectControl;
        PropertyGridControl propertyGridControl;
        BuildOutput buildOutput;

        Splitter splitterHorizontal, splitterVertical;

        private void InitializeComponent()
        {
            Title = "MonoGame Pipeline Tool";
            Icon = Icon.FromResource("Icons.monogame.png");
            Width = 750;
            Height = 550;

            InitalizeCommands();
            InitalizeMenu();
            InitalizeContextMenu();
            InitalizeToolbar();

            splitterHorizontal = new Splitter();
            splitterHorizontal.Orientation = Orientation.Horizontal;
            splitterHorizontal.Position = 200;

            splitterVertical = new Splitter();
            splitterVertical.Orientation = Orientation.Vertical;
            splitterVertical.Position = 230;
            splitterVertical.FixedPanel = SplitterFixedPanel.None;

            projectControl = new ProjectControl();
            splitterVertical.Panel1 = projectControl;

            propertyGridControl = new PropertyGridControl();
            splitterVertical.Panel2 = propertyGridControl;

            splitterHorizontal.Panel1 = splitterVertical;

            buildOutput = new BuildOutput();
            splitterHorizontal.Panel2 = buildOutput;

            Content = splitterHorizontal;

            projectControl.MouseDoubleClick += CmdOpenItem_Executed;

            cmdNew.Executed += CmdNew_Executed;
            cmdOpen.Executed += CmdOpen_Executed;
            cmdClose.Executed += CmdClose_Executed;
            cmdImport.Executed += CmdImport_Executed;
            cmdSave.Executed += CmdSave_Executed;
            cmdSaveAs.Executed += CmdSaveAs_Executed;
            cmdExit.Executed += CmdExit_Executed;

            cmdUndo.Executed += CmdUndo_Executed;
            cmdRedo.Executed += CmdRedo_Executed;
            cmdExclude.Executed += CmdExclude_Executed;
            cmdRename.Executed += CmdRename_Executed;
            cmdDelete.Executed += CmdDelete_Executed;

            cmdNewItem.Executed += CmdNewItem_Executed;
            cmdNewFolder.Executed += CmdNewFolder_Executed;
            cmdExistingItem.Executed += CmdExistingItem_Executed;
            cmdExistingFolder.Executed += CmdExistingFolder_Executed;

            cmdBuild.Executed += CmdBuild_Executed;
            cmdRebuild.Executed += CmdRebuild_Executed;
            cmdClean.Executed += CmdClean_Executed;
            cmdCancelBuild.Executed += CmdCancelBuild_Executed;
            cmdDebugMode.CheckedChanged += CmdDebugMode_Executed;
            cmdFilterOutput.CheckedChanged += CmdFilterOutput_Executed;

            cmdHelp.Executed += CmdHelp_Executed;
            cmdAbout.Executed += CmdAbout_Executed;

            cmdOpenItem.Executed += CmdOpenItem_Executed;
            cmdOpenItemWith.Executed += CmdOpenItemWith_Executed;
            cmdOpenItemLocation.Executed += CmdOpenItemLocation_Executed;
            cmdRebuildItem.Executed += CmdRebuildItem_Executed;
        }

        private void InitalizeCommands()
        {
            // File Commands

            cmdNew = new Command();
            cmdNew.Shortcut = Application.Instance.CommonModifier | Keys.N;
            cmdNew.ToolTip = "New";
            cmdNew.MenuText = "New...";

            cmdOpen = new Command();
            cmdOpen.MenuText = "Open...";
            cmdOpen.ToolTip = "Open";
            cmdOpen.Shortcut = Application.Instance.CommonModifier | Keys.O;

            cmdClose = new Command();
            cmdClose.MenuText = "Close";

            cmdImport = new Command();
            cmdImport.MenuText = "Import";

            cmdSave = new Command();
            cmdSave.MenuText = "Save...";
            cmdSave.ToolTip = "Save";
            cmdSave.Shortcut = Application.Instance.CommonModifier | Keys.S;

            cmdSaveAs = new Command();
            cmdSaveAs.MenuText = "Save As";

            cmdExit = new Command();
            cmdExit.MenuText = "Exit";
            cmdExit.Shortcut = Application.Instance.CommonModifier | Keys.Q;

            // Edit Commands

            cmdUndo = new Command();
            cmdUndo.MenuText = "Undo";
            cmdUndo.ToolTip = "Undo";
            cmdUndo.Shortcut = Application.Instance.CommonModifier | Keys.Z;

            cmdRedo = new Command();
            cmdRedo.MenuText = "Redo";
            cmdRedo.ToolTip = "Redo";
            cmdRedo.Shortcut = Application.Instance.CommonModifier | Keys.Y;

            cmdAdd = new Command();
            cmdAdd.MenuText = "Add";

            cmdExclude = new Command();
            cmdExclude.MenuText = "Exclude From Project";

            cmdRename = new Command();
            cmdRename.MenuText = "Rename";

            cmdDelete = new Command();
            cmdDelete.MenuText = "Delete";
            cmdDelete.Shortcut = Keys.Delete;

            // Add Submenu

            cmdNewItem = new Command();
            cmdNewItem.MenuText = "New Item...";
            cmdNewItem.ToolTip = "New Item";

            cmdNewFolder = new Command();
            cmdNewFolder.MenuText = "New Folder...";
            cmdNewFolder.ToolTip = "New Folder";

            cmdExistingItem = new Command();
            cmdExistingItem.MenuText = "Existing Item...";
            cmdExistingItem.ToolTip = "Add Existing Item";

            cmdExistingFolder = new Command();
            cmdExistingFolder.MenuText = "Existing Folder...";
            cmdExistingFolder.ToolTip = "Add Existing Folder";

            // Build Commands

            cmdBuild = new Command();
            cmdBuild.MenuText = "Build";
            cmdBuild.ToolTip = "Build";
            cmdBuild.Shortcut = Keys.F6;

            cmdRebuild = new Command();
            cmdRebuild.MenuText = "Rebuild";
            cmdRebuild.ToolTip = "Rebuild";

            cmdClean = new Command();
            cmdClean.MenuText = "Clean";
            cmdClean.ToolTip = "Clean";

            cmdCancelBuild = new Command();
            cmdCancelBuild.MenuText = "Cancel Build";
            cmdCancelBuild.ToolTip = "Cancel Build";

            cmdDebugMode = new CheckCommand();
            cmdDebugMode.MenuText = "Debug Mode";

            cmdFilterOutput = new CheckCommand();
            cmdFilterOutput.MenuText = "Filter Output";
            cmdFilterOutput.ToolTip = "Filter Output";
            cmdFilterOutput.Checked = true;

            // Help Commands

            cmdHelp = new Command();
            cmdHelp.MenuText = "View Help";
            cmdHelp.Shortcut = Keys.F1;

            cmdAbout = new Command();
            cmdAbout.MenuText = "About";

            // Context Menu

            cmdOpenItem = new Command();
            cmdOpenItem.MenuText = "Open";

            cmdOpenItemWith = new Command();
            cmdOpenItemWith.MenuText = "Open With";

            cmdOpenItemLocation = new Command();
            cmdOpenItemLocation.MenuText = "Open Containing Directory";

            cmdRebuildItem = new Command();
            cmdRebuildItem.MenuText = "Rebuild";

            ReloadIcons();
        }

        public void ReloadIcons()
        {
            Global.SetIcon(cmdNew);
            Global.SetIcon(cmdOpen);
            Global.SetIcon(cmdClose);
            Global.SetIcon(cmdImport);
            Global.SetIcon(cmdSave);
            Global.SetIcon(cmdSaveAs);
            Global.SetIcon(cmdExit);
            Global.SetIcon(cmdUndo);
            Global.SetIcon(cmdRedo);
            Global.SetIcon(cmdAdd);
            Global.SetIcon(cmdExclude);
            Global.SetIcon(cmdRename);
            Global.SetIcon(cmdDelete);
            Global.SetIcon(cmdNewItem);
            Global.SetIcon(cmdNewFolder);
            Global.SetIcon(cmdExistingItem);
            Global.SetIcon(cmdExistingFolder);
            Global.SetIcon(cmdBuild);
            Global.SetIcon(cmdRebuild);
            Global.SetIcon(cmdClean);
            Global.SetIcon(cmdCancelBuild);
            Global.SetIcon(cmdDebugMode);
            Global.SetIcon(cmdFilterOutput);
            Global.SetIcon(cmdHelp);
            Global.SetIcon(cmdAbout);
            Global.SetIcon(cmdOpenItem);
            Global.SetIcon(cmdOpenItemWith);
            Global.SetIcon(cmdOpenItemLocation);
            Global.SetIcon(cmdRebuildItem);
        }

        private void InitalizeMenu()
        {
            Menu = menubar = new MenuBar();
            Menu.Style = "MenuBar";

            menuFile = new ButtonMenuItem();
            menuFile.Text = "File";
            menuFile.Items.Add(cmdNew);
            menuFile.Items.Add(cmdOpen);

            menuRecent = new ButtonMenuItem();
            menuRecent.Text = "Open Recent";
            menuFile.Items.Add(menuRecent);

            menuFile.Items.Add(cmdClose);
            menuFile.Items.Add(new SeparatorMenuItem());
            menuFile.Items.Add(cmdImport);
            menuFile.Items.Add(new SeparatorMenuItem());
            menuFile.Items.Add(cmdSave);
            menuFile.Items.Add(cmdSaveAs);
            Menu.Items.Add(menuFile);

            menuEdit = new ButtonMenuItem();
            menuEdit.Text = "Edit";
            menuEdit.Items.Add(cmdUndo);
            menuEdit.Items.Add(cmdRedo);
            menuEdit.Items.Add(new SeparatorMenuItem());

            menuAdd = (ButtonMenuItem)cmdAdd.CreateMenuItem();
            menuAdd.Items.Add(cmdNewItem);
            menuAdd.Items.Add(cmdNewFolder);
            menuAdd.Items.Add(new SeparatorMenuItem());
            menuAdd.Items.Add(cmdExistingItem);
            menuAdd.Items.Add(cmdExistingFolder);
            menuEdit.Items.Add(menuAdd);

            menuEdit.Items.Add(new SeparatorMenuItem());
            menuEdit.Items.Add(cmdExclude);
            menuEdit.Items.Add(new SeparatorMenuItem());
            menuEdit.Items.Add(cmdRename);
            menuEdit.Items.Add(cmdDelete);
            Menu.Items.Add(menuEdit);

            menuBuild = new ButtonMenuItem();
            menuBuild.Text = "Build";
            menuBuild.Items.Add(cmdBuild);
            menuBuild.Items.Add(cmdRebuild);
            menuBuild.Items.Add(cmdClean);
            menuBuild.Items.Add(cmdCancelBuild);
            menuBuild.Items.Add(new SeparatorMenuItem());
            menuBuild.Items.Add(cmdDebugMode);
            menuBuild.Items.Add(cmdFilterOutput);
            Menu.Items.Add(menuBuild);

            menuHelp = new ButtonMenuItem();
            menuHelp.Text = "Help";
            menuHelp.Items.Add(cmdHelp);
            Menu.Items.Add(menuHelp);

            Menu.QuitItem = cmdExit;
            Menu.AboutItem = cmdAbout;
        }

        private void InitalizeContextMenu()
        {
            cmOpenItem = cmdOpenItem.CreateMenuItem();
            cmOpenItemWith = cmdOpenItemWith.CreateMenuItem();

            cmAdd = (ButtonMenuItem)cmdAdd.CreateMenuItem();
            cmAdd.Items.Add(cmdNewItem.CreateMenuItem());
            cmAdd.Items.Add(cmdNewFolder.CreateMenuItem());
            cmAdd.Items.Add(new SeparatorMenuItem());
            cmAdd.Items.Add(cmdExistingItem.CreateMenuItem());
            cmAdd.Items.Add(cmdExistingFolder.CreateMenuItem());

            cmOpenItemLocation = cmdOpenItemLocation.CreateMenuItem();
            cmRebuildItem = cmdRebuildItem.CreateMenuItem();
            cmExclude = cmdExclude.CreateMenuItem();
            cmRename = cmdRename.CreateMenuItem();
            cmDelete = cmdDelete.CreateMenuItem();
        }

        private void InitalizeToolbar()
        {
            toolBuild = cmdBuild.CreateToolItem();
            toolRebuild = cmdRebuild.CreateToolItem();
            toolClean = cmdClean.CreateToolItem();
            toolCancelBuild = cmdCancelBuild.CreateToolItem();

            ToolBar = toolbar = new ToolBar();
            ToolBar.Style = "ToolBar";
            ToolBar.Items.Add(cmdNew);
            ToolBar.Items.Add(cmdOpen);
            ToolBar.Items.Add(cmdSave);
            ToolBar.Items.Add(new SeparatorToolItem());
            ToolBar.Items.Add(cmdUndo);
            ToolBar.Items.Add(cmdRedo);
            ToolBar.Items.Add(new SeparatorToolItem());
            ToolBar.Items.Add(cmdNewItem);
            ToolBar.Items.Add(cmdExistingItem);
            ToolBar.Items.Add(cmdNewFolder);
            ToolBar.Items.Add(cmdExistingFolder);
            ToolBar.Items.Add(new SeparatorToolItem());
            ToolBar.Items.Add(toolBuild);
            ToolBar.Items.Add(toolRebuild);
            ToolBar.Items.Add(toolClean);
            ToolBar.Items.Add(new SeparatorToolItem());
            ToolBar.Items.Add(cmdFilterOutput);
        }
    }
}

