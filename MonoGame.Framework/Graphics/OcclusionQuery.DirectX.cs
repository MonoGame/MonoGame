// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    partial class OcclusionQuery
    {
        private Query _query;

        private void PlatformConstruct()
        {
            //if (graphicsDevice._d3dDevice.FeatureLevel == SharpDX.Direct3D.FeatureLevel.Level_9_1)
            //    throw new NotSupportedException("The Reach profile does not support occlusion queries.");

            var queryDescription = new QueryDescription
            {
                Flags = QueryFlags.None,
                Type = QueryType.Occlusion
            };
            _query = new Query(GraphicsDevice._d3dDevice, queryDescription);
        }
        
        private void PlatformBegin()
        {
            var d3dContext = GraphicsDevice._d3dContext;
            lock(d3dContext)
                d3dContext.Begin(_query);
        }

        private void PlatformEnd()
        {
            var d3dContext = GraphicsDevice._d3dContext;
            lock (d3dContext)
                d3dContext.End(_query);
        }

        private bool PlatformGetResult(out int pixelCount)
        {
            var d3dContext = GraphicsDevice._d3dContext;
            ulong count;
            bool isComplete;

            lock (d3dContext)
                isComplete = d3dContext.GetData(_query, out count);

            pixelCount = (int)count;
            return isComplete;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                    _query.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

