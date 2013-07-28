using System.Collections;
using JSIL.Meta;
using Microsoft.Xna.Framework;

namespace JSIL {
    [JSExternal]
    class OpenTKService {
        private static readonly OpenTKService _Instance = new OpenTKService();

        public static OpenTKService Instance {
            [JSReplacement("JSIL.Host.getService('opentk')")]
            get {
                return _Instance;
            }
        }

        public void StartRunLoop (GamePlatform platform) {
        }

        public void VertexAttribPointers (System.IntPtr vertexBuffer, int vertexStride, IList elements) {
        }
    }
}