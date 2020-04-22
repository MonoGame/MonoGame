The 2MGFX tool is used to build a MonoGame Effect from an input Microsoft FX or MGFX file.


## Command Line
The command line options are:

```
2MGFX <SourceFile> <OutputFile> [/Debug] [/Profile:<DirectX_11,OpenGL,PlayStation4>]
```

### Source File
The input effect file in typical FX format with samplers, techniques, and passes defined.  This parameter is required.

### Output File
The file to write for the output compiled MGFX file.  This parameter is required.

NOTE: The generated file is not an XNB file for use with the ContentManager.

### Debug Info
If the `/Debug` flag is passed the resulting compiled MGFX file will contain extra debug information and the fewest possible optimizations.

### Platform Profile
The `/Profile` option defines the platform we're targeting with this effect file.  It can be one of the following:

```
DirectX_11
OpenGL
PlayStation4
PSVita
XboxOne
Switch
```
NOTE: PlayStation 4, Xbox One, PS Vita, and Switch support is only available to licensed console developers.

### Help
If you use `/?`, `/help`, or simply pass no paramters to 2MGFX.exe you will get information about these command line options.

## Runtime Use
The resulting compiled MGFX file can be used from your game code like so:

```csharp
byte[] bytecode = File.ReadAllBytes("mycompiled.mgfx");
var effect = new Effect(bytecode);
```

This is basically how the stock effects (BasicEffect, DualTextureEffect, etc) are compiled and loaded.

