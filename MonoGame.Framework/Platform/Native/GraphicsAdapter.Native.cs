// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Xna.Framework.Graphics;

partial class GraphicsAdapter
{
    internal unsafe MGG_GraphicsAdapter* Handle;

    private static unsafe void PlatformInitializeAdapters(out ReadOnlyCollection<GraphicsAdapter> adapters)
    {
        var found = new List<GraphicsAdapter>();
        
        while (true)
        {
            var handle = MGG.GraphicsAdapter_Get(NativeGamePlatform.GraphicsSystem, found.Count);
            if (handle == null)
                break;

            MGG_GraphicsAdaptor_Info info;
            MGG.GraphicsAdapter_GetInfo(handle, out info);

            var adapter = new GraphicsAdapter();
            adapter.Handle = handle;
            adapter.DeviceName = Marshal.PtrToStringUTF8(info.DeviceName);
            adapter.Description = Marshal.PtrToStringUTF8(info.Description);
            adapter.DeviceId = info.DeviceId;
            adapter.Revision = info.Revision;
            adapter.VendorId = info.VendorId;
            adapter.SubSystemId = info.SubSystemId;
            adapter.MonitorHandle = info.MonitorHandle;
            
            // Assume the first adapter is the default for now.
            adapter.IsDefaultAdapter = found.Count == 0;

            adapter._currentDisplayMode = new DisplayMode(
                info.CurrentDisplayMode.width,
                info.CurrentDisplayMode.height,
                info.CurrentDisplayMode.format);

            var modes = new List<DisplayMode>();

            for (int i=0; i < info.DisplayModeCount; i++)
            {
                var mode = new DisplayMode(
                    info.DisplayModes[i].width,
                    info.DisplayModes[i].height,
                    info.DisplayModes[i].format);

                modes.Add(mode);
            }

            adapter._supportedDisplayModes = new DisplayModeCollection(modes);

            found.Add(adapter);
        }

        adapters = new ReadOnlyCollection<GraphicsAdapter>(found);
    }

    private bool PlatformIsProfileSupported(GraphicsProfile graphicsProfile)
    {
        // This isn't needed in 2024!
        return true;
    }
}
