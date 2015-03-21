extern alias MicrosoftXnaFramework;
extern alias MicrosoftXnaGamerServices;
using MsXna_Guide = MicrosoftXnaGamerServices::Microsoft.Xna.Framework.GamerServices.Guide;
using MsXna_MessageBoxIcon = MicrosoftXnaGamerServices::Microsoft.Xna.Framework.GamerServices.MessageBoxIcon;
using MsXna_PlayerIndex = MicrosoftXnaFramework::Microsoft.Xna.Framework.PlayerIndex;

using System;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class KeyboardInput
    {
        private static TaskCompletionSource<string> tcs;

        private static Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            tcs = new TaskCompletionSource<string>();
            MsXna_Guide.BeginShowKeyboardInput(MsXna_PlayerIndex.One, title, description, defaultText,
                ar =>
                {
                    var result = MsXna_Guide.EndShowKeyboardInput(ar);

                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(result);
                },
                null,
                usePasswordMode);

            return tcs.Task;
        }

        private static void PlatformCancel(string result)
        {
            throw new NotSupportedException();
        }
    }
}
