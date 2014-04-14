using System;
using System.IO;
using NUnit.Core;

namespace MonoGame.Tests {
	public class TestEventListenerBase : EventListener {
		private readonly TextWriter _stdoutStandin;
		protected TextWriter StdoutStandin { get { return _stdoutStandin; } }

		private readonly StreamWriter _stdout;

		public TestEventListenerBase ()
		{
			_stdoutStandin = new StringWriter ();
			Console.SetOut (_stdoutStandin);
			_stdout = new StreamWriter (Console.OpenStandardOutput ());
			_stdout.AutoFlush = true;
		}

		public virtual void RunStarted (string name, int testCount)
		{
			_stdout.WriteLine("Run Started: {0}", name);
		}

		public virtual void RunFinished (Exception exception)
		{
			_stdout.WriteLine ();
		}

		public virtual void RunFinished (TestResult result)
		{
			_stdout.WriteLine ();
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
			_stdoutStandin.WriteLine(testName.FullName);
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

			_stdout.Write (output);

			_stdoutStandin.WriteLine("Finished: " + result.FullName);
			_stdoutStandin.WriteLine();
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

