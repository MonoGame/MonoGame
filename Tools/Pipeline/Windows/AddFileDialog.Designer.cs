namespace MonoGame.Tools.Pipeline
{
    partial class AddFileDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.radioButtonCopy = new System.Windows.Forms.RadioButton();
            this.radioButtonLink = new System.Windows.Forms.RadioButton();
            this.radioButtonSkip = new System.Windows.Forms.RadioButton();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(389, 49);
            this.label1.TabIndex = 0;
            this.label1.Text = "The file %file is outside of the target directory. What would you like to do?";
            // 
            // radioButtonCopy
            // 
            this.radioButtonCopy.AutoSize = true;
            this.radioButtonCopy.Checked = true;
            this.radioButtonCopy.Location = new System.Drawing.Point(29, 61);
            this.radioButtonCopy.Name = "radioButtonCopy";
            this.radioButtonCopy.Size = new System.Drawing.Size(156, 17);
            this.radioButtonCopy.TabIndex = 1;
            this.radioButtonCopy.TabStop = true;
            this.radioButtonCopy.Text = "Copy the file to the directory";
            this.radioButtonCopy.UseVisualStyleBackColor = true;
            // 
            // radioButtonLink
            // 
            this.radioButtonLink.AutoSize = true;
            this.radioButtonLink.Location = new System.Drawing.Point(29, 84);
            this.radioButtonLink.Name = "radioButtonLink";
            this.radioButtonLink.Size = new System.Drawing.Size(118, 17);
            this.radioButtonLink.TabIndex = 2;
            this.radioButtonLink.Text = "Add a link to the file";
            this.radioButtonLink.UseVisualStyleBackColor = true;
            // 
            // radioButtonSkip
            // 
            this.radioButtonSkip.AutoSize = true;
            this.radioButtonSkip.Location = new System.Drawing.Point(29, 107);
            this.radioButtonSkip.Name = "radioButtonSkip";
            this.radioButtonSkip.Size = new System.Drawing.Size(115, 17);
            this.radioButtonSkip.TabIndex = 3;
            this.radioButtonSkip.Text = "Skip adding the file";
            this.radioButtonSkip.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(20, 162);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(215, 17);
            this.checkBox1.TabIndex = 4;
            this.checkBox1.Text = "Use the same action for all selected files";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Location = new System.Drawing.Point(0, 185);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(413, 45);
            this.panel1.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(326, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button2.Location = new System.Drawing.Point(245, 10);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "OK";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // AddFileDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(413, 230);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.radioButtonSkip);
            this.Controls.Add(this.radioButtonLink);
            this.Controls.Add(this.radioButtonCopy);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddFileDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AddFileDialog";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButtonCopy;
        private System.Windows.Forms.RadioButton radioButtonLink;
        private System.Windows.Forms.RadioButton radioButtonSkip;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}