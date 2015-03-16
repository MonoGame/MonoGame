namespace MonoGame.Tools.Pipeline
{
    partial class NewContentDialog
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._buttonPanel = new System.Windows.Forms.Panel();
            this._cancelBtn = new System.Windows.Forms.Button();
            this._okBtn = new System.Windows.Forms.Button();
            this._namePanel = new System.Windows.Forms.Panel();
            this._namePanelTable = new System.Windows.Forms.TableLayoutPanel();
            this._name = new System.Windows.Forms.TextBox();
            this._namePanelLabel = new System.Windows.Forms.Label();
            this._listView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1.SuspendLayout();
            this._buttonPanel.SuspendLayout();
            this._namePanel.SuspendLayout();
            this._namePanelTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this._buttonPanel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this._namePanel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this._listView, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(283, 323);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // _buttonPanel
            // 
            this._buttonPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonPanel.BackColor = System.Drawing.SystemColors.Control;
            this._buttonPanel.Controls.Add(this._cancelBtn);
            this._buttonPanel.Controls.Add(this._okBtn);
            this._buttonPanel.Location = new System.Drawing.Point(0, 293);
            this._buttonPanel.Margin = new System.Windows.Forms.Padding(0);
            this._buttonPanel.Name = "_buttonPanel";
            this._buttonPanel.Size = new System.Drawing.Size(283, 30);
            this._buttonPanel.TabIndex = 1;
            // 
            // _cancelBtn
            // 
            this._cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelBtn.Location = new System.Drawing.Point(199, 3);
            this._cancelBtn.Name = "_cancelBtn";
            this._cancelBtn.Size = new System.Drawing.Size(75, 23);
            this._cancelBtn.TabIndex = 1;
            this._cancelBtn.Text = "Cancel";
            this._cancelBtn.UseVisualStyleBackColor = true;
            // 
            // _okBtn
            // 
            this._okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okBtn.Enabled = false;
            this._okBtn.Location = new System.Drawing.Point(118, 3);
            this._okBtn.Name = "_okBtn";
            this._okBtn.Size = new System.Drawing.Size(75, 23);
            this._okBtn.TabIndex = 0;
            this._okBtn.Text = "Ok";
            this._okBtn.UseVisualStyleBackColor = true;
            // 
            // _namePanel
            // 
            this._namePanel.AutoSize = true;
            this._namePanel.Controls.Add(this._namePanelTable);
            this._namePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._namePanel.Location = new System.Drawing.Point(3, 264);
            this._namePanel.Name = "_namePanel";
            this._namePanel.Size = new System.Drawing.Size(277, 26);
            this._namePanel.TabIndex = 2;
            // 
            // _namePanelTable
            // 
            this._namePanelTable.AutoSize = true;
            this._namePanelTable.ColumnCount = 2;
            this._namePanelTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._namePanelTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._namePanelTable.Controls.Add(this._name, 1, 0);
            this._namePanelTable.Controls.Add(this._namePanelLabel, 0, 0);
            this._namePanelTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this._namePanelTable.Location = new System.Drawing.Point(0, 0);
            this._namePanelTable.Name = "_namePanelTable";
            this._namePanelTable.RowCount = 1;
            this._namePanelTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._namePanelTable.Size = new System.Drawing.Size(277, 26);
            this._namePanelTable.TabIndex = 2;
            // 
            // _name
            // 
            this._name.Dock = System.Windows.Forms.DockStyle.Fill;
            this._name.Location = new System.Drawing.Point(47, 3);
            this._name.Name = "_name";
            this._name.Size = new System.Drawing.Size(227, 20);
            this._name.TabIndex = 1;
            // 
            // _namePanelLabel
            // 
            this._namePanelLabel.AutoSize = true;
            this._namePanelLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._namePanelLabel.Location = new System.Drawing.Point(3, 0);
            this._namePanelLabel.Name = "_namePanelLabel";
            this._namePanelLabel.Size = new System.Drawing.Size(38, 26);
            this._namePanelLabel.TabIndex = 2;
            this._namePanelLabel.Text = "Name:";
            this._namePanelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _listView
            // 
            this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this._listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this._listView.HideSelection = false;
            this._listView.LabelWrap = false;
            this._listView.Location = new System.Drawing.Point(3, 3);
            this._listView.MultiSelect = false;
            this._listView.Name = "_listView";
            this._listView.Scrollable = false;
            this._listView.Size = new System.Drawing.Size(277, 255);
            this._listView.TabIndex = 3;
            this._listView.UseCompatibleStateImageBehavior = false;
            this._listView.View = System.Windows.Forms.View.Details;
            this._listView.SelectedIndexChanged += new System.EventHandler(this.OnSelectedValueChanged);
            this._listView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 417;
            // 
            // NewContentDialog
            // 
            this.AcceptButton = this._okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.CancelButton = this._cancelBtn;
            this.ClientSize = new System.Drawing.Size(283, 323);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewContentDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Content";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this._buttonPanel.ResumeLayout(false);
            this._namePanel.ResumeLayout(false);
            this._namePanel.PerformLayout();
            this._namePanelTable.ResumeLayout(false);
            this._namePanelTable.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel _namePanel;
        private System.Windows.Forms.TextBox _name;
        private System.Windows.Forms.TableLayoutPanel _namePanelTable;
        private System.Windows.Forms.Label _namePanelLabel;
        private System.Windows.Forms.ListView _listView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Panel _buttonPanel;
        private System.Windows.Forms.Button _cancelBtn;
        private System.Windows.Forms.Button _okBtn;

    }
}