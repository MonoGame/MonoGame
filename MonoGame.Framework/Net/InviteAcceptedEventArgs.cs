using System;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public class InviteAcceptedEventArgs : EventArgs
    {
        public InviteAcceptedEventArgs(SignedInGamer gamer)
        {
            this.Gamer = gamer;
        }

        public SignedInGamer Gamer { get; }
        public bool IsCurrentSession { get; }
    }
}