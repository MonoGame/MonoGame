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
	class CommandLineInterface : EventListener
	{
		public static void RunMain(string [] args)
		{
			CoreExtensions.Host.InitializeService ();

			var assembly = Assembly.GetExecutingAssembly ();

			var simpleTestRunner = new SimpleTestRunner ();
			TestPackage package = new TestPackage (assembly.GetName ().Name);
			package.Assemblies.Add (assembly.Location);
			if (!simpleTestRunner.Load (package)) {
				Console.WriteLine ("Could not find the tests.");
				return;
			}

			var result = simpleTestRunner.Run (new CommandLineInterface ());

			string outputPath = Path.Combine (Directory.GetCurrentDirectory (), "test_results.xml");
			var resultWriter = new XmlResultWriter (outputPath);
			resultWriter.SaveTestResult (result);

			var htmlOutputPath = Path.Combine (Directory.GetCurrentDirectory (), "test_results.html");
			var transform = new XslTransform();
			transform.Load(Path.Combine("Resources", "tests.xsl"));
			transform.Transform (outputPath, htmlOutputPath);

			System.Diagnostics.Process.Start (htmlOutputPath);
		}

		private TextWriter stdout_standin;
		private StreamWriter stdout;
		private CommandLineInterface ()
		{
			stdout_standin = new StringWriter ();
			Console.SetOut (stdout_standin);
			stdout = new StreamWriter (Console.OpenStandardOutput ());
			stdout.AutoFlush = true;
		}

		public void RunStarted (string name, int testCount)
		{
			stdout.WriteLine("Run Started: {0}", name);
		}

		public void RunFinished (Exception exception)
		{
			// Console.WriteLine("RunFinished error");
		}

		public void RunFinished (TestResult result)
		{
			// Console.WriteLine("RunFinished success");
		}

		public void SuiteFinished (TestResult result)
		{
			// Console.WriteLine("SuiteFinished");
		}

		public void SuiteStarted (TestName testName)
		{
			// Console.WriteLine("SuiteStarted");
		}

		public void TestStarted (TestName testName)
		{
			// Console.WriteLine("Test {0}", testName.FullName);
		}

		public void TestFinished (TestResult result)
		{
			char output;
			switch (result.ResultState) {
			case ResultState.Cancelled:
				output = 'C';
				break;
			case ResultState.Error:
				output = 'E';
				break;
			case ResultState.Failure:
				output = 'F';
				break;
			case ResultState.Ignored:
				output = 'I';
				break;
			case ResultState.Inconclusive:
				output = '?';
				break;
			case ResultState.NotRunnable:
				output = 'N';
				break;
			case ResultState.Skipped:
				output = 'S';
				break;
			default:
			case ResultState.Success:
				output = '.';
				break;
			}

			stdout.Write (output);
		}

		public void TestOutput (TestOutput testOutput)
		{
			// Console.WriteLine("TestOutput");
		}

		public void UnhandledException (Exception exception)
		{
			// Console.WriteLine("UnhandledException");
		}
	}
}
