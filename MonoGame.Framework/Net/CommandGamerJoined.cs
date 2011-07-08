using System;

namespace Microsoft.Xna.Framework.Net
{
	public class CommandGamerJoined
	{
		int gamerInternalIndex = -1;
		GamerStates states;
		public CommandGamerJoined (int internalIndex, bool isHost, bool isLocal)
		{
			gamerInternalIndex = internalIndex;
			
			if (isHost)
				states = states | GamerStates.Host;
			if (isLocal)
				states = states | GamerStates.Local;
			
		}
		
		public GamerStates State
		{
			get { return states; }
		}
		
		public int InternalIndex
		{
			get { return gamerInternalIndex; }
		}
		
		public static CommandEventType Command {
			get { return CommandEventType.GamerJoined; }
		}
	}
}

