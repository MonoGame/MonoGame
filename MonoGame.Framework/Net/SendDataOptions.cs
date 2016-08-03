namespace Microsoft.Xna.Framework.Net
{
    public enum SendDataOptions
    {
        None,
        Reliable,
        InOrder,
        ReliableInOrder,
        Chat // Force no encryption to comply with international regulations, can be combined with InOrder and should have its own channel
    }
}