﻿using System;
using System.IO;
using System.Text.Json;

namespace ___SafeGameName___.Core.Settings;

public class DesktopSettingsStorage : BaseSettingsStorage
{
    public DesktopSettingsStorage()
    {
        SpecialFolderPath = Environment.SpecialFolder.ApplicationData;
    }
}
