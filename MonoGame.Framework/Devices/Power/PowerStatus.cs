namespace MonoGame.Framework.Devices.Power
{
    /// <summary>
    /// Represents the current power status of the device, including battery charge status,
    /// power line status, and battery life percentage.
    /// </summary>
    public sealed partial class PowerStatus
    {
        public BatteryChargeStatus BatteryChargeStatus => PlatformBatteryChargeStatus();
        public PowerLineStatus PowerLineStatus => PlatformPowerLineStatus();
        public int BatteryLifePercent => PlatformBatteryLifePercent();
    }
}