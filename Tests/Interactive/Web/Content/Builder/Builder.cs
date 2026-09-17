using MonoGame.Framework.Content.Pipeline.Builder;

namespace Content;

public class Builder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        ContentCollection contentCollection = new ContentCollection();
        contentCollection.Include<WildcardRule>("*.spritefont");
        contentCollection.Include<WildcardRule>("*.wav");

        // BrowserHostValidation opens these XNB fixtures through TitleContainer.OpenStream.
        contentCollection.IncludeCopy<WildcardRule>("arial.xnb");
        contentCollection.IncludeCopy<WildcardRule>("monogame_logo.xnb");
        contentCollection.IncludeCopy<WildcardRule>("validation_effect.xnb");
        contentCollection.IncludeCopy<WildcardRule>("validation-raw.txt");
        return contentCollection;
    }
}
