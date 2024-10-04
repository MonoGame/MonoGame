namespace ___SafeGameName___.Core.Settings;
internal class SettingsManager
{
    private readonly ISettingsStorage storage;
    private ___SafeGameName___Settings settings;

    public SettingsManager(ISettingsStorage storage)
    {
        this.storage = storage;
        settings = this.storage.LoadSettings() ?? new ___SafeGameName___Settings();
    }

    public ___SafeGameName___Settings Settings => settings;

    public void Save()
    {
        storage.SaveSettings(settings);
    }

    public void Load()
    {
        settings = storage.LoadSettings() ?? new ___SafeGameName___Settings();
    }
}
