#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License
using System;

namespace Microsoft.Xna.Framework.Input
{
    // Summary:
    //     Enumerates input device buttons.
    [Flags]
    public enum Buttons
    {
        // Summary:
        //     Directional pad down
        DPadUp = 1,
        //
        // Summary:
        //     Directional pad up
        DPadDown = 2,
        //
        // Summary:
        //     Directional pad left
        DPadLeft = 4,
        //
        // Summary:
        //     Directional pad right
        DPadRight = 8,
        //
        // Summary:
        //     START button
        Start = 16,
        //
        // Summary:
        //     BACK button
        Back = 32,
        //
        // Summary:
        //     Left stick button (pressing the left stick)
        LeftStick = 64,
        //
        // Summary:
        //     Right stick button (pressing the right stick)
        RightStick = 128,
        //
        // Summary:
        //     Left bumper (shoulder) button
        LeftShoulder = 256,
        //
        // Summary:
        //     Right bumper (shoulder) button
        RightShoulder = 512,
        //
        // Summary:
        //     Big button
        BigButton = 2048,
        //
        // Summary:
        //     A button
        A = 4096,
        //
        // Summary:
        //     B button
        B = 8192,
        //
        // Summary:
        //     X button
        X = 16384,
        //
        // Summary:
        //     Y button
        Y = 32768,
        //
        // Summary:
        //     Left stick is towards the left
        LeftThumbstickLeft = 2097152,
        //
        // Summary:
        //     Right trigger
        RightTrigger = 4194304,
        //
        // Summary:
        //     Left trigger
        LeftTrigger = 8388608,
        //
        // Summary:
        //     Right stick is towards up
        RightThumbstickUp = 16777216,
        //
        // Summary:
        //     Right stick is towards down
        RightThumbstickDown = 33554432,
        //
        // Summary:
        //     Right stick is towards the right
        RightThumbstickRight = 67108864,
        //
        // Summary:
        //     Right stick is towards the left
        RightThumbstickLeft = 134217728,
        //
        // Summary:
        //     Left stick is towards up
        LeftThumbstickUp = 268435456,
        //
        // Summary:
        //     Left stick is towards down
        LeftThumbstickDown = 536870912,
        //
        // Summary:
        //     Left stick is towards the right
        LeftThumbstickRight = 1073741824,
    }

}
