using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Xna.Framework.Input
{
    public partial class KeyboardInput
    {
        private static TaskCompletionSource<string> tcs;
        private static UIAlertView alert;

        private static Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            tcs = new TaskCompletionSource<string>();

            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                alert = new UIAlertView();
                alert.Title = title;
                alert.Message = description;
                alert.AlertViewStyle = usePasswordMode ? UIAlertViewStyle.SecureTextInput : UIAlertViewStyle.PlainTextInput;
                alert.AddButton("Cancel");
                alert.AddButton("Ok");
                UITextField alertTextField = alert.GetTextField(0);
                alertTextField.KeyboardType = UIKeyboardType.ASCIICapable;
                alertTextField.Placeholder = defaultText;
                alert.Dismissed += (sender, e) =>
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(e.ButtonIndex == 1 ? null : alert.GetTextField(0).Text);
                };
                alert.Show();
            });

            return tcs.Task;
        }

        private static void PlatformCancel(string result)
        {
            if (!tcs.Task.IsCompleted)
                tcs.SetResult(result);

            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                alert.DismissWithClickedButtonIndex(0, true);
            });
        }
    }
}
