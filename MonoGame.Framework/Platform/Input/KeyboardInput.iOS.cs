using System;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class KeyboardInput
    {
        private static TaskCompletionSource<string> tcs;
        private static UIAlertController alert;

        private static Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            tcs = new TaskCompletionSource<string>();

            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                alert = new UIAlertController ();
                alert.Title = title;
                alert.Message = description;
                alert.AddTextField (CreateTextField (defaultText, usePasswordMode));

                Action<UIAlertAction> cancelAction = (action) => {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult (null);
                };

                Action<UIAlertAction> okAction = (action) => {

                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult (alert.TextFields [0].Text);
                };

                alert.AddAction (UIAlertAction.Create ("Cancel", UIAlertActionStyle.Cancel, cancelAction));
                alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, okAction));

                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController (alert, true, null);
            });

            return tcs.Task;
        }

        private static Action<UITextField> CreateTextField (string defaultText, bool usePasswordMode)
        {
            Action<UITextField> create = (alertTextField) => {
                alertTextField.Text = defaultText;
                alertTextField.SecureTextEntry = usePasswordMode;
                alertTextField.KeyboardType = UIKeyboardType.ASCIICapable;
                alertTextField.AutocorrectionType = UITextAutocorrectionType.No;
                alertTextField.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
                alertTextField.Text = defaultText;
            };

            return create;
        }

        private static void PlatformCancel(string result)
        {
            if (!tcs.Task.IsCompleted)
                tcs.SetResult(result);

            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                alert.DismissViewController (true, null);
            });
        }
    }
}
