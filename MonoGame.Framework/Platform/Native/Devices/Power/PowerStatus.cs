using MonoGame.Framework.Devices.Power;

namespace MonoGame.Framework.Devices.Power
{
    public sealed partial class PowerStatus
    {
        private BatteryChargeStatus PlatformBatteryChargeStatus()
        {
            return BatteryChargeStatus.Unknown;
        }

        private PowerLineStatus PlatformPowerLineStatus()
        {
            return PowerLineStatus.Unknown;
        }

        private int PlatformBatteryLifePercent()
        {
            return -1;
        }
    }
}
