// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework;

internal sealed class BrowserHostReadyInfo
{
    public int HostVersion { get; }
    public string ApplicationName { get; }
    public string ContentBaseUri { get; }
    public string CanvasId { get; }
    public string CanvasHandle { get; }
    public string GraphicsApi { get; }
    public string GraphicsContextHandle { get; }
    public int CanvasWidth { get; }
    public int CanvasHeight { get; }
    public int CanvasClientWidth { get; }
    public int CanvasClientHeight { get; }
    public double DevicePixelRatio { get; }
    public bool IsPageVisible { get; }
    public bool HasFocus { get; }
        
    public BrowserHostReadyInfo(
        int hostVersion,
        string applicationName,
        string contentBaseUri,
        string canvasId,
        string canvasHandle,
        string graphicsApi,
        string graphicsContextHandle,
        int canvasWidth,
        int canvasHeight,
        int canvasClientWidth,
        int canvasClientHeight,
        double devicePixelRatio,
        bool isPageVisible,
        bool hasFocus)
    {
        if (hostVersion <= 0)
            throw new ArgumentOutOfRangeException(nameof(hostVersion));

        if (string.IsNullOrWhiteSpace(applicationName))
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(applicationName));

        if (string.IsNullOrWhiteSpace(contentBaseUri))
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(contentBaseUri));

        if (string.IsNullOrWhiteSpace(canvasId))
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(canvasId));

        if (string.IsNullOrWhiteSpace(canvasHandle))
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(canvasHandle));

        if (string.IsNullOrWhiteSpace(graphicsApi))
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(graphicsApi));

        if (string.IsNullOrWhiteSpace(graphicsContextHandle))
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(graphicsContextHandle));

        if (canvasWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(canvasWidth));

        if (canvasHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(canvasHeight));

        if (canvasClientWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(canvasClientWidth));

        if (canvasClientHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(canvasClientHeight));

        if (devicePixelRatio <= 0)
            throw new ArgumentOutOfRangeException(nameof(devicePixelRatio));

        HostVersion = hostVersion;
        ApplicationName = applicationName;
        ContentBaseUri = contentBaseUri;
        CanvasId = canvasId;
        CanvasHandle = canvasHandle;
        GraphicsApi = graphicsApi;
        GraphicsContextHandle = graphicsContextHandle;
        CanvasWidth = canvasWidth;
        CanvasHeight = canvasHeight;
        CanvasClientWidth = canvasClientWidth;
        CanvasClientHeight = canvasClientHeight;
        DevicePixelRatio = devicePixelRatio;
        IsPageVisible = isPageVisible;
        HasFocus = hasFocus;
    }
}
