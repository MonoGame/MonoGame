
namespace Microsoft.Xna.Framework.Content.Pipeline
{
    public class ContentItem
    {
        OpaqueDataDictionary opaqueData = new OpaqueDataDictionary();

        public ContentIdentity Identity { get; set; }

        public string Name { get; set; }

        public OpaqueDataDictionary OpaqueData { get { return opaqueData; } }

        public ContentItem()
        {
        }
    }
}
