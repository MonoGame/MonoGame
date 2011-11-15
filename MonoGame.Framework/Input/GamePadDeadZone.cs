using System;

namespace Microsoft.Xna.Framework.Input
{
    // Summary:
    //     Specifies a type of dead zone processing to apply to Xbox 360 Controller
    //     analog sticks when calling GetState.
    //
    // Parameters:
    //   Circular:
    //     The combined X and Y position of each stick is compared to the dead zone.
    //     This provides better control than IndependentAxes when the stick is used
    //     as a two-dimensional control surface, such as when controlling a character's
    //     view in a first-person game.
    //
    //   IndependentAxes:
    //     The X and Y positions of each stick are compared against the dead zone independently.
    //     This setting is the default when calling GetState.
    //
    //   None:
    //     The values of each stick are not processed and are returned by GetState as
    //     "raw" values. This is best if you intend to implement your own dead zone
    //     processing.
    public enum GamePadDeadZone
    {
        None,
        IndependentAxes,
        Circular
    };
}