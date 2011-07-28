using System;

namespace Microsoft.Xna.Framework.Net
{
	internal class CommandGamerJoined : ICommand
	{
		int gamerInternalIndex = -1;
		internal long remoteUniqueIdentifier = -1;
		GamerStates states;
		string gamerTag = string.Empty;
		string displayName = string.Empty;
		
		public CommandGamerJoined (int internalIndex, bool isHost, bool isLocal)
		{
			gamerInternalIndex = internalIndex;
			
			if (isHost)
				states = states | GamerStates.Host;
			if (isLocal)
				states = states | GamerStates.Local;
			
		}
		
		public CommandGamerJoined (long uniqueIndentifier)
		{
			this.remoteUniqueIdentifier = uniqueIndentifier;
			
		}
		
		public string DisplayName {
			get {
				return displayName;
			}
			set {
				displayName = value;
			}
		}	
		
		public string GamerTag {
			get {
				return gamerTag;
			}
			set {
				gamerTag = value;
			}
		}	
		
		public GamerStates State
		{
			get { return states; }
			set { states = value; }
		}
		
		public int InternalIndex
		{
			get { return gamerInternalIndex; }
		}
		
		public CommandEventType Command {
			get { return CommandEventType.GamerJoined; }
		}
	}
}

