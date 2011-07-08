using System;

namespace Microsoft.Xna.Framework.Net
{
	public class CommandSessionStateChange
	{
		
		NetworkSessionState newState;
		NetworkSessionState oldState;
		
		public CommandSessionStateChange (NetworkSessionState newState, NetworkSessionState oldState)
		{
			this.newState = newState;
			this.oldState = oldState;
		}
		
		public NetworkSessionState NewState
		{
			get { return newState; }
		}
		
		public NetworkSessionState OldState
		{
			get { return oldState; }
		}
		
		public static CommandEventType Command {
			get { return CommandEventType.SessionStateChange; }
		}		
	}
}

