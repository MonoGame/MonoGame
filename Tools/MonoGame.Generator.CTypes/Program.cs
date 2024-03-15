using System.Reflection;
using System.Text;

static string GetCEnumType(string cstype)
{
    return cstype switch
    {
        "System.Byte" => "csbyte",
        "System.Int16" => "csshort",
        "System.UInt16" => "csushort",
        "System.Int32" => "csint",
        "System.UInt32" => "csuint",
        "System.Int64" => "cslong",
        "System.UInt64" => "csulong",
        _ => "CS" + cstype
    };
};

var repoDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../");
var monogamePlatformDir = Path.Combine(repoDirectory, "src/monogame/include");
var monogameFrameworkPath = Path.Combine(repoDirectory, "Artifacts/MonoGame.Framework/Native/Debug/MonoGame.Framework.dll");
var assembly = Assembly.LoadFile(monogameFrameworkPath);
var outputText = new StringBuilder();
var duplicateChecker = new Dictionary<string, string>();

outputText.AppendLine($"""
//
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes
//

#pragma once

#include "csharp_common.h"

""");

foreach (var enumType in assembly.GetTypes())
{
    if (!enumType.IsEnum)
        continue;

    if (enumType.IsNested)
        continue;

    if (!enumType.FullName!.StartsWith("MonoGame") && !enumType.FullName!.StartsWith("Microsoft.Xna.Framework"))
        continue;

    if (duplicateChecker.TryGetValue(enumType.Name, out string? dupFullName))
    {
        Console.WriteLine($"""
        WARNING: Duplicate enum name for {enumType.Name}:
        - {enumType.FullName}
        - {dupFullName}

        """);
        continue;
    }

    var enumValues = Enum.GetValues(enumType);

    // Write all values to output
    outputText.AppendLine($$"""
    enum CS{{enumType.Name}} : {{GetCEnumType(Enum.GetUnderlyingType(enumType).ToString())}}
    {
    """);
    foreach (var enumValue in enumValues)
    {
        outputText.AppendLine($"    {enumValue} = {((Enum)enumValue).ToString("d")},");
    }
    outputText.AppendLine("""
    };

    """);

    outputText.AppendLine($$"""
    class ECS{{enumType.Name}}
    {
    public:
        static const char* ToString(CS{{enumType.Name}} enumValue)
        {
            switch (enumValue)
            {
    """);
    foreach (var enumValue in enumValues)
    {
        outputText.AppendLine($"            case {enumValue}: return \"{enumValue}\";");
    }
    outputText.AppendLine("""
            }

            return "Unknown Value";
        }
    };

    """);

    duplicateChecker.Add(enumType.Name, enumType.FullName!);
}

if (!Directory.Exists(monogamePlatformDir))
    Directory.CreateDirectory(monogamePlatformDir);

File.WriteAllText(Path.Combine(monogamePlatformDir, "csharp_enums.h"), outputText.ToString());
