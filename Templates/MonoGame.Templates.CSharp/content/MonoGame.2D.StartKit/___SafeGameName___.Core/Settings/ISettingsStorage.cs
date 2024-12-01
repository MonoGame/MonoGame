namespace ___SafeGameName___.Core.Settings;

public interface ISettingsStorage
{
    public string SettingsFileName { get; set; }
    void SaveSettings<T>(T settings) where T : new();
    T LoadSettings<T>() where T : new();
}