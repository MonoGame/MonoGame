The [Pipeline Tool](pipeline.md) is used to organize and define content for use with MonoGame.  It is installed as part of the MonoGame SDK Installer or can be build [directly from source](https://github.com/mono/MonoGame/tree/develop/Tools/Pipeline) if needed.

## Create A Project

To start a new project just select "New..." from the "File" menu.  This will give you a new empty project to add content to.

If you are starting from an existing XNA project, the Pipeline Tool supports importing your existing .contentproj.  Again you can access this from the the "File" menu:

<p align="center">
<img src="images/pipeline_import.png"/>
</p>

This creates a new project, adding all your content and content settings from the XNA project.  If you happened to be using custom processors you may need to edit the assembly references to link to the correct paths which we discuss next.

## Project Settings

You can edit the content project settings directly from the property grid editor after selecting the project node in the tree view:

<p align="center">
<img src="images/pipeline_project.png"/>
</p>

This is where you setup the folders for output, the platform to target, the assembly references for custom processors, etc.

Note that currently the Pipeline tool is not setup to support multiple target platforms.  This means you may need to manage mutliple content projects or manually change the target platform between builds.  We are working on adding functionaliy to support multiple platforms and configurations within a single project.


## Adding Content Items

Once you have a project setup you can then add content to it for building.  You can do this from the "Edit" menu:

<p align="center">
<img src="images/pipeline_items.png"/>
</p>

Selecting "New Item..." will bring up the New Item dialog which displays a list of new items that can be created:

<p align="center">
<img src="images/pipeline_newitem.png"/>
</p>

When you select "Add Item..." you get to select an existing item from disk to add to the content project.


## Custom Content Processors

Justl ike XNA, the MonoGame content pipeline supports custom content processors.  To use them you need to rebuild them correctly to work against MonoGame.

The first step is removing all `Microsoft.Xna.Framework.XXX` references and replacing them with references to `MonoGame.Framework` and `MonoGame.Framework.Content.Pipeline`.  This is required as you will no longer be building against Microsoft XNA.

Once you references are working you then need to change your assembly target platform.  MonoGame does not support x86 (aka 32bit) assemblies in the content pipeline.  This is mainly to allow of processing really big content as well as to simplify the number of configurations and native code dependancies.  For this reason you should try to target "Any CPU" with your custom content assembly.

After you have done these fixes you should be able to add these new processors to the content project "References".


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
