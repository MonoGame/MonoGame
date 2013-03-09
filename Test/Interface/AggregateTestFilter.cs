using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Core;

namespace MonoGame.Tests {
	class AggregateTestFilter : ITestFilter {

		private readonly ITestFilter[] _filters;

		public AggregateTestFilter (IEnumerable<ITestFilter> filters)
		{
			if (filters == null)
				throw new ArgumentNullException("filters");
			_filters = filters.ToArray();
		}

		#region ITestFilter Implementation

		public bool IsEmpty
		{
			get { return false; }
		}

		public bool Match (ITest test)
		{
			return false;
		}

		public bool Pass (ITest test)
		{
			if (test.IsSuite)
				return true;

			foreach (var filter in _filters)
				if (!filter.Pass (test))
					return false;

			return true;
		}

		#endregion ITestFilter Implementation
	}
}

