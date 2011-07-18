using System;

namespace Microsoft.Xna.Framework.Net
{
	internal class CommandGamerLeft : ICommand
	{
		int gamerInternalIndex = -1;
		internal long remoteUniqueIdentifier = -1;
		
		public CommandGamerLeft (int internalIndex)
		{
			gamerInternalIndex = internalIndex;
			
		}
		
		public CommandGamerLeft (long uniqueIndentifier)
		{
			this.remoteUniqueIdentifier = uniqueIndentifier;
			
		}		
		
		public int InternalIndex
		{
			get { return gamerInternalIndex; }
		}
		
		public CommandEventType Command {
			get { return CommandEventType.GamerLeft; }
		}
	}
}

