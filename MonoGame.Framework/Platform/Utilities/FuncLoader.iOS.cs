using System;
using System.Runtime.InteropServices;
using ObjCRuntime;

namespace MonoGame.Framework.Utilities
{
    internal class FuncLoader
    {
        public static IntPtr LoadLibrary(string libname)
        {
            return Dlfcn.dlopen(libname, 0);
        }

        public static T LoadFunction<T>(IntPtr library, string function, bool throwIfNotFound = false)
        {
            var ret = Dlfcn.dlsym(library, function);

            if (ret == IntPtr.Zero)
            {
                if (throwIfNotFound)
                    throw new EntryPointNotFoundException(function);

                return default(T);
            }

            // TODO: Use the function bellow once Protobuild gets axed
            // requires .NET Framework 4.5.1 and its useful for corert
            // return Marshal.GetDelegateForFunctionPointer<T>(ret);

            return (T)(object)Marshal.GetDelegateForFunctionPointer(ret, typeof(T));
        }
    }
}
