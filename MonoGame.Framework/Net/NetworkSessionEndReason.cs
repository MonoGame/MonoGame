namespace Microsoft.Xna.Framework.Net
{
    public enum NetworkSessionEndReason : byte
    {
        ClientSignedOut,
        HostEndedSession,
        RemovedByHost,
        Disconnected,
    }
}
