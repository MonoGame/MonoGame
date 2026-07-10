using System.Windows.Forms;
using MonoGame.Framework.Devices.Power;

public partial class PowerStatus
{
    private BatteryChargeStatus PlatformBatteryChargeStatus()
    {
        return SystemInformation.PowerStatus.BatteryChargeStatus switch
        {
            SystemInformation.PowerStatus.BatteryChargeStatus.BatteryChargeStatus.Charging => BatteryChargeStatus.Charging,
            SystemInformation.PowerStatus.BatteryChargeStatus.BatteryChargeStatus.NoSystemBattery => BatteryChargeStatus.NoBattery,
            SystemInformation.PowerStatus.BatteryChargeStatus.BatteryChargeStatus.NotCharging => BatteryChargeStatus.OnBattery,
            SystemInformation.PowerStatus.BatteryChargeStatus.BatteryChargeStatus.Unknown => BatteryChargeStatus.Unknown,
            SystemInformation.PowerStatus.BatteryChargeStatus.BatteryChargeStatus.High => BatteryChargeStatus.OnBattery,
            SystemInformation.PowerStatus.BatteryChargeStatus.BatteryChargeStatus.Low => BatteryChargeStatus.OnBattery,
            SystemInformation.PowerStatus.BatteryChargeStatus.BatteryChargeStatus.Critical => BatteryChargeStatus.OnBattery,
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
