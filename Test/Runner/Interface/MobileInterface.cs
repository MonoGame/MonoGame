using System;
using System.IO;
using System.Reflection;
using System.Xml.Xsl;

using NUnit.Core;
using NUnit.Core.Builders;
using NUnit.Core.Extensibility;
using NUnit.Util;

namespace MonoGame.Tests.Interface {
	class MobileInterface {
		public static void RunAsync (string[] args)
		{
			Paths.SetStandardWorkingDirectory ();
			File.WriteAllText ("status.txt", "running");

			var runOptions = RunOptions.Parse (args);

			if (runOptions.ShouldShowHelp) {
				runOptions.ShowHelp ();
				return;
			}

			CoreExtensions.Host.InitializeService ();

			var assembly = Assembly.GetExecutingAssembly ();

			var runner = new ThreadedTestRunner(new SimpleTestRunner());
			TestPackage package = new TestPackage (assembly.GetName ().Name);
			package.Assemblies.Add (assembly.Location);
			if (!runner.Load (package)) {
				Console.WriteLine ("Could not find the tests.");
				return;
			}

			var listener = new MobileListener (runOptions);
			var filter = new AggregateTestFilter (runOptions.Filters);
			runner.BeginRun (listener, filter, false, LoggingThreshold.Warn);
		}

		class MobileListener : TestEventListenerBase {

			private readonly RunOptions _runOptions;
			public MobileListener(RunOptions runOptions)
			{
				if (runOptions == null)
					throw new ArgumentNullException("runOptions");
				_runOptions = runOptions;
			}

			public override void RunFinished(TestResult result)
			{
				base.RunFinished (result);

				var resultWriter = new XmlResultWriter (_runOptions.XmlResultsPath);
				resultWriter.SaveTestResult (result);

				if (_runOptions.PerformXslTransform) {
					var transform = new XslTransform ();
					transform.Load (_runOptions.XslTransformPath);
					transform.Transform (
						_runOptions.XmlResultsPath,
						_runOptions.TransformedResultsPath);
				}

				File.WriteAllText (_runOptions.StdoutPath, StdoutStandin.ToString ());

				//if (runOptions.PerformXslTransform && runOptions.ShouldLaunchResults)
				//	System.Diagnostics.Process.Start (runOptions.TransformedResultsPath);

				File.WriteAllText("status.txt", "finished");
			}
		}
	}
}
