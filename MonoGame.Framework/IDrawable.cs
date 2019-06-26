// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    public interface IDrawable
    {
        int DrawOrder { get; }
        bool Visible { get; }
		
		event EventHandler<EventArgs> DrawOrderChanged;
        event EventHandler<EventArgs> VisibleChanged;

        void Draw(GameTime gameTime);      
    }
}

