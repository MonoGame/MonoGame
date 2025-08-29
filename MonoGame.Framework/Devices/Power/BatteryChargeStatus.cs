namespace MonoGame.Framework.Devices.Power
{
    /// <summary>
    /// Represents the current charge status of a device's battery.
    /// </summary>
    /// <remarks>
    /// This enumeration is used to indicate the battery's charging state, such as whether it is charging, full, unplugged, or if no battery is present.
    /// </remarks>
    public enum BatteryChargeStatus
    {
        Unknown,
        Full,
        Charging,
        OnBattery,
        NoBattery
    }
}