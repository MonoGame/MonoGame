﻿using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MessageBox
    {
        private static UIAlertView alert;

        private static Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            var tcs = new TaskCompletionSource<int?>();
            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                alert = new UIAlertView();
                alert.Title = title;
                alert.Message = description;
                foreach (string button in buttons)
                    alert.AddButton(button);
                alert.Dismissed += (sender, e) =>
                {
                    if (!tcs.Task.IsCompleted)
					    tcs.SetResult((int)e.ButtonIndex);
                };
                alert.Show();
            });

            return tcs.Task;
        }

        private static void PlatformSetResult(int result)
        {
            alert.DismissWithClickedButtonIndex(result, true);
        }
    }
}
