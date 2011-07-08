using System;

namespace Microsoft.Xna.Framework.Net
{
	public class CommandEvent
	{
		CommandEventType command;
		object commandObject;
		
		public CommandEvent (CommandEventType command, object commandObject)
		{
			this.command = command;
			this.commandObject = commandObject;
		}
		
		public CommandEventType Commnad
		{
			get { return command; }
			
		}
		
		public object CommandObject
		{
			get { return commandObject; }
		}
	}
}

