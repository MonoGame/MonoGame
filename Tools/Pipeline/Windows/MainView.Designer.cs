// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
            System.Windows.Forms.ToolStripSeparator _toolStripSeparator3;
            System.Windows.Forms.ToolStripSeparator _toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator _toolStripSeparator2;
            System.Windows.Forms.SplitContainer _splitTreeProps;
            System.Windows.Forms.SplitContainer _splitEditorOutput;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            this._treeView = new System.Windows.Forms.TreeView();
            this._propertyGrid = new System.Windows.Forms.PropertyGrid();
            this._outputWindow = new System.Windows.Forms.TextBox();
            this._mainMenu = new System.Windows.Forms.MenuStrip();
            this._fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._newMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._closeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._importMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._newItemMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addItemMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._deleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._buildMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._buildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._rebuilMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._cleanMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._cancelBuildSeparator = new System.Windows.Forms.ToolStripSeparator();
            this._cancelBuildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._viewHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this._outputWindow.Multiline = true;
            this._outputWindow.Name = "_outputWindow";
            this._outputWindow.ReadOnly = true;
            this._outputWindow.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._outputWindow.Size = new System.Drawing.Size(531, 537);
            this._outputWindow.TabIndex = 0;
            this._outputWindow.WordWrap = false;
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
            this._mainMenu.MenuActivate += new System.EventHandler(this._mainMenu_MenuActivate);
            // 
            // _fileMenu
            // 
            this._fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newMenuItem,
            this._openMenuItem,
            this._closeMenuItem,
            this.toolStripSeparator3,
            this._importMenuItem,
            _toolStripSeparator3,
            this._saveMenuItem,
            this._saveAsMenuItem,
            _toolStripSeparator1,
            this._exitMenuItem});
            this._fileMenu.Name = "_fileMenu";
            this._fileMenu.Size = new System.Drawing.Size(37, 20);
            this._fileMenu.Text = "&File";
            // 
            // _newMenuItem
            // 
            this._newMenuItem.Name = "_newMenuItem";
            this._newMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.N)));
            this._newMenuItem.Size = new System.Drawing.Size(182, 22);
            this._newMenuItem.Text = "New...";
            this._newMenuItem.Click += new System.EventHandler(this.NewMenuItemClick);
            // 
            // _openMenuItem
            // 
            this._openMenuItem.Name = "_openMenuItem";
            this._openMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this._openMenuItem.Size = new System.Drawing.Size(182, 22);
            this._openMenuItem.Text = "Open...";
            this._openMenuItem.Click += new System.EventHandler(this.OpenMenuItemClick);
            // 
            // _closeMenuItem
            // 
            this._closeMenuItem.Name = "_closeMenuItem";
            this._closeMenuItem.Size = new System.Drawing.Size(182, 22);
            this._closeMenuItem.Text = "Close";
            this._closeMenuItem.Click += new System.EventHandler(this.CloseMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(179, 6);
            // 
            // _importMenuItem
            // 
            this._importMenuItem.Name = "_importMenuItem";
            this._importMenuItem.Size = new System.Drawing.Size(182, 22);
            this._importMenuItem.Text = "Import...";
            this._importMenuItem.Click += new System.EventHandler(this.ImportMenuItem_Click);
            // 
            // _saveMenuItem
            // 
            this._saveMenuItem.Name = "_saveMenuItem";
            this._saveMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this._saveMenuItem.Size = new System.Drawing.Size(182, 22);
            this._saveMenuItem.Text = "&Save";
            this._saveMenuItem.Click += new System.EventHandler(this.SaveMenuItemClick);
            // 
            // _saveAsMenuItem
            // 
            this._saveAsMenuItem.Name = "_saveAsMenuItem";
            this._saveAsMenuItem.Size = new System.Drawing.Size(182, 22);
            this._saveAsMenuItem.Text = "Save &As...";
            this._saveAsMenuItem.Click += new System.EventHandler(this.SaveAsMenuItemClick);
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
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator2,
            this._newItemMenuItem,
            this._addItemMenuItem,
            this.toolStripSeparator1,
            this._deleteMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Enabled = false;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Enabled = false;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(131, 6);
            // 
            // _newItemMenuItem
            // 
            this._newItemMenuItem.Name = "_newItemMenuItem";
            this._newItemMenuItem.Size = new System.Drawing.Size(134, 22);
            this._newItemMenuItem.Text = "&New Item...";
            // 
            // _addItemMenuItem
            // 
            this._addItemMenuItem.Name = "_addItemMenuItem";
            this._addItemMenuItem.Size = new System.Drawing.Size(134, 22);
            this._addItemMenuItem.Text = "&Add Item...";
            this._addItemMenuItem.Click += new System.EventHandler(this.AddMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(131, 6);
            // 
            // _deleteMenuItem
            // 
            this._deleteMenuItem.Name = "_deleteMenuItem";
            this._deleteMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this._deleteMenuItem.Size = new System.Drawing.Size(134, 22);
            this._deleteMenuItem.Text = "&Delete";
            this._deleteMenuItem.Click += new System.EventHandler(this.DeleteMenuItem_Click);
            // 
            // _buildMenu
            // 
            this._buildMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._buildMenuItem,
            this._rebuilMenuItem,
            this._cleanMenuItem,
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
            // _rebuilMenuItem
            // 
            this._rebuilMenuItem.Name = "_rebuilMenuItem";
            this._rebuilMenuItem.Size = new System.Drawing.Size(173, 22);
            this._rebuilMenuItem.Text = "&Rebuild";
            this._rebuilMenuItem.Click += new System.EventHandler(this.RebuilMenuItemClick);
            // 
            // _cleanMenuItem
            // 
            this._cleanMenuItem.Name = "_cleanMenuItem";
            this._cleanMenuItem.Size = new System.Drawing.Size(173, 22);
            this._cleanMenuItem.Text = "&Clean";
            this._cleanMenuItem.Click += new System.EventHandler(this.CleanMenuItemClick);
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
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(_splitEditorOutput);
            this.Controls.Add(this._mainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this._mainMenu;
            this.MinimumSize = new System.Drawing.Size(320, 240);
            this.Name = "MainView";
            this.Text = "MonoGame Pipeline";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainView_FormClosing);
            _splitTreeProps.Panel1.ResumeLayout(false);
            _splitTreeProps.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(_splitTreeProps)).EndInit();
            _splitTreeProps.ResumeLayout(false);
            _splitEditorOutput.Panel1.ResumeLayout(false);
            _splitEditorOutput.Panel2.ResumeLayout(false);
            _splitEditorOutput.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(_splitEditorOutput)).EndInit();
            _splitEditorOutput.ResumeLayout(false);
            this._mainMenu.ResumeLayout(false);
            this._mainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _mainMenu;
        private System.Windows.Forms.ToolStripMenuItem _fileMenu;
        private System.Windows.Forms.TreeView _treeView;
        private System.Windows.Forms.PropertyGrid _propertyGrid;
        private System.Windows.Forms.ToolStripMenuItem _newMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _openMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _buildMenu;
        private System.Windows.Forms.ToolStripMenuItem _buildMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _rebuilMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _cleanMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpMenu;
        private System.Windows.Forms.ToolStripMenuItem _viewHelpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _aboutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveAsMenuItem;
        private System.Windows.Forms.TextBox _outputWindow;
        private System.Windows.Forms.ToolStripMenuItem _closeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _importMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _deleteMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem _newItemMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _addItemMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _cancelBuildMenuItem;
        private System.Windows.Forms.ToolStripSeparator _cancelBuildSeparator;
    }
}

