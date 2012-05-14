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
using System.Runtime.CompilerServices;
using Android.Content;
using Android.OS;

namespace Microsoft.Xna.Framework
{
    public static class PowerStatus
    {
        private static BroadcastReceiver _batteryStatusReceiver;
        private static BatteryStatus _batteryStatus = BatteryStatus.Unknown;
        private static int _batteryLevel;
        private static int _batteryLevelScale = 100;

        static PowerStatus()
        {
            _batteryStatusReceiver = new BatteryStatusBroadCastReceiver();
            Game.Activity.RegisterReceiver(_batteryStatusReceiver, new IntentFilter(Intent.ActionBatteryChanged));
        }

        private class BatteryStatusBroadCastReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                _batteryLevel = intent.GetIntExtra("level", 0);
                _batteryLevelScale = intent.GetIntExtra("scale", 100);
                _batteryStatus = (BatteryStatus)intent.GetIntExtra("status", (int)BatteryStatus.Unknown);
            }
        }



        public static Microsoft.Xna.Framework.BatteryChargeStatus BatteryChargeStatus
        {
            get
            {
                switch (_batteryStatus)
                {
                    case BatteryStatus.Charging:
                        return BatteryChargeStatus.Charging;
                    case BatteryStatus.Full:
                        return BatteryChargeStatus.High;
                    case BatteryStatus.Unknown:
                        return BatteryChargeStatus.Unknown;
                    default:
                        if (BatteryLifePercent >= 60.0f)
                            return BatteryChargeStatus.High;
                        if (BatteryLifePercent >= 20.0f)
                            return BatteryChargeStatus.Low;
                        return BatteryChargeStatus.Critical;
                }
            }
        }

        public static TimeSpan? BatteryFullLifetime
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static float? BatteryLifePercent { get { return _batteryLevel * 100 / _batteryLevelScale; } }

        public static TimeSpan? BatteryLifeRemaining
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static Microsoft.Xna.Framework.PowerLineStatus PowerLineStatus
        {
            get
            {
                return (_batteryStatus == BatteryManager.BatteryStatusCharging || _batteryStatus == BatteryManager.BatteryStatusFull) ? PowerLineStatus.Online : PowerLineStatus.Offline;
            }
        }
    }
}

