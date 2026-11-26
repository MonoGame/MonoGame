using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class MessageBox
    {
        private static TaskCompletionSource<int?> _tcs;
        private static UIAlertController _alert;

        private static Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            _tcs = new TaskCompletionSource<int?>();
            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                _alert = UIAlertController.Create(title, description, UIAlertControllerStyle.Alert);
                foreach (string button in buttons)
                {
                    UIAlertAction action = UIAlertAction.Create(button, UIAlertActionStyle.Default, Handler);
                    _alert.AddAction(action);
                }

                UIViewController rootViewController;
                if(UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                    rootViewController = GetRootViewController();
                else
                    rootViewController = UIApplication.SharedApplication.KeyWindow?.RootViewController;

                rootViewController?.PresentViewController(_alert, true, null);
                return;

                void Handler(UIAlertAction action)
                {
                    if (!_tcs.Task.IsCompleted)
                    {
                        _tcs.SetResult(Array.IndexOf(_alert.Actions,action));
                    }
                }
            });

            return _tcs.Task;
        }

        private static void PlatformCancel(int? result)
        {
            if (!_tcs.Task.IsCompleted)
                _tcs.SetResult(result);

            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                _alert.DismissViewController(true, null);
            });
        }

        [SupportedOSPlatform("ios13.0")]
        private static UIViewController GetRootViewController() =>
            UIApplication.SharedApplication.ConnectedScenes
                .OfType<UIWindowScene>()
                .FirstOrDefault(scene => scene.ActivationState == UISceneActivationState.ForegroundActive)?
                .Windows
                .FirstOrDefault(w => w.IsKeyWindow)?
                .RootViewController;

    }
}
