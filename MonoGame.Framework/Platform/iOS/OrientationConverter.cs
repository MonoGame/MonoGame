// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using UIKit;

namespace Microsoft.Xna.Framework
{
    public static class OrientationConverter
    {
        public static DisplayOrientation UIDeviceOrientationToDisplayOrientation(UIDeviceOrientation orientation)
        {
            switch (orientation)
            {
            case UIDeviceOrientation.FaceDown: return DisplayOrientation.Unknown;
            case UIDeviceOrientation.FaceUp: return DisplayOrientation.Unknown;
            default:
			// NOTE: in XNA, Orientation Left is a 90 degree rotation counterclockwise, while on iOS
			// it is a 90 degree rotation CLOCKWISE. They are BACKWARDS! 
            case UIDeviceOrientation.LandscapeLeft: return DisplayOrientation.LandscapeRight;
            case UIDeviceOrientation.LandscapeRight: return DisplayOrientation.LandscapeLeft;
            case UIDeviceOrientation.Portrait: return DisplayOrientation.Portrait;
            case UIDeviceOrientation.PortraitUpsideDown: return DisplayOrientation.PortraitDown;
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
            case UIInterfaceOrientation.PortraitUpsideDown: return DisplayOrientation.PortraitDown;
            }
        }

        public static UIInterfaceOrientationMask ToUIInterfaceOrientationMask (DisplayOrientation orientation)
        {
            switch (Normalize(orientation))
            {
                case((DisplayOrientation)0):
                case((DisplayOrientation)3):
                    return UIInterfaceOrientationMask.Landscape;
                // NOTE: in XNA, Orientation Left is a 90 degree rotation counterclockwise, while on iOS
		// it is a 90 degree rotation CLOCKWISE. They are BACKWARDS! 
                case((DisplayOrientation)2):
                    return UIInterfaceOrientationMask.LandscapeLeft;
                case((DisplayOrientation)1):
                    return UIInterfaceOrientationMask.LandscapeRight;
                case((DisplayOrientation)4):
                    return UIInterfaceOrientationMask.Portrait;
                case((DisplayOrientation)8):
                    return UIInterfaceOrientationMask.PortraitUpsideDown;
                case((DisplayOrientation)7):
                    return UIInterfaceOrientationMask.AllButUpsideDown;
                default:
                    return UIInterfaceOrientationMask.All;
            }
        }

        public static DisplayOrientation Normalize(DisplayOrientation orientation)
        {
            var normalized = orientation;
			
			// Xna's "default" displayorientation is Landscape Left/Right.
            if (normalized == DisplayOrientation.Default)
            {
                normalized |= DisplayOrientation.LandscapeLeft;
				normalized |= DisplayOrientation.LandscapeRight;
                normalized &= ~DisplayOrientation.Default;
            }
            return normalized;
        }
    }
}
