using System;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class KeyboardInput
    {
        public static bool IsVisible { get; private set; }

        /// <summary>
        /// Displays the keyboard input interface asynchronously.
        /// </summary>
        /// <param name="title">Title of the dialog box.</param>
        /// <param name="description">Description of the dialog box.</param>
        /// <param name="defaultText">Default text displayed in the input area.</param>
        /// <param name="usePasswordMode">If password mode is enabled, the characters entered are not displayed.</param>
        /// <returns>Text entered by the player. Null if back was used.</returns>
        /// <exception cref="System.Exception">Thrown when the message box is already visible</exception>
        /// <example>
        /// <code>
        /// var name = await KeyboardInput.Show("Name", "What's your name?", "Player");
        /// </code>
        /// </example>
        public static async Task<string> Show(string title, string description, string defaultText = "", bool usePasswordMode = false)
        {
            if (IsVisible)
                throw new Exception("The function cannot be completed at this time: the KeyboardInput UI is already active. Wait until KeyboardInput.IsVisible is false before issuing this call.");

            IsVisible = true;

            var result = await PlatformShow(title, description, defaultText, usePasswordMode);

            IsVisible = false;

            return result;
        }

        /// <summary>
        /// Hides the keyboard input interface and returns the parameter as the result of <see cref="Show"/>
        /// </summary>
        /// <param name="result">Result to return</param>
        /// <exception cref="System.Exception">Thrown when the keyboard input is not visible</exception>
        /// <example>
        /// <code>
        /// var nameTask = KeyboardInput.Show("Name", "What's your name?", "Player");
        /// KeyboardInput.Cancel("John Doe");
        /// var name = await nameTask;
        /// </code>
        /// </example>
        public static void Cancel(string result)
        {
            if (!IsVisible)
                throw new Exception("The function cannot be completed at this time: the MessageBox UI is not active.");

            PlatformCancel(result);
        }
    }
}
