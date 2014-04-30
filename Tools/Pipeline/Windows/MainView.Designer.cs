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
            this._treeView = new System.Windows.Forms.TreeView();
            this._propertyGrid = new System.Windows.Forms.PropertyGrid();
            this._outputWindow = new System.Windows.Forms.TextBox();
            this._mainMenu = new System.Windows.Forms.MenuStrip();
            this._fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._newMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._buildMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._buildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._rebuilMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._cleanMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._viewHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            _toolStripSeparator3.Size = new System.Drawing.Size(149, 6);
            // 
            // _toolStripSeparator1
            // 
            _toolStripSeparator1.Name = "_toolStripSeparator1";
            _toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
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
            this._buildMenu,
            this._helpMenu});
            this._mainMenu.Location = new System.Drawing.Point(0, 0);
            this._mainMenu.Name = "_mainMenu";
            this._mainMenu.Size = new System.Drawing.Size(784, 24);
            this._mainMenu.TabIndex = 0;
            this._mainMenu.Text = "menuStrip1";
            // 
            // _fileMenu
            // 
            this._fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newMenuItem,
            this._openMenuItem,
            this.closeToolStripMenuItem,
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
            this._newMenuItem.Size = new System.Drawing.Size(152, 22);
            this._newMenuItem.Text = "New...";
            this._newMenuItem.Click += new System.EventHandler(this.NewMenuItemClick);
            // 
            // _openMenuItem
            // 
            this._openMenuItem.Name = "_openMenuItem";
            this._openMenuItem.Size = new System.Drawing.Size(152, 22);
            this._openMenuItem.Text = "Open...";
            this._openMenuItem.Click += new System.EventHandler(this.OpenMenuItemClick);
            // 
            // _saveMenuItem
            // 
            this._saveMenuItem.Name = "_saveMenuItem";
            this._saveMenuItem.Size = new System.Drawing.Size(152, 22);
            this._saveMenuItem.Text = "&Save";
            this._saveMenuItem.Click += new System.EventHandler(this.SaveMenuItemClick);
            // 
            // _saveAsMenuItem
            // 
            this._saveAsMenuItem.Name = "_saveAsMenuItem";
            this._saveAsMenuItem.Size = new System.Drawing.Size(152, 22);
            this._saveAsMenuItem.Text = "Save &As";
            this._saveAsMenuItem.Click += new System.EventHandler(this.SaveAsMenuItemClick);
            // 
            // _exitMenuItem
            // 
            this._exitMenuItem.Name = "_exitMenuItem";
            this._exitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this._exitMenuItem.Size = new System.Drawing.Size(152, 22);
            this._exitMenuItem.Text = "E&xit";
            this._exitMenuItem.Click += new System.EventHandler(this.ExitMenuItemClick);
            // 
            // _buildMenu
            // 
            this._buildMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._buildMenuItem,
            this._rebuilMenuItem,
            this._cleanMenuItem});
            this._buildMenu.Name = "_buildMenu";
            this._buildMenu.Size = new System.Drawing.Size(46, 20);
            this._buildMenu.Text = "&Build";
            // 
            // _buildMenuItem
            // 
            this._buildMenuItem.Name = "_buildMenuItem";
            this._buildMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this._buildMenuItem.Size = new System.Drawing.Size(120, 22);
            this._buildMenuItem.Text = "&Build";
            this._buildMenuItem.Click += new System.EventHandler(this.BuildMenuItemClick);
            // 
            // _rebuilMenuItem
            // 
            this._rebuilMenuItem.Name = "_rebuilMenuItem";
            this._rebuilMenuItem.Size = new System.Drawing.Size(120, 22);
            this._rebuilMenuItem.Text = "&Rebuild";
            this._rebuilMenuItem.Click += new System.EventHandler(this.RebuilMenuItemClick);
            // 
            // _cleanMenuItem
            // 
            this._cleanMenuItem.Name = "_cleanMenuItem";
            this._cleanMenuItem.Size = new System.Drawing.Size(120, 22);
            this._cleanMenuItem.Text = "&Clean";
            this._cleanMenuItem.Click += new System.EventHandler(this.CleanMenuItemClick);
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
            // 
            // _aboutMenuItem
            // 
            this._aboutMenuItem.Name = "_aboutMenuItem";
            this._aboutMenuItem.Size = new System.Drawing.Size(146, 22);
            this._aboutMenuItem.Text = "&About...";
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.CloseMenuItem_Click);
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(_splitEditorOutput);
            this.Controls.Add(this._mainMenu);
            this.MainMenuStrip = this._mainMenu;
            this.MaximizeBox = false;
            this.Name = "MainView";
            this.Text = "Pipeline";
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
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
    }
}

