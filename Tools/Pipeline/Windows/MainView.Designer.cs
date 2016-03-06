// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class MainView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator _toolStripSeparator3;
            System.Windows.Forms.ToolStripSeparator _toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator _toolStripSeparator2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            this._mainMenu = new System.Windows.Forms.MenuStrip();
            this._fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._newProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openRecentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._closeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._importProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._undoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._redoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._addMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.existingItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.existingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._renameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._deleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._buildMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._buildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._rebuildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._cleanMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this._filterOutputMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._debuggerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._cancelBuildSeparator = new System.Windows.Forms.ToolStripSeparator();
            this._cancelBuildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._viewHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._treeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._treeOpenFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._treeAddMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this._treeSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._treeOpenFileLocationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._treeRebuildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this._treeRenameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._treeDeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolNew = new System.Windows.Forms.ToolStripButton();
            this._toolOpen = new System.Windows.Forms.ToolStripButton();
            this._toolSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this._toolNewItem = new System.Windows.Forms.ToolStripButton();
            this._toolAddItem = new System.Windows.Forms.ToolStripButton();
            this._toolNewFolder = new System.Windows.Forms.ToolStripButton();
            this._toolAddFolder = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this._toolBuild = new System.Windows.Forms.ToolStripButton();
            this._toolRebuild = new System.Windows.Forms.ToolStripButton();
            this._toolClean = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this._toolFilterOutput = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._splitEditorOutput = new System.Windows.Forms.SplitContainer();
            this._splitTreeProps = new System.Windows.Forms.SplitContainer();
            this._propertyGrid = new System.Windows.Forms.PropertyGrid();
            this._treeView = new MonoGame.Tools.Pipeline.MultiSelectTreeview();
            this._outputTabs = new MonoGame.Tools.Pipeline.Windows.Controls.TabControlEx();
            this._outputTabPage1 = new System.Windows.Forms.TabPage();
            this._outputWindow = new System.Windows.Forms.RichTextBox();
            this._outputTabPage2 = new System.Windows.Forms.TabPage();
            this._filterOutputWindow = new MonoGame.Tools.Pipeline.Windows.Controls.FilterOutputControl();
            _toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            _toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            _toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._mainMenu.SuspendLayout();
            this._treeContextMenu.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitEditorOutput)).BeginInit();
            this._splitEditorOutput.Panel1.SuspendLayout();
            this._splitEditorOutput.Panel2.SuspendLayout();
            this._splitEditorOutput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitTreeProps)).BeginInit();
            this._splitTreeProps.Panel1.SuspendLayout();
            this._splitTreeProps.Panel2.SuspendLayout();
            this._splitTreeProps.SuspendLayout();
            this._outputTabs.SuspendLayout();
            this._outputTabPage1.SuspendLayout();
            this._outputTabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // _toolStripSeparator3
            // 
            _toolStripSeparator3.Name = "_toolStripSeparator3";
            _toolStripSeparator3.Size = new System.Drawing.Size(179, 6);
            // 
            // _toolStripSeparator1
            // 
            _toolStripSeparator1.Name = "_toolStripSeparator1";
            _toolStripSeparator1.Size = new System.Drawing.Size(179, 6);
            // 
            // _toolStripSeparator2
            // 
            _toolStripSeparator2.Name = "_toolStripSeparator2";
            _toolStripSeparator2.Size = new System.Drawing.Size(143, 6);
            // 
            // _mainMenu
            // 
            this._mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileMenu,
            this.editToolStripMenuItem,
            this._buildMenu,
            this._helpMenu});
            this._mainMenu.Location = new System.Drawing.Point(0, 0);
            this._mainMenu.Name = "_mainMenu";
            this._mainMenu.Size = new System.Drawing.Size(784, 24);
            this._mainMenu.TabIndex = 0;
            this._mainMenu.Text = "menuStrip1";
            this._mainMenu.MenuActivate += new System.EventHandler(this.MainMenuMenuActivate);
            // 
            // _fileMenu
            // 
            this._fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newProjectMenuItem,
            this._openProjectMenuItem,
            this._openRecentMenuItem,
            this._closeMenuItem,
            this.toolStripSeparator3,
            this._importProjectMenuItem,
            _toolStripSeparator3,
            this._saveMenuItem,
            this._saveAsMenuItem,
            _toolStripSeparator1,
            this._exitMenuItem});
            this._fileMenu.Name = "_fileMenu";
            this._fileMenu.Size = new System.Drawing.Size(37, 20);
            this._fileMenu.Text = "&File";
            // 
            // _newProjectMenuItem
            // 
            this._newProjectMenuItem.Name = "_newProjectMenuItem";
            this._newProjectMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.N)));
            this._newProjectMenuItem.Size = new System.Drawing.Size(182, 22);
            this._newProjectMenuItem.Text = "New...";
            this._newProjectMenuItem.Click += new System.EventHandler(this.OnNewProjectClick);
            // 
            // _openProjectMenuItem
            // 
            this._openProjectMenuItem.Name = "_openProjectMenuItem";
            this._openProjectMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this._openProjectMenuItem.Size = new System.Drawing.Size(182, 22);
            this._openProjectMenuItem.Text = "Open...";
            this._openProjectMenuItem.Click += new System.EventHandler(this.OnOpenProjectClick);
            // 
            // _openRecentMenuItem
            // 
            this._openRecentMenuItem.Name = "_openRecentMenuItem";
            this._openRecentMenuItem.Size = new System.Drawing.Size(182, 22);
            this._openRecentMenuItem.Text = "Open Recent";
            // 
            // _closeMenuItem
            // 
            this._closeMenuItem.Name = "_closeMenuItem";
            this._closeMenuItem.Size = new System.Drawing.Size(182, 22);
            this._closeMenuItem.Text = "Close";
            this._closeMenuItem.Click += new System.EventHandler(this.OnCloseProjectClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(179, 6);
            // 
            // _importProjectMenuItem
            // 
            this._importProjectMenuItem.Name = "_importProjectMenuItem";
            this._importProjectMenuItem.Size = new System.Drawing.Size(182, 22);
            this._importProjectMenuItem.Text = "Import...";
            this._importProjectMenuItem.Click += new System.EventHandler(this.OnImportProjectClick);
            // 
            // _saveMenuItem
            // 
            this._saveMenuItem.Name = "_saveMenuItem";
            this._saveMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this._saveMenuItem.Size = new System.Drawing.Size(182, 22);
            this._saveMenuItem.Text = "&Save";
            this._saveMenuItem.Click += new System.EventHandler(this.OnSaveProjectClick);
            // 
            // _saveAsMenuItem
            // 
            this._saveAsMenuItem.Name = "_saveAsMenuItem";
            this._saveAsMenuItem.Size = new System.Drawing.Size(182, 22);
            this._saveAsMenuItem.Text = "Save &As...";
            this._saveAsMenuItem.Click += new System.EventHandler(this.OnSaveAsProjectClick);
            // 
            // _exitMenuItem
            // 
            this._exitMenuItem.Name = "_exitMenuItem";
            this._exitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this._exitMenuItem.Size = new System.Drawing.Size(182, 22);
            this._exitMenuItem.Text = "E&xit";
            this._exitMenuItem.Click += new System.EventHandler(this.ExitMenuItemClick);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._undoMenuItem,
            this._redoMenuItem,
            this.toolStripSeparator2,
            this._addMenuItem,
            this.toolStripSeparator1,
            this._renameMenuItem,
            this._deleteMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // _undoMenuItem
            // 
            this._undoMenuItem.Enabled = false;
            this._undoMenuItem.Name = "_undoMenuItem";
            this._undoMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this._undoMenuItem.Size = new System.Drawing.Size(144, 22);
            this._undoMenuItem.Text = "Undo";
            this._undoMenuItem.Click += new System.EventHandler(this.OnUndoClick);
            // 
            // _redoMenuItem
            // 
            this._redoMenuItem.Enabled = false;
            this._redoMenuItem.Name = "_redoMenuItem";
            this._redoMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this._redoMenuItem.Size = new System.Drawing.Size(144, 22);
            this._redoMenuItem.Text = "Redo";
            this._redoMenuItem.Click += new System.EventHandler(this.OnRedoClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(141, 6);
            // 
            // _addMenuItem
            // 
            this._addMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newItemToolStripMenuItem,
            this.newFolderToolStripMenuItem,
            this.toolStripMenuItem2,
            this.existingItemToolStripMenuItem,
            this.existingFolderToolStripMenuItem});
            this._addMenuItem.Name = "_addMenuItem";
            this._addMenuItem.Size = new System.Drawing.Size(144, 22);
            this._addMenuItem.Text = "Add";
            // 
            // newItemToolStripMenuItem
            // 
            this.newItemToolStripMenuItem.Name = "newItemToolStripMenuItem";
            this.newItemToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.newItemToolStripMenuItem.Text = "New Item...";
            this.newItemToolStripMenuItem.Click += new System.EventHandler(this.OnNewItemClick);
            // 
            // newFolderToolStripMenuItem
            // 
            this.newFolderToolStripMenuItem.Name = "newFolderToolStripMenuItem";
            this.newFolderToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.newFolderToolStripMenuItem.Text = "New Folder...";
            this.newFolderToolStripMenuItem.Click += new System.EventHandler(this.OnNewFolderClick);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(156, 6);
            // 
            // existingItemToolStripMenuItem
            // 
            this.existingItemToolStripMenuItem.Name = "existingItemToolStripMenuItem";
            this.existingItemToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.existingItemToolStripMenuItem.Text = "Existing Item...";
            this.existingItemToolStripMenuItem.Click += new System.EventHandler(this.OnAddItemClick);
            // 
            // existingFolderToolStripMenuItem
            // 
            this.existingFolderToolStripMenuItem.Name = "existingFolderToolStripMenuItem";
            this.existingFolderToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.existingFolderToolStripMenuItem.Text = "Existing Folder...";
            this.existingFolderToolStripMenuItem.Click += new System.EventHandler(this.OnAddFolderClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(141, 6);
            // 
            // _renameMenuItem
            // 
            this._renameMenuItem.Name = "_renameMenuItem";
            this._renameMenuItem.Size = new System.Drawing.Size(144, 22);
            this._renameMenuItem.Text = "Rename";
            this._renameMenuItem.Click += new System.EventHandler(this.OnRenameItemClick);
            // 
            // _deleteMenuItem
            // 
            this._deleteMenuItem.Name = "_deleteMenuItem";
            this._deleteMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this._deleteMenuItem.Size = new System.Drawing.Size(144, 22);
            this._deleteMenuItem.Text = "&Delete";
            this._deleteMenuItem.Click += new System.EventHandler(this.OnDeleteItemClick);
            // 
            // _buildMenu
            // 
            this._buildMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._buildMenuItem,
            this._rebuildMenuItem,
            this._cleanMenuItem,
            this.toolStripSeparator5,
            this._filterOutputMenuItem,
            this._debuggerMenuItem,
            this._cancelBuildSeparator,
            this._cancelBuildMenuItem});
            this._buildMenu.Name = "_buildMenu";
            this._buildMenu.Size = new System.Drawing.Size(46, 20);
            this._buildMenu.Text = "&Build";
            // 
            // _buildMenuItem
            // 
            this._buildMenuItem.Name = "_buildMenuItem";
            this._buildMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this._buildMenuItem.Size = new System.Drawing.Size(173, 22);
            this._buildMenuItem.Text = "&Build";
            this._buildMenuItem.Click += new System.EventHandler(this.BuildMenuItemClick);
            // 
            // _rebuildMenuItem
            // 
            this._rebuildMenuItem.Name = "_rebuildMenuItem";
            this._rebuildMenuItem.Size = new System.Drawing.Size(173, 22);
            this._rebuildMenuItem.Text = "&Rebuild";
            this._rebuildMenuItem.Click += new System.EventHandler(this.RebuildMenuItemClick);
            // 
            // _cleanMenuItem
            // 
            this._cleanMenuItem.Name = "_cleanMenuItem";
            this._cleanMenuItem.Size = new System.Drawing.Size(173, 22);
            this._cleanMenuItem.Text = "&Clean";
            this._cleanMenuItem.Click += new System.EventHandler(this.CleanMenuItemClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(170, 6);
            // 
            // _filterOutputMenuItem
            // 
            this._filterOutputMenuItem.Checked = true;
            this._filterOutputMenuItem.CheckOnClick = true;
            this._filterOutputMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._filterOutputMenuItem.Name = "_filterOutputMenuItem";
            this._filterOutputMenuItem.Size = new System.Drawing.Size(173, 22);
            this._filterOutputMenuItem.Text = "Filter Output";
            this._filterOutputMenuItem.CheckedChanged += new System.EventHandler(this.FilterOutputMenuItem_CheckedChanged);
            // 
            // _debuggerMenuItem
            // 
            this._debuggerMenuItem.CheckOnClick = true;
            this._debuggerMenuItem.Name = "_debuggerMenuItem";
            this._debuggerMenuItem.Size = new System.Drawing.Size(173, 22);
            this._debuggerMenuItem.Text = "Debug Mode";
            // 
            // _cancelBuildSeparator
            // 
            this._cancelBuildSeparator.Name = "_cancelBuildSeparator";
            this._cancelBuildSeparator.Size = new System.Drawing.Size(170, 6);
            this._cancelBuildSeparator.Visible = false;
            // 
            // _cancelBuildMenuItem
            // 
            this._cancelBuildMenuItem.Name = "_cancelBuildMenuItem";
            this._cancelBuildMenuItem.ShortcutKeyDisplayString = "Ctrl+Break";
            this._cancelBuildMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Pause)));
            this._cancelBuildMenuItem.Size = new System.Drawing.Size(173, 22);
            this._cancelBuildMenuItem.Text = "Cancel";
            this._cancelBuildMenuItem.Visible = false;
            this._cancelBuildMenuItem.Click += new System.EventHandler(this.CancelBuildMenuItemClick);
            // 
            // _helpMenu
            // 
            this._helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._viewHelpMenuItem,
            _toolStripSeparator2,
            this._aboutMenuItem});
            this._helpMenu.Name = "_helpMenu";
            this._helpMenu.Size = new System.Drawing.Size(44, 20);
            this._helpMenu.Text = "&Help";
            // 
            // _viewHelpMenuItem
            // 
            this._viewHelpMenuItem.Name = "_viewHelpMenuItem";
            this._viewHelpMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this._viewHelpMenuItem.Size = new System.Drawing.Size(146, 22);
            this._viewHelpMenuItem.Text = "&View Help";
            this._viewHelpMenuItem.Click += new System.EventHandler(this.ViewHelpMenuItemClick);
            // 
            // _aboutMenuItem
            // 
            this._aboutMenuItem.Name = "_aboutMenuItem";
            this._aboutMenuItem.Size = new System.Drawing.Size(146, 22);
            this._aboutMenuItem.Text = "&About...";
            this._aboutMenuItem.Click += new System.EventHandler(this.AboutMenuItemClick);
            // 
            // _treeContextMenu
            // 
            this._treeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._treeOpenFileMenuItem,
            this._treeAddMenu,
            this._treeSeparator1,
            this._treeOpenFileLocationMenuItem,
            this._treeRebuildMenuItem,
            this.toolStripSeparator4,
            this._treeRenameMenuItem,
            this._treeDeleteMenuItem});
            this._treeContextMenu.Name = "itemContextMenu";
            this._treeContextMenu.Size = new System.Drawing.Size(174, 148);
            this._treeContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.MainMenuMenuActivate);
            // 
            // _treeOpenFileMenuItem
            // 
            this._treeOpenFileMenuItem.Name = "_treeOpenFileMenuItem";
            this._treeOpenFileMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeOpenFileMenuItem.Text = "Open File";
            this._treeOpenFileMenuItem.Click += new System.EventHandler(this.ContextMenu_OpenFile_Click);
            // 
            // _treeAddMenu
            // 
            this._treeAddMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripSeparator7,
            this.toolStripMenuItem5,
            this.toolStripMenuItem6});
            this._treeAddMenu.Name = "_treeAddMenu";
            this._treeAddMenu.Size = new System.Drawing.Size(173, 22);
            this._treeAddMenu.Text = "Add";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(159, 22);
            this.toolStripMenuItem3.Text = "New Item...";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.OnNewItemClick);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(159, 22);
            this.toolStripMenuItem4.Text = "New Folder...";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.OnNewFolderClick);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(156, 6);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(159, 22);
            this.toolStripMenuItem5.Text = "Existing Item...";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.OnAddItemClick);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(159, 22);
            this.toolStripMenuItem6.Text = "Existing Folder...";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.OnAddFolderClick);
            // 
            // _treeSeparator1
            // 
            this._treeSeparator1.Name = "_treeSeparator1";
            this._treeSeparator1.Size = new System.Drawing.Size(170, 6);
            // 
            // _treeOpenFileLocationMenuItem
            // 
            this._treeOpenFileLocationMenuItem.Name = "_treeOpenFileLocationMenuItem";
            this._treeOpenFileLocationMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeOpenFileLocationMenuItem.Text = "Open File Location";
            this._treeOpenFileLocationMenuItem.Click += new System.EventHandler(this.ContextMenu_OpenFileLocation_Click);
            // 
            // _treeRebuildMenuItem
            // 
            this._treeRebuildMenuItem.Name = "_treeRebuildMenuItem";
            this._treeRebuildMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeRebuildMenuItem.Text = "Rebuild";
            this._treeRebuildMenuItem.Click += new System.EventHandler(this.RebuildItemsMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(170, 6);
            // 
            // _treeRenameMenuItem
            // 
            this._treeRenameMenuItem.Name = "_treeRenameMenuItem";
            this._treeRenameMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeRenameMenuItem.Text = "Rename";
            this._treeRenameMenuItem.Click += new System.EventHandler(this.OnRenameItemClick);
            // 
            // _treeDeleteMenuItem
            // 
            this._treeDeleteMenuItem.Name = "_treeDeleteMenuItem";
            this._treeDeleteMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this._treeDeleteMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeDeleteMenuItem.Text = "&Delete";
            this._treeDeleteMenuItem.Click += new System.EventHandler(this.OnDeleteItemClick);
            // 
            // _toolNew
            // 
            this._toolNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolNew.Image = ((System.Drawing.Image)(resources.GetObject("_toolNew.Image")));
            this._toolNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolNew.Name = "_toolNew";
            this._toolNew.Size = new System.Drawing.Size(23, 22);
            this._toolNew.Text = "New";
            this._toolNew.Click += new System.EventHandler(this.OnNewProjectClick);
            // 
            // _toolOpen
            // 
            this._toolOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolOpen.Image = ((System.Drawing.Image)(resources.GetObject("_toolOpen.Image")));
            this._toolOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolOpen.Name = "_toolOpen";
            this._toolOpen.Size = new System.Drawing.Size(23, 22);
            this._toolOpen.Text = "Open";
            this._toolOpen.Click += new System.EventHandler(this.OnOpenProjectClick);
            // 
            // _toolSave
            // 
            this._toolSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolSave.Image = ((System.Drawing.Image)(resources.GetObject("_toolSave.Image")));
            this._toolSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolSave.Name = "_toolSave";
            this._toolSave.Size = new System.Drawing.Size(23, 22);
            this._toolSave.Text = "Save";
            this._toolSave.Click += new System.EventHandler(this.OnSaveProjectClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // _toolNewItem
            // 
            this._toolNewItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolNewItem.Image = ((System.Drawing.Image)(resources.GetObject("_toolNewItem.Image")));
            this._toolNewItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolNewItem.Name = "_toolNewItem";
            this._toolNewItem.Size = new System.Drawing.Size(23, 22);
            this._toolNewItem.Text = "Add New Item";
            this._toolNewItem.Click += new System.EventHandler(this.OnNewItemClick);
            // 
            // _toolAddItem
            // 
            this._toolAddItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolAddItem.Image = ((System.Drawing.Image)(resources.GetObject("_toolAddItem.Image")));
            this._toolAddItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolAddItem.Name = "_toolAddItem";
            this._toolAddItem.Size = new System.Drawing.Size(23, 22);
            this._toolAddItem.Text = "Add Existing Item";
            this._toolAddItem.Click += new System.EventHandler(this.OnAddItemClick);
            // 
            // _toolNewFolder
            // 
            this._toolNewFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolNewFolder.Image = ((System.Drawing.Image)(resources.GetObject("_toolNewFolder.Image")));
            this._toolNewFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolNewFolder.Name = "_toolNewFolder";
            this._toolNewFolder.Size = new System.Drawing.Size(23, 22);
            this._toolNewFolder.Text = "Add New Folder";
            this._toolNewFolder.Click += new System.EventHandler(this.OnNewFolderClick);
            // 
            // _toolAddFolder
            // 
            this._toolAddFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolAddFolder.Image = ((System.Drawing.Image)(resources.GetObject("_toolAddFolder.Image")));
            this._toolAddFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolAddFolder.Name = "_toolAddFolder";
            this._toolAddFolder.Size = new System.Drawing.Size(23, 22);
            this._toolAddFolder.Text = "Add Existing Folder";
            this._toolAddFolder.Click += new System.EventHandler(this.OnAddFolderClick);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            // 
            // _toolBuild
            // 
            this._toolBuild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolBuild.Image = ((System.Drawing.Image)(resources.GetObject("_toolBuild.Image")));
            this._toolBuild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolBuild.Name = "_toolBuild";
            this._toolBuild.Size = new System.Drawing.Size(23, 22);
            this._toolBuild.Text = "Build";
            this._toolBuild.Click += new System.EventHandler(this.BuildMenuItemClick);
            // 
            // _toolRebuild
            // 
            this._toolRebuild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolRebuild.Image = ((System.Drawing.Image)(resources.GetObject("_toolRebuild.Image")));
            this._toolRebuild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolRebuild.Name = "_toolRebuild";
            this._toolRebuild.Size = new System.Drawing.Size(23, 22);
            this._toolRebuild.Text = "Rebuild";
            this._toolRebuild.Click += new System.EventHandler(this.RebuildMenuItemClick);
            // 
            // _toolClean
            // 
            this._toolClean.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolClean.Image = ((System.Drawing.Image)(resources.GetObject("_toolClean.Image")));
            this._toolClean.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolClean.Name = "_toolClean";
            this._toolClean.Size = new System.Drawing.Size(23, 22);
            this._toolClean.Text = "Clean";
            this._toolClean.Click += new System.EventHandler(this.CleanMenuItemClick);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 25);
            // 
            // _toolFilterOutput
            // 
            this._toolFilterOutput.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolFilterOutput.Image = ((System.Drawing.Image)(resources.GetObject("_toolFilterOutput.Image")));
            this._toolFilterOutput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolFilterOutput.Name = "_toolFilterOutput";
            this._toolFilterOutput.Size = new System.Drawing.Size(23, 22);
            this._toolFilterOutput.Text = "Filter Output";
            this._toolFilterOutput.Click += new System.EventHandler(this._toolFilterOutput_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolNew,
            this._toolOpen,
            this._toolSave,
            this.toolStripSeparator6,
            this._toolNewItem,
            this._toolAddItem,
            this._toolNewFolder,
            this._toolAddFolder,
            this.toolStripSeparator8,
            this._toolBuild,
            this._toolRebuild,
            this._toolClean,
            this.toolStripSeparator9,
            this._toolFilterOutput});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(4, 0, 1, 0);
            this.toolStrip1.Size = new System.Drawing.Size(784, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _splitEditorOutput
            // 
            this._splitEditorOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitEditorOutput.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._splitEditorOutput.Location = new System.Drawing.Point(0, 49);
            this._splitEditorOutput.Name = "_splitEditorOutput";
            // 
            // _splitEditorOutput.Panel1
            // 
            this._splitEditorOutput.Panel1.Controls.Add(this._splitTreeProps);
            // 
            // _splitEditorOutput.Panel2
            // 
            this._splitEditorOutput.Panel2.Controls.Add(this._outputTabs);
            this._splitEditorOutput.Size = new System.Drawing.Size(784, 512);
            this._splitEditorOutput.SplitterDistance = 249;
            this._splitEditorOutput.TabIndex = 4;
            this._splitEditorOutput.TabStop = false;
            // 
            // _splitTreeProps
            // 
            this._splitTreeProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitTreeProps.Location = new System.Drawing.Point(0, 0);
            this._splitTreeProps.Name = "_splitTreeProps";
            this._splitTreeProps.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _splitTreeProps.Panel1
            // 
            this._splitTreeProps.Panel1.Controls.Add(this._treeView);
            // 
            // _splitTreeProps.Panel2
            // 
            this._splitTreeProps.Panel2.Controls.Add(this._propertyGrid);
            this._splitTreeProps.Size = new System.Drawing.Size(249, 512);
            this._splitTreeProps.SplitterDistance = 200;
            this._splitTreeProps.TabIndex = 1;
            this._splitTreeProps.TabStop = false;
            // 
            // _propertyGrid
            // 
            this._propertyGrid.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this._propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._propertyGrid.Location = new System.Drawing.Point(0, 0);
            this._propertyGrid.Name = "_propertyGrid";
            this._propertyGrid.Size = new System.Drawing.Size(249, 308);
            this._propertyGrid.TabIndex = 0;
            // 
            // _treeView
            // 
            this._treeView.AllowDrop = true;
            this._treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._treeView.DragOverNodeBackColor = System.Drawing.SystemColors.Highlight;
            this._treeView.DragOverNodeForeColor = System.Drawing.SystemColors.HighlightText;
            this._treeView.ItemHeight = 18;
            this._treeView.Location = new System.Drawing.Point(0, 0);
            this._treeView.Name = "_treeView";
            this._treeView.Size = new System.Drawing.Size(249, 200);
            this._treeView.TabIndex = 0;
            this._treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewAfterSelect);
            this._treeView.DragDrop += new System.Windows.Forms.DragEventHandler(this._treeView_DragDrop);
            this._treeView.DragOver += new System.Windows.Forms.DragEventHandler(this._treeView_DragOver);
            this._treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeViewOnKeyDown);
            this._treeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeViewMouseUp);
            // 
            // _outputTabs
            // 
            this._outputTabs.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this._outputTabs.Controls.Add(this._outputTabPage1);
            this._outputTabs.Controls.Add(this._outputTabPage2);
            this._outputTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._outputTabs.HideTabHeader = true;
            this._outputTabs.Location = new System.Drawing.Point(0, 0);
            this._outputTabs.Margin = new System.Windows.Forms.Padding(0);
            this._outputTabs.Name = "_outputTabs";
            this._outputTabs.Padding = new System.Drawing.Point(0, 0);
            this._outputTabs.SelectedIndex = 0;
            this._outputTabs.Size = new System.Drawing.Size(531, 512);
            this._outputTabs.TabIndex = 0;
            this._outputTabs.TabStop = false;
            // 
            // _outputTabPage1
            // 
            this._outputTabPage1.Controls.Add(this._outputWindow);
            this._outputTabPage1.Location = new System.Drawing.Point(4, 25);
            this._outputTabPage1.Margin = new System.Windows.Forms.Padding(0);
            this._outputTabPage1.Name = "_outputTabPage1";
            this._outputTabPage1.Size = new System.Drawing.Size(523, 483);
            this._outputTabPage1.TabIndex = 0;
            this._outputTabPage1.Text = "_outputTabPage1";
            this._outputTabPage1.UseVisualStyleBackColor = true;
            // 
            // _outputWindow
            // 
            this._outputWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._outputWindow.HideSelection = false;
            this._outputWindow.Location = new System.Drawing.Point(0, 0);
            this._outputWindow.Name = "_outputWindow";
            this._outputWindow.ReadOnly = true;
            this._outputWindow.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this._outputWindow.Size = new System.Drawing.Size(523, 483);
            this._outputWindow.TabIndex = 0;
            this._outputWindow.TabStop = false;
            this._outputWindow.Text = "";
            // 
            // _outputTabPage2
            // 
            this._outputTabPage2.Controls.Add(this._filterOutputWindow);
            this._outputTabPage2.Location = new System.Drawing.Point(4, 25);
            this._outputTabPage2.Margin = new System.Windows.Forms.Padding(0);
            this._outputTabPage2.Name = "_outputTabPage2";
            this._outputTabPage2.Size = new System.Drawing.Size(523, 483);
            this._outputTabPage2.TabIndex = 0;
            this._outputTabPage2.Text = "_outputTabPage2";
            this._outputTabPage2.UseVisualStyleBackColor = true;
            // 
            // _filterOutputWindow
            // 
            this._filterOutputWindow.BackColor = System.Drawing.SystemColors.Control;
            this._filterOutputWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._filterOutputWindow.FullRowSelect = true;
            this._filterOutputWindow.ImageIndex = 0;
            this._filterOutputWindow.ItemHeight = 20;
            this._filterOutputWindow.Location = new System.Drawing.Point(0, 0);
            this._filterOutputWindow.Name = "_filterOutputWindow";
            this._filterOutputWindow.SelectedImageIndex = 0;
            this._filterOutputWindow.ShowLines = false;
            this._filterOutputWindow.ShowNodeToolTips = true;
            this._filterOutputWindow.Size = new System.Drawing.Size(523, 483);
            this._filterOutputWindow.TabIndex = 0;
            // 
            // MainView
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this._splitEditorOutput);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this._mainMenu);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this._mainMenu;
            this.MinimumSize = new System.Drawing.Size(320, 240);
            this.Name = "MainView";
            this.Text = "MonoGame Pipeline";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainView_FormClosing);
            this.Load += new System.EventHandler(this.MainView_Load);
            this.Shown += new System.EventHandler(this.MainView_Shown);
            this.SizeChanged += new System.EventHandler(this.MainView_SizeChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainView_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainView_DragEnter);
            this._mainMenu.ResumeLayout(false);
            this._mainMenu.PerformLayout();
            this._treeContextMenu.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this._splitEditorOutput.Panel1.ResumeLayout(false);
            this._splitEditorOutput.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitEditorOutput)).EndInit();
            this._splitEditorOutput.ResumeLayout(false);
            this._splitTreeProps.Panel1.ResumeLayout(false);
            this._splitTreeProps.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitTreeProps)).EndInit();
            this._splitTreeProps.ResumeLayout(false);
            this._outputTabs.ResumeLayout(false);
            this._outputTabPage1.ResumeLayout(false);
            this._outputTabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _mainMenu;
        private System.Windows.Forms.ToolStripMenuItem _fileMenu;
        private System.Windows.Forms.ToolStripMenuItem _newProjectMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _openProjectMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _buildMenu;
        private System.Windows.Forms.ToolStripMenuItem _buildMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _rebuildMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _cleanMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpMenu;
        private System.Windows.Forms.ToolStripMenuItem _viewHelpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _aboutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveAsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _closeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _importProjectMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _undoMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _redoMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _deleteMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _cancelBuildMenuItem;
        private System.Windows.Forms.ToolStripSeparator _cancelBuildSeparator;
        private System.Windows.Forms.ContextMenuStrip _treeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem _treeDeleteMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem _treeRebuildMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem _filterOutputMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _debuggerMenuItem;
        private System.Windows.Forms.ToolStripSeparator _treeSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _treeOpenFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _treeOpenFileLocationMenuItem;
        private ToolStripMenuItem _openRecentMenuItem;
        private ToolStripMenuItem _addMenuItem;
        private ToolStripMenuItem newItemToolStripMenuItem;
        private ToolStripMenuItem newFolderToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem existingItemToolStripMenuItem;
        private ToolStripMenuItem existingFolderToolStripMenuItem;
        private ToolStripMenuItem _treeAddMenu;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripMenuItem toolStripMenuItem6;
        private ToolStripMenuItem _renameMenuItem;
        private ToolStripMenuItem _treeRenameMenuItem;
        private ToolStripButton _toolNew;
        private ToolStripButton _toolOpen;
        private ToolStripButton _toolSave;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripButton _toolNewItem;
        private ToolStripButton _toolAddItem;
        private ToolStripButton _toolNewFolder;
        private ToolStripButton _toolAddFolder;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripButton _toolBuild;
        private ToolStripButton _toolRebuild;
        private ToolStripButton _toolClean;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripButton _toolFilterOutput;
        private ToolStrip toolStrip1;
        private SplitContainer _splitEditorOutput;
        private SplitContainer _splitTreeProps;
        private MultiSelectTreeview _treeView;
        private PropertyGrid _propertyGrid;
        private Windows.Controls.TabControlEx _outputTabs;
        private TabPage _outputTabPage1;
        private RichTextBox _outputWindow;
        private TabPage _outputTabPage2;
        private Windows.Controls.FilterOutputControl _filterOutputWindow;
    }
}

