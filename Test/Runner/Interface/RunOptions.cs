using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NDesk.Options;
using NUnit.Core;
using System.IO;

namespace MonoGame.Tests {
	public class RunOptions {
		public RunOptions (
			OptionSet optionSet,
			bool shouldLaunchResults, bool shouldPerformXslTransform, bool shouldShowHelp,
			string xmlResultsPath, string transformedResultsPath, string xslTransformPath,
			string stdoutPath, IEnumerable<ITestFilter> filters)
		{
			if (filters == null)
				throw new ArgumentNullException("filters");

			_optionSet = optionSet;

			_shouldLaunchResults = shouldLaunchResults;
			_shouldPerformXslTransform = shouldPerformXslTransform;
			_shouldShowHelp = shouldShowHelp;

			_xmlResultsPath = xmlResultsPath;
			_transformedResultsPath = transformedResultsPath;
			_xslTransformPath = xslTransformPath;
			_stdoutPath = stdoutPath;

			_filters = filters.ToArray ();
		}

		private readonly OptionSet _optionSet;
		public OptionSet OptionSet { get { return _optionSet; } }

		private readonly bool _shouldLaunchResults;
		public bool ShouldLaunchResults { get { return _shouldLaunchResults; } }

		private readonly bool _shouldPerformXslTransform;
		public bool PerformXslTransform { get { return _shouldPerformXslTransform; } }

		private readonly bool _shouldShowHelp;
		public bool ShouldShowHelp { get { return _shouldShowHelp; } }

		private readonly string _xmlResultsPath;
		public string XmlResultsPath { get { return _xmlResultsPath; } }

		private readonly string _transformedResultsPath;
		public string TransformedResultsPath { get { return _transformedResultsPath; } }

		private readonly string _xslTransformPath;
		public string XslTransformPath { get { return _xslTransformPath; } }

		private readonly string _stdoutPath;
		public string StdoutPath { get { return _stdoutPath; } }

		private readonly ITestFilter[] _filters;
		public IEnumerable<ITestFilter> Filters { get { return _filters; } }

		public void ShowHelp ()
		{
			if (_optionSet == null) {
				Console.WriteLine ("Help is not available");
				return;
			}

			string executableName = Path.GetFileName (
				Assembly.GetExecutingAssembly ().Location);
			Console.WriteLine ("Usage: {0} [OPTIONS]+", executableName);
			Console.WriteLine ("Options:");
			_optionSet.WriteOptionDescriptions (Console.Out);
		}

		public static RunOptions Parse (string [] args) {
			Paths.SetStandardWorkingDirectory ();
			var directory = Directory.GetCurrentDirectory ();

			bool shouldLaunchResults = true;
			bool shouldPerformXslTransform = true;
			bool shouldShowHelp = false;

			string xmlResultsPath = Path.Combine (directory, "test_results.xml");
			string transformedResultsPath = Path.Combine (directory, "test_results.html");
			string xslTransformPath = Path.Combine ("Assets", "tests.xsl");
			string stdoutPath = Path.Combine (directory, "stdout.txt");

			var filters = new List<ITestFilter>();
			var optionSet = new OptionSet () {
				{ "i|include=", x => filters.Add(RegexTestFilter.Parse(x, TestFilterAction.Include)) },
				{ "x|exclude=", x => filters.Add(RegexTestFilter.Parse(x, TestFilterAction.Exclude)) },
				{ "no-launch-results", x => shouldLaunchResults = false },
				{ "no-xsl-transform", x => shouldPerformXslTransform = false },
				{ "xml-results=", x => xmlResultsPath = x },
				{ "xsl-transform=", x => xslTransformPath = x },
				{ "transformed-results=", x => transformedResultsPath = x },
				{ "stdout=", x => stdoutPath = x },
//				{ "v|verbose",  x => ++verbose },
				{ "h|?|help",   x => shouldShowHelp = true },
			};

			List<string> extra = optionSet.Parse (args);
			if (extra.Count > 0)
				Console.WriteLine (
					"Ignoring {0} unrecognized argument(s): {1}",
					extra.Count, string.Join (", ", extra));

			return new RunOptions(
				optionSet, shouldLaunchResults, shouldPerformXslTransform, shouldShowHelp,
				xmlResultsPath, transformedResultsPath, xslTransformPath, stdoutPath, filters);
		}
	}
}

