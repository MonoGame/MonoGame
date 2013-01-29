using System;
using MonoMac.AppKit;

namespace Microsoft.Xna.Framework
{
	public class NSEventArgs : EventArgs
	{
		public NSEventArgs (NSEvent theEvent)
		{
			TheEvent = theEvent;
		}

		public NSEvent TheEvent
		{
			get;
			private set;
		}
	}
}

