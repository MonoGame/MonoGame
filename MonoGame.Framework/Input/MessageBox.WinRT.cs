using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class MessageBox
    {
        private static readonly CoreDispatcher dispatcher;
        private static TaskCompletionSource<int?> tcs;
        private static IAsyncOperation<IUICommand> dialogResult; 

        static MessageBox()
        {
            dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        }

        private static Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            // TODO: MessageDialog only supports two buttons
            if (buttons.Count == 3)
                throw new NotSupportedException("This platform does not support three buttons");

            tcs = new TaskCompletionSource<int?>();

            MessageDialog dialog = new MessageDialog(description, title);
            foreach (string button in buttons)
                dialog.Commands.Add(new UICommand(button, null, dialog.Commands.Count));

            dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    try
                    {
                        // PlatformSetResult will cancel the task, resulting in an exception
                        dialogResult = dialog.ShowAsync();
                        var result = await dialogResult;
                        if (!tcs.Task.IsCompleted)
                            tcs.SetResult(result == null ? null : (int?)result.Id);
                    }
                    catch (TaskCanceledException)
                    {
                        if (!tcs.Task.IsCompleted)
                            tcs.SetResult(null);
                    }
                });

            return tcs.Task;
        }

        private static void PlatformCancel(int? result)
        {
            // TODO: MessageDialog doesn't hide on Windows Phone 8.1
            tcs.SetResult(result);
            dialogResult.Cancel();
        }
    }
}
