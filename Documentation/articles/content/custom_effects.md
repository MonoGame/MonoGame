A core element of Microsoft XNA is the effect system which is used for all rendering.

For MonoGame we have the burden of supporting stock and custom effects for desktop GLSL, mobile GLSL, DirectX HLSL, and custom formats like that of the PlayStation Mobile.  There currently is no effect system or shader language that supports all the platforms we require, forcing us to build a new custom effect system.

# MGFX
MGFX is MonoGame's own "FX" runtime and tools which with the following core goals:

* Support a similar technique, passes, shaders structure as Microsoft FX files.
* Have a textual format for ease of editing.
* Have a compiled and optimized binary format for runtime use.
* Be cross-platform and support multiple shader languages and bytecodes.
* Easy to extend for future platforms and features.

# Stock Effects
MonoGame has the following effects built-in and fully supported on current platforms:

* SpriteEffect
* BasicEffect
* AlphaTestEffect
* DualTextureEffect
* EnvironmentMapEffect
* SkinnedEffect

Under the hood these effects use the same system and tools as one would for a custom Effect.  The source and pre-compiled versions of these effects can be found in the ['MonoGame.Framework\Graphics\Effect\Resources'](https://github.com/MonoGame/MonoGame/tree/develop/MonoGame.Framework/Graphics/Effect/Resources) folder.

If your game requires an extra little bit of performance you can easily hand edit the existing effects to remove unnecessary features or optimize for specific hardware and rebuild them with the MGFX tool.

# Custom Effects
To use a custom effect with MonoGame you must do one of the following (not both):
* Run the effect file through the [MonoGame Effect content processor](mgcb.md) for loading via the `ContentManager` (Recommended).
* Process your effect file with the [2MGFX tool](2mgfx.md) and load them yourself at runtime.


### Effect Writing Tips
These are some tips for writing or converting effects for use with MonoGame.

* The supported shader models when targeting DX are the following:
  * `vs_4_0_level_9_1` and `ps_4_0_level_9_1` 
  * `vs_4_0_level_9_3` and `ps_4_0_level_9_3`
  * `vs_4_0` and `ps_4_0` (requires `HiDef` `GraphicsProfile` at runtime)
  * `vs_4_1` and `ps_4_1` (requires `HiDef` `GraphicsProfile` at runtime)
  * `vs_5_0` and `ps_5_0` (requires `HiDef` `GraphicsProfile` at runtime)
* When targeting GL platforms we automatically translate FX files to GLSL using a library called [MojoShader](http://icculus.org/mojoshader/).  The supported feature levels are the following:
  * `vs_2_0` and `ps_2_0`
  * `vs_3_0` and `ps_3_0`
* You can use preprocessor checks to add conditional code or compilation depending on defined symbols. MonoGame defines the following symbols when compiling effects:
  * `2MGFX`
  * `HLSL` and `SM4` for DirectX
  * `OpenGL` and `GLSL` for OpenGL
  
  As an example, you can conditionally set shader models depending on the platform with the following code:
  ```
  #if OPENGL
      #define VS_SHADERMODEL vs_3_0
      #define PS_SHADERMODEL ps_3_0
  #else
      #define VS_SHADERMODEL vs_4_0_level_9_1
      #define PS_SHADERMODEL ps_4_0_level_9_1
  #endif

  technique
  {
      pass
      {
          VertexShader = compile VS_SHADERMODEL MainVS();
          PixelShader = compile PS_SHADERMODEL MainPS();
      }
  };
  ```
  Custom symbols can be defined from the [Pipeline Tool](pipeline.md) or via [2MGFX](2mgfx.md).
* Make sure the pixel shaders inputs **exactly match** the vertex shader outputs so the parameters are passed in the correct registers. The parameters need to have the same size and order. Omitting parameters might not break compilation, but can cause unexpected results.
* Note that on GL platforms default values on Effect parameters do not work.  Either set the parameter from code or use a real constant like a #define.
* The effect compiler is aggressive about removing unused parameters, be sure the parameters you are setting are actually used.
* Preshaders are not supported.
* If you think you've found a bug porting a shader [please let us know](https://github.com/MonoGame/MonoGame/issues).
