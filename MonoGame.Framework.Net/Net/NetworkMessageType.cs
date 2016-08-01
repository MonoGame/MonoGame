using System;

namespace Microsoft.Xna.Framework.Net
{
	// These are for internal use specifying the Data Packet Contents
	internal enum NetworkMessageType : byte
	{
		Data = 0,
		GamerJoined = 1,
		GamerLeft = 2,
		Introduction = 3,
		GamerProfile = 4,
		RequestGamerProfile = 5,
		GamerStateChange = 6,
		SessionStateChange = 7,
	}
}

