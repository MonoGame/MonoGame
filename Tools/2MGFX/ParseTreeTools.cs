using System;

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
       
		public static bool ParseBool(string value)
		{
		    if (value.ToLower() == "true" || value == "1")
				return true;
		    if (value.ToLower() == "false" || value == "0")
		        return false;

		    throw new Exception("Invalid boolean value '" + value + "'");
		}
	}
}
