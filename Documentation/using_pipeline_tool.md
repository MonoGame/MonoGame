The [Pipeline Tool](pipeline.md) is used to orginize and define content for use with MonoGame.

## Importing From XNA

In addition to creating new projects, the Pipeline Tool supports importing your existing XNA .contentproj files.  You can access this from the the File menu.

<p align="center">
<img src="images/pipeline_import.png"/>
</p>

This creates a new project, adding all your content and content settings from the XNA project.  If you happened to be using custom processors you may need to edit the assembly references to link to the correct paths.

## Adding Content Items

 TODO!


## Custom Content Processors

If you are using custom content processors you need to rebuild them for use with MonoGame.

 * Remove all Microsoft.Xna references.
 * Reference the MonoGame.Framework.Content.Pipeline assembly.
 * Build in either "Any Cpu" or "x64" modes.
 

## Linking Content To Your Game

Once you have built your content you have a few different ways to add the XNBs to your game project.

### Manual Copy

The simplest and method is to simply copy the content into a Content folder under the output folder where you game executable is.  This should work for most desktop builds.


### Add As Content

If you are using Visual Studio you can simply add the content files into your C# game project.  Create a folder in the project called Content then right click on the folder and select Add -> Existing Item.

<p align="center">
<img src="images/existing_item.png"/>
</p>

You will now see a file dialog from which you can add your content files.  Note that if you don't want Visual Studio to make a local copy of the files be sure to use "Add As Link".

<p align="center">
<img src="images/add_as_link.png"/>
</p>

Once the files are added you need to select them all and change their build action to "Content" and "Copy if newer".

<p align="center">
<img src="images/copy_if_newer.png"/>
</p>


### Add With Wildcard

The more automatic option is to hand edit your game .csproj and have it include you content using wildcards.  To do this just open the .csproj with any text editor then add the following after any other `<ItemGroup>`:

```
  <ItemGroup>
    <Content Include="Content\**\*.*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
```

Then any files you put in a Content folder within your game project will automatically be included in the build.


## Reporting Bugs

The MonoGame content pipeline, MGCB, and the Pipeline tool are all very new in MonoGame.  If you run into any problems with them please ask for help on the [community site](http://community.monogame.net/) or submit a [bug report on GitHub](https://github.com/mono/MonoGame/issues).
