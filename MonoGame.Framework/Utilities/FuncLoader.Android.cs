using System;
using System.Runtime.InteropServices;

namespace MonoGame.Utilities
{
    internal class FuncLoader
    {
        [DllImport("dl")]
        public static extern IntPtr dlopen(string path, int flags);

        [DllImport("dl")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);

        private const int RTLD_LAZY = 0x0001;

        public static IntPtr LoadLibrary(string libname)
        {
            return dlopen(libname, RTLD_LAZY);
        }

        public static T LoadFunction<T>(IntPtr library, string function, bool throwIfNotFound = false)
        {
            var ret = dlsym(library, function);

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
