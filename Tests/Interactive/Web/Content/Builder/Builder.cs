using MonoGame.Framework.Content.Pipeline.Builder;

namespace Content;

public class Builder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        var contentCollection = new ContentCollection();
        contentCollection.Include<WildcardRule>("*");
        return contentCollection;
    }
}
