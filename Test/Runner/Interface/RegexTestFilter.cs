using System;
using System.Text.RegularExpressions;

using NUnit.Core;

namespace MonoGame.Tests {
	class RegexTestFilter : ITestFilter {
		private readonly Regex _regex;
		private readonly TestFilterAction _action;

		public RegexTestFilter (Regex regex, TestFilterAction action)
		{
			if (regex == null)
				throw new ArgumentNullException ("regex");
			_regex = regex;
			_action = action;
		}

		public static RegexTestFilter Parse (string s, TestFilterAction filterAction)
		{
			if (string.IsNullOrEmpty (s))
				throw new ArgumentException ("Filter string cannot be null or empty", "s");
	
			string pattern;
			if (s.Length > 1 && s.StartsWith ("/") && s.EndsWith ("/")) {
				pattern = s.Substring (1, s.Length - 2);
			} else {
				pattern = string.Join ("", ".*", s, ".*");
			}
	
			var regex = new Regex (pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			return new RegexTestFilter (regex, filterAction);
		}

		#region ITestFilter Members
		public bool Pass(ITest test)
		{
			var match = _regex.Match (test.TestName.FullName);

			if (_action == TestFilterAction.Exclude)
				return !match.Success;
			return match.Success;
		}

		public bool Match(ITest test)
		{
			throw new NotImplementedException();
		}

		public bool IsEmpty
		{
			get { return false; }
		}
		#endregion
	}
}

