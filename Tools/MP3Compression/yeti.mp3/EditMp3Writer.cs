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
//  LAME ( LAME Ain't an Mp3 Encoder ) 
//  You must call the fucntion "beVersion" to obtain information  like version 
//  numbers (both of the DLL and encoding engine), release date and URL for 
//  lame_enc's homepage. All this information should be made available to the 
//  user of your product through a dialog box or something similar.
//  You must see all information about LAME project and legal license infos at 
//  http://www.mp3dev.org/  The official LAME site
//
//  About Thomson and/or Fraunhofer patents:
//  Any use of this product does not convey a license under the relevant 
//  intellectual property of Thomson and/or Fraunhofer Gesellschaft nor imply 
//  any right to use this product in any finished end user or ready-to-use final 
//  product. An independent license for such use is required. 
//  For details, please visit http://www.mp3licensing.com.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Yeti.MMedia;
using Yeti.MMedia.Mp3;
using WaveLib;

namespace Yeti.MMedia.Mp3
{
	/// <summary>
	/// Summary description for EditMp3Writer.
	/// </summary>
	public class EditMp3Writer : System.Windows.Forms.UserControl, IEditAudioWriterConfig
	{
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private Yeti.MMedia.EditFormat editFormat1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBoxMpegVersion;
    private System.Windows.Forms.ComboBox comboBoxBitRate;
    private System.Windows.Forms.CheckBox checkBoxVBR;
    private System.Windows.Forms.ComboBox comboBoxMaxBitRate;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox comboBoxVBRMethod;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox comboBoxAvgBitrate;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.CheckBox checkBoxCopyRight;
    private System.Windows.Forms.ToolTip toolTip1;
    private System.Windows.Forms.CheckBox checkBoxCRC;
    private System.Windows.Forms.CheckBox checkBoxOriginal;
    private System.Windows.Forms.CheckBox checkBoxPrivate;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TrackBar trackBarVBRQuality;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.GroupBox groupBoxVBR;
    private System.ComponentModel.IContainer components;
    
    private Lame.BE_CONFIG m_Config = null;
    private const string Mpeg1BitRates = "32,40,48,56,64,80,96,112,128,160,192,224,256,320";
    private const string Mpeg2BitRates = "8,16,24,32,40,48,56,64,80,96,112,128,144,160";

		public EditMp3Writer()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			m_Config = new Yeti.Lame.BE_CONFIG(editFormat1.Format);
      DoSetInitialValues();
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

    private bool m_FireConfigChangeEvent = true;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.components = new System.ComponentModel.Container();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.editFormat1 = new Yeti.MMedia.EditFormat();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.checkBoxPrivate = new System.Windows.Forms.CheckBox();
      this.checkBoxOriginal = new System.Windows.Forms.CheckBox();
      this.checkBoxCRC = new System.Windows.Forms.CheckBox();
      this.checkBoxCopyRight = new System.Windows.Forms.CheckBox();
      this.checkBoxVBR = new System.Windows.Forms.CheckBox();
      this.groupBoxVBR = new System.Windows.Forms.GroupBox();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.comboBoxVBRMethod = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.trackBarVBRQuality = new System.Windows.Forms.TrackBar();
      this.label6 = new System.Windows.Forms.Label();
      this.comboBoxAvgBitrate = new System.Windows.Forms.ComboBox();
      this.label5 = new System.Windows.Forms.Label();
      this.comboBoxMaxBitRate = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.comboBoxBitRate = new System.Windows.Forms.ComboBox();
      this.textBoxMpegVersion = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.groupBoxVBR.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarVBRQuality)).BeginInit();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(336, 280);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.editFormat1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(328, 254);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Input format";
      // 
      // editFormat1
      // 
      this.editFormat1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.editFormat1.Location = new System.Drawing.Point(0, 0);
      this.editFormat1.Name = "editFormat1";
      this.editFormat1.ReadOnly = true;
      this.editFormat1.Size = new System.Drawing.Size(328, 254);
      this.editFormat1.TabIndex = 0;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.checkBoxPrivate);
      this.tabPage2.Controls.Add(this.checkBoxOriginal);
      this.tabPage2.Controls.Add(this.checkBoxCRC);
      this.tabPage2.Controls.Add(this.checkBoxCopyRight);
      this.tabPage2.Controls.Add(this.checkBoxVBR);
      this.tabPage2.Controls.Add(this.groupBoxVBR);
      this.tabPage2.Controls.Add(this.comboBoxBitRate);
      this.tabPage2.Controls.Add(this.textBoxMpegVersion);
      this.tabPage2.Controls.Add(this.label2);
      this.tabPage2.Controls.Add(this.label1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(328, 254);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "MP3 config";
      // 
      // checkBoxPrivate
      // 
      this.checkBoxPrivate.Location = new System.Drawing.Point(248, 48);
      this.checkBoxPrivate.Name = "checkBoxPrivate";
      this.checkBoxPrivate.Size = new System.Drawing.Size(72, 24);
      this.checkBoxPrivate.TabIndex = 9;
      this.checkBoxPrivate.Text = "Private";
      this.toolTip1.SetToolTip(this.checkBoxPrivate, "Controls the private bit of MP3 stream");
      this.checkBoxPrivate.CheckedChanged += new System.EventHandler(this.Control_Changed);
      // 
      // checkBoxOriginal
      // 
      this.checkBoxOriginal.Location = new System.Drawing.Point(168, 48);
      this.checkBoxOriginal.Name = "checkBoxOriginal";
      this.checkBoxOriginal.Size = new System.Drawing.Size(72, 24);
      this.checkBoxOriginal.TabIndex = 8;
      this.checkBoxOriginal.Text = "Original";
      this.toolTip1.SetToolTip(this.checkBoxOriginal, "Controls the original bit of MP3 stream");
      this.checkBoxOriginal.CheckedChanged += new System.EventHandler(this.Control_Changed);
      // 
      // checkBoxCRC
      // 
      this.checkBoxCRC.Location = new System.Drawing.Point(88, 48);
      this.checkBoxCRC.Name = "checkBoxCRC";
      this.checkBoxCRC.Size = new System.Drawing.Size(72, 24);
      this.checkBoxCRC.TabIndex = 7;
      this.checkBoxCRC.Text = "CRC";
      this.toolTip1.SetToolTip(this.checkBoxCRC, "If set enables CRC-checksum in the bitstream");
      this.checkBoxCRC.CheckedChanged += new System.EventHandler(this.Control_Changed);
      // 
      // checkBoxCopyRight
      // 
      this.checkBoxCopyRight.Location = new System.Drawing.Point(8, 48);
      this.checkBoxCopyRight.Name = "checkBoxCopyRight";
      this.checkBoxCopyRight.Size = new System.Drawing.Size(72, 24);
      this.checkBoxCopyRight.TabIndex = 6;
      this.checkBoxCopyRight.Text = "Copyright";
      this.toolTip1.SetToolTip(this.checkBoxCopyRight, "Controls the copyrightb bit of MP3 stream");
      this.checkBoxCopyRight.CheckedChanged += new System.EventHandler(this.Control_Changed);
      // 
      // checkBoxVBR
      // 
      this.checkBoxVBR.Location = new System.Drawing.Point(8, 72);
      this.checkBoxVBR.Name = "checkBoxVBR";
      this.checkBoxVBR.Size = new System.Drawing.Size(192, 24);
      this.checkBoxVBR.TabIndex = 5;
      this.checkBoxVBR.Text = "Enable Variable Bit Rate (VBR)";
      this.checkBoxVBR.CheckedChanged += new System.EventHandler(this.checkBoxVBR_CheckedChanged);
      // 
      // groupBoxVBR
      // 
      this.groupBoxVBR.Controls.Add(this.label8);
      this.groupBoxVBR.Controls.Add(this.label7);
      this.groupBoxVBR.Controls.Add(this.comboBoxVBRMethod);
      this.groupBoxVBR.Controls.Add(this.label4);
      this.groupBoxVBR.Controls.Add(this.trackBarVBRQuality);
      this.groupBoxVBR.Controls.Add(this.label6);
      this.groupBoxVBR.Controls.Add(this.comboBoxAvgBitrate);
      this.groupBoxVBR.Controls.Add(this.label5);
      this.groupBoxVBR.Controls.Add(this.comboBoxMaxBitRate);
      this.groupBoxVBR.Controls.Add(this.label3);
      this.groupBoxVBR.Location = new System.Drawing.Point(8, 96);
      this.groupBoxVBR.Name = "groupBoxVBR";
      this.groupBoxVBR.Size = new System.Drawing.Size(304, 144);
      this.groupBoxVBR.TabIndex = 4;
      this.groupBoxVBR.TabStop = false;
      this.groupBoxVBR.Text = "VBR options";
      // 
      // label8
      // 
      this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label8.Location = new System.Drawing.Point(256, 64);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(32, 16);
      this.label8.TabIndex = 13;
      this.label8.Text = "Min";
      // 
      // label7
      // 
      this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label7.Location = new System.Drawing.Point(152, 64);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(56, 16);
      this.label7.TabIndex = 12;
      this.label7.Text = "Max.";
      // 
      // comboBoxVBRMethod
      // 
      this.comboBoxVBRMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxVBRMethod.Items.AddRange(new object[] {
                                                           "NONE",
                                                           "DEFAULT",
                                                           "OLD",
                                                           "NEW",
                                                           "MTRH",
                                                           "ABR"});
      this.comboBoxVBRMethod.Location = new System.Drawing.Point(8, 32);
      this.comboBoxVBRMethod.Name = "comboBoxVBRMethod";
      this.comboBoxVBRMethod.Size = new System.Drawing.Size(121, 21);
      this.comboBoxVBRMethod.TabIndex = 7;
      this.comboBoxVBRMethod.SelectedIndexChanged += new System.EventHandler(this.comboBoxVBRMethod_SelectedIndexChanged);
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(8, 16);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(96, 24);
      this.label4.TabIndex = 6;
      this.label4.Text = "VBR method:";
      // 
      // trackBarVBRQuality
      // 
      this.trackBarVBRQuality.LargeChange = 0;
      this.trackBarVBRQuality.Location = new System.Drawing.Point(144, 32);
      this.trackBarVBRQuality.Maximum = 9;
      this.trackBarVBRQuality.Name = "trackBarVBRQuality";
      this.trackBarVBRQuality.Size = new System.Drawing.Size(144, 42);
      this.trackBarVBRQuality.TabIndex = 11;
      this.trackBarVBRQuality.Scroll += new System.EventHandler(this.Control_Changed);
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(152, 16);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(100, 16);
      this.label6.TabIndex = 10;
      this.label6.Text = "VBR quality:";
      // 
      // comboBoxAvgBitrate
      // 
      this.comboBoxAvgBitrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxAvgBitrate.Location = new System.Drawing.Point(8, 112);
      this.comboBoxAvgBitrate.Name = "comboBoxAvgBitrate";
      this.comboBoxAvgBitrate.Size = new System.Drawing.Size(121, 21);
      this.comboBoxAvgBitrate.TabIndex = 9;
      this.comboBoxAvgBitrate.SelectedIndexChanged += new System.EventHandler(this.Control_Changed);
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(8, 96);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(88, 16);
      this.label5.TabIndex = 8;
      this.label5.Text = "Average bit rate:";
      // 
      // comboBoxMaxBitRate
      // 
      this.comboBoxMaxBitRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxMaxBitRate.Location = new System.Drawing.Point(8, 72);
      this.comboBoxMaxBitRate.Name = "comboBoxMaxBitRate";
      this.comboBoxMaxBitRate.Size = new System.Drawing.Size(121, 21);
      this.comboBoxMaxBitRate.TabIndex = 5;
      this.comboBoxMaxBitRate.SelectedIndexChanged += new System.EventHandler(this.BitRateChange);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 56);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(104, 16);
      this.label3.TabIndex = 4;
      this.label3.Text = "Max bit rate:";
      // 
      // comboBoxBitRate
      // 
      this.comboBoxBitRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxBitRate.Location = new System.Drawing.Point(168, 24);
      this.comboBoxBitRate.Name = "comboBoxBitRate";
      this.comboBoxBitRate.Size = new System.Drawing.Size(121, 21);
      this.comboBoxBitRate.TabIndex = 3;
      this.toolTip1.SetToolTip(this.comboBoxBitRate, "Minimum bit rate if VBR is specified ");
      this.comboBoxBitRate.SelectedIndexChanged += new System.EventHandler(this.BitRateChange);
      // 
      // textBoxMpegVersion
      // 
      this.textBoxMpegVersion.Location = new System.Drawing.Point(8, 24);
      this.textBoxMpegVersion.Name = "textBoxMpegVersion";
      this.textBoxMpegVersion.ReadOnly = true;
      this.textBoxMpegVersion.Size = new System.Drawing.Size(120, 20);
      this.textBoxMpegVersion.TabIndex = 2;
      this.textBoxMpegVersion.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(8, 8);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(96, 16);
      this.label2.TabIndex = 1;
      this.label2.Text = "MPEG Version:";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(168, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Bit rate:";
      // 
      // EditMp3Writer
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "EditMp3Writer";
      this.Size = new System.Drawing.Size(336, 280);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.groupBoxVBR.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarVBRQuality)).EndInit();
      this.ResumeLayout(false);

    }
		#endregion

    #region IEditAudioWriterConfig Members

    public AudioWriterConfig Config
    {
      get
      {
        Lame.BE_CONFIG cfg = new Yeti.Lame.BE_CONFIG(editFormat1.Format, uint.Parse(comboBoxBitRate.SelectedItem.ToString()));
        cfg.format.lhv1.bCopyright = checkBoxCopyRight.Checked? 1 : 0;
        cfg.format.lhv1.bCRC = checkBoxCRC.Checked? 1 : 0;
        cfg.format.lhv1.bOriginal = checkBoxOriginal.Checked? 1 : 0;
        cfg.format.lhv1.bPrivate = checkBoxPrivate.Checked? 1 : 0;
        if ( checkBoxVBR.Checked )
        {
          cfg.format.lhv1.bEnableVBR = 1;
          if ( comboBoxVBRMethod.SelectedIndex > 0 )
          {
            cfg.format.lhv1.nVbrMethod = (Lame.VBRMETHOD)(comboBoxVBRMethod.SelectedIndex+1);
          }
          else
          {
            cfg.format.lhv1.nVbrMethod = Lame.VBRMETHOD.VBR_METHOD_DEFAULT;
          }
          cfg.format.lhv1.dwMaxBitrate = uint.Parse(comboBoxMaxBitRate.SelectedItem.ToString());
          if (cfg.format.lhv1.dwMaxBitrate < cfg.format.lhv1.dwBitrate)
          {
            cfg.format.lhv1.dwMaxBitrate = cfg.format.lhv1.dwBitrate;
          }
          cfg.format.lhv1.dwVbrAbr_bps = uint.Parse(comboBoxAvgBitrate.SelectedItem.ToString());
          if (cfg.format.lhv1.dwVbrAbr_bps < cfg.format.lhv1.dwBitrate)
          {
            cfg.format.lhv1.dwVbrAbr_bps = cfg.format.lhv1.dwBitrate;
          }
          cfg.format.lhv1.nVBRQuality = trackBarVBRQuality.Value;
        }
        else
        {
          cfg.format.lhv1.bEnableVBR = 0;
        }
        return new Mp3WriterConfig(editFormat1.Format, cfg);
      }
      set
      {
        editFormat1.Format = value.Format;
        m_Config = ((Mp3WriterConfig)value).Mp3Config;
        DoSetInitialValues();
      }
    }

    #endregion

    #region IConfigControl Members

    public void DoApply()
    {
      // TODO:  Add EditMp3Writer.DoApply implementation
    }

    public string ControlName
    {
      get
      {
        return "MP3 Writer config";
      }
    }

    public event System.EventHandler ConfigChange;

    public Control ConfigControl
    {
      get
      {
        return this;
      }
    }

    public void DoSetInitialValues()
    {
      m_FireConfigChangeEvent = false;
      try
      {
        int i;
        string[] rates;
        Lame.LHV1 hv = m_Config.format.lhv1;
        editFormat1.DoSetInitialValues();
        if (hv.dwMpegVersion == Lame.LHV1.MPEG2)
        {
          textBoxMpegVersion.Text = "MPEG2";
          rates = Mpeg2BitRates.Split(',');
        }
        else
        {
          textBoxMpegVersion.Text = "MPEG1";
          rates = Mpeg1BitRates.Split(',');
        }
        comboBoxBitRate.Items.Clear();
        comboBoxBitRate.Items.AddRange(rates);
        comboBoxMaxBitRate.Items.Clear();
        comboBoxMaxBitRate.Items.AddRange(rates);
        comboBoxAvgBitrate.Items.Clear();
        comboBoxAvgBitrate.Items.AddRange(rates);
        i = comboBoxBitRate.Items.IndexOf(hv.dwBitrate.ToString());
        comboBoxBitRate.SelectedIndex = i;
        comboBoxAvgBitrate.SelectedIndex = i;
        comboBoxMaxBitRate.SelectedIndex = i;
        checkBoxCopyRight.Checked = hv.bCopyright != 0;
        checkBoxCRC.Checked = hv.bCRC != 0;
        checkBoxOriginal.Checked = hv.bOriginal != 0;
        checkBoxPrivate.Checked = hv.bPrivate != 0;
        comboBoxVBRMethod.SelectedIndex = (int)hv.nVbrMethod + 1;
        if ( (hv.nVBRQuality >=0) && (hv.nVBRQuality <= 9) )
        {
          trackBarVBRQuality.Value = hv.nVBRQuality;
        }
        else
        {
          trackBarVBRQuality.Value = 0;
        }
        checkBoxVBR.Checked = groupBoxVBR.Enabled = hv.bEnableVBR != 0;
      }
      finally
      {
        m_FireConfigChangeEvent = true;
      }
    }

    #endregion

    protected virtual void DoConfigChange(System.EventArgs e)
    {
      if ( m_FireConfigChangeEvent && (ConfigChange != null) )
      {
        ConfigChange(this, e);
      }
    }

    private void checkBoxVBR_CheckedChanged(object sender, System.EventArgs e)
    {
      if (checkBoxVBR.Checked)
      {
        groupBoxVBR.Enabled = true;
        if (comboBoxVBRMethod.SelectedIndex < 1)
        {
          comboBoxVBRMethod.SelectedIndex = 1;
        }
      }
      else
      {
        groupBoxVBR.Enabled = false;
      }
      DoConfigChange(e);
    }

    private void Control_Changed(object sender, System.EventArgs e)
    {
      DoConfigChange(e);
    }

    private void BitRateChange(object sender, System.EventArgs e)
    {
      if (comboBoxMaxBitRate.SelectedIndex < comboBoxBitRate.SelectedIndex )
      {
        comboBoxMaxBitRate.SelectedIndex = comboBoxBitRate.SelectedIndex;
      }
      DoConfigChange(e);
    }

    private void comboBoxVBRMethod_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (checkBoxVBR.Checked && (comboBoxVBRMethod.SelectedIndex == 0))
      {
        comboBoxVBRMethod.SelectedIndex = 1;
      }
      DoConfigChange(e);
    }

  }
}
