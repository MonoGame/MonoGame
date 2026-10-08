

namespace MonoGame.Framework.Devices.Power
{
    public partial class PowerStatus
    {
        private BatteryChargeStatus PlatformBatteryChargeStatus()
        {
            Sdl.PowerState state = Sdl.SDL_GetPowerInfo(out _, out _);
            return state switch
            {
                Sdl.PowerState.Charged => BatteryChargeStatus.Full,
                Sdl.PowerState.Charging => BatteryChargeStatus.Charging,
                Sdl.PowerState.OnBattery => BatteryChargeStatus.OnBattery,
                Sdl.PowerState.NoBattery => BatteryChargeStatus.NoBattery,
                _ => BatteryChargeStatus.Unknown
            };
        }

        private PowerLineStatus PlatformPowerLineStatus()
        {
            Sdl.PowerState state = Sdl.SDL_GetPowerInfo(out _, out _);
            return state == Sdl.PowerState.OnBattery ? PowerLineStatus.Offline : PowerLineStatus.Online;
        }

        private int PlatformBatteryLifePercent()
        {
            Sdl.SDL_GetPowerInfo(out _, out int percent);
            return percent;
        }
    }
}