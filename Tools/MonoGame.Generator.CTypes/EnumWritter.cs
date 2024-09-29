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
        // MonoGame - Copyright (C) The MonoGame Team
        // This file is subject to the terms and conditions defined in
        // file 'LICENSE.txt', which is part of this source code package.

        // This code is auto generated, don't modify it by hand.
        // To regenerate it run: Tools/MonoGame.Generator.CTypes

        #pragma once

        #include "api_common.h"


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
        var enumNames = Enum.GetNames(type);
        var enumValues = Enum.GetValues(type);

        var name = Util.GetCTypeOrEnum(type);

        // Write all values to output
        _outputText.AppendLine($$"""
        enum class {{name}} : {{Util.GetCType(Enum.GetUnderlyingType(type))}}
        {
        """);
        for (int i = 0; i < enumNames.Length; i++)
        {
            var enumValue = (Enum)enumValues.GetValue(i);
            _outputText.AppendLine($"    {enumNames[i]} = {enumValue.ToString("d")},");
        }
        _outputText.AppendLine("""
        };

        """);

        // TODO: This causes issues with enums that have repeat
        // values causing the `switch` to not work.
        //
        // We don't use this feature now anyway.
        /*
        _outputText.AppendLine($$"""
        static const char* {{name}}_ToString({{name}} enumValue)
        {
            switch (enumValue)
            {
        """);
        for (int i = 0; i < enumNames.Length; i++)
        {
            _outputText.AppendLine($"        case {name}::{enumNames[i]}: return \"{enumNames[i]}\";");
        }
        _outputText.AppendLine("""
            }

            return "Unknown Value";
        }

        """);
        */
    }

    public void Flush(string dirPath)
    {
        foreach (var pair in _types)
            Generate(pair.Value);

        var path = Path.Combine(dirPath, "api_enums.h");
        var text = _outputText.ToString().ReplaceLineEndings();

        File.WriteAllText(path, text);
    }

}
