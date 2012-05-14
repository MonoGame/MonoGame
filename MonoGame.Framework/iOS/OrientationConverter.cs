#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2011 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion

using System;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework
{
    public static class OrientationConverter
    {
        public static DisplayOrientation UIDeviceOrientationToDisplayOrientation(UIDeviceOrientation orientation)
        {
            switch (orientation)
            {
            case UIDeviceOrientation.FaceDown: return DisplayOrientation.FaceDown;
            case UIDeviceOrientation.FaceUp: return DisplayOrientation.FaceUp;
            default:
			// NOTE: in XNA, Orientation Left is a 90 degree rotation counterclockwise, while on iOS
			// it is a 90 degree rotation CLOCKWISE. They are BACKWARDS! 
            case UIDeviceOrientation.LandscapeLeft: return DisplayOrientation.LandscapeRight;
            case UIDeviceOrientation.LandscapeRight: return DisplayOrientation.LandscapeLeft;
            case UIDeviceOrientation.Portrait: return DisplayOrientation.Portrait;
            case UIDeviceOrientation.PortraitUpsideDown: return DisplayOrientation.PortraitUpsideDown;
            }
        }

        public static DisplayOrientation ToDisplayOrientation(UIInterfaceOrientation orientation)
        {
            switch (orientation)
            {
            default:
			// NOTE: in XNA, Orientation Left is a 90 degree rotation counterclockwise, while on iOS
			// it is a 90 degree rotation CLOCKWISE. They are BACKWARDS! 
            case UIInterfaceOrientation.LandscapeLeft: return DisplayOrientation.LandscapeRight;
            case UIInterfaceOrientation.LandscapeRight: return DisplayOrientation.LandscapeLeft;
            case UIInterfaceOrientation.Portrait: return DisplayOrientation.Portrait;
            case UIInterfaceOrientation.PortraitUpsideDown: return DisplayOrientation.PortraitUpsideDown;
            }
        }

        public static DisplayOrientation Normalize(DisplayOrientation orientation)
        {
            var normalized = orientation;
			
			// Xna's "default" displayorientation is Landscape Left/Right.
            if ((normalized & DisplayOrientation.Default) != 0)
            {
                normalized |= DisplayOrientation.LandscapeLeft;
				normalized |= DisplayOrientation.LandscapeRight;
                normalized &= ~DisplayOrientation.Default;
            }
            return normalized;
        }
    }
}
