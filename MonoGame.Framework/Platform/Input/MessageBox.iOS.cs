using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class MessageBox
    {
        private static TaskCompletionSource<int?> tcs;
        private static UIAlertView alert;

        private static Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            tcs = new TaskCompletionSource<int?>();
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

        private static void PlatformCancel(int? result)
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
