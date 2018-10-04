Localization is an important part of any game. While it can be possible to design a
game that is region independent, its quite hard. At some point you will need to 
produce localized text and graphics. 

MonoGame has a simple localization system built in. If you want to develop your own
system you are still able to do so. But the default system should be good enough for
most use cases.

# Creating resx files.

MonoGame runs on .net/Mono on most platforms. Localization is handled by those platforms
via the use of resx files. There are walkthroughs on [MSDN](https://msdn.microsoft.com/en-us/library/aa992030(v=vs.100).aspx)
which walk you through the process. A simplified version is presented here.

Create a .resx file in the IDE e.g Foo.resx and add it to your game project. Note this needs to be added to the 
main app projects. The Foo.resx file should have an Action of EmbeddedResouce and a Generator value of ResXFileCodeGenerator. 
There is a snippet from the .csproj

```xml
    <EmbeddedResource Include="Foo.resx">
      <Generator>ResXFileCodeGenerator</Generator>
      <LastGenOutput>Foo.Designer.cs</LastGenOutput>
    </EmbeddedResource>
```

Add any string resources to that file. These are in the form of a Key/Value pair. You can use the built in editor 
or manually edit the .resx file by hand. Its an xml file so you can view the contents easily.

```xml
	<data name="Wall_Style" xml:space="preserve">
		<value>Wall Style : {0}</value>
	</data>
```

What happens when the resx is processed by the generator and produces a Foo.Designer.cs file which is then 
included in your project. You can then access the "string" value by using code as follows

```csharp
	var s = MyProject.Foo.Wall_Style;
```

Note in the example we have a place holder ({0}) for additional text. You can still use te property of Foo.Wall_Style with
things like string.Format.

```csharp
	int i = 1;
	var s = string.Format (MyProject.Foo.Wall_Style, i);
```

All this means you dont need to hard the string directly. When accessing MyProject.Foo.Wall_Style the code will lookup the value from 
the embedded resx file. 

You can add support for a new language by adding a new resx file which uses the language/region code e.g Foo.de-DE.resx.
This new file will contain the translations for that language/region. In the example we are targetting German.
 
## Universal Windows Platform (UWP) considerations.

Unfortunately UWP does not support resx files anymore. They have a new file called resw. The format is similar but 
incompatible. As a result you will need to duplicate the data into a set of resw files to get the to work on UWP. The 
process is like the standrd resx process.

# Upgrading your SpriteFont files

By default the SpriteFont processor uses a limited set of characters to generate the font. While this is fine for english 
languages it would probably not include special characters needed for other languages (French, Arabic, Korean etc).

As a result MonoGame has a LocalizedFontProcessor which does something slightly different. The process looks at the resx 
files you provide it with and generates an optimized spritefont which only contains the characters your game uses. 

To make use of this functionality you ned to tell the spritefont which resx files to use. Open the .spritefont with a 
xml/text editor and add lines like this inside the Asset node

```xml
<ResourceFiles>
      <Resx>..\Foo.resx</Resx>
      <Resx>..\Foo.de-DE.resx</Resx>
</ResourceFiles>
```

Note the paths are relative to the .spritefont directory. In the example above the resx files are in the directory
above the .spritefont.

You should end up with a .spritefont file like this

```xml
<?xml version="1.0" encoding="utf-8"?>
<XnaContent xmlns:Graphics="Microsoft.Xna.Framework.Content.Pipeline.Graphics">
  <Asset Type="Graphics:FontDescription">
    <FontName>Verdana</FontName>
    <Size>14</Size>
    <Spacing>1</Spacing>
    <Style>Regular</Style>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#32;</End>
      </CharacterRegion>
    </CharacterRegions>
    <ResourceFiles>
      <Resx>..\Foo.resx</Resx>
      <Resx>..\Foo.de-DE.resx</Resx>
    </ResourceFiles>
  </Asset>
</XnaContent>
```

Once that is done you then need to change the .mgcb file so that the SpriteFontProcessor is replaced with 
the LocalizedFontProcessor. This can be done by editing the .mgcb file or using the Pipeline tool. After
that you can just compile your content as normal. If the processor has any trouble resolving or reading the
resx files you will get an error.

# Loading the Font

Loading the font can be done in the normal way. The end result of the process is a .xnb file containing a normal
SpriteFont. 

```csharp
	var font = Content.Load<SpriteFont>("Foo");
```

# Other Localized assets

Not all localized assets will be fonts. In certain situtions you might need to swap out an entire texture or spritesheet.
For these cases a new method has been added to the ContentManager, LoadLocalized. The idea behind this method is that it will
look for localized files BEFORE loading the default one. 

So for example say you have an asset, MyCharacter. You have a MyCharacter.xnb file which contains the data for that item. You 
can also has a MyCharacter.de-DE.xnb file which contains the German version of that asset. This asset could be a Texture, Audio
or any other game asset. You can then use LoadLocalized to load the localized version of the asset.

```csharp
var myCharacter = Content.LoadLocalized<Texture2D>("MyCharacter");
```

The decision on which localized asset to load is made by looking for a file with the following patterns

```xml
<AssetName>.<CurrentCulture.Name>
<AssetName>.<CurrentCulture.TwoLetterISOLanguageName>
```

These values are retrieved from 

```csharp
	CultureInfo.CurrentCulture.Name                        // eg. "en-US"
	CultureInfo.CurrentCulture.TwoLetterISOLanguageName     // eg. "en"
```

which are part of the System.Globalization namespace. On a side note you can also use the `LoadLocalized` to load language 
specific SpriteFonts. They just need to be named in the same way as we have described above.
