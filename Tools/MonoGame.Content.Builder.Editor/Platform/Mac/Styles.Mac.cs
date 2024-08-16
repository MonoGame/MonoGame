// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto;
using Eto.Mac.Forms;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace MonoGame.Tools.Pipeline
{
    public static class Styles
    {
        public static void Load()
        {
            Style.Add<ApplicationHandler>("PipelineTool", h => h.AppDelegate = new AppDelegate());
        }
    }

    [Register("AppDelegate")]
    public partial class AppDelegate : Eto.Mac.AppDelegate
    {
        public override bool OpenFile(NSApplication sender, string filename)
        {
            PipelineController.Instance.OpenProject(filename);
            return true;
        }
    }
}
