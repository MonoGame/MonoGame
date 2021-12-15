# MonoGame Content Builder (MGCB)

The MonoGame Content Builder is a command line tool for building XNB content on Windows, Mac, and Linux desktop systems.

Typically, it is executed by the [MGCB Editor](mgcb_editor.md) when editing content or by `MonoGame.Content.Builder.Task` during the build process
of a MonoGame project. Alternatively you can use it yourself from the command line for specialized build pipelines or for debugging content processing.

## Installation

MGCB can be installed as a [.NET tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools).
Make sure you have the .NET SDK installed. You can download it [here](https://dotnet.microsoft.com/download).

In a terminal run `dotnet tool install -g dotnet-mgcb` to install MGCB. Then you can execute MGCB by simply running `mgcb`.

## Command Line Options

The options are processed “left to right”. When an option is repeated, it is overwritten.

### Output Directory

```
/outputDir:<directory_path>
```

Specifies the directory where all content is written. Defaults to the current working directory.

### Intermediate Directory

```
/intermediateDir:<directory_path>
```

Specifies the directory where all intermediate files are written. Defaults to the current working directory.

### Rebuild Content

```
/rebuild
```

Force a full rebuild of all content.

### Clean Content

```
/clean
```

Delete all previously built content and intermediate files. Only the `/intermediateDir` and `/outputDir` need to be defined for clean to do its job.

### Incremental Build

```
/incremental
```

Only build content that changed since the last build.

### Assembly Reference

```
/reference:<assembly_path>
```

An optional parameter which adds an assembly reference which contains importers, processors, or writers needed during content building.

### Target Platform

```
/platform:<target_Platform>
```

Set the target platform for this build. It must be a member of the [TargetPlatform](xref:Microsoft.Xna.Framework.Content.Pipeline.TargetPlatform) enum:

* Windows
* iOS
* Android
* DesktopGL
* MacOSX
* WindowsStoreApp
* NativeClient
* PlayStation4
* WindowsPhone8
* RaspberryPi
* XboxOne
* Switch

If not set, it will default to Windows.

NOTE: PlayStation 4, Xbox One, and Switch support is only available to licensed console developers.

### Target Graphics Profile

```
/profile:<graphics_Profile>
```

Set the target graphics profile for this build. It must be a member of the [GraphicsProfile](xref:Microsoft.Xna.Framework.Graphics.GraphicsProfile) enum:
* HiDef
* Reach

If not set, it will default to HiDef.

### Target Build Configuration

```
/config:<build_config>
```

The optional build configuration name from the build system. This is sometimes used as a hint in content processors.

### Content Compression

```
/compress
```

Uses LZ4 compression to compress the contents of the XNB files. Content build times will increase with this option enabled. Compression is not recommended for Android as the app package is already compressed. This is not compatible with LZX compression used in XNA content.

### Content Importer Name

```
/importer:<class_name>
```

An optional parameter which defines the class name of the content importer for reading source content. If the option is omitted or used without a class name the default content importer for the source type is used.

### Content Processor Name

```
/processor:<class_name>
```

An optional parameter which defines the class name of the content processor for processing imported content. If the option is omitted used without a class name the default content processor for the imported content is used.

Note that when you change the processor all previously defined `/processorParam` are cleared.

### Content Processor Parameter

```
/processorParam:<name>=<value>
```

An optional parameter which defines a parameter name and value to set on a content processor.

Note all defined processor parameters are cleared when the `/processor` is set.

### Build Content File

```
/build:<content_filepath>
/build:<content_filepath>;<destination_filepath>
```

Instructs the content builder to build the specified content file using the previously set switches and options. Optional destination path may be specified if you want to change the output file path.

### Launch Debugger

```
/launchdebugger
```

Allows a debugger to attach to the MGCB executable before content is built.

### Response File

```
/@:<response_filepath>
```

This defines a text response file (sometimes called a command file) that contains the same options and switches you would normally find on the command line.

Each switch is specified on a new line. Comment lines are prefixed with #. These lines are ignored. You can specify multiple response files or mix normal command line switches with response files.

An example response file could look like this:

```
# Directories
/outputDir:bin/foo
/intermediateDir:obj/foo

/rebuild

# Build a texture
/importer:TextureImporter
/processor:TextureProcessor
/processorParam:ColorKeyEnabled=false
/build:Textures\wood.png
/build:Textures\metal.png
/build:Textures\plastic.png
```

#### Preprocessor Macros

Response files support preprocessor macros to allow conditionals within a response file.

```
$if <name>=<value>
$endif
```

Preprocessor symbols can be defined from the command line with the `define` option or in a response file with the `$set` directive.

```
<example command line>
MGCB.exe /define:BuildEffects=No /@:example.mgcb

<example.mgcb file>
$if BuildEffects=Yes
   /importer:EffectImporter
   /processor:EffectProcessor
   /build:Effects\custom.fx
   # all other effects here....
$endif
```

```
$set BuildEffects=Yes

$if BuildEffects=Yes
    # ...
    # This is executed
$endif
```

For booleans you can omit a value to set a symbol and to check if it is set:

```
$set BuildEffects

$if BuildEffects
    # ...
    # This is executed
$endif
```

### Customizing your Build Process

When building content from your project with `MonoGame.Content.Builder.Task`, there are a few ways to hook into the build process. `MonoGame.Content.Builder.Task` runs a target called
`RunContentBuilder` just before your project builds. If you want to do any processing before or after this process you can use the `BeforeTargets` and `AfterTargets` mechanism provided
by `msbuild` to run your own targets.

```
<Target Name="MyBeforeTarget" BeforeTargets="RunContentBuilder">
   <Message Text="MyBeforeTarget Ran"/>
</Target>
<Target Name="MyAfterTarget" AfterTargets="RunContentBuilder">
   <Message Text="MyAfterTarget Ran"/>
</Target>
```

If you want to customize the arguments sent to the `MGCB.exe` as part of the build process you can use the `<MonoGameMGCBAdditionalArguments>` property to define those.
For example to pass in the current project configuration you could include the following code in a PropertyGroup in your .csproj file.

```
<MonoGameMGCBAdditionalArguments>-config:$(Configuration)</MonoGameMGCBAdditionalArguments>
```
