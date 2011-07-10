using System;

namespace Microsoft.Xna.Framework.Net
{
	internal interface ICommand
	{
		
		CommandEventType Command { get; }
	}
}

