// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.ComponentModel;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.CompilerServices;

namespace MonoGame.Generator.CTypes;

class PinvokeWritter
{
    private readonly string _name;

    private readonly StringBuilder _outputText;

    private readonly Dictionary<string, Type> _handles;

    private readonly List<MethodInfo> _methods;

    private EnumWritter _enumWritter;

    private StructWritter _structWritter;

    public PinvokeWritter(Type type, StructWritter structWritter, EnumWritter enumWritter)
    {
        _name = type.Name;
        _enumWritter = enumWritter;
        _structWritter = structWritter;

        _outputText = new StringBuilder($"""
        // MonoGame - Copyright (C) The MonoGame Team
        // This file is subject to the terms and conditions defined in
        // file 'LICENSE.txt', which is part of this source code package.
                        
        // This code is auto generated, don't modify it by hand.
        // To regenerate it run: Tools/MonoGame.Generator.CTypes

        #pragma once

        #include "api_common.h"
        #include "api_enums.h"
        #include "api_structs.h"
        

        
        """)
        {

        };
        _handles = [];
        _methods = [];
    }

    private static bool IsHandle(Type type)
    {
        if (!type.IsPointer)
            return false;

        type = type.GetElementType();

        return type.IsMGHandle();
    }

    private bool TryAppendHandle(Type type)
    {
        if (!IsHandle(type))
            return false;

        if (_handles.ContainsKey(type.Name))
            return false;

        _handles.Add(type.Name, type);

        return true;
    }

    public bool Append(MethodInfo method)
    {
        if (!method.IsStatic)
            return false;

        var import = method.GetCustomAttribute<LibraryImportAttribute>();
        if (import == null)
            return false;

        TryAppendHandle(method.ReturnType);
        _enumWritter.TryAppend(method.ReturnType);
        _structWritter.TryAppend(method.ReturnType);

        foreach (var arg in method.GetParameters())
        {
            TryAppendHandle(arg.ParameterType);
            _enumWritter.TryAppend(arg.ParameterType);
            _structWritter.TryAppend(arg.ParameterType);
        }

        _methods.Add(method);
        return true;
    }

    private void GenerateHandle(Type type)
    {
        type = type.GetElementType();
        _outputText.AppendLine($"struct {type.Name};");
    }

    private void GenerateMethod(MethodInfo method)
    {
        var import = method.GetCustomAttribute<LibraryImportAttribute>();

        var rtype = Util.GetCTypeOrEnum(method.ReturnType);
        var fname = import.EntryPoint ?? method.Name;

        var arguments = new List<string>();
        foreach (var arg in method.GetParameters())
            arguments.Add(Util.GetCType(arg));

        _outputText.AppendLine($"MG_EXPORT {rtype} {fname}({string.Join(", ", arguments)});");
    }

    public void Flush(string dirPath)
    {
        // Write the handles.
        foreach (var pair in _handles)
            GenerateHandle(pair.Value);

        _outputText.AppendLine();

        // Write the pinvokes.
        foreach (var method in _methods)
            GenerateMethod(method);

        var path = Path.Combine(dirPath, $"api_{_name}.h");
        var text = _outputText.ToString().ReplaceLineEndings();

        File.WriteAllText(path, text);
    }
}
