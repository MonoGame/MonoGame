using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class MessageBox
    {
        private static TaskCompletionSource<int?> tcs;
        private static AlertDialog alert;

        private static Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            tcs = new TaskCompletionSource<int?>();
            Game.Activity.RunOnUiThread(() =>
            {
                alert = new AlertDialog.Builder(Game.Activity).Create();

                alert.SetTitle(title);
                alert.SetMessage(description);

                alert.SetButton((int)DialogButtonType.Positive, buttons[0], (sender, args) =>
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(0);
                });

                if (buttons.Count > 1)
                {
                    alert.SetButton((int)DialogButtonType.Negative, buttons[1], (sender, args) =>
                    {
                        if (!tcs.Task.IsCompleted)
                            tcs.SetResult(1);
                    });
                }

                if (buttons.Count > 2)
                {
                    alert.SetButton((int)DialogButtonType.Neutral, buttons[2], (sender, args) =>
                    {
                        if (!tcs.Task.IsCompleted)
                            tcs.SetResult(2);
                    });
                }

                alert.CancelEvent += (sender, args) =>
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(null);
                };

                alert.Show();
            });

            return tcs.Task;
        }

        private static void PlatformCancel(int? result)
        {
            alert.Dismiss();
            tcs.SetResult(result);
        }
    }
}
