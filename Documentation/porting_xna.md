MonoGame implements the same [API](https://en.wikipedia.org/wiki/Application_programming_interface)
as XNA 4.0. That means you usually do not have to change your game code to port from XNA to
MonoGame. There are however some exceptions and some things to keep in mind when porting to MonoGame.

If your game targets XNA 3.1, you might want to use this archived migration cheatsheet to upgrade
to 4.0: [http://www.nelxon.com/blog/xna-3-1-to-xna-4-0-cheatsheet/](https://web.archive.org/web/20110217153321/http://www.nelxon.com/blog/xna-3-1-to-xna-4-0-cheatsheet/).

## Missing/removed API

- The Storage namespace was removed due to portability issues (short discussion [here](https://github.com/MonoGame/MonoGame/issues/4311)).
- GamerServices is not included in the main assembly. This part of MonoGame is not very well maintained due to low usage and difficulties
in providing the GamerServices API for different platforms.

## Effects

MonoGame does not use the legacy fxc compiler for effects that XNA used. Instead MonoGame uses the DX11 compiler.
The way MonoGame handles shaders imposes some restrictions and causes some caveats in what is and is not supported.
This is all documented in the [custom effects](custom_effects.md) documentation page.

## Half pixel offset

XNA uses the DirectX9 graphics API. MonoGame uses the newer DX11 API for DirectX platforms.
DirectX9 interprets UV coordinates differently from other graphics API's. This is typically
referred to as the half-pixel offset. 
MonoGame supports replicating XNA behavior (currently only on OpenGL platforms) by setting
the `UseStandardPixelAddressing` flag in `GraphicsDeviceManager` to `false`. This flag is
set to `true` by default to encourage users to use the modern style of pixel addressing.
DirectX platforms will ignore setting the `UseStandardPixelAddressing` flag and will
always render with a half pixel offset compared to XNA. This is usually not noticeable.

`SpriteBatch` rendering is not affected by the flag. Regardless of what value the flag has,
`SpriteBatch` will render things exactly the same as in XNA.

If you migrated your game from XNA and some things seem blurred out or very slightly offset,
you may want to try to disable the `UseStandardPixelAddressing` flag.

