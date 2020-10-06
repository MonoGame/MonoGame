# Why use the Content Pipeline

The MonoGame team has been putting a lot of effort into a cross-platform content pipeline, but given that for the most part we support loading native assets like .png, .mp3, .wav why bother? Well, it all boils down to a couple of words.. performance, efficiency. Let us look at an example, graphics are some of the biggest assets games use, they are also a major resource hog. Textures will probably take up most of the room in your deployment and will be taking up most of the memory on your device as well.

## Textures

Formats like PNG's are generally not optimized for games.

For example, most GPUs have specific hardware supported compressed formats for textures. PNGs or JPEGs just get uncompressed when passed to the GPU. By using the hardware compression you often get 4x to 8x more textures space for your game as well as faster load times and smaller packages.

If we have a 256×256 32 bit .png texture that we are using in our game, we don’t want to bother with all this compiling to .xnb rubbish that people do, we just want to use the texture as a raw .png file. On disk .png is very impressive in its size, that image probably only takes up 2-5 kb on disk, keeping your application package size down. Great!

Now, what happens when we load this .png from storage on a device (like an iPhone). Firstly it is loaded from storage into memory and decompressed/unpacked from its compressed png format into raw bytes. This is done because the GPU on your device doesn’t know how to use a png image directly, it can only use certain types of compression. So we unpacked the image into memory, this is 262,144 bytes (256x256x4, the x4 is because we have 1 byte per channel Red, Green, Blue, and Alpha). 

> Note that 262 KB  is quite a bit bigger than the compressed size. 

The next thing to do is create a texture for that data because your device cannot compress on the fly (yet), it has to use that data as is. So in creating the texture we used 262kb  of graphics memory on the GPU. That doesn’t sound too bad, but if you are using larger textures say 1024×1024 then you are using 4 MB of GPU memory for that one texture. Multiply that over the number of textures in your game and you soon run out of texture memory on the GPU. Then the GPU has to swap that data out into system memory (if it supports that) or throw an error when you try to create textures that won’t fit into available memory. 

So to sum up:

> using **.pngs** = smaller package size & higher memory usage & less textures

Now let us look at a texture that has been pre-processed using the content pipeline, because we know we are targeting iOS we know the GPU’s on those devices support using PVRTC texture compression directly. So we take our sample .png and compress it at build time using PVRTC and we end up with is a 32kb file (size depends on the texture, alpha channel, etc). Hmm, that is a lot bigger than the .png on disk, but that is not the whole story. The difference here is that there is no need to unpack/decompress it which saves on load time, also, we can create a texture from that data directly so we only use 32kb of texture memory on the GPU and not 262kb. That is a massive saving.

Summing up:

> **compressed textures** = larger package size (maybe) & lower memory usage & more textures

Now we only looked at iOS, but the same applies to desktop environments, most desktop GPUs support DXT texture compression, so the content pipeline will produce DXT compressed textures which can be loaded and used directly. The only platform that is a pain is Android because the Android platform does not have consistent support for compressed textures at the moment and MonoGame has to decompress DXT textures on the device and use it directly. However, even Android will get compressed texture support eventually. There is also a piece of work happening where the pipeline tool will automatically pick a texture format to use, so for opaque textures, it will use ETC1 (which is supported on all android devices but doesn’t support alpha channels) but for textures with an alpha channel, it will use RGBA4444 (dithered) but also allow the user to pick from a wide variety of compression options manually such as PVRTC, ATITC, DXT/S3TC, ETC1, and RGBA4444. This will give the developer the choice of what to use/support.

## Audio

Now let us look at audio. All the different platforms support different audio formats, if you are handling this yourself you will need to manually convert all your files and include the right ones for each platform. A better option would be to keep one source file (be it .mp3, .wmv, etc) and then convert it to a supported format for the target platform at build time? Ok, it does make for longer build times, but at least we know the music will work. MonoGame uses ffmpeg to do the heavy lifting when converting between formats as it can pretty much convert any type to any other type which is really cool.

Most platforms have audio processing that is optimized to certain compressed formats. By not using them you loose performance and system memory.  Often this is when people resort to just using a format optimized for your current target platform. You could save the sound effects as ADPCM and run pretty optimally on Windows systems. The problem here is only apparent once you try to take your game to another platform where ACPCM is not optimal. You need to manually re-export all your game content in the new optimal format for that platform… and you did keep your original content uncompressed right?

## Shaders

This is an area that causes real pain, custom shaders. There are a number of shading languages that you can use depending on the platform you are targeting. For OpenGL based systems that is GLSL, for DirectX based systems it is HLSL, there is also CG from Nvidia. 

The Effect system in XNA/MonoGame was designed around the HLSL language as it is based around the .fx format, which allows a developer to write both vertex and pixel shaders in one place. Historically both GLSL and HLSL have separate vertex and pixel shaders.  HLSL until recently compiled and linked these at build time, however, GLSL does this at runtime. Now without a content pipeline or some form of tooling a developer would need to write two shaders, one for HLSL and one for GLSL. The good news is the MonoGame MGFX.exe tool can create a shader in .fx format and also enable it to work in GLSL. It does this by using an open-source library called libmojoshader, which does some funky HLSL to GLSL instruction conversion to create an OpenGL-based shader, but rather than doing that at runtime, we do it at build time so we do not need to deploy mojoshader with the OpenGL based games. All this saves you the hassle of having to write and maintain two shaders.

> From MonoGame 3.8, the MGFX tool can build content on any platform. For MonoGame 3.7 or earlier, MGFX only runs on a windows box, this is because it needs the DirectX shader tooling to compile the HLSL before passing it to libmojoshader for conversion (for OpenGL platform targets).

## Models

For the most part, the model support in XNA/MonoGame is pretty good. XNA supports .x, .fbx files for 3D models.  MonoGame, thanks to the excellent assimp project, supports a much wider range of models including .3ds. However, some of these formats might produce some weirdness at render time as only .fbx has been fully tested. Also note that assimp does not support the older version .fbx format which ships with most of the original XNA samples, so you will need to [convert those to the new format manually](https://www.codeproject.com/articles/1041397/updating-old-fbx-files-for-the-modern-era). 

> A nice trick I found was to open the old .fbx in Visual Studio 2012+ and then save it again under a new name. Oddly VS seems to know about .fbx files and will save the model in the new format :).  [This article](https://www.codeproject.com/articles/1041397/updating-old-fbx-files-for-the-modern-era) walks through some of the quick, easy, and FREE methods for updating older .fbx files.

Now what happens when you use a 3D model, is that it is converted by the pipeline into an optimized internal format that will contain the Vertices, Texture Coordinates, and Normals. The pipeline will also pull out the textures used in the model and put those through the pipeline too, so you automatically get optimized textures without having to do all of that stuff yourself (talk about making it easy).

## Summary

This is why using the content pipeline and XNBs is superior. You feed uncompressed source content to the MonoGame content pipeline, tell it what platform you’re building for, and it takes care of converting it to the most optimal format for that platform. When you decide to ship to another platform… it is a one-click change to rebuild your content optimized for that platform.

Hopefully, you have got a good idea of why you should use the content pipeline in your games. Using the raw assets is ok when you are putting together a simple demo or proof of concept but sooner or later you will need to start optimizing your content. My advice would be to use the Pipeline tooling from the outset so you get used to it.

Information on the [Pipeline tool](~/articles/tools/mgcb_editor.md) can be found here.
