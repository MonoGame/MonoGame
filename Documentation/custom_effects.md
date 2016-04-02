A core element of Microsoft XNA is the effect system which is used for all rendering.

For MonoGame we have the burden of supporting stock and custom effects for desktop GLSL, mobile GLSL, DirectX HLSL, and custom formats like that of the PlayStation Mobile.  There currently is no effect system or shader language that supports all the platforms we require, forcing us to build a new custom effect system.

# MGFX
MGFX is MonoGame's own "FX" runtime and tools which with the following core goals:

* Support a similar technique, passes, shaders structure as Microsoft FX files.
* Have a textual format for ease of editing.
* Have a compiled and optimized binary format for runtime use.
* Be cross-platform and support multiple shader languages and bytecodes.
* Easy to extend for future platforms and features.


### PlayStation Mobile
The PSM platform uses neither GLSL or HLSL and instead has a custom shader system of its own which MGFX does not support at this time. For this reason custom effects require PSM specific coding to function.  The stock effects in PSM are limited in functionality and still in development.

# Stock Effects
The following stock effects in MonoGame and fully supported on current platforms:

* BasicEffect
* AlphaTestEffect
* DualTextureEffect
* EnvironmentMapEffect
* SkinnedEffect

Under the hood these effects use the same system and tools as one would for a custom Effect.  The source and pre-compiled versions of these effects can be found in the 'MonoGame.Framework\Graphics\Effect\Resources' folder.

If your game requires an extra little bit of performance you can easily hand edit the existing effects to remove unnecessary features or optimize for specific hardware and rebuild them with the MGFX tool.

# Custom Effects
To use a custom effect with MonoGame you must do one of the following (not both):
* Process your effect file with the [2MGFX tool](2mgfx.md) and load them yourself at runtime.
* Run the effect file thru the [MonoGame Effect content processor](mgcb.md) for loading via the `ContentManager`.


### Effect Writing Tips
These are some tips for writing or converting effects for use with MonoGame.

