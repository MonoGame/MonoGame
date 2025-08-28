

namespace MonoGame.Framework.Devices.Power
{
    public sealed partial class PowerStatus
    {
        public string BatteryChargeStatus => PlatformBatteryChargeStatus;
        public string PowerLineStatus => PlatformPowerLineStatus;
        public int BatteryLifePercent => PlatformBatteryLifePercent;
    }
}