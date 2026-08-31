// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;

namespace Microsoft.Xna.Framework;

[SupportedOSPlatform("browser")]
internal static class BrowserAssetCache
{
    private static readonly Dictionary<string, byte[]> s_contentByPath = new Dictionary<string, byte[]>(StringComparer.Ordinal);
    private static readonly object s_syncRoot = new object();

    internal static Stream OpenReadStream(string relativePath)
    {
        if (!TryGetContentBytes(relativePath, out byte[] contentBytes))
            return null;

        return new MemoryStream(contentBytes, false);
    }

    private static bool TryGetContentBytes(string relativePath, out byte[] contentBytes)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            contentBytes = null;
            return false;
        }

        string normalizedPath = relativePath.Replace('\\', '/');

        lock (s_syncRoot)
        {
            if (s_contentByPath.TryGetValue(normalizedPath, out contentBytes))
                return true;
        }

        string encodedContent = WebInterop.TryGetContentBase64(normalizedPath);
        if (string.IsNullOrEmpty(encodedContent))
        {
            contentBytes = null;
            return false;
        }

        byte[] decodedContent;
        try
        {
            decodedContent = Convert.FromBase64String(encodedContent);
        }
        catch (FormatException)
        {
            contentBytes = null;
            return false;
        }

        lock (s_syncRoot)
        {
            if (!s_contentByPath.TryGetValue(normalizedPath, out contentBytes))
            {
                s_contentByPath.Add(normalizedPath, decodedContent);
                contentBytes = decodedContent;
            }
        }

        return true;
    }
}
