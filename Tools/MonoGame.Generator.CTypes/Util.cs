// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoGame.Generator.CTypes;

static class Util
{
    public static string GetCType(Type cstype) => cstype.ToString() switch
    {
        "System.Byte" => "mgbyte",
        "System.Int16" => "mgshort",
        "System.UInt16" => "mgushort",
        "System.Int32" => "mgint",
        "System.UInt32" => "mguint",
        "System.Int64" => "mglong",
        "System.UInt64" => "mgulong",
        "System.Void" => "void",
        "System.Boolean" => "mgbool",
        "System.Single" => "mgfloat",
        "System.Double" => "mgdouble",
        "System.IntPtr" => "void*",
        "System.UIntPtr" => "void*",
        "System.Char" => "mgchar",
        "System.String" => "const char*",
        _ => cstype.Name.ToString()
    };

    public static string GetCTypeOrEnum(Type type, string functionPointerName = null)
    {
        if (type.IsArray)
            return GetCTypeOrEnum(type.GetElementType()) + "*";

        if (type.IsByRef)
            return GetCTypeOrEnum(type.GetElementType()) + "&";

        if (type.IsPointer)
            return GetCTypeOrEnum(type.GetElementType()) + "*";

        if (type.IsEnum)
            return $"MG{type.Name}";

        return GetCType(type);
    }

    public static string GetCType(ParameterInfo param)
    {
        var type = param.ParameterType;

        if (type.FullName.Contains("String[]"))
            type = type;

            var result = new StringBuilder();

        if (type.BaseType != null && type.BaseType.FullName == "System.MulticastDelegate")
        {
            var invokeMethod = type.GetMethod("Invoke");

            result.Append("void (*");
            result.Append(param.Name);
            result.Append(")(");

            var args = new List<string>();
            foreach (var arg in invokeMethod.GetParameters())
                args.Add(GetCTypeOrEnum(arg.ParameterType));

            result.Append(string.Join(",", args));
            result.Append(")");
        }
        else if (type.IsFunctionPointer)
        {
            result.Append(GetCTypeOrEnum(type.GetFunctionPointerReturnType()));
            result.Append(" (*");
            result.Append(param.Name);
            result.Append(")(");

            var args = new List<string>();
            foreach (var arg in type.GetFunctionPointerParameterTypes())
                args.Add(GetCTypeOrEnum(arg));

            result.Append(string.Join(",", args));
            result.Append(")");
        }
        else
        {
            result.Append(GetCTypeOrEnum(type));
            result.Append(" ");
            result.Append(param.Name);
        }

        return result.ToString();
    }


    public static bool IsMGHandle(this Type type)
    {
        foreach (var obj in type.GetCustomAttributes(false))
        {
            var atype = obj.GetType();
            if (atype.FullName == "MonoGame.Interop.MGHandleAttribute")
                return true;
        }

        return false;
    }

    public static int GetFieldOffset(this FieldInfo field)
    {
        var att = field.GetCustomAttribute<FieldOffsetAttribute>();
        return att.Value;
    }

    public static int SizeOf(Type type)
    {
        // Inefficienct but works for our generator needs.

        var method = new DynamicMethod("SizeOfImpl", typeof(uint), new Type[0], typeof(Util), false);
        ILGenerator gen = method.GetILGenerator();
        gen.Emit(OpCodes.Sizeof, type);
        gen.Emit(OpCodes.Ret);
        var func = (Func<uint>)method.CreateDelegate(typeof(Func<uint>));

        return checked((int)func());
    }
}
