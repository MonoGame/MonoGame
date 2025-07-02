// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Reflection;
using MonoGame.Generator.CTypes;

void GenerateCTypeBindings(string repoDirectory, string outputDirectory, string assemblyPath, string namespacePrefix)
{
    var monogamePlatformDir = Path.Combine(repoDirectory, outputDirectory);
    var monogameFrameworkPath = Path.Combine(repoDirectory, assemblyPath);

    AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
    {
        var folder = Path.GetDirectoryName(monogameFrameworkPath);
        var name = new AssemblyName(args.Name).Name + ".dll";
        var candidate = Path.Combine(folder, name);
        if (File.Exists(candidate))
            return Assembly.LoadFrom(candidate);
        return null;
    };

    var assembly = Assembly.LoadFile(monogameFrameworkPath);
    var enumWritter = new EnumWritter();
    var structWritter = new StructWritter(enumWritter);

    if (!Directory.Exists(monogamePlatformDir))
        Directory.CreateDirectory(monogamePlatformDir);

    try
    {
        foreach (var type in assembly.GetTypes())
        {
            // Look for our interop types.
            if (!type.FullName!.StartsWith(namespacePrefix))
                continue;

            // All our interop pinvokes are static classes.
            if (!type.IsClass || !type.IsAbstract || !type.IsSealed)
                continue;

            var writter = new PinvokeWritter(type, structWritter, enumWritter);
            foreach (var method in type.GetMethods())
                writter.Append(method);

            writter.Flush(monogamePlatformDir);
        }
    }
    catch (ReflectionTypeLoadException ex)
    {
        Console.WriteLine($"ReflectionTypeLoadException: {ex.Message}");
        foreach (var loaderEx in ex.LoaderExceptions)
            Console.WriteLine(loaderEx?.ToString());
        throw;
    }

    enumWritter.Flush(monogamePlatformDir);
    structWritter.Flush(monogamePlatformDir);
}

var repoDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../");
GenerateCTypeBindings(
    repoDirectory,
    "native/monogame/include",
    "Artifacts/MonoGame.Framework/Native/Debug/MonoGame.Framework.dll",
    "MonoGame.Interop.");

GenerateCTypeBindings(
    repoDirectory,
    "native/pipeline/include",
    "Artifacts/MonoGame.Framework.Content.Pipeline/Debug/MonoGame.Framework.Content.Pipeline.dll",
    "MonoGame.Framework.Content.Pipeline.Interop.");
