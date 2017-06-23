// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using OpenGL;
using System.Collections.Generic;
using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDebug
    {
        private static Queue<GraphicsDebugMessage> _cachedMessages;

        static GraphicsDebug()
        {
            _cachedMessages = new Queue<GraphicsDebugMessage>();
        }

        private void PlatformConstruct()
        {
        }

        private bool PlatformTryDequeueMessage(out GraphicsDebugMessage message)
        {
            if (_cachedMessages.Count > 0)
            {
                message = _cachedMessages.Dequeue();
                return true;
            }

            // No messages have been retrieved from OpenGL.
            message = null;
            return false;
        }

        private void PlatformDispose()
        {
        }

        internal static void PlatformAppendMessage(int source, int type, uint id, int severity, int length, string errorMessage, IntPtr userParam)
        {
            _cachedMessages.Enqueue(new GraphicsDebugMessage
            {
                Source = Enum.GetName(typeof(OpenGL.DebugSource), source),
                Type = Enum.GetName(typeof(OpenGL.DebugType), type),
                Id = id,
                Severity = Enum.GetName(typeof(OpenGL.DebugSeverity), severity),
                Message = errorMessage,
                UserdataPointer = userParam
            });
        }
    }
}
