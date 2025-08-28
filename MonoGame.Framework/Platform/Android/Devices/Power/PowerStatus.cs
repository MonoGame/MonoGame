using Android.App;
using Android.Content;
using Android.OS;

public partial class PowerStatus
{
    private string PlatformBatteryChargeStatus()
    {
        var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Application.Context.RegisterReceiver(null, filter);
        if (battery != null)
        {
            int status = battery.GetIntExtra(BatteryManager.ExtraStatus, -1);

            switch ((global::Android.OS.BatteryStatus)status)
            {
                case global::Android.OS.BatteryStatus.Charging:
                    return "Charging";
                case global::Android.OS.BatteryStatus.Full:
                    return "Full";
                case global::Android.OS.BatteryStatus.Discharging:
                    return "Discharging";
                case global::Android.OS.BatteryStatus.NotCharging:
                    return "Not Charging";
                case global::Android.OS.BatteryStatus.Unknown:
                default:
                    return "Unknown";
            }
        }

        return "Unknown";
    }

    private string PlatformPowerLineStatus()
    {
        var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Application.Context.RegisterReceiver(null, filter);
        if (battery != null)
        {
            int plugged = battery.GetIntExtra(global::Android.OS.BatteryManager.ExtraPlugged, -1);
            if (plugged == (int)global::Android.OS.BatteryPlugged.Ac)
                return "AC";
            if (plugged == (int)global::Android.OS.BatteryPlugged.Usb)
                return "USB";
            if (plugged == (int)global::Android.OS.BatteryPlugged.Wireless)
                return "Wireless";
        }
        return "Unplugged";
    }

    private int PlatformBatteryLifePercent()
    {
        var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Application.Context.RegisterReceiver(null, filter);
        if (battery != null)
        {
            int level = battery.GetIntExtra(global::Android.OS.BatteryManager.ExtraLevel, -1);
            int scale = battery.GetIntExtra(global::Android.OS.BatteryManager.ExtraScale, -1);
            if (level >= 0 && scale > 0)
                return (int)((level / (float)scale) * 100);
        }
        return -1;
    }
}