using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

namespace TemplateWizard
{	
	public enum CodeSharing {
			None, // Why?
			SharedProject,
			NetStandardLibrary,
		}

	public enum ReferenceVia {
		Local,
		Nuget,
	};
	
	public partial class ProjectSelectionPanel : Panel
	{	
		EnumRadioButtonList<CodeSharing> codeSharingOption;
		EnumRadioButtonList<ReferenceVia> referenceOption;
		CheckBox chkDesktopGL, chkUWP, chkiOS, chktvOS, chkMacOS, chkAndroid, chkWindowsDX;

		public event EventHandler OnUpdateOptions;

		public int Flags { get; set; }

		public int CodeSharingOption {
			get {
				return (int)codeSharingOption.SelectedValue;
			}
			set {
				codeSharingOption.SelectedValue = (CodeSharing)value;
			}
		}

		public int ReferenceOption {
			get {
				return (int)referenceOption.SelectedValue;
			}
			set {
				referenceOption.SelectedValue = (ReferenceVia)value;
			}
		}

		[Flags]
		public enum Target {
			iOS = 0x1,
			Android = 0x2,
			tvOS = 0x4,
			DesktopGL = 0x8,
			UWP = 0x16,
			WindowsDX = 0x32,
			MacOS = 0x64,
		}

		void SomethingChanged (object s, EventArgs e)
		{
			Target t = 0;
			if (chkiOS.Checked.Value)
				t |= Target.iOS;
			if (chktvOS.Checked.Value)
				t |= Target.tvOS;
			if (chkUWP.Checked.Value)
				t |= Target.UWP;
			if (chkWindowsDX.Checked.Value)
				t |= Target.WindowsDX;
			if (chkMacOS.Checked.Value)
				t |= Target.MacOS;
			if (chkDesktopGL.Checked.Value)
				t |= Target.DesktopGL;
			if (chkAndroid.Checked.Value)
				t |= Target.Android;
			Flags = (int)t;
			if (OnUpdateOptions != null)
					OnUpdateOptions (s, e);
		}

		void InitializeComponent()
		{
			codeSharingOption = new EnumRadioButtonList<CodeSharing> () {
				SelectedValue = CodeSharing.SharedProject,
			};
			codeSharingOption.SelectedValueChanged += SomethingChanged;

			referenceOption = new EnumRadioButtonList<ReferenceVia> () {
				SelectedValue = ReferenceVia.Local,
			};
			referenceOption.SelectedValueChanged += SomethingChanged;

			chkDesktopGL = new CheckBox () {
				Width = 110,
				Text = "DesktopGL",
				Enabled = true,
			};
			chkDesktopGL.CheckedChanged += SomethingChanged;
			chkiOS = new CheckBox () {
				Width = 110,
				Text = "iOS",
				Enabled = true,
			};
			chkiOS.CheckedChanged += SomethingChanged;
			chktvOS = new CheckBox () {
				Width = 110,
				Text = "tvOS",
				Enabled = true,
			};
			chktvOS.CheckedChanged += SomethingChanged;
			chkUWP = new CheckBox () {
				Width = 225,
				Text = "Universal Windows Platform",
				Enabled = Environment.OSVersion.Platform == PlatformID.Win32NT,
			};
			chkUWP.CheckedChanged += SomethingChanged;
			chkWindowsDX = new CheckBox () {
				Width = 110,
				Text = "Windows (DX)",
				Enabled = Environment.OSVersion.Platform == PlatformID.Win32NT,
			};
			chkWindowsDX.CheckedChanged += SomethingChanged;
			chkMacOS = new CheckBox () {
				Width = 110,
				Text = "MacOS",
				Enabled = true,
			};
			chkMacOS.CheckedChanged += SomethingChanged;
			chkAndroid = new CheckBox () {
				Width = 110,
				Text = "Android",
				Enabled = true,
			};
			chkAndroid.CheckedChanged += SomethingChanged;

			Content = new StackLayout
			{
				Padding = 10,
				Spacing = 10,
				Width = 500,
				Height = 200,
				Items = {
					new GroupBox {
						Text = "Platforms",
						Content = new StackLayout {
							Items =  {
								new StackLayout {
									Orientation = Orientation.Horizontal,
									Spacing = 5,
									Items  = {
										chkDesktopGL,
										chkUWP,
										chkWindowsDX,
									},
								},
								new StackLayout {
									Orientation = Orientation.Horizontal,
									Spacing = 5,
									Items  = {
										chkMacOS,										
										chkiOS,
										chktvOS,
										chkAndroid,
									},
								},
							},
						},
					},
					new GroupBox {
						Text = "Code Sharing",
						Content = 
							new StackLayout {
								Orientation = Orientation.Horizontal,
								Spacing = 5,
								Items  = {
									codeSharingOption,
								}
							},
					},
					new GroupBox {
						Text = "Reference MonoGame via",
						Content = 
							new StackLayout {
								Orientation = Orientation.Horizontal,
								Spacing = 5,
								Items  = {
									referenceOption,
								}
						}
					},
				},	
			};
		}
	}
}
