namespace Microsoft.Xna.Framework;

/// <summary>
///     Defines a generalized method that a value type or class implements to create a type-specific method for
///     determining equality of instances by reference.
/// </summary>
/// <typeparam name="T">The type of values or objects to compare.</typeparam>
public interface IEquatableByRef<T>
{
    /// <summary>
    ///     Indicates whether the current value or object is equal to another value or object of the same type by
    ///     reference.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the current value or object is equal to the <paramref name="other" /> parameter; otherwise,
    ///     <c>false</c>.
    /// </returns>
    /// <param name="other">A value or object to compare with this value or object.</param>
    bool Equals(ref T other);
}
