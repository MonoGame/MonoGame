using System;
using Microsoft.Xna.Framework.Graphics;

namespace TwoMGFX
{
	public class ParseTreeTools
	{
		public static TextureAddressMode ParseAddressMode(string value)
		{
			switch (value.ToLower())
			{
				case "clamp":
					return TextureAddressMode.Clamp;
				case "mirror":
					return TextureAddressMode.Mirror;
				case "wrap":
					return TextureAddressMode.Wrap;
				default:
					throw new Exception("Unknown texture address mode '" + value + "'.");
			}
		}

		public static TextureFilterType ParseTextureFilterType(string value)
		{
			switch (value.ToLower())
			{
				case "linear":
					return TextureFilterType.Linear;
				case "point":
					return TextureFilterType.Point;
				case "anisotropic":
					return TextureFilterType.Anisotropic;
				default:
					throw new Exception("Unknown texture filter type '" + value + "'.");
			}
		}

		public static bool ParseBool(string value)
		{
			if (value == "true" || value == "1")
				return true;
			else if (value == "false" || value == "0")
				return false;
			else
				throw new Exception("Invalid boolean value '" + value + "'");
		}

		public static BlendFunction ParseBlendFunction(string value)
		{
			switch (value)
			{
				case "Add":
					return BlendFunction.Add;
				case "Subtract":
					return BlendFunction.Subtract;
				case "RevSubtract":
					return BlendFunction.ReverseSubtract;
				case "Min":
					return BlendFunction.Min;
				case "Max":
					return BlendFunction.Max;
				default:
					throw new Exception("Invalid blend function value '" + value + "'");
			}
		}

		public static Blend ParseBlend(string value)
		{
			switch (value)
			{
				case "Zero":
					return Blend.Zero;
				case "One":
					return Blend.One;
				case "SrcColor":
					return Blend.SourceColor;
				case "InvSrcColor":
					return Blend.InverseSourceColor;
				case "SrcAlpha":
					return Blend.SourceAlpha;
				case "InvSrcAlpha":
					return Blend.InverseSourceAlpha;
				case "DestAlpha":
					return Blend.DestinationAlpha;
				case "InvDestAlpha":
					return Blend.InverseDestinationAlpha;
				case "DestColor":
					return Blend.DestinationColor;
				case "InvDestColor":
					return Blend.InverseDestinationColor;
				case "SrcAlphaSat":
					return Blend.SourceAlphaSaturation;
				case "BlendFactor":
					return Blend.BlendFactor;
				case "InvBlendFactor":
					return Blend.InverseBlendFactor;
				default:
					throw new Exception("Invalid blend value '" + value + "'");
			}
		}
	}
}
