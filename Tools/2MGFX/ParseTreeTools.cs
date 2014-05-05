using System;
using Microsoft.Xna.Framework.Graphics;

namespace TwoMGFX
{
	public static class ParseTreeTools
	{
        public static float ParseFloat(string value)
	    {
            // Remove all whitespace and trailing F or f.
	        value = value.Replace("f", "");
            value = value.Replace("F", "");
	        value = value.Replace(" ", "");
            return float.Parse(value);
	    }

        public static int ParseInt(string value)
        {
            return (int)Math.Floor(ParseFloat(value));
        }

	    public static FillMode ParseFillMode(string value)
        {
            switch (value.ToLower())
            {
                case "solid":
                    return FillMode.Solid;
                case "wireframe":
                    return FillMode.WireFrame;
                default:
                    throw new Exception("Unknown fill mode '" + value + "'.");
            }
        }

        public static CullMode ParseCullMode(string value)
        {
            switch (value.ToLower())
            {
                case "none":
                    return CullMode.None;
                case "cw":
                    return CullMode.CullClockwiseFace;
                case "ccw":
                    return CullMode.CullCounterClockwiseFace;
                default:
                    throw new Exception("Unknown cull mode '" + value + "'.");
            }
        }
        
		public static bool ParseBool(string value)
		{
			if (value.ToLower() == "true" || value == "1")
				return true;
            else if (value.ToLower() == "false" || value == "0")
				return false;
			else
				throw new Exception("Invalid boolean value '" + value + "'");
		}

		public static BlendFunction ParseBlendFunction(string value)
		{
			switch (value.ToLower())
			{
				case "add":
					return BlendFunction.Add;
				case "subtract":
					return BlendFunction.Subtract;
				case "revsubtract":
					return BlendFunction.ReverseSubtract;
				case "min":
					return BlendFunction.Min;
				case "max":
					return BlendFunction.Max;
				default:
					throw new Exception("Invalid blend function value '" + value + "'");
			}
		}

		public static Blend ParseBlend(string value)
		{
			switch (value.ToLower())
			{
				case "zero":
					return Blend.Zero;
				case "one":
					return Blend.One;
				case "srccolor":
					return Blend.SourceColor;
				case "invsrccolor":
					return Blend.InverseSourceColor;
				case "srcalpha":
					return Blend.SourceAlpha;
				case "invsrcalpha":
					return Blend.InverseSourceAlpha;
				case "destalpha":
					return Blend.DestinationAlpha;
				case "invdestalpha":
					return Blend.InverseDestinationAlpha;
				case "destcolor":
					return Blend.DestinationColor;
				case "invdestcolor":
					return Blend.InverseDestinationColor;
				case "srcalphasat":
					return Blend.SourceAlphaSaturation;
				case "blendfactor":
					return Blend.BlendFactor;
				case "invblendfactor":
					return Blend.InverseBlendFactor;
				default:
					throw new Exception("Invalid blend value '" + value + "'");
			}
		}
	}
}
