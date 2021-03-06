# Migrating from XNA

MonoGame is API compatible with [XNA 4.0](https://docs.microsoft.com/en-us/previous-versions/windows/xna/bb200104(v=xnagamestudio.41)) even down to the namespaces.  That means you do not have to change much of your game code to port from XNA to MonoGame. There are however some exceptions and some things to keep in mind.

> If your game targets XNA 3.1, you might want to use this archived migration cheatsheet to upgrade
to 4.0:
>
> [http://www.nelxon.com/blog/xna-3-1-to-xna-4-0-cheatsheet/](https://www.nelsonhurst.com/xna-cheatsheet/)

## Missing/removed API

- The Storage namespace was removed due to portability issues (short discussion [here](https://github.com/MonoGame/MonoGame/issues/4311)).
- GamerServices was removed. This part of MonoGame was badly maintained due to low usage and difficulties
in providing the GamerServices API for different platforms.

## Effects

MonoGame does not use the legacy fxc compiler for effects that XNA used. Instead, MonoGame uses the DX11 compiler.
The way MonoGame handles shaders imposes some restrictions and causes some caveats in what is and is not supported.

This is all documented in the [custom effects](content/custom_effects.md) documentation page.

## Half pixel offset

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
