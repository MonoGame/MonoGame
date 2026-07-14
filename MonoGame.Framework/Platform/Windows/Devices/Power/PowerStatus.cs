using System.Windows.Forms;
using FormsBatteryChargeStatus = System.Windows.Forms.BatteryChargeStatus;
using FormsPowerLineStatus = System.Windows.Forms.PowerLineStatus;

namespace MonoGame.Framework.Devices.Power
{
    public sealed partial class PowerStatus
    {
        private BatteryChargeStatus PlatformBatteryChargeStatus()
        {
            return SystemInformation.PowerStatus.BatteryChargeStatus switch
            {
                FormsBatteryChargeStatus.Charging => BatteryChargeStatus.Charging,
                FormsBatteryChargeStatus.NoSystemBattery => BatteryChargeStatus.NoBattery,
                FormsBatteryChargeStatus.NotCharging => BatteryChargeStatus.OnBattery,
                FormsBatteryChargeStatus.Unknown => BatteryChargeStatus.Unknown,
                FormsBatteryChargeStatus.High => BatteryChargeStatus.OnBattery,
                FormsBatteryChargeStatus.Low => BatteryChargeStatus.OnBattery,
                FormsBatteryChargeStatus.Critical => BatteryChargeStatus.OnBattery,
                _ => BatteryChargeStatus.Unknown
            };
        }

        private PowerLineStatus PlatformPowerLineStatus()
        {
            return SystemInformation.PowerStatus.PowerLineStatus switch
            {
                FormsPowerLineStatus.Offline => PowerLineStatus.Offline,
                FormsPowerLineStatus.Online => PowerLineStatus.Online,
                _ => PowerLineStatus.Unknown
            };
        }

        private int PlatformBatteryLifePercent()
        {
            return (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
        }
    }
}