using System.Text;

namespace MonoGame.Generator.CTypes;

class EnumWritter
{
    private readonly StringBuilder _outputText;
    private readonly Dictionary<string, string> _duplicateChecker;

    public EnumWritter()
    {
        _outputText = new StringBuilder($"""
        //
        // This code is auto generated, don't modify it by hand.
        // To regenerate it run: Tools/MonoGame.Generator.CTypes
        //

        #pragma once

        #include "csharp_common.h"


        """);
        _duplicateChecker = [];
    }

    public static bool IsValid(Type type)
    {
        return type.IsEnum && !type.IsNested;
    }

    public bool Append(Type type)
    {
        if (!IsValid(type))
            return false;

        if (_duplicateChecker.TryGetValue(type.Name, out string? dupFullName))
        {
            if (type.FullName != type.FullName)
            {
                Console.WriteLine($"""
                WARNING: Duplicate enum name for {type.Name}:
                - {type.FullName}
                - {dupFullName}

                """);
            }

            return false;
        }
        
        var enumValues = Enum.GetValues(type);

        // Write all values to output
        _outputText.AppendLine($$"""
        enum CS{{type.Name}} : {{Util.GetCEnumType(Enum.GetUnderlyingType(type).ToString())}}
        {
        """);
        foreach (var enumValue in enumValues)
        {
            _outputText.AppendLine($"    {enumValue} = {((Enum)enumValue).ToString("d")},");
        }
        _outputText.AppendLine("""
        };

        """);

        _outputText.AppendLine($$"""
        class ECS{{type.Name}}
        {
        public:
            static const char* ToString(CS{{type.Name}} enumValue)
            {
                switch (enumValue)
                {
        """);
        foreach (var enumValue in enumValues)
        {
            _outputText.AppendLine($"            case {enumValue}: return \"{enumValue}\";");
        }
        _outputText.AppendLine("""
                }

                return "Unknown Value";
            }
        };

        """);

        _duplicateChecker.Add(type.Name, type.FullName!);
        return true;
    }

    public void Flush(string dirPath) => File.WriteAllText(Path.Combine(dirPath, "csharp_enums.h"), _outputText.ToString());
}
