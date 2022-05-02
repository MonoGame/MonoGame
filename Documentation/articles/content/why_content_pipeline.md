# Why use the Content Pipeline

The MonoGame team has been putting a lot of effort into a cross-platform content pipeline, but would you use the Content Pipeline when MonoGame also supports loading assets natively, like .png, .mp3, .wav? Well, it all boils down to a couple of words, performance and efficiency.

## Textures

Most image formats (like PNGs) are generally not optimized for games, most GPUs have specific hardware supported compressed formats for textures. PNGs or JPEGs just get uncompressed when passed to the GPU. By using the hardware compression you often get 4x to 8x more textures space for your game as well as faster load times and smaller packages.

When we load the .png from storage at runtime, the texture is then loaded into memory and decompressed/unpacked from its compressed png format into raw bytes.

> Note that 262 KB  is quite a bit bigger than the compressed size.

A new texture is then created for that data because your device cannot decompress on the fly (yet) so it has to use that data as is. Creating the texture uses 262kb of graphics memory on the GPU. That does not sound too bad, but if you are using larger textures say 1024×1024, then you are using 4 MB or more of GPU memory for that one texture. Multiply that over the number of textures in your game and you soon run out of texture memory on the GPU. If this happens then the GPU has to swap that data out into system memory (if it supports that) or throw an error when you try to create textures that will not fit into available memory.

> So to sum up:
>
> using **.pngs** = smaller package size & higher memory usage & less textures

If you pre-process the texture using the content pipeline, because we know that we are targeting iOS and we know the GPU on those devices support using PVRTC texture compression, we can compress the textures ready for the GPU to use. So we take our sample .png and compress it at build time using PVRTC and we end up with a 32kb file (size depends on the texture, alpha channel, etc). Hmm, that is a lot bigger than the .png on disk, but that is not the whole story. The difference here is that there is no need to unpack/decompress it at runtime which saves on load time, also, we can create a texture from that data directly so we only use 32kb of texture memory on the GPU and not 262kb. That is a massive saving.

> Summing up:
>
> **compressed textures** = larger package size (maybe) & lower runtime memory usage & more textures

This applies to all platforms as most desktop GPUs support DXT texture compression, so the content pipeline will produce DXT compressed textures which can be loaded and used directly. The Android platform currently does not have consistent support for compressed textures at the moment so MonoGame has to decompress DXT textures on the device and use it directly, when this changes MonoGame will adapt.

In the Content Pipeline tool, MonoGame will automatically pick the correct texture format to use, so for opaque textures, it will use ETC1 (which is supported on all android devices but does not support alpha channels) but for textures with an alpha channel, it will use RGBA4444 (dithered). It will also allow the user to override this default and enable picking from a wide variety of compression options manually such as PVRTC, ATITC, DXT/S3TC, ETC1, and RGBA4444. This will give the developer the choice of what to use/support.

## Audio

All platforms support different audio formats, if you are handling this yourself you will need to manually convert all your files and include the right formats for each platform. A better option would be to keep one source file (be it .mp3, .wmv, etc) and then convert it to a supported format for the target platform at build time. This creates longer build times, but at least we know the music will work. MonoGame uses ffmpeg to do the heavy lifting when converting between formats as it can pretty much convert any type to any other type which is really cool.

> The build times are only for the first time the asset is processed, the Content Pipeline will preserve the generated content for subsequent builds until the source is changed

Most platforms have audio processing that is optimized to certain compressed formats. By not using them you loose performance and system memory.  If you save the sound effects as ADPCM, these run pretty optimally on Windows systems. The problem however, is that once you try to take your game to another platform where ACPCM is not optimal. You will need to manually re-export all your game content into the new optimal format for that platform (assuming you kept all your original content uncompressed)

## Shaders

There are a number of shading languages that you can use depending on the platform you are targeting:

- For OpenGL based systems that is GLSL
- For DirectX based systems it is HLSL
- There is also CG from Nvidia.

The Effect system in XNA/MonoGame was designed around the HLSL language as it is based around the .fx format, which allows a developer to write both vertex and pixel shaders in one place. Historically both GLSL and HLSL have separate vertex and pixel shaders.  HLSL until recently compiled and linked these at build time, however, GLSL does this at runtime.

Without a content pipeline or some form of tooling a developer would need to write two shaders, one for HLSL and one for GLSL. The good news is the MonoGame MGFX.exe tool can create a shader in .fx format and also enable it to work in GLSL. It does this by using an open-source library called libmojoshader, which does some funky HLSL to GLSL instruction conversion to create an OpenGL-based shader, but rather than doing that at runtime, we do it at build time so we do not need to deploy mojoshader with the OpenGL based games. All this saves you the hassle of having to write and maintain two shaders.

> From MonoGame 3.8, the MGFX tool can build content on any platform. For MonoGame 3.7 or earlier, MGFX only runs on a windows box, this is because it needs the DirectX shader tooling to compile the HLSL before passing it to libmojoshader for conversion (for OpenGL platform targets).

## Models

MonoGame, thanks to the excellent assimp project, supports a much wider range of models including .x, .fbx and .3ds. However, some of these formats might produce some weirdness at render time as only .fbx has been fully tested. Also note that assimp does not support the older version .fbx format which ships with most of the original XNA samples, so you will need to [convert those to the new format manually](https://www.codeproject.com/articles/1041397/updating-old-fbx-files-for-the-modern-era).

> A nice trick to upgrade the old .fbx is to open them in Visual Studio 2012+ and then save it again under a new name. [This article](https://www.codeproject.com/articles/1041397/updating-old-fbx-files-for-the-modern-era) walks through some of the quick, easy, and FREE methods for updating older .fbx files.

When the MonoGame content pipeline processes a Model file, it is converted into an optimized internal format that will contain the Vertices, Texture Coordinates, and Normals. The pipeline will also pull out the textures used in the model and put those through the pipeline too, so you automatically get optimized textures automatically.

## Summary

This is why using the content pipeline and XNB's is superior. You feed uncompressed source content to the MonoGame content pipeline, tell it what platform you are building for and it takes care of converting it to the most optimal format for that platform. When you decide to ship to another platform it is a one-click change to rebuild your content optimized for that platform.

Hopefully, you have a good idea of why you should use the content pipeline in your games. Using the raw assets is ok when you are putting together a simple demo or proof of concept but sooner or later you will need to start optimizing your content. My advice would be to use the Pipeline tooling from the outset so you get used to it.

Information on the [Pipeline tool](~/articles/tools/mgcb_editor.md) can be found here.
