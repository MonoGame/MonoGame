using SDL2;
using MonoGame.Framework.Devices.Power;

public partial class PowerStatus
{
    private BatteryChargeStatus PlatformBatteryChargeStatus()
    {
        SDL.SDL_PowerState state = SDL.SDL_GetPowerInfo(out _, out _);
        return state switch
        {
            SDL.SDL_PowerState.SDL_POWERSTATE_CHARGED => BatteryChargeStatus.Full,
            SDL.SDL_PowerState.SDL_POWERSTATE_CHARGING => BatteryChargeStatus.Charging,
            SDL.SDL_PowerState.SDL_POWERSTATE_ON_BATTERY => BatteryChargeStatus.OnBattery,
            SDL.SDL_PowerState.SDL_POWERSTATE_NO_BATTERY => BatteryChargeStatus.NoBattery,
            _ => BatteryChargeStatus.Unknown
        };
    }

    private PowerLineStatus PlatformPowerLineStatus()
    {
        SDL.SDL_PowerState state = SDL.SDL_GetPowerInfo(out _, out _);
        return state == SDL.SDL_PowerState.SDL_POWERSTATE_ON_BATTERY ? PowerLineStatus.Offline : PowerLineStatus.Online;
    }

    private int PlatformBatteryLifePercent()
    {
        SDL.SDL_GetPowerInfo(out int percent, out _);
        return percent;
    }
}