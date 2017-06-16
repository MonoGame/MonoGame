namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDebug
    {
        private bool PlatformTryDequeueMessage(out GraphicsDebugMessage message)
        {
            message = null;
            return false;
        }
    }
}
