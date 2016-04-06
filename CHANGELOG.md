# Change Log


## 3.5.1 Release - 3/30/2016

 - Fixed negative values when pressing up on left thumbstick on Mac.
 - Removed exception and just return empty state when requesting an invalid GamePad index.
 - Fixed texture processing for 64bpp textures.
 - Fixed Texture2D.SaveAsPng on Mac.


## 3.5 Release - 3/17/2016

 - Content Pipeline Integration for Xamarin Studio and MonoDevleop on Mac and Linux.
 - Automatic inclusion of XNBs into your final project on Mac and Linux.
 - Improved Mac and Linux installers.
 - Assemblies are now installed locally on Mac and Linux just like they are on Windows.
 - New cross-platform “Desktop” project where same binary and content will work on Windows, Linux and Mac desktops.
 - Better Support for Xamarin.Mac and Xam.Mac.
 - Apple TV support (requires to be built from source at the moment).
 - Various sound system fixes.
 - New GraphicsMetrics API.
 - Optimizations to SpriteBatch performance and garbage generation.
 - Many improvements to the Pipeline tool: added toolbar, new filtered output view, new templates, drag and drop, and more.
 - New GamePad support for UWP.
 - Mac and Linux now support Vorbis compressed music.
 - Major refactor of texture support in content pipeline.
 - Added 151 new unit tests.
 - Big improvements to FBX and model content processing.
 - Various fixes to XML serialization.
 - MediaLibrary implementation for Windows platforms.
 - Removed PlayStation Mobile platform.
 - Added content pipeline extension template project.
 - Support for binding multiple vertex buffers in a draw call.
 - Fixed deadzone issues in GamePad support.
 - OcclusionQuery support for DX platforms.
 - Fixed incorrect z depth in SpriteBatch.
 - Lots of OpenTK backend fixes.
 - Much improved font processing.
 - Added new VertexPosition vertex format.
 - Better VS project template installation under Windows.


## 3.4 Release - 4/29/2015

 - Removed old XNA content pipeline extensions.
 - Added all missing PackedVector types.
 - Replacement of old SDL joystick path with OpenTK.
 - Added SamplerState.ComparisonFunction feature to DX and OGL platforms.
 - Fixed bug where content importers would not be autodetected on upper case file extensions.
 - Fixed compatibility with XNA sound effect XNBs.
 - Lots of reference doc improvements.
 - Added SamplerState.BorderColor feature to DX and OGL platforms.
 - Lots of improvements to the Mac, Linux and Windows versions of the Pipeline GUI tool.
 - Fixes for bad key mapping on Linux.
 - Support for texture arrays on DX platforms.
 - Fixed broken ModelMesh.Tag
 - VS templates will now only install if VS is detected on your system.
 - Added Color.MonoGameOrange.
 - Fixed Xact SoundBack loading bug on Android.
 - Added support for a bunch of missing render states to MGFX.
 - Added support for sRGB texture formats to DX and OGL platforms.
 - Added RasterizerState.DepthClipEnable support for DX and OGL platforms.
 - New support for the Windows 10 UAP plafform.
 - Fixed bug which caused the GamePad left thumbstick to not work correctly.
 - Preliminary base classed for future Joystick API.
 - Performance improvement on iOS by avoiding unnessasary GL context changes.
 - Fixed bug where MediaPlayer volume affected all sounds.
 - New XamarinStudio/MonoDevelop Addin for Mac.
 - New Mac installer packages.


## 3.3 Release - 3/16/2015

 - Support for vertex texture fetch on Windows.
 - New modern classes for KeyboardInput and MessageBox.
 - Added more validation to draw calls and render states.
 - Cleaned up usage of statics to support multiple GraphicsDevice instances.
 - Support Window.Position on WindowsGL platform.
 - Reduction of redundant OpenGL calls.
 - Fullscreen support for Windows DX platform.
 - Implemented Texture2D SaveAsPng and SaveAsJpeg for Android.
 - Improved GamePad deadzone calculations.
 - We now use FFmpeg for audio content building.
 - BoundingSphere fixes and optimizations.
 - Many improvements to Linux platform.
 - Various fixes to FontTextureProcessor.
 - New Windows Universal App template for Windows Store and Windows Phone support.
 - Many fixes to reduce garbage generation during runtime.
 - Adding support for TextureFormatOptions to FontDescriptionProcessor.
 - XNA compatibility improvements to FontDescriptionProcessor.
 - Resuscitated the unit test framework with 100s of additional unit tests.
 - BoundingFrustum fixes and optimizations.
 - Added VS2013 project templates.
 - Moved to new MonoGame logo.
 - Added MSAA render target support for OpenGL platforms.
 - Added optional content compression support to content pipeline and runtime.
 - TextureCube content reader and GetData fixes.
 - New OpenAL software implementation for Android.
 - Xact compatibility improvements.
 - Lots of Android fixes and improvements.
 - Added MediaLibrary implementation for Android, iOS, Windows Phone, and Windows Store.
 - Added ReflectiveWriter implementation to content pipeline.
 - Fixes to Texture2D.GetData on DirectX platforms.
 - SpriteFont rendering performance optimizations.
 - Huge refactor of ModelProcessor to be more compatible with XNA.
 - Moved NET and GamerServices into its own MonoGame.Framework.Net assembly.
 - Runtime support for ETC1 textures for Androud.
 - Improved compatibility for FBXImporter and XImporter.
 - Multiple SpritBatch compatibility fixes.
 - We now use FreeImage in TextureImporter to support many more input formats.
 - MGFX parsing and render state improvements.
 - New Pipeline GUI tool for managing content projects for Windows, Mac, and Linux desktops.
 - New implementation of content pipeline IntermediateSerializer.
 - All tools and content pipeline built for 64-bit.
 - New documentation system.
 - Implement web platform (JSIL) stubs.
 - Lots of fixes to PSM.
 - Added Protobuild support for project generation.
 - Major refactor of internals to better separate platform specific code.
 - Added MGCB command line tool to Windows installer.


