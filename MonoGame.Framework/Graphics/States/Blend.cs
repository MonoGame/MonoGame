// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
namespace Microsoft.Xna.Framework.Graphics
{
	public enum Blend
	{
		One,				// Each component of the color is multiplied by (1, 1, 1, 1).
		Zero,	 			// Each component of the color is multiplied by (0, 0, 0, 0).
		SourceColor,		// Each component of the color is multiplied by the source color. This can be represented as (Rs, Gs, Bs, As), where R, G, B, and A respectively stand for the red, green, blue, and alpha source values.
		InverseSourceColor,	// Each component of the color is multiplied by the inverse of the source color. This can be represented as (1 − Rs, 1 − Gs, 1 − Bs, 1 − As) where R, G, B, and A respectively stand for the red, green, blue, and alpha destination values.
		SourceAlpha,		// Each component of the color is multiplied by the alpha value of the source. This can be represented as (As, As, As, As), where As is the alpha source value. 
		InverseSourceAlpha,	// Each component of the color is multiplied by the inverse of the alpha value of the source. This can be represented as (1 − As, 1 − As, 1 − As, 1 − As), where As is the alpha destination value.
		DestinationColor,	//Each component color is multiplied by the destination color. This can be represented as (Rd, Gd, Bd, Ad), where R, G, B, and A respectively stand for red, green, blue, and alpha destination values.
		InverseDestinationColor,	// Each component of the color is multiplied by the inverse of the destination color. This can be represented as (1 − Rd, 1 − Gd, 1 − Bd, 1 − Ad), where Rd, Gd, Bd, and Ad respectively stand for the red, green, blue, and alpha destination values.
		DestinationAlpha,	// Each component of the color is multiplied by the alpha value of the destination. This can be represented as (Ad, Ad, Ad, Ad), where Ad is the destination alpha value.
		InverseDestinationAlpha,	// Each component of the color is multiplied by the inverse of the alpha value of the source. This can be represented as (1 − As, 1 − As, 1 − As, 1 − As), where As is the alpha destination value.
		BlendFactor,		// Each component of the color is multiplied by a constant set in BlendFactor.
		InverseBlendFactor,	//Each component of the color is multiplied by the inverse of a constant set in BlendFactor.
		SourceAlphaSaturation,	// Each component of the color is multiplied by either the alpha of the source color, or the inverse of the alpha of the source color, whichever is greater. This can be represented as (f, f, f, 1), where f = min(A, 1 − Ad).
	}
}

