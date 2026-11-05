// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGame.InteractiveTests;

internal static class TestRunner
{
    private static void Main(string[] args)
    {
        InteractiveTests tests = new InteractiveTests();
        Console.WriteLine($"Provided options : {string.Join(' ', args)}; {tests.HelpStr()}");

        IReadOnlyList<InteractiveTest> filteredTests = tests.Parse(args);
        if (filteredTests.Count == 0)
        {
            return;
        }

        InteractiveTest test = filteredTests[0];
        TestGame testGame = test.Create() as TestGame;
        if (testGame is null)
        {
            return;
        }

        using (testGame)
        {
            Console.WriteLine($"--Running {test.Name}");
            testGame.Exiting += OnTestGameExiting;
            testGame.Run();
            Console.WriteLine($"--Finished {test.Name}");
        }
    }

    private static void OnTestGameExiting(object sender, ExitingEventArgs eventArgs)
    {
        if (sender is Game game)
        {
            game.Exit();
        }
    }
}