## 3.2 Release - 4/7/2014

 - Implemented missing PackedVector types.
 - VS2013 support for MonoGame templates.
 - Big improvement to XInput performance on Windows/Windows8.
 - Added GameWindow.TextInput event enhancement.
 - Added Xamarin.Mac compatability.
 - Support for WPF interop under DirectX.
 - Enhancement to support multiple GameWindows on Windows under DirectX.
 - Various SpriteFont compatibility improvements.
 - OpenAL performance/memory/error handling improvements.
 - Reduction of Effect runtime memory usage.
 - Support for DXT/S3TC textures on Android.
 - Touch support on Windows desktop games.
 - Added new RenderTarget3D enhancement.
 - OUYA gamepad improvements.
 - Internal improvements to reduce garbage generation.
 - Various windowing fixes for OpenTK on Linux, Mac, and Windows.
 - Automatic support for content reloading on resume for Android.
 - Support for TextureCube, Texture3D, and RenderTargetCube on DirectX.
 - Added TitleContainer.SupportRetina enhancement for loading @2x content.
 - Lots of Android/Kindle compatibility fixes.
 - Added enhancement GameWindow.IsBorderless.
 - OpenGL now supports multiple render targets.
 - Game.IsRunningSlowly working accurately to XNA.
 - Game tick resolution improvements.
 - XACT compatibility improvements.
 - Various fixes and improvements to math types.
 - DrawUserIndexedPrimitives now works with 32bit indicies.
 - GamerServices fixes under iOS.
 - Various MonoGame FX improvements and fixes.
 - Render target fixes for Windows Phone.
 - MediaPlayer/MediaQueue/Song fixes on Windows Phone.
 - XNA accuracy fixes to TitleContainer.
 - Fixes to SpriteBatch performance and compatibility with XNA.
 - Threading fixes around SoundEffectInstance.
 - Support for Song.Duration.
 - Fixed disposal of OpenGL shader program cache.
 - Improved support of PoT textures in OpenGL.
 - Implemented missing EffectParameter SetValue/GetValue calls.
 - Touch fixes to Windows Phone.
 - Fixes to orientation support in iOS.
 - Lots of PSM fixes which make it usable for 2D games.
 - New Windows desktop platform using DirectX/XAudio.
 - Old Windows project renamed WindowsGL.
 - Fixed offsetInBytes parameter in IndexBuffer/VertexBuffer SetData.
 - Fixed subpixel offset when viewport is changed in OpenGL.
 - Tons of content pipeline improvements making it close to complete.


## 3.0.1 Release - 3/3/2013

 - Fix template error.
 - Fix offsetInBytes parameter in IndexBuffer/VertexBuffer SetData.
 - Fixes the scale applied on the origin in SpriteBatch.
 - Fixed render targets on WP8.
 - Removed minVertexIndex Exception.
 - Fixed some threading issues on iOS.
 - Use generic link for opening store on iOS.
 - Fix Matrix::Transpose.
 - Fixed vertexOffset in DrawUserIndexedPrimitives in GL.
 - Keys.RightControl/RightShift Support for WinRT.
 - Dispose in ShaderProgramCache.
 - IsRunningSlowly Fix.


## 3.0 Release - 1/21/2013

 - 3D (many thanks to Infinite Flight Studios for the code and Sickhead Games in taking the time to merge the code in).
 - New platforms: Windows 8, Windows Phone 8, OUYA, PlayStation Mobile (including Vita).
 - Custom Effects.
 - PVRTC support for iOS.
 - iOS supports compressed Songs.
 - Skinned Meshs.
 - VS2012 templates.
 - New Windows Installer.
 - New MonoDevelop Package/AddIn.
 - A LOT of bug fixes.
 - Closer XNA 4 compatibility.


