// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NUnitLite;
[assembly: Apartment(ApartmentState.STA)]

// NOTE: This is just here to make older versions of 
// Resharper's Unit Test Runner work.  Can be removed later.
[SetUpFixture]
class AssemblyConfig
{
    [OneTimeSetUp]
    public static void Setup()
    {
        var assemblyPath = typeof(AssemblyConfig).Assembly.Location;
        var workingDir = Path.GetDirectoryName(assemblyPath);
        Directory.SetCurrentDirectory(workingDir);
    }
}

namespace MonoGame.Tests
{
    static class Program
    {
        [STAThread]
        public static int Main(string [] args)
        {
            return new AutoRun().Execute(args);
        }
    }
}