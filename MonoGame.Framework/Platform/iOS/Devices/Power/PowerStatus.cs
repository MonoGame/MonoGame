using UIKit;

namespace MonoGame.Framework.Devices.Power
{
    public sealed partial class PowerStatus
    {
        public PowerStatus()
        {
            UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
        }

        private BatteryChargeStatus PlatformBatteryChargeStatus()
        {
            return UIDevice.CurrentDevice.BatteryState switch
            {
                UIDeviceBatteryState.Charging => BatteryChargeStatus.Charging,
                UIDeviceBatteryState.Full => BatteryChargeStatus.Full,
                UIDeviceBatteryState.Unplugged => BatteryChargeStatus.OnBattery,
                UIDeviceBatteryState.Unknown => BatteryChargeStatus.Unknown,
                _ => BatteryChargeStatus.Unknown
            };
        }

        private PowerLineStatus PlatformPowerLineStatus()
        {
            var state = UIDevice.CurrentDevice.BatteryState;

            return state switch
            {
                UIDeviceBatteryState.Charging => PowerLineStatus.Online,
                UIDeviceBatteryState.Full => PowerLineStatus.Online,
                UIDeviceBatteryState.Unplugged => PowerLineStatus.Offline,
                _ => PowerLineStatus.Unknown
            };
        }

        private int PlatformBatteryLifePercent()
        {
            float level = UIDevice.CurrentDevice.BatteryLevel;
            if (level < 0)
                return -1;
            return (int)(level * 100);
        }
    }
}