using System.Collections;
using Microsoft.Xna.Framework;

#if JSIL
using JSIL.Meta;
#endif

namespace JSIL {
    [JSExternal]
    class OpenTKService {
        private static readonly OpenTKService _Instance = new OpenTKService();

        public static OpenTKService Instance {
#if JSIL
            [JSReplacement("JSIL.Host.getService('opentk')")]
#endif
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