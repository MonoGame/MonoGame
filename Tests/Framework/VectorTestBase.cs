using System;

namespace MonoGame.Tests.Framework;

/// <summary>
/// Contains members needed by multiple vector test classes.
/// </summary>
internal class VectorTestBase
{
    /// <summary>
    /// A static property that gets a generated array of all values in <see cref="MidpointRounding"/>,
    /// for use with <see cref="NUnit.Framework.TestCaseSourceAttribute"/>.
    /// </summary>
    /// <example>
    /// [Test]
    /// [TetsCaseSource(nameof(MidpointRoundingValujes))]
    /// public void TestMethod(MidpointRounding rounding)
    /// {
    ///     // Do some tests with the provided rounding value
    /// }
    /// </example>
    protected static MidpointRounding[] MidpointRoundingValues { get; } = Enum.GetValues<MidpointRounding>();
}
