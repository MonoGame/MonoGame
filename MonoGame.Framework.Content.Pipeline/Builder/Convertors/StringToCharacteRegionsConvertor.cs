// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Builder.Convertors
{
    public class StringToCharacterRegionsConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            var regions = (CharacterRegionsDescription)value;
            var line = "";

            foreach (var reg in regions.Array)
            {
                if (line != "")
                    line += ",";

                line += string.Format("{0}|{1}", (int)reg.Start, (int)reg.End);
            }

            return line;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                try
                {
                    var split = value.ToString().Split(',');
                    var array = new CharacterRegion[split.Length];

                    for (int i = 0; i < split.Length; i++)
                    {
                        var tmpsplit = split[i].Split('|');
                        array[i] = new CharacterRegion((char)int.Parse(tmpsplit[0]), (char)int.Parse(tmpsplit[1]));
                    }

                    return new CharacterRegionsDescription { Array = array };
                }
                catch
                {
                    throw new ArgumentException(string.Format("Could not convert from string({0}) to CharacterRegions.", value));                    
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
