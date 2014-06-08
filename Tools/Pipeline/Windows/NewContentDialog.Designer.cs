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
            this._listBox = new System.Windows.Forms.ListBox();
            this._buttonPanel = new System.Windows.Forms.Panel();
            this._cancelBtn = new System.Windows.Forms.Button();
            this._okBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._namePanel = new System.Windows.Forms.Panel();
            this._namePanelTable = new System.Windows.Forms.TableLayoutPanel();
            this._name = new System.Windows.Forms.TextBox();
            this._namePanelLabel = new System.Windows.Forms.Label();
            this._buttonPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this._namePanel.SuspendLayout();
            this._namePanelTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // _listBox
            // 
            this._listBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listBox.FormattingEnabled = true;
            this._listBox.Items.AddRange(new object[] {
            "SpriteFont",
            "SpriteSheet"});
            this._listBox.Location = new System.Drawing.Point(3, 3);
            this._listBox.Name = "_listBox";
            this._listBox.Size = new System.Drawing.Size(447, 382);
            this._listBox.TabIndex = 0;
            this._listBox.SelectedValueChanged += new System.EventHandler(this.OnListBoxSelectedValueChanged);
            // 
            // _buttonPanel
            // 
            this._buttonPanel.AutoSize = true;
            this._buttonPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._buttonPanel.BackColor = System.Drawing.SystemColors.Control;
            this._buttonPanel.Controls.Add(this._cancelBtn);
            this._buttonPanel.Controls.Add(this._okBtn);
            this._buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._buttonPanel.Location = new System.Drawing.Point(3, 423);
            this._buttonPanel.Name = "_buttonPanel";
            this._buttonPanel.Size = new System.Drawing.Size(447, 32);
            this._buttonPanel.TabIndex = 1;
            // 
            // _cancelBtn
            // 
            this._cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelBtn.Location = new System.Drawing.Point(363, 6);
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
            this._okBtn.Location = new System.Drawing.Point(282, 6);
            this._okBtn.Name = "_okBtn";
            this._okBtn.Size = new System.Drawing.Size(75, 23);
            this._okBtn.TabIndex = 0;
            this._okBtn.Text = "Ok";
            this._okBtn.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this._listBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._namePanel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this._buttonPanel, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(453, 458);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // _namePanel
            // 
            this._namePanel.AutoSize = true;
            this._namePanel.Controls.Add(this._namePanelTable);
            this._namePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._namePanel.Location = new System.Drawing.Point(3, 391);
            this._namePanel.Name = "_namePanel";
            this._namePanel.Size = new System.Drawing.Size(447, 26);
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
            this._namePanelTable.Size = new System.Drawing.Size(447, 26);
            this._namePanelTable.TabIndex = 2;
            // 
            // _name
            // 
            this._name.Dock = System.Windows.Forms.DockStyle.Fill;
            this._name.Location = new System.Drawing.Point(47, 3);
            this._name.Name = "_name";
            this._name.Size = new System.Drawing.Size(397, 20);
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
            // NewContentDialog
            // 
            this.AcceptButton = this._okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.CancelButton = this._cancelBtn;
            this.ClientSize = new System.Drawing.Size(453, 458);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewContentDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Content";
            this._buttonPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this._namePanel.ResumeLayout(false);
            this._namePanel.PerformLayout();
            this._namePanelTable.ResumeLayout(false);
            this._namePanelTable.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox _listBox;
        private System.Windows.Forms.Panel _buttonPanel;
        private System.Windows.Forms.Button _cancelBtn;
        private System.Windows.Forms.Button _okBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel _namePanel;
        private System.Windows.Forms.TextBox _name;
        private System.Windows.Forms.TableLayoutPanel _namePanelTable;
        private System.Windows.Forms.Label _namePanelLabel;

    }
}