//
//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  yetiicb@hotmail.com
//
//  Copyright (C) 2002-2003 Idael Cardoso. 
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Yeti.MMedia
{
	/// <summary>
	/// Summary description for EditWaveWriter.
	/// </summary>
	public class EditWaveWriter : System.Windows.Forms.UserControl, IEditAudioWriterConfig
	{
    private System.Windows.Forms.GroupBox groupBox1;
    private Yeti.MMedia.EditFormat editFormat1;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EditWaveWriter()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.editFormat1 = new Yeti.MMedia.EditFormat();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.editFormat1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(312, 208);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Audio format";
      // 
      // editFormat1
      // 
      this.editFormat1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.editFormat1.Location = new System.Drawing.Point(3, 16);
      this.editFormat1.Name = "editFormat1";
      this.editFormat1.ReadOnly = false;
      this.editFormat1.Size = new System.Drawing.Size(306, 189);
      this.editFormat1.TabIndex = 0;
      this.editFormat1.ConfigChange += new System.EventHandler(this.editFormat1_ConfigChange);
      // 
      // EditWaveWriter
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "EditWaveWriter";
      this.Size = new System.Drawing.Size(312, 208);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

    #region IEditAudioWriterConfig Members

    public AudioWriterConfig Config
    {
      get
      {
        return new AudioWriterConfig(editFormat1.Format);
      }
      set
      {
        editFormat1.Format = value.Format;
      }
    }

    #endregion

    #region IConfigControl Members

    public void DoApply()
    {
      editFormat1.DoApply();
    }

    public void DoSetInitialValues()
    {
      editFormat1.DoSetInitialValues();
    }

    public Control ConfigControl
    {
      get
      {
        return this;
      }
    }

    public string ControlName
    {
      get
      {
        return "Wave writer config";
      }
    }

    public event System.EventHandler ConfigChange;

    #endregion

    private void editFormat1_ConfigChange(object sender, System.EventArgs e)
    {
      if (ConfigChange != null)
      {
        ConfigChange(sender, e);
      }
    }
  }
}
