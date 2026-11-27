using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class KeyboardInput
    {
        private static TaskCompletionSource<string> _tcs;
        private static UIAlertController _alert;

        private static Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            _tcs = new TaskCompletionSource<string>();

            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                _alert = UIAlertController.Create(title, description, UIAlertControllerStyle.Alert);
                UIAlertAction cancelButton = UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, Handler);
                UIAlertAction okButton = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, Handler);
                _alert.AddAction(cancelButton);
                _alert.AddAction(okButton);
                _alert.AddTextField(alertTextField =>
                {
                    alertTextField.SecureTextEntry = usePasswordMode;
                    alertTextField.KeyboardType = UIKeyboardType.ASCIICapable;
                    alertTextField.AutocorrectionType = UITextAutocorrectionType.No;
                    alertTextField.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
                    alertTextField.Text = defaultText;
                });

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
                        _tcs.SetResult(action.Style == UIAlertActionStyle.Cancel ? null : _alert.TextFields[0].Text);
                    }
                }
            });

            return _tcs.Task;
        }

        private static void PlatformCancel(string result)
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
