using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsAdapter
    {
        int _displayIndex;

        private static void PlatformInitializeAdapters(out ReadOnlyCollection<GraphicsAdapter> adapters)
        {
            var adapterCount = Sdl.Display.GetNumVideoDisplays();
            var adapterList = new List<GraphicsAdapter>(adapterCount);

            for (var i = 0; i < adapterCount; i++)
            {
                var adapter = CreateAdapter(i);
                adapterList.Add(adapter);
            }
            adapters = new ReadOnlyCollection<GraphicsAdapter>(adapterList);
        }

        private static GraphicsAdapter CreateAdapter(int displayIndex)
        {
            var adapter = new GraphicsAdapter();

            adapter._displayIndex = displayIndex;

            adapter.DeviceName = Sdl.Display.GetDisplayName(displayIndex);
            adapter.Description = Sdl.Display.GetDisplayName(displayIndex);
            Sdl.Display.Mode currentMode;
            Sdl.Display.GetCurrentDisplayMode(displayIndex, out currentMode);
            /*
            adapter.DeviceId = device.Description1.DeviceId;
            adapter.Revision = device.Description1.Revision;
            adapter.VendorId = device.Description1.VendorId;
            adapter.SubSystemId = device.Description1.SubsystemId;
            adapter.MonitorHandle = monitor.Description.MonitorHandle;
            */

            var modes = new List<DisplayMode>();
            var modeCount = Sdl.Display.GetNumDisplayModes(displayIndex);

            for (int i = 0; i < modeCount; i++)
            {
                Sdl.Display.Mode mode;
                Sdl.Display.GetDisplayMode(displayIndex, i, out mode);
                // We are only using one format, Color
                // mode.Format gets the Color format from SDL
                var displayMode = new DisplayMode(mode.Width, mode.Height, SurfaceFormat.Color);
                if (modes.Contains(displayMode))
                    continue;

                modes.Add(displayMode);

                if (adapter._currentDisplayMode == null)
                {
                    if (mode.Width == currentMode.Width && mode.Height == currentMode.Height && displayMode.Format == SurfaceFormat.Color)
                        adapter._currentDisplayMode = displayMode;
                }

            }

            modes.Sort(delegate (DisplayMode a, DisplayMode b)
            {
                if (a == b) return 0;
                if (a.Format <= b.Format && a.Width <= b.Width && a.Height <= b.Height) return -1;
                else return 1;
            });

            adapter._supportedDisplayModes = new DisplayModeCollection(modes);

            if (adapter._currentDisplayMode == null) //(i.e. desktop mode wasn't found in the available modes)
                adapter._currentDisplayMode = new DisplayMode(currentMode.Width, currentMode.Height, SurfaceFormat.Color);

            return adapter;
        }

        private bool PlatformIsProfileSupported(GraphicsProfile graphicsProfile)
        {
            if (UseReferenceDevice)
                return true;

            switch (graphicsProfile)
            {
                case GraphicsProfile.Reach:
                    return true;
                case GraphicsProfile.HiDef:
                    bool result = true;
                    // TODO: check adapter capabilities...
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
