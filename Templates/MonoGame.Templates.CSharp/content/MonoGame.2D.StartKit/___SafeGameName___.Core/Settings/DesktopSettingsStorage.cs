using System;
using System.IO;
using System.Text.Json;

namespace ___SafeGameName___.Core.Settings;

public class DesktopSettingsStorage : BaseSettingsStorage
{
    public DesktopSettingsStorage()
    {
        SpecialFolderPath = Environment.SpecialFolder.ApplicationData;
    }

    public override void SaveSettings(___SafeGameName___Settings gameSettings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(gameSettings, options);

        // Ensure that the directory exists
        string directoryPath = Path.GetDirectoryName(SettingsFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(SettingsFilePath, jsonString);
    }

    public override ___SafeGameName___Settings LoadSettings()
    {
        if (!File.Exists(SettingsFilePath))
            return null;

        string jsonString = File.ReadAllText(SettingsFilePath);
        return JsonSerializer.Deserialize<___SafeGameName___Settings>(jsonString);
    }
}

