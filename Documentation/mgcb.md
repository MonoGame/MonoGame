The MonoGame Content Builder (MGCB.exe) is a command line tool for building XNB content on Windows, Mac, and Linux desktop systems.

Typically it is executed by the [Pipeline GUI tool](pipeline.md) when editing content or indirectly from VisualStudio or MonoDevelop during the build process of a MonoGame project.  Alternatively you can use it yourself from the command line for specialized build pipelines or for debugging content processing.

## Command Line Options
The options are processed "left to right".  When an option is repeated the last option always wins.

### Output Directory
```
/outputDir:<directory_path>
```
It specifies the directory where all content is written.  If this option is omitted the output will be put into the current working directory.

### Intermediate Directory
```
/intermediateDir:<directory_path>
```
It specifies the directory where all intermediate files are written.  If this option is omitted the intermediate data will be put into the current working directory.

### Rebuild Content
```
/rebuild 
```
An optional parameter which forces a full rebuild of all content.

### Clean Content
```
/clean
```
Delete all previously built content and intermediate files.  Only the `/intermediateDir` and `/outputDir` need to be defined for clean to do its job.

### Incremental Build
```
/incremental
```
Skip cleaning files not included in the current build.  Useful for custom tools which only require a subset of the game content built.

### Assembly Reference
```
/reference:<assembly_path> 
```
An optional parameter which adds an assembly reference which contains importers, processors, or writers needed during content building.

### Target Platform
```
/platform:<target_Platform> 
```
Set the target platform for this build. It must be a member of the TargetPlatform enum:
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
* PSVita
* XboxOne
* Switch

If not set it will default to Windows.

NOTE: PlayStation 4, Xbox One, PS Vita, and Switch support is only available to licensed console developers.

### Target Graphics Profile
```
/profile:<graphics_Profile> 
```
Set the target graphics profile for this build. It must be a member of the GraphicsProfile enum:
* HiDef
* Reach

If not set it will default to HiDef.

### Target Build Configuration
```
/config:<build_config> 
```
The optional build configuration name from the build system.  This is sometimes used as a hint in content processors.

### Content Compression
```
/compress
```
Uses LZ4 compression to compress the contents of the XNB files.  Content build times will increase with this option enabled.  Compression is not recommended for platforms such as Android, Windows Phone 8 and Windows 8 as the app package is already compressed.  This is not compatible with LZX compression used in XNA content.

### Content Importer Name
```
/importer:<class_name>
```
An optional parameter which defines the class name of the content importer for reading source content.  If the option is omitted or used without a class name the default content importer for the source type is used.

### Content Processor Name
```
/processor:<class_name>
```
An optional parameter which defines the class name of the content processor for processing imported content.  If the option is omitted used without a class name the default content processor for the imported content is used.

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
Instructs the content builder to build the specified content file using the previously set switches and options. Optional destination path may be specified if you want to change the output filepath.

### Response File
```
/@:<response_filepath>
```
This defines a text response file (sometimes called a command file) that contains the same options and switches you would normally find on the command line.

Each switch is specified on a new line.  Comment lines are prefixed with #.  You can specify multiple response files or mix normal command line switches with response files.

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
### Launch Debugger
```
/launchdebugger
```
Allows a debugger to attach to the MGCB executable before content is built.
### Define Preprocessor Parameter
```
/define <name>=<value>
```
Sets or creates a preprocessor parameter with the given name and value.
### Preprocessor Macros
```
$if <name>=<value>
$endif
```
Preprocessor macros are intended to allow conditionals within a response file.

The preprocess step is what expands a response file command into its composite commands for each line in the file. However, a line is only emitted if all conditionals which contain the line evaluate true.
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

### Customizing your Build Process

When building content from your project via `msbuild` there are a few ways to can hook into the build process. The `MonoGame.Content.Builder.targets` runs a target called
`BuildContent` just before your project builds. If you want to do any processing before or after this process you can use the `BeforeTargets` and `AfterTargets` mechanism provided
by `msbuild` to run your own targest.

```
<Target Name="MyBeforeTarget" BeforeTargets="BuildContent">
   <Message Text="MyBeforeTarget Ran"/>
</Target>
<Target Name="MyAfterTarget" AfterTargets="BuildContent">
   <Message Text="MyAfterTarget Ran"/>
</Target>
``` 

If you want to customise the arguements sent to the `MGCB.exe` as part of the build process you can use the `<MonoGameMGCBAdditionalArguments>` property to define those. 
For example if you wanted to pass in the current project configuration you could define

```
<MonoGameMGCBAdditionalArguments>-config:$(Configuration)</MonoGameMGCBAdditionalArguments>
```

