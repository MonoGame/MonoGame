using System;
using System.Runtime.InteropServices;

namespace MonoGame.Effect
{
	internal class MojoShader
    {
#if OLD_CONTENT_PROCESSORS
        const string mojoshader_dll = "libmojoshader_32.dll";
#else
        const string mojoshader_dll = "libmojoshader_64.dll";
#endif

		public partial class NativeConstants {
		    
		    /// _INCL_MOJOSHADER_H_ -> 
		    /// Error generating expression: Value cannot be null.
		    ///Parameter name: node
		    public const string _INCL_MOJOSHADER_H_ = "";
		    
		    /// _INCL_MOJOSHADER_VERSION_H_ -> 
		    /// Error generating expression: Value cannot be null.
		    ///Parameter name: node
		    public const string _INCL_MOJOSHADER_VERSION_H_ = "";
		    
		    /// MOJOSHADER_VERSION -> 1111
		    public const int MOJOSHADER_VERSION = 1111;
		    
		    /// MOJOSHADER_CHANGESET -> "hg-1111:91a6af79b5e4"
		    public const string MOJOSHADER_CHANGESET = "hg-1111:91a6af79b5e4";
		    
		    /// MOJOSHADER_POSITION_NONE -> (-3)
		    public const int MOJOSHADER_POSITION_NONE = -3;
		    
		    /// MOJOSHADER_POSITION_BEFORE -> (-2)
		    public const int MOJOSHADER_POSITION_BEFORE = -2;
		    
		    /// MOJOSHADER_POSITION_AFTER -> (-1)
		    public const int MOJOSHADER_POSITION_AFTER = -1;
		    
		    /// MOJOSHADER_PROFILE_D3D -> "d3d"
		    public const string MOJOSHADER_PROFILE_D3D = "d3d";
		    
		    /// MOJOSHADER_PROFILE_BYTECODE -> "bytecode"
		    public const string MOJOSHADER_PROFILE_BYTECODE = "bytecode";
		    
		    /// MOJOSHADER_PROFILE_GLSL -> "glsl"
		    public const string MOJOSHADER_PROFILE_GLSL = "glsl";
		    
		    /// MOJOSHADER_PROFILE_GLSL120 -> "glsl120"
		    public const string MOJOSHADER_PROFILE_GLSL120 = "glsl120";
		    
		    /// MOJOSHADER_PROFILE_ARB1 -> "arb1"
		    public const string MOJOSHADER_PROFILE_ARB1 = "arb1";
		    
		    /// MOJOSHADER_PROFILE_NV2 -> "nv2"
		    public const string MOJOSHADER_PROFILE_NV2 = "nv2";
		    
		    /// MOJOSHADER_PROFILE_NV3 -> "nv3"
		    public const string MOJOSHADER_PROFILE_NV3 = "nv3";
		    
		    /// MOJOSHADER_PROFILE_NV4 -> "nv4"
		    public const string MOJOSHADER_PROFILE_NV4 = "nv4";
		    
		    /// MOJOSHADER_SRC_PROFILE_HLSL_VS_1_1 -> "hlsl_vs_1_1"
		    public const string MOJOSHADER_SRC_PROFILE_HLSL_VS_1_1 = "hlsl_vs_1_1";
		    
		    /// MOJOSHADER_SRC_PROFILE_HLSL_VS_2_0 -> "hlsl_vs_2_0"
		    public const string MOJOSHADER_SRC_PROFILE_HLSL_VS_2_0 = "hlsl_vs_2_0";
		    
		    /// MOJOSHADER_SRC_PROFILE_HLSL_VS_3_0 -> "hlsl_vs_3_0"
		    public const string MOJOSHADER_SRC_PROFILE_HLSL_VS_3_0 = "hlsl_vs_3_0";
		    
		    /// MOJOSHADER_SRC_PROFILE_HLSL_PS_1_1 -> "hlsl_ps_1_1"
		    public const string MOJOSHADER_SRC_PROFILE_HLSL_PS_1_1 = "hlsl_ps_1_1";
		    
		    /// MOJOSHADER_SRC_PROFILE_HLSL_PS_1_2 -> "hlsl_ps_1_2"
		    public const string MOJOSHADER_SRC_PROFILE_HLSL_PS_1_2 = "hlsl_ps_1_2";
		    
		    /// MOJOSHADER_SRC_PROFILE_HLSL_PS_1_3 -> "hlsl_ps_1_3"
		    public const string MOJOSHADER_SRC_PROFILE_HLSL_PS_1_3 = "hlsl_ps_1_3";
		    
		    /// MOJOSHADER_SRC_PROFILE_HLSL_PS_1_4 -> "hlsl_ps_1_4"
		    public const string MOJOSHADER_SRC_PROFILE_HLSL_PS_1_4 = "hlsl_ps_1_4";
		    
		    /// MOJOSHADER_SRC_PROFILE_HLSL_PS_2_0 -> "hlsl_ps_2_0"
		    public const string MOJOSHADER_SRC_PROFILE_HLSL_PS_2_0 = "hlsl_ps_2_0";
		    
		    /// MOJOSHADER_SRC_PROFILE_HLSL_PS_3_0 -> "hlsl_ps_3_0"
		    public const string MOJOSHADER_SRC_PROFILE_HLSL_PS_3_0 = "hlsl_ps_3_0";
		    
