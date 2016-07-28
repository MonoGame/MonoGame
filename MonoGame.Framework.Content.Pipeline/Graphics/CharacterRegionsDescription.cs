// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Helper class to hold character regions array so it can be saved and loaded.
    /// </summary>
    public class CharacterRegionsDescription
    {
        public CharacterRegion[] Array { get; set; }

        public CharacterRegionsDescription()
        {
            Array = new [] { new CharacterRegion((char)32, (char)126) };
        }

        public static explicit operator CharacterRegionsDescription (string line)
        {
            var split = line.Split(',');
            var array = new CharacterRegion[split.Length];

            for (int i = 0; i < split.Length; i++)
            {
                var tmpsplit = split[i].Split('|');
                array[i] = new CharacterRegion((char)int.Parse(tmpsplit[0]), (char)int.Parse(tmpsplit[1]));
            }

            return new CharacterRegionsDescription { Array = array };
        }
    }
}

