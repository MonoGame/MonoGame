using System;
using System.IO;
using System.Text.Json;

namespace ___SafeGameName___.Core.Settings;

public abstract class BaseSettingsStorage : ISettingsStorage
{
    protected BaseSettingsStorage()
    {
        SettingsFileName = "settings.json"; // Default settings file name
    }

    protected static Environment.SpecialFolder SpecialFolderPath { get; set; }

    private string settingsFileName;
    public string SettingsFileName
    {
        get => settingsFileName;

        set
        {
            if (settingsFileName != value)
            {
                settingsFileName = value;
            }
        }
    }
    protected string SettingsFilePath => Path.Combine(Environment.GetFolderPath(SpecialFolderPath), "___SafeGameName___", SettingsFileName);

    public virtual void SaveSettings<T>(T settings) where T : new()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(settings, options);

        // Ensure that the directory exists
        string directoryPath = Path.GetDirectoryName(SettingsFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(SettingsFilePath, jsonString);
    }

    public virtual T LoadSettings<T>() where T : new()
    {
        if (!File.Exists(SettingsFilePath))
            return new T();

        string jsonString = File.ReadAllText(SettingsFilePath);
        return JsonSerializer.Deserialize<T>(jsonString) ?? new T();
    }
}