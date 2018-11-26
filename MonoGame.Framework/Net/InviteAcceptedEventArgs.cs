using System;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public class InviteAcceptedEventArgs : EventArgs
    {
        public InviteAcceptedEventArgs(SignedInGamer gamer, bool isCurrentSession)
        {
            this.Gamer = gamer;
            this.IsCurrentSession = isCurrentSession;
        }

        public SignedInGamer Gamer { get; private set; }
        public bool IsCurrentSession { get; private set; }
    }
}
