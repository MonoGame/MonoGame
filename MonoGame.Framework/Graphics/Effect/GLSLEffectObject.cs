using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	public class GLSLEffectObject
	{

		public enum GLSLRENDERSTATETYPE {
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

		public enum GLSLTEXTURESTAGESTATETYPE {
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

		public enum GLSLTRANSFORMSTATETYPE {
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

		public const int PARAMETER_ANNOTATION = 4;

		public enum glslEffectParameterClass
		{
			Scalar = 0,
			Vector = 1,
			MatrixRows = 2,
			MatrixColumns = 3,
			Object = 4,
			Struct = 5,
		}

		public enum glslEffectParameterType
		{
			Void = 0,
			Bool = 1,
			Int32 = 2,
			Single = 3,
			String = 4,
			Texture = 5,
			Texture1D = 6,
			Texture2D = 7,
			Texture3D = 8,
			TextureCube = 9,
			Sampler = 10,
			Sampler1D = 11,
			Sampler2D = 12,
			Sampler3D = 13,
			SamplerCube = 14,
			PixelShader = 15,
			VertexShader = 16,
			ShaderFunc = 17,
			UInt32 = 18,
			Struct = 19
		}

		enum GLSLSAMPLERSTATETYPE {
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

		// Effect States (Direct3D 9)
		// http://msdn.microsoft.com/en-us/library/windows/desktop/bb173347(v=vs.85).aspx
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
		
		public class glslParameter
		{
			public string name;
			public string semantic;
			public object data;
			public glslEffectParameterClass class_;
			public glslEffectParameterType  type;
			public uint rows;
			public uint columns;
			public uint element_count;
			public uint annotation_count;
			public uint member_count;
			public uint flags;
			public uint bytes;
			public glslVectorType vectorType;

			public glslParameter[] annotation_handles;
			public glslParameter[] member_handles;
		}
		
		public class glsl_state
		{
			public glslStateInfo operation;
			public uint index;
			public STATE_TYPE type;
			public glslParameter parameter;
		}

		public class glslSampler
		{
			public uint state_count;
			public glsl_state[] states;
		}
		
		public class glslPass
		{
			public string name;
			public uint state_count;
			public uint annotation_count;

			public glsl_state[] states;
			public glslParameter[] annotation_handles;
		}

		public class glslTechnique
		{
			public string name;
			public uint pass_count;
			public uint annotation_count;

			public glslParameter[] annotation_handles;
			public glslPass[] pass_handles;
		}

		public struct glslStateInfo
		{
			public STATE_CLASS class_;
			public uint op;
			public string name;

			public glslStateInfo(STATE_CLASS class_, uint op, string name) {
				this.class_ = class_;
				this.op = op;
				this.name = name;
			}
		}

		private glslStateInfo[] state_table =
		{
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ZENABLE, "ZENABLE"), /* 0x0 */
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.FILLMODE, "FILLMODE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.SHADEMODE, "SHADEMODE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ZWRITEENABLE, "ZWRITEENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ALPHATESTENABLE, "ALPHATESTENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.LASTPIXEL, "LASTPIXEL"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.SRCBLEND, "SRCBLEND"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.DESTBLEND, "DESTBLEND"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.CULLMODE, "CULLMODE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ZFUNC, "ZFUNC"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ALPHAREF, "ALPHAREF"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ALPHAFUNC, "ALPHAFUNC"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.DITHERENABLE, "DITHERENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ALPHABLENDENABLE, "ALPHABLENDENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.FOGENABLE, "FOGENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.SPECULARENABLE, "SPECULARENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.FOGCOLOR, "FOGCOLOR"), /* 0x10 */
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.FOGTABLEMODE, "FOGTABLEMODE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.FOGSTART, "FOGSTART"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.FOGEND, "FOGEND"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.FOGDENSITY, "FOGDENSITY"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.RANGEFOGENABLE, "RANGEFOGENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.STENCILENABLE, "STENCILENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.STENCILFAIL, "STENCILFAIL"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.STENCILZFAIL, "STENCILZFAIL"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.STENCILPASS, "STENCILPASS"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.STENCILFUNC, "STENCILFUNC"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.STENCILREF, "STENCILREF"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.STENCILMASK, "STENCILMASK"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.STENCILWRITEMASK, "STENCILWRITEMASK"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.TEXTUREFACTOR, "TEXTUREFACTOR"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP0, "WRAP0"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP1, "WRAP1"), /* 0x20 */
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP2, "WRAP2"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP3, "WRAP3"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP4, "WRAP4"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP5, "WRAP5"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP6, "WRAP6"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP7, "WRAP7"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP8, "WRAP8"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP9, "WRAP9"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP10, "WRAP10"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP11, "WRAP11"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP12, "WRAP12"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP13, "WRAP13"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP14, "WRAP14"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.WRAP15, "WRAP15"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.CLIPPING, "CLIPPING"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.LIGHTING, "LIGHTING"), /* 0x30 */
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.AMBIENT, "AMBIENT"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.FOGVERTEXMODE, "FOGVERTEXMODE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.COLORVERTEX, "COLORVERTEX"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.LOCALVIEWER, "LOCALVIEWER"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.NORMALIZENORMALS, "NORMALIZENORMALS"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.DIFFUSEMATERIALSOURCE, "DIFFUSEMATERIALSOURCE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.SPECULARMATERIALSOURCE, "SPECULARMATERIALSOURCE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.AMBIENTMATERIALSOURCE, "AMBIENTMATERIALSOURCE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.EMISSIVEMATERIALSOURCE, "EMISSIVEMATERIALSOURCE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.VERTEXBLEND, "VERTEXBLEND"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.CLIPPLANEENABLE, "CLIPPLANEENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.POINTSIZE, "POINTSIZE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.POINTSIZE_MIN, "POINTSIZE_MIN"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.POINTSIZE_MAX, "POINTSIZE_MAX"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.POINTSPRITEENABLE, "POINTSPRITEENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.POINTSCALEENABLE, "POINTSCALEENABLE"), /* 0x40 */
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.POINTSCALE_A, "POINTSCALE_A"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.POINTSCALE_B, "POINTSCALE_B"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.POINTSCALE_C, "POINTSCALE_C"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.MULTISAMPLEANTIALIAS, "MULTISAMPLEANTIALIAS"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.MULTISAMPLEMASK, "MULTISAMPLEMASK"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.PATCHEDGESTYLE, "PATCHEDGESTYLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.DEBUGMONITORTOKEN, "DEBUGMONITORTOKEN"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.INDEXEDVERTEXBLENDENABLE, "INDEXEDVERTEXBLENDENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.COLORWRITEENABLE, "COLORWRITEENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.TWEENFACTOR, "TWEENFACTOR"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.BLENDOP, "BLENDOP"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.POSITIONDEGREE, "POSITIONDEGREE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.NORMALDEGREE, "NORMALDEGREE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.SCISSORTESTENABLE, "SCISSORTESTENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.SLOPESCALEDEPTHBIAS, "SLOPESCALEDEPTHBIAS"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ANTIALIASEDLINEENABLE, "ANTIALIASEDLINEENABLE"), /* 0x50 */
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.MINTESSELLATIONLEVEL, "MINTESSELLATIONLEVEL"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.MAXTESSELLATIONLEVEL, "MAXTESSELLATIONLEVEL"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ADAPTIVETESS_X, "ADAPTIVETESS_X"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ADAPTIVETESS_Y, "ADAPTIVETESS_Y"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ADAPTIVETESS_Z, "ADAPTIVETESS_Z"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ADAPTIVETESS_W, "ADAPTIVETESS_W"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.ENABLEADAPTIVETESSELLATION, "ENABLEADAPTIVETESSELLATION"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.TWOSIDEDSTENCILMODE, "TWOSIDEDSTENCILMODE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.CCW_STENCILFAIL, "CCW_STENCILFAIL"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.CCW_STENCILZFAIL, "CCW_STENCILZFAIL"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.CCW_STENCILPASS, "CCW_STENCILPASS"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.CCW_STENCILFUNC, "CCW_STENCILFUNC"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.COLORWRITEENABLE1, "COLORWRITEENABLE1"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.COLORWRITEENABLE2, "COLORWRITEENABLE2"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.COLORWRITEENABLE3, "COLORWRITEENABLE3"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.BLENDFACTOR, "BLENDFACTOR"), /* 0x60 */
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.SRGBWRITEENABLE, "SRGBWRITEENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.DEPTHBIAS, "DEPTHBIAS"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.SEPARATEALPHABLENDENABLE, "SEPARATEALPHABLENDENABLE"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.SRCBLENDALPHA, "SRCBLENDALPHA"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.DESTBLENDALPHA, "DESTBLENDALPHA"),
			new glslStateInfo(STATE_CLASS.RENDERSTATE, (uint)GLSLRENDERSTATETYPE.BLENDOPALPHA, "BLENDOPALPHA"),
			/* Texture stages */
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.COLOROP, "COLOROP"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.COLORARG0, "COLORARG0"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.COLORARG1, "COLORARG1"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.COLORARG2, "COLORARG2"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.ALPHAOP, "ALPHAOP"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.ALPHAARG0, "ALPHAARG0"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.ALPHAARG1, "ALPHAARG1"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.ALPHAARG2, "ALPHAARG2"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.RESULTARG, "RESULTARG"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.BUMPENVMAT00, "BUMPENVMAT00"), /* 0x70 */
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.BUMPENVMAT01, "BUMPENVMAT01"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.BUMPENVMAT10, "BUMPENVMAT10"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.BUMPENVMAT11, "BUMPENVMAT11"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.TEXCOORDINDEX, "TEXCOORDINDEX"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.BUMPENVLSCALE, "BUMPENVLSCALE"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.BUMPENVLOFFSET, "BUMPENVLOFFSET"),
			new glslStateInfo(STATE_CLASS.TEXTURESTAGE, (uint)GLSLTEXTURESTAGESTATETYPE.TEXTURETRANSFORMFLAGS, "TEXTURETRANSFORMFLAGS"),
			/* */
			new glslStateInfo(STATE_CLASS.UNKNOWN, 0, "UNKNOWN"),
			/* NPatchMode */
			new glslStateInfo(STATE_CLASS.NPATCHMODE, 0, "NPatchMode"),
			/* */
			new glslStateInfo(STATE_CLASS.UNKNOWN, 0, "UNKNOWN"),
			/* Transform */
			new glslStateInfo(STATE_CLASS.TRANSFORM, (uint)GLSLTRANSFORMSTATETYPE.PROJECTION, "PROJECTION"),
			new glslStateInfo(STATE_CLASS.TRANSFORM, (uint)GLSLTRANSFORMSTATETYPE.VIEW, "VIEW"),
			new glslStateInfo(STATE_CLASS.TRANSFORM, (uint)GLSLTRANSFORMSTATETYPE.WORLD, "WORLD"),
			new glslStateInfo(STATE_CLASS.TRANSFORM, (uint)GLSLTRANSFORMSTATETYPE.TEXTURE0, "TEXTURE0"),
			/* Material */
			new glslStateInfo(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.DIFFUSE, "MaterialDiffuse"),
			new glslStateInfo(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.AMBIENT, "MaterialAmbient"), /* 0x80 */
			new glslStateInfo(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.SPECULAR, "MaterialSpecular"),
			new glslStateInfo(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.EMISSIVE, "MaterialEmissive"),
			new glslStateInfo(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.POWER, "MaterialPower"),
			/* Light */
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.TYPE, "LightType"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.DIFFUSE, "LightDiffuse"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.SPECULAR, "LightSpecular"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.AMBIENT, "LightAmbient"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.POSITION, "LightPosition"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.DIRECTION, "LightDirection"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.RANGE, "LightRange"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.FALLOFF, "LightFallOff"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.ATTENUATION0, "LightAttenuation0"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.ATTENUATION1, "LightAttenuation1"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.ATTENUATION2, "LightAttenuation2"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.THETA, "LightTheta"),
			new glslStateInfo(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.PHI, "LightPhi"), /* 0x90 */
			/* Ligthenable */
			new glslStateInfo(STATE_CLASS.LIGHTENABLE, 0, "LightEnable"),
			/* Vertexshader */
			new glslStateInfo(STATE_CLASS.VERTEXSHADER, 0, "Vertexshader"),
			/* Pixelshader */
			new glslStateInfo(STATE_CLASS.PIXELSHADER, 0, "Pixelshader"),
			/* Shader constants */
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstantF"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSBOOL, "VertexShaderConstantB"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSINT, "VertexShaderConstantI"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant1"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant2"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant3"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant4"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstantF"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSBOOL, "PixelShaderConstantB"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSINT, "PixelShaderConstantI"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant1"), /* 0xa0 */
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant2"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant3"),
			new glslStateInfo(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant4"),
			/* Texture */
			new glslStateInfo(STATE_CLASS.TEXTURE, 0, "Texture"),
			/* Sampler states */
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.ADDRESSU, "AddressU"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.ADDRESSV, "AddressV"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.ADDRESSW, "AddressW"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.BORDERCOLOR, "BorderColor"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.MAGFILTER, "MagFilter"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.MINFILTER, "MinFilter"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.MIPFILTER, "MipFilter"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.MIPMAPLODBIAS, "MipMapLodBias"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.MAXMIPLEVEL, "MaxMipLevel"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.MAXANISOTROPY, "MaxAnisotropy"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.SRGBTEXTURE, "SRGBTexture"),
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.ELEMENTINDEX, "ElementIndex"), /* 0xb0 */
			new glslStateInfo(STATE_CLASS.SAMPLERSTATE, (uint)GLSLSAMPLERSTATETYPE.DMAPOFFSET, "DMAPOffset"),
			/* Set sampler */
			new glslStateInfo(STATE_CLASS.SETSAMPLER, 0, "Sampler"),
		};

		internal struct ShaderProg
		{
			public string shaderName;
			public glslEffectParameterType shaderType;
			public int shaderLength;
			public string shaderCode;
			public int shaderId;
			public List<string> shaderReqs;
			public GLSLShader glslShaderObject;
		}

		private struct PassInfo
		{
			public string FragProg;
			public string VertProg;
		}

		public enum TargetPlatform
		{
			Unknown,
			Windows,
			Xbox360,
			Zune
		}

		public enum glslVectorType
		{
			Scalar = 1,
			Vector2 = 2,
			Vector3 = 3,
			Vector4 = 4,
			Matrix2 = 5,
			Matrix3 = 6,
			Matrix4 = 7,
			Matrix23 = 8,
			Matrix24 = 9,
			Matrix32 = 10,
			Matrix34 = 11,
			Matrix42 = 12,
			Matrix43 = 13,
			Array = 14,
		}

		public class VertexAttributeInfo
		{
			public VertexAttributeType Type { get; set; }
			public string Name { get; set; }
		}

		[Flags]
		public enum VertexAttributeType
		{
			None = 0,
			Position = 1,
			Color = 2,
			Normal = 4,
			TexCoord = 8,
		}

		public class SamplerIndexInfo
		{
			public int Index { get; set; }
			public string Name { get; set; }
		}


		MemoryStream effectCodeStream;
		BinaryReader reader;

		public glslParameter[] parameter_handles;
		public glslTechnique[] technique_handles;
		
		public glslParameter[] objects;
		
		int objectIndex = 0;
		int parameter_handles_index = 0;

		public GLSLEffectObject (byte[] effectCode)
		{
			effectCodeStream = new MemoryStream(effectCode);
			reader = new BinaryReader (effectCodeStream);

			int magic = reader.ReadInt32 ();

			// check version
			if (reader.ReadInt32 () != 4)
				throw new Exception (); // TODO better exception, invalid version, must be 4
			
			int varSize = reader.ReadInt32 ();

			uint parameterCount = reader.ReadUInt32 ();  // Variable Count
			uint objectCount = reader.ReadUInt32();  // Total object count - Variables and objects

			objects = new glslParameter[objectCount];
			parameter_handles = new glslParameter[parameterCount];


			// create our param lists
			List<EffectParameter> effectParamList = new List<EffectParameter> ();
			Dictionary<string, EffectParameter> effectParamDict = new Dictionary<string, EffectParameter> ();
			
			// shader prog lists
			List<ShaderProg> shaderProgs = new List<ShaderProg> ();
			List<ShaderProg> shaderFuncs = new List<ShaderProg> ();
			
			//glslParseVariables (reader, objectCount, effectParamList, effectParamDict, shaderFuncs, shaderProgs);
			for (int p = 0; p < objectCount + parameterCount; p++) {
				glslParameter param = parseEffectParameter();
				if (param.class_ == glslEffectParameterClass.Object)
					objects[objectIndex++] = param;
				else
					parameter_handles[parameter_handles_index++] = param;
			}

			// read techniques and passes
			int techSize = reader.ReadInt32 ();
			int techCount = (int)reader.ReadInt16 ();

			technique_handles = new glslTechnique[techCount];

			for (int i=0; i<techCount; i++) {
				technique_handles[i] = parse_effect_technique();
			}
		}

		private string glslParseString ()
		{
			return glslParseString(reader);
		}

		private string glslParseString (BinaryReader reader)
		{
			byte stringCount = reader.ReadByte ();
			byte[] bstr = new byte[(int)stringCount];

			reader.Read (bstr, 0, (int)stringCount);
			reader.ReadByte();  // null terminated string
			return Encoding.ASCII.GetString (bstr);
		}

		private void parseDefType(glslParameter param, glslParameter parent, uint flags)
		{
			
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
				param.type = (glslEffectParameterType)reader.ReadUInt32();
				param.class_ = (glslEffectParameterClass)reader.ReadUInt32 ();
				param.name = glslParseString ();
				param.semantic = glslParseString ();
				param.element_count = reader.ReadUInt32();

				switch (param.class_)
				{
				case glslEffectParameterClass.Vector:
					param.columns = reader.ReadUInt32();
					param.rows = reader.ReadUInt32();
					param.bytes = 4 * param.rows * param.columns;
					break;
				
				case glslEffectParameterClass.Scalar:
					switch (param.type) {
					case glslEffectParameterType.Sampler:
					case glslEffectParameterType.Sampler1D:
					case glslEffectParameterType.Sampler2D:
					case glslEffectParameterType.Sampler3D:
					case glslEffectParameterType.SamplerCube:
						param.bytes = 0;
						break;
					default:
						throw new NotImplementedException("Type " + param.type + " not implemented for Scalar");
					}
					break;
				case glslEffectParameterClass.MatrixRows:
				case glslEffectParameterClass.MatrixColumns:
					param.rows = reader.ReadUInt32();
					param.columns = reader.ReadUInt32();
					param.bytes = 4 * param.rows * param.columns;
					break;
				
				case glslEffectParameterClass.Struct:
					param.member_count = reader.ReadUInt32 ();
					break;
					
				case glslEffectParameterClass.Object:
					switch (param.type)
					{
					case glslEffectParameterType.String:
						param.bytes = 4; //sizeof(LPCSTR)
						break;
					
					case glslEffectParameterType.PixelShader:
						param.bytes = reader.ReadUInt32(); //sizeof(LPDIRECT3DPIXELSHADER9)
						break;
					
					case glslEffectParameterType.VertexShader:
						param.bytes = reader.ReadUInt32(); //sizeof(LPDIRECT3DVERTEXSHADER9)
						break;
					
					case glslEffectParameterType.Texture:
					case glslEffectParameterType.Texture1D:
					case glslEffectParameterType.Texture2D:
					case glslEffectParameterType.Texture3D:
					case glslEffectParameterType.TextureCube:
						param.bytes = 4; //sizeof(LPDIRECT3DBASETEXTURE9)
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
				param.member_handles = new glslParameter[param.element_count];
				for (int i=0; i<param.element_count; i++) {
					param.member_handles[i] = new glslParameter();

					//we read the same typedef over and over...
					//parse_effect_typedef (effectStream.Position, param.member_handles[i], param, flags);
					param_bytes += param.member_handles[i].bytes;
				}
				param.bytes = param_bytes;
			} else if (param.member_count > 0) {
				param.member_handles = new glslParameter[param.member_count];
				for (int i=0; i<param.member_count; i++) {
					param.member_handles[i] = new glslParameter();

					//parse_effect_typedef(effectStream.Position, param.member_handles[i], null, flags);
					//effectStream.Seek(param.member_handles[i].bytes, SeekOrigin.Current);
					param.bytes += param.member_handles[i].bytes;
				}
			}
		}

		private byte[] sliceBytes(byte[] data, uint start, uint stop) {
			byte[] ret = new byte[stop-start];
			for (uint i=start; i<stop; i++) {
				ret[i-start] = data[i];
			}
			return ret;
		}

		private void parseValue(glslParameter param, byte[] data)
		{
			if (param.element_count != 0) {
				param.data = data;
				uint curOffset = 0;
				for (int i=0; i<param.element_count; i++) {
					parseValue(param.member_handles[i],
						sliceBytes(data, curOffset, curOffset+param.member_handles[i].bytes));
					curOffset += param.member_handles[i].bytes;
				}
			} else {
				switch (param.class_)
				{
				case glslEffectParameterClass.Scalar:
				case glslEffectParameterClass.Vector:
				case glslEffectParameterClass.MatrixRows:
				case glslEffectParameterClass.MatrixColumns:
					param.data = data;
					break;

				case glslEffectParameterClass.Struct:
					param.data = data;
					uint curOffset = 0;
					for (int i=0; i<param.member_count; i++) {
						parseValue(param.member_handles[i],
							sliceBytes(data, curOffset, curOffset+param.member_handles[i].bytes));
						curOffset += param.member_handles[i].bytes;
					}
					break;

				case glslEffectParameterClass.Object:

					switch (param.type)
					{
					case glslEffectParameterType.String:
					case glslEffectParameterType.Texture:
					case glslEffectParameterType.Texture1D:
					case glslEffectParameterType.Texture2D:
					case glslEffectParameterType.Texture3D:
					case glslEffectParameterType.TextureCube:
						param.data = data;
						break;
					case glslEffectParameterType.PixelShader:
						param.data = data;
						if (param.flags == 0)
							parseShader(param);
						break;
					case glslEffectParameterType.VertexShader:
						param.data = data;
						if (param.flags == 0)
							parseShader(param);
						break;
					
					case glslEffectParameterType.Sampler:
					case glslEffectParameterType.Sampler1D:
					case glslEffectParameterType.Sampler2D:
					case glslEffectParameterType.Sampler3D:
					case glslEffectParameterType.SamplerCube:
						//param.data = parse_sampler();
						break;
					}
					
					break;
				}
			}

		}

		private void parseInitValue(glslParameter param)
		{
			byte[] data = reader.ReadBytes((int)param.bytes);
			parseValue(param, data);
		}

		private glslParameter parse_effect_annotation()
		{
			glslParameter ret = new glslParameter();
			ret.flags = PARAMETER_ANNOTATION;
			ret.data = glslParseString();
			return ret;
		}
		
		private glslParameter parseEffectParameter()
		{
			glslParameter param = new glslParameter();
			param.flags = reader.ReadUInt32();
			param.annotation_count = reader.ReadUInt32();

			parseDefType(param, null, param.flags);

			parseInitValue(param);

			if (param.annotation_count > 0) {
				param.annotation_handles = new glslParameter[param.annotation_count];
				for (int i=0; i<param.annotation_count; i++) {
					param.annotation_handles[i] = parse_effect_annotation();
				}
			}

			return param;
		}
		
		private glsl_state parse_state()
		{
			glsl_state ret = new glsl_state();
			ret.parameter = new glslParameter();

			ret.type = STATE_TYPE.CONSTANT;
			ret.operation = state_table[reader.ReadUInt32 ()];
			ret.index = reader.ReadUInt32 ();

			ret.parameter.flags = reader.ReadUInt32();
			ret.parameter.annotation_count = reader.ReadUInt32();

			//long typedefOffset = effectReader.ReadInt32 ();
			parseDefType(ret.parameter, null, 0);

			//long valueOffset = effectReader.ReadInt32 ();
			parseInitValue(ret.parameter);
			if (ret.operation.class_ == STATE_CLASS.PIXELSHADER ||
			    ret.operation.class_ == STATE_CLASS.VERTEXSHADER) {
				var shaderName = (byte[])ret.parameter.data;
				var shaderObject = Encoding.ASCII.GetString(shaderName);
				foreach (glslParameter pam in objects) {
					if (pam.name.Equals(shaderObject)) {
						ret.parameter.data = pam.data;
						break;
					}
				}
				//string shaderObject = new string((byte[])ret.data);
			}
//			if (ret.operation.class_ == STATE_CLASS.RENDERSTATE) {
//				//parse the render parameter
//				switch (ret.operation.op) {
//				case (uint)D3DRENDERSTATETYPE.STENCILENABLE:
//				case (uint)D3DRENDERSTATETYPE.ALPHABLENDENABLE:
//				case (uint)D3DRENDERSTATETYPE.SCISSORTESTENABLE:
//					ret.parameter.data = BitConverter.ToInt32 ((byte[])ret.parameter.data, 0) != 0;
//					break;
//				case (uint)D3DRENDERSTATETYPE.COLORWRITEENABLE:
//					ret.parameter.data = (ColorWriteChannels)BitConverter.ToInt32 ((byte[])ret.parameter.data, 0);
//					break;
//				case (uint)D3DRENDERSTATETYPE.BLENDOP:
//					switch (BitConverter.ToInt32((byte[])ret.parameter.data, 0)) {
//					case 1: ret.parameter.data = BlendFunction.Add; break;
//					case 2: ret.parameter.data = BlendFunction.Subtract; break;
//					case 3: ret.parameter.data = BlendFunction.ReverseSubtract; break;
//					case 4: ret.parameter.data = BlendFunction.Min; break;
//					case 5: ret.parameter.data = BlendFunction.Max; break;
//					default:
//						throw new NotSupportedException();
//					}
//					break;
//				case (uint)D3DRENDERSTATETYPE.SRCBLEND:
//				case (uint)D3DRENDERSTATETYPE.DESTBLEND:
//					switch (BitConverter.ToInt32((byte[])ret.parameter.data, 0)) {
//					case 1: ret.parameter.data = Blend.Zero; break;
//					case 2: ret.parameter.data = Blend.One; break;
//					case 3: ret.parameter.data = Blend.SourceColor; break;
//					case 4: ret.parameter.data = Blend.InverseSourceColor; break;
//					case 5: ret.parameter.data = Blend.SourceAlpha; break;
//					case 6: ret.parameter.data = Blend.InverseSourceAlpha; break;
//					case 7: ret.parameter.data = Blend.DestinationAlpha; break;
//					case 8: ret.parameter.data = Blend.InverseDestinationAlpha; break;
//					case 9: ret.parameter.data = Blend.DestinationColor; break;
//					case 10: ret.parameter.data = Blend.InverseDestinationColor; break;
//					case 11: ret.parameter.data = Blend.SourceAlphaSaturation; break;
//					case 14: ret.parameter.data = Blend.BlendFactor; break;
//					case 15: ret.parameter.data = Blend.InverseDestinationAlpha; break;
//					default:
//						throw new NotSupportedException();
//					}
//					break;
//				case (uint)D3DRENDERSTATETYPE.CULLMODE:
//					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) {
//					case 1: ret.parameter.data = CullMode.None; break;
//					case 2: ret.parameter.data = CullMode.CullClockwiseFace; break;
//					case 3: ret.parameter.data = CullMode.CullCounterClockwiseFace; break;
//					default:
//						throw new NotSupportedException();
//					}
//					break;
//				case (uint)D3DRENDERSTATETYPE.STENCILFUNC:
//					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) {
//					case 1: ret.parameter.data = CompareFunction.Never; break;
//					case 2: ret.parameter.data = CompareFunction.Less; break;
//					case 3: ret.parameter.data = CompareFunction.Equal; break;
//					case 4: ret.parameter.data = CompareFunction.LessEqual; break;
//					case 5: ret.parameter.data = CompareFunction.Greater; break;
//					case 6: ret.parameter.data = CompareFunction.NotEqual; break;
//					case 7: ret.parameter.data = CompareFunction.GreaterEqual; break;
//					case 8: ret.parameter.data = CompareFunction.Always; break;
//					default:
//						throw new NotSupportedException();
//					}
//					break;
//				case (uint)D3DRENDERSTATETYPE.STENCILFAIL:
//				case (uint)D3DRENDERSTATETYPE.STENCILPASS:
//					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) {
//					case 1: ret.parameter.data = StencilOperation.Keep; break;
//					case 2: ret.parameter.data = StencilOperation.Zero; break;
//					case 3: ret.parameter.data = StencilOperation.Replace; break;
//					case 4: ret.parameter.data = StencilOperation.IncrementSaturation; break;
//					case 5: ret.parameter.data = StencilOperation.DecrementSaturation; break;
//					case 6: ret.parameter.data = StencilOperation.Invert; break;
//					case 7: ret.parameter.data = StencilOperation.Increment; break;
//					case 8: ret.parameter.data = StencilOperation.Decrement; break;
//					default:
//						throw new NotSupportedException();
//					}
//					break;
//				case (uint)D3DRENDERSTATETYPE.STENCILREF:
//					ret.parameter.data = BitConverter.ToInt32 ((byte[])ret.parameter.data, 0);
//					break;
//				default:
//					throw new NotImplementedException();
//				}
//			}
//			


			return ret;
		}

		private glslPass parse_effect_pass()
		{
			glslPass pass = new glslPass ();
			pass.name = glslParseString();
			pass.annotation_count = reader.ReadUInt32();
			pass.state_count = reader.ReadUInt32();

			if (pass.annotation_count > 0) {
				pass.annotation_handles = new glslParameter[pass.annotation_count];
				for (int i=0; i<pass.annotation_count; i++) {
					pass.annotation_handles[i] = parse_effect_annotation();
				}
			}

			pass.states = new glsl_state[pass.state_count];
			for (int i=0; i<pass.state_count; i++) {
				pass.states[i] = parse_state();
			}

			return pass;
		}
		
		private glslTechnique parse_effect_technique()
		{
			glslTechnique tech = new glslTechnique ();
			tech.name = glslParseString ();

			tech.annotation_count = reader.ReadUInt32 ();
			tech.pass_count = reader.ReadUInt32();

			if (tech.annotation_count > 0) {
				tech.annotation_handles = new glslParameter[tech.annotation_count];
				for (int i=0; i<tech.annotation_count; i++) {
					tech.annotation_handles[i] = parse_effect_annotation();
				}
			}

			int passSize = reader.ReadInt32 ();  // not used right now
			tech.pass_handles = new glslPass[tech.pass_count];

			for (int j = 0; j < tech.pass_count; j++) {
				tech.pass_handles [j] = parse_effect_pass();
			}

			return tech;
		}

//		private void glslParseVariables (BinaryReader reader, int varCount, List<EffectParameter> paramList, Dictionary<string, EffectParameter> paramDict, List<ShaderProg> shaderFuncs, List<ShaderProg> shaderProgs)
//		{
//
//			var paramCount = 0;
//
//			// read through variable objects
//			for (int i = 0; i < varCount; i++) {
//				// get type
//				glslEffectParameterType oType = (glslEffectParameterType)reader.ReadByte ();
//
//				string varName;
//				string shaderCode;
//				int j;
//				//glslVectorType varVecType;
//				int codeLength;
//
//				switch (oType) {
//				case glslEffectParameterType.Bool:
//				case glslEffectParameterType.Int32:
//				case glslEffectParameterType.Single:
//				case glslEffectParameterType.Sampler:
//				case glslEffectParameterType.Sampler1D:
//				case glslEffectParameterType.Sampler2D:
//				case glslEffectParameterType.Sampler3D:
//				case glslEffectParameterType.Texture:
//				case glslEffectParameterType.Texture1D:
//				case glslEffectParameterType.Texture2D:
//				case glslEffectParameterType.Texture3D:
//				case glslEffectParameterType.TextureCube:
//					objects[i] = parseEffectParameter();
//					parameter_handles[paramCount++] = objects[i];
//
//					EffectParameterClass paramClass;
//					if (varVecType > VectorType.Scalar)
//					if (varVecType > VectorType.Vector4)
//						paramClass = EffectParameterClass.Matrix; // change this to whatever
//					else
//						paramClass = EffectParameterClass.Vector;
//                        else if (type == OpenGLEffectParameterType.Struct)
//                            paramClass = EffectParameterClass.Struct;
//					else
//						paramClass = EffectParameterClass.Scalar;
//					type = (EffectParameterType)oType;
//					//param = new EffectParameter (this, varName, paramClass, type);
//
//                        if (type == EffectParameterType.Struct)
//                        {
//                            List<EffectParameter> memberList = new List<EffectParameter>();
//                            Dictionary<string, EffectParameter> memberDict = new Dictionary<string, EffectParameter>();
//
//                            int memberCount = (int)reader.ReadByte();
//                            glslParseVariables(reader, memberCount, memberList, memberDict, null, null);
//
//                            EffectParameterCollection memberCollection = new EffectParameterCollection(this, memberCount, memberList, memberDict);
//                            param.StructureMembers = memberCollection;
//                        }
//
					//paramList.Add (param);
					//paramDict [varName] = param;
//					break;
//				case glslEffectParameterType.PixelShader:
//				case glslEffectParameterType.VertexShader:
//					shaderProgs.Add (parseShader(oType));
//					break;
//                    case EffectParameterType.ShaderFunc:
//                        varName = ReadManifestString(reader);
//                        varVecType = (VectorType)reader.ReadByte();
//                        codeLength = reader.ReadInt32();
//                        shaderCode = Encoding.ASCII.GetString(reader.ReadBytes(codeLength), 0, codeLength);
//
//                        ShaderProg func = new ShaderProg();
//                        func.shaderName = varName;
//                        func.shaderType = type;
//                        func.shaderLength = codeLength;
//                        func.shaderCode = shaderCode;
//
//                        shaderFuncs.Add(func);
//                        break;
//				default:
//					throw new Exception (); // TODO better exception, currently unsupported type or invalid
//				}
//			}
//
//		}

		private void parseShader(glslParameter param)
		{
			ShaderProg prog = new ShaderProg ();

			prog.shaderName = param.name;
			var attInfo = new VertexAttributeInfo[0];
			var samplIndices = new SamplerIndexInfo[0];
			byte[] shaderData = (byte[])param.data;
			// parse out attributes for vertex shaders
			if (param.type == glslEffectParameterType.VertexShader) {
				MemoryStream vertexStream = new MemoryStream(shaderData);
				BinaryReader vertexReader = new BinaryReader(vertexStream);
				uint numAttributes = vertexReader.ReadUInt32();

				attInfo = new VertexAttributeInfo[numAttributes];
				for (int x = 0; x < numAttributes; x++) {
					attInfo[x] = new VertexAttributeInfo();
					attInfo[x].Type = (VertexAttributeType)vertexReader.ReadByte();
					attInfo[x].Name = glslParseString(vertexReader);
				}
				param.bytes = vertexReader.ReadUInt32();
				param.data = sliceBytes(shaderData, (uint)vertexStream.Position, (uint)shaderData.Length);
				param.bytes = (uint)((byte[])param.data).Length;
				vertexStream.Close();
				vertexReader.Close();
			}

			// parse out attributes for vertex shaders
			if (param.type == glslEffectParameterType.PixelShader) {
				MemoryStream fragStream = new MemoryStream(shaderData);
				BinaryReader fragReader = new BinaryReader(fragStream);
				uint numIndices = fragReader.ReadUInt32();

				samplIndices = new SamplerIndexInfo[numIndices];
				for (int x = 0; x < numIndices; x++) {
					samplIndices[x] = new SamplerIndexInfo();
					samplIndices[x].Index = fragReader.ReadByte();
					samplIndices[x].Name = glslParseString(fragReader);
				}
				param.bytes = fragReader.ReadUInt32();
				param.data = sliceBytes(shaderData, (uint)fragStream.Position, (uint)shaderData.Length);
				param.bytes = (uint)((byte[])param.data).Length;
				fragStream.Close();
				fragReader.Close();
			}

			prog.shaderLength = (int)param.bytes;
			byte[] code = (byte[])param.data;
			prog.shaderCode = Encoding.ASCII.GetString(code);
			prog.shaderType = param.type;
			GLSLShader shader = new GLSLShader(prog);
			// set the vector attributes
			shader.attributes = attInfo;
			// set the sampler indices
			shader.samplerIndices = samplIndices;

			prog.glslShaderObject = shader;
			prog.shaderId = shader.shaderHandle;
			param.data = prog;

		}

	}
}

