using System;

namespace ___SafeGameName___.Core.Settings;

public class DesktopSettingsStorage : BaseSettingsStorage
{
    public DesktopSettingsStorage()
    {
        SpecialFolderPath = Environment.SpecialFolder.ApplicationData;
    }
}