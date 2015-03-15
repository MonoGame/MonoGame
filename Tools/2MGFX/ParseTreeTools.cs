using System;
using System.Globalization;

namespace TwoMGFX
{
	public static class ParseTreeTools
	{
        public static float ParseFloat(string value)
        {
            // Remove all whitespace and trailing F or f.
            value = value.Replace(" ", "");
            value = value.TrimEnd('f', 'F');
            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static int ParseInt(string value)
        {
            // We read it as a float and cast it down to
            // an integer to match Microsoft FX behavior.
            return (int)Math.Floor(ParseFloat(value));
        }
       
		public static bool ParseBool(string value)
		{
		    if (value.ToLowerInvariant() == "true" || value == "1")
				return true;
		    if (value.ToLowerInvariant() == "false" || value == "0")
		        return false;

		    throw new Exception("Invalid boolean value '" + value + "'");
		}
	}
}
