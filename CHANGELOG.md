# Changelog
 
## 3.8.1 HOTFIX Release - July 26, 2022

## What's Changed
 - Fix MonoGame publisher name for the VS marketplace by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7847
 - Corrected 'Framwork' to 'Framework' by @benjitrosch in https://github.com/MonoGame/MonoGame/pull/7850
 - added missing words to platforms.md by @MrGrak in https://github.com/MonoGame/MonoGame/pull/7852
 - Fixed grammatical error from 'project' to 'projects' by @benjitrosch in https://github.com/MonoGame/MonoGame/pull/7855
 - Grammar Hotfix on README by @Emersont1 in https://github.com/MonoGame/MonoGame/pull/7856
 - Remove redundant dependencies by @vpenades in https://github.com/MonoGame/MonoGame/pull/7854
 - Fixes Visual Studio freezing with the extension by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7857
 - Updated migration guide by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7858
 - More migration guide updates by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7859
 - Wild cards don't work in dotnet-tools.json by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7860
 - Removed explicit global usings by @vpenades in https://github.com/MonoGame/MonoGame/pull/7853

## 3.8.1 Release - July 24, 2022

## What's Changed
 - Update build version to 3.8.1.xxxx by @tomspilman in https://github.com/MonoGame/MonoGame/pull/7296
 - Fix broken links by @rejurime in https://github.com/MonoGame/MonoGame/pull/7297
 - [DesktopGL] Fix setting backbuffer size not working in constructor by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7299
 - Add quotation marks around MGCBPath by @bjornenalfa in https://github.com/MonoGame/MonoGame/pull/7304
 - [Templates] Update FSharp projects by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7302
 - fix Texture.GetSharedHandle() by @nkast in https://github.com/MonoGame/MonoGame/pull/7306
 - [MGCB Editor] Update registration handling files by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7311
 - Updated to the latest version of StbSharp by @rds1983 in https://github.com/MonoGame/MonoGame/pull/7312
 - Support MSAA in SwapChainRenderTarget by @nkast in https://github.com/MonoGame/MonoGame/pull/7307
 - Updated link for the XNA 3.1 to XNA 4.0 cheatsheet by @SimonDarksideJ in https://github.com/MonoGame/MonoGame/pull/7321
 - Updated and added links to latest MonoGame 3.8 content in tutorials.md by @SimonDarksideJ in https://github.com/MonoGame/MonoGame/pull/7322
 - Explicitly document BackToFront and FrontToBack use an unstable sort. by @goosenoises in https://github.com/MonoGame/MonoGame/pull/7323
 - Update mgfxc_wine_setup.sh by @Kwyrky in https://github.com/MonoGame/MonoGame/pull/7327
 - Fix multisampling in DesktopGL by @sk-zk in https://github.com/MonoGame/MonoGame/pull/7338
 - [MGCB Editor] macOS fixes by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7337
 - Fix shader error messages for OpenGL by @cpt-max in https://github.com/MonoGame/MonoGame/pull/7340
 - Added IEffectBones interface, implemented by SkinnedEffect. by @vpenades in https://github.com/MonoGame/MonoGame/pull/7344
 - Update NVTT (fixes #5866) by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7354
 - Edited the Getting Started parts of the documentation for grammar, clarity, and style by @HopefulFrog in https://github.com/MonoGame/MonoGame/pull/7374
 - Updated Matrix.CreatePerspective - to support infinite far planes. by @vpenades in https://github.com/MonoGame/MonoGame/pull/7367
 - Add missing underscore in code section by @pbedn in https://github.com/MonoGame/MonoGame/pull/7349
 - Update CONTRIBUTING.md by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7394
 - Added ICurveEvaluator{T} interface. by @vpenades in https://github.com/MonoGame/MonoGame/pull/7387
 - Update Eto.Froms for VS for Mac addin by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7365
 - Fix for MGFXO file format to increases collection size limits. by @vpenades in https://github.com/MonoGame/MonoGame/pull/7397
 - NUnit Test Attachments To preview Test images in VS. by @vpenades in https://github.com/MonoGame/MonoGame/pull/7413
 - [macOS] Properly check current folder if we are inside of .app bundle by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7416
 - fix 7402: enqueue buffer ref before sending to OAL by @lodicolo in https://github.com/MonoGame/MonoGame/pull/7403
 - Fixes 3D Audio Direction Issues On Platforms Using OpenAL by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7404
 - Fixes Stopping Looped Sounds On Platforms Using XAudio by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7405
 - Fixes Buffer Bindings Cache Issue On Platforms Using OpenGL by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7406
 - Fixes 3D Audio Direction Issues On Platforms Using X3DAudio by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7389
 - Proposal for adding basic interoperability with System.Numerics.Vectors by @vpenades in https://github.com/MonoGame/MonoGame/pull/7417
 - [VSMac] Mark .spritefont files as .xml files by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7426
 - Updated console references by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7432
 - Removed residual OUYA references by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7433
 - [VSMac] Fix freeze upon opening a mgcb file and bump dependencies by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7435
 - [MGCB Editor] Fix help link (fixes #7428) by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7438
 - Changed uses of Math to MathF where it was used for floats by @initram in https://github.com/MonoGame/MonoGame/pull/7390
 - [MGCB.Task] Separate obj folder per target framework (fixes #7409) by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7441
 - Revert "Changed uses of Math to MathF where it was used for floats" by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7442
 - File drop event on DesktopGL and WindowsDX platforms by @Quant1um in https://github.com/MonoGame/MonoGame/pull/7362
 - [MGCB Editor] Bump Eto.Forms version (fixes #7418) by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7439
 - Update SDL to 2.0.14 by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7445
 - Help with compatibility toward NativeAOT by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7462
 - Generic Threading.BlockOnUIThread for reducing garbage allocs by @TechPizzaDev in https://github.com/MonoGame/MonoGame/pull/7384
 - Fix UWP Vsync support by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7463
 - [DesktopGL] Improve mouse handling outside gamewindow by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7440
 - Updated to the latest StbSharp 2.22.5, which added support for HDR images by @rds1983 in https://github.com/MonoGame/MonoGame/pull/7467
 - Fix type collisions and default implementations for consoles by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7466
 - Removed threading limitations by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7472
 - Fixed a typo in the comments. charaters --> characters by @rbwhitaker in https://github.com/MonoGame/MonoGame/pull/7473
 - fixed grammar in CONTRIBUTING.md by @algobytewise in https://github.com/MonoGame/MonoGame/pull/7469
 - Fixed a small handful of typos in the comments of SpriteFont. by @rbwhitaker in https://github.com/MonoGame/MonoGame/pull/7479
 - Update copyright year to 2021 in LICENSE.txt by @monegit in https://github.com/MonoGame/MonoGame/pull/7482
 - Fixes PacketNumber For Non-Haptic Gamepads On Platforms Using SDL by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7487
 - Fixes Duplicate/Missing Gamepads On Platforms Using SDL by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7486
 - Bump GtkSharp to fix crashes on Arch Linux by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7493
 - Update NVorbis submodule by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7495
 - Added selective ContentManager asset unloading by @kimimaru4000 in https://github.com/MonoGame/MonoGame/pull/6663
 - Update Mac Extension Link by @gideongrinberg in https://github.com/MonoGame/MonoGame/pull/7497
 - Remove deprecated property Color.TransparentBlack by @nkast in https://github.com/MonoGame/MonoGame/pull/7505
 - Remove duplicate check with wrong argument name by @nkast in https://github.com/MonoGame/MonoGame/pull/7504
 - fix spelling error by @nkast in https://github.com/MonoGame/MonoGame/pull/7503
 - Prevent App suspension when the user press 'B' button on UWP (xbox controller) by @nkast in https://github.com/MonoGame/MonoGame/pull/7500
 - fix PipelineBuildEvent deserialization by @nkast in https://github.com/MonoGame/MonoGame/pull/7502
 - Support drawing point primitives by @roy-t in https://github.com/MonoGame/MonoGame/pull/7477
 - Update samples.md by @Michael1993 in https://github.com/MonoGame/MonoGame/pull/7358
 - Merged the two requirements files into the REQUIREMENTS.md by @RicoGuerra in https://github.com/MonoGame/MonoGame/pull/7484
 - Upgrade tooling to .NET 5 by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7437
 - Upgraded projects and documentation to NET5 by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7514
 - Changed Math to MathF by @initram in https://github.com/MonoGame/MonoGame/pull/7451
 - Shared MSAA RenderTarget by @nkast in https://github.com/MonoGame/MonoGame/pull/7506
 - Added pixels processing function parameter to the Texture2D.FromStream/TextureFromFile by @rds1983 in https://github.com/MonoGame/MonoGame/pull/7369
 - Docs/content pipeline update by @SimonDarksideJ in https://github.com/MonoGame/MonoGame/pull/7370
 - Don't recompile the same shader multiple times. by @cpt-max in https://github.com/MonoGame/MonoGame/pull/7392
 - Update Game.cs by @SAJenkin in https://github.com/MonoGame/MonoGame/pull/7528
 - Fix FontDescriptionProcessor kerning by @nkast in https://github.com/MonoGame/MonoGame/pull/7501
 - Fix assembly trimming in templates by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7529
 - Fixed GLDesktop PlatformStop() resolving issue #7372 by @JacksonDorsett in https://github.com/MonoGame/MonoGame/pull/7453
 - Fix VSCode launch configuration for MGCB Editor on Mac by @the-maverick-m in https://github.com/MonoGame/MonoGame/pull/7535
 - OpenGL debug context for better error messages by @cpt-max in https://github.com/MonoGame/MonoGame/pull/7536
 - cask is no longer a brew command by @miluchen in https://github.com/MonoGame/MonoGame/pull/7543
 - Fix iOS GamePlatform to run pending background tasks #7520 by @Mindfulplays in https://github.com/MonoGame/MonoGame/pull/7522
 - Removed PS Vita support by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7531
 - Optimized ApplyAlpha by removing two unnecessary arithmetic operations by @rds1983 in https://github.com/MonoGame/MonoGame/pull/7555
 - [Feature] GitHub Actions integration by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7511
 - [Content Pipeline] Fix library names when packaging the nuget by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7557
 - Null song parameters for MediaPlayer.Play are not allowed in XNA by @james0x0A in https://github.com/MonoGame/MonoGame/pull/7558
 - Fixes XACT ADPCM Compression Playback by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7564
 - Fixes XACT Cue.IsPlaying Behaviour When Paused by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7563
 - Fixes EffectParameter SetValue Using Int For Float Parameters by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7568
 - Added a keyboard shortcut (F2) to rename a file. by @rbwhitaker in https://github.com/MonoGame/MonoGame/pull/7575
 - Fix NRE for Game._gameTimer by @JacksonDorsett in https://github.com/MonoGame/MonoGame/pull/7587
 - Enable setting 8 render targets when targetting Windows by @bjornenalfa in https://github.com/MonoGame/MonoGame/pull/7549
 - Fixed Comment for Intersects Method by @JacksonDorsett in https://github.com/MonoGame/MonoGame/pull/7592
 - remove unused usings from Content readers by @nkast in https://github.com/MonoGame/MonoGame/pull/7595
 - remove CLSCompliant(false) from internal members by @nkast in https://github.com/MonoGame/MonoGame/pull/7594
 - remove unused reader/writer classes by @nkast in https://github.com/MonoGame/MonoGame/pull/7597
 - Added PlayStation 5 support by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7645
 - Fix back and start buttons on iOS by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7694
 - Update stuff to .NET 6 by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7688
 - net6 mobile and UWP fixes by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7697
 - Fixed a broken link in custom_effects docs. by @davidhesselbom in https://github.com/MonoGame/MonoGame/pull/7663
 - terminology, spelling & grammar corrections by @mikeirvingweb in https://github.com/MonoGame/MonoGame/pull/7657
 - Fix comment typos in enum GestureType by @Brett208 in https://github.com/MonoGame/MonoGame/pull/7658
 - Update SDL_GameControllerDB by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7702
 - Console check fake project by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7698
 - Improve GamePad support on macOS and introduce support for rumble by @carlfriess in https://github.com/MonoGame/MonoGame/pull/7690
 - Fix mgcb building content during nuget package restore by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7709
 - Visual Studio 2022 Extension for templates by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7699
 - Fix up mgcb-editor dotnet tool by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7708
 - Optimized SpriteFont GlyphPacker by @TechPizzaDev in https://github.com/MonoGame/MonoGame/pull/7464
 - Add thread locking to the UWP gamepads dictionary by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7655
 - Multisampling on Android by @j0nat in https://github.com/MonoGame/MonoGame/pull/7561
 - proposal: additional vertex structs by @lodicolo in https://github.com/MonoGame/MonoGame/pull/7421
 - Fixes UWP Set Window Size Thread Access Error by @squarebananas in https://github.com/MonoGame/MonoGame/pull/7570
 - Fix templates for VS for Mac 2022 by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7764
 - Fix memory leak on desktop gl static variable by @adnanioricce in https://github.com/MonoGame/MonoGame/pull/7684
 - Make the VS extension to open .mgcb by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7765
 - Fix broken links to Extended samples in docs by @NogginBox in https://github.com/MonoGame/MonoGame/pull/7767
 - RTL text rendering overload for SpriteBatch.DrawString by @BlueElectivire in https://github.com/MonoGame/MonoGame/pull/7644
 - fix GamePad.GetCapabilities(...) race codition by @nkast in https://github.com/MonoGame/MonoGame/pull/7770
 - Update templates by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7771
 - Build system upgrades by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7772
 - Add automatic upload of artifacts to GitHub packages by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7773
 - [Actions] Use GITHUB_TOKEN for deploy job by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7775
 - Fix obsolete call on iOS by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7777
 - Fix SoundEffectInstance pooling by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7776
 - Update wine script for NET 6 by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7790
 - Auto versioning of VS extension by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7788
 - Added RollForward directive by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7789
 - Cleaned up rollforward from mobiles by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7791
 - Remove default Inner Exceptions by @nkast in https://github.com/MonoGame/MonoGame/pull/7800
 - fix list of supported surfaces by @nkast in https://github.com/MonoGame/MonoGame/pull/7801
 - Update submodules by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7806
 - Add Buttons.None by @TimerbaevRR in https://github.com/MonoGame/MonoGame/pull/7818
 - Fix Buttons.None by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7822
 - [Actions] Fix UWP package path by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7825
 - [Actions] Build VS for Mac addin by @harry-cpp in https://github.com/MonoGame/MonoGame/pull/7824
 - [WIP] Documentation update for 3.8.1 by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7798
 - Removed trimming property from templates by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7829
 - Create a release when pushing a tag by @mrhelmut in https://github.com/MonoGame/MonoGame/pull/7828

## 3.8 Release - August 10, 2020

### Added
 - New Plane constructor. [#6085](https://github.com/MonoGame/MonoGame/pull/6085)
 - Round, Ceiling, Floor functions for Vector2/3/4. [#6318](https://github.com/MonoGame/MonoGame/pull/6318)
 - Dotnet templating templates. [#6494](https://github.com/MonoGame/MonoGame/pull/6494)
 - TransformMatrix for SpriteEffect. [#6623](https://github.com/MonoGame/MonoGame/pull/6623)
 - SpriteBatch constructor overload with capacity parameter. [#6682](https://github.com/MonoGame/MonoGame/pull/6682)
 - [OpenGL] Support for separate blend states per render target. [#6343](https://github.com/MonoGame/MonoGame/pull/6343)
 - Multiply operator overload for Color: `scale * Color`. [#6747](https://github.com/MonoGame/MonoGame/pull/6747)
 - Overloads for garbageless BoundingBox.CreateFromPoints. [#6743](https://github.com/MonoGame/MonoGame/pull/6743)
 - UseStandardPixelAddressing flag. [#6621](https://github.com/MonoGame/MonoGame/pull/6621) [#6780](https://github.com/MonoGame/MonoGame/pull/6780)
 - [MGCB] Search fonts in HKEY_CURRENT_USER on Windows. [#6671](https://github.com/MonoGame/MonoGame/pull/6671)
 - Garbageless GetPressedKeys overload. [#6643](https://github.com/MonoGame/MonoGame/pull/6643)
 - Expose GraphicsDevice.DiscardColor to set the clear color of render targets. [#6832](https://github.com/MonoGame/MonoGame/pull/6832)
 - KeyDown and KeyUp events on GameWindow. [#6762](https://github.com/MonoGame/MonoGame/pull/6762)
 - FromFile for Texture2D and SoundEffect. [#6586](https://github.com/MonoGame/MonoGame/pull/6586)
 - PlatformInfo class to query runtime platform. [#6846](https://github.com/MonoGame/MonoGame/pull/6846) [#6873](https://github.com/MonoGame/MonoGame/pull/6873)
 - API to query graphical system at runtime. [#6872](https://github.com/MonoGame/MonoGame/pull/6872)
 - Etc2 texture format support. [#6864](https://github.com/MonoGame/MonoGame/pull/6864)
 - MonoGame.Content.Builder package. [#6905](https://github.com/MonoGame/MonoGame/pull/6905)
 - τ constant. [#6937](https://github.com/MonoGame/MonoGame/pull/6937)
 - [UWP] Overloads for more detailed vibration control. [#6933](https://github.com/MonoGame/MonoGame/pull/6933)
 - [MGCB Editor] Always use Headerbar. [#6938](https://github.com/MonoGame/MonoGame/pull/6938)
 - Joystick display name for the Joystick API. [#7008](https://github.com/MonoGame/MonoGame/pull/7008)
 - [OpenGL] Overload with base index for draw instancing. [#7016](https://github.com/MonoGame/MonoGame/pull/7016)
 - Google Stadia Support. [#7020](https://github.com/MonoGame/MonoGame/pull/7020) [#7049](https://github.com/MonoGame/MonoGame/pull/7049)
 - LastJoystickIndex for the Joystick API. [#7017](https://github.com/MonoGame/MonoGame/pull/7017)
 - MouseState.ToString. [#7091](https://github.com/MonoGame/MonoGame/pull/7091)
 - [MGCB Editor] .NET Core tool. [#7090](https://github.com/MonoGame/MonoGame/pull/7090)
 - [MGFXC] Wine support. [#7098](https://github.com/MonoGame/MonoGame/pull/7098) [#7103](https://github.com/MonoGame/MonoGame/pull/7103) [#7107](https://github.com/MonoGame/MonoGame/pull/7107) [#7188](https://github.com/MonoGame/MonoGame/pull/7188)
 - EffectParameter.SetValue for int array. [#7143](https://github.com/MonoGame/MonoGame/pull/7143)
 - Add Web platform information to the content builder. [#7144](https://github.com/MonoGame/MonoGame/pull/7144)
 - [macOS] MonoGame Content pad for VS for Mac. [#7266](https://github.com/MonoGame/MonoGame/pull/7266)

### Removed
 - Remove obsolete SpriteBatch.Draw overload. [#6818](https://github.com/MonoGame/MonoGame/pull/6818)
 - Disable Piranha portable build. [#6844](https://github.com/MonoGame/MonoGame/pull/6844)
 - Removed MonoGame.Framework.Net. [#6900](https://github.com/MonoGame/MonoGame/pull/6900) [#6775](https://github.com/MonoGame/MonoGame/pull/6775)
 - Remove internal PNG Utility Classes. [#6976](https://github.com/MonoGame/MonoGame/pull/6976)
 - Protobuild removed and replaced with solutions and projects. [#7040](https://github.com/MonoGame/MonoGame/pull/7040) [#7041](https://github.com/MonoGame/MonoGame/pull/7041)

### Changed
 - OpenAssetImporter Improvements and FbxImporter Unit Tests. [#6158](https://github.com/MonoGame/MonoGame/pull/6158)
 - Made sound system optional. [#6629](https://github.com/MonoGame/MonoGame/pull/6629)
 - [UWP] Do not set minimum window size. [#6763](https://github.com/MonoGame/MonoGame/pull/6763)
 - Added Cake build script to replace NANT. [#6776](https://github.com/MonoGame/MonoGame/pull/6776)
 - Marked Color.TransparentBlack obsolete. [#6817](https://github.com/MonoGame/MonoGame/pull/6817)
 - [SDL] Performance and garbage optimizations to Joystick. [#6829](https://github.com/MonoGame/MonoGame/pull/6829) [#6834](https://github.com/MonoGame/MonoGame/pull/6834)
 - Track GamePad packet number and general optimizations. [#6760](https://github.com/MonoGame/MonoGame/pull/6760)
 - [DirectX] Basic performance enhancements to Texture2D. [#6726](https://github.com/MonoGame/MonoGame/pull/6726)
 - Inline Vector2 operator / and Point.ToVector2(). [#6700](https://github.com/MonoGame/MonoGame/pull/6700)
 - Make OggStreamer thread a background thread. [#6848](https://github.com/MonoGame/MonoGame/pull/6848)
 - Made it so Texture2D in any format could be saved as Png/Jpeg when using MonoGame.DesktopGL backend. [#6855](https://github.com/MonoGame/MonoGame/pull/6855)
 - Upgrade SDK projects to .NET 4.5.2. [#6902](https://github.com/MonoGame/MonoGame/pull/6902)  [#6903](https://github.com/MonoGame/MonoGame/pull/6903) [#7050](https://github.com/MonoGame/MonoGame/pull/7050)
 - Reduced garbage in Direct X-specific SetRenderTargets methods. [#7034](https://github.com/MonoGame/MonoGame/pull/7034)
 - MouseState Improvements. [#6973](https://github.com/MonoGame/MonoGame/pull/6973)
 - KeyboardState Improvements. [#6972](https://github.com/MonoGame/MonoGame/pull/6972)
 - [SDL] Updated controller DB. [#7124](https://github.com/MonoGame/MonoGame/pull/7124)
 - We now pool ContentManager scratch buffers. [#5921](https://github.com/MonoGame/MonoGame/pull/5921)
 - [OpenAL] Clean up sound disposal. [#7097](https://github.com/MonoGame/MonoGame/pull/7097)
 - [macOS] Replaced System.Drawing with StbImageSharp to enable tests. [#7071](https://github.com/MonoGame/MonoGame/pull/7071)
 - [Docs] Moved to DocFX v2 for documentation. [#7127](https://github.com/MonoGame/MonoGame/pull/7127)
 - [SDL] Bump dependencies to SDL 2.0.12. [#7133](https://github.com/MonoGame/MonoGame/pull/7133)
 - [Docs] Update for 3.8. [#7117](https://github.com/MonoGame/MonoGame/pull/7117) [#7240](https://github.com/MonoGame/MonoGame/pull/7240)
 - [MGCB Editor] Changed position for context menu item Rebuild. [#7176](https://github.com/MonoGame/MonoGame/pull/7176)
 - [MGCB] Replace symbols for assembly references. [#7272](https://github.com/MonoGame/MonoGame/pull/7272)

### Fixed
 - Support arrays > 255 elements in shaders. [#5995](https://github.com/MonoGame/MonoGame/pull/5995)
 - [DesktopGL] Fix setting mouse cursor from Texture2D. [#6187](https://github.com/MonoGame/MonoGame/pull/6187)
 - Remove trailing NUL chars in registry font name. [#6577](https://github.com/MonoGame/MonoGame/pull/6577)
 - [OpenAL] Fix loading OpenAL-Soft in DesktopGL when # is in path. [#6581](https://github.com/MonoGame/MonoGame/pull/6581)
 - [WindowsDX] Avoid freezing while moving/resizing window. [#6594](https://github.com/MonoGame/MonoGame/pull/6594)
 - [iOS] Fixed background music stopping on game startup. [#6596](https://github.com/MonoGame/MonoGame/pull/6596)
 - Fixed GameUpdateRequiredException namespace. [#6597](https://github.com/MonoGame/MonoGame/pull/6597)
 - [MGCB] Fixed build not cleaning empty projects. [#6615](https://github.com/MonoGame/MonoGame/pull/6615)
 - Reduce garbage from TextInput. [#6664](https://github.com/MonoGame/MonoGame/pull/6664)
 - Fix for loading native libraries on Amazon Fire HD 8. [#6677](https://github.com/MonoGame/MonoGame/pull/6677)
 - Fixes reflection support on SpriteBatch ctor. [#6709](https://github.com/MonoGame/MonoGame/pull/6709)
 - [Android] Fixed crashes on launch and resume. [#6741](https://github.com/MonoGame/MonoGame/pull/6741)
 - [Android] Fixed restoration of an activity after Game.Exit. [#6739](https://github.com/MonoGame/MonoGame/pull/6739)
 - [SDL] Fix rumble infinity threshold. [#6744](https://github.com/MonoGame/MonoGame/pull/6744)
 - [SDL] Fixed OnTextInput. [#6749](https://github.com/MonoGame/MonoGame/pull/6749)
 - Move touches are now aged too. (fixes #6753). [#6756](https://github.com/MonoGame/MonoGame/pull/6756)
 - [Android] Fixed Texture2D.GetData corrupting the Texture2D object. [#6729](https://github.com/MonoGame/MonoGame/pull/6729)
 - [MGCB Editor] Fix assembly load API used for custom user references. [#6770](https://github.com/MonoGame/MonoGame/pull/6770)
 - Fix nuget metadata URL properties. [#6774](https://github.com/MonoGame/MonoGame/pull/6774)
 - Fix IRenderTarget comments. [#6785](https://github.com/MonoGame/MonoGame/pull/6785)
 - Catch ArgumentException when trying to get encoding upon PipelineController.DoBuild. [#6826](https://github.com/MonoGame/MonoGame/pull/6826)
 - [OpenAL] Free source when sound instance is disposed. [#6813](https://github.com/MonoGame/MonoGame/pull/6813)
 - Use Resume when Play is called on Paused sound effect instance. [#6815](https://github.com/MonoGame/MonoGame/pull/6815)
 - Fixed BoundingSphere.Intersects(BoundingBox). [#6666](https://github.com/MonoGame/MonoGame/pull/6666)
 - Override Dispose in DrawableGameComponent to unload content. [#6858](https://github.com/MonoGame/MonoGame/pull/6858)
 - Fixed SwapChain is null in GraphicsDevice.PlatformDispose. [#6869](https://github.com/MonoGame/MonoGame/pull/6869)
 - Swap red and blue when setting mouse cursor texture. [#6859](https://github.com/MonoGame/MonoGame/pull/6859)
 - [DesktopGL] Fix IsActive not getting set to false. [#6895](https://github.com/MonoGame/MonoGame/pull/6895)
 - Fix FileHelpers Uri getting confused by double slashes. [#6924](https://github.com/MonoGame/MonoGame/pull/6924)
 - [UWP] Vibrate controller handles instead of triggers. [#6933](https://github.com/MonoGame/MonoGame/pull/6933)
 - Fix for CoreRT support. [#6948](https://github.com/MonoGame/MonoGame/pull/6948)
 - Fix a crash on macOS and CoreRT related to mishandling unmanaged function pointers. [#6949](https://github.com/MonoGame/MonoGame/pull/6949)
 - [MGCB] Rethrow InvalidContentException for Importers instead of wrapping it in a new PipelineException. [#6954](https://github.com/MonoGame/MonoGame/pull/6954)
 - Throw InvalidContentException with error line number/position in ContentIdentity. [#6953](https://github.com/MonoGame/MonoGame/pull/6953)
 - Fall back to less precise frame pacing on non-Windows platforms. [#6941](https://github.com/MonoGame/MonoGame/pull/6941)
 - [UWP] Fixes content loading to work with packaged files (and not just resources). [#6964](https://github.com/MonoGame/MonoGame/pull/6964)
 - [UWP] Fixes window resizing upon GraphicsDeviceManager.ApplyChanges(). [#6964](https://github.com/MonoGame/MonoGame/pull/6964)
 - [Android] fix GamePad.Back emulation. [#6951](https://github.com/MonoGame/MonoGame/pull/6951)
 - [Android] Fixes android APIs fallback. [#6952](https://github.com/MonoGame/MonoGame/pull/6952)
 - Fixed sample count in SoundEffect. [#6996](https://github.com/MonoGame/MonoGame/pull/6996)
 - [SDL] Fixed memory leak in controller mapping by freeing the returned string. [#7021](https://github.com/MonoGame/MonoGame/pull/7021)
 - [macOS] Look for native libraries in Frameworks folder for .app bundles. [#7022](https://github.com/MonoGame/MonoGame/pull/7022)
 - [OpenGL] Fixed PlatformClear and PlatformPresent to be private in GraphicsDevice. [#7026](https://github.com/MonoGame/MonoGame/pull/7026)
 - [DesktopGL] Fixed GraphicsAdapter.Description to return renderer. [#6959](https://github.com/MonoGame/MonoGame/pull/6959)
 - [OpenGL] Query correct value for the bounds to GL.ActiveTexture. [#6970](https://github.com/MonoGame/MonoGame/pull/6970)
 - Fix Content.mgcb not being visible in VS 2019. [#7064](https://github.com/MonoGame/MonoGame/pull/7064)
 - [iOS] Fix initialization crash on GLES 2.0 devices. [#7047](https://github.com/MonoGame/MonoGame/pull/7047)
 - [iOS] Fix GL error and race condition. [#7073](https://github.com/MonoGame/MonoGame/pull/7073)
 - [MGCB Editor] Mac fixes. [#7077](https://github.com/MonoGame/MonoGame/pull/7077)
 - [OpenAL] Fixing audio duplicating sourceids. [#7095](https://github.com/MonoGame/MonoGame/pull/7095)
 - [MGCB] Fix FindCommand not looking system locations on macOS/Linux. [#7100](https://github.com/MonoGame/MonoGame/pull/7100)
 - Fix FuncLoader not properly checking the current folder (fixes #7101). [#7102](https://github.com/MonoGame/MonoGame/pull/7102)
 - Fix .NET Standard template using wrong nuget. [#7099](https://github.com/MonoGame/MonoGame/pull/7099)
 - [DirectX] Fixed MSAA RenderTarget GetData and SetData. [#5838](https://github.com/MonoGame/MonoGame/pull/5838)
 - Fixed deconstruction of color for bytes and floats. [#7109](https://github.com/MonoGame/MonoGame/pull/7109)
 - [MGCB] Fix failing to find ffmpeg because it tries relative path. [#7138](https://github.com/MonoGame/MonoGame/pull/7138)
 - [MGFXC] Various fixes. [#7148](https://github.com/MonoGame/MonoGame/pull/7148)
 - Fixed a bug where The "New Item" dialogue would have no templates listed. [#7141](https://github.com/MonoGame/MonoGame/pull/7141)
 - [MGCB Editor] Darken text in output. [#7163](https://github.com/MonoGame/MonoGame/pull/7163)
 - [MGCB] Fix trying to chmod root files. [#7183](https://github.com/MonoGame/MonoGame/pull/7183)
 - [MGCB Editor] Fixed to not require root privileges for installing. [#7185](https://github.com/MonoGame/MonoGame/pull/7185)
 - [SDL] Better debugging for macOS. [#7187](https://github.com/MonoGame/MonoGame/pull/7187)
 - Fixed content failing to load on NetCore. [#7199](https://github.com/MonoGame/MonoGame/pull/7199)
 - [MGCB Editor] Fix parsing filename from arguments on macOS. [#7215](https://github.com/MonoGame/MonoGame/pull/7215)
 - [Linux] Flatpak packaging upgrades. [#7224](https://github.com/MonoGame/MonoGame/pull/7224)
 - Fix path in RebuildMGFX.bat. [#7239](https://github.com/MonoGame/MonoGame/pull/7239)
 - [DirectX] Fix issues with RenderTexture2D and SwapChainRenderTarget. [#7225](https://github.com/MonoGame/MonoGame/pull/7225)
 - [macOS] VSMac addin fixes. [#7253](https://github.com/MonoGame/MonoGame/pull/7253)
 - [MGCB Editor] PropertyGrid fixes. [#7256](https://github.com/MonoGame/MonoGame/pull/7256)
 - [MGCB Editor] General fixes. [#7262](https://github.com/MonoGame/MonoGame/pull/7262)
 - Fix a race condition when playing sound in worker threads. [#7219](https://github.com/MonoGame/MonoGame/pull/7219)
 - [Android] Fixed surface recreation when resuming from sleep mode on FireTV. [#7211](https://github.com/MonoGame/MonoGame/pull/7211)
 - Fix loading of content with special characters in a file name. [#7267](https://github.com/MonoGame/MonoGame/pull/7267)


## 3.7.1 Release - December 8, 2018

 - MGCB now generates content building statistics. [#6401](https://github.com/MonoGame/MonoGame/pull/6401)
 - Fixes to dependency loading in Pipeline Tool. [#6450](https://github.com/MonoGame/MonoGame/pull/6450)
 - Fixed crash when canceling choose folder dialog in Pipeline Tool. [#6449](https://github.com/MonoGame/MonoGame/pull/6449)
 - Fix add item dialog jumping around in Pipeline Tool. [#6451](https://github.com/MonoGame/MonoGame/pull/6451)
 - Fix OpenAL library loading on some Android phones. [#6454](https://github.com/MonoGame/MonoGame/pull/6454)
 - Fix Gamepad index tracking under UWP. [#6456](https://github.com/MonoGame/MonoGame/pull/6456)
 - Rename "Copy Asset Path" to "Copy Asset Name" for consistency with XNA in Pipeline Tool. [#6457](https://github.com/MonoGame/MonoGame/pull/6457)
 - Fix TextInput Keys argument for UWP. [#6455](https://github.com/MonoGame/MonoGame/pull/6455)
 - Add new GamePad.GetState() overloads to support different dead zone modes. [#6467](https://github.com/MonoGame/MonoGame/pull/6467)
 - Fixed incorrect offset DynamicSoundEffectInstance.SubmitBuffer under XAudio. [#6523](https://github.com/MonoGame/MonoGame/pull/6523)
 - Improved accuracy of fixed time step. [#6535](https://github.com/MonoGame/MonoGame/pull/6535)
 - Ensure intermediate output path exists before writing stats in Pipeline Tool. [#6503](https://github.com/MonoGame/MonoGame/pull/6503)
 - Fix for special window close case under SDL. [#6489](https://github.com/MonoGame/MonoGame/pull/6489)
 - Marshal microphone identifiers as UTF-8. [#6530](https://github.com/MonoGame/MonoGame/pull/6530)
 - Clear the current selections when excluding items in the Pipeline Tool. [#6549](https://github.com/MonoGame/MonoGame/pull/6549)
 - Enable standard derivatives extension for GLSL shaders. [#6501](https://github.com/MonoGame/MonoGame/pull/6501)
 - Fixed framebuffer object EXT loading under OpenGL. [#6562](https://github.com/MonoGame/MonoGame/pull/6562)
 - Fixed GL.RenderbufferStorage for devices that use the EXT entry points. [#6563](https://github.com/MonoGame/MonoGame/pull/6563)
 - Fix VS template installation when C# folder is missing. [#6544](https://github.com/MonoGame/MonoGame/pull/6544)
 - Fix for SDL loading when a '#' is in the directory path. [#6573](https://github.com/MonoGame/MonoGame/pull/6573)
 - Restored Buttons[] constructor in GamePadState fixing XNA compatibility. [#6572](https://github.com/MonoGame/MonoGame/pull/6572)


## 3.7 Release - September 23, 2018

 - Remove Scale and Rotation properties from Matrix. [#5584](https://github.com/MonoGame/MonoGame/pull/5584)
 - Added Switch as a platform. [#5596](https://github.com/MonoGame/MonoGame/pull/5596)
 - DirectX: Fixed multisample clamping logic. [#5477](https://github.com/MonoGame/MonoGame/pull/5477)
 - SDL Gamepad DB update. [#5605](https://github.com/MonoGame/MonoGame/pull/5605)
 - Add Missing method OpaqueDataDictionary.GetValue. [#5637](https://github.com/MonoGame/MonoGame/pull/5637)
 - Increase code coverage in Model* family. [#5632](https://github.com/MonoGame/MonoGame/pull/5632)
 - Fix scroll wheel events on Windows Universal. [#5631](https://github.com/MonoGame/MonoGame/pull/5631)
 - Implement GetHashCode on Vertex types. [#5654](https://github.com/MonoGame/MonoGame/pull/5654)
 - Implement GetHashCode and ToString methods for Joystick. [#5670](https://github.com/MonoGame/MonoGame/pull/5670)
 - Fixed Gamepad DPad on Android. [#5673](https://github.com/MonoGame/MonoGame/pull/5673)
 - Pipeline process not terminating on exit fix. [#5672](https://github.com/MonoGame/MonoGame/pull/5672)
 - Added Joystick.IsSupported property. [#5678](https://github.com/MonoGame/MonoGame/pull/5678)
 - Use GraphicsCapabilities.MaxTextureAnisotropy on SamplerState. [#5676](https://github.com/MonoGame/MonoGame/pull/5676)
 - Make SpriteBatch.End throw when Begin not called. [#5689](https://github.com/MonoGame/MonoGame/pull/5689)
 - Add Open Output Directory option to Pipeline Tool. [#5690](https://github.com/MonoGame/MonoGame/pull/5690)
 - Rename Exit to Quit on Pipeline Tool Linux Headerbar. [#5687](https://github.com/MonoGame/MonoGame/pull/5687)
 - Added minimum size to the Pipeline Tool window. [#5692](https://github.com/MonoGame/MonoGame/pull/5692)
 - Added Id and DisplayName properties to Gamepad. [#5625](https://github.com/MonoGame/MonoGame/pull/5625)
 - Improved GameController database loading for DesktopGL. [#5606](https://github.com/MonoGame/MonoGame/pull/5606)
 - RPC curves are now updated before Cue is played. [#5709](https://github.com/MonoGame/MonoGame/pull/5709)
 - Fixes to Texture2D.FromStream on Windows DirectX. [#5712](https://github.com/MonoGame/MonoGame/pull/5712)
 - Support DistanceScale and DopplerFactor under OpenAL. [#5718](https://github.com/MonoGame/MonoGame/pull/5718)
 - Implemented Microphone for OpenAL platforms. [#5651](https://github.com/MonoGame/MonoGame/pull/5651)
 - Implemented caching of staging resources used to copy data from a Texture2D under DirectX. [#5704](https://github.com/MonoGame/MonoGame/pull/5704)
 - Reusable function for raising events. [#5713](https://github.com/MonoGame/MonoGame/pull/5713)
 - Remove reference to SharpDX from project templates. [#5611](https://github.com/MonoGame/MonoGame/pull/5611)
 - Improvements to VideoPlayer for Desktop DirectX. [#5737](https://github.com/MonoGame/MonoGame/pull/5737)
 - Use SharpDX NuGet packages from our NuGet packages. [#5748](https://github.com/MonoGame/MonoGame/pull/5748)
 - Fixed leaks that affected shutting down and recreating GraphicsDevice under DirectX. [#5728](https://github.com/MonoGame/MonoGame/pull/5728)
 - Texture2D mipmap generation and population fixes. [#5614](https://github.com/MonoGame/MonoGame/pull/5614)
 - Remove SharpDX.RawInput.dll reference from DirectX graphics backend. [#5723](https://github.com/MonoGame/MonoGame/pull/5723)
 - New fast Texture2D.FromStream implementation for DesktopGL ported from STB. [#5630](https://github.com/MonoGame/MonoGame/pull/5630)
 - Added support DrawInstancedPrimitives on OpenGL platforms. [#4920](https://github.com/MonoGame/MonoGame/pull/4920)
 - Fixed mouse touch event to release when mouse moves outside the client area or we loses focus. [#5641](https://github.com/MonoGame/MonoGame/pull/5641)
 - Added GraphicsAdapter.UseDebugLayers to enable GPU debug features in release builds. [#5791](https://github.com/MonoGame/MonoGame/pull/5791)
 - Fixed DirectX back buffer update when multisampling changes. [#5617](https://github.com/MonoGame/MonoGame/pull/5617)
 - Adds Xbox One S controller support to Linux. [#5797](https://github.com/MonoGame/MonoGame/pull/5797)
 - Do not allow the Pipeline tool to delete files outside the content folder. [#5820](https://github.com/MonoGame/MonoGame/pull/5820)
 - OpenGL Mouse.SetCursor now works with alpha correctly. [#5829](https://github.com/MonoGame/MonoGame/pull/5829)
 - Implement Mouse.SetCursor() for Windows. [#5831](https://github.com/MonoGame/MonoGame/pull/5831)
 - Fix pre-emptive song finish in OggStreamer. [#5821](https://github.com/MonoGame/MonoGame/pull/5821)
 - UWP Templates use target version selected in wizard. [#5819](https://github.com/MonoGame/MonoGame/pull/5819)
 - Implement Mouse.WindowHandle under Windows DirectX. [#5816](https://github.com/MonoGame/MonoGame/pull/5816)
 - Improve shader error/warning parsing in Pipeline Tool. [#5849](https://github.com/MonoGame/MonoGame/pull/5849)
 - Fix crash on multi-editing bool values in Pipeline Tool. [#5859](https://github.com/MonoGame/MonoGame/pull/5859)
 - Fixes to XACT sound effect pooling. [#5832](https://github.com/MonoGame/MonoGame/pull/5832)
 - Improved disposal of OpenGL resources. [#5850](https://github.com/MonoGame/MonoGame/pull/5850)
 - Better support for WAV audio formats in content pipeline and FromStream. [#5750](https://github.com/MonoGame/MonoGame/pull/5750)
 - Fix for build hang with no mgcb file in project. [#5886](https://github.com/MonoGame/MonoGame/pull/5886)
 - Removed deprecated Rider settings from Linux installer. [#5881](https://github.com/MonoGame/MonoGame/pull/5881)
 - Improved performance of SpriteFont.MeasureString() & SpriteBatch.DrawString(). [#5874](https://github.com/MonoGame/MonoGame/pull/5874)
 - Sort content when saving MGCB files. [#5930](https://github.com/MonoGame/MonoGame/pull/5930)
 - Fix a crash when building content in xbuild. [#5897](https://github.com/MonoGame/MonoGame/pull/5897)
 - Fixed back button problems in UWP. [#5810](https://github.com/MonoGame/MonoGame/pull/5810)
 - Removed Windows 8.1 and Windows Phone 8.1 support. [#5809](https://github.com/MonoGame/MonoGame/pull/5809)
 - Upgrade to SharpDX 4.0.1. [#5949](https://github.com/MonoGame/MonoGame/pull/5949)
 - Update the UWP Template to use the Latest SDK. [#5931](https://github.com/MonoGame/MonoGame/pull/5931)
 - Fixed the Scissor rect calculation on DesktopGL and OpenGL platforms. [#5977](https://github.com/MonoGame/MonoGame/pull/5977)
 - Calculate the Client Bounds a bit later. [#5975](https://github.com/MonoGame/MonoGame/pull/5975)
 - Rework Android OpenGL Framebuffer Support. [#5993](https://github.com/MonoGame/MonoGame/pull/5993)
 - Implemented GraphicsDevice.GetBackBufferData. [#5114](https://github.com/MonoGame/MonoGame/pull/5114)
 - Optimizations to Length and Normalize in Vector3 and Vector4. [#6004](https://github.com/MonoGame/MonoGame/pull/6004)
 - Added MGCB man page for Linux. [#5987](https://github.com/MonoGame/MonoGame/pull/5987)
 - Included mgcb autocomplete for bash. [#5985](https://github.com/MonoGame/MonoGame/pull/5985)
 - Fixed GamePad.SetVibration crash. [#5965](https://github.com/MonoGame/MonoGame/pull/5965)
 - Fallback SurfaceFormat for RenderTargets. [#6170](https://github.com/MonoGame/MonoGame/pull/6170)
 - Added O(1) EffectParameter lookups by name. [#6146](https://github.com/MonoGame/MonoGame/pull/6146)
 - Reduce MouseState garbage in Desktop DirectX. [#6168](https://github.com/MonoGame/MonoGame/pull/6168)
 - Made SpriteFont constructor public. [#6126](https://github.com/MonoGame/MonoGame/pull/6126)
 - New Template System using Nuget. [#6135](https://github.com/MonoGame/MonoGame/pull/6135)
 - Use StbSharp for all Texture2D.FromStream. [#6008](https://github.com/MonoGame/MonoGame/pull/6008)
 - Dynamic reference loading in Pipeline Tool. [#6202](https://github.com/MonoGame/MonoGame/pull/6202)
 - Fix Pipeline tool to work regardless of Mono changes. [#6197](https://github.com/MonoGame/MonoGame/pull/6197)
 - Update Template Icons and Fix Mac Info.plist. [#6209](https://github.com/MonoGame/MonoGame/pull/6209)
 - Fix typo in VS2013 Shared Project Template. [#6216](https://github.com/MonoGame/MonoGame/pull/6216)
 - Fill up dotnet template info. [#6226](https://github.com/MonoGame/MonoGame/pull/6226)
 - Support Mac Unit Tests. [#5952](https://github.com/MonoGame/MonoGame/pull/5952)
 - Updated Assimp to latest version. [#6222](https://github.com/MonoGame/MonoGame/pull/6222)
 - Make sure that the window titlebar is within screen bounds on DesktopGL. [#6258](https://github.com/MonoGame/MonoGame/pull/6258)
 - Fixed trigger/dpad button state and reduced garbage in iOS Gamepad. [#6271](https://github.com/MonoGame/MonoGame/pull/6271)
 - Updated Windows Universal Min SDK Versions. [#6257](https://github.com/MonoGame/MonoGame/pull/6257)
 - Fix property content serialization detection when using a property named `Item`. [#5996](https://github.com/MonoGame/MonoGame/pull/5996)
 - Fix launcher default mimetype in Linux installer. [#6275](https://github.com/MonoGame/MonoGame/pull/6275)
 - Restore NVTT. [#6239](https://github.com/MonoGame/MonoGame/pull/6239)
 - Support unicode in window title under DesktopGL. [#6335](https://github.com/MonoGame/MonoGame/pull/6335)
 - Add crash report window to Pipeline Tool. [#6272](https://github.com/MonoGame/MonoGame/pull/6272)
 - Fix linking for copy action in Pipeline Tool. [#6398](https://github.com/MonoGame/MonoGame/pull/6398)
 - Implemented KeyboardInput and MessageBox for Windows DX. [#6410](https://github.com/MonoGame/MonoGame/pull/6410)
 - Fixed audio interruption bug on iOS. [#6433](https://github.com/MonoGame/MonoGame/pull/6433)


## 3.6 Release - February 28, 2017

 - Fixed XML deserialization of Curve type. [#5494](https://github.com/MonoGame/MonoGame/pull/5494)
 - Fix #5498 Pipeline Tool template loading on MacOS. [#5501](https://github.com/MonoGame/MonoGame/pull/5501)
 - Fix typo in the exclude.addins which cause warnings when installing the Addin in XS. [#5500](https://github.com/MonoGame/MonoGame/pull/5500)
 - Added support for arbitrary defines passed to the Effect compiler. [#5496](https://github.com/MonoGame/MonoGame/pull/5496)
 - Fixed GraphicsDevice.Present() to check for current render target. [#5389](https://github.com/MonoGame/MonoGame/pull/5389)
 - Custom texture compression for SpriteFonts. [#5299](https://github.com/MonoGame/MonoGame/pull/5299)
 - Performance improvements to SpriteBatch.DrawString(). [#5226](https://github.com/MonoGame/MonoGame/pull/5226)
 - Removed the OUYA platform [#5194](https://github.com/MonoGame/MonoGame/pull/5194)
 - Dispose of all graphical resources in unit tests. [#5133](https://github.com/MonoGame/MonoGame/pull/5133)
 - Throw NoSuitableGraphicsDeviceException if graphics device creation fails. [#5130](https://github.com/MonoGame/MonoGame/pull/5130)
 - Optimized and added additional constructors to Color. [#5117](https://github.com/MonoGame/MonoGame/pull/5117)
 - Added SamplerState.TextureFilterMode to correctly support comparison filtering. [#5112](https://github.com/MonoGame/MonoGame/pull/5112)
 - Fixed Apply3D() on stereo SoundEffect. [#5099](https://github.com/MonoGame/MonoGame/pull/5099)
 - Fixed Effect.OnApply to return void to match XNA. [#5090](https://github.com/MonoGame/MonoGame/pull/5090)
 - Fix crash when DynamicSoundEffectInstance not disposed. [#5075](https://github.com/MonoGame/MonoGame/pull/5075)
 - Texture2D.FromStream now correctly throws on null arguments. [#5050](https://github.com/MonoGame/MonoGame/pull/5050)
 - Implemented GraphicsAdapter for DirectX platforms. [#5024](https://github.com/MonoGame/MonoGame/pull/5024)
 - Fixed initialization of GameComponent when created within another GameComponent. [#5020](https://github.com/MonoGame/MonoGame/pull/5020)
 - Improved SoundEffect internal platform extendability. [#5006](https://github.com/MonoGame/MonoGame/pull/5006)
 - Refactored audio processing for platform extensibility. [#5001](https://github.com/MonoGame/MonoGame/pull/5001)
 - Refactored texture processing for platform extensibility. [#4996](https://github.com/MonoGame/MonoGame/pull/4996)
 - Refactor ShaderProfile to allow for pipeline extensibility. [#4992](https://github.com/MonoGame/MonoGame/pull/4992)
 - Removed unnessasary dictionary lookup for user index buffers for DirectX platforms. [#4988](https://github.com/MonoGame/MonoGame/pull/4988)
 - New SetRenderTargets() method which allows for variable target count. [#4987](https://github.com/MonoGame/MonoGame/pull/4987)
 - Added support for XACT reverb and filter effects. [#4974](https://github.com/MonoGame/MonoGame/pull/4974)
 - Remove array in GamePadDPad constructor. [#4970](https://github.com/MonoGame/MonoGame/pull/4970)
 - Updated to the latest version of Protobuild. [#4964](https://github.com/MonoGame/MonoGame/pull/4964)
 - Fixed static VBs and IBs on UWP on XB1. [#4955](https://github.com/MonoGame/MonoGame/pull/4955)
 - Updated to the latest version of Protobuild. [#4950](https://github.com/MonoGame/MonoGame/pull/4950)
 - Update Xamarin Studio addin for latest platform changes. [#4926](https://github.com/MonoGame/MonoGame/pull/4926)
 - Replace OpenTK with custom OpenGL bindings [#4874](https://github.com/MonoGame/MonoGame/pull/4874)
 - Fix Mouse updating when moving the Window. [#4924](https://github.com/MonoGame/MonoGame/pull/4924)
 - Fix incorrect use of startIndex in Texture2D.GetData DX. [#4833](https://github.com/MonoGame/MonoGame/pull/4833)
 - Cleanup of AssemblyInfo for framework assembly. [#4810](https://github.com/MonoGame/MonoGame/pull/4810)
 - New SDL2 backend for desktop GL platforms. [#4428](https://github.com/MonoGame/MonoGame/pull/4428)
 - Two MaterialProcessor properties fixed. [#4746](https://github.com/MonoGame/MonoGame/pull/4746)
 - Fixed thumbstick virtual buttons to always use independent axes. [#4742](https://github.com/MonoGame/MonoGame/pull/4742)
 - Fixed back buffer MSAA on DirectX platforms. [#4739](https://github.com/MonoGame/MonoGame/pull/4739)
 - Added new CHANGELOG.md to project. [#4732](https://github.com/MonoGame/MonoGame/pull/4732)
 - Added obsolete attribute and updated documentation. [#4731](https://github.com/MonoGame/MonoGame/pull/4731)
 - Fixed layout of UWP windows in VS template to ignore window chrome. [#4727](https://github.com/MonoGame/MonoGame/pull/4727)
 - Remove support for reading raw assets through ContentManager. [#4726](https://github.com/MonoGame/MonoGame/pull/4726)
 - Implemented DynamicSoundEffectInstance for DirectX and OpenAL platforms. [#4715](https://github.com/MonoGame/MonoGame/pull/4715)
 - Removed unused Yeti Mp3 compressor. [#4713](https://github.com/MonoGame/MonoGame/pull/4713)
 - MonoGame Portable Assemblies. [#4712](https://github.com/MonoGame/MonoGame/pull/4712)
 - Fixed RGBA64 packing and added unit tests. [#4683](https://github.com/MonoGame/MonoGame/pull/4683)
 - Fix Gamepad crash when platform doesn't support the amount. [#4677](https://github.com/MonoGame/MonoGame/pull/4677)
 - Fixed Song stopping before they are finished on Windows. [#4668](https://github.com/MonoGame/MonoGame/pull/4668)
 - Removed the Linux .deb installer. [#4665](https://github.com/MonoGame/MonoGame/pull/4665)
 - OpenAssetImporter is now automatically selected for all the formats it supports. [#4663](https://github.com/MonoGame/MonoGame/pull/4663)
 - Fixed broken unit tests under Linux. [#4614](https://github.com/MonoGame/MonoGame/pull/4614)
 - Split out Title Container into partial classes. [#4590](https://github.com/MonoGame/MonoGame/pull/4590)
 - Added Rider Support to Linux installer. [#4589](https://github.com/MonoGame/MonoGame/pull/4589)
 - Implement vertexStride in VertexBuffer.SetData for OpenGL. [#4568](https://github.com/MonoGame/MonoGame/pull/4568)
 - Performance improvement to SpriteBatch vertex generation. [#4547](https://github.com/MonoGame/MonoGame/pull/4547)
 - Optimization of indices initialization in SpriteBatcher. [#4546](https://github.com/MonoGame/MonoGame/pull/4546)
 - Optimized ContentReader to decode LZ4 compressed streams directly. [#4522](https://github.com/MonoGame/MonoGame/pull/4522)
 - TitleContainer partial class cleanup. [#4520](https://github.com/MonoGame/MonoGame/pull/4520)
 - Remove raw asset support from ContentManager. [#4489](https://github.com/MonoGame/MonoGame/pull/4489)
 - Initial implementation of RenderTargetCube for OpenGL. [#4488](https://github.com/MonoGame/MonoGame/pull/4488)
 - Removed unnecessary platform differences in MGFX. [#4486](https://github.com/MonoGame/MonoGame/pull/4486)
 - SoundEffect fixes and tests. [#4469](https://github.com/MonoGame/MonoGame/pull/4469)
 - Cleanup FX syntax for shader compiler. [#4462](https://github.com/MonoGame/MonoGame/pull/4462)
 - General Improvements to Pipeline Gtk implementation. [#4459](https://github.com/MonoGame/MonoGame/pull/4459)
 - ShaderProfile Refactor. [#4438](https://github.com/MonoGame/MonoGame/pull/4438)
 - GraphicsDeviceManager partial class refactor. [#4425](https://github.com/MonoGame/MonoGame/pull/4425)
 - Remove legacy Storage classes. [#4320](https://github.com/MonoGame/MonoGame/pull/4320)
 - Added mipmap generation for DirectX render targets. [#4189](https://github.com/MonoGame/MonoGame/pull/4189)
 

## 3.5.1 Release - March 30, 2016

 - Fixed negative values when pressing up on left thumbstick on Mac.
 - Removed exception and just return empty state when requesting an invalid GamePad index.
 - Fixed texture processing for 64bpp textures.
 - Fixed Texture2D.SaveAsPng on Mac.


## 3.5 Release - March 17, 2016

 - Content Pipeline Integration for Xamarin Studio and MonoDevleop on Mac and Linux.
 - Automatic inclusion of XNBs into your final project on Mac and Linux.
 - Improved Mac and Linux installers.
 - Assemblies are now installed locally on Mac and Linux just like they are on Windows.
 - New cross-platform "Desktop" project where same binary and content will work on Windows, Linux and Mac desktops.
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


## 3.4 Release - April 29, 2015

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


## 3.3 Release - March 16, 2015

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


## 3.2 Release - April 7, 2014

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


## 3.0.1 Release - March 3, 2013

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


## 3.0 Release - January 1, 2013

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


## 2.5.1 Release - June 18, 2012

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
 

## 2.5 Release - March 29, 2012

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


## 2.1 Release - October 28, 2011

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


## 2.0 Release - October 28, 2011

 - Project renamed MonoGame.
 - Project moved to GitHub.
 - Support for Linux, Mac, Linux, and OpenGL on Windows.


## 0.7 Release - December 2, 2009

 - First stable release.
 - Originally named XnaTouch.
 - iPhone support only.
 - 2D rendering support.
 - Audio support.
 - Networking support.
 - Partial multitouch support.
 - Partial accelerometer support.
