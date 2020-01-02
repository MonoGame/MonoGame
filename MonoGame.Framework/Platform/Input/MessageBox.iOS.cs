using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class MessageBox
    {
        private static TaskCompletionSource<int?> tcs;
        private static UIAlertController alert;

        private static UIAlertAction CreateAction (int index, string button)
        {
            Action<UIAlertAction> completionHandler = (alert) => { 
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult (index);
            };

            return UIAlertAction.Create (button, UIAlertActionStyle.Default, completionHandler);
        }

        private static Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            tcs = new TaskCompletionSource<int?>();
            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                alert = new UIAlertController();
                alert.Title = title;
                alert.Message = description;

                for (int i = 0; i < buttons.Count; i++)
                    alert.AddAction(CreateAction (i, buttons[i]));

                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController (alert, true, null);
            });

            return tcs.Task;
        }

        private static void PlatformCancel(int? result)
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
