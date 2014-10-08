#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement
*/
#endregion License

using System;
using System.IO;
using System.Reflection;
using System.Xml.Xsl;
using NUnit.Core;
using NUnit.Util;

namespace MonoGame.Tests
{
	class CommandLineInterface
	{
		public static int RunMain (string [] args)
		{
			var runOptions = RunOptions.Parse (args);

			if (runOptions.ShouldShowHelp) {
				runOptions.ShowHelp ();
				return 0;
			}

			CoreExtensions.Host.InitializeService ();

			var assembly = Assembly.GetExecutingAssembly ();

			var runner = new SimpleTestRunner ();
			TestPackage package = new TestPackage (assembly.GetName ().Name);
			package.Assemblies.Add (assembly.Location);
			if (!runner.Load (package)) {
				Console.WriteLine ("Could not find the tests.");
				return -1;
			}

			var listener = new CommandLineTestEventListener(runOptions);
			var filter = new AggregateTestFilter (runOptions.Filters);
			var results = runner.Run (listener, filter, false, LoggingThreshold.Off);
		    return results.IsFailure ? 1 : 0;
		}

		private class CommandLineTestEventListener : TestEventListenerBase {
			private readonly RunOptions _runOptions;
			public CommandLineTestEventListener (RunOptions runOptions)
			{
				if (runOptions == null)
					throw new ArgumentNullException("runOptions");
				_runOptions = runOptions;
			}

			public override void RunFinished(TestResult result)
			{
				base.RunFinished(result);

				var resultWriter = new XmlResultWriter (_runOptions.XmlResultsPath);
				resultWriter.SaveTestResult (result);

				if (_runOptions.PerformXslTransform) {
					var transform = new XslTransform ();
					transform.Load (_runOptions.XslTransformPath);
					transform.Transform (_runOptions.XmlResultsPath, _runOptions.TransformedResultsPath);
				}

				File.WriteAllText (_runOptions.StdoutPath, StdoutStandin.ToString ());

				if (_runOptions.PerformXslTransform && _runOptions.ShouldLaunchResults)
					System.Diagnostics.Process.Start (_runOptions.TransformedResultsPath);
			}

			public override void RunFinished(Exception exception)
			{
				base.RunFinished(exception);
			}
		}
	}
}
