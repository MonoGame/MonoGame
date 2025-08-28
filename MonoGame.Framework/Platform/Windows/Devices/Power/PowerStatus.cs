using System.Windows.Forms;

public partial class PowerStatus
{
    private string PlatformBatteryChargeStatus()
    {
        switch (SystemInformation.PowerStatus.BatteryChargeStatus)
        {
            case BatteryChargeStatus.Charging:
                return "Charging";
            case BatteryChargeStatus.NoSystemBattery:
                return "NoSystemBattery";
            case BatteryChargeStatus.NotCharging:
                return "NotCharging";
            case BatteryChargeStatus.Unknown:
                return "Unknown";
            case BatteryChargeStatus.High:
            case BatteryChargeStatus.Low:
            case BatteryChargeStatus.Critical:
            default:
                return "Discharging";
        }
    }

    private string PlatformPowerLineStatus()
    {
        switch (SystemInformation.PowerStatus.PowerLineStatus)
        {
            case PowerLineStatus.Offline:
                return "Unplugged";
            case PowerLineStatus.Online:
                return "Plugged";
            default:
                return "Unknown";
        }
    }

    private int PlatformBatteryLifePercent()
    {
        return (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
    }
}