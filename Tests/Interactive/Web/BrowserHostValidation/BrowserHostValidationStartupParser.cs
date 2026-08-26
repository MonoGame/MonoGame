// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace BrowserHostValidation;

internal static class BrowserHostValidationStartupParser
{
    internal static BrowserHostReadyInfo ParseReadyInfo(string startupInfoJson)
    {
        if (string.IsNullOrWhiteSpace(startupInfoJson))
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(startupInfoJson));

        using JsonDocument document = JsonDocument.Parse(startupInfoJson);
        JsonElement root = document.RootElement;

        return new BrowserHostReadyInfo(
            GetRequiredInt32(root, "hostVersion"),
            GetRequiredString(root, "applicationName"),
            GetRequiredString(root, "contentBaseUri"),
            GetRequiredString(root, "canvasId"),
            GetRequiredString(root, "canvasHandle"),
            GetRequiredString(root, "graphicsApi"),
            GetRequiredString(root, "graphicsContextHandle"),
            GetRequiredInt32(root, "canvasWidth"),
            GetRequiredInt32(root, "canvasHeight"),
            GetRequiredInt32(root, "canvasClientWidth"),
            GetRequiredInt32(root, "canvasClientHeight"),
            GetRequiredDouble(root, "devicePixelRatio"),
            GetRequiredBoolean(root, "isPageVisible"),
            GetRequiredBoolean(root, "hasFocus"));
    }

    private static bool GetRequiredBoolean(JsonElement root, string propertyName)
    {
        if (!TryGetProperty(root, propertyName, out JsonElement property))
            throw new InvalidOperationException("Missing required property '" + propertyName + "'.");

        return property.GetBoolean();
    }

    private static double GetRequiredDouble(JsonElement root, string propertyName)
    {
        if (!TryGetProperty(root, propertyName, out JsonElement property))
            throw new InvalidOperationException("Missing required property '" + propertyName + "'.");

        return property.GetDouble();
    }

    private static int GetRequiredInt32(JsonElement root, string propertyName)
    {
        if (!TryGetProperty(root, propertyName, out JsonElement property))
            throw new InvalidOperationException("Missing required property '" + propertyName + "'.");

        return property.GetInt32();
    }

    private static string GetRequiredString(JsonElement root, string propertyName)
    {
        if (!TryGetProperty(root, propertyName, out JsonElement property))
            throw new InvalidOperationException("Missing required property '" + propertyName + "'.");

        string value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Property '" + propertyName + "' must not be empty.");

        return value;
    }

    private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement property)
    {
        if (root.TryGetProperty(propertyName, out property))
            return true;

        property = default;
        return false;
    }
}
