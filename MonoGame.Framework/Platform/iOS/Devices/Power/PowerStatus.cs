using UIKit;

public partial class PowerStatus
{
    public PowerStatus()
    {
        UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
    }

    private string PlatformBatteryChargeStatus()
    {
        switch (UIDevice.CurrentDevice.BatteryState)
        {
            case UIDeviceBatteryState.Charging:
                return "Charging";
            case UIDeviceBatteryState.Full:
                return "Full";
            case UIDeviceBatteryState.Unplugged:
                return "Unplugged";
            case UIDeviceBatteryState.Unknown:
            default:
                return "Unknown";
        }
    }

    private string PlatformPowerLineStatus()
    {
        var state = UIDevice.CurrentDevice.BatteryState;

        if (state == UIDeviceBatteryState.Charging || state == UIDeviceBatteryState.Full)
            return "Plugged";

        if (state == UIDeviceBatteryState.Unplugged)
            return "Unplugged";

        return "Unknown";
    }

    private int PlatformBatteryLifePercent()
    {
        float level = UIDevice.CurrentDevice.BatteryLevel;
        if (level < 0)
            return -1;
        return (int)(level * 100);
    }
}