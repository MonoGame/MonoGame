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

            return Marshal.GetDelegateForFunctionPointer<T>(ret);
        }
    }
}
