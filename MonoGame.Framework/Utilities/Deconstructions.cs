// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;

namespace MonoGame.Framework
{
    public static class Deconstructions
    {
        public static void Deconstruct(this BoundingBox bb, out Vector3 min, out Vector3 max)
        {
            min = bb.Min;
            max = bb.Max;
        }

        public static void Deconstruct(this BoundingSphere bs, out Vector3 center, out float radius)
        {
            center = bs.Center;
            radius = bs.Radius;
        }

        public static void Deconstruct(this Color c, out float r, out float g, out float b)
        {
            r = c.R;
            g = c.G;
            b = c.B;
        }

        public static void Deconstruct(this Color c, out float r, out float g, out float b, out float a)
        {
            r = c.R;
            g = c.G;
            b = c.B;
            a = c.A;
        }

        public static void Deconstruct(this Plane p, out Vector3 normal, out float d)
        {
            normal = p.Normal;
            d = p.D;
        }

        public static void Deconstruct(this Point p, out int x, out int y)
        {
            x = p.X;
            y = p.Y;
        }

        public static void Deconstruct(this Quaternion q, out float x, out float y, out float z, out float w)
        {
            x = q.X;
            y = q.Y;
            z = q.Z;
            w = q.W;
        }

        public static void Deconstruct(this Ray r, out Vector3 direction, out Vector3 position)
        {
            direction = r.Direction;
            position = r.Position;
        }

        public static void Deconstruct(this Rectangle r, out int x, out int y, out int width, out int height)
        {
            x = r.X;
            y = r.Y;
            width = r.Width;
            height = r.Height;
        }

        public static void Deconstruct(this Vector2 v, out float x, out float y)
        {
            x = v.X;
            y = v.Y;
        }

        public static void Deconstruct(this Vector3 v, out float x, out float y, out float z)
        {
            x = v.X;
            y = v.Y;
            z = v.Z;
        }

        public static void Deconstruct(this Vector4 v, out float x, out float y, out float z, out float w)
        {
            x = v.X;
            y = v.Y;
            z = v.Z;
            w = v.W;
        }
    }
}
