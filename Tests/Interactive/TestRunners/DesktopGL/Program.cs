// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.InteractiveTests
{
    class TestRunner
    {
        /// <summary>
        /// Runs a specific <see cref="InteractiveTest"/> instance specified from the
        /// command line. If multiple tests match, the first one is run.
        /// Invoking without any command-line args shows all the discovered tests.
        /// </summary>
        static void Main(string[] args)
        {
            var tests = new InteractiveTests();
            Console.WriteLine($"Provided options : {string.Join(' ', args)}; {tests.HelpStr()}");
            var filteredTests = tests.Parse(args);
            if (filteredTests.Count > 0)
            {
                var test = filteredTests[0];
                var testGame = test.Create() as TestGame;
                if (testGame != null)
                {
                    Console.WriteLine($"--Running {test.Name}");
                    testGame.Exiting += (o, e) => { testGame.Exit(); };
                    testGame.Run();
                    Console.WriteLine($"--Finished {test.Name}");
                }

                // We currently support running exactly one test per run.
                // This is due to MonoGame limitation of creating a single SDL thread/Window.
            }
        }
    }
}
