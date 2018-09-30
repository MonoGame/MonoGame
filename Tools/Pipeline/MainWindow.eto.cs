// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto;
using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    partial class MainWindow
    {
        /// <summary>
        /// Pipeline menu bar.
        /// Required to Stop Eto Forms adding System Menu Items on MacOS
        /// This is because `IncludeSystemItems` defaults to `All` 
        /// and the menus are populated in the constructor.
        /// </summary>
        class PipelineMenuBar : MenuBar
        {
            public PipelineMenuBar()
            {
                Style = "MenuBar";
                IncludeSystemItems = MenuBarSystemItems.None;
            }
        }

        public Command cmdNew, cmdOpen, cmdClose, cmdImport, cmdSave, cmdSaveAs, cmdExit;
        public Command cmdUndo, cmdRedo, cmdAdd, cmdExclude, cmdRename, cmdDelete;
        public Command cmdNewItem, cmdNewFolder, cmdExistingItem, cmdExistingFolder;
        public Command cmdBuild, cmdRebuild, cmdClean, cmdCancelBuild;
        public CheckCommand cmdDebugMode;
        public Command cmdHelp, cmdAbout;
        public Command cmdOpenItem, cmdOpenItemWith, cmdOpenItemLocation, cmdOpenOutputItemLocation, cmdCopyAssetName, cmdRebuildItem;

        ToolBar toolbar;
        ButtonMenuItem menuFile, menuRecent, menuEdit, menuAdd, menuView, menuBuild, menuHelp;
        ToolItem toolBuild, toolRebuild, toolClean, toolCancelBuild;
        MenuItem cmOpenItem, cmOpenItemWith, cmOpenItemLocation, cmOpenOutputItemLocation, cmCopyAssetPath, cmRebuildItem, cmExclude, cmRename, cmDelete;
        ButtonMenuItem cmAdd;

        ProjectControl projectControl;
        PropertyGridControl propertyGridControl;
        BuildOutput buildOutput;

        Splitter splitterHorizontal, splitterVertical;

        private void InitializeComponent()
        {
            Title = "MonoGame Pipeline Tool";
            Icon = Icon.FromResource("Icons.monogame.png");
            Size = new Size(750, 550);
            MinimumSize = new Size(400, 400);

            InitalizeCommands();
            InitalizeMenu();
            InitalizeContextMenu();
            InitalizeToolbar();

            splitterHorizontal = new Splitter();
            splitterHorizontal.Orientation = Orientation.Horizontal;
            splitterHorizontal.Position = 200;
            splitterHorizontal.Panel1MinimumSize = 100;
            splitterHorizontal.Panel2MinimumSize = 100;

            splitterVertical = new Splitter();
            splitterVertical.Orientation = Orientation.Vertical;
            splitterVertical.Position = 230;
            splitterVertical.FixedPanel = SplitterFixedPanel.None;
            splitterVertical.Panel1MinimumSize = 100;
            splitterVertical.Panel2MinimumSize = 100;

            projectControl = new ProjectControl();
            _pads.Add(projectControl);
            splitterVertical.Panel1 = projectControl;

            propertyGridControl = new PropertyGridControl();
            _pads.Add(propertyGridControl);
            splitterVertical.Panel2 = propertyGridControl;

            splitterHorizontal.Panel1 = splitterVertical;

            buildOutput = new BuildOutput();
            _pads.Add(buildOutput);
            splitterHorizontal.Panel2 = buildOutput;

            Content = splitterHorizontal;

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
            cmdDebugMode.Executed += CmdDebugMode_Executed;

            cmdHelp.Executed += CmdHelp_Executed;
            cmdAbout.Executed += CmdAbout_Executed;

            cmdOpenItem.Executed += CmdOpenItem_Executed;
            cmdOpenItemWith.Executed += CmdOpenItemWith_Executed;
            cmdOpenItemLocation.Executed += CmdOpenItemLocation_Executed;
            cmdOpenOutputItemLocation.Executed += CmdOpenOutputItemLocation_Executed;
            cmdCopyAssetName.Executed += CmdCopyAssetPath_Executed;
            cmdRebuildItem.Executed += CmdRebuildItem_Executed;
        }

        private void InitalizeCommands()
        {
            // File Commands

            cmdNew = new Command();
            cmdNew.MenuText = "New...";
            cmdNew.ToolTip = "New";
            cmdNew.Image = Global.GetEtoIcon("Commands.New.png");
            cmdNew.Shortcut = Application.Instance.CommonModifier | Keys.N;

            cmdOpen = new Command();
            cmdOpen.MenuText = "Open...";
            cmdOpen.ToolTip = "Open";
            cmdOpen.Image = Global.GetEtoIcon("Commands.Open.png");
            cmdOpen.Shortcut = Application.Instance.CommonModifier | Keys.O;

            cmdClose = new Command();
            cmdClose.MenuText = "Close";
            cmdClose.Image = Global.GetEtoIcon("Commands.Close.png");

            cmdImport = new Command();
            cmdImport.MenuText = "Import";

            cmdSave = new Command();
            cmdSave.MenuText = "Save...";
            cmdSave.ToolTip = "Save";
            cmdSave.Image = Global.GetEtoIcon("Commands.Save.png");
            cmdSave.Shortcut = Application.Instance.CommonModifier | Keys.S;

            cmdSaveAs = new Command();
            cmdSaveAs.MenuText = "Save As";
            cmdSaveAs.Image = Global.GetEtoIcon("Commands.SaveAs.png");

            cmdExit = new Command();
            cmdExit.MenuText = Global.Unix ? "Quit" : "Exit";
            cmdExit.Shortcut = Application.Instance.CommonModifier | Keys.Q;

            // Edit Commands

            cmdUndo = new Command();
            cmdUndo.MenuText = "Undo";
            cmdUndo.ToolTip = "Undo";
            cmdUndo.Image = Global.GetEtoIcon("Commands.Undo.png");
            cmdUndo.Shortcut = Application.Instance.CommonModifier | Keys.Z;

            cmdRedo = new Command();
            cmdRedo.MenuText = "Redo";
            cmdRedo.ToolTip = "Redo";
            cmdRedo.Image = Global.GetEtoIcon("Commands.Redo.png");
            cmdRedo.Shortcut = Application.Instance.CommonModifier | Keys.Y;

            cmdAdd = new Command();
            cmdAdd.MenuText = "Add";

            cmdExclude = new Command();
            cmdExclude.MenuText = "Exclude From Project";

            cmdRename = new Command();
            cmdRename.MenuText = "Rename";
            cmdRename.Image = Global.GetEtoIcon("Commands.Rename.png");

            cmdDelete = new Command();
            cmdDelete.MenuText = "Delete";
            cmdDelete.Image = Global.GetEtoIcon("Commands.Delete.png");
            cmdDelete.Shortcut = Keys.Delete;

            // Add Submenu

            cmdNewItem = new Command();
            cmdNewItem.MenuText = "New Item...";
            cmdNewItem.ToolTip = "New Item";
            cmdNewItem.Image = Global.GetEtoIcon("Commands.NewItem.png");

            cmdNewFolder = new Command();
            cmdNewFolder.MenuText = "New Folder...";
            cmdNewFolder.ToolTip = "New Folder";
            cmdNewFolder.Image = Global.GetEtoIcon("Commands.NewFolder.png");

            cmdExistingItem = new Command();
            cmdExistingItem.MenuText = "Existing Item...";
            cmdExistingItem.ToolTip = "Add Existing Item";
            cmdExistingItem.Image = Global.GetEtoIcon("Commands.ExistingItem.png");

            cmdExistingFolder = new Command();
            cmdExistingFolder.MenuText = "Existing Folder...";
            cmdExistingFolder.ToolTip = "Add Existing Folder";
            cmdExistingFolder.Image = Global.GetEtoIcon("Commands.ExistingFolder.png");

            // Build Commands

            cmdBuild = new Command();
            cmdBuild.MenuText = "Build";
            cmdBuild.ToolTip = "Build";
            cmdBuild.Image = Global.GetEtoIcon("Commands.Build.png");
            cmdBuild.Shortcut = Keys.F6;

            cmdRebuild = new Command();
            cmdRebuild.MenuText = "Rebuild";
            cmdRebuild.ToolTip = "Rebuild";
            cmdRebuild.Image = Global.GetEtoIcon("Commands.Rebuild.png");

            cmdClean = new Command();
            cmdClean.MenuText = "Clean";
            cmdClean.ToolTip = "Clean";
            cmdClean.Image = Global.GetEtoIcon("Commands.Clean.png");

            cmdCancelBuild = new Command();
            cmdCancelBuild.MenuText = "Cancel Build";
            cmdCancelBuild.ToolTip = "Cancel Build";
            cmdCancelBuild.Image = Global.GetEtoIcon("Commands.CancelBuild.png");

            cmdDebugMode = new CheckCommand();
            cmdDebugMode.MenuText = "Debug Mode";

            // Help Commands

            cmdHelp = new Command();
            cmdHelp.MenuText = "View Help";
            cmdHelp.Shortcut = Keys.F1;
            cmdHelp.Image = Global.GetEtoIcon("Commands.Help.png");

            cmdAbout = new Command();
            cmdAbout.MenuText = "About";

            // Context Menu

            cmdOpenItem = new Command();
            cmdOpenItem.MenuText = "Open";
            cmdOpenItem.Image = Global.GetEtoIcon("Commands.OpenItem.png");

            cmdOpenItemWith = new Command();
            cmdOpenItemWith.MenuText = "Open With";

            cmdOpenItemLocation = new Command();
            cmdOpenItemLocation.MenuText = "Open Containing Directory";

            cmdOpenOutputItemLocation = new Command();
            cmdOpenOutputItemLocation.MenuText = "Open Output Directory";

            cmdCopyAssetName = new Command();
            cmdCopyAssetName.MenuText = "Copy Asset Name";

            cmdRebuildItem = new Command();
            cmdRebuildItem.Image = Global.GetEtoIcon("Commands.Rebuild.png");
            cmdRebuildItem.MenuText = "Rebuild";
        }

        private void InitalizeMenu()
        {
            Menu = new PipelineMenuBar();

            menuFile = new ButtonMenuItem();
            menuFile.Text = "&File";
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
            menuEdit.Text = "&Edit";
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
            //menuEdit.Items.Add(cmdDelete);
            Menu.Items.Add(menuEdit);

            // View Commands

            menuView = new ButtonMenuItem();
            menuView.Text = "&View";
            Menu.Items.Add(menuView);

            menuBuild = new ButtonMenuItem();
            menuBuild.Text = "&Build";
            menuBuild.Items.Add(cmdBuild);
            menuBuild.Items.Add(cmdRebuild);
            menuBuild.Items.Add(cmdClean);
            menuBuild.Items.Add(cmdCancelBuild);
            menuBuild.Items.Add(new SeparatorMenuItem());
            menuBuild.Items.Add(cmdDebugMode);
            Menu.Items.Add(menuBuild);

            menuHelp = new ButtonMenuItem();
            menuHelp.Text = "&Help";
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
            cmOpenOutputItemLocation = cmdOpenOutputItemLocation.CreateMenuItem();
            cmCopyAssetPath = cmdCopyAssetName.CreateMenuItem();
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
            ToolBar.Items.Add(new SeparatorToolItem { Type = SeparatorToolItemType.Divider });
            ToolBar.Items.Add(cmdUndo);
            ToolBar.Items.Add(cmdRedo);
            ToolBar.Items.Add(new SeparatorToolItem { Type = SeparatorToolItemType.Divider });
            ToolBar.Items.Add(cmdNewItem);
            ToolBar.Items.Add(cmdExistingItem);
            ToolBar.Items.Add(cmdNewFolder);
            ToolBar.Items.Add(cmdExistingFolder);
            ToolBar.Items.Add(new SeparatorToolItem { Type = SeparatorToolItemType.Divider });
            ToolBar.Items.Add(toolBuild);
            ToolBar.Items.Add(toolRebuild);
            ToolBar.Items.Add(toolClean);
            toolbar.Items.Add(toolCancelBuild);
        }
    }
}

