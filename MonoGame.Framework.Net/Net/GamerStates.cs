using System;

namespace Microsoft.Xna.Framework.Net
{
	[Flags]
	public enum GamerStates
	{
		Local 		= 0x000001,
		Host 		= 0x000010,
		HasVoice 	= 0x000100,
		Guest 		= 0x001000,
		MutedByLocalUser = 0x010000,
		PrivateSlot	=  0x100000,
		Ready		= 0x1000000,
		
	}
}

