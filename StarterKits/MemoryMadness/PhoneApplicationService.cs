using System;
using System.Collections.Generic;
namespace MemoryMadness
{
	public class PhoneApplicationService
	{
		private static PhoneApplicationService _current = null;
		private Dictionary<string,object> _state;
		
		private PhoneApplicationService ()
		{
			_state = new Dictionary<string, object>();
		}
		
		public static PhoneApplicationService Current
		{
			get
			{
				if (_current == null)
					_current = new PhoneApplicationService();
				return _current;
			}
		}
		
		public Dictionary<string,object> State
		{
			get
			{
				return _state;
			}
		}
	}
}

