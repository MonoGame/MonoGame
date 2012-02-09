using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DXEffectObject
	{

		public enum D3DRENDERSTATETYPE {
		    ZENABLE                   =   7,
		    FILLMODE                  =   8,
		    SHADEMODE                 =   9,
		    ZWRITEENABLE              =  14,
		    ALPHATESTENABLE           =  15,
		    LASTPIXEL                 =  16,
		    SRCBLEND                  =  19,
		    DESTBLEND                 =  20,
		    CULLMODE                  =  22,
		    ZFUNC                     =  23,
		    ALPHAREF                  =  24,
		    ALPHAFUNC                 =  25,
		    DITHERENABLE              =  26,
		    ALPHABLENDENABLE          =  27,
		    FOGENABLE                 =  28,
		    SPECULARENABLE            =  29,
		    FOGCOLOR                  =  34,
		    FOGTABLEMODE              =  35,
		    FOGSTART                  =  36,
		    FOGEND                    =  37,
		    FOGDENSITY                =  38,
		    RANGEFOGENABLE            =  48,
		    STENCILENABLE             =  52,
		    STENCILFAIL               =  53,
		    STENCILZFAIL              =  54,
		    STENCILPASS               =  55,
		    STENCILFUNC               =  56,
		    STENCILREF                =  57,
		    STENCILMASK               =  58,
		    STENCILWRITEMASK          =  59,
		    TEXTUREFACTOR             =  60,
		    WRAP0                     = 128,
		    WRAP1                     = 129,
		    WRAP2                     = 130,
		    WRAP3                     = 131,
		    WRAP4                     = 132,
		    WRAP5                     = 133,
		    WRAP6                     = 134,
		    WRAP7                     = 135,
		    CLIPPING                  = 136,
		    LIGHTING                  = 137,
		    AMBIENT                   = 139,
		    FOGVERTEXMODE             = 140,
		    COLORVERTEX               = 141,
		    LOCALVIEWER               = 142,
		    NORMALIZENORMALS          = 143,
		    DIFFUSEMATERIALSOURCE     = 145,
		    SPECULARMATERIALSOURCE    = 146,
		    AMBIENTMATERIALSOURCE     = 147,
		    EMISSIVEMATERIALSOURCE    = 148,
		    VERTEXBLEND               = 151,
		    CLIPPLANEENABLE           = 152,
		    POINTSIZE                 = 154,
		    POINTSIZE_MIN             = 155,
		    POINTSPRITEENABLE         = 156,
		    POINTSCALEENABLE          = 157,
		    POINTSCALE_A              = 158,
		    POINTSCALE_B              = 159,
		    POINTSCALE_C              = 160,
		    MULTISAMPLEANTIALIAS      = 161,
		    MULTISAMPLEMASK           = 162,
		    PATCHEDGESTYLE            = 163,
		    DEBUGMONITORTOKEN         = 165,
		    POINTSIZE_MAX             = 166,
		    INDEXEDVERTEXBLENDENABLE  = 167,
		    COLORWRITEENABLE          = 168,
		    TWEENFACTOR               = 170,
		    BLENDOP                   = 171,
		    POSITIONDEGREE            = 172,
		    NORMALDEGREE              = 173,
		    SCISSORTESTENABLE         = 174,
		    SLOPESCALEDEPTHBIAS       = 175,
		    ANTIALIASEDLINEENABLE     = 176,
		    MINTESSELLATIONLEVEL      = 178,
		    MAXTESSELLATIONLEVEL      = 179,
		    ADAPTIVETESS_X            = 180,
		    ADAPTIVETESS_Y            = 181,
		    ADAPTIVETESS_Z            = 182,
		    ADAPTIVETESS_W            = 183,
		    ENABLEADAPTIVETESSELLATION= 184,
		    TWOSIDEDSTENCILMODE       = 185,
		    CCW_STENCILFAIL           = 186,
		    CCW_STENCILZFAIL          = 187,
		    CCW_STENCILPASS           = 188,
		    CCW_STENCILFUNC           = 189,
		    COLORWRITEENABLE1         = 190,
		    COLORWRITEENABLE2         = 191,
		    COLORWRITEENABLE3         = 192,
		    BLENDFACTOR               = 193,
		    SRGBWRITEENABLE           = 194,
		    DEPTHBIAS                 = 195,
		    WRAP8                     = 198,
		    WRAP9                     = 199,
		    WRAP10                    = 200,
		    WRAP11                    = 201,
		    WRAP12                    = 202,
		    WRAP13                    = 203,
		    WRAP14                    = 204,
		    WRAP15                    = 205,
		    SEPARATEALPHABLENDENABLE  = 206,
		    SRCBLENDALPHA             = 207,
		    DESTBLENDALPHA            = 208,
		    BLENDOPALPHA              = 209,
		
		    FORCE_DWORD               = 0x7fffffff
		}
		
		public enum D3DTEXTURESTAGESTATETYPE {
		    COLOROP               =  1,
		    COLORARG1             =  2,
		    COLORARG2             =  3,
		    ALPHAOP               =  4,
		    ALPHAARG1             =  5,
		    ALPHAARG2             =  6,
		    BUMPENVMAT00          =  7,
		    BUMPENVMAT01          =  8,
		    BUMPENVMAT10          =  9,
		    BUMPENVMAT11          = 10,
		    TEXCOORDINDEX         = 11,
		    BUMPENVLSCALE         = 22,
		    BUMPENVLOFFSET        = 23,
		    TEXTURETRANSFORMFLAGS = 24,
		    COLORARG0             = 26,
		    ALPHAARG0             = 27,
		    RESULTARG             = 28,
		    CONSTANT              = 32,
		
		    FORCE_DWORD           = 0x7fffffff
		}
		
		public enum D3DTRANSFORMSTATETYPE {
		    VIEW            =  2,
		    PROJECTION      =  3,
		    TEXTURE0        = 16,
		    TEXTURE1        = 17,
		    TEXTURE2        = 18,
		    TEXTURE3        = 19,
		    TEXTURE4        = 20,
		    TEXTURE5        = 21,
		    TEXTURE6        = 22,
		    TEXTURE7        = 23,
			WORLD           = 256,
		    FORCE_DWORD     = 0x7fffffff
		}
		
		public const int D3DX_PARAMETER_SHARED = 1;
		public const int D3DX_PARAMETER_LITERAL = 2;
		public const int D3DX_PARAMETER_ANNOTATION = 4;
		
		public enum D3DXPARAMETER_CLASS
		{
			SCALAR,
			VECTOR,
			MATRIX_ROWS,
			MATRIX_COLUMNS,
			OBJECT,
			STRUCT,
			FORCE_DWORD = 0x7fffffff,
		}
		
		public enum D3DXPARAMETER_TYPE
		{
			VOID,
			BOOL,
			INT,
			FLOAT,
			STRING,
			TEXTURE,
			TEXTURE1D,
			TEXTURE2D,
			TEXTURE3D,
			TEXTURECUBE,
			SAMPLER,
			SAMPLER1D,
			SAMPLER2D,
			SAMPLER3D,
			SAMPLERCUBE,
			PIXELSHADER,
			VERTEXSHADER,
			PIXELFRAGMENT,
			VERTEXFRAGMENT,
			UNSUPPORTED,
			FORCE_DWORD = 0x7fffffff,
		}
		
		enum D3DSAMPLERSTATETYPE {
		    ADDRESSU       = 1,
		    ADDRESSV       = 2,
		    ADDRESSW       = 3,
		    BORDERCOLOR    = 4,
		    MAGFILTER      = 5,
		    MINFILTER      = 6,
		    MIPFILTER      = 7,
		    MIPMAPLODBIAS  = 8,
		    MAXMIPLEVEL    = 9,
		    MAXANISOTROPY  = 10,
		    SRGBTEXTURE    = 11,
		    ELEMENTINDEX   = 12,
		    DMAPOFFSET     = 13,
		                                
		    FORCE_DWORD   = 0x7fffffff,
		};

		public enum STATE_CLASS
		{
		    LIGHTENABLE,
		    FVF,
		    LIGHT,
		    MATERIAL,
		    NPATCHMODE,
		    PIXELSHADER,
		    RENDERSTATE,
		    SETSAMPLER,
		    SAMPLERSTATE,
		    TEXTURE,
		    TEXTURESTAGE,
		    TRANSFORM,
		    VERTEXSHADER,
		    SHADERCONST,
		    UNKNOWN,
		};

		public enum MATERIAL_TYPE
		{
		    DIFFUSE,
		    AMBIENT,
		    SPECULAR,
		    EMISSIVE,
		    POWER,
		};

		public enum LIGHT_TYPE
		{
		    TYPE,
		    DIFFUSE,
		    SPECULAR,
		    AMBIENT,
		    POSITION,
		    DIRECTION,
		    RANGE,
		    FALLOFF,
		    ATTENUATION0,
		    ATTENUATION1,
		    ATTENUATION2,
		    THETA,
		    PHI,
		};

		public enum SHADER_CONSTANT_TYPE
		{
		    VSFLOAT,
		    VSBOOL,
		    VSINT,
		    PSFLOAT,
		    PSBOOL,
		    PSINT,
		}
		
		public enum STATE_TYPE
		{
			CONSTANT,
			PARAMETER,
			EXPRESSION,
			EXPRESSIONINDEX,
		}
		
		public class d3dx_parameter
		{
			public string name;
			public string semantic;
			public object data;
			public D3DXPARAMETER_CLASS class_;
			public D3DXPARAMETER_TYPE  type;
			public uint rows;
			public uint columns;
			public uint element_count;
			public uint annotation_count;
			public uint member_count;
			public uint flags;
			public uint bytes;
			
			public d3dx_parameter[] annotation_handles;
			public d3dx_parameter[] member_handles;
		}
		
		public class d3dx_state
		{
			public state_info operation;
			public uint index;
			public STATE_TYPE type;
			public d3dx_parameter parameter;
		}
		
		public class d3dx_sampler
		{
			public uint state_count;
			public d3dx_state[] states;
		}
		
		public class d3dx_pass
		{
			public string name;
			public uint state_count;
			public uint annotation_count;
			
			public d3dx_state[] states;
			public d3dx_parameter[] annotation_handles;
		}
		
		public class d3dx_technique
		{
			public string name;
			public uint pass_count;
			public uint annotation_count;
			
			public d3dx_parameter[] annotation_handles;
			public d3dx_pass[] pass_handles;
		}


		public struct state_info
		{
			public STATE_CLASS class_;
			public uint op;
			public string name;

			public state_info(STATE_CLASS class_, uint op, string name) {
				this.class_ = class_;
				this.op = op;
				this.name = name;
			}
		}
		private state_info[] state_table =
		{
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ZENABLE, "ZENABLE"), /* 0x0 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.FILLMODE, "FILLMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.SHADEMODE, "SHADEMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ZWRITEENABLE, "ZWRITEENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ALPHATESTENABLE, "ALPHATESTENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.LASTPIXEL, "LASTPIXEL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.SRCBLEND, "SRCBLEND"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.DESTBLEND, "DESTBLEND"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.CULLMODE, "CULLMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ZFUNC, "ZFUNC"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ALPHAREF, "ALPHAREF"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ALPHAFUNC, "ALPHAFUNC"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.DITHERENABLE, "DITHERENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ALPHABLENDENABLE, "ALPHABLENDENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.FOGENABLE, "FOGENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.SPECULARENABLE, "SPECULARENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.FOGCOLOR, "FOGCOLOR"), /* 0x10 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.FOGTABLEMODE, "FOGTABLEMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.FOGSTART, "FOGSTART"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.FOGEND, "FOGEND"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.FOGDENSITY, "FOGDENSITY"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.RANGEFOGENABLE, "RANGEFOGENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.STENCILENABLE, "STENCILENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.STENCILFAIL, "STENCILFAIL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.STENCILZFAIL, "STENCILZFAIL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.STENCILPASS, "STENCILPASS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.STENCILFUNC, "STENCILFUNC"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.STENCILREF, "STENCILREF"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.STENCILMASK, "STENCILMASK"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.STENCILWRITEMASK, "STENCILWRITEMASK"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.TEXTUREFACTOR, "TEXTUREFACTOR"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP0, "WRAP0"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP1, "WRAP1"), /* 0x20 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP2, "WRAP2"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP3, "WRAP3"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP4, "WRAP4"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP5, "WRAP5"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP6, "WRAP6"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP7, "WRAP7"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP8, "WRAP8"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP9, "WRAP9"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP10, "WRAP10"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP11, "WRAP11"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP12, "WRAP12"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP13, "WRAP13"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP14, "WRAP14"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.WRAP15, "WRAP15"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.CLIPPING, "CLIPPING"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.LIGHTING, "LIGHTING"), /* 0x30 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.AMBIENT, "AMBIENT"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.FOGVERTEXMODE, "FOGVERTEXMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.COLORVERTEX, "COLORVERTEX"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.LOCALVIEWER, "LOCALVIEWER"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.NORMALIZENORMALS, "NORMALIZENORMALS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.DIFFUSEMATERIALSOURCE, "DIFFUSEMATERIALSOURCE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.SPECULARMATERIALSOURCE, "SPECULARMATERIALSOURCE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.AMBIENTMATERIALSOURCE, "AMBIENTMATERIALSOURCE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.EMISSIVEMATERIALSOURCE, "EMISSIVEMATERIALSOURCE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.VERTEXBLEND, "VERTEXBLEND"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.CLIPPLANEENABLE, "CLIPPLANEENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.POINTSIZE, "POINTSIZE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.POINTSIZE_MIN, "POINTSIZE_MIN"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.POINTSIZE_MAX, "POINTSIZE_MAX"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.POINTSPRITEENABLE, "POINTSPRITEENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.POINTSCALEENABLE, "POINTSCALEENABLE"), /* 0x40 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.POINTSCALE_A, "POINTSCALE_A"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.POINTSCALE_B, "POINTSCALE_B"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.POINTSCALE_C, "POINTSCALE_C"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.MULTISAMPLEANTIALIAS, "MULTISAMPLEANTIALIAS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.MULTISAMPLEMASK, "MULTISAMPLEMASK"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.PATCHEDGESTYLE, "PATCHEDGESTYLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.DEBUGMONITORTOKEN, "DEBUGMONITORTOKEN"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.INDEXEDVERTEXBLENDENABLE, "INDEXEDVERTEXBLENDENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.COLORWRITEENABLE, "COLORWRITEENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.TWEENFACTOR, "TWEENFACTOR"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.BLENDOP, "BLENDOP"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.POSITIONDEGREE, "POSITIONDEGREE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.NORMALDEGREE, "NORMALDEGREE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.SCISSORTESTENABLE, "SCISSORTESTENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.SLOPESCALEDEPTHBIAS, "SLOPESCALEDEPTHBIAS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ANTIALIASEDLINEENABLE, "ANTIALIASEDLINEENABLE"), /* 0x50 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.MINTESSELLATIONLEVEL, "MINTESSELLATIONLEVEL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.MAXTESSELLATIONLEVEL, "MAXTESSELLATIONLEVEL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ADAPTIVETESS_X, "ADAPTIVETESS_X"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ADAPTIVETESS_Y, "ADAPTIVETESS_Y"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ADAPTIVETESS_Z, "ADAPTIVETESS_Z"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ADAPTIVETESS_W, "ADAPTIVETESS_W"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.ENABLEADAPTIVETESSELLATION, "ENABLEADAPTIVETESSELLATION"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.TWOSIDEDSTENCILMODE, "TWOSIDEDSTENCILMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.CCW_STENCILFAIL, "CCW_STENCILFAIL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.CCW_STENCILZFAIL, "CCW_STENCILZFAIL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.CCW_STENCILPASS, "CCW_STENCILPASS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.CCW_STENCILFUNC, "CCW_STENCILFUNC"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.COLORWRITEENABLE1, "COLORWRITEENABLE1"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.COLORWRITEENABLE2, "COLORWRITEENABLE2"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.COLORWRITEENABLE3, "COLORWRITEENABLE3"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.BLENDFACTOR, "BLENDFACTOR"), /* 0x60 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.SRGBWRITEENABLE, "SRGBWRITEENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.DEPTHBIAS, "DEPTHBIAS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.SEPARATEALPHABLENDENABLE, "SEPARATEALPHABLENDENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.SRCBLENDALPHA, "SRCBLENDALPHA"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.DESTBLENDALPHA, "DESTBLENDALPHA"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)D3DRENDERSTATETYPE.BLENDOPALPHA, "BLENDOPALPHA"),
			/* Texture stages */
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.COLOROP, "COLOROP"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.COLORARG0, "COLORARG0"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.COLORARG1, "COLORARG1"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.COLORARG2, "COLORARG2"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.ALPHAOP, "ALPHAOP"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.ALPHAARG0, "ALPHAARG0"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.ALPHAARG1, "ALPHAARG1"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.ALPHAARG2, "ALPHAARG2"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.RESULTARG, "RESULTARG"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.BUMPENVMAT00, "BUMPENVMAT00"), /* 0x70 */
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.BUMPENVMAT01, "BUMPENVMAT01"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.BUMPENVMAT10, "BUMPENVMAT10"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.BUMPENVMAT11, "BUMPENVMAT11"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.TEXCOORDINDEX, "TEXCOORDINDEX"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.BUMPENVLSCALE, "BUMPENVLSCALE"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.BUMPENVLOFFSET, "BUMPENVLOFFSET"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)D3DTEXTURESTAGESTATETYPE.TEXTURETRANSFORMFLAGS, "TEXTURETRANSFORMFLAGS"),
			/* */
			new state_info(STATE_CLASS.UNKNOWN, 0, "UNKNOWN"),
			/* NPatchMode */
			new state_info(STATE_CLASS.NPATCHMODE, 0, "NPatchMode"),
			/* */
			new state_info(STATE_CLASS.UNKNOWN, 0, "UNKNOWN"),
			/* Transform */
			new state_info(STATE_CLASS.TRANSFORM, (uint)D3DTRANSFORMSTATETYPE.PROJECTION, "PROJECTION"),
			new state_info(STATE_CLASS.TRANSFORM, (uint)D3DTRANSFORMSTATETYPE.VIEW, "VIEW"),
			new state_info(STATE_CLASS.TRANSFORM, (uint)D3DTRANSFORMSTATETYPE.WORLD, "WORLD"),
			new state_info(STATE_CLASS.TRANSFORM, (uint)D3DTRANSFORMSTATETYPE.TEXTURE0, "TEXTURE0"),
			/* Material */
			new state_info(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.DIFFUSE, "MaterialDiffuse"),
			new state_info(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.AMBIENT, "MaterialAmbient"), /* 0x80 */
			new state_info(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.SPECULAR, "MaterialSpecular"),
			new state_info(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.EMISSIVE, "MaterialEmissive"),
			new state_info(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.POWER, "MaterialPower"),
			/* Light */
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.TYPE, "LightType"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.DIFFUSE, "LightDiffuse"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.SPECULAR, "LightSpecular"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.AMBIENT, "LightAmbient"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.POSITION, "LightPosition"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.DIRECTION, "LightDirection"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.RANGE, "LightRange"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.FALLOFF, "LightFallOff"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.ATTENUATION0, "LightAttenuation0"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.ATTENUATION1, "LightAttenuation1"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.ATTENUATION2, "LightAttenuation2"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.THETA, "LightTheta"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.PHI, "LightPhi"), /* 0x90 */
			/* Ligthenable */
			new state_info(STATE_CLASS.LIGHTENABLE, 0, "LightEnable"),
			/* Vertexshader */
			new state_info(STATE_CLASS.VERTEXSHADER, 0, "Vertexshader"),
			/* Pixelshader */
			new state_info(STATE_CLASS.PIXELSHADER, 0, "Pixelshader"),
			/* Shader constants */
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstantF"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSBOOL, "VertexShaderConstantB"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSINT, "VertexShaderConstantI"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant1"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant2"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant3"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant4"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstantF"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSBOOL, "PixelShaderConstantB"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSINT, "PixelShaderConstantI"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant1"), /* 0xa0 */
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant2"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant3"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant4"),
			/* Texture */
			new state_info(STATE_CLASS.TEXTURE, 0, "Texture"),
			/* Sampler states */
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.ADDRESSU, "AddressU"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.ADDRESSV, "AddressV"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.ADDRESSW, "AddressW"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.BORDERCOLOR, "BorderColor"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.MAGFILTER, "MagFilter"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.MINFILTER, "MinFilter"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.MIPFILTER, "MipFilter"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.MIPMAPLODBIAS, "MipMapLodBias"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.MAXMIPLEVEL, "MaxMipLevel"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.MAXANISOTROPY, "MaxAnisotropy"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.SRGBTEXTURE, "SRGBTexture"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.ELEMENTINDEX, "ElementIndex"), /* 0xb0 */
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)D3DSAMPLERSTATETYPE.DMAPOFFSET, "DMAPOffset"),
			/* Set sampler */
			new state_info(STATE_CLASS.SETSAMPLER, 0, "Sampler"),
		};



		private MemoryStream effectStream;
		private BinaryReader effectReader;
		
		public d3dx_parameter[] parameter_handles;
		public d3dx_technique[] technique_handles;

		public d3dx_parameter[] objects;

		public DXEffectObject(byte[] effectCode)
		{
			uint baseOffset = 8;
			uint tag = BitConverter.ToUInt32(effectCode, 0);
			if (tag == 0xBCF00BCF) {
				//handling this extra stuff should really be in the Effect class
				baseOffset += BitConverter.ToUInt32(effectCode, 4);
			} else if (tag != 0xFEFF0901) {
				//effect too old or too new, or ascii which we can't compile atm
				throw new NotImplementedException();
			}


			uint startoffset = BitConverter.ToUInt32(effectCode, (int)(baseOffset-4));

			effectStream = new MemoryStream(effectCode,
				(int)baseOffset, (int)(effectCode.Length-baseOffset));
			effectReader = new BinaryReader(effectStream);

			effectStream.Seek(startoffset, SeekOrigin.Current);
			
			uint parameterCount = effectReader.ReadUInt32();
			uint techniqueCount = effectReader.ReadUInt32();
			effectReader.ReadUInt32(); //unkn
			uint objectCount = effectReader.ReadUInt32();
			
			objects = new d3dx_parameter[objectCount];
			
			parameter_handles = new d3dx_parameter[parameterCount];
			for (int i=0; i<parameterCount; i++) {
				parameter_handles[i] = parse_effect_parameter();
			}
			
			technique_handles = new d3dx_technique[techniqueCount];
			for (int i=0; i<techniqueCount; i++) {
				technique_handles[i] = parse_effect_technique();
			}
			
			uint stringCount = effectReader.ReadUInt32 ();
			uint resourceCount = effectReader.ReadUInt32 ();
			
			for (int i=0; i<stringCount; i++) {
				uint id = effectReader.ReadUInt32 ();
				parse_data(objects[id]);
			}
			
			for (int i=0; i<resourceCount; i++) {
				parse_resource();
			}
			
			effectReader.Close();
			effectStream.Close();
		}
		
		private string parse_name(long offset)
		{
			long oldPos = effectStream.Position; effectStream.Seek (offset, SeekOrigin.Begin);
			
			byte[] rb = effectReader.ReadBytes(effectReader.ReadInt32());
			string r = System.Text.ASCIIEncoding.ASCII.GetString (rb);
			r = r.Replace ("\0", "");
			
			effectStream.Seek (oldPos, SeekOrigin.Begin);
			return r;
		}
		
		private void parse_data(d3dx_parameter param)
		{
			uint size = effectReader.ReadUInt32 ();
			switch (param.type)
			{
			case D3DXPARAMETER_TYPE.STRING:
				param.data = parse_name(effectStream.Position-4);
				effectReader.ReadBytes((int)((size+3) & ~3));	break;
			case D3DXPARAMETER_TYPE.VERTEXSHADER:
				param.data = new DXShader(effectReader.ReadBytes((int)((size+3) & ~3)));
				break;
			case D3DXPARAMETER_TYPE.PIXELSHADER:
				param.data = new DXShader(effectReader.ReadBytes((int)((size+3) & ~3)));
				break;
			}
			

		}
		
		private byte[] copy_data()
		{
			uint size = effectReader.ReadUInt32 ();
			return effectReader.ReadBytes((int)((size+3) & ~3));
		}
		
		private void parse_resource()
		{
			d3dx_state state;
			
			uint technique_index = effectReader.ReadUInt32 ();
			uint index = effectReader.ReadUInt32 ();
			uint element_index = effectReader.ReadUInt32 ();
			uint state_index = effectReader.ReadUInt32 ();
			uint usage = effectReader.ReadUInt32 ();
			
			if (technique_index == 0xffffffff) {
				d3dx_parameter parameter = parameter_handles[index];
				
				if (element_index != 0xffffffff) {
					if (parameter.element_count != 0) {
						parameter = parameter.member_handles[element_index];
					}
				}
				d3dx_sampler sampler = (d3dx_sampler)parameter.data;
				
				state = sampler.states[state_index];
			} else {
				d3dx_technique technique = technique_handles[technique_index];
				d3dx_pass pass = technique.pass_handles[index];
				
				state = pass.states[state_index];
			}
			
			//parameter assignment
			d3dx_parameter param = state.parameter;
			Console.WriteLine ("resource usage="+usage.ToString());
			switch (usage)
			{
			case 0:
				switch (param.type)
				{
				case D3DXPARAMETER_TYPE.VERTEXSHADER:
				case D3DXPARAMETER_TYPE.PIXELSHADER:
					state.type = STATE_TYPE.CONSTANT;
					parse_data(param);
					break;
				case D3DXPARAMETER_TYPE.BOOL:
				case D3DXPARAMETER_TYPE.INT:
				case D3DXPARAMETER_TYPE.FLOAT:
				case D3DXPARAMETER_TYPE.STRING:
					//assignment by FXLVM expression
					state.type = STATE_TYPE.EXPRESSION;
					param.data = copy_data();
					break;
				
				default:
					throw new NotImplementedException();
				}
				break;
			case 1:
				state.type = STATE_TYPE.PARAMETER;
				//the state's parameter is another parameter
				//the we are given its name
				
				uint nameLength_ = effectReader.ReadUInt32 ();
				string name = parse_name (effectStream.Position-4);
				effectStream.Seek ((nameLength_+3) & ~3, SeekOrigin.Current);
				
				foreach (d3dx_parameter findParam in parameter_handles) {
					if (findParam.name == name) {
						param.data = findParam.data;
						param.name = findParam.name;
						//todo: copy other stuff
						break;
					}
				}
				break;
			case 2:
				//Array index by FXLVM expression
				state.type = STATE_TYPE.EXPRESSIONINDEX;
				//preceded by array name
				
				//annoying hax to extract the name
				uint length = effectReader.ReadUInt32 ();
				uint nameLength = effectReader.ReadUInt32 ();
				string paramName = parse_name (effectStream.Position-4);
				effectStream.Seek ( (nameLength+3) & ~3, SeekOrigin.Current);
				byte[] expressionData = effectReader.ReadBytes ((int)(length-4-nameLength));
				
				param.data = new DXExpression(paramName, expressionData);
				break;
			default:
				Console.WriteLine ("Unknown usage "+usage.ToString());
				break;
			}
			
		}
		
		private void parse_effect_typedef(long offset, d3dx_parameter param, d3dx_parameter parent, uint flags)
		{
			long oldPos = effectStream.Position; effectStream.Seek (offset, SeekOrigin.Begin);
			
			param.flags = flags;
			
			if (parent != null) {
				/* elements */
				//we evaluate recursively because elements may have members
				param.type = parent.type;
				param.class_ = parent.class_;
				param.name = parent.name;
				param.semantic = parent.semantic;
				param.element_count = 0;
				param.annotation_count = 0;
				param.member_count = parent.member_count;
				param.bytes = parent.bytes;
				param.rows = parent.rows;
				param.columns = parent.columns;
			} else {
				param.type = (D3DXPARAMETER_TYPE)effectReader.ReadUInt32();
				param.class_ = (D3DXPARAMETER_CLASS)effectReader.ReadUInt32();
				param.name = parse_name(effectReader.ReadUInt32 ());
				param.semantic = parse_name(effectReader.ReadUInt32());
				param.element_count = effectReader.ReadUInt32();
				
				switch (param.class_)
				{
				case D3DXPARAMETER_CLASS.VECTOR:
					param.columns = effectReader.ReadUInt32();
					param.rows = effectReader.ReadUInt32();
					param.bytes = 4 * param.rows * param.columns;
					break;
				
				case D3DXPARAMETER_CLASS.SCALAR:
				case D3DXPARAMETER_CLASS.MATRIX_ROWS:
				case D3DXPARAMETER_CLASS.MATRIX_COLUMNS:
					param.rows = effectReader.ReadUInt32();
					param.columns = effectReader.ReadUInt32();
					param.bytes = 4 * param.rows * param.columns;
					break;
				
				case D3DXPARAMETER_CLASS.STRUCT:
					param.member_count = effectReader.ReadUInt32 ();
					break;
					
				case D3DXPARAMETER_CLASS.OBJECT:
					switch (param.type)
					{
					case D3DXPARAMETER_TYPE.STRING:
						param.bytes = 4; //sizeof(LPCSTR)
						break;
					
					case D3DXPARAMETER_TYPE.PIXELSHADER:
						param.bytes = 4; //sizeof(LPDIRECT3DPIXELSHADER9)
						break;
					
					case D3DXPARAMETER_TYPE.VERTEXSHADER:
						param.bytes = 4; //sizeof(LPDIRECT3DVERTEXSHADER9)
						break;
					
					case D3DXPARAMETER_TYPE.TEXTURE:
					case D3DXPARAMETER_TYPE.TEXTURE1D:
					case D3DXPARAMETER_TYPE.TEXTURE2D:
					case D3DXPARAMETER_TYPE.TEXTURE3D:
					case D3DXPARAMETER_TYPE.TEXTURECUBE:
						param.bytes = 4; //sizeof(LPDIRECT3DBASETEXTURE9)
						break;
					
					case D3DXPARAMETER_TYPE.SAMPLER:
					case D3DXPARAMETER_TYPE.SAMPLER1D:
					case D3DXPARAMETER_TYPE.SAMPLER2D:
					case D3DXPARAMETER_TYPE.SAMPLER3D:
					case D3DXPARAMETER_TYPE.SAMPLERCUBE:
						param.bytes = 0;
						break;
					
					default:
						throw new NotImplementedException();
					}
					break;
				
				default:
					throw new NotImplementedException();
					
				}
			}
			
			if (param.element_count > 0) {
				uint param_bytes = 0;
				param.member_handles = new d3dx_parameter[param.element_count];
				for (int i=0; i<param.element_count; i++) {
					param.member_handles[i] = new d3dx_parameter();

					//we read the same typedef over and over...
					parse_effect_typedef (effectStream.Position, param.member_handles[i], param, flags);
					param_bytes += param.member_handles[i].bytes;
				}
				param.bytes = param_bytes;
			} else if (param.member_count > 0) {
				param.member_handles = new d3dx_parameter[param.member_count];
				for (int i=0; i<param.member_count; i++) {
					param.member_handles[i] = new d3dx_parameter();

					parse_effect_typedef(effectStream.Position, param.member_handles[i], null, flags);
					effectStream.Seek(param.member_handles[i].bytes, SeekOrigin.Current);
					param.bytes += param.member_handles[i].bytes;
				}
			}
			
			effectStream.Seek (oldPos, SeekOrigin.Begin);
		}
		
		private d3dx_sampler parse_sampler()
		{
			d3dx_sampler ret = new d3dx_sampler();
			
			ret.state_count = effectReader.ReadUInt32 ();
			if (ret.state_count > 0) {
				ret.states = new d3dx_state[ret.state_count];
				for (int i=0; i<ret.state_count; i++) {
					ret.states[i] = parse_state();
				}
			}
			
			return ret;
		}

		private byte[] sliceBytes(byte[] data, uint start, uint stop) {
			byte[] ret = new byte[stop-start];
			for (uint i=start; i<stop; i++) {
				ret[i-start] = data[i];
			}
			return ret;
		}
		
		private void parse_value(d3dx_parameter param, byte[] data)
		{
			if (param.element_count != 0) {
				param.data = data;
				uint curOffset = 0;
				for (int i=0; i<param.element_count; i++) {
					parse_value(param.member_handles[i],
						sliceBytes(data, curOffset, curOffset+param.member_handles[i].bytes));
					curOffset += param.member_handles[i].bytes;
				}
			} else {
				switch (param.class_)
				{
				case D3DXPARAMETER_CLASS.SCALAR:
				case D3DXPARAMETER_CLASS.VECTOR:
				case D3DXPARAMETER_CLASS.MATRIX_ROWS:
				case D3DXPARAMETER_CLASS.MATRIX_COLUMNS:
					param.data = data;
					break;
				
				case D3DXPARAMETER_CLASS.STRUCT:
					param.data = data;
					uint curOffset = 0;
					for (int i=0; i<param.member_count; i++) {
						parse_value(param.member_handles[i],
							sliceBytes(data, curOffset, curOffset+param.member_handles[i].bytes));
						curOffset += param.member_handles[i].bytes;
					}
					break;
				
				case D3DXPARAMETER_CLASS.OBJECT:
					
					switch (param.type)
					{
					case D3DXPARAMETER_TYPE.STRING:
					case D3DXPARAMETER_TYPE.TEXTURE:
					case D3DXPARAMETER_TYPE.TEXTURE1D:
					case D3DXPARAMETER_TYPE.TEXTURE2D:
					case D3DXPARAMETER_TYPE.TEXTURE3D:
					case D3DXPARAMETER_TYPE.TEXTURECUBE:
					case D3DXPARAMETER_TYPE.PIXELSHADER:
					case D3DXPARAMETER_TYPE.VERTEXSHADER:
						uint id = effectReader.ReadUInt32 ();
						objects[id] = param;
						param.data = data;
						break;
					
					case D3DXPARAMETER_TYPE.SAMPLER:
					case D3DXPARAMETER_TYPE.SAMPLER1D:
					case D3DXPARAMETER_TYPE.SAMPLER2D:
					case D3DXPARAMETER_TYPE.SAMPLER3D:
					case D3DXPARAMETER_TYPE.SAMPLERCUBE:
						param.data = parse_sampler();
						break;
					}
					
					break;
				}
			}

		}
		
		private void parse_init_value(long offset, d3dx_parameter param)
		{
			long oldPos = effectStream.Position; effectStream.Seek (offset, SeekOrigin.Begin);
			
			byte[] data = effectReader.ReadBytes((int)param.bytes);
			effectStream.Seek (offset, SeekOrigin.Begin);
			parse_value(param, data);
			
			effectStream.Seek (oldPos, SeekOrigin.Begin);
		}
		
		private d3dx_parameter parse_effect_annotation()
		{
			d3dx_parameter ret = new d3dx_parameter();
			
			ret.flags = D3DX_PARAMETER_ANNOTATION;
			
			long typedefOffset = effectReader.ReadInt32 ();
			parse_effect_typedef(typedefOffset, ret, null, D3DX_PARAMETER_ANNOTATION);
			
			long valueOffset = effectReader.ReadInt32 ();
			parse_init_value(valueOffset, ret);
			
			return ret;
		}
		
		private d3dx_parameter parse_effect_parameter()
		{
			d3dx_parameter ret = new d3dx_parameter();
			
			long typedefOffset = effectReader.ReadInt32 ();
			long valueOffset = effectReader.ReadInt32 ();
			ret.flags = effectReader.ReadUInt32 ();
			ret.annotation_count = effectReader.ReadUInt32 ();
			parse_effect_typedef(typedefOffset, ret, null, ret.flags);
			
			parse_init_value(valueOffset, ret);

			if (ret.annotation_count > 0) {
				ret.annotation_handles = new d3dx_parameter[ret.annotation_count];
				for (int i=0; i<ret.annotation_count; i++) {
					ret.annotation_handles[i] = parse_effect_annotation();
				}
			}
			
			return ret;
		}
		
		private d3dx_state parse_state()
		{
			d3dx_state ret = new d3dx_state();
			ret.parameter = new d3dx_parameter();

			ret.type = STATE_TYPE.CONSTANT;
			ret.operation = state_table[effectReader.ReadUInt32 ()];
			ret.index = effectReader.ReadUInt32 ();
			
			long typedefOffset = effectReader.ReadInt32 ();
			parse_effect_typedef(typedefOffset, ret.parameter, null, 0);

			long valueOffset = effectReader.ReadInt32 ();
			parse_init_value(valueOffset, ret.parameter);
			
			if (ret.operation.class_ == STATE_CLASS.RENDERSTATE) {
				//parse the render parameter
				switch (ret.operation.op) {
				case (uint)D3DRENDERSTATETYPE.STENCILENABLE:
				case (uint)D3DRENDERSTATETYPE.ALPHABLENDENABLE:
				case (uint)D3DRENDERSTATETYPE.SCISSORTESTENABLE:
					ret.parameter.data = BitConverter.ToInt32 ((byte[])ret.parameter.data, 0) != 0;
					break;
				case (uint)D3DRENDERSTATETYPE.COLORWRITEENABLE:
					ret.parameter.data = (ColorWriteChannels)BitConverter.ToInt32 ((byte[])ret.parameter.data, 0);
					break;
				case (uint)D3DRENDERSTATETYPE.BLENDOP:
					switch (BitConverter.ToInt32((byte[])ret.parameter.data, 0)) {
					case 1: ret.parameter.data = BlendFunction.Add; break;
					case 2: ret.parameter.data = BlendFunction.Subtract; break;
					case 3: ret.parameter.data = BlendFunction.ReverseSubtract; break;
					case 4: ret.parameter.data = BlendFunction.Min; break;
					case 5: ret.parameter.data = BlendFunction.Max; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.SRCBLEND:
				case (uint)D3DRENDERSTATETYPE.DESTBLEND:
					switch (BitConverter.ToInt32((byte[])ret.parameter.data, 0)) {
					case 1: ret.parameter.data = Blend.Zero; break;
					case 2: ret.parameter.data = Blend.One; break;
					case 3: ret.parameter.data = Blend.SourceColor; break;
					case 4: ret.parameter.data = Blend.InverseSourceColor; break;
					case 5: ret.parameter.data = Blend.SourceAlpha; break;
					case 6: ret.parameter.data = Blend.InverseSourceAlpha; break;
					case 7: ret.parameter.data = Blend.DestinationAlpha; break;
					case 8: ret.parameter.data = Blend.InverseDestinationAlpha; break;
					case 9: ret.parameter.data = Blend.DestinationColor; break;
					case 10: ret.parameter.data = Blend.InverseDestinationColor; break;
					case 11: ret.parameter.data = Blend.SourceAlphaSaturation; break;
					case 14: ret.parameter.data = Blend.BlendFactor; break;
					case 15: ret.parameter.data = Blend.InverseDestinationAlpha; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.CULLMODE:
					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) {
					case 1: ret.parameter.data = CullMode.None; break;
					case 2: ret.parameter.data = CullMode.CullClockwiseFace; break;
					case 3: ret.parameter.data = CullMode.CullCounterClockwiseFace; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.STENCILFUNC:
					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) {
					case 1: ret.parameter.data = CompareFunction.Never; break;
					case 2: ret.parameter.data = CompareFunction.Less; break;
					case 3: ret.parameter.data = CompareFunction.Equal; break;
					case 4: ret.parameter.data = CompareFunction.LessEqual; break;
					case 5: ret.parameter.data = CompareFunction.Greater; break;
					case 6: ret.parameter.data = CompareFunction.NotEqual; break;
					case 7: ret.parameter.data = CompareFunction.GreaterEqual; break;
					case 8: ret.parameter.data = CompareFunction.Always; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.STENCILFAIL:
				case (uint)D3DRENDERSTATETYPE.STENCILPASS:
					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) {
					case 1: ret.parameter.data = StencilOperation.Keep; break;
					case 2: ret.parameter.data = StencilOperation.Zero; break;
					case 3: ret.parameter.data = StencilOperation.Replace; break;
					case 4: ret.parameter.data = StencilOperation.IncrementSaturation; break;
					case 5: ret.parameter.data = StencilOperation.DecrementSaturation; break;
					case 6: ret.parameter.data = StencilOperation.Invert; break;
					case 7: ret.parameter.data = StencilOperation.Increment; break;
					case 8: ret.parameter.data = StencilOperation.Decrement; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.STENCILREF:
					ret.parameter.data = BitConverter.ToInt32 ((byte[])ret.parameter.data, 0);
					break;
				default:
					throw new NotImplementedException();
				}
			}
			
			
			
			return ret;
		}
		
		private d3dx_pass parse_effect_pass()
		{
			d3dx_pass ret = new d3dx_pass();
			
			ret.name = parse_name (effectReader.ReadUInt32());
			ret.annotation_count = effectReader.ReadUInt32 ();
			ret.state_count = effectReader.ReadUInt32 ();

			if (ret.annotation_count > 0) {
				ret.annotation_handles = new d3dx_parameter[ret.annotation_count];
				for (int i=0; i<ret.annotation_count; i++) {
					ret.annotation_handles[i] = parse_effect_annotation();
				}
			}
			
			ret.states = new d3dx_state[ret.state_count];
			for (int i=0; i<ret.state_count; i++) {
				ret.states[i] = parse_state();
			}
			
			return ret;
		}
		
		private d3dx_technique parse_effect_technique()
		{
			d3dx_technique ret = new d3dx_technique();
			
			ret.name = parse_name (effectReader.ReadUInt32());
			ret.annotation_count = effectReader.ReadUInt32 ();
			ret.pass_count = effectReader.ReadUInt32 ();

			if (ret.annotation_count > 0) {
				ret.annotation_handles = new d3dx_parameter[ret.annotation_count];
				for (int i=0; i<ret.annotation_count; i++) {
					ret.annotation_handles[i] = parse_effect_annotation();
				}
			}

			if (ret.pass_count > 0) {
				ret.pass_handles = new d3dx_pass[ret.pass_count];
				for (int i=0; i<ret.pass_count; i++) {
					ret.pass_handles[i] = parse_effect_pass();
				}
			}
			
			return ret;
		}
		
	}
}

