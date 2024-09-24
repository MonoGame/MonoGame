// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoGame.Generator.CTypes;

class StructWritter
{
    private readonly Dictionary<string, Type> _types;
    private readonly StringBuilder _outputText;
    private readonly EnumWritter _enumWritter;

    public StructWritter(EnumWritter enumWritter)
    {
        _enumWritter = enumWritter;

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
        if (!type.IsValueType)
            return false;

        if (type.IsEnum)
            return false;

        if (type.IsNested)
            return false;

        if (type.FullName.StartsWith("System."))
            return false;

        return true;
    }

    public bool TryAppend(Type type)
    {
        if (type.ToString().Contains("MGG_InputElement"))
            type = type;

        if (type.IsArray)
            type = type.GetElementType();

        if (type.IsPointer || type.IsByRef)
        {
            type = type.GetElementType();

            // Our handle types don't need to be exported.
            if (type.IsMGHandle())
                return false;            
        }

        if (!IsValid(type))
            return false;

        if (_types.ContainsKey(type.Name))
            return true;

        foreach (var field in type.GetFields())
        {
            TryAppend(field.FieldType);
            _enumWritter.TryAppend(field.FieldType);
        }

        _types.Add(type.Name, type);
        return true;
    }

    private void Generate(Type type)
    {
        var explicitLayout = type.IsExplicitLayout;

        if (explicitLayout)
            _outputText.AppendLine("#pragma pack(push,1)");

        _outputText.AppendLine($$"""
        struct {{type.Name}}
        {
        """);

        int soffset = 0;

        if (explicitLayout)
            _outputText.AppendLine("union {");

        // We export all public and private fields skipping statics.
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (var field in type.GetFields(flags))
        {
            var ftype = Util.GetCTypeOrEnum(field.FieldType);

            if (explicitLayout)
            {
                var offset = field.GetFieldOffset();
                if (offset != 0)
                    _outputText.AppendLine($"    MG_FIELD_OFFSET({offset}, {ftype}, {field.Name});");
                else
                    _outputText.AppendLine($"    {ftype} {field.Name};");
            }
            else
                _outputText.AppendLine($"    {ftype} {field.Name};");
        }

        if (explicitLayout)
            _outputText.AppendLine("};");

        _outputText.AppendLine($$"""
        };
        """);

        if (explicitLayout)
            _outputText.AppendLine("#pragma pack(pop)");

        _outputText.AppendLine();
    }

    public void Flush(string dirPath)
    {
        foreach (var pair in _types)
            Generate(pair.Value);

        var path = Path.Combine(dirPath, "api_structs.h");
        var text = _outputText.ToString().ReplaceLineEndings();

        File.WriteAllText(path, text);
    }

}
