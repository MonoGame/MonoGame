using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Utilities
{
    internal class FuncLoader
    {
        private class Windows
        {
            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibraryW(string lpszLib);
        }

        private class Linux
        {
            [DllImport("libdl.so.2")]
            public static extern IntPtr dlopen(string path, int flags);

            [DllImport("libdl.so.2")]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
        }

        private class OSX
        {
            [DllImport("/usr/lib/libSystem.dylib")]
            public static extern IntPtr dlopen(string path, int flags);

            [DllImport("/usr/lib/libSystem.dylib")]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
        }

        private const int RTLD_LAZY = 0x0001;

        public static IntPtr LoadLibraryExt(string libname)
        {
            var ret = IntPtr.Zero;
            var assemblyLocation = Path.GetDirectoryName(System.AppContext.BaseDirectory) ?? "./";

            // Try .NET Framework / mono locations
            if (CurrentPlatform.OS == OS.MacOSX)
            {
                ret = LoadLibrary(Path.Combine(assemblyLocation, libname));

                // Look in Frameworks for .app bundles
                if (ret == IntPtr.Zero)
                    ret = LoadLibrary(Path.Combine(assemblyLocation, "..", "Frameworks", libname));
            }
            else
            {
                if (Environment.Is64BitProcess)
                    ret = LoadLibrary(Path.Combine(assemblyLocation, "x64", libname));
                else
                    ret = LoadLibrary(Path.Combine(assemblyLocation, "x86", libname));
            }

            // Try .NET Core development locations
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(Path.Combine(assemblyLocation, "runtimes", CurrentPlatform.Rid, "native", libname));

            // Try current folder (.NET Core will copy it there after publish)
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(Path.Combine(assemblyLocation, libname));

            // Try alternate way of checking current folder
            // assemblyLocation is null if we are inside macOS app bundle
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, libname));

            // Try loading system library
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(libname);

            // Welp, all failed, PANIC!!!
            if (ret == IntPtr.Zero)
                throw new Exception("Failed to load library: " + libname);

            return ret;
        }

        public static IntPtr LoadLibrary(string libname)
        {
            if (CurrentPlatform.OS == OS.Windows)
                return Windows.LoadLibraryW(libname);

            if (CurrentPlatform.OS == OS.MacOSX)
                return OSX.dlopen(libname, RTLD_LAZY);

            if (CurrentPlatform.OS == OS.Browser)
                return IntPtr.Zero;

            return Linux.dlopen(libname, RTLD_LAZY);
        }

        public static T LoadFunction<T>(IntPtr library, string function, bool throwIfNotFound = false)
        {
            var ret = IntPtr.Zero;

            if (CurrentPlatform.OS == OS.Windows)
                ret = Windows.GetProcAddress(library, function);
            else if (CurrentPlatform.OS == OS.MacOSX)
                ret = OSX.dlsym(library, function);
            else if (CurrentPlatform.OS==OS.Browser)
                ret = GetBrowserFunc(function);
            else
                ret = Linux.dlsym(library, function);

            if (ret == IntPtr.Zero)
            {
                if (throwIfNotFound)
                    throw new EntryPointNotFoundException(function);

                return default;
            }

#if NETSTANDARD
            return Marshal.GetDelegateForFunctionPointer<T>(ret);
#else
            return (T)(object)Marshal.GetDelegateForFunctionPointer(ret, typeof(T));
#endif
        }

        static IntPtr GetBrowserFunc(string function)
        {
            var ptr = function switch
            {
                "SDL_Init" => Marshal.GetFunctionPointerForDelegate(Sdl.Init_Import),
                "SDL_DisableScreenSaver" => Marshal.GetFunctionPointerForDelegate(Sdl.DisableScreenSaver_Import),
                "SDL_GetVersion" => Marshal.GetFunctionPointerForDelegate(Sdl.GetVersion_Import),
                "SDL_PollEvent" => Marshal.GetFunctionPointerForDelegate(Sdl.PollEvent_Import),
                "SDL_PumpEvents" => Marshal.GetFunctionPointerForDelegate(Sdl.PumpEvents_Import),
                "SDL_CreateRGBSurfaceFrom" => Marshal.GetFunctionPointerForDelegate(Sdl.CreateRGBSurfaceFrom_Import),
                "SDL_FreeSurface" => Marshal.GetFunctionPointerForDelegate(Sdl.FreeSurface_Import),
                "SDL_GetError" => Marshal.GetFunctionPointerForDelegate(Sdl.GetError_Import),
                "SDL_ClearError" => Marshal.GetFunctionPointerForDelegate(Sdl.ClearError_Import),
                "SDL_GetHint" => Marshal.GetFunctionPointerForDelegate(Sdl.GetHint_Import),
                "SDL_LoadBMP_RW" => Marshal.GetFunctionPointerForDelegate(Sdl.LoadBMP_RW_Import),
                "SDL_Quit" => Marshal.GetFunctionPointerForDelegate(Sdl.Quit_Import),
                "SDL_RWFromMem" => Marshal.GetFunctionPointerForDelegate(Sdl.RWFromMem_Import),
                "SDL_SetHint" => Marshal.GetFunctionPointerForDelegate(Sdl.SetHint_Import),
                "SDL_CreateWindow" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.CreateWindow_Import),
                "SDL_DestroyWindow" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.DestroyWindow_Import),
                "SDL_GetWindowID" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.GetWindowID_Import),
                "SDL_GetWindowDisplayIndex" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.GetWindowDisplayIndex_Import),
                "SDL_GetWindowFlags" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.GetWindowFlags_Import),
                "SDL_SetWindowIcon" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.SetWindowIcon_Import),
                "SDL_GetWindowPosition" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.GetWindowPosition_Import),
                "SDL_GetWindowSize" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.GetWindowSize_Import),
                "SDL_SetWindowBordered" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.SetWindowBordered_Import),
                "SDL_SetWindowFullscreen" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.SetWindowFullscreen_Import),
                "SDL_SetWindowPosition" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.SetWindowPosition_Import),
                "SDL_SetWindowResizable" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.SetWindowResizable_Import),
                "SDL_SetWindowSize" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.SetWindowSize_Import),
                "SDL_SetWindowTitle" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.SetWindowTitle_Import),
                "SDL_ShowWindow" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.ShowWindow_Import),
                "SDL_GetWindowWMInfo" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.GetWindowWMInfo_Import),
                "SDL_GetWindowBordersSize" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.GetWindowBordersSize_Import),
                "SDL_ShowMessageBox" => Marshal.GetFunctionPointerForDelegate(Sdl.Window.ShowMessageBox_Import),
                "SDL_GetDisplayBounds" => Marshal.GetFunctionPointerForDelegate(Sdl.Display.GetDisplayBounds_Import),
                "SDL_GetCurrentDisplayMode" => Marshal.GetFunctionPointerForDelegate(Sdl.Display.GetCurrentDisplayMode_Import),
                "SDL_GetDisplayMode" => Marshal.GetFunctionPointerForDelegate(Sdl.Display.GetDisplayMode_Import),
                "SDL_GetClosestDisplayMode" => Marshal.GetFunctionPointerForDelegate(Sdl.Display.GetClosestDisplayMode_Import),
                "SDL_GetDisplayName" => Marshal.GetFunctionPointerForDelegate(Sdl.Display.GetDisplayName_Import),
                "SDL_GetNumDisplayModes" => Marshal.GetFunctionPointerForDelegate(Sdl.Display.GetNumDisplayModes_Import),
                "SDL_GetNumVideoDisplays" => Marshal.GetFunctionPointerForDelegate(Sdl.Display.GetNumVideoDisplays_Import),
                "SDL_GL_CreateContext" => Marshal.GetFunctionPointerForDelegate(Sdl.GL.GL_CreateContext_Import),
                "SDL_GL_DeleteContext" => Marshal.GetFunctionPointerForDelegate(Sdl.GL.GL_DeleteContext_Import),
                "SDL_GL_GetCurrentContext" => Marshal.GetFunctionPointerForDelegate(Sdl.GL.GL_GetCurrentContext_Import),
                "SDL_GL_GetProcAddress" => Marshal.GetFunctionPointerForDelegate(Sdl.GL.GL_GetProcAddress_Import),
                "SDL_GL_GetSwapInterval" => Marshal.GetFunctionPointerForDelegate(Sdl.GL.GL_GetSwapInterval_Import),
                "SDL_GL_MakeCurrent" => Marshal.GetFunctionPointerForDelegate(Sdl.GL.GL_MakeCurrent_Import),
                "SDL_GL_SetAttribute" => Marshal.GetFunctionPointerForDelegate(Sdl.GL.GL_SetAttribute_Import),
                "SDL_GL_SetSwapInterval" => Marshal.GetFunctionPointerForDelegate(Sdl.GL.GL_SetSwapInterval_Import),
                "SDL_GL_SwapWindow" => Marshal.GetFunctionPointerForDelegate(Sdl.GL.GL_SwapWindow_Import),
                "SDL_CreateColorCursor" => Marshal.GetFunctionPointerForDelegate(Sdl.Mouse.CreateColorCursor_Import),
                "SDL_CreateSystemCursor" => Marshal.GetFunctionPointerForDelegate(Sdl.Mouse.CreateSystemCursor_Import),
                "SDL_FreeCursor" => Marshal.GetFunctionPointerForDelegate(Sdl.Mouse.FreeCursor_Import),
                "SDL_GetGlobalMouseState" => Marshal.GetFunctionPointerForDelegate(Sdl.Mouse.GetGlobalMouseState_Import),
                "SDL_GetMouseState" => Marshal.GetFunctionPointerForDelegate(Sdl.Mouse.GetMouseState_Import),
                "SDL_SetCursor" => Marshal.GetFunctionPointerForDelegate(Sdl.Mouse.SetCursor_Import),
                "SDL_ShowCursor" => Marshal.GetFunctionPointerForDelegate(Sdl.Mouse.ShowCursor_Import),
                "SDL_WarpMouseInWindow" => Marshal.GetFunctionPointerForDelegate(Sdl.Mouse.WarpMouseInWindow_Import),
                "SDL_GetModState" => Marshal.GetFunctionPointerForDelegate(Sdl.Keyboard.GetModState_Import),
                "SDL_JoystickClose" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickClose_Import),
                "SDL_JoystickFromInstanceID" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickFromInstanceID_Import),
                "SDL_JoystickGetAxis" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickGetAxis_Import),
                "SDL_JoystickGetButton" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickGetButton_Import),
                "SDL_JoystickName" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickName_Import),
                "SDL_JoystickGetGUID" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickGetGUID_Import),
                "SDL_JoystickGetHat" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickGetHat_Import),
                "SDL_JoystickInstanceID" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickInstanceID_Import),
                "SDL_JoystickOpen" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickOpen_Import),
                "SDL_JoystickNumAxes" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickNumAxes_Import),
                "SDL_JoystickNumButtons" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickNumButtons_Import),
                "SDL_JoystickNumHats" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.JoystickNumHats_Import),
                "SDL_NumJoysticks" => Marshal.GetFunctionPointerForDelegate(Sdl.Joystick.NumJoysticks_Import),
                "SDL_free" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.free_Import),
                "SDL_GameControllerAddMapping" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerAddMapping_Import),
                "SDL_GameControllerAddMappingsFromRW" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerAddMappingsFromRW_Import),
                "SDL_GameControllerHasButton" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerHasButton_Import),
                "SDL_GameControllerHasAxis" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerHasAxis_Import),
                "SDL_GameControllerClose" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerClose_Import),
                "SDL_GameControllerGetAxis" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerGetAxis_Import),
                "SDL_GameControllerGetButton" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerGetButton_Import),
                "SDL_GameControllerGetJoystick" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerGetJoystick_Import),
                "SDL_IsGameController" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.IsGameController_Import),
                "SDL_GameControllerMapping" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerMapping_Import),
                "SDL_GameControllerOpen" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerOpen_Import),
                "SDL_GameControllerName" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerName_Import),
                "SDL_GameControllerRumble" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerRumble_Import),
                "SDL_GameControllerHasRumble" => Marshal.GetFunctionPointerForDelegate(Sdl.GameController.GameControllerHasRumble_Import),
                "SDL_HapticClose" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticClose_Import),
                "SDL_HapticEffectSupported" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticEffectSupported_Import),
                "SDL_JoystickIsHaptic" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.JoystickIsHaptic_Import),
                "SDL_HapticNewEffect" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticNewEffect_Import),
                "SDL_HapticOpen" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticOpen_Import),
                "SDL_HapticOpenFromJoystick" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticOpenFromJoystick_Import),
                "SDL_HapticRumbleInit" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticRumbleInit_Import),
                "SDL_HapticRumblePlay" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticRumblePlay_Import),
                "SDL_HapticRumbleSupported" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticRumbleSupported_Import),
                "SDL_HapticRunEffect" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticRunEffect_Import),
                "SDL_HapticStopAll" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticStopAll_Import),
                "SDL_HapticUpdateEffect" => Marshal.GetFunctionPointerForDelegate(Sdl.Haptic.HapticUpdateEffect_Import),

                _ => IntPtr.Zero,
            };

            if (ptr == IntPtr.Zero)
            {
                Console.WriteLine("Unknown Function: "+function);
            }

            return ptr;
        }
    }
}
