using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MessageBox
    {
        public static bool IsVisible { get; private set; }

        public static async Task<int?> Show(string title, string description, IEnumerable<string> buttons)
        {
            if (IsVisible)
                throw new Exception("The function cannot be completed at this time: the MessageBox UI is already active. Wait until MessageBox.IsVisible is false before issuing this call.");

            IsVisible = true;

            var buttonsList = buttons.ToList();
            if (buttonsList.Count > 3 || buttonsList.Count == 0)
                throw new ArgumentException("Invalid number of buttons: one to three required", "buttons");

            var result = await PlatformShow(title, description, buttonsList);

            IsVisible = false;

            return result;
        }

        public static void SetResult(int result)
        {
            if (!IsVisible)
                throw new Exception("The function cannot be completed at this time: the MessageBox UI is not active.");

            PlatformSetResult(result);
        }
    }
}