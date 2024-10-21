namespace ___SafeGameName___.Core.Settings;
internal class SettingsManager<T> where T : new()
{
    private readonly ISettingsStorage storage;
    public ISettingsStorage Storage => storage;

    private T settings;
    public T Settings => settings;

    public SettingsManager(ISettingsStorage storage)
    {
        this.storage = storage;
        Load();
    }

    public void Save()
    {
        storage.SaveSettings<T>(settings);
    }

    public void Load()
    {
        settings = storage.LoadSettings<T>();
    }
}
