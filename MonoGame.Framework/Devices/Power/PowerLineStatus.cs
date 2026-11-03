namespace MonoGame.Framework.Devices.Power
{
    /// <summary>
    /// Represents the current PowerLine status of a device's battery.
    /// </summary>
    /// <remarks>
    /// This enumeration is used to indicate the power line status, such as whether it is online, offline, or if the status is unknown.
    /// </remarks>
    public enum PowerLineStatus
    {
        Unknown,
        Online,
        Offline
    }
}