using Android.App;
using Android.Content;
using Android.OS;
using MonoGame.Framework.Devices.Power;

public partial class PowerStatus
{
    private BatteryChargeStatus PlatformBatteryChargeStatus()
    {
        var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Application.Context.RegisterReceiver(null, filter);
        if (battery != null)
        {
            int status = battery.GetIntExtra(BatteryManager.ExtraStatus, -1);

            return status switch
            {
                (int)global::Android.OS.BatteryStatus.Charging => BatteryChargeStatus.Charging,
                (int)global::Android.OS.BatteryStatus.Full => BatteryChargeStatus.Full,
                (int)global::Android.OS.BatteryStatus.Discharging => BatteryChargeStatus.OnBattery,
                (int)global::Android.OS.BatteryStatus.NotCharging => BatteryChargeStatus.OnBattery,
                (int)global::Android.OS.BatteryStatus.Unknown => BatteryChargeStatus.Unknown,
                _ => BatteryChargeStatus.Unknown
            };
        }

        return BatteryChargeStatus.Unknown;
    }

    private PowerLineStatus PlatformPowerLineStatus()
    {
        var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Application.Context.RegisterReceiver(null, filter);
        if (battery != null)
        {
            int plugged = battery.GetIntExtra(global::Android.OS.BatteryManager.ExtraPlugged, -1);
            return plugged switch
            {
                (int)global::Android.OS.BatteryPlugged.Ac => PowerLineStatus.Online,
                (int)global::Android.OS.BatteryPlugged.Usb => PowerLineStatus.Online,
                (int)global::Android.OS.BatteryPlugged.Wireless => PowerLineStatus.Online,
                _ => PowerLineStatus.Offline
            };
        }
        return PowerLineStatus.Unknown;
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