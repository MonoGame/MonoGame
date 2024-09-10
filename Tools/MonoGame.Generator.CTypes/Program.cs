// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Reflection;
using MonoGame.Generator.CTypes;

var repoDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../");
var monogamePlatformDir = Path.Combine(repoDirectory, "native/monogame/include");
var monogameFrameworkPath = Path.Combine(repoDirectory, "Artifacts/MonoGame.Framework/Native/Debug/MonoGame.Framework.dll");
var assembly = Assembly.LoadFile(monogameFrameworkPath);
var enumWritter = new EnumWritter();
var structWritter = new StructWritter(enumWritter);

if (!Directory.Exists(monogamePlatformDir))
    Directory.CreateDirectory(monogamePlatformDir);

foreach (var type in assembly.GetTypes())
{
    // Look for our interop types.
    if (!type.FullName!.StartsWith("MonoGame.Interop."))
        continue;

    // All our interop pinvokes are static classes.
    if (!type.IsClass || !type.IsAbstract || !type.IsSealed)
        continue;

    var writter = new PinvokeWritter(type, structWritter, enumWritter);
    foreach (var method in type.GetMethods())
        writter.Append(method);

    writter.Flush(monogamePlatformDir);
}

enumWritter.Flush(monogamePlatformDir);
structWritter.Flush(monogamePlatformDir);