* Use the [DX11 feature levels](http://msdn.microsoft.com/en-us/library/windows/desktop/ff476876.aspx) `vs_4_0_level_9_1` or `ps_4_0_level_9_1` when targeting Windows 8 Metro applications and wanting to support all devices.  Higher shader models work, but might not run on all Windows 8 systems.
* When targeting Windows Phone 8 you can use `vs_4_0_level_9_3` or `ps_4_0_level_9_3`.
* When targeting GL platforms we automatically translate FX files to GLSL using a library called [MojoShader](http://icculus.org/mojoshader/).  It will only work with `vs_3_0` or `ps_3_0` or lower shaders.
* Make sure the pixel shaders inputs exactly match the vertex shader outputs so the parameters are passed in the correct registers.
* You can use `#if SM4` to add conditional code for DirectX platforms.
* On DirectX platforms use the `SV_Position` semantic instead of `POSITION` in vertex shader inputs.
* Note that on GL platforms default values on Effect parameters do not work.  Either set the parameter from code or use a real constant like a #define.
* Do not name your sampler `Sampler` - it will not compile.
* The effect compiler is aggressive about removing unused paramters, be sure the parameters you are setting are actually used.
* If you think you've found a bug porting a shader [please let us know](https://github.com/mono/MonoGame/issues).

### Summary of some work arounds in the process of transferring effects files across to MonoGame from XNA

#### Aligning your version of the content processor and the runtime
For shader .fx files, the version of the of the content processor you are using to compile your xnb files (ie. **MonoGameContentProcessors.dll**) **must be aligned** with the version of the runtime you are running your game against (ie. **MonoGame.Framework.dll**)

Otherwise you may get **"Wrong MGFX file version!"** raised by ReadEffect() in Effect.cs when you do a **content.Load<Effect>**
[because the version written in to the .xnb file is not a match with the value of MGFXVersion 
that has been put in the run time].

So if you have this error, the first thing to check is that your content processor is the same up-to-dateness as the version of the runtime you are running your game against.


#### Shader model compilation:
(a) For **OpenGL** versions of MonoGame (eg. Mac, Linus, WindowsGL) the shader model must be **SM 3.0 or lower**

(b) For **DirectX** versions of MonoGame (eg. WinRT, Windows8) the shader model must be **SM 4.0 or higher**.



In the DirectX case it is typical for people to use the:

`vs_4_0_level_9_1`
and
`ps_4_0_level_9_1`

compilation profiles, because these provide backwards compatibility for Shader Model 2 shaders.



#### Windows8 configuration == DX11 (inside the content processor)
(a) The "Windows8" build configuration is used internally in the MonoGame content processor code to indicate compilation to target DX11.

Look at the code on line 29 of MGEffectProcessor.cs! 

`options.DX11Profile = platform == MonoGamePlatform.Windows8 ? true : false; `



The **ContentHelper.cs** file in the ContentProcessors project reads 
the build configuration name you are using then does this:

`switch (platform.ToUpper())`    
`{`     
`case "WINDOWS":`     
`     return MonoGamePlatform.Windows;`       
`case "WINDOWS8":`       
`     return MonoGamePlatform.Windows8;`       
`case "IOS":`      
`     return MonoGamePlatform.iOS;`     
`case "ANDROID":`    
`     return MonoGamePlatform.Android;`     
`case "LINUX":`     
`     return MonoGamePlatform.Linux;`     
`case "OSX":`     
`     return MonoGamePlatform.OSX;`     
`case "PSM":`     
`     return MonoGamePlatform.PSM;`     
`etc.`     
`}`      

Which then gets used by MGEffectProcessor.cs (as above)
`options.DX11Profile = platform == MonoGamePlatform.Windows8 ? true : false; `
to set the flag specifying DX11.

In other words, in order to compile .xnb files from your .fx files that will be able to
run against the Windows DirectX version of MonoGame, you **MUST** use build profile that
is **specifically called "Windows8" !!**  ... 
either the one supplied with the project code or one you make yourself.

At some point this getting written into a registry key  
called "MONOGAME_PLATFORM" (HKEY_CURRENT_USER/Environment/MONOGAME_PLATFORM),
and then gets read back from that key via:
`var platform = Environment.GetEnvironmentVariable("MONOGAME_PLATFORM", EnvironmentVariableTarget.User);`

If you don't realize this, having just converted all your .fx files to run against  
SM 4.0, when you come to compile them for DX11, you will be told:  
"Vertex shader 'SimpleVS' must be SM 3.0 or lower!"  


(b) The "Windows" build configuration is used for the WindowsGL target.

(c) Aside: The WindowsGL version of MonoGame is primarily intended to be a test environment. 
Don't necessarily expect great performance from this build.


#### Windows8, DX11 profile, switch POSITION0 to SV_POSITION (in .fx file)
When compiling for the Windows8, DX11 profile, the POSITION and POSITION0 semantics must be replaced by SV_POSITION.

Otherwise you will get:
A first chance exception of type 'SharpDX.SharpDXException' occurred in SharpDX.DLL
Additional information: HRESULT: [0x80070057], Module: [Unknown], ApiCode: [Unknown/Unknown], Message: The parameter is incorrect.

in
`new SharpDX.Direct3D11.InputLayout`

There may be other similars.


# Roadmap
There is still work to be done for better support of custom effects and shaders in MonoGame:

* Support GLSL in FX files.
  * Write a new preprocessor replacing [D3DPreprocess](http://msdn.microsoft.com/en-us/library/windows/desktop/dd607332.aspx).
  * Replace MojoShader with [HL2GLSL](https://github.com/SickheadGames/HL2GLSL).
* Create an automated tests for custom effects.
* Support PlayStation Suite shaders in MGFX tools and formats.
* Support pre-compiled GLSL assembly instead of GLSL code.
