extern alias MicrosoftXnaFramework;
extern alias MicrosoftXnaGamerServices;
using MsXna_Guide = MicrosoftXnaGamerServices::Microsoft.Xna.Framework.GamerServices.Guide;
using MsXna_MessageBoxIcon = MicrosoftXnaGamerServices::Microsoft.Xna.Framework.GamerServices.MessageBoxIcon;
using MsXna_PlayerIndex = MicrosoftXnaFramework::Microsoft.Xna.Framework.PlayerIndex;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class MessageBox
    {
        private static TaskCompletionSource<int?> tcs;

        private static Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            tcs = new TaskCompletionSource<int?>();
            MsXna_Guide.BeginShowMessageBox(MsXna_PlayerIndex.One, title, description, buttons, 0, MsXna_MessageBoxIcon.None,
                ar =>
                {
                    var result = MsXna_Guide.EndShowMessageBox(ar);

                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(result);
                },
                null);

            return tcs.Task;
        }

        private static void PlatformCancel(int? result)
        {
            throw new NotSupportedException();
        }
    }
}
