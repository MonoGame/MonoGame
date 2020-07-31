// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Interface for drawable entities.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// The draw order of this <see cref="IDrawable"/> relative
        /// to other <see cref="IDrawable"/> instances.
        /// </summary>
        int DrawOrder { get; }

        /// <summary>
        /// Indicates if <see cref="Draw"/> will be called.
        /// </summary>
        bool Visible { get; }
		
        /// <summary>
        /// Raised when <see cref="DrawOrder"/> changed.
        /// </summary>
		event EventHandler<EventArgs> DrawOrderChanged;

        /// <summary>
        /// Raised when <see cref="Visible"/> changed.
        /// </summary>
        event EventHandler<EventArgs> VisibleChanged;

        /// <summary>
        /// Called when this <see cref="IDrawable"/> should draw itself.
        /// </summary>
        /// <param name="gameTime">The elapsed time since the last call to <see cref="Draw"/>.</param>
        void Draw(GameTime gameTime);      
    }
}

