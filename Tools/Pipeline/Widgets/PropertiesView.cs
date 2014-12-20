using System.Collections.Generic;
using System.Collections;
using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PropertiesView : Gtk.Bin
	{
		public PropertiesView ()
		{
			this.Build ();
		}

		protected void OnVbox1SizeAllocated (object o, SizeAllocatedArgs args)
		{
			vpaned1.Position = vpaned1.Allocation.Height - 50;
		}
	}
}

