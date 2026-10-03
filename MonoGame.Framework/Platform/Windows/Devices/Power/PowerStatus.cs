using System.Windows.Forms;

namespace MonoGame.Framework.Devices.Power
{
    public sealed partial class PowerStatus
    {
        private BatteryChargeStatus PlatformBatteryChargeStatus()
        {
            return SystemInformation.PowerStatus.BatteryChargeStatus switch
            {
                System.Windows.Forms.BatteryChargeStatus.Charging => BatteryChargeStatus.Charging,
                System.Windows.Forms.BatteryChargeStatus.NoSystemBattery => BatteryChargeStatus.NoBattery,
                System.Windows.Forms.BatteryChargeStatus.Unknown => BatteryChargeStatus.Unknown,
                System.Windows.Forms.BatteryChargeStatus.High => BatteryChargeStatus.OnBattery,
                System.Windows.Forms.BatteryChargeStatus.Low => BatteryChargeStatus.OnBattery,
                System.Windows.Forms.BatteryChargeStatus.Critical => BatteryChargeStatus.OnBattery,
                _ => BatteryChargeStatus.Unknown
            };
        }

        private PowerLineStatus PlatformPowerLineStatus()
        {
            return SystemInformation.PowerStatus.PowerLineStatus switch
            {
                System.Windows.Forms.PowerLineStatus.Offline => PowerLineStatus.Offline,
                System.Windows.Forms.PowerLineStatus.Online => PowerLineStatus.Online,
                _ => PowerLineStatus.Unknown
            };
        }

        private int PlatformBatteryLifePercent()
        {
            return (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
        }
    }
}