// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;

namespace MonoGame.InteractiveTests {

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class InteractiveTestAttribute : Attribute {
		public InteractiveTestAttribute (string name, string category)
		{
			_name = name;
			_category = category;
		}

		public InteractiveTestAttribute (string name)
			: this(name, null)
		{
		}

		public InteractiveTestAttribute ()
			: this(null, null)
		{
		}

		private readonly string _name;
		public string Name {
			get { return _name; }
		}

		private readonly string _category;
		public string Category {
			get { return _category; }
		}
	}
}

