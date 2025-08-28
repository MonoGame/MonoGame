using SDL2;

public partial class PowerStatus
{
    private string PlatformBatteryChargeStatus()
    {
        SDL.SDL_PowerState powerInfo = SDL.SDL_GetPowerInfo(out _, out _);
        return powerInfo switch
        {
            SDL.SDL_PowerState.SDL_POWERSTATE_CHARGED => "Charged",
            SDL.SDL_PowerState.SDL_POWERSTATE_CHARGING => "Charging",
            SDL.SDL_PowerState.SDL_POWERSTATE_ON_BATTERY => "OnBattery",
            SDL.SDL_PowerState.SDL_POWERSTATE_NO_BATTERY => "NoBattery",
            _ => "Unknown"
        };
    }

    private string PlatformPowerLineStatus()
    {
        SDL.SDL_PowerState powerInfo = SDL.SDL_GetPowerInfo(out _, out _);
        return powerInfo == SDL.SDL_PowerState.SDL_POWERSTATE_ON_BATTERY ? "Unplugged" : "Plugged";
    }

    private int PlatformBatteryLifePercent()
    {
        SDL.SDL_GetPowerInfo(out int percent, out _);
        return percent;
    }
}