# Getting Started

This section walks you through the basics of MonoGame and helps you create your first game.

First, select the toolset and operating system you will be working with to create your first MonoGame project and then continue your journey to understand the basic layout of a MonoGame project.

By the end of this tutorial set, you will have a working project to start building from for your target platform and be ready to tackle your next steps.

## Creating a new project

- [Visual Studio](1_creating_a_new_project_vs.md)
- [Visual Studio for Mac](1_creating_a_new_project_vsm.md)
- [DotNet core CLI](1_creating_a_new_project_netcore.md)

## Building your game

- [Understanding the Code](2_understanding_the_code.md)
- [Adding Content](3_adding_content.md)
- [Adding Basic Code](4_adding_basic_code.md)

## Migrating from MonoGame 3.7 to 3.8

If you currently have a MonoGame 3.7 project and wish to upgrade it to MonoGame 3.8, please check our handy [Upgrade Guide](~/articles/migrate38.md).

## Porting from XNA to MonoGame

MonoGame implements the same [API](https://en.wikipedia.org/wiki/Application_programming_interface)
as [XNA 4.0](https://docs.microsoft.com/en-us/previous-versions/windows/xna/bb200104(v=xnagamestudio.41)). That means you usually do not have to change your game code to port from XNA to
MonoGame. There are however some exceptions and some things to keep in mind when porting to MonoGame.

> If your game targets XNA 3.1, you might want to use this archived migration cheatsheet to upgrade
to 4.0:
>
> [http://www.nelxon.com/blog/xna-3-1-to-xna-4-0-cheatsheet/](https://web.archive.org/web/20110217153321/http://www.nelxon.com/blog/xna-3-1-to-xna-4-0-cheatsheet/)

### Missing/removed API

- The Storage namespace was removed due to portability issues (short discussion [here](https://github.com/MonoGame/MonoGame/issues/4311)).
- GamerServices was removed. This part of MonoGame was badly maintained due to low usage and difficulties
in providing the GamerServices API for different platforms.

### Effects

MonoGame does not use the legacy fxc compiler for effects that XNA used. Instead, MonoGame uses the DX11 compiler.
The way MonoGame handles shaders imposes some restrictions and causes some caveats in what is and is not supported.

This is all documented in the [custom effects](~/articles/content/custom_effects.md) documentation page.

### Half pixel offset

XNA uses the DirectX9 graphics API. MonoGame uses the newer DX11 API for DirectX platforms.
DirectX9 interprets UV coordinates differently from other graphics API's. This is typically
referred to as the half-pixel offset.

MonoGame supports replicating XNA behavior (currently only on OpenGL platforms) by setting
the `PreferHalfPixelOffset` flag in [`GraphicsDeviceManager`](xref:Microsoft.Xna.Framework.GraphicsDeviceManager) to `true`. This flag is
set to `false` by default to encourage users to use the modern style of pixel addressing.
DirectX platforms will ignore setting the `PreferHalfPixelOffset` flag and will
always render with a half pixel offset compared to XNA. This is usually not noticeable.

This value is passed to `UseHalfPixelOffset` in [`GraphicsDevice`](xref:Microsoft.Xna.Framework.Graphics.GraphicsDevice). If `UseHalfPixelOffset`
is `true`, you have to add half-pixel offset to a Projection matrix.

[`SpriteBatch`](xref:Microsoft.Xna.Framework.Graphics.SpriteBatch) rendering is not affected by the flag.
Regardless of what value the flag has, `SpriteBatch` will render things exactly the same as in XNA.

If you migrated your game from XNA and some things seem blurred out or very slightly offset,
you may want to try to enable the `PreferHalfPixelOffset` flag.
