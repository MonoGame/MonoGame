#region File Description
//-----------------------------------------------------------------------------
// TouchCollectionExtensions.cs
//
// MonoGame Foundation Game Platform
// Copyright (C) MonoGame Foundation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace ___SafeGameName___.Core.Inputs;

/// <summary>
/// Provides extension methods for the TouchCollection type.
/// </summary>
public static class TouchCollectionExtensions
{
    /// <summary>
    /// Determines if there are any touches on the screen.
    /// </summary>
    /// <param name="touchState">The current TouchCollection.</param>
    /// <returns>True if there are any touches in the Pressed or Moved state, false otherwise</returns>
    public static bool AnyTouch(this TouchCollection touchState)
    {
        foreach (TouchLocation location in touchState)
        {
            if (location.State == TouchLocationState.Pressed || location.State == TouchLocationState.Moved)
            {
                return true;
            }
        }
        return false;
    }
}
