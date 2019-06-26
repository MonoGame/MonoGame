using System;

namespace Microsoft.Xna.Framework.Net
{
	internal class CommandReceiveData : ICommand
	{
		internal byte[] data;
		internal long remoteUniqueIdentifier = -1;
		internal NetworkGamer gamer;
		
		public CommandReceiveData (long remoteUniqueIdentifier, byte[] data)
		{
			this.remoteUniqueIdentifier = remoteUniqueIdentifier;
			this.data = data;
				
		}
		
		public CommandEventType Command {
			get { return CommandEventType.ReceiveData; }
		}
	}
}

