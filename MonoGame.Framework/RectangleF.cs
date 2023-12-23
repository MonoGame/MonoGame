using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
///     Describes a 2D-rectangle using floating points.
/// </summary>
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct RectangleF : IEquatable<RectangleF>, IEquatableByRef<RectangleF>
{
    /// <summary>
    ///     The <see cref="RectangleF" /> with <see cref="X" />, <see cref="Y" />, <see cref="Width" /> and
    ///     <see cref="Height" /> all set to <code>0.0f</code>.
    /// </summary>
    public static readonly RectangleF Empty = new();

    /// <summary>
    ///     The x coordinate of the top-left corner of this <see cref="RectangleF" />.
    /// </summary>
    [DataMember] public float X;

    /// <summary>
    ///     The y coordinate of the top-left corner of this <see cref="RectangleF" />.
    /// </summary>
    [DataMember] public float Y;

    /// <summary>
    ///     The width of this <see cref="RectangleF" />.
    /// </summary>
    [DataMember] public float Width;

    /// <summary>
    ///     The height of this <see cref="RectangleF" />.
    /// </summary>
    [DataMember] public float Height;

    /// <summary>
    ///     Returns the x coordinate of the left edge of this <see cref="RectangleF" />.
    /// </summary>
    public float Left => X;

    /// <summary>
    ///     Returns the x coordinate of the right edge of this <see cref="RectangleF" />.
    /// </summary>
    public float Right => X + Width;

    /// <summary>
    ///     Returns the y coordinate of the top edge of this <see cref="RectangleF" />.
    /// </summary>
    public float Top => Y;

    /// <summary>
    ///     Returns the y coordinate of the bottom edge of this <see cref="RectangleF" />.
    /// </summary>
    public float Bottom => Y + Height;

    /// <summary>
    ///     Gets the <see cref="Vector2" /> representing the top-left of this <see cref="RectangleF" />.
    /// </summary>
    public Vector2 TopLeft => new(X, Y);

    /// <summary>
    ///     Gets the <see cref="Vector2" /> representing the top-right of this <see cref="RectangleF" />.
    /// </summary>
    public Vector2 TopRight => new(X + Width, Y);

    /// <summary>
    ///     Gets the <see cref="Vector2" /> representing the bottom-left of this <see cref="RectangleF" />.
    /// </summary>
    public Vector2 BottomLeft => new(X, Y + Height);

    /// <summary>
    ///     Gets the <see cref="Vector2" /> representing the bottom-right of this <see cref="RectangleF" />.
    /// </summary>
    public Vector2 BottomRight => new(X + Width, Y + Height);

    /// <summary>
    ///     Whether this <see cref="RectangleF" /> has a <see cref="Width" /> and
    ///     <see cref="Height" /> of 0, and a <see cref="Location" /> of (0, 0).
    /// </summary>
    public bool IsEmpty => Width == 0 && Height == 0 && X == 0 && Y == 0;

    /// <summary>
    ///     The top-left coordinates of this <see cref="RectangleF" />.
    /// </summary>
    public Vector2 Location
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    /// <summary>
    ///     The width-height coordinates of this <see cref="RectangleF" />.
    /// </summary>
    public Vector2 Size
    {
        get => new(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    /// <summary>
    ///     A <see cref="Vector2" /> located in the center of this <see cref="RectangleF" />.
    /// </summary>
    /// <remarks>
    ///     If <see cref="Width" /> or <see cref="Height" /> is an odd number,
    ///     the center point will be rounded down.
    /// </remarks>
    public Vector2 Center => new(X + Width / 2, Y + Height / 2);

    /// <summary>
    /// Creates a new instance of <see cref="RectangleF"/> struct, with the specified
    /// position, width, and height.
    /// </summary>
    /// <param name="x">The x coordinate of the top-left corner of the created <see cref="RectangleF"/>.</param>
    /// <param name="y">The y coordinate of the top-left corner of the created <see cref="RectangleF"/>.</param>
    /// <param name="width">The width of the created <see cref="RectangleF"/>.</param>
    /// <param name="height">The height of the created <see cref="RectangleF"/>.</param>
    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Creates a new instance of <see cref="RectangleF"/> struct, with the specified
    /// location and size.
    /// </summary>
    /// <param name="location">The x and y coordinates of the top-left corner of the created <see cref="RectangleF"/>.</param>
    /// <param name="size">The width and height of the created <see cref="RectangleF"/>.</param>
    public RectangleF(Vector2 location, Vector2 size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.X;
        Height = size.Y;
    }

    /// <summary>
    ///     Deconstruction method for <see cref="RectangleF"/>.
    /// </summary>
    public void Deconstruct(out float x, out float y, out float width, out float height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }

    /// <summary>
    /// Compares whether two <see cref="RectangleF"/> instances are equal.
    /// </summary>
    /// <param name="a"><see cref="RectangleF"/> instance on the left of the equal sign.</param>
    /// <param name="b"><see cref="RectangleF"/> instance on the right of the equal sign.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public static bool operator ==(RectangleF a, RectangleF b) => a.Equals(b);

    /// <summary>
    /// Compares whether two <see cref="RectangleF"/> instances are not equal.
    /// </summary>
    /// <param name="a"><see cref="RectangleF"/> instance on the left of the not equal sign.</param>
    /// <param name="b"><see cref="RectangleF"/> instance on the right of the not equal sign.</param>
    /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
    public static bool operator !=(RectangleF a, RectangleF b) => !(a == b);

    /// <summary>
    /// Gets whether the provided coordinates lie within the bounds of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="x">The x coordinate of the point to check for containment.</param>
    /// <param name="y">The y coordinate of the point to check for containment.</param>
    /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
    public bool Contains(int x, int y)
    {
        return ((((X <= x) && (x < (X + Width))) && (Y <= y)) && (y < (Y + Height)));
    }

    /// <summary>
    /// Gets whether the provided coordinates lie within the bounds of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="x">The x coordinate of the point to check for containment.</param>
    /// <param name="y">The y coordinate of the point to check for containment.</param>
    /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
    public bool Contains(float x, float y)
    {
        return ((((X <= x) && (x < (X + Width))) && (Y <= y)) && (y < (Y + Height)));
    }

    /// <summary>
    /// Gets whether the provided <see cref="Point"/> lies within the bounds of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
    /// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
    public bool Contains(Point value)
    {
        return ((((X <= value.X) && (value.X < (X + Width))) && (Y <= value.Y)) && (value.Y < (Y + Height)));
    }

    /// <summary>
    /// Gets whether the provided <see cref="Point"/> lies within the bounds of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
    /// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</returns>
    public bool Contains(ref Point value)
    {
        return ((((X <= value.X) && (value.X < (X + Width))) && (Y <= value.Y)) && (value.Y < (Y + Height)));
    }

    /// <summary>
    /// Gets whether the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
    /// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
    public bool Contains(Vector2 value)
    {
        return ((((X <= value.X) && (value.X < (X + Width))) && (Y <= value.Y)) && (value.Y < (Y + Height)));
    }

    /// <summary>
    /// Gets whether the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
    /// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</returns>
    public bool Contains(ref Vector2 value)
    {
        return ((((X <= value.X) && (value.X < (X + Width))) && (Y <= value.Y)) && (value.Y < (Y + Height)));
    }

    /// <summary>
    /// Gets whether the provided <see cref="RectangleF"/> lies within the bounds of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="value">The <see cref="RectangleF"/> to check for inclusion in this <see cref="RectangleF"/>.</param>
    /// <returns><c>true</c> if the provided <see cref="RectangleF"/>'s bounds lie entirely inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
    public bool Contains(RectangleF value)
    {
        return ((((X <= value.X) && ((value.X + value.Width) <= (X + Width))) && (Y <= value.Y)) && ((value.Y + value.Height) <= (Y + Height)));
    }

    /// <summary>
    /// Gets whether the provided <see cref="RectangleF"/> lies within the bounds of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="value">The <see cref="RectangleF"/> to check for inclusion in this <see cref="RectangleF"/>.</param>
    /// <returns><c>true</c> if the provided <see cref="RectangleF"/>'s bounds lie entirely inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</returns>
    public bool Contains(ref RectangleF value)
    {
        return ((((X <= value.X) && ((value.X + value.Width) <= (X + Width))) && (Y <= value.Y)) && ((value.Y + value.Height) <= (Y + Height)));
    }

    /// <inheritdoc />
    public bool Equals(RectangleF other) => Equals(ref other);

    /// <inheritdoc />
    public bool Equals(ref RectangleF other) => X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is RectangleF other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

    /// <summary>
    /// Adjusts the edges of this <see cref="RectangleF"/> by specified horizontal and vertical amounts.
    /// </summary>
    /// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
    /// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
    public void Inflate(int horizontalAmount, int verticalAmount)
    {
        X -= horizontalAmount;
        Y -= verticalAmount;
        Width += horizontalAmount * 2;
        Height += verticalAmount * 2;
    }

    /// <summary>
    /// Adjusts the edges of this <see cref="RectangleF"/> by specified horizontal and vertical amounts.
    /// </summary>
    /// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
    /// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
    public void Inflate(float horizontalAmount, float verticalAmount)
    {
        X -= horizontalAmount;
        Y -= verticalAmount;
        Width += horizontalAmount * 2;
        Height += verticalAmount * 2;
    }

    /// <summary>
    /// Gets whether the other <see cref="RectangleF"/> intersects with this rectangle.
    /// </summary>
    /// <param name="value">The other rectangle for testing.</param>
    /// <returns><c>true</c> if other <see cref="RectangleF"/> intersects with this rectangle; <c>false</c> otherwise.</returns>
    public bool Intersects(RectangleF value)
    {
        return value.Left < Right &&
               Left < value.Right &&
               value.Top < Bottom &&
               Top < value.Bottom;
    }

    /// <summary>
    /// Gets whether the other <see cref="RectangleF"/> intersects with this rectangle.
    /// </summary>
    /// <param name="value">The other rectangle for testing.</param>
    /// <returns><c>true</c> if other <see cref="RectangleF"/> intersects with this rectangle; <c>false</c> otherwise. As an output parameter.</returns>
    public bool Intersects(ref RectangleF value)
    {
        return value.Left < Right &&
               Left < value.Right &&
               value.Top < Bottom &&
               Top < value.Bottom;
    }

    /// <summary>
    ///     Computes the <see cref="RectangleF" /> that is in common between the specified
    ///     <see cref="RectangleF" /> and this <see cref="RectangleF" />.
    /// </summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <returns>
    ///     A <see cref="RectangleF" /> that is in common between both the <paramref name="rectangle" /> and
    ///     this <see cref="RectangleF"/>, if they intersect; otherwise, <see cref="Empty"/>.
    /// </returns>
    public RectangleF Intersect(RectangleF rectangle)
    {
        Intersect(ref this, ref rectangle, out var result);
        return result;
    }

    /// <summary>
    ///     Computes the <see cref="RectangleF" /> that is in common between the specified
    ///     <see cref="RectangleF" /> and this <see cref="RectangleF" />.
    /// </summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <returns>
    ///     A <see cref="RectangleF" /> that is in common between both the <paramref name="rectangle" /> and
    ///     this <see cref="RectangleF"/>, if they intersect; otherwise, <see cref="Empty"/>.
    /// </returns>
    public RectangleF Intersect(ref RectangleF rectangle)
    {
        Intersect(ref this, ref rectangle, out var result);
        return result;
    }

    /// <summary>
    /// Creates a new <see cref="RectangleF"/> that contains overlapping region of two other rectangles.
    /// </summary>
    /// <param name="value1">The first <see cref="RectangleF"/>.</param>
    /// <param name="value2">The second <see cref="RectangleF"/>.</param>
    /// <returns>Overlapping region of the two rectangles.</returns>
    public static RectangleF Intersect(RectangleF value1, RectangleF value2)
    {
        Intersect(ref value1, ref value2, out var rectangle);
        return rectangle;
    }

    /// <summary>
    /// Creates a new <see cref="RectangleF"/> that contains overlapping region of two other rectangles.
    /// </summary>
    /// <param name="value1">The first <see cref="RectangleF"/>.</param>
    /// <param name="value2">The second <see cref="RectangleF"/>.</param>
    /// <param name="result">Overlapping region of the two rectangles as an output parameter.</param>
    public static void Intersect(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
    {
        if (value1.Intersects(value2))
        {
            var right = Math.Min(value1.Right, value2.Right);
            var left = Math.Max(value1.X, value2.X);
            var top = Math.Max(value1.Y, value2.Y);
            var bottom = Math.Min(value1.Bottom, value2.Bottom);
            result = new RectangleF(left, top, right - left, bottom - top);
        }
        else
        {
            result = Empty;
        }
    }

    /// <summary>
    /// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="offsetX">The x coordinate to add to this <see cref="RectangleF"/>.</param>
    /// <param name="offsetY">The y coordinate to add to this <see cref="RectangleF"/>.</param>
    public void Offset(int offsetX, int offsetY)
    {
        X += offsetX;
        Y += offsetY;
    }

    /// <summary>
    /// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="offsetX">The x coordinate to add to this <see cref="RectangleF"/>.</param>
    /// <param name="offsetY">The y coordinate to add to this <see cref="RectangleF"/>.</param>
    public void Offset(float offsetX, float offsetY)
    {
        X += offsetX;
        Y += offsetY;
    }

    /// <summary>
    /// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="amount">The x and y components to add to this <see cref="RectangleF"/>.</param>
    public void Offset(Point amount)
    {
        X += amount.X;
        Y += amount.Y;
    }

    /// <summary>
    /// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="amount">The x and y components to add to this <see cref="RectangleF"/>.</param>
    public void Offset(Vector2 amount)
    {
        X += amount.X;
        Y += amount.Y;
    }

    /// <summary>
    /// Creates a new <see cref="RectangleF"/> that completely contains two other rectangles.
    /// </summary>
    /// <param name="value1">The first <see cref="RectangleF"/>.</param>
    /// <param name="value2">The second <see cref="RectangleF"/>.</param>
    /// <returns>The union of the two rectangles.</returns>
    public static RectangleF Union(RectangleF value1, RectangleF value2)
    {
        var x = Math.Min(value1.X, value2.X);
        var y = Math.Min(value1.Y, value2.Y);
        return new RectangleF(x, y,
            Math.Max(value1.Right, value2.Right) - x,
            Math.Max(value1.Bottom, value2.Bottom) - y);
    }

    /// <summary>
    /// Creates a new <see cref="RectangleF"/> that completely contains two other rectangles.
    /// </summary>
    /// <param name="value1">The first <see cref="RectangleF"/>.</param>
    /// <param name="value2">The second <see cref="RectangleF"/>.</param>
    /// <param name="result">The union of the two rectangles as an output parameter.</param>
    public static void Union(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
    {
        result.X = Math.Min(value1.X, value2.X);
        result.Y = Math.Min(value1.Y, value2.Y);
        result.Width = Math.Max(value1.Right, value2.Right) - result.X;
        result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
    }

    /// <summary>
    ///     Computes the <see cref="RectangleF" /> that contains both the specified <see cref="RectangleF" /> and this <see cref="RectangleF" />.
    /// </summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <returns>
    ///     A <see cref="RectangleF" /> that contains both the <paramref name="rectangle" /> and
    ///     this <see cref="RectangleF" />.
    /// </returns>
    public RectangleF Union(RectangleF rectangle)
    {
        Union(ref this, ref rectangle, out var result);
        return result;
    }

    /// <summary>
    ///     Computes the <see cref="RectangleF" /> that contains both the specified <see cref="RectangleF" /> and this <see cref="RectangleF" />.
    /// </summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <returns>
    ///     A <see cref="RectangleF" /> that contains both the <paramref name="rectangle" /> and
    ///     this <see cref="RectangleF" />.
    /// </returns>
    public RectangleF Union(ref RectangleF rectangle)
    {
        Union(ref this, ref rectangle, out var result);
        return result;
    }

    /// <summary>
    ///     Computes the squared distance from this <see cref="RectangleF"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="vector">The point.</param>
    /// <returns>The squared distance from this <see cref="RectangleF"/> to the <paramref name="vector"/>.</returns>
    public float SquaredDistanceTo(Vector2 vector)
    {
        var squaredDistance = 0.0f;

        // for each axis add up the excess distance outside the box

        // x-axis
        if (vector.X < TopLeft.X)
        {
            var distance = TopLeft.X - vector.X;
            squaredDistance += distance * distance;
        }
        else if (vector.X > BottomRight.X)
        {
            var distance = BottomRight.X - vector.X;
            squaredDistance += distance * distance;
        }

        // y-axis
        if (vector.Y < TopLeft.Y)
        {
            var distance = TopLeft.Y - vector.Y;
            squaredDistance += distance * distance;
        }
        else if (vector.Y > BottomRight.Y)
        {
            var distance = BottomRight.Y - vector.Y;
            squaredDistance += distance * distance;
        }
        return squaredDistance;
    }

    /// <summary>
    ///     Computes the distance from this <see cref="RectangleF"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The distance from this <see cref="RectangleF"/> to the <paramref name="point"/>.</returns>
    public float DistanceTo(Vector2 point)
    {
        return (float)Math.Sqrt(SquaredDistanceTo(point));
    }

    /// <summary>
    ///     Computes the closest <see cref="Vector2" /> on this <see cref="RectangleF" /> to a specified
    ///     <see cref="Vector2" />.
    /// </summary>
    /// <param name="vector">The point.</param>
    /// <returns>The closest <see cref="Vector2" /> on this <see cref="RectangleF" /> to the <paramref name="vector" />.</returns>
    public Vector2 ClosestPointTo(Vector2 vector)
    {
        var result = vector;

        // For each coordinate axis, if the point coordinate value is outside box, clamp it to the box, else keep it as is
        if (result.X < TopLeft.X)
            result.X = TopLeft.X;
        else if (result.X > BottomRight.X)
            result.X = BottomRight.X;

        if (result.Y < TopLeft.Y)
            result.Y = TopLeft.Y;
        else if (result.Y > BottomRight.Y)
            result.Y = BottomRight.Y;

        return result;
    }

    /// <summary>
    ///     Performs an implicit conversion from a <see cref="RectangleF" /> to a <see cref="RectangleF" />.
    /// </summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <returns>
    ///     The resulting <see cref="RectangleF" />.
    /// </returns>
    public static implicit operator RectangleF(Rectangle rectangle)
    {
        return new RectangleF
        {
            X = rectangle.X,
            Y = rectangle.Y,
            Width = rectangle.Width,
            Height = rectangle.Height
        };
    }

    /// <inheritdoc />
    public override string ToString() => "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";

    internal string DebugDisplayString => string.Concat(X, "  ", Y, "  ", Width, "  ", Height);
}
