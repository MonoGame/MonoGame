using System;
using System.IO;

namespace ___SafeGameName___.Core.Settings;

public abstract class BaseSettingsStorage : ISettingsStorage
{
    protected static Environment.SpecialFolder SpecialFolderPath { get; set; }

    protected string SettingsFilePath => Path.Combine(Environment.GetFolderPath(SpecialFolderPath), "___SafeGameName___", "settings.json");
    public abstract void SaveSettings(___SafeGameName___Settings gameSettings);
    public abstract ___SafeGameName___Settings LoadSettings();

}

