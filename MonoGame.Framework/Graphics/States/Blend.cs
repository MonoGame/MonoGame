// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines a blend mode.
    /// </summary>
	public enum Blend
	{
        /// <summary>
        /// Each component of the color is multiplied by {1, 1, 1, 1}.
        /// </summary>
		One,
        /// <summary>
        /// Each component of the color is multiplied by {0, 0, 0, 0}.
        /// </summary>
		Zero,
        /// <summary>
        /// Each component of the color is multiplied by the source color. 
        /// {Rs, Gs, Bs, As}, where Rs, Gs, Bs, As are color source values.
        /// </summary>
		SourceColor,
        /// <summary>
        /// Each component of the color is multiplied by the inverse of the source color.
        ///  {1 − Rs, 1 − Gs, 1 − Bs, 1 − As}, where Rs, Gs, Bs, As are color source values.
        /// </summary>
		InverseSourceColor,
        /// <summary>
        /// Each component of the color is multiplied by the alpha value of the source. 
        /// {As, As, As, As}, where As is the source alpha value.
        /// </summary>
		SourceAlpha,
        /// <summary>
        /// Each component of the color is multiplied by the inverse of the alpha value of the source. 
        /// {1 − As, 1 − As, 1 − As, 1 − As}, where As is the source alpha value.
        /// </summary>
		InverseSourceAlpha,
        /// <summary>
        /// Each component color is multiplied by the destination color. 
        /// {Rd, Gd, Bd, Ad}, where Rd, Gd, Bd, Ad are color destination values.
        /// </summary>
		DestinationColor,	
        /// <summary>
        /// Each component of the color is multiplied by the inversed destination color. 
        /// {1 − Rd, 1 − Gd, 1 − Bd, 1 − Ad}, where Rd, Gd, Bd, Ad are color destination values.
        /// </summary>
		InverseDestinationColor,
        /// <summary>
        /// Each component of the color is multiplied by the alpha value of the destination.
        /// {Ad, Ad, Ad, Ad}, where Ad is the destination alpha value.
        /// </summary>
		DestinationAlpha,	
        /// <summary>
        /// Each component of the color is multiplied by the inversed alpha value of the destination. 
        /// {1 − Ad, 1 − Ad, 1 − Ad, 1 − Ad}, where Ad is the destination alpha value.
        /// </summary>
		InverseDestinationAlpha,
	    /// <summary>
        /// Each component of the color is multiplied by a constant in the <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsDevice.BlendFactor"/>.
	    /// </summary>
		BlendFactor,
        /// <summary>
        /// Each component of the color is multiplied by a inversed constant in the <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsDevice.BlendFactor"/>.
        /// </summary>
		InverseBlendFactor,
        /// <summary>
        /// Each component of the color is multiplied by either the alpha of the source color, or the inverse of the alpha of the source color, whichever is greater. 
        /// {f, f, f, 1}, where f = min(As, 1 − As), where As is the source alpha value.
        /// </summary>
		SourceAlphaSaturation
	}
}