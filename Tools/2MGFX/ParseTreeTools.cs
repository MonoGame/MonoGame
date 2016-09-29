using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;

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

	    public static Color ParseColor(string value)
	    {
	        var hexValue = Convert.ToUInt32(value, 16);

	        byte r, g, b, a;
	        if (value.Length == 8)
	        {
	            r = (byte) ((hexValue >> 16) & 0xFF);
                g = (byte) ((hexValue >> 8) & 0xFF);
                b = (byte) ((hexValue >> 0) & 0xFF);
	            a = 255;
	        }
	        else if (value.Length == 10)
	        {
                r = (byte) ((hexValue >> 24) & 0xFF);
                g = (byte) ((hexValue >> 16) & 0xFF);
                b = (byte) ((hexValue >> 8) & 0xFF);
                a = (byte) ((hexValue >> 0) & 0xFF);
	        }
	        else
	        {
	            throw new NotSupportedException();
	        }

            return new Color(r, g, b, a);
	    }

        public static void WhitespaceNodes(TokenType type, List<ParseNode> nodes, ref string sourceFile)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.Token.Type != type)
                {
                    WhitespaceNodes(type, n.Nodes, ref sourceFile);
                    continue;
                }

                // Get the full content of this node.
                var start = n.Token.StartPos;
                var end = n.Token.EndPos;
                var length = end - n.Token.StartPos;
                var content = sourceFile.Substring(start, length);

                // Replace the content of this node with whitespace.
                for (var c = 0; c < length; c++)
                {
                    if (!char.IsWhiteSpace(content[c]))
                        content = content.Replace(content[c], ' ');
                }

                // Add the whitespace back to the source file.
                var newfile = sourceFile.Substring(0, start);
                newfile += content;
                newfile += sourceFile.Substring(end);
                sourceFile = newfile;
            }
        }
    }
}
