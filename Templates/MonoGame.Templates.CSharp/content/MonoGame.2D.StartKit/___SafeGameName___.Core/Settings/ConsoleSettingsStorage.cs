namespace ___SafeGameName___.Core.Settings;

public class ConsoleSettingsStorage : BaseSettingsStorage
{
    // Use console-specific APIs to handle save data
    public override void SaveSettings(___SafeGameName___Settings gameSettings)
    {
        // Console specific save implementation
    }

    public override ___SafeGameName___Settings LoadSettings()
    {
        // Console specific load implementation
        return null; // TODO This is just a placeholder
    }
}

