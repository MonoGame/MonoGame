// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    static public class TypeExtensions
    {
        public static Color ToColor(this System.Drawing.Color color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }

        public static Vector3 ToVector3(this System.Drawing.Color color)
        {
            return new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
        }

        public static void AddUnique<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }

        public static void AddRangeUnique<T>(this List<T> dstList, List<T> list)
        {
            foreach (var i in list)
            {
                if (!dstList.Contains(i))
                    dstList.Add(i);
            }
        }
    }
}
