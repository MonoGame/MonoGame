// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// An interface for <see cref="GameComponent"/>.
    /// </summary>
    public interface IGameComponent
    {
        /// <summary>
        /// Initializes the component. Used to load non-graphical resources.
        /// </summary>
        void Initialize();
    }
}

