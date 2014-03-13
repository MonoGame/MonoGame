using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class DepthStencilState
    {
        static readonly Dictionary<CompareFunction, DepthFuncMode> MapDepthCompareFunction = new Dictionary<CompareFunction, DepthFuncMode> {
            { CompareFunction.Always,        DepthFuncMode.Always    },
            { CompareFunction.Equal,         DepthFuncMode.Equal     },
            { CompareFunction.GreaterEqual,  DepthFuncMode.GEqual    },
            { CompareFunction.Greater,       DepthFuncMode.Greater   },
            { CompareFunction.LessEqual,     DepthFuncMode.LEqual    },
            { CompareFunction.Less,          DepthFuncMode.Less      },
            { CompareFunction.NotEqual,      DepthFuncMode.NotEqual },
            { CompareFunction.Never,         DepthFuncMode.Never     },
        };
        
        static readonly Dictionary<CompareFunction, StencilFuncMode> MapStencilCompareFunction = new Dictionary<CompareFunction, StencilFuncMode> {
            { CompareFunction.Always,        StencilFuncMode.Always    },
            { CompareFunction.Equal,         StencilFuncMode.Equal     },
            { CompareFunction.GreaterEqual,  StencilFuncMode.GEqual    },
            { CompareFunction.Greater,       StencilFuncMode.Greater   },
            { CompareFunction.LessEqual,     StencilFuncMode.LEqual    },
            { CompareFunction.Less,          StencilFuncMode.Less      },
            { CompareFunction.NotEqual,      StencilFuncMode.NotEqual },
            { CompareFunction.Never,         StencilFuncMode.Never     },
        };
        
        internal void PlatformApplyState(GraphicsDevice device)
        {
            var g = device.Context;
            
            // FIXME: More advanced stencil attributes
            
            g.SetDepthFunc(
                MapDepthCompareFunction[DepthBufferFunction],
                DepthBufferWriteEnable
            );
            
            g.Enable(EnableMode.DepthTest, DepthBufferEnable);
            
            g.SetStencilFunc(
                MapStencilCompareFunction[StencilFunction],
                ReferenceStencil, StencilMask, StencilWriteMask
            );
            
            g.Enable(EnableMode.StencilTest, StencilEnable);
        }
	}
}

