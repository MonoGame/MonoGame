using System;

namespace Microsoft.Xna.Framework.Net
{
	internal class CommandSessionStateChange : ICommand
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
		
		public CommandEventType Command {
			get { return CommandEventType.SessionStateChange; }
		}		
	}
}

