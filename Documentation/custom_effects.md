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

# Roadmap
There is still work to be done for better support of custom effects and shaders in MonoGame:

* Support GLSL in FX files.
* * Replace MojoShader with [HL2GLSL](https://github.com/SickheadGames/HL2GLSL).
* Create an automated tests for custom effects.
* Support PlayStation Suite shaders in MGFX tools and formats.
* Support pre-compiled GLSL assembly instead of GLSL code.