## 2.5.1 Release - 6/18/2012

 - Updated android to use enumerations rather than hardocded ids as part of the Mono for Android 4.2 update.
 - Changed the Android video player to make use of the ViewView.
 - Corrected namespaces for SongReader and SoundEffectReader.
 - Updated the Keyboard mapping for android.
 - Added RectangleArrayReader.
 - Removed links to the third party GamePadBridge.
 - Added some missing mouseState operators.
 - Replaced all calls to DateTime.Now with DateTime.UtcNow.
 - Fixed SpriteFont rendering (again).
 - Added code to correclty dispose of Textures on all platforms.
 - Added some fixes for the sound on iOS.
 - Adding missing MediaQueue class.
 - Fixed Rectangle Intersect code.
 - Changed the way UserPrimitives work on windows.
 - Made sure the @2x file support on iOS works.
 - Updated project templates.
 - Added project templates for MacOS.
 - Fixed MonoDevelop.MonoGame AddIn so it works on Linux.
 

## 2.5 Release - 3/29/2012

### Fixes and Features
 - Minor fixes to the Networking stack to make it more reliable when looking for games.
 - SpriteBatch Fixes including making sure the matrix parameter is applied in both gles 1.1 and gles 2.0.
 - Updated IDrawable and IUpdatable interfaces to match XNA 4.0.
 - Fixed the Tick method.
 - Updated VideoPlayer constructor contract to match XNA 4.0.
 - Added Code to Lookup the Host Application Guid for Networking, the guid id is now pulled from the AssemblyInfo.cs if one is present.
 - Uses OpenAL on all platforms except Android.
 - Added Dxt5 decompression support.
 - Improves SpriteFont to conform more closely to XNA 4.0.
 - Moved DynamicVertexBuffer and DynamicIndexBuffer into its own files.

### iOS
 - Fixed Console.WriteLine problem.
 - Fixed loading of @2x Retina files.
 - Fixed Landscape Rendering.
 - Fixed Orientations changes correctly animate.
 - Fixed Guide.BeginShowKeyboardInput.
 - Fixed StorageDevice AOT compile problem.
 - Fixed SpriteBatch to respect matrices when drawn.
 - Fixed DoubleTap, improves touches in serial Game instances.
 - Fixed App startup in non-Portrait orientations.
 - Fixed UnauthorizedAccessException using TitleContainer.
 - Fixed a runtime JIT error that was occuring with List<AddJournalEntry<T>().
 - Guide.ShowKeyboard is not working.
 - App Backgrounding has regressed. A patch is already being tested in the develop branch and the fix will be rolled out as part of the v2.5.1.

### Android
 - Project Templates for MonoDevelop.
 - Fixed a few issues with Gestures.
 - Fixed the name of the assembly to be MonoGame.Framework.Android.
 - Fixed a Memory Leak in Texture Loading.
 - Force linear filter and clamp wrap on npot textures in ES2.0 on Android.
 - Added SetData and GetData support for Texture2D.
 - Guide.SignIn picks up the first email account on the phone.
 - CatapultWars does not render correctly under gles 1.1.

### MacOS X
 - SoundEffectInstance.Stop now works correctly.

### Linux
 - Project Templates for Visual Studio and MonoDevelop.
 - Fixed a bug when loading of Wav files.

### Windows
 - Project Templates for Visual Studio and MonoDevelop.
 - Fixed a bug when loading of Wav files.
 - Added Game.IsMouseVisible implementation for Windows.
 - Guide.SignIn picks up the logged in user.
 - Added a new Installer to install the MonoDevelop and / or Visual Studio Templates and binaries.


## 2.1 Release - 10/28/2011

### Features
 - Content Manager rewritten to use partial classes and implementation of cached assets that are loaded.  Greatly improves memory footprint.
 - Experimental support for GamePads and Joysticks.  Enhancements will be coming to integrate better for developers.
 - ContentReader improvements across the board.
 - Improved support for XACT audio.
 - StarterKits VectorRumble.

### iOS
 - Gesture support has been improved.
 - Better support for portrait to landscape rotations.
 - Fixed a rendering bug related to upsidedown portrait mode.
 - Better WaveBank support.
 - The Guide functionality is only available in iOS, for this release.

### Android
 - Updated to support Mono for Android 4.0.
 - Improvements to the Orientation Support.
 - Changed Sound system to use SoundPool.
 - Added Tap and DoubleTap Gesture Support.

### MacOS X
 - A lot of enhancements and fixes for Full Screen and Windowed control.
 - Cursor support fixed for IsMouseVisible.
 - Implementation of IsActive property and the events Activated and Deactivated.
 - First steps of DrawPrimitives, DrawUserPrimitives, DrawIndexedPrimitives.
 - Better WaveBank support.
 - Support for ApplyChanges() and setting the backbuffer and viewport sizes correctly.

### Linux
 - All new implementation which share quite a bit of code between MacOS X and Windows.
 - Added shader support via the Effects class.

### Windows
 - All new implementation which shares quite a bit of code between MacOS and Linux.


## 2.0 Release - 10/28/2011

 - Project renamed MonoGame.
 - Project moved to GitHub.
 - Support for Linux, Mac, Linux, and OpenGL on Windows.


## 0.7 Release - 12/2/2009

 - First stable release.
 - Originally named XnaTouch.
 - iPhone support only.
 - 2D rendering support.
 - Audio support.
 - Networking support.
 - Partial multitouch support.
 - Partial accelerometer support.
