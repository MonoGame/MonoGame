namespace ___SafeGameName___.Core.Settings;

public interface ISettingsStorage
{
    void SaveSettings(___SafeGameName___Settings settings);
    ___SafeGameName___Settings LoadSettings();
}

