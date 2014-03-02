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
            this._mainMenu = new System.Windows.Forms.MenuStrip();
            this._fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._newMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._saveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._buildMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._buildMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._rebuilMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._cleanMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._viewHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._treeView = new System.Windows.Forms.TreeView();
            this._propertyGrid = new System.Windows.Forms.PropertyGrid();
            this._mainMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // _mainMenu
            // 
            this._mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileMenu,
            this._buildMenu,
            this._helpMenu});
            this._mainMenu.Location = new System.Drawing.Point(0, 0);
            this._mainMenu.Name = "_mainMenu";
            this._mainMenu.Size = new System.Drawing.Size(273, 24);
            this._mainMenu.TabIndex = 0;
            this._mainMenu.Text = "menuStrip1";
            // 
            // _fileMenu
            // 
            this._fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newMenuItem,
            this._openMenuItem,
            this._toolStripSeparator3,
            this._saveMenuItem,
            this._saveAsMenuItem,
            this._toolStripSeparator1,
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
            // 
            // _toolStripSeparator3
            // 
            this._toolStripSeparator3.Name = "_toolStripSeparator3";
            this._toolStripSeparator3.Size = new System.Drawing.Size(149, 6);
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
            // _toolStripSeparator1
            // 
            this._toolStripSeparator1.Name = "_toolStripSeparator1";
            this._toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
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
            // 
            // _rebuilMenuItem
            // 
            this._rebuilMenuItem.Name = "_rebuilMenuItem";
            this._rebuilMenuItem.Size = new System.Drawing.Size(120, 22);
            this._rebuilMenuItem.Text = "&Rebuild";
            // 
            // _cleanMenuItem
            // 
            this._cleanMenuItem.Name = "_cleanMenuItem";
            this._cleanMenuItem.Size = new System.Drawing.Size(120, 22);
            this._cleanMenuItem.Text = "&Clean";
            // 
            // _helpMenu
            // 
            this._helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._viewHelpMenuItem,
            this._toolStripSeparator2,
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
            // _toolStripSeparator2
            // 
            this._toolStripSeparator2.Name = "_toolStripSeparator2";
            this._toolStripSeparator2.Size = new System.Drawing.Size(143, 6);
            // 
            // _aboutMenuItem
            // 
            this._aboutMenuItem.Name = "_aboutMenuItem";
            this._aboutMenuItem.Size = new System.Drawing.Size(146, 22);
            this._aboutMenuItem.Text = "&About...";
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.Location = new System.Drawing.Point(0, 24);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this._treeView);
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._propertyGrid);
            this._splitContainer.Size = new System.Drawing.Size(273, 325);
            this._splitContainer.SplitterDistance = 128;
            this._splitContainer.TabIndex = 1;
            // 
            // _treeView
            // 
            this._treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._treeView.Location = new System.Drawing.Point(0, 0);
            this._treeView.Name = "_treeView";
            this._treeView.Size = new System.Drawing.Size(273, 128);
            this._treeView.TabIndex = 0;
            // 
            // _propertyGrid
            // 
            this._propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._propertyGrid.Location = new System.Drawing.Point(0, 0);
            this._propertyGrid.Name = "_propertyGrid";
            this._propertyGrid.Size = new System.Drawing.Size(273, 193);
            this._propertyGrid.TabIndex = 0;
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(273, 349);
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._mainMenu);
            this.MainMenuStrip = this._mainMenu;
            this.MaximizeBox = false;
            this.Name = "MainView";
            this.Text = "Pipeline";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainView_FormClosing);
            this._mainMenu.ResumeLayout(false);
            this._mainMenu.PerformLayout();
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _mainMenu;
        private System.Windows.Forms.ToolStripMenuItem _fileMenu;
        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.TreeView _treeView;
        private System.Windows.Forms.PropertyGrid _propertyGrid;
        private System.Windows.Forms.ToolStripMenuItem _newMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _openMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _buildMenu;
        private System.Windows.Forms.ToolStripMenuItem _buildMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _rebuilMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _cleanMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpMenu;
        private System.Windows.Forms.ToolStripMenuItem _viewHelpMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _aboutMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem _saveAsMenuItem;
    }
}

