using MonoGame.Framework.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class MessageBox
    {
        private static TaskCompletionSource<int?> _tcs;

        private static Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            _tcs = new TaskCompletionSource<int?>();

            Sdl.Window.MessageBoxData data = new Sdl.Window.MessageBoxData();
            data.flags = 0;
            data.title = title;
            data.message = description;
            data.numbuttons = 0;
            data.buttons = IntPtr.Zero;
            data.colorScheme = IntPtr.Zero;

            if (buttons != null &&
                buttons.Count > 0)
            {
                Sdl.Window.MessageBoxButtonData[] buttonData = new Sdl.Window.MessageBoxButtonData[buttons.Count];

                for (int i = 0; i < buttons.Count; i++)
                {
                    // We need to reverse button order on Windows only
                    // SDL3 has a flag to reverse that but SDL2 doesn't
                    int reverseIndex = (CurrentPlatform.OS == OS.Windows ? buttons.Count - i - 1 : i);

                    buttonData[i] = new Sdl.Window.MessageBoxButtonData();
                    buttonData[i].flags = 0;
                    buttonData[i].buttonid = reverseIndex;
                    buttonData[i].text = IntPtr.Zero;

                    if (buttons[i] != null)
                    {
                        // convert to C string pointer data because we need to marshal a struct array
                        byte[] bytes = Encoding.UTF8.GetBytes(buttons[reverseIndex]);
                        IntPtr mem = Marshal.AllocHGlobal(bytes.Length + 1);
                        Marshal.Copy(bytes, 0, mem, bytes.Length);
                        unsafe
                        {
                            ((byte*)mem)[bytes.Length] = 0;
                        }

                        buttonData[i].text = mem;
                    }
                }

                data.numbuttons = buttons.Count;

                // convert the struct array to pointer
                unsafe
                {
                    fixed (Sdl.Window.MessageBoxButtonData* buttonsPtr = &buttonData[0])
                    {
                        data.buttons = (IntPtr)buttonsPtr;
                    }
                }
            }

            int result = -1;

            int error = Sdl.Window.ShowMessageBox(ref data, out result);

            if (error == 0)
                _tcs.SetResult(result);
            else
                _tcs.SetResult(-1);

            return _tcs.Task;
        }

        private static void PlatformCancel(int? result)
        {
            if (_tcs != null)
                _tcs.SetResult(result);
        }
    }
}
