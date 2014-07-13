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
            System.Windows.Forms.SplitContainer _splitTreeProps;
            System.Windows.Forms.SplitContainer _splitEditorOutput;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            this._treeView = new MonoGame.Tools.Pipeline.MultiSelectTreeview();
            this._propertyGrid = new System.Windows.Forms.PropertyGrid();
            this._outputWindow = new System.Windows.Forms.RichTextBox();
            this._mainMenu = new System.Windows.Forms.MenuStrip();
            this._fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._newProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this._newItemMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addItemMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._deleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._buildMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._buildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._rebuildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._cleanMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this._debuggerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._cancelBuildSeparator = new System.Windows.Forms.ToolStripSeparator();
            this._cancelBuildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._viewHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._treeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._treeNewItemMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._treeAddItemMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._treeDeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this._treeOpenFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._treeOpenFileLocationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this._treeRebuildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            _toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            _toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            _toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            _splitTreeProps = new System.Windows.Forms.SplitContainer();
            _splitEditorOutput = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(_splitTreeProps)).BeginInit();
            _splitTreeProps.Panel1.SuspendLayout();
            _splitTreeProps.Panel2.SuspendLayout();
            _splitTreeProps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(_splitEditorOutput)).BeginInit();
            _splitEditorOutput.Panel1.SuspendLayout();
            _splitEditorOutput.Panel2.SuspendLayout();
            _splitEditorOutput.SuspendLayout();
            this._mainMenu.SuspendLayout();
            this._treeContextMenu.SuspendLayout();
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
            // _splitTreeProps
            // 
            _splitTreeProps.Dock = System.Windows.Forms.DockStyle.Fill;
            _splitTreeProps.Location = new System.Drawing.Point(0, 0);
            _splitTreeProps.Name = "_splitTreeProps";
            _splitTreeProps.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _splitTreeProps.Panel1
            // 
            _splitTreeProps.Panel1.Controls.Add(this._treeView);
            // 
            // _splitTreeProps.Panel2
            // 
            _splitTreeProps.Panel2.Controls.Add(this._propertyGrid);
            _splitTreeProps.Size = new System.Drawing.Size(249, 537);
            _splitTreeProps.SplitterDistance = 210;
            _splitTreeProps.TabIndex = 1;
            _splitTreeProps.TabStop = false;
            // 
            // _treeView
            // 
            this._treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._treeView.Location = new System.Drawing.Point(0, 0);
            this._treeView.Name = "_treeView";
            this._treeView.Size = new System.Drawing.Size(249, 210);
            this._treeView.TabIndex = 0;
            this._treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewAfterSelect);
            this._treeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeViewMouseUp);
            // 
            // _propertyGrid
            // 
            this._propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._propertyGrid.Location = new System.Drawing.Point(0, 0);
            this._propertyGrid.Name = "_propertyGrid";
            this._propertyGrid.Size = new System.Drawing.Size(249, 323);
            this._propertyGrid.TabIndex = 0;
            // 
            // _splitEditorOutput
            // 
            _splitEditorOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            _splitEditorOutput.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            _splitEditorOutput.Location = new System.Drawing.Point(0, 24);
            _splitEditorOutput.Name = "_splitEditorOutput";
            // 
            // _splitEditorOutput.Panel1
            // 
            _splitEditorOutput.Panel1.Controls.Add(_splitTreeProps);
            // 
            // _splitEditorOutput.Panel2
            // 
            _splitEditorOutput.Panel2.Controls.Add(this._outputWindow);
            _splitEditorOutput.Size = new System.Drawing.Size(784, 537);
            _splitEditorOutput.SplitterDistance = 249;
            _splitEditorOutput.TabIndex = 2;
            _splitEditorOutput.TabStop = false;
            // 
            // _outputWindow
            // 
            this._outputWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._outputWindow.HideSelection = false;
            this._outputWindow.Location = new System.Drawing.Point(0, 0);
            this._outputWindow.Name = "_outputWindow";
            this._outputWindow.ReadOnly = true;
            this._outputWindow.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this._outputWindow.Size = new System.Drawing.Size(531, 537);
            this._outputWindow.TabIndex = 0;
            this._outputWindow.TabStop = false;
            this._outputWindow.Text = "";
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
            this._newItemMenuItem,
            this._addItemMenuItem,
            this.toolStripSeparator1,
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
            // _newItemMenuItem
            // 
            this._newItemMenuItem.Name = "_newItemMenuItem";
            this._newItemMenuItem.Size = new System.Drawing.Size(144, 22);
            this._newItemMenuItem.Text = "&New Item...";
            this._newItemMenuItem.Click += new System.EventHandler(this.OnNewItemClick);
            // 
            // _addItemMenuItem
            // 
            this._addItemMenuItem.Name = "_addItemMenuItem";
            this._addItemMenuItem.Size = new System.Drawing.Size(144, 22);
            this._addItemMenuItem.Text = "&Add Item...";
            this._addItemMenuItem.Click += new System.EventHandler(this.OnAddItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(141, 6);
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
            this._treeNewItemMenuItem,
            this._treeAddItemMenuItem,
            this._treeDeleteMenuItem,
            this.toolStripSeparator6,
            this._treeOpenFileMenuItem,
            this._treeOpenFileLocationMenuItem,
            this.toolStripSeparator4,
            this._treeRebuildMenuItem});
            this._treeContextMenu.Name = "itemContextMenu";
            this._treeContextMenu.Size = new System.Drawing.Size(174, 148);
            this._treeContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.MainMenuMenuActivate);
            // 
            // _treeNewItemMenuItem
            // 
            this._treeNewItemMenuItem.Name = "_treeNewItemMenuItem";
            this._treeNewItemMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeNewItemMenuItem.Text = "New Item...";
            this._treeNewItemMenuItem.Click += new System.EventHandler(this.OnNewItemClick);
            // 
            // _treeAddItemMenuItem
            // 
            this._treeAddItemMenuItem.Name = "_treeAddItemMenuItem";
            this._treeAddItemMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeAddItemMenuItem.Text = "&Add Item...";
            this._treeAddItemMenuItem.Click += new System.EventHandler(this.OnAddItemClick);
            // 
            // _treeDeleteMenuItem
            // 
            this._treeDeleteMenuItem.Name = "_treeDeleteMenuItem";
            this._treeDeleteMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this._treeDeleteMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeDeleteMenuItem.Text = "&Delete";
            this._treeDeleteMenuItem.Click += new System.EventHandler(this.OnDeleteItemClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(170, 6);
            // 
            // _treeOpenFileMenuItem
            // 
            this._treeOpenFileMenuItem.Name = "_treeOpenFileMenuItem";
            this._treeOpenFileMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeOpenFileMenuItem.Text = "Open File";
            this._treeOpenFileMenuItem.Click += new System.EventHandler(this.ContextMenu_OpenFile_Click);
            // 
            // _treeOpenFileLocationMenuItem
            // 
            this._treeOpenFileLocationMenuItem.Name = "_treeOpenFileLocationMenuItem";
            this._treeOpenFileLocationMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeOpenFileLocationMenuItem.Text = "Open File Location";
            this._treeOpenFileLocationMenuItem.Click += new System.EventHandler(this.ContextMenu_OpenFileLocation_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(170, 6);
            // 
            // _treeRebuildMenuItem
            // 
            this._treeRebuildMenuItem.Name = "_treeRebuildMenuItem";
            this._treeRebuildMenuItem.Size = new System.Drawing.Size(173, 22);
            this._treeRebuildMenuItem.Text = "Rebuild";
            this._treeRebuildMenuItem.Click += new System.EventHandler(this.RebuildItemsMenuItemClick);
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(_splitEditorOutput);
            this.Controls.Add(this._mainMenu);
            this.MainMenuStrip = this._mainMenu;
            this.MinimumSize = new System.Drawing.Size(320, 240);
            this.Name = "MainView";
            this.Text = "MonoGame Pipeline";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainView_FormClosing);
            this.Load += new System.EventHandler(this.MainView_Load);
            this.Shown += new System.EventHandler(this.MainView_Shown);
            _splitTreeProps.Panel1.ResumeLayout(false);
            _splitTreeProps.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(_splitTreeProps)).EndInit();
            _splitTreeProps.ResumeLayout(false);
            _splitEditorOutput.Panel1.ResumeLayout(false);
            _splitEditorOutput.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(_splitEditorOutput)).EndInit();
            _splitEditorOutput.ResumeLayout(false);
            this._mainMenu.ResumeLayout(false);
            this._mainMenu.PerformLayout();
            this._treeContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _mainMenu;
        private System.Windows.Forms.ToolStripMenuItem _fileMenu;
        private MultiSelectTreeview _treeView;
        private System.Windows.Forms.PropertyGrid _propertyGrid;
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
        private System.Windows.Forms.RichTextBox _outputWindow;
        private System.Windows.Forms.ToolStripMenuItem _closeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _importProjectMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _undoMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _redoMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _deleteMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem _newItemMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _addItemMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _cancelBuildMenuItem;
        private System.Windows.Forms.ToolStripSeparator _cancelBuildSeparator;
        private System.Windows.Forms.ContextMenuStrip _treeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem _treeDeleteMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem _treeRebuildMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem _debuggerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _treeAddItemMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _treeNewItemMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem _treeOpenFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _treeOpenFileLocationMenuItem;
    }
}

