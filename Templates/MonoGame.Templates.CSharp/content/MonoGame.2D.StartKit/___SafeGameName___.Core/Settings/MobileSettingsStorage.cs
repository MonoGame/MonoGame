using System;
using System.IO;
using System.Text.Json;

namespace ___SafeGameName___.Core.Settings;

public class MobileSettingsStorage : BaseSettingsStorage
{
    public MobileSettingsStorage()
    {
        SpecialFolderPath = Environment.SpecialFolder.Personal;
    }
}

