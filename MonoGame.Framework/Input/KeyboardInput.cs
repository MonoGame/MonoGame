using System;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input
{
    public partial class KeyboardInput
    {
        public static bool IsVisible { get; private set; }

        public static async Task<string> Show(string title, string description, string defaultText = "", bool usePasswordMode = false)
        {
            if (IsVisible)
                throw new Exception("The function cannot be completed at this time: the KeyboardInput UI is already active. Wait until KeyboardInput.IsVisible is false before issuing this call.");

            IsVisible = true;

            var result = await PlatformShow(title, description, defaultText, usePasswordMode);

            IsVisible = false;

            return result;
        }

        public static void SetResult(string result)
        {
            if (!IsVisible)
                throw new Exception("The function cannot be completed at this time: the MessageBox UI is not active.");

            PlatformSetResult(result);
        }
    }
}
