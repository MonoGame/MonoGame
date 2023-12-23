using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

/// <summary>
///     The <see cref="IContentManager" /> is the run-time component which loads managed objects from the binary files
///     produced by the design time content pipeline. It also manages the lifespan of the loaded objects, disposing the
///     content manager will also dispose any assets which are themselves <see cref="IDisposable" />.
/// </summary>
public interface IContentManager : IDisposable
{
    /// <summary>
    ///     Gets or sets the root directory associated with this <see cref="IContentManager" />.
    /// </summary>
    string RootDirectory { get; set; }

    /// <summary>
    ///     Loads an asset that has been processed by the Content Pipeline.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of asset to load. <see cref="Model" />, <see cref="Effect" />, <see cref="SpriteFont" />,
    ///     <see cref="Texture" />, <see cref="Texture2D" /> and <see cref="TextureCube" /> are all supported by default by the
    ///     standard Content Pipeline processor, but additional types may be loaded by extending the processor.
    /// </typeparam>
    /// <param name="assetName">Asset name, relative to the loader root directory, and not including the .xnb file extension.</param>
    /// <returns>The loaded asset. Repeated calls to load the same asset will return the same object instance.</returns>
    T Load<T>(string assetName);

    /// <summary>
    ///     Disposes all data that was loaded by this ContentManager.
    /// </summary>
    void Unload();

    /// <summary>
    ///     Unloads the asset with the given name.
    /// </summary>
    /// <param name="assetName">The name of the asset to unload.</param>
    void UnloadAsset(string assetName);

    /// <summary>
    ///     Unloads the given assets.
    /// </summary>
    /// <param name="assetNames">The name of the assets to unload.</param>
    void UnloadAssets(IList<string> assetNames);
}