		    // MOJOSHADER_AST_DATATYPE_CONST -> (1 << 31)
		    public const int MOJOSHADER_AST_DATATYPE_CONST = (1) << (31);
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_uniform {
		    
		    /// MOJOSHADER_uniformType->Anonymous_cf91fd71_65e4_4c31_a6d5_9488d7f3d32a
		    public MOJOSHADER_uniformType type;
		    
		    /// int
		    public int index;
		    
		    /// int
		    public int array_count;
		    
		    /// int
		    public int constant;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string name;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_constant {
		    
		    /// MOJOSHADER_uniformType->Anonymous_cf91fd71_65e4_4c31_a6d5_9488d7f3d32a
		    public MOJOSHADER_uniformType type;
		    
		    /// int
		    public int index;
		    
		    /// Anonymous_5371dd6a_e42a_47c1_91d1_a2af9a8283be
		    public Anonymous_5371dd6a_e42a_47c1_91d1_a2af9a8283be value;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_sampler {
		    
		    /// MOJOSHADER_samplerType->Anonymous_a752a39b_b479_42b0_9502_e39ba7d86100
		    public MOJOSHADER_samplerType type;
		    
		    /// int
		    public int index;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string name;
		    
		    /// int
		    public int texbem;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_samplerMap {
		    
		    /// int
		    public int index;
		    
		    /// MOJOSHADER_samplerType->Anonymous_a752a39b_b479_42b0_9502_e39ba7d86100
		    public MOJOSHADER_samplerType type;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_attribute {
		    
		    /// MOJOSHADER_usage->Anonymous_9c01433d_7bb5_4c50_bf77_e65cef0661b5
		    public MOJOSHADER_usage usage;
		    
		    /// int
		    public int index;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string name;
		}
		
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
		public struct MOJOSHADER_swizzle {
		    
		    /// MOJOSHADER_usage->Anonymous_9c01433d_7bb5_4c50_bf77_e65cef0661b5
		    public MOJOSHADER_usage usage;
		    
		    /// unsigned int
		    public uint index;
		    
		    /// unsigned char[4]
		    [MarshalAs(UnmanagedType.ByValTStr, SizeConst=4)]
		    public string swizzles;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_symbolTypeInfo {
		    
		    /// MOJOSHADER_symbolClass->Anonymous_681c4b26_94f7_4142_a8e9_b970fe0b60df
		    public MOJOSHADER_symbolClass parameter_class;
		    
		    /// MOJOSHADER_symbolType->Anonymous_d8534f21_7f44_465d_8843_40a435dbb54a
		    public MOJOSHADER_symbolType parameter_type;
		    
		    /// unsigned int
		    public uint rows;
		    
		    /// unsigned int
		    public uint columns;
		    
		    /// unsigned int
		    public uint elements;
		    
		    /// unsigned int
		    public uint member_count;
		    
		    /// MOJOSHADER_symbolStructMember*
		    public IntPtr members;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_symbolStructMember {
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string name;
		    
		    /// MOJOSHADER_symbolTypeInfo
		    public MOJOSHADER_symbolTypeInfo info;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_symbol {
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string name;
		    
		    /// MOJOSHADER_symbolRegisterSet->Anonymous_9ff7ac54_131c_43b0_a295_9830d24ac76b
		    public MOJOSHADER_symbolRegisterSet register_set;
		    
		    /// unsigned int
		    public uint register_index;
		    
		    /// unsigned int
		    public uint register_count;
		    
		    /// MOJOSHADER_symbolTypeInfo
		    public MOJOSHADER_symbolTypeInfo info;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_error {
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string error;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string filename;
		    
		    /// int
		    public int error_position;
		}
		
		public enum MOJOSHADER_preshaderOpcode {
		    
		    MOJOSHADER_PRESHADEROP_NOP,
		    
		    MOJOSHADER_PRESHADEROP_MOV,
		    
		    MOJOSHADER_PRESHADEROP_NEG,
		    
		    MOJOSHADER_PRESHADEROP_RCP,
		    
		    MOJOSHADER_PRESHADEROP_FRC,
		    
		    MOJOSHADER_PRESHADEROP_EXP,
		    
		    MOJOSHADER_PRESHADEROP_LOG,
		    
		    MOJOSHADER_PRESHADEROP_RSQ,
		    
		    MOJOSHADER_PRESHADEROP_SIN,
		    
		    MOJOSHADER_PRESHADEROP_COS,
		    
		    MOJOSHADER_PRESHADEROP_ASIN,
		    
		    MOJOSHADER_PRESHADEROP_ACOS,
		    
		    MOJOSHADER_PRESHADEROP_ATAN,
		    
		    MOJOSHADER_PRESHADEROP_MIN,
		    
		    MOJOSHADER_PRESHADEROP_MAX,
		    
		    MOJOSHADER_PRESHADEROP_LT,
		    
		    MOJOSHADER_PRESHADEROP_GE,
		    
		    MOJOSHADER_PRESHADEROP_ADD,
		    
		    MOJOSHADER_PRESHADEROP_MUL,
		    
		    MOJOSHADER_PRESHADEROP_ATAN2,
		    
		    MOJOSHADER_PRESHADEROP_DIV,
		    
		    MOJOSHADER_PRESHADEROP_CMP,
		    
		    MOJOSHADER_PRESHADEROP_MOVC,
		    
		    MOJOSHADER_PRESHADEROP_DOT,
		    
		    MOJOSHADER_PRESHADEROP_NOISE,
		    
		    MOJOSHADER_PRESHADEROP_SCALAR_OPS,
		    
		    /// MOJOSHADER_PRESHADEROP_MIN_SCALAR -> MOJOSHADER_PRESHADEROP_SCALAR_OPS
		    MOJOSHADER_PRESHADEROP_MIN_SCALAR = MOJOSHADER_preshaderOpcode.MOJOSHADER_PRESHADEROP_SCALAR_OPS,
		    
		    MOJOSHADER_PRESHADEROP_MAX_SCALAR,
		    
		    MOJOSHADER_PRESHADEROP_LT_SCALAR,
		    
		    MOJOSHADER_PRESHADEROP_GE_SCALAR,
		    
		    MOJOSHADER_PRESHADEROP_ADD_SCALAR,
		    
		    MOJOSHADER_PRESHADEROP_MUL_SCALAR,
		    
		    MOJOSHADER_PRESHADEROP_ATAN2_SCALAR,
		    
		    MOJOSHADER_PRESHADEROP_DIV_SCALAR,
		    
		    MOJOSHADER_PRESHADEROP_DOT_SCALAR,
		    
		    MOJOSHADER_PRESHADEROP_NOISE_SCALAR,
		}
		
		public enum MOJOSHADER_preshaderOperandType {
		    
		    /// MOJOSHADER_PRESHADEROPERAND_LITERAL -> 1
		    MOJOSHADER_PRESHADEROPERAND_LITERAL = 1,
		    
		    /// MOJOSHADER_PRESHADEROPERAND_INPUT -> 2
		    MOJOSHADER_PRESHADEROPERAND_INPUT = 2,
		    
		    /// MOJOSHADER_PRESHADEROPERAND_OUTPUT -> 4
		    MOJOSHADER_PRESHADEROPERAND_OUTPUT = 4,
		    
		    /// MOJOSHADER_PRESHADEROPERAND_TEMP -> 7
		    MOJOSHADER_PRESHADEROPERAND_TEMP = 7,
		    
		    /// MOJOSHADER_PRESHADEROPERAND_UNKN -> 0xff
		    MOJOSHADER_PRESHADEROPERAND_UNKN = 255,
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_preshaderOperand {
		    
		    /// MOJOSHADER_preshaderOperandType
		    public MOJOSHADER_preshaderOperandType type;
		    
		    /// unsigned int
		    public uint index;
		    
		    /// int
		    public int indexingType;
		    
		    /// unsigned int
		    public uint indexingIndex;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_preshaderInstruction {
		    
		    /// MOJOSHADER_preshaderOpcode
		    public MOJOSHADER_preshaderOpcode opcode;
		    
		    /// unsigned int
		    public uint element_count;
		    
		    /// unsigned int
		    public uint operand_count;
		    
		    /// MOJOSHADER_preshaderOperand[4]
		    [MarshalAs(UnmanagedType.ByValArray, SizeConst=4, ArraySubType=UnmanagedType.Struct)]
		    public MOJOSHADER_preshaderOperand[] operands;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_preshader {
		    
		    /// unsigned int
		    public uint literal_count;
		    
		    /// double*
		    public IntPtr literals;
		    
		    /// unsigned int
		    public uint temp_count;
		    
		    /// unsigned int
		    public uint symbol_count;
		    
		    /// MOJOSHADER_symbol*
		    public IntPtr symbols;
		    
		    /// unsigned int
		    public uint instruction_count;
		    
		    /// MOJOSHADER_preshaderInstruction*
		    public IntPtr instructions;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_parseData {
		    
		    /// int
		    public int error_count;
		    
		    /// MOJOSHADER_error*
		    public IntPtr errors;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string profile;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string output;
		    
		    /// int
		    public int output_len;
		    
		    /// int
		    public int instruction_count;
		    
		    /// MOJOSHADER_shaderType->Anonymous_96517ad6_cc69_4542_8537_054e63919d54
		    public MOJOSHADER_shaderType shader_type;
		    
		    /// int
		    public int major_ver;
		    
		    /// int
		    public int minor_ver;
		    
		    /// int
		    public int uniform_count;
		    
		    /// MOJOSHADER_uniform*
		    public IntPtr uniforms;
		    
		    /// int
		    public int constant_count;
		    
		    /// MOJOSHADER_constant*
		    public IntPtr constants;
		    
		    /// int
		    public int sampler_count;
		    
		    /// MOJOSHADER_sampler*
		    public IntPtr samplers;
		    
		    /// int
		    public int attribute_count;
		    
		    /// MOJOSHADER_attribute*
		    public IntPtr attributes;
		    
		    /// int
		    public int output_count;
		    
		    /// MOJOSHADER_attribute*
		    public IntPtr outputs;
		    
		    /// int
		    public int swizzle_count;
		    
		    /// MOJOSHADER_swizzle*
		    public IntPtr swizzles;
		    
		    /// int
		    public int symbol_count;
		    
		    /// MOJOSHADER_symbol*
		    public IntPtr symbols;
		    
		    /// MOJOSHADER_preshader*
		    public IntPtr preshader;
		    
		    /// MOJOSHADER_malloc
		    public IntPtr malloc;
		    
		    /// MOJOSHADER_free
		    public IntPtr free;
		    
		    /// void*
		    public IntPtr malloc_data;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_effectParam {
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string name;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string semantic;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_effectState {
		    
		    /// unsigned int
		    public uint type;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_effectPass {
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string name;
		    
		    /// unsigned int
		    public uint state_count;
		    
		    /// MOJOSHADER_effectState*
		    public IntPtr states;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_effectTechnique {
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string name;
		    
		    /// unsigned int
		    public uint pass_count;
		    
		    /// MOJOSHADER_effectPass*
		    public IntPtr passes;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_effectTexture {
		    
		    /// unsigned int
		    public uint param;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string name;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_effectShader {
		    
		    /// unsigned int
		    public uint technique;
		    
		    /// unsigned int
		    public uint pass;
		    
		    /// MOJOSHADER_parseData*
		    public IntPtr shader;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_effect {
		    
		    /// int
		    public int error_count;
		    
		    /// MOJOSHADER_error*
		    public IntPtr errors;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string profile;
		    
		    /// int
		    public int param_count;
		    
		    /// MOJOSHADER_effectParam*
		    public IntPtr @params;
		    
		    /// int
		    public int technique_count;
		    
		    /// MOJOSHADER_effectTechnique*
		    public IntPtr techniques;
		    
		    /// int
		    public int texture_count;
		    
		    /// MOJOSHADER_effectTexture*
		    public IntPtr textures;
		    
		    /// int
		    public int shader_count;
		    
		    /// MOJOSHADER_effectShader*
		    public IntPtr shaders;
		    
		    /// MOJOSHADER_malloc
		    public IntPtr malloc;
		    
		    /// MOJOSHADER_free
		    public IntPtr free;
		    
		    /// void*
		    public IntPtr malloc_data;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_preprocessorDefine {
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string identifier;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string definition;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_preprocessData {
		    
		    /// int
		    public int error_count;
		    
		    /// MOJOSHADER_error*
		    public IntPtr errors;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string output;
		    
		    /// int
		    public int output_len;
		    
		    /// MOJOSHADER_malloc
		    public IntPtr malloc;
		    
		    /// MOJOSHADER_free
		    public IntPtr free;
		    
		    /// void*
		    public IntPtr malloc_data;
		}
		
		/// Return Type: int
		///inctype: MOJOSHADER_includeType->Anonymous_f1eed39d_7d1b_46d4_972e_a3229d15c26e
		///fname: char*
		///parent: char*
		///outdata: char**
		///outbytes: unsigned int*
		///m: MOJOSHADER_malloc
		///f: MOJOSHADER_free
		///d: void*
		public delegate int MOJOSHADER_includeOpen(MOJOSHADER_includeType inctype, [In()] [MarshalAs(UnmanagedType.LPStr)] string fname, [In()] [MarshalAs(UnmanagedType.LPStr)] string parent, ref IntPtr outdata, ref uint outbytes, IntPtr m, IntPtr f, IntPtr d);
		
		/// Return Type: void
		///data: char*
		///m: MOJOSHADER_malloc
		///f: MOJOSHADER_free
		///d: void*
		public delegate void MOJOSHADER_includeClose([In()] [MarshalAs(UnmanagedType.LPStr)] string data, IntPtr m, IntPtr f, IntPtr d);
		
		public enum MOJOSHADER_astDataTypeType {
		    
		    MOJOSHADER_AST_DATATYPE_NONE,
		    
		    MOJOSHADER_AST_DATATYPE_BOOL,
		    
		    MOJOSHADER_AST_DATATYPE_INT,
		    
		    MOJOSHADER_AST_DATATYPE_UINT,
		    
		    MOJOSHADER_AST_DATATYPE_FLOAT,
		    
		    MOJOSHADER_AST_DATATYPE_FLOAT_SNORM,
		    
		    MOJOSHADER_AST_DATATYPE_FLOAT_UNORM,
		    
		    MOJOSHADER_AST_DATATYPE_HALF,
		    
		    MOJOSHADER_AST_DATATYPE_DOUBLE,
		    
		    MOJOSHADER_AST_DATATYPE_STRING,
		    
		    MOJOSHADER_AST_DATATYPE_SAMPLER_1D,
		    
		    MOJOSHADER_AST_DATATYPE_SAMPLER_2D,
		    
		    MOJOSHADER_AST_DATATYPE_SAMPLER_3D,
		    
		    MOJOSHADER_AST_DATATYPE_SAMPLER_CUBE,
		    
		    MOJOSHADER_AST_DATATYPE_SAMPLER_STATE,
		    
		    MOJOSHADER_AST_DATATYPE_SAMPLER_COMPARISON_STATE,
		    
		    MOJOSHADER_AST_DATATYPE_STRUCT,
		    
		    MOJOSHADER_AST_DATATYPE_ARRAY,
		    
		    MOJOSHADER_AST_DATATYPE_VECTOR,
		    
		    MOJOSHADER_AST_DATATYPE_MATRIX,
		    
		    MOJOSHADER_AST_DATATYPE_BUFFER,
		    
		    MOJOSHADER_AST_DATATYPE_FUNCTION,
		    
		    MOJOSHADER_AST_DATATYPE_USER,
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astDataTypeStructMember {
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string identifier;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astDataTypeStruct {
		    
		    /// MOJOSHADER_astDataTypeType
		    public MOJOSHADER_astDataTypeType type;
		    
		    /// MOJOSHADER_astDataTypeStructMember*
		    public IntPtr members;
		    
		    /// int
		    public int member_count;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astDataTypeArray {
		    
		    /// MOJOSHADER_astDataTypeType
		    public MOJOSHADER_astDataTypeType type;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr @base;
		    
		    /// int
		    public int elements;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astDataTypeMatrix {
		    
		    /// MOJOSHADER_astDataTypeType
		    public MOJOSHADER_astDataTypeType type;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr @base;
		    
		    /// int
		    public int rows;
		    
		    /// int
		    public int columns;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astDataTypeBuffer {
		    
		    /// MOJOSHADER_astDataTypeType
		    public MOJOSHADER_astDataTypeType type;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr @base;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astDataTypeFunction {
		    
		    /// MOJOSHADER_astDataTypeType
		    public MOJOSHADER_astDataTypeType type;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr retval;
		    
		    /// MOJOSHADER_astDataType**
		    public IntPtr @params;
		    
		    /// int
		    public int num_params;
		    
		    /// int
		    public int intrinsic;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astDataTypeUser {
		    
		    /// MOJOSHADER_astDataTypeType
		    public MOJOSHADER_astDataTypeType type;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr details;
		    
		    /// char*
            public IntPtr name;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		public struct MOJOSHADER_astDataType {
		    
		    /// MOJOSHADER_astDataTypeType
		    [FieldOffset(0)]
		    public MOJOSHADER_astDataTypeType type;
		    
		    /// MOJOSHADER_astDataTypeArray
		    [FieldOffset(0)]
		    public MOJOSHADER_astDataTypeArray array;
		    
		    /// MOJOSHADER_astDataTypeStruct
		    [FieldOffset(0)]
		    public MOJOSHADER_astDataTypeStruct structure;
		    
		    /// MOJOSHADER_astDataTypeVector->MOJOSHADER_astDataTypeArray
		    [FieldOffset(0)]
		    public MOJOSHADER_astDataTypeArray vector;
		    
		    /// MOJOSHADER_astDataTypeMatrix
		    [FieldOffset(0)]
		    public MOJOSHADER_astDataTypeMatrix matrix;
		    
		    /// MOJOSHADER_astDataTypeBuffer
		    [FieldOffset(0)]
		    public MOJOSHADER_astDataTypeBuffer buffer;
		    
		    /// MOJOSHADER_astDataTypeUser
		    [FieldOffset(0)]
		    public MOJOSHADER_astDataTypeUser user;
		    
		    /// MOJOSHADER_astDataTypeFunction
		    [FieldOffset(0)]
		    public MOJOSHADER_astDataTypeFunction function;
		}
		
		public enum MOJOSHADER_astNodeType {
		    
		    MOJOSHADER_AST_OP_START_RANGE,
		    
		    MOJOSHADER_AST_OP_START_RANGE_UNARY,
		    
		    MOJOSHADER_AST_OP_PREINCREMENT,
		    
		    MOJOSHADER_AST_OP_PREDECREMENT,
		    
		    MOJOSHADER_AST_OP_NEGATE,
		    
		    MOJOSHADER_AST_OP_COMPLEMENT,
		    
		    MOJOSHADER_AST_OP_NOT,
		    
		    MOJOSHADER_AST_OP_POSTINCREMENT,
		    
		    MOJOSHADER_AST_OP_POSTDECREMENT,
		    
		    MOJOSHADER_AST_OP_CAST,
		    
		    MOJOSHADER_AST_OP_END_RANGE_UNARY,
		    
		    MOJOSHADER_AST_OP_START_RANGE_BINARY,
		    
		    MOJOSHADER_AST_OP_COMMA,
		    
		    MOJOSHADER_AST_OP_MULTIPLY,
		    
		    MOJOSHADER_AST_OP_DIVIDE,
		    
		    MOJOSHADER_AST_OP_MODULO,
		    
		    MOJOSHADER_AST_OP_ADD,
		    
		    MOJOSHADER_AST_OP_SUBTRACT,
		    
		    MOJOSHADER_AST_OP_LSHIFT,
		    
		    MOJOSHADER_AST_OP_RSHIFT,
		    
		    MOJOSHADER_AST_OP_LESSTHAN,
		    
		    MOJOSHADER_AST_OP_GREATERTHAN,
		    
		    MOJOSHADER_AST_OP_LESSTHANOREQUAL,
		    
		    MOJOSHADER_AST_OP_GREATERTHANOREQUAL,
		    
		    MOJOSHADER_AST_OP_EQUAL,
		    
		    MOJOSHADER_AST_OP_NOTEQUAL,
		    
		    MOJOSHADER_AST_OP_BINARYAND,
		    
		    MOJOSHADER_AST_OP_BINARYXOR,
		    
		    MOJOSHADER_AST_OP_BINARYOR,
		    
		    MOJOSHADER_AST_OP_LOGICALAND,
		    
		    MOJOSHADER_AST_OP_LOGICALOR,
		    
		    MOJOSHADER_AST_OP_ASSIGN,
		    
		    MOJOSHADER_AST_OP_MULASSIGN,
		    
		    MOJOSHADER_AST_OP_DIVASSIGN,
		    
		    MOJOSHADER_AST_OP_MODASSIGN,
		    
		    MOJOSHADER_AST_OP_ADDASSIGN,
		    
		    MOJOSHADER_AST_OP_SUBASSIGN,
		    
		    MOJOSHADER_AST_OP_LSHIFTASSIGN,
		    
		    MOJOSHADER_AST_OP_RSHIFTASSIGN,
		    
		    MOJOSHADER_AST_OP_ANDASSIGN,
		    
		    MOJOSHADER_AST_OP_XORASSIGN,
		    
		    MOJOSHADER_AST_OP_ORASSIGN,
		    
		    MOJOSHADER_AST_OP_DEREF_ARRAY,
		    
		    MOJOSHADER_AST_OP_END_RANGE_BINARY,
		    
		    MOJOSHADER_AST_OP_START_RANGE_TERNARY,
		    
		    MOJOSHADER_AST_OP_CONDITIONAL,
		    
		    MOJOSHADER_AST_OP_END_RANGE_TERNARY,
		    
		    MOJOSHADER_AST_OP_START_RANGE_DATA,
		    
		    MOJOSHADER_AST_OP_IDENTIFIER,
		    
		    MOJOSHADER_AST_OP_INT_LITERAL,
		    
		    MOJOSHADER_AST_OP_FLOAT_LITERAL,
		    
		    MOJOSHADER_AST_OP_STRING_LITERAL,
		    
		    MOJOSHADER_AST_OP_BOOLEAN_LITERAL,
		    
		    MOJOSHADER_AST_OP_END_RANGE_DATA,
		    
		    MOJOSHADER_AST_OP_START_RANGE_MISC,
		    
		    MOJOSHADER_AST_OP_DEREF_STRUCT,
		    
		    MOJOSHADER_AST_OP_CALLFUNC,
		    
		    MOJOSHADER_AST_OP_CONSTRUCTOR,
		    
		    MOJOSHADER_AST_OP_END_RANGE_MISC,
		    
		    MOJOSHADER_AST_OP_END_RANGE,
		    
		    MOJOSHADER_AST_COMPUNIT_START_RANGE,
		    
		    MOJOSHADER_AST_COMPUNIT_FUNCTION,
		    
		    MOJOSHADER_AST_COMPUNIT_TYPEDEF,
		    
		    MOJOSHADER_AST_COMPUNIT_STRUCT,
		    
		    MOJOSHADER_AST_COMPUNIT_VARIABLE,
		    
		    MOJOSHADER_AST_COMPUNIT_END_RANGE,
		    
		    MOJOSHADER_AST_STATEMENT_START_RANGE,
		    
		    MOJOSHADER_AST_STATEMENT_EMPTY,
		    
		    MOJOSHADER_AST_STATEMENT_BREAK,
		    
		    MOJOSHADER_AST_STATEMENT_CONTINUE,
		    
		    MOJOSHADER_AST_STATEMENT_DISCARD,
		    
		    MOJOSHADER_AST_STATEMENT_BLOCK,
		    
		    MOJOSHADER_AST_STATEMENT_EXPRESSION,
		    
		    MOJOSHADER_AST_STATEMENT_IF,
		    
		    MOJOSHADER_AST_STATEMENT_SWITCH,
		    
		    MOJOSHADER_AST_STATEMENT_FOR,
		    
		    MOJOSHADER_AST_STATEMENT_DO,
		    
		    MOJOSHADER_AST_STATEMENT_WHILE,
		    
		    MOJOSHADER_AST_STATEMENT_RETURN,
		    
		    MOJOSHADER_AST_STATEMENT_TYPEDEF,
		    
		    MOJOSHADER_AST_STATEMENT_STRUCT,
		    
		    MOJOSHADER_AST_STATEMENT_VARDECL,
		    
		    MOJOSHADER_AST_STATEMENT_END_RANGE,
		    
		    MOJOSHADER_AST_MISC_START_RANGE,
		    
		    MOJOSHADER_AST_FUNCTION_PARAMS,
		    
		    MOJOSHADER_AST_FUNCTION_SIGNATURE,
		    
		    MOJOSHADER_AST_SCALAR_OR_ARRAY,
		    
		    MOJOSHADER_AST_TYPEDEF,
		    
		    MOJOSHADER_AST_PACK_OFFSET,
		    
		    MOJOSHADER_AST_VARIABLE_LOWLEVEL,
		    
		    MOJOSHADER_AST_ANNOTATION,
		    
		    MOJOSHADER_AST_VARIABLE_DECLARATION,
		    
		    MOJOSHADER_AST_STRUCT_DECLARATION,
		    
		    MOJOSHADER_AST_STRUCT_MEMBER,
		    
		    MOJOSHADER_AST_SWITCH_CASE,
		    
		    MOJOSHADER_AST_ARGUMENTS,
		    
		    MOJOSHADER_AST_MISC_END_RANGE,
		    
		    MOJOSHADER_AST_END_RANGE,
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astNodeInfo {
		    
		    /// MOJOSHADER_astNodeType
		    public MOJOSHADER_astNodeType type;
		    
		    /// char*
            public IntPtr filename;
		    
		    /// unsigned int
		    public uint line;
		}
		
		public enum MOJOSHADER_astVariableAttributes {
		    
		    // MOJOSHADER_AST_VARATTR_EXTERN -> (1<<0)
		    MOJOSHADER_AST_VARATTR_EXTERN = (1) << (0),
		    
		    // MOJOSHADER_AST_VARATTR_NOINTERPOLATION -> (1<<1)
		    MOJOSHADER_AST_VARATTR_NOINTERPOLATION = (1) << (1),
		    
		    // MOJOSHADER_AST_VARATTR_SHARED -> (1<<2)
		    MOJOSHADER_AST_VARATTR_SHARED = (1) << (2),
		    
		    // MOJOSHADER_AST_VARATTR_STATIC -> (1<<3)
		    MOJOSHADER_AST_VARATTR_STATIC = (1) << (3),
		    
		    // MOJOSHADER_AST_VARATTR_UNIFORM -> (1<<4)
		    MOJOSHADER_AST_VARATTR_UNIFORM = (1) << (4),
		    
		    // MOJOSHADER_AST_VARATTR_VOLATILE -> (1<<5)
		    MOJOSHADER_AST_VARATTR_VOLATILE = (1) << (5),
		    
		    // MOJOSHADER_AST_VARATTR_CONST -> (1<<6)
		    MOJOSHADER_AST_VARATTR_CONST = (1) << (6),
		    
		    // MOJOSHADER_AST_VARATTR_ROWMAJOR -> (1<<7)
		    MOJOSHADER_AST_VARATTR_ROWMAJOR = (1) << (7),
		    
		    // MOJOSHADER_AST_VARATTR_COLUMNMAJOR -> (1<<8)
		    MOJOSHADER_AST_VARATTR_COLUMNMAJOR = (1) << (8),
		}
		
		public enum MOJOSHADER_astIfAttributes {
		    
		    MOJOSHADER_AST_IFATTR_NONE,
		    
		    MOJOSHADER_AST_IFATTR_BRANCH,
		    
		    MOJOSHADER_AST_IFATTR_FLATTEN,
		    
		    MOJOSHADER_AST_IFATTR_IFALL,
		    
		    MOJOSHADER_AST_IFATTR_IFANY,
		    
		    MOJOSHADER_AST_IFATTR_PREDICATE,
		    
		    MOJOSHADER_AST_IFATTR_PREDICATEBLOCK,
		}
		
		public enum MOJOSHADER_astSwitchAttributes {
		    
		    MOJOSHADER_AST_SWITCHATTR_NONE,
		    
		    MOJOSHADER_AST_SWITCHATTR_FLATTEN,
		    
		    MOJOSHADER_AST_SWITCHATTR_BRANCH,
		    
		    MOJOSHADER_AST_SWITCHATTR_FORCECASE,
		    
		    MOJOSHADER_AST_SWITCHATTR_CALL,
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astGeneric {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpression {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astArguments {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr argument;
		    
		    /// MOJOSHADER_astArguments*
		    public IntPtr next;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionUnary {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr operand;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionBinary {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr left;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr right;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionTernary {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr left;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr center;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr right;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionIdentifier {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// char*
            public IntPtr identifier;
		    
		    /// int
		    public int index;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionIntLiteral {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// int
		    public int value;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionFloatLiteral {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// double
		    public double value;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionStringLiteral {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// char*
            public IntPtr @string;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionBooleanLiteral {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// int
		    public int value;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionConstructor {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// MOJOSHADER_astArguments*
		    public IntPtr args;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionDerefStruct {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr identifier;
		    
		    /// char*
            public IntPtr member;
		    
		    /// int
		    public int isswizzle;
		    
		    /// int
		    public int member_index;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionCallFunction {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// MOJOSHADER_astExpressionIdentifier*
		    public IntPtr identifier;
		    
		    /// MOJOSHADER_astArguments*
		    public IntPtr args;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionCast {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr operand;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astCompilationUnit {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astCompilationUnit*
		    public IntPtr next;
		}
		
		public enum MOJOSHADER_astFunctionStorageClass {
		    
		    MOJOSHADER_AST_FNSTORECLS_NONE,
		    
		    MOJOSHADER_AST_FNSTORECLS_INLINE,
		}
		
		public enum MOJOSHADER_astInputModifier {
		    
		    MOJOSHADER_AST_INPUTMOD_NONE,
		    
		    MOJOSHADER_AST_INPUTMOD_IN,
		    
		    MOJOSHADER_AST_INPUTMOD_OUT,
		    
		    MOJOSHADER_AST_INPUTMOD_INOUT,
		    
		    MOJOSHADER_AST_INPUTMOD_UNIFORM,
		}
		
		public enum MOJOSHADER_astInterpolationModifier {
		    
		    MOJOSHADER_AST_INTERPMOD_NONE,
		    
		    MOJOSHADER_AST_INTERPMOD_LINEAR,
		    
		    MOJOSHADER_AST_INTERPMOD_CENTROID,
		    
		    MOJOSHADER_AST_INTERPMOD_NOINTERPOLATION,
		    
		    MOJOSHADER_AST_INTERPMOD_NOPERSPECTIVE,
		    
		    MOJOSHADER_AST_INTERPMOD_SAMPLE,
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astFunctionParameters {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// MOJOSHADER_astInputModifier
		    public MOJOSHADER_astInputModifier input_modifier;
		    
		    /// char*
            public IntPtr identifier;
		    
		    /// char*
            public IntPtr semantic;
		    
		    /// MOJOSHADER_astInterpolationModifier
		    public MOJOSHADER_astInterpolationModifier interpolation_modifier;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr initializer;
		    
		    /// MOJOSHADER_astFunctionParameters*
		    public IntPtr next;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astFunctionSignature {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// char*
            public IntPtr identifier;
		    
		    /// MOJOSHADER_astFunctionParameters*
		    public IntPtr @params;
		    
		    /// MOJOSHADER_astFunctionStorageClass
		    public MOJOSHADER_astFunctionStorageClass storage_class;
		    
		    /// char*
            public IntPtr semantic;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astScalarOrArray {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// char*
            public IntPtr identifier;
		    
		    /// int
		    public int isarray;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr dimension;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astAnnotations {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr initializer;
		    
		    /// MOJOSHADER_astAnnotations*
		    public IntPtr next;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astPackOffset {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// char*
            public IntPtr ident1;
		    
		    /// char*
            public IntPtr ident2;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astVariableLowLevel {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astPackOffset*
		    public IntPtr packoffset;
		    
		    /// char*
            public IntPtr register_name;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astStructMembers {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// char*
            public IntPtr semantic;
		    
		    /// MOJOSHADER_astScalarOrArray*
		    public IntPtr details;
		    
		    /// MOJOSHADER_astInterpolationModifier
		    public MOJOSHADER_astInterpolationModifier interpolation_mod;
		    
		    /// MOJOSHADER_astStructMembers*
		    public IntPtr next;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astStructDeclaration {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// char*
            public IntPtr name;
		    
		    /// MOJOSHADER_astStructMembers*
		    public IntPtr members;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astVariableDeclaration {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// int
		    public int attributes;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// MOJOSHADER_astStructDeclaration*
		    public IntPtr anonymous_datatype;
		    
		    /// MOJOSHADER_astScalarOrArray*
		    public IntPtr details;
		    
		    /// char*
            public IntPtr semantic;
		    
		    /// MOJOSHADER_astAnnotations*
		    public IntPtr annotations;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr initializer;
		    
		    /// MOJOSHADER_astVariableLowLevel*
		    public IntPtr lowlevel;
		    
		    /// MOJOSHADER_astVariableDeclaration*
		    public IntPtr next;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astBlockStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr statements;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astReturnStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr expr;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astExpressionStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr expr;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astIfStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		    
		    /// int
		    public int attributes;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr expr;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr statement;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr else_statement;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astSwitchCases {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr expr;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr statement;
		    
		    /// MOJOSHADER_astSwitchCases*
		    public IntPtr next;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astSwitchStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		    
		    /// int
		    public int attributes;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr expr;
		    
		    /// MOJOSHADER_astSwitchCases*
		    public IntPtr cases;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astWhileStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		    
		    /// int
		    public int unroll;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr expr;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr statement;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astForStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		    
		    /// int
		    public int unroll;
		    
		    /// MOJOSHADER_astVariableDeclaration*
		    public IntPtr var_decl;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr initializer;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr looptest;
		    
		    /// MOJOSHADER_astExpression*
		    public IntPtr counter;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr statement;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astTypedef {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astDataType*
		    public IntPtr datatype;
		    
		    /// int
		    public int isconst;
		    
		    /// MOJOSHADER_astScalarOrArray*
		    public IntPtr details;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astTypedefStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		    
		    /// MOJOSHADER_astTypedef*
		    public IntPtr type_info;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astVarDeclStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		    
		    /// MOJOSHADER_astVariableDeclaration*
		    public IntPtr declaration;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astStructStatement {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr next;
		    
		    /// MOJOSHADER_astStructDeclaration*
		    public IntPtr struct_info;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astCompilationUnitFunction {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astCompilationUnit*
		    public IntPtr next;
		    
		    /// MOJOSHADER_astFunctionSignature*
		    public IntPtr declaration;
		    
		    /// MOJOSHADER_astStatement*
		    public IntPtr definition;
		    
		    /// int
		    public int index;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astCompilationUnitTypedef {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astCompilationUnit*
		    public IntPtr next;
		    
		    /// MOJOSHADER_astTypedef*
		    public IntPtr type_info;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astCompilationUnitStruct {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astCompilationUnit*
		    public IntPtr next;
		    
		    /// MOJOSHADER_astStructDeclaration*
		    public IntPtr struct_info;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astCompilationUnitVariable {
		    
		    /// MOJOSHADER_astNodeInfo
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astCompilationUnit*
		    public IntPtr next;
		    
		    /// MOJOSHADER_astVariableDeclaration*
		    public IntPtr declaration;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		public struct MOJOSHADER_astNode {
		    
		    /// MOJOSHADER_astNodeInfo
		    [FieldOffset(0)]
		    public MOJOSHADER_astNodeInfo ast;
		    
		    /// MOJOSHADER_astGeneric
		    [FieldOffset(0)]
		    public MOJOSHADER_astGeneric generic;
		    
		    /// MOJOSHADER_astExpression
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpression expression;
		    
		    /// MOJOSHADER_astArguments
		    [FieldOffset(0)]
		    public MOJOSHADER_astArguments arguments;
		    
		    /// MOJOSHADER_astExpressionUnary
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionUnary unary;
		    
		    /// MOJOSHADER_astExpressionBinary
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionBinary binary;
		    
		    /// MOJOSHADER_astExpressionTernary
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionTernary ternary;
		    
		    /// MOJOSHADER_astExpressionIdentifier
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionIdentifier identifier;
		    
		    /// MOJOSHADER_astExpressionIntLiteral
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionIntLiteral intliteral;
		    
		    /// MOJOSHADER_astExpressionFloatLiteral
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionFloatLiteral floatliteral;
		    
		    /// MOJOSHADER_astExpressionStringLiteral
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionStringLiteral stringliteral;
		    
		    /// MOJOSHADER_astExpressionBooleanLiteral
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionBooleanLiteral boolliteral;
		    
		    /// MOJOSHADER_astExpressionConstructor
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionConstructor constructor;
		    
		    /// MOJOSHADER_astExpressionDerefStruct
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionDerefStruct derefstruct;
		    
		    /// MOJOSHADER_astExpressionCallFunction
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionCallFunction callfunc;
		    
		    /// MOJOSHADER_astExpressionCast
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionCast cast;
		    
		    /// MOJOSHADER_astCompilationUnit
		    [FieldOffset(0)]
		    public MOJOSHADER_astCompilationUnit compunit;
		    
		    /// MOJOSHADER_astFunctionParameters
		    [FieldOffset(0)]
		    public MOJOSHADER_astFunctionParameters @params;
		    
		    /// MOJOSHADER_astFunctionSignature
		    [FieldOffset(0)]
		    public MOJOSHADER_astFunctionSignature funcsig;
		    
		    /// MOJOSHADER_astScalarOrArray
		    [FieldOffset(0)]
		    public MOJOSHADER_astScalarOrArray soa;
		    
		    /// MOJOSHADER_astAnnotations
		    [FieldOffset(0)]
		    public MOJOSHADER_astAnnotations annotations;
		    
		    /// MOJOSHADER_astPackOffset
		    [FieldOffset(0)]
		    public MOJOSHADER_astPackOffset packoffset;
		    
		    /// MOJOSHADER_astVariableLowLevel
		    [FieldOffset(0)]
		    public MOJOSHADER_astVariableLowLevel varlowlevel;
		    
		    /// MOJOSHADER_astStructMembers
		    [FieldOffset(0)]
		    public MOJOSHADER_astStructMembers structmembers;
		    
		    /// MOJOSHADER_astStructDeclaration
		    [FieldOffset(0)]
		    public MOJOSHADER_astStructDeclaration structdecl;
		    
		    /// MOJOSHADER_astVariableDeclaration
		    [FieldOffset(0)]
		    public MOJOSHADER_astVariableDeclaration vardecl;
		    
		    /// MOJOSHADER_astStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astStatement stmt;
		    
		    /// MOJOSHADER_astEmptyStatement->MOJOSHADER_astStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astStatement emptystmt;
		    
		    /// MOJOSHADER_astBreakStatement->MOJOSHADER_astStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astStatement breakstmt;
		    
		    /// MOJOSHADER_astContinueStatement->MOJOSHADER_astStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astStatement contstmt;
		    
		    /// MOJOSHADER_astDiscardStatement->MOJOSHADER_astStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astStatement discardstmt;
		    
		    /// MOJOSHADER_astBlockStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astBlockStatement blockstmt;
		    
		    /// MOJOSHADER_astReturnStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astReturnStatement returnstmt;
		    
		    /// MOJOSHADER_astExpressionStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astExpressionStatement exprstmt;
		    
		    /// MOJOSHADER_astIfStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astIfStatement ifstmt;
		    
		    /// MOJOSHADER_astSwitchCases
		    [FieldOffset(0)]
		    public MOJOSHADER_astSwitchCases cases;
		    
		    /// MOJOSHADER_astSwitchStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astSwitchStatement switchstmt;
		    
		    /// MOJOSHADER_astWhileStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astWhileStatement whilestmt;
		    
		    /// MOJOSHADER_astDoStatement->MOJOSHADER_astWhileStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astWhileStatement dostmt;
		    
		    /// MOJOSHADER_astForStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astForStatement forstmt;
		    
		    /// MOJOSHADER_astTypedef
		    [FieldOffset(0)]
		    public MOJOSHADER_astTypedef typdef;
		    
		    /// MOJOSHADER_astTypedefStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astTypedefStatement typedefstmt;
		    
		    /// MOJOSHADER_astVarDeclStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astVarDeclStatement vardeclstmt;
		    
		    /// MOJOSHADER_astStructStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_astStructStatement structstmt;
		    
		    /// MOJOSHADER_astCompilationUnitFunction
		    [FieldOffset(0)]
		    public MOJOSHADER_astCompilationUnitFunction funcunit;
		    
		    /// MOJOSHADER_astCompilationUnitTypedef
		    [FieldOffset(0)]
		    public MOJOSHADER_astCompilationUnitTypedef typedefunit;
		    
		    /// MOJOSHADER_astCompilationUnitStruct
		    [FieldOffset(0)]
		    public MOJOSHADER_astCompilationUnitStruct structunit;
		    
		    /// MOJOSHADER_astCompilationUnitVariable
		    [FieldOffset(0)]
		    public MOJOSHADER_astCompilationUnitVariable varunit;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_astData {
		    
		    /// int
		    public int error_count;
		    
		    /// MOJOSHADER_error*
		    public IntPtr errors;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string source_profile;
		    
		    /// MOJOSHADER_astNode*
		    public IntPtr ast;
		    
		    /// MOJOSHADER_malloc
		    public IntPtr malloc;
		    
		    /// MOJOSHADER_free
		    public IntPtr free;
		    
		    /// void*
		    public IntPtr malloc_data;
		    
		    /// void*
		    public IntPtr opaque;
		}
		
		public enum MOJOSHADER_irNodeType {
		    
		    MOJOSHADER_IR_START_RANGE_EXPR,
		    
		    MOJOSHADER_IR_CONSTANT,
		    
		    MOJOSHADER_IR_TEMP,
		    
		    MOJOSHADER_IR_BINOP,
		    
		    MOJOSHADER_IR_MEMORY,
		    
		    MOJOSHADER_IR_CALL,
		    
		    MOJOSHADER_IR_ESEQ,
		    
		    MOJOSHADER_IR_ARRAY,
		    
		    MOJOSHADER_IR_CONVERT,
		    
		    MOJOSHADER_IR_SWIZZLE,
		    
		    MOJOSHADER_IR_CONSTRUCT,
		    
		    MOJOSHADER_IR_END_RANGE_EXPR,
		    
		    MOJOSHADER_IR_START_RANGE_STMT,
		    
		    MOJOSHADER_IR_MOVE,
		    
		    MOJOSHADER_IR_EXPR_STMT,
		    
		    MOJOSHADER_IR_JUMP,
		    
		    MOJOSHADER_IR_CJUMP,
		    
		    MOJOSHADER_IR_SEQ,
		    
		    MOJOSHADER_IR_LABEL,
		    
		    MOJOSHADER_IR_DISCARD,
		    
		    MOJOSHADER_IR_END_RANGE_STMT,
		    
		    MOJOSHADER_IR_START_RANGE_MISC,
		    
		    MOJOSHADER_IR_EXPRLIST,
		    
		    MOJOSHADER_IR_END_RANGE_MISC,
		    
		    MOJOSHADER_IR_END_RANGE,
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irNodeInfo {
		    
		    /// MOJOSHADER_irNodeType
		    public MOJOSHADER_irNodeType type;
		    
		    /// char*
		    public IntPtr filename;
		    
		    /// unsigned int
		    public uint line;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irGeneric {
		    
		    /// MOJOSHADER_irNodeInfo
		    public MOJOSHADER_irNodeInfo ir;
		}
		
		public enum MOJOSHADER_irBinOpType {
		    
		    MOJOSHADER_IR_BINOP_ADD,
		    
		    MOJOSHADER_IR_BINOP_SUBTRACT,
		    
		    MOJOSHADER_IR_BINOP_MULTIPLY,
		    
		    MOJOSHADER_IR_BINOP_DIVIDE,
		    
		    MOJOSHADER_IR_BINOP_MODULO,
		    
		    MOJOSHADER_IR_BINOP_AND,
		    
		    MOJOSHADER_IR_BINOP_OR,
		    
		    MOJOSHADER_IR_BINOP_XOR,
		    
		    MOJOSHADER_IR_BINOP_LSHIFT,
		    
		    MOJOSHADER_IR_BINOP_RSHIFT,
		    
		    MOJOSHADER_IR_BINOP_UNKNOWN,
		}
		
		public enum MOJOSHADER_irConditionType {
		    
		    MOJOSHADER_IR_COND_EQL,
		    
		    MOJOSHADER_IR_COND_NEQ,
		    
		    MOJOSHADER_IR_COND_LT,
		    
		    MOJOSHADER_IR_COND_GT,
		    
		    MOJOSHADER_IR_COND_LEQ,
		    
		    MOJOSHADER_IR_COND_GEQ,
		    
		    MOJOSHADER_IR_COND_UNKNOWN,
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irExprInfo {
		    
		    /// MOJOSHADER_irNodeInfo
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// MOJOSHADER_astDataTypeType
		    public MOJOSHADER_astDataTypeType type;
		    
		    /// int
		    public int elements;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irConstant {
		    
		    /// MOJOSHADER_irExprInfo
		    public MOJOSHADER_irExprInfo info;
		    
		    /// Anonymous_3a13e6d2_72d8_4c86_b5bf_9aff36c73111
		    public Anonymous_3a13e6d2_72d8_4c86_b5bf_9aff36c73111 value;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irTemp {
		    
		    /// MOJOSHADER_irExprInfo
		    public MOJOSHADER_irExprInfo info;
		    
		    /// int
		    public int index;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irBinOp {
		    
		    /// MOJOSHADER_irExprInfo
		    public MOJOSHADER_irExprInfo info;
		    
		    /// MOJOSHADER_irBinOpType
		    public MOJOSHADER_irBinOpType op;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr left;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr right;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irMemory {
		    
		    /// MOJOSHADER_irExprInfo
		    public MOJOSHADER_irExprInfo info;
		    
		    /// int
		    public int index;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irCall {
		    
		    /// MOJOSHADER_irExprInfo
		    public MOJOSHADER_irExprInfo info;
		    
		    /// int
		    public int index;
		    
		    /// MOJOSHADER_irExprList*
		    public IntPtr args;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irESeq {
		    
		    /// MOJOSHADER_irExprInfo
		    public MOJOSHADER_irExprInfo info;
		    
		    /// MOJOSHADER_irStatement*
		    public IntPtr stmt;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr expr;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irArray {
		    
		    /// MOJOSHADER_irExprInfo
		    public MOJOSHADER_irExprInfo info;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr array;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr element;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irConvert {
		    
		    /// MOJOSHADER_irExprInfo
		    public MOJOSHADER_irExprInfo info;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr expr;
		}
		
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
		public struct MOJOSHADER_irSwizzle {
		    
		    /// MOJOSHADER_irExprInfo
		    public MOJOSHADER_irExprInfo info;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr expr;
		    
		    /// char[4]
		    public IntPtr channels;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irConstruct {
		    
		    /// MOJOSHADER_irExprInfo
		    public MOJOSHADER_irExprInfo info;
		    
		    /// MOJOSHADER_irExprList*
		    public IntPtr args;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		public struct MOJOSHADER_irExpression {
		    
		    /// MOJOSHADER_irNodeInfo
		    [FieldOffset(0)]
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// MOJOSHADER_irExprInfo
		    [FieldOffset(0)]
		    public MOJOSHADER_irExprInfo info;
		    
		    /// MOJOSHADER_irConstant
		    [FieldOffset(0)]
		    public MOJOSHADER_irConstant constant;
		    
		    /// MOJOSHADER_irTemp
		    [FieldOffset(0)]
		    public MOJOSHADER_irTemp temp;
		    
		    /// MOJOSHADER_irBinOp
		    [FieldOffset(0)]
		    public MOJOSHADER_irBinOp binop;
		    
		    /// MOJOSHADER_irMemory
		    [FieldOffset(0)]
		    public MOJOSHADER_irMemory memory;
		    
		    /// MOJOSHADER_irCall
		    [FieldOffset(0)]
		    public MOJOSHADER_irCall call;
		    
		    /// MOJOSHADER_irESeq
		    [FieldOffset(0)]
		    public MOJOSHADER_irESeq eseq;
		    
		    /// MOJOSHADER_irArray
		    [FieldOffset(0)]
		    public MOJOSHADER_irArray array;
		    
		    /// MOJOSHADER_irConvert
		    [FieldOffset(0)]
		    public MOJOSHADER_irConvert convert;
		    
		    /// MOJOSHADER_irSwizzle
		    [FieldOffset(0)]
		    public MOJOSHADER_irSwizzle swizzle;
		    
		    /// MOJOSHADER_irConstruct
		    [FieldOffset(0)]
		    public MOJOSHADER_irConstruct construct;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irMove {
		    
		    /// MOJOSHADER_irNodeInfo
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr dst;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr src;
		    
		    /// int
		    public int writemask;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irExprStmt {
		    
		    /// MOJOSHADER_irNodeInfo
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr expr;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irJump {
		    
		    /// MOJOSHADER_irNodeInfo
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// int
		    public int label;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irCJump {
		    
		    /// MOJOSHADER_irNodeInfo
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// MOJOSHADER_irConditionType
		    public MOJOSHADER_irConditionType cond;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr left;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr right;
		    
		    /// int
		    public int iftrue;
		    
		    /// int
		    public int iffalse;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irSeq {
		    
		    /// MOJOSHADER_irNodeInfo
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// MOJOSHADER_irStatement*
		    public IntPtr first;
		    
		    /// MOJOSHADER_irStatement*
		    public IntPtr next;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irLabel {
		    
		    /// MOJOSHADER_irNodeInfo
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// int
		    public int index;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		public struct MOJOSHADER_irStatement {
		    
		    /// MOJOSHADER_irNodeInfo
		    [FieldOffset(0)]
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// MOJOSHADER_irGeneric
		    [FieldOffset(0)]
		    public MOJOSHADER_irGeneric generic;
		    
		    /// MOJOSHADER_irMove
		    [FieldOffset(0)]
		    public MOJOSHADER_irMove move;
		    
		    /// MOJOSHADER_irExprStmt
		    [FieldOffset(0)]
		    public MOJOSHADER_irExprStmt expr;
		    
		    /// MOJOSHADER_irJump
		    [FieldOffset(0)]
		    public MOJOSHADER_irJump jump;
		    
		    /// MOJOSHADER_irCJump
		    [FieldOffset(0)]
		    public MOJOSHADER_irCJump cjump;
		    
		    /// MOJOSHADER_irSeq
		    [FieldOffset(0)]
		    public MOJOSHADER_irSeq seq;
		    
		    /// MOJOSHADER_irLabel
		    [FieldOffset(0)]
		    public MOJOSHADER_irLabel label;
		    
		    /// MOJOSHADER_irDiscard->MOJOSHADER_irGeneric
		    [FieldOffset(0)]
		    public MOJOSHADER_irGeneric discard;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_irExprList {
		    
		    /// MOJOSHADER_irNodeInfo
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// MOJOSHADER_irExpression*
		    public IntPtr expr;
		    
		    /// MOJOSHADER_irExprList*
		    public IntPtr next;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		public struct MOJOSHADER_irMisc {
		    
		    /// MOJOSHADER_irNodeInfo
		    [FieldOffset(0)]
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// MOJOSHADER_irGeneric
		    [FieldOffset(0)]
		    public MOJOSHADER_irGeneric generic;
		    
		    /// MOJOSHADER_irExprList
		    [FieldOffset(0)]
		    public MOJOSHADER_irExprList exprlist;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		public struct MOJOSHADER_irNode {
		    
		    /// MOJOSHADER_irNodeInfo
		    [FieldOffset(0)]
		    public MOJOSHADER_irNodeInfo ir;
		    
		    /// MOJOSHADER_irGeneric
		    [FieldOffset(0)]
		    public MOJOSHADER_irGeneric generic;
		    
		    /// MOJOSHADER_irExpression
		    [FieldOffset(0)]
		    public MOJOSHADER_irExpression expr;
		    
		    /// MOJOSHADER_irStatement
		    [FieldOffset(0)]
		    public MOJOSHADER_irStatement stmt;
		    
		    /// MOJOSHADER_irMisc
		    [FieldOffset(0)]
		    public MOJOSHADER_irMisc misc;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct MOJOSHADER_compileData {
		    
		    /// int
		    public int error_count;
		    
		    /// MOJOSHADER_error*
		    public IntPtr errors;
		    
		    /// int
		    public int warning_count;
		    
		    /// MOJOSHADER_error*
		    public IntPtr warnings;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string source_profile;
		    
		    /// char*
		    [MarshalAs(UnmanagedType.LPStr)]
		    public string output;
		    
		    /// int
		    public int output_len;
		    
		    /// int
		    public int symbol_count;
		    
		    /// MOJOSHADER_symbol*
		    public IntPtr symbols;
		    
		    /// MOJOSHADER_malloc
		    public IntPtr malloc;
		    
		    /// MOJOSHADER_free
		    public IntPtr free;
		    
		    /// void*
		    public IntPtr malloc_data;
		}
		
		/// Return Type: void*
		///fnname: char*
		///data: void*
		public delegate IntPtr MOJOSHADER_glGetProcAddress([In()] [MarshalAs(UnmanagedType.LPStr)] string fnname, IntPtr data);
		
		[StructLayout(LayoutKind.Explicit)]
		public struct Anonymous_5371dd6a_e42a_47c1_91d1_a2af9a8283be {
		    
		    /// float[4]
		    [MarshalAs(UnmanagedType.ByValArray, SizeConst=4, ArraySubType=UnmanagedType.R4)]
		    [FieldOffset(0)]
		    public IntPtr f;
		    
		    /// int[4]
		    [MarshalAs(UnmanagedType.ByValArray, SizeConst=4, ArraySubType=UnmanagedType.I4)]
		    [FieldOffset(0)]
		    public IntPtr i;
		    
		    /// int
		    [FieldOffset(0)]
		    public int b;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		public struct Anonymous_3a13e6d2_72d8_4c86_b5bf_9aff36c73111 {
		    
		    /// int[16]
		    [MarshalAs(UnmanagedType.ByValArray, SizeConst=16, ArraySubType=UnmanagedType.I4)]
		    [FieldOffset(0)]
		    public IntPtr ival;
		    
		    /// float[16]
		    [MarshalAs(UnmanagedType.ByValArray, SizeConst=16, ArraySubType=UnmanagedType.R4)]
		    [FieldOffset(0)]
            public IntPtr fval;
		}
		
		public enum MOJOSHADER_uniformType {
		    
		    /// MOJOSHADER_UNIFORM_UNKNOWN -> -1
		    MOJOSHADER_UNIFORM_UNKNOWN = -1,
		    
		    MOJOSHADER_UNIFORM_FLOAT,
		    
		    MOJOSHADER_UNIFORM_INT,
		    
		    MOJOSHADER_UNIFORM_BOOL,
		}
		
		public enum MOJOSHADER_samplerType {
		    
		    /// MOJOSHADER_SAMPLER_UNKNOWN -> -1
		    MOJOSHADER_SAMPLER_UNKNOWN = -1,
		    
		    MOJOSHADER_SAMPLER_2D = 0,
		    
		    MOJOSHADER_SAMPLER_CUBE = 1,
		    
		    MOJOSHADER_SAMPLER_VOLUME = 2,

            MOJOSHADER_SAMPLER_1D = 3,
		}
		
		public enum MOJOSHADER_usage {
		    
		    /// MOJOSHADER_USAGE_UNKNOWN -> -1
		    MOJOSHADER_USAGE_UNKNOWN = -1,
		    
		    MOJOSHADER_USAGE_POSITION,
		    
		    MOJOSHADER_USAGE_BLENDWEIGHT,
		    
		    MOJOSHADER_USAGE_BLENDINDICES,
		    
		    MOJOSHADER_USAGE_NORMAL,
		    
		    MOJOSHADER_USAGE_POINTSIZE,
		    
		    MOJOSHADER_USAGE_TEXCOORD,
		    
		    MOJOSHADER_USAGE_TANGENT,
		    
		    MOJOSHADER_USAGE_BINORMAL,
		    
		    MOJOSHADER_USAGE_TESSFACTOR,
		    
		    MOJOSHADER_USAGE_POSITIONT,
		    
		    MOJOSHADER_USAGE_COLOR,
		    
		    MOJOSHADER_USAGE_FOG,
		    
		    MOJOSHADER_USAGE_DEPTH,
		    
		    MOJOSHADER_USAGE_SAMPLE,
		    
		    MOJOSHADER_USAGE_TOTAL,
		}
		
		public enum MOJOSHADER_symbolClass {
		    
		    MOJOSHADER_SYMCLASS_SCALAR,
		    
		    MOJOSHADER_SYMCLASS_VECTOR,
		    
		    MOJOSHADER_SYMCLASS_MATRIX_ROWS,
		    
		    MOJOSHADER_SYMCLASS_MATRIX_COLUMNS,
		    
		    MOJOSHADER_SYMCLASS_OBJECT,
		    
		    MOJOSHADER_SYMCLASS_STRUCT,
		}
		
		public enum MOJOSHADER_symbolType {
		    
		    MOJOSHADER_SYMTYPE_VOID,
		    
		    MOJOSHADER_SYMTYPE_BOOL,
		    
		    MOJOSHADER_SYMTYPE_INT,
		    
		    MOJOSHADER_SYMTYPE_FLOAT,
		    
		    MOJOSHADER_SYMTYPE_STRING,
		    
		    MOJOSHADER_SYMTYPE_TEXTURE,
		    
		    MOJOSHADER_SYMTYPE_TEXTURE1D,
		    
		    MOJOSHADER_SYMTYPE_TEXTURE2D,
		    
		    MOJOSHADER_SYMTYPE_TEXTURE3D,
		    
		    MOJOSHADER_SYMTYPE_TEXTURECUBE,
		    
		    MOJOSHADER_SYMTYPE_SAMPLER,
		    
		    MOJOSHADER_SYMTYPE_SAMPLER1D,
		    
		    MOJOSHADER_SYMTYPE_SAMPLER2D,
		    
		    MOJOSHADER_SYMTYPE_SAMPLER3D,
		    
		    MOJOSHADER_SYMTYPE_SAMPLERCUBE,
		    
		    MOJOSHADER_SYMTYPE_PIXELSHADER,
		    
		    MOJOSHADER_SYMTYPE_VERTEXSHADER,
		    
		    MOJOSHADER_SYMTYPE_PIXELFRAGMENT,
		    
		    MOJOSHADER_SYMTYPE_VERTEXFRAGMENT,
		    
		    MOJOSHADER_SYMTYPE_UNSUPPORTED,
		}
		
		public enum MOJOSHADER_symbolRegisterSet {
		    
		    MOJOSHADER_SYMREGSET_BOOL,
		    
		    MOJOSHADER_SYMREGSET_INT4,
		    
		    MOJOSHADER_SYMREGSET_FLOAT4,
		    
		    MOJOSHADER_SYMREGSET_SAMPLER,
		}
		
		public enum MOJOSHADER_shaderType {
		    
		    // MOJOSHADER_TYPE_UNKNOWN -> 0
		    MOJOSHADER_TYPE_UNKNOWN = 0,
		    
		    // MOJOSHADER_TYPE_PIXEL -> (1<<0)
		    MOJOSHADER_TYPE_PIXEL = (1) << (0),
		    
		    // MOJOSHADER_TYPE_VERTEX -> (1<<1)
		    MOJOSHADER_TYPE_VERTEX = (1) << (1),
		    
		    // MOJOSHADER_TYPE_GEOMETRY -> (1<<2)
		    MOJOSHADER_TYPE_GEOMETRY = (1) << (2),
		    
		    // MOJOSHADER_TYPE_ANY -> 0xFFFFFFFF
		    MOJOSHADER_TYPE_ANY = -1,
		}
		
		public enum MOJOSHADER_includeType {
		    
		    MOJOSHADER_INCLUDETYPE_LOCAL,
		    
		    MOJOSHADER_INCLUDETYPE_SYSTEM,
		}
		
		public enum MOJOSHADER_attributeType {
		    
		    // MOJOSHADER_ATTRIBUTE_UNKNOWN -> -1
		    MOJOSHADER_ATTRIBUTE_UNKNOWN = -1,
		    
		    MOJOSHADER_ATTRIBUTE_BYTE,
		    
		    MOJOSHADER_ATTRIBUTE_UBYTE,
		    
		    MOJOSHADER_ATTRIBUTE_SHORT,
		    
		    MOJOSHADER_ATTRIBUTE_USHORT,
		    
		    MOJOSHADER_ATTRIBUTE_INT,
		    
		    MOJOSHADER_ATTRIBUTE_UINT,
		    
		    MOJOSHADER_ATTRIBUTE_FLOAT,
		    
		    MOJOSHADER_ATTRIBUTE_DOUBLE,
		    
		    MOJOSHADER_ATTRIBUTE_HALF_FLOAT,
		}
		
		public partial class NativeMethods {
		    
		    /// Return Type: int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_version")]
		public static extern  int MOJOSHADER_version() ;
		
		    
		    /// Return Type: char*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_changeset")]
		public static extern  IntPtr MOJOSHADER_changeset() ;
		
		    
		    /// Return Type: int
		    ///profile: char*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_maxShaderModel")]
		public static extern  int MOJOSHADER_maxShaderModel([In()] [MarshalAs(UnmanagedType.LPStr)] string profile) ;
		
		    
		    /// Return Type: MOJOSHADER_parseData*
		    ///tokenbuf: char*
		    ///bufsize: int
		    ///m: MOJOSHADER_malloc
		    ///f: MOJOSHADER_free
		    ///d: void*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_parseExpression")]
		public static extern  IntPtr MOJOSHADER_parseExpression([In()] byte[] tokenbuf, int bufsize, IntPtr m, IntPtr f, IntPtr d) ;
		
		    
		    /// Return Type: void
		    ///param0: MOJOSHADER_preshader*
		    ///param1: float*
		    ///param2: float*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_runPreshader")]
		public static extern  void MOJOSHADER_runPreshader(ref MOJOSHADER_preshader param0, ref float param1, ref float param2) ;
		
		    
		    /// Return Type: MOJOSHADER_parseData*
		    ///profile: char*
		    ///tokenbuf: char*
		    ///bufsize: int
		    ///swiz: MOJOSHADER_swizzle*
		    ///swizcount: int
		    ///smap: MOJOSHADER_samplerMap*
		    ///smapcount: int
		    ///m: MOJOSHADER_malloc
		    ///f: MOJOSHADER_free
		    ///d: void*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_parse")]
		public static extern  IntPtr MOJOSHADER_parse([In()] [MarshalAs(UnmanagedType.LPStr)] string profile, [In()] byte[] tokenbuf, int bufsize, IntPtr swiz, int swizcount, IntPtr smap, int smapcount, IntPtr m, IntPtr f, IntPtr d) ;
		
		    
		    /// Return Type: void
		    ///data: MOJOSHADER_parseData*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_freeParseData")]
		public static extern  void MOJOSHADER_freeParseData(ref MOJOSHADER_parseData data) ;
		
		    
		    /// Return Type: MOJOSHADER_effect*
		    ///profile: char*
		    ///buf: char*
		    ///_len: int
		    ///swiz: MOJOSHADER_swizzle*
		    ///swizcount: int
		    ///smap: MOJOSHADER_samplerMap*
		    ///smapcount: int
		    ///m: MOJOSHADER_malloc
		    ///f: MOJOSHADER_free
		    ///d: void*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_parseEffect")]
		public static extern  IntPtr MOJOSHADER_parseEffect([In()] [MarshalAs(UnmanagedType.LPStr)] string profile, [In()] [MarshalAs(UnmanagedType.LPStr)] string buf, int _len, ref MOJOSHADER_swizzle swiz, int swizcount, ref MOJOSHADER_samplerMap smap, int smapcount, IntPtr m, IntPtr f, IntPtr d) ;
		
		    
		    /// Return Type: void
		    ///effect: MOJOSHADER_effect*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_freeEffect")]
		public static extern  void MOJOSHADER_freeEffect(ref MOJOSHADER_effect effect) ;
		
		    
		    /// Return Type: MOJOSHADER_preprocessData*
		    ///filename: char*
		    ///source: char*
		    ///sourcelen: unsigned int
		    ///defines: MOJOSHADER_preprocessorDefine*
		    ///define_count: unsigned int
		    ///include_open: MOJOSHADER_includeOpen
		    ///include_close: MOJOSHADER_includeClose
		    ///m: MOJOSHADER_malloc
		    ///f: MOJOSHADER_free
		    ///d: void*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_preprocess")]
		public static extern  IntPtr MOJOSHADER_preprocess([In()] [MarshalAs(UnmanagedType.LPStr)] string filename, [In()] [MarshalAs(UnmanagedType.LPStr)] string source, uint sourcelen, ref MOJOSHADER_preprocessorDefine defines, uint define_count, MOJOSHADER_includeOpen include_open, MOJOSHADER_includeClose include_close, IntPtr m, IntPtr f, IntPtr d) ;
		
		    
		    /// Return Type: void
		    ///data: MOJOSHADER_preprocessData*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_freePreprocessData")]
		public static extern  void MOJOSHADER_freePreprocessData(ref MOJOSHADER_preprocessData data) ;
		
		    
		    /// Return Type: MOJOSHADER_parseData*
		    ///filename: char*
		    ///source: char*
		    ///sourcelen: unsigned int
		    ///comments: char**
		    ///comment_count: unsigned int
		    ///symbols: MOJOSHADER_symbol*
		    ///symbol_count: unsigned int
		    ///defines: MOJOSHADER_preprocessorDefine*
		    ///define_count: unsigned int
		    ///include_open: MOJOSHADER_includeOpen
		    ///include_close: MOJOSHADER_includeClose
		    ///m: MOJOSHADER_malloc
		    ///f: MOJOSHADER_free
		    ///d: void*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_assemble")]
		public static extern  IntPtr MOJOSHADER_assemble([In()] [MarshalAs(UnmanagedType.LPStr)] string filename, [In()] [MarshalAs(UnmanagedType.LPStr)] string source, uint sourcelen, ref IntPtr comments, uint comment_count, ref MOJOSHADER_symbol symbols, uint symbol_count, ref MOJOSHADER_preprocessorDefine defines, uint define_count, MOJOSHADER_includeOpen include_open, MOJOSHADER_includeClose include_close, IntPtr m, IntPtr f, IntPtr d) ;
		
		    
		    /// Return Type: MOJOSHADER_astData*
		    ///srcprofile: char*
		    ///filename: char*
		    ///source: char*
		    ///sourcelen: unsigned int
		    ///defs: MOJOSHADER_preprocessorDefine*
		    ///define_count: unsigned int
		    ///include_open: MOJOSHADER_includeOpen
		    ///include_close: MOJOSHADER_includeClose
		    ///m: MOJOSHADER_malloc
		    ///f: MOJOSHADER_free
		    ///d: void*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_parseAst")]
		public static extern  IntPtr MOJOSHADER_parseAst([In()] [MarshalAs(UnmanagedType.LPStr)] string srcprofile, [In()] [MarshalAs(UnmanagedType.LPStr)] string filename, [In()] [MarshalAs(UnmanagedType.LPStr)] string source, uint sourcelen, ref MOJOSHADER_preprocessorDefine defs, uint define_count, MOJOSHADER_includeOpen include_open, MOJOSHADER_includeClose include_close, IntPtr m, IntPtr f, IntPtr d) ;
		
		    
		    /// Return Type: void
		    ///data: MOJOSHADER_astData*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_freeAstData")]
		public static extern  void MOJOSHADER_freeAstData(ref MOJOSHADER_astData data) ;
		
		    
		    /// Return Type: MOJOSHADER_compileData*
		    ///srcprofile: char*
		    ///filename: char*
		    ///source: char*
		    ///sourcelen: unsigned int
		    ///defs: MOJOSHADER_preprocessorDefine*
		    ///define_count: unsigned int
		    ///include_open: MOJOSHADER_includeOpen
		    ///include_close: MOJOSHADER_includeClose
		    ///m: MOJOSHADER_malloc
		    ///f: MOJOSHADER_free
		    ///d: void*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_compile")]
		public static extern  IntPtr MOJOSHADER_compile([In()] [MarshalAs(UnmanagedType.LPStr)] string srcprofile, [In()] [MarshalAs(UnmanagedType.LPStr)] string filename, [In()] [MarshalAs(UnmanagedType.LPStr)] string source, uint sourcelen, ref MOJOSHADER_preprocessorDefine defs, uint define_count, MOJOSHADER_includeOpen include_open, MOJOSHADER_includeClose include_close, IntPtr m, IntPtr f, IntPtr d) ;
		
		    
		    /// Return Type: void
		    ///data: MOJOSHADER_compileData*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_freeCompileData")]
		public static extern  void MOJOSHADER_freeCompileData(ref MOJOSHADER_compileData data) ;
		
		    
		    /// Return Type: int
		    ///lookup: MOJOSHADER_glGetProcAddress
		    ///d: void*
		    ///profs: char**
		    ///size: int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glAvailableProfiles")]
		public static extern  int MOJOSHADER_glAvailableProfiles(MOJOSHADER_glGetProcAddress lookup, IntPtr d, ref IntPtr profs, int size) ;
		
		    
		    /// Return Type: char*
		    ///lookup: MOJOSHADER_glGetProcAddress
		    ///d: void*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glBestProfile")]
		public static extern  IntPtr MOJOSHADER_glBestProfile(MOJOSHADER_glGetProcAddress lookup, IntPtr d) ;
		
		    
		    /// Return Type: char*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glGetError")]
		public static extern  IntPtr MOJOSHADER_glGetError() ;
		
		    
		    /// Return Type: int
		    ///shader_type: MOJOSHADER_shaderType->Anonymous_96517ad6_cc69_4542_8537_054e63919d54
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glMaxUniforms")]
		public static extern  int MOJOSHADER_glMaxUniforms(MOJOSHADER_shaderType shader_type) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: float*
		    ///vec4count: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glSetVertexShaderUniformF")]
		public static extern  void MOJOSHADER_glSetVertexShaderUniformF(uint idx, ref float data, uint vec4count) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: float*
		    ///vec4count: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glGetVertexShaderUniformF")]
		public static extern  void MOJOSHADER_glGetVertexShaderUniformF(uint idx, ref float data, uint vec4count) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: int*
		    ///ivec4count: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glSetVertexShaderUniformI")]
		public static extern  void MOJOSHADER_glSetVertexShaderUniformI(uint idx, ref int data, uint ivec4count) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: int*
		    ///ivec4count: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glGetVertexShaderUniformI")]
		public static extern  void MOJOSHADER_glGetVertexShaderUniformI(uint idx, ref int data, uint ivec4count) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: int*
		    ///bcount: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glSetVertexShaderUniformB")]
		public static extern  void MOJOSHADER_glSetVertexShaderUniformB(uint idx, ref int data, uint bcount) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: int*
		    ///bcount: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glGetVertexShaderUniformB")]
		public static extern  void MOJOSHADER_glGetVertexShaderUniformB(uint idx, ref int data, uint bcount) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: float*
		    ///vec4count: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glSetPixelShaderUniformF")]
		public static extern  void MOJOSHADER_glSetPixelShaderUniformF(uint idx, ref float data, uint vec4count) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: float*
		    ///vec4count: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glGetPixelShaderUniformF")]
		public static extern  void MOJOSHADER_glGetPixelShaderUniformF(uint idx, ref float data, uint vec4count) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: int*
		    ///ivec4count: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glSetPixelShaderUniformI")]
		public static extern  void MOJOSHADER_glSetPixelShaderUniformI(uint idx, ref int data, uint ivec4count) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: int*
		    ///ivec4count: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glGetPixelShaderUniformI")]
		public static extern  void MOJOSHADER_glGetPixelShaderUniformI(uint idx, ref int data, uint ivec4count) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: int*
		    ///bcount: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glSetPixelShaderUniformB")]
		public static extern  void MOJOSHADER_glSetPixelShaderUniformB(uint idx, ref int data, uint bcount) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: int*
		    ///bcount: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glGetPixelShaderUniformB")]
		public static extern  void MOJOSHADER_glGetPixelShaderUniformB(uint idx, ref int data, uint bcount) ;
		
		    
		    /// Return Type: void
		    ///sampler: unsigned int
		    ///mat00: float
		    ///mat01: float
		    ///mat10: float
		    ///mat11: float
		    ///lscale: float
		    ///loffset: float
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glSetLegacyBumpMapEnv")]
		public static extern  void MOJOSHADER_glSetLegacyBumpMapEnv(uint sampler, float mat00, float mat01, float mat10, float mat11, float lscale, float loffset) ;
		
		    
		    /// Return Type: void
		    ///usage: MOJOSHADER_usage->Anonymous_9c01433d_7bb5_4c50_bf77_e65cef0661b5
		    ///index: int
		    ///size: unsigned int
		    ///type: MOJOSHADER_attributeType->Anonymous_2f2591e6_1657_418c_9f54_80f3acd43cbe
		    ///normalized: int
		    ///stride: unsigned int
		    ///ptr: void*
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glSetVertexAttribute")]
		public static extern  void MOJOSHADER_glSetVertexAttribute(MOJOSHADER_usage usage, int index, uint size, MOJOSHADER_attributeType type, int normalized, uint stride, IntPtr ptr) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: float*
		    ///vec4n: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glSetVertexPreshaderUniformF")]
		public static extern  void MOJOSHADER_glSetVertexPreshaderUniformF(uint idx, ref float data, uint vec4n) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: float*
		    ///vec4n: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glGetVertexPreshaderUniformF")]
		public static extern  void MOJOSHADER_glGetVertexPreshaderUniformF(uint idx, ref float data, uint vec4n) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: float*
		    ///vec4n: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glSetPixelPreshaderUniformF")]
		public static extern  void MOJOSHADER_glSetPixelPreshaderUniformF(uint idx, ref float data, uint vec4n) ;
		
		    
		    /// Return Type: void
		    ///idx: unsigned int
		    ///data: float*
		    ///vec4n: unsigned int
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glGetPixelPreshaderUniformF")]
		public static extern  void MOJOSHADER_glGetPixelPreshaderUniformF(uint idx, ref float data, uint vec4n) ;
		
		    
		    /// Return Type: void
		    [DllImport(mojoshader_dll, EntryPoint="MOJOSHADER_glProgramReady")]
		public static extern  void MOJOSHADER_glProgramReady() ;
		
		}

		
	}
}

