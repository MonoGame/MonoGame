// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Creates a <see cref="InteractiveTest"/> from applicable types in our binary.
    /// Also allows filtering of tests based on platforms/command-line args.
    /// </summary>
    public class InteractiveTests
    {
        private readonly List<InteractiveTest> _interactiveTests = new();

        private readonly List<InteractiveTest> _filteredTests = new();

        public InteractiveTests()
        {
            _interactiveTests.Clear();
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                InteractiveTest test;
                if (!InteractiveTest.TryCreateFrom(type, out test)) { continue; }

                if (test.MatchesPlatform(PlatformInfo.MonoGamePlatform)) { _interactiveTests.Add(test); }
            }

            GameDebug.LogInfo($"--Discovered {_interactiveTests.Count} tests.");

            // Also turn on GraphicsDebug messages.
            GraphicsDebug.EnableOutputDebugMessagesToConsole();
        }

        public IReadOnlyList<InteractiveTest> Tests { get { return _interactiveTests; } }

        /// <summary>
        /// Parses the passed-in arg and returns an `InteractiveTest` game. See HelpStr for more details.
        /// </summary>
        public IReadOnlyList<InteractiveTest> Parse(string[] args)
        {
            _filteredTests.Clear();
            if (args == null || args.Length == 0) { return _filteredTests; }

            foreach (var test in _interactiveTests)
            {
                foreach (var testName in args)
                {
                    var name = testName.ToLower();
                    if (test.Category.ToLower().Contains(name)) { _filteredTests.Add(test); }
                    else if (test.Name.ToLower().Contains(name)) { _filteredTests.Add(test); }
                }
            }

            return _filteredTests;
        }

        public string HelpStr()
        {
            var testStr = "";
            foreach (var test in _interactiveTests) { testStr += $"{test.Name}\n"; }

            return $@"Interactive tests available:
{testStr}
";
        }
    }
}
