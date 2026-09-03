using MonoGame.Framework.Content.Pipeline.Builder;

namespace Content;

public class Builder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        ContentCollection contentCollection = new ContentCollection();
        contentCollection.Include<WildcardRule>("*");
        contentCollection.IncludeCopy<WildcardRule>("*.txt");
        return contentCollection;
    }
}
