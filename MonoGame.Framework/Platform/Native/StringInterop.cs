// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MonoGame.Interop;

/// <summary>
/// Helpers for string interop in the new native backend.
/// </summary>
internal unsafe static class StringInterop
{
    /// <summary>
    /// Returns a quick safe over estimated size of a final UTF8 string for stackalloc.
    /// </summary>
    static public int GetMaxSize(string str)
    {
        return (str.Length * 4) + 1;
    }

    /// <summary>
    /// Returns a quick safe over estimated size of a final UTF8 string for stackalloc to fit a collection of strings.
    /// </summary>
    static public int GetMaxSize(IEnumerable<string> strs)
    {
        int size = 0;
        foreach (var s in strs)
            size += (s.Length * 4) + 1;
        return size;
    }

    /// <summary>
    /// Makes a copy of the source string into the destination UTF8 native string.
    /// </summary>
    /// <returns>The count of bytes copied to the destination UTF8 native string.</returns>
    static public int CopyString(byte* dest, string source)
    {
        int count;
        fixed (char* s = source)
        {
            count = Encoding.UTF8.GetBytes(s, source.Length, dest, source.Length * 4);
            dest[count] = 0;
        }

        return count;
    }

    /// <summary>
    /// Makes a copy of the source string collection into a single destination null delimited UTF8 native string.
    /// </summary>
    /// <returns>The total count of bytes copied to the destination UTF8 native string.</returns>
    static public int CopyStrings(byte* dest, IEnumerable<string> source)
    {
        int count = 0;

        foreach (var str in source)
        {
            fixed (char* s = str)
            {
                count = Encoding.UTF8.GetBytes(s, str.Length, dest, str.Length * 4);
                dest[count] = 0;
                dest += count;
            }
        }

        return count;
    }

    /// <summary>
    /// Returns a C# string from a UTF8 null terminated native string
    /// </summary>
    static public string ToString(byte* str)
    {
        return Marshal.PtrToStringUTF8((IntPtr)str);
    }

    /// <summary>
    /// Frees a string allocated within the native backend.
    /// </summary>
    /// <param name="str"></param>
    static public void Free(byte* str)
    {
        Marshal.FreeHGlobal((IntPtr)str);
    }
}
