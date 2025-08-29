using System.Windows.Forms;
using MonoGame.Framework.Devices.Power;

public partial class PowerStatus
{
    private BatteryChargeStatus PlatformBatteryChargeStatus()
    {
        return SystemInformation.PowerStatus.BatteryChargeStatus switch
        {
            BatteryChargeStatus.Charging => BatteryChargeStatus.Charging,
            BatteryChargeStatus.NoSystemBattery => BatteryChargeStatus.NoBattery,
            BatteryChargeStatus.NotCharging => BatteryChargeStatus.OnBattery,
            BatteryChargeStatus.Unknown => BatteryChargeStatus.Unknown,
            BatteryChargeStatus.High => BatteryChargeStatus.OnBattery,
            BatteryChargeStatus.Low => BatteryChargeStatus.OnBattery,
            BatteryChargeStatus.Critical => BatteryChargeStatus.OnBattery,
            _ => BatteryChargeStatus.Unknown
        };
    }

    private PowerLineStatus PlatformPowerLineStatus()
    {
        return SystemInformation.PowerStatus.PowerLineStatus switch
        {
            PowerLineStatus.Offline => PowerLineStatus.Offline,
            PowerLineStatus.Online => PowerLineStatus.Online,
            _ => PowerLineStatus.Unknown
        };
    }

    private int PlatformBatteryLifePercent()
    {
        return (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
    }
}