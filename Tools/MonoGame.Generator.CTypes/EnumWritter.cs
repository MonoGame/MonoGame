// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;

namespace MonoGame.Generator.CTypes;

class EnumWritter
{
    private readonly Dictionary<string, Type> _types;
    private readonly StringBuilder _outputText;

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

        _types = [];
    }

    private static bool IsValid(Type type)
    {
        return type.IsEnum && !type.IsNested;
    }

    public bool TryAppend(Type type)
    {
        if (type.IsByRef)
            type = type.GetElementType();

        if (type.IsPointer)
            type = type.GetElementType();

        if (!IsValid(type))
            return false;

        if (_types.ContainsKey(type.Name))
            return true;

        _types.Add(type.Name, type);
        return true;
    }

    private void Generate(Type type)
    {
        var enumValues = Enum.GetValues(type);

        var name = Util.GetCTypeOrEnum(type);

        // Write all values to output
        _outputText.AppendLine($$"""
        enum class {{name}} : {{Util.GetCType(Enum.GetUnderlyingType(type))}}
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
        static const char* {{name}}_ToString({{name}} enumValue)
        {
            switch (enumValue)
            {
        """);
        foreach (var enumValue in enumValues)
        {
            _outputText.AppendLine($"        case {name}::{enumValue}: return \"{enumValue}\";");
        }
        _outputText.AppendLine("""
            }

            return "Unknown Value";
        }

        """);
    }

    public void Flush(string dirPath)
    {
        foreach (var pair in _types)
            Generate(pair.Value);

        var path = Path.Combine(dirPath, "csharp_enums.h");
        var text = _outputText.ToString().ReplaceLineEndings();

        File.WriteAllText(path, text);
    }

}
