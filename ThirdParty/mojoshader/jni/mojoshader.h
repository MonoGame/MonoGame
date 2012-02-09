/**
 * MojoShader; generate shader programs from bytecode of compiled
 *  Direct3D shaders.
 *
 * Please see the file LICENSE.txt in the source's root directory.
 *
 *  This file written by Ryan C. Gordon.
 */

#ifndef _INCL_MOJOSHADER_H_
#define _INCL_MOJOSHADER_H_

#ifdef __cplusplus
extern "C" {
#endif

/* You can define this if you aren't generating mojoshader_version.h */
#ifndef MOJOSHADER_NO_VERSION_INCLUDE
#include "mojoshader_version.h"
#endif

#ifndef MOJOSHADER_VERSION
#define MOJOSHADER_VERSION -1
#endif

#ifndef MOJOSHADER_CHANGESET
#define MOJOSHADER_CHANGESET "???"
#endif

/*
 * For determining the version of MojoShader you are using:
 *    const int compiled_against = MOJOSHADER_VERSION;
 *    const int linked_against = MOJOSHADER_version();
 *
 * The version is a single integer that increments, not a major/minor value.
 */
int MOJOSHADER_version(void);

/*
 * For determining the revision control changeset of MojoShader you are using:
 *    const char *compiled_against = MOJOSHADER_CHANGESET;
 *    const char *linked_against = MOJOSHADER_changeset();
 *
 * The version is an arbitrary, null-terminated ASCII string. It is probably
 *  a hash that represents a revision control changeset, and can't be
 *  compared to any other string to determine chronology.
 *
 * Do not attempt to free this string; it's statically allocated.
 */
const char *MOJOSHADER_changeset(void);

/*
 * These allocators work just like the C runtime's malloc() and free()
 *  (in fact, they probably use malloc() and free() internally if you don't
 *  specify your own allocator, but don't rely on that behaviour).
 * (data) is the pointer you supplied when specifying these allocator
 *  callbacks, in case you need instance-specific data...it is passed through
 *  to your allocator unmolested, and can be NULL if you like.
 */
typedef void *(*MOJOSHADER_malloc)(int bytes, void *data);
typedef void (*MOJOSHADER_free)(void *ptr, void *data);


/*
 * These are enum values, but they also can be used in bitmasks, so we can
 *  test if an opcode is acceptable: if (op->shader_types & ourtype) {} ...
 */
typedef enum
{
    MOJOSHADER_TYPE_UNKNOWN  = 0,
    MOJOSHADER_TYPE_PIXEL    = (1 << 0),
    MOJOSHADER_TYPE_VERTEX   = (1 << 1),
    MOJOSHADER_TYPE_GEOMETRY = (1 << 2),  /* (not supported yet.) */
    MOJOSHADER_TYPE_ANY = 0xFFFFFFFF   /* used for bitmasks */
} MOJOSHADER_shaderType;

/*
 * Data types for vertex attribute streams.
 */
typedef enum
{
    MOJOSHADER_ATTRIBUTE_UNKNOWN = -1,  /* housekeeping; not returned. */
    MOJOSHADER_ATTRIBUTE_BYTE,
    MOJOSHADER_ATTRIBUTE_UBYTE,
    MOJOSHADER_ATTRIBUTE_SHORT,
    MOJOSHADER_ATTRIBUTE_USHORT,
    MOJOSHADER_ATTRIBUTE_INT,
    MOJOSHADER_ATTRIBUTE_UINT,
    MOJOSHADER_ATTRIBUTE_FLOAT,
    MOJOSHADER_ATTRIBUTE_DOUBLE,
    MOJOSHADER_ATTRIBUTE_HALF_FLOAT,  /* MAYBE available in your OpenGL! */
} MOJOSHADER_attributeType;

/*
 * Data types for uniforms. See MOJOSHADER_uniform for more information.
 */
typedef enum
{
    MOJOSHADER_UNIFORM_UNKNOWN = -1, /* housekeeping value; never returned. */
    MOJOSHADER_UNIFORM_FLOAT,
    MOJOSHADER_UNIFORM_INT,
    MOJOSHADER_UNIFORM_BOOL,
} MOJOSHADER_uniformType;

/*
 * These are the uniforms to be set for a shader. "Uniforms" are what Direct3D
 *  calls "Constants" ... IDirect3DDevice::SetVertexShaderConstantF() would
 *  need this data, for example. These integers are register indexes. So if
 *  index==6 and type==MOJOSHADER_UNIFORM_FLOAT, that means we'd expect a
 *  4-float vector to be specified for what would be register "c6" in D3D
 *  assembly language, before drawing with the shader.
 * (array_count) means this is an array of uniforms...this happens in some
 *  profiles when we see a relative address ("c0[a0.x]", not the usual "c0").
 *  In those cases, the shader was built to set some range of constant
 *  registers as an array. You should set this array with (array_count)
 *  elements from the constant register file, starting at (index) instead of
 *  just a single uniform. To be extra difficult, you'll need to fill in the
 *  correct values from the MOJOSHADER_constant data into the appropriate
 *  parts of the array, overriding the constant register file. Fun!
 * (constant) says whether this is a constant array; these need to be loaded
 *  once at creation time, from the constant list and not ever updated from
 *  the constant register file. This is a workaround for limitations in some
 *  profiles.
 * (name) is a profile-specific variable name; it may be NULL if it isn't
 *  applicable to the requested profile.
 */
typedef struct MOJOSHADER_uniform
{
    MOJOSHADER_uniformType type;
    int index;
    int array_count;
    int constant;
    const char *name;
} MOJOSHADER_uniform;

/*
 * These are the constants defined in a shader. These are data values
 *  hardcoded in a shader (with the DEF, DEFI, DEFB instructions), which
 *  override your Uniforms. This data is largely for informational purposes,
 *  since they are compiled in and can't be changed, like Uniforms can be.
 * These integers are register indexes. So if index==6 and
 *  type==MOJOSHADER_UNIFORM_FLOAT, that means we'd expect a 4-float vector
 *  to be specified for what would be register "c6" in D3D assembly language,
 *  before drawing with the shader.
 * (value) is the value of the constant, unioned by type.
 */
typedef struct MOJOSHADER_constant
{
    MOJOSHADER_uniformType type;
    int index;
    union
    {
        float f[4];  /* if type==MOJOSHADER_UNIFORM_FLOAT */
        int i[4];    /* if type==MOJOSHADER_UNIFORM_INT */
        int b;       /* if type==MOJOSHADER_UNIFORM_BOOL */
    } value;
} MOJOSHADER_constant;

/*
 * Data types for samplers. See MOJOSHADER_sampler for more information.
 */
typedef enum
{
    MOJOSHADER_SAMPLER_UNKNOWN = -1, /* housekeeping value; never returned. */
    MOJOSHADER_SAMPLER_2D,
    MOJOSHADER_SAMPLER_CUBE,
    MOJOSHADER_SAMPLER_VOLUME,
} MOJOSHADER_samplerType;

/*
 * These are the samplers to be set for a shader. ...
 *  IDirect3DDevice::SetTexture() would need this data, for example.
 * These integers are the sampler "stage". So if index==6 and
 *  type==MOJOSHADER_SAMPLER_2D, that means we'd expect a regular 2D texture
 *  to be specified for what would be register "s6" in D3D assembly language,
 *  before drawing with the shader.
 * (name) is a profile-specific variable name; it may be NULL if it isn't
 *  applicable to the requested profile.
 */
typedef struct MOJOSHADER_sampler
{
    MOJOSHADER_samplerType type;
    int index;
    const char *name;
} MOJOSHADER_sampler;

/*
 * Data types for attributes. See MOJOSHADER_attribute for more information.
 */
typedef enum
{
    MOJOSHADER_USAGE_UNKNOWN = -1,  /* housekeeping value; never returned. */
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
    MOJOSHADER_USAGE_TOTAL,  /* housekeeping value; never returned. */
} MOJOSHADER_usage;

/*
 * These are the attributes to be set for a shader. "Attributes" are what
 *  Direct3D calls "Vertex Declarations Usages" ...
 *  IDirect3DDevice::CreateVertexDeclaration() would need this data, for
 *  example. Each attribute is associated with an array of data that uses one
 *  element per-vertex. So if usage==MOJOSHADER_USAGE_COLOR and index==1, that
 *  means we'd expect a secondary color array to be bound to this shader
 *  before drawing.
 * (name) is a profile-specific variable name; it may be NULL if it isn't
 *  applicable to the requested profile.
 */
typedef struct MOJOSHADER_attribute
{
    MOJOSHADER_usage usage;
    int index;
    const char *name;
} MOJOSHADER_attribute;

/*
 * Use this if you want to specify newly-parsed code to swizzle incoming
 *  data. This can be useful if you know, at parse time, that a shader
 *  will be processing data on COLOR0 that should be RGBA, but you'll
 *  be passing it a vertex array full of ARGB instead.
 */
typedef struct MOJOSHADER_swizzle
{
    MOJOSHADER_usage usage;
    unsigned int index;
    unsigned char swizzles[4];  /* {0,1,2,3} == .xyzw, {2,2,2,2} == .zzzz */
} MOJOSHADER_swizzle;


/*
 * MOJOSHADER_symbol data.
 *
 * These are used to expose high-level information in shader bytecode.
 *  They associate HLSL variables with registers. This data is used for both
 *  debugging and optimization.
 */

typedef enum
{
    MOJOSHADER_SYMREGSET_BOOL,
    MOJOSHADER_SYMREGSET_INT4,
    MOJOSHADER_SYMREGSET_FLOAT4,
    MOJOSHADER_SYMREGSET_SAMPLER,
} MOJOSHADER_symbolRegisterSet;

typedef enum
{
    MOJOSHADER_SYMCLASS_SCALAR,
    MOJOSHADER_SYMCLASS_VECTOR,
    MOJOSHADER_SYMCLASS_MATRIX_ROWS,
    MOJOSHADER_SYMCLASS_MATRIX_COLUMNS,
    MOJOSHADER_SYMCLASS_OBJECT,
    MOJOSHADER_SYMCLASS_STRUCT,
} MOJOSHADER_symbolClass;

typedef enum
{
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
} MOJOSHADER_symbolType;

typedef struct MOJOSHADER_symbolStructMember MOJOSHADER_symbolStructMember;

typedef struct MOJOSHADER_symbolTypeInfo
{
    MOJOSHADER_symbolClass parameter_class;
    MOJOSHADER_symbolType parameter_type;
    unsigned int rows;
    unsigned int columns;
    unsigned int elements;
    unsigned int member_count;
    MOJOSHADER_symbolStructMember *members;
} MOJOSHADER_symbolTypeInfo;

struct MOJOSHADER_symbolStructMember
{
    const char *name;
    MOJOSHADER_symbolTypeInfo info;
};

typedef struct MOJOSHADER_symbol
{
    const char *name;
    MOJOSHADER_symbolRegisterSet register_set;
    unsigned int register_index;
    unsigned int register_count;
    MOJOSHADER_symbolTypeInfo info;
} MOJOSHADER_symbol;


/*
 * These are used with MOJOSHADER_error as special case positions.
 */
#define MOJOSHADER_POSITION_NONE (-3)
#define MOJOSHADER_POSITION_BEFORE (-2)
#define MOJOSHADER_POSITION_AFTER (-1)

typedef struct MOJOSHADER_error
{
    /*
     * Human-readable error, if there is one. Will be NULL if there was no
     *  error. The string will be UTF-8 encoded, and English only. Most of
     *  these shouldn't be shown to the end-user anyhow.
     */
    const char *error;

    /*
     * Filename where error happened. This can be NULL if the information
     *  isn't available.
     */
    const char *filename;

    /*
     * Position of error, if there is one. Will be MOJOSHADER_POSITION_NONE if
     *  there was no error, MOJOSHADER_POSITION_BEFORE if there was an error
     *  before processing started, and MOJOSHADER_POSITION_AFTER if there was
     *  an error during final processing. If >= 0, MOJOSHADER_parse() sets
     *  this to the byte offset (starting at zero) into the bytecode you
     *  supplied, and MOJOSHADER_assemble(), MOJOSHADER_parseAst(), and
     *  MOJOSHADER_compile() sets this to a a line number in the source code
     *  you supplied (starting at one).
     */
    int error_position;
} MOJOSHADER_error;


/* !!! FIXME: document me. */
typedef enum MOJOSHADER_preshaderOpcode
{
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
    MOJOSHADER_PRESHADEROP_MIN_SCALAR = MOJOSHADER_PRESHADEROP_SCALAR_OPS,
    MOJOSHADER_PRESHADEROP_MAX_SCALAR,
    MOJOSHADER_PRESHADEROP_LT_SCALAR,
    MOJOSHADER_PRESHADEROP_GE_SCALAR,
    MOJOSHADER_PRESHADEROP_ADD_SCALAR,
    MOJOSHADER_PRESHADEROP_MUL_SCALAR,
    MOJOSHADER_PRESHADEROP_ATAN2_SCALAR,
    MOJOSHADER_PRESHADEROP_DIV_SCALAR,
    MOJOSHADER_PRESHADEROP_DOT_SCALAR,
    MOJOSHADER_PRESHADEROP_NOISE_SCALAR,
} MOJOSHADER_preshaderOpcode;

typedef enum MOJOSHADER_preshaderOperandType
{
	MOJOSHADER_PRESHADEROPERAND_LITERAL = 1,
	MOJOSHADER_PRESHADEROPERAND_INPUT = 2,
	MOJOSHADER_PRESHADEROPERAND_OUTPUT = 4,
	MOJOSHADER_PRESHADEROPERAND_TEMP = 7,
    MOJOSHADER_PRESHADEROPERAND_UNKN = 0xff,
} MOJOSHADER_preshaderOperandType;

typedef struct MOJOSHADER_preshaderOperand
{
    MOJOSHADER_preshaderOperandType type;
    unsigned int index;
	int indexingType;
	unsigned int indexingIndex;
} MOJOSHADER_preshaderOperand;

typedef struct MOJOSHADER_preshaderInstruction
{
    MOJOSHADER_preshaderOpcode opcode;
    unsigned int element_count;
    unsigned int operand_count;
    MOJOSHADER_preshaderOperand operands[4];
} MOJOSHADER_preshaderInstruction;

typedef struct MOJOSHADER_preshader
{
    unsigned int literal_count;
    double *literals;
    unsigned int temp_count;  /* scalar, not vector! */
    unsigned int symbol_count;
    MOJOSHADER_symbol *symbols;
    unsigned int instruction_count;
    MOJOSHADER_preshaderInstruction *instructions;
} MOJOSHADER_preshader;

/*
 * Structure used to return data from parsing of a shader...
 */
/* !!! FIXME: most of these ints should be unsigned. */
typedef struct MOJOSHADER_parseData
{
    /*
     * The number of elements pointed to by (errors).
     */
    int error_count;

    /*
     * (error_count) elements of data that specify errors that were generated
     *  by parsing this shader.
     * This can be NULL if there were no errors or if (error_count) is zero.
     */
    MOJOSHADER_error *errors;

    /*
     * The name of the profile used to parse the shader. Will be NULL on error.
     */
    const char *profile;

    /*
     * Bytes of output from parsing. Most profiles produce a string of source
     *  code, but profiles that do binary output may not be text at all.
     *  Will be NULL on error.
     */
    const char *output;

    /*
     * Byte count for output, not counting any null terminator. Most profiles
     *  produce an ASCII string of source code (which will be null-terminated
     *  even though that null char isn't included in output_len), but profiles
     *  that do binary output may not be text at all. Will be 0 on error.
     */
    int output_len;

    /*
     * Count of Direct3D instruction slots used. This is meaningless in terms
     *  of the actual output, as the profile will probably grow or reduce
     *  the count (or for high-level languages, not have that information at
     *  all). Also, as with Microsoft's own assembler, this value is just a
     *  rough estimate, as unpredicable real-world factors make the actual
     *  value vary at least a little from this count. Still, it can give you
     *  a rough idea of the size of your shader. Will be zero on error.
     */
    int instruction_count;

    /*
     * The type of shader we parsed. Will be MOJOSHADER_TYPE_UNKNOWN on error.
     */
    MOJOSHADER_shaderType shader_type;

    /*
     * The shader's major version. If this was a "vs_3_0", this would be 3.
     */
    int major_ver;

    /*
     * The shader's minor version. If this was a "ps_1_4", this would be 4.
     *  Two notes: for "vs_2_x", this is 1, and for "vs_3_sw", this is 255.
     */
    int minor_ver;

    /*
     * The number of elements pointed to by (uniforms).
     */
    int uniform_count;

    /*
     * (uniform_count) elements of data that specify Uniforms to be set for
     *  this shader. See discussion on MOJOSHADER_uniform for details.
     * This can be NULL on error or if (uniform_count) is zero.
     */
    MOJOSHADER_uniform *uniforms;

    /*
     * The number of elements pointed to by (constants).
     */
    int constant_count;

    /*
     * (constant_count) elements of data that specify constants used in
     *  this shader. See discussion on MOJOSHADER_constant for details.
     * This can be NULL on error or if (constant_count) is zero.
     *  This is largely informational: constants are hardcoded into a shader.
     *  The constants that you can set like parameters are in the "uniforms"
     *  list.
     */
    MOJOSHADER_constant *constants;

    /*
     * The number of elements pointed to by (samplers).
     */
    int sampler_count;

    /*
     * (sampler_count) elements of data that specify Samplers to be set for
     *  this shader. See discussion on MOJOSHADER_sampler for details.
     * This can be NULL on error or if (sampler_count) is zero.
     */
    MOJOSHADER_sampler *samplers;

    /* !!! FIXME: this should probably be "input" and not "attribute" */
    /*
     * The number of elements pointed to by (attributes).
     */
    int attribute_count;

    /* !!! FIXME: this should probably be "input" and not "attribute" */
    /*
     * (attribute_count) elements of data that specify Attributes to be set
     *  for this shader. See discussion on MOJOSHADER_attribute for details.
     * This can be NULL on error or if (attribute_count) is zero.
     */
    MOJOSHADER_attribute *attributes;

    /*
     * The number of elements pointed to by (outputs).
     */
    int output_count;

    /*
     * (output_count) elements of data that specify outputs this shader
     *  writes to. See discussion on MOJOSHADER_attribute for details.
     * This can be NULL on error or if (output_count) is zero.
     */
    MOJOSHADER_attribute *outputs;

    /*
     * The number of elements pointed to by (swizzles).
     */
    int swizzle_count;

    /* !!! FIXME: this should probably be "input" and not "attribute" */
    /*
     * (swizzle_count) elements of data that specify swizzles the shader will
     *  apply to incoming attributes. This is a copy of what was passed to
     *  MOJOSHADER_parseData().
     * This can be NULL on error or if (swizzle_count) is zero.
     */
    MOJOSHADER_swizzle *swizzles;

    /*
     * The number of elements pointed to by (symbols).
     */
    int symbol_count;

    /*
     * (symbol_count) elements of data that specify high-level symbol data
     *  for the shader. This will be parsed from the CTAB section
     *  in bytecode, and will be a copy of what you provide to
     *  MOJOSHADER_assemble(). This data is optional.
     * This can be NULL on error or if (symbol_count) is zero.
     */
    MOJOSHADER_symbol *symbols;

    /*
     * !!! FIXME: document me.
     * This can be NULL on error or if no preshader was available.
     */
    MOJOSHADER_preshader *preshader;

    /*
     * This is the malloc implementation you passed to MOJOSHADER_parse().
     */
    MOJOSHADER_malloc malloc;

    /*
     * This is the free implementation you passed to MOJOSHADER_parse().
     */
    MOJOSHADER_free free;

    /*
     * This is the pointer you passed as opaque data for your allocator.
     */
    void *malloc_data;
} MOJOSHADER_parseData;


/*
 * Profile string for Direct3D assembly language output.
 */
#define MOJOSHADER_PROFILE_D3D "d3d"

/*
 * Profile string for passthrough of the original bytecode, unchanged.
 */
#define MOJOSHADER_PROFILE_BYTECODE "bytecode"

/*
 * Profile string for GLSL: OpenGL high-level shader language output.
 */
#define MOJOSHADER_PROFILE_GLSL "glsl"

/*
 * Profile string for GLSL 1.20: minor improvements to base GLSL spec.
 */
#define MOJOSHADER_PROFILE_GLSL120 "glsl120"

/*
 * Profile string for OpenGL ARB 1.0 shaders: GL_ARB_(vertex|fragment)_program.
 */
#define MOJOSHADER_PROFILE_ARB1 "arb1"

/*
 * Profile string for OpenGL ARB 1.0 shaders with Nvidia 2.0 extensions:
 *  GL_NV_vertex_program2_option and GL_NV_fragment_program2
 */
#define MOJOSHADER_PROFILE_NV2 "nv2"

/*
 * Profile string for OpenGL ARB 1.0 shaders with Nvidia 3.0 extensions:
 *  GL_NV_vertex_program3 and GL_NV_fragment_program2
 */
#define MOJOSHADER_PROFILE_NV3 "nv3"

/*
 * Profile string for OpenGL ARB 1.0 shaders with Nvidia 4.0 extensions:
 *  GL_NV_gpu_program4
 */
#define MOJOSHADER_PROFILE_NV4 "nv4"

/*
 * Determine the highest supported Shader Model for a profile.
 */
int MOJOSHADER_maxShaderModel(const char *profile);

const MOJOSHADER_parseData *MOJOSHADER_parseExpression(const unsigned char *tokenbuf,
                                      const unsigned int bufsize,
                                      MOJOSHADER_malloc m,
									  MOJOSHADER_free f, void *d);


void MOJOSHADER_runPreshader(const MOJOSHADER_preshader*, const float*, float*);

/*
 * Parse a compiled Direct3D shader's bytecode.
 *
 * This is your primary entry point into MojoShader. You need to pass it
 *  a compiled D3D shader and tell it which "profile" you want to use to
 *  convert it into useful data.
 *
 * The available profiles are the set of MOJOSHADER_PROFILE_* defines.
 *  Note that MojoShader may be built without support for all listed
 *  profiles (in which case using one here will return with an error).
 *
 * As parsing requires some memory to be allocated, you may provide a custom
 *  allocator to this function, which will be used to allocate/free memory.
 *  They function just like malloc() and free(). We do not use realloc().
 *  If you don't care, pass NULL in for the allocator functions. If your
 *  allocator needs instance-specific data, you may supply it with the
 *  (d) parameter. This pointer is passed as-is to your (m) and (f) functions.
 *
 * This function returns a MOJOSHADER_parseData.
 *
 * This function will never return NULL, even if the system is completely
 *  out of memory upon entry (in which case, this function returns a static
 *  MOJOSHADER_parseData object, which is still safe to pass to
 *  MOJOSHADER_freeParseData()).
 *
 * You can tell the generated program to swizzle certain inputs. If you know
 *  that COLOR0 should be RGBA but you're passing in ARGB, you can specify
 *  a swizzle of { MOJOSHADER_USAGE_COLOR, 0, {1,2,3,0} } to (swiz). If the
 *  input register in the code would produce reg.ywzx, that swizzle would
 *  change it to reg.wzxy ... (swiz) can be NULL.
 *
 * This function is thread safe, so long as (m) and (f) are too, and that
 *  (tokenbuf) remains intact for the duration of the call. This allows you
 *  to parse several shaders on separate CPU cores at the same time.
 */
const MOJOSHADER_parseData *MOJOSHADER_parse(const char *profile,
                                             const unsigned char *tokenbuf,
                                             const unsigned int bufsize,
                                             const MOJOSHADER_swizzle *swiz,
                                             const unsigned int swizcount,
                                             MOJOSHADER_malloc m,
                                             MOJOSHADER_free f,
                                             void *d);

/*
 * Call this to dispose of parsing results when you are done with them.
 *  This will call the MOJOSHADER_free function you provided to
 *  MOJOSHADER_parse multiple times, if you provided one.
 *  Passing a NULL here is a safe no-op.
 *
 * This function is thread safe, so long as any allocator you passed into
 *  MOJOSHADER_parse() is, too.
 */
void MOJOSHADER_freeParseData(const MOJOSHADER_parseData *data);


/* Effects interface... */  /* !!! FIXME: THIS API IS NOT STABLE YET! */

typedef struct MOJOSHADER_effectParam
{
    const char *name;
    const char *semantic;
} MOJOSHADER_effectParam;

typedef struct MOJOSHADER_effectState
{
    unsigned int type;
} MOJOSHADER_effectState;

typedef struct MOJOSHADER_effectPass
{
    const char *name;
    unsigned int state_count;
    MOJOSHADER_effectState *states;
} MOJOSHADER_effectPass;

typedef struct MOJOSHADER_effectTechnique
{
    const char *name;
    unsigned int pass_count;
    MOJOSHADER_effectPass *passes;
} MOJOSHADER_effectTechnique;

typedef struct MOJOSHADER_effectTexture
{
    unsigned int param;
    const char *name;
} MOJOSHADER_effectTexture;

typedef struct MOJOSHADER_effectShader
{
    unsigned int technique;
    unsigned int pass;
    const MOJOSHADER_parseData *shader;
} MOJOSHADER_effectShader;

/*
 * Structure used to return data from parsing of an effect file...
 */
/* !!! FIXME: most of these ints should be unsigned. */
typedef struct MOJOSHADER_effect
{
    /*
     * The number of elements pointed to by (errors).
     */
    int error_count;

    /*
     * (error_count) elements of data that specify errors that were generated
     *  by parsing this shader.
     * This can be NULL if there were no errors or if (error_count) is zero.
     */
    MOJOSHADER_error *errors;

    /*
     * The name of the profile used to parse the shader. Will be NULL on error.
     */
    const char *profile;

    /*
     * The number of params pointed to by (params).
     */
    int param_count;

    /*
     * (param_count) elements of data that specify parameter bind points for
     *  this effect.
     * This can be NULL on error or if (param_count) is zero.
     */
    MOJOSHADER_effectParam *params;

    /*
     * The number of elements pointed to by (techniques).
     */
    int technique_count;

    /*
     * (technique_count) elements of data that specify techniques used in
     *  this effect. Each technique contains a series of passes, and each pass
     *  specifies state and shaders that affect rendering.
     * This can be NULL on error or if (technique_count) is zero.
     */
    MOJOSHADER_effectTechnique *techniques;

    /*
     * The number of elements pointed to by (textures).
     */
    int texture_count;

    /*
     * (texture_count) elements of data that specify textures used in
     *  this effect.
     * This can be NULL on error or if (texture_count) is zero.
     */
    MOJOSHADER_effectTexture *textures;

    /*
     * The number of elements pointed to by (shaders).
     */
    int shader_count;

    /*
     * (shader_count) elements of data that specify shaders used in
     *  this effect.
     * This can be NULL on error or if (shader_count) is zero.
     */
    MOJOSHADER_effectShader *shaders;

    /*
     * This is the malloc implementation you passed to MOJOSHADER_parseEffect().
     */
    MOJOSHADER_malloc malloc;

    /*
     * This is the free implementation you passed to MOJOSHADER_parseEffect().
     */
    MOJOSHADER_free free;

    /*
     * This is the pointer you passed as opaque data for your allocator.
     */
    void *malloc_data;
} MOJOSHADER_effect;

/* !!! FIXME: document me. */
const MOJOSHADER_effect *MOJOSHADER_parseEffect(const char *profile,
                                                const unsigned char *buf,
                                                const unsigned int _len,
                                                const MOJOSHADER_swizzle *swiz,
                                                const unsigned int swizcount,
                                                MOJOSHADER_malloc m,
                                                MOJOSHADER_free f,
                                                void *d);


/* !!! FIXME: document me. */
void MOJOSHADER_freeEffect(const MOJOSHADER_effect *effect);


/* Preprocessor interface... */

/*
 * Structure used to pass predefined macros. Maps to D3DXMACRO.
 *  You can have macro arguments: set identifier to "a(b, c)" or whatever.
 */
typedef struct MOJOSHADER_preprocessorDefine
{
    const char *identifier;
    const char *definition;
} MOJOSHADER_preprocessorDefine;

/*
 * Used with the MOJOSHADER_includeOpen callback. Maps to D3DXINCLUDE_TYPE.
 */
typedef enum
{
    MOJOSHADER_INCLUDETYPE_LOCAL,   /* local header: #include "blah.h" */
    MOJOSHADER_INCLUDETYPE_SYSTEM   /* system header: #include <blah.h> */
} MOJOSHADER_includeType;


/*
 * Structure used to return data from preprocessing of a shader...
 */
/* !!! FIXME: most of these ints should be unsigned. */
typedef struct MOJOSHADER_preprocessData
{
    /*
     * The number of elements pointed to by (errors).
     */
    int error_count;

    /*
     * (error_count) elements of data that specify errors that were generated
     *  by parsing this shader.
     * This can be NULL if there were no errors or if (error_count) is zero.
     */
    MOJOSHADER_error *errors;

    /*
     * Bytes of output from preprocessing. This is a UTF-8 string. We
     *  guarantee it to be NULL-terminated. Will be NULL on error.
     */
    const char *output;

    /*
     * Byte count for output, not counting any null terminator.
     *  Will be 0 on error.
     */
    int output_len;

    /*
     * This is the malloc implementation you passed to MOJOSHADER_parse().
     */
    MOJOSHADER_malloc malloc;

    /*
     * This is the free implementation you passed to MOJOSHADER_parse().
     */
    MOJOSHADER_free free;

    /*
     * This is the pointer you passed as opaque data for your allocator.
     */
    void *malloc_data;
} MOJOSHADER_preprocessData;


/*
 * This callback allows an app to handle #include statements for the
 *  preprocessor. When the preprocessor sees an #include, it will call this
 *  function to obtain the contents of the requested file. This is optional;
 *  the preprocessor will open files directly if no callback is supplied, but
 *  this allows an app to retrieve data from something other than the
 *  traditional filesystem (for example, headers packed in a .zip file or
 *  headers generated on-the-fly).
 *
 * This function maps to ID3DXInclude::Open()
 *
 * (inctype) specifies the type of header we wish to include.
 * (fname) specifies the name of the file specified on the #include line.
 * (parent) is a string of the entire source file containing the include, in
 *  its original, not-yet-preprocessed state. Note that this is just the
 *  contents of the specific file, not all source code that the preprocessor
 *  has seen through other includes, etc.
 * (outdata) will be set by the callback to a pointer to the included file's
 *  contents. The callback is responsible for allocating this however they
 *  see fit (we provide allocator functions, but you may ignore them). This
 *  pointer must remain valid until the includeClose callback runs. This
 *  string does not need to be NULL-terminated.
 * (outbytes) will be set by the callback to the number of bytes pointed to
 *  by (outdata).
 * (m),(f), and (d) are the allocator details that the application passed to
 *  MojoShader. If these were NULL, MojoShader may have replaced them with its
 *  own internal allocators.
 *
 * The callback returns zero on error, non-zero on success.
 *
 * If you supply an includeOpen callback, you must supply includeClose, too.
 */
typedef int (*MOJOSHADER_includeOpen)(MOJOSHADER_includeType inctype,
                            const char *fname, const char *parent,
                            const char **outdata, unsigned int *outbytes,
                            MOJOSHADER_malloc m, MOJOSHADER_free f, void *d);

/*
 * This callback allows an app to clean up the results of a previous
 *  includeOpen callback.
 *
 * This function maps to ID3DXInclude::Close()
 *
 * (data) is the data that was returned from a previous call to includeOpen.
 *  It is now safe to deallocate this data.
 * (m),(f), and (d) are the same allocator details that were passed to your
 *  includeOpen callback.
 *
 * If you supply an includeClose callback, you must supply includeOpen, too.
 */
typedef void (*MOJOSHADER_includeClose)(const char *data,
                            MOJOSHADER_malloc m, MOJOSHADER_free f, void *d);


/*
 * This function is optional. Even if you are dealing with shader source
 *  code, you don't need to explicitly use the preprocessor, as the compiler
 *  and assembler will use it behind the scenes. In fact, you probably never
 *  need this function unless you are debugging a custom tool (or debugging
 *  MojoShader itself).
 *
 * Preprocessing roughly follows the syntax of an ANSI C preprocessor, as
 *  Microsoft's Direct3D assembler and HLSL compiler use this syntax. Please
 *  note that we try to match the output you'd get from Direct3D's
 *  preprocessor, which has some quirks if you're expecting output that matches
 *  a generic C preprocessor.
 *
 * This function maps to D3DXPreprocessShader().
 *
 * (filename) is a NULL-terminated UTF-8 filename. It can be NULL. We do not
 *  actually access this file, as we obtain our data from (source). This
 *  string is copied when we need to report errors while processing (source),
 *  as opposed to errors in a file referenced via the #include directive in
 *  (source). If this is NULL, then errors will report the filename as NULL,
 *  too.
 *
 * (source) is an string of UTF-8 text to preprocess. It does not need to be
 *  NULL-terminated.
 *
 * (sourcelen) is the length of the string pointed to by (source), in bytes.
 *
 * (defines) points to (define_count) preprocessor definitions, and can be
 *  NULL. These are treated by the preprocessor as if the source code started
 *  with one #define for each entry you pass in here.
 *
 * (include_open) and (include_close) let the app control the preprocessor's
 *  behaviour for #include statements. Both are optional and can be NULL, but
 *  both must be specified if either is specified.
 *
 * This will return a MOJOSHADER_preprocessorData. You should pass this
 *  return value to MOJOSHADER_freePreprocessData() when you are done with
 *  it.
 *
 * This function will never return NULL, even if the system is completely
 *  out of memory upon entry (in which case, this function returns a static
 *  MOJOSHADER_preprocessData object, which is still safe to pass to
 *  MOJOSHADER_freePreprocessData()).
 *
 * As preprocessing requires some memory to be allocated, you may provide a
 *  custom allocator to this function, which will be used to allocate/free
 *  memory. They function just like malloc() and free(). We do not use
 *  realloc(). If you don't care, pass NULL in for the allocator functions.
 *  If your allocator needs instance-specific data, you may supply it with the
 *  (d) parameter. This pointer is passed as-is to your (m) and (f) functions.
 *
 * This function is thread safe, so long as the various callback functions
 *  are, too, and that the parameters remains intact for the duration of the
 *  call. This allows you to preprocess several shaders on separate CPU cores
 *  at the same time.
 */
const MOJOSHADER_preprocessData *MOJOSHADER_preprocess(const char *filename,
                             const char *source, unsigned int sourcelen,
                             const MOJOSHADER_preprocessorDefine *defines,
                             unsigned int define_count,
                             MOJOSHADER_includeOpen include_open,
                             MOJOSHADER_includeClose include_close,
                             MOJOSHADER_malloc m, MOJOSHADER_free f, void *d);


/*
 * Call this to dispose of preprocessing results when you are done with them.
 *  This will call the MOJOSHADER_free function you provided to
 *  MOJOSHADER_preprocess() multiple times, if you provided one.
 *  Passing a NULL here is a safe no-op.
 *
 * This function is thread safe, so long as any allocator you passed into
 *  MOJOSHADER_preprocess() is, too.
 */
void MOJOSHADER_freePreprocessData(const MOJOSHADER_preprocessData *data);


/* Assembler interface... */

/*
 * This function is optional. Use this to convert Direct3D shader assembly
 *  language into bytecode, which can be handled by MOJOSHADER_parse().
 *
 * (filename) is a NULL-terminated UTF-8 filename. It can be NULL. We do not
 *  actually access this file, as we obtain our data from (source). This
 *  string is copied when we need to report errors while processing (source),
 *  as opposed to errors in a file referenced via the #include directive in
 *  (source). If this is NULL, then errors will report the filename as NULL,
 *  too.
 *
 * (source) is an UTF-8 string of valid Direct3D shader assembly source code.
 *  It does not need to be NULL-terminated.
 *
 * (sourcelen) is the length of the string pointed to by (source), in bytes.
 *
 * (comments) points to (comment_count) NULL-terminated UTF-8 strings, and
 *  can be NULL. These strings are inserted as comments in the bytecode.
 *
 * (symbols) points to (symbol_count) symbol structs, and can be NULL. These
 *  become a CTAB field in the bytecode. This is optional, but
 *  MOJOSHADER_parse() needs CTAB data for all arrays used in a program, or
 *  relative addressing will not be permitted, so you'll want to at least
 *  provide symbol information for those. The symbol data is 100% trusted
 *  at this time; it will not be checked to see if it matches what was
 *  assembled in any way whatsoever.
 *
 * (defines) points to (define_count) preprocessor definitions, and can be
 *  NULL. These are treated by the preprocessor as if the source code started
 *  with one #define for each entry you pass in here.
 *
 * (include_open) and (include_close) let the app control the preprocessor's
 *  behaviour for #include statements. Both are optional and can be NULL, but
 *  both must be specified if either is specified.
 *
 * This will return a MOJOSHADER_parseData, like MOJOSHADER_parse() would,
 *  except the profile will be MOJOSHADER_PROFILE_BYTECODE and the output
 *  will be the assembled bytecode instead of some other language. This output
 *  can be pushed back through MOJOSHADER_parseData() with a different profile.
 *
 * This function will never return NULL, even if the system is completely
 *  out of memory upon entry (in which case, this function returns a static
 *  MOJOSHADER_parseData object, which is still safe to pass to
 *  MOJOSHADER_freeParseData()).
 *
 * As assembling requires some memory to be allocated, you may provide a
 *  custom allocator to this function, which will be used to allocate/free
 *  memory. They function just like malloc() and free(). We do not use
 *  realloc(). If you don't care, pass NULL in for the allocator functions.
 *  If your allocator needs instance-specific data, you may supply it with the
 *  (d) parameter. This pointer is passed as-is to your (m) and (f) functions.
 *
 * This function is thread safe, so long as the various callback functions
 *  are, too, and that the parameters remains intact for the duration of the
 *  call. This allows you to assemble several shaders on separate CPU cores
 *  at the same time.
 */
const MOJOSHADER_parseData *MOJOSHADER_assemble(const char *filename,
                             const char *source, unsigned int sourcelen,
                             const char **comments, unsigned int comment_count,
                             const MOJOSHADER_symbol *symbols,
                             unsigned int symbol_count,
                             const MOJOSHADER_preprocessorDefine *defines,
                             unsigned int define_count,
                             MOJOSHADER_includeOpen include_open,
                             MOJOSHADER_includeClose include_close,
                             MOJOSHADER_malloc m, MOJOSHADER_free f, void *d);


/* High level shading language support... */

/*
 * Source profile strings for HLSL: Direct3D High Level Shading Language.
 */
#define MOJOSHADER_SRC_PROFILE_HLSL_VS_1_1 "hlsl_vs_1_1"
#define MOJOSHADER_SRC_PROFILE_HLSL_VS_2_0 "hlsl_vs_2_0"
#define MOJOSHADER_SRC_PROFILE_HLSL_VS_3_0 "hlsl_vs_3_0"
#define MOJOSHADER_SRC_PROFILE_HLSL_PS_1_1 "hlsl_ps_1_1"
#define MOJOSHADER_SRC_PROFILE_HLSL_PS_1_2 "hlsl_ps_1_2"
#define MOJOSHADER_SRC_PROFILE_HLSL_PS_1_3 "hlsl_ps_1_3"
#define MOJOSHADER_SRC_PROFILE_HLSL_PS_1_4 "hlsl_ps_1_4"
#define MOJOSHADER_SRC_PROFILE_HLSL_PS_2_0 "hlsl_ps_2_0"
#define MOJOSHADER_SRC_PROFILE_HLSL_PS_3_0 "hlsl_ps_3_0"


/* Abstract Syntax Tree interface... */

/*
 * ATTENTION: This adds a lot of stuff to the API, but almost everyone can
 *  ignore this section. Seriously, go ahead and skip over anything that has
 *  "AST" in it, unless you know why you'd want to use it.
 *
 * ALSO: This API is still evolving! We make no promises at this time to keep
 *  source or binary compatibility for the AST pieces.
 *
 * Important notes:
 *  - ASTs are the result of parsing the source code: a program that fails to
 *    compile will often parse successfully. Undeclared variables,
 *    type incompatibilities, etc, aren't detected at this point.
 *  - Vector swizzles (the ".xyzw" part of "MyVec4.xyzw") will look like
 *    structure dereferences. We don't realize these are actually swizzles
 *    until semantic analysis.
 *  - MOJOSHADER_astDataType info is not reliable when returned from
 *    MOJOSHADER_parseAst()! Most of the datatype info will be missing or have
 *    inaccurate data types. We sort these out during semantic analysis, which
 *    happens after the AST parsing is complete. A few are filled in, or can
 *    be deduced fairly trivially by processing several pieces into one.
 *    It's enough that you can reproduce the original source code, more or
 *    less, from the AST.
 */

/* High-level datatypes for AST nodes. */
typedef enum MOJOSHADER_astDataTypeType
{
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
} MOJOSHADER_astDataTypeType;
#define MOJOSHADER_AST_DATATYPE_CONST (1 << 31)

typedef union MOJOSHADER_astDataType MOJOSHADER_astDataType;

// This is just part of DataTypeStruct, never appears outside of it.
typedef struct MOJOSHADER_astDataTypeStructMember
{
    const MOJOSHADER_astDataType *datatype;
    const char *identifier;
} MOJOSHADER_astDataTypeStructMember;

typedef struct MOJOSHADER_astDataTypeStruct
{
    MOJOSHADER_astDataTypeType type;
    const MOJOSHADER_astDataTypeStructMember *members;
    int member_count;
} MOJOSHADER_astDataTypeStruct;

typedef struct MOJOSHADER_astDataTypeArray
{
    MOJOSHADER_astDataTypeType type;
    const MOJOSHADER_astDataType *base;
    int elements;
} MOJOSHADER_astDataTypeArray;

typedef MOJOSHADER_astDataTypeArray MOJOSHADER_astDataTypeVector;

typedef struct MOJOSHADER_astDataTypeMatrix
{
    MOJOSHADER_astDataTypeType type;
    const MOJOSHADER_astDataType *base;
    int rows;
    int columns;
} MOJOSHADER_astDataTypeMatrix;

typedef struct MOJOSHADER_astDataTypeBuffer
{
    MOJOSHADER_astDataTypeType type;
    const MOJOSHADER_astDataType *base;
} MOJOSHADER_astDataTypeBuffer;

typedef struct MOJOSHADER_astDataTypeFunction
{
    MOJOSHADER_astDataTypeType type;
    const MOJOSHADER_astDataType *retval;
    const MOJOSHADER_astDataType **params;
    int num_params;
    int intrinsic;  /* non-zero for built-in functions */
} MOJOSHADER_astDataTypeFunction;

typedef struct MOJOSHADER_astDataTypeUser
{
    MOJOSHADER_astDataTypeType type;
    const MOJOSHADER_astDataType *details;
    const char *name;
} MOJOSHADER_astDataTypeUser;

union MOJOSHADER_astDataType
{
    MOJOSHADER_astDataTypeType type;
    MOJOSHADER_astDataTypeArray array;
    MOJOSHADER_astDataTypeStruct structure;
    MOJOSHADER_astDataTypeVector vector;
    MOJOSHADER_astDataTypeMatrix matrix;
    MOJOSHADER_astDataTypeBuffer buffer;
    MOJOSHADER_astDataTypeUser user;
    MOJOSHADER_astDataTypeFunction function;
};

/* Structures that make up the parse tree... */

typedef enum MOJOSHADER_astNodeType
{
    MOJOSHADER_AST_OP_START_RANGE,         /* expression operators. */

    MOJOSHADER_AST_OP_START_RANGE_UNARY,   /* unary operators. */
    MOJOSHADER_AST_OP_PREINCREMENT,
    MOJOSHADER_AST_OP_PREDECREMENT,
    MOJOSHADER_AST_OP_NEGATE,
    MOJOSHADER_AST_OP_COMPLEMENT,
    MOJOSHADER_AST_OP_NOT,
    MOJOSHADER_AST_OP_POSTINCREMENT,
    MOJOSHADER_AST_OP_POSTDECREMENT,
    MOJOSHADER_AST_OP_CAST,
    MOJOSHADER_AST_OP_END_RANGE_UNARY,

    MOJOSHADER_AST_OP_START_RANGE_BINARY,  /* binary operators. */
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

    MOJOSHADER_AST_OP_START_RANGE_TERNARY,  /* ternary operators. */
    MOJOSHADER_AST_OP_CONDITIONAL,
    MOJOSHADER_AST_OP_END_RANGE_TERNARY,

    MOJOSHADER_AST_OP_START_RANGE_DATA,     /* expression operands. */
    MOJOSHADER_AST_OP_IDENTIFIER,
    MOJOSHADER_AST_OP_INT_LITERAL,
    MOJOSHADER_AST_OP_FLOAT_LITERAL,
    MOJOSHADER_AST_OP_STRING_LITERAL,
    MOJOSHADER_AST_OP_BOOLEAN_LITERAL,
    MOJOSHADER_AST_OP_END_RANGE_DATA,

    MOJOSHADER_AST_OP_START_RANGE_MISC,     /* other expression things. */
    MOJOSHADER_AST_OP_DEREF_STRUCT,
    MOJOSHADER_AST_OP_CALLFUNC,
    MOJOSHADER_AST_OP_CONSTRUCTOR,
    MOJOSHADER_AST_OP_END_RANGE_MISC,
    MOJOSHADER_AST_OP_END_RANGE,

    MOJOSHADER_AST_COMPUNIT_START_RANGE,    /* things in global scope. */
    MOJOSHADER_AST_COMPUNIT_FUNCTION,
    MOJOSHADER_AST_COMPUNIT_TYPEDEF,
    MOJOSHADER_AST_COMPUNIT_STRUCT,
    MOJOSHADER_AST_COMPUNIT_VARIABLE,
    MOJOSHADER_AST_COMPUNIT_END_RANGE,

    MOJOSHADER_AST_STATEMENT_START_RANGE,   /* statements in function scope. */
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

    MOJOSHADER_AST_MISC_START_RANGE,        /* misc. syntactic glue. */
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

    MOJOSHADER_AST_END_RANGE
} MOJOSHADER_astNodeType;

typedef struct MOJOSHADER_astNodeInfo
{
    MOJOSHADER_astNodeType type;
    const char *filename;
    unsigned int line;
} MOJOSHADER_astNodeInfo;

typedef enum MOJOSHADER_astVariableAttributes
{
    MOJOSHADER_AST_VARATTR_EXTERN = (1 << 0),
    MOJOSHADER_AST_VARATTR_NOINTERPOLATION = (1 << 1),
    MOJOSHADER_AST_VARATTR_SHARED = (1 << 2),
    MOJOSHADER_AST_VARATTR_STATIC = (1 << 3),
    MOJOSHADER_AST_VARATTR_UNIFORM = (1 << 4),
    MOJOSHADER_AST_VARATTR_VOLATILE = (1 << 5),
    MOJOSHADER_AST_VARATTR_CONST = (1 << 6),
    MOJOSHADER_AST_VARATTR_ROWMAJOR = (1 << 7),
    MOJOSHADER_AST_VARATTR_COLUMNMAJOR = (1 << 8)
} MOJOSHADER_astVariableAttributes;

typedef enum MOJOSHADER_astIfAttributes
{
    MOJOSHADER_AST_IFATTR_NONE,
    MOJOSHADER_AST_IFATTR_BRANCH,
    MOJOSHADER_AST_IFATTR_FLATTEN,
    MOJOSHADER_AST_IFATTR_IFALL,
    MOJOSHADER_AST_IFATTR_IFANY,
    MOJOSHADER_AST_IFATTR_PREDICATE,
    MOJOSHADER_AST_IFATTR_PREDICATEBLOCK,
} MOJOSHADER_astIfAttributes;

typedef enum MOJOSHADER_astSwitchAttributes
{
    MOJOSHADER_AST_SWITCHATTR_NONE,
    MOJOSHADER_AST_SWITCHATTR_FLATTEN,
    MOJOSHADER_AST_SWITCHATTR_BRANCH,
    MOJOSHADER_AST_SWITCHATTR_FORCECASE,
    MOJOSHADER_AST_SWITCHATTR_CALL
} MOJOSHADER_astSwitchAttributes;

/* You can cast any AST node pointer to this. */
typedef struct MOJOSHADER_astGeneric
{
    MOJOSHADER_astNodeInfo ast;
} MOJOSHADER_astGeneric;

typedef struct MOJOSHADER_astExpression
{
    MOJOSHADER_astNodeInfo ast;
    const MOJOSHADER_astDataType *datatype;
} MOJOSHADER_astExpression;

typedef struct MOJOSHADER_astArguments
{
    MOJOSHADER_astNodeInfo ast;  /* Always MOJOSHADER_AST_ARGUMENTS */
    MOJOSHADER_astExpression *argument;
    struct MOJOSHADER_astArguments *next;
} MOJOSHADER_astArguments;

typedef struct MOJOSHADER_astExpressionUnary
{
    MOJOSHADER_astNodeInfo ast;
    const MOJOSHADER_astDataType *datatype;
    MOJOSHADER_astExpression *operand;
} MOJOSHADER_astExpressionUnary;

typedef struct MOJOSHADER_astExpressionBinary
{
    MOJOSHADER_astNodeInfo ast;
    const MOJOSHADER_astDataType *datatype;
    MOJOSHADER_astExpression *left;
    MOJOSHADER_astExpression *right;
} MOJOSHADER_astExpressionBinary;

typedef struct MOJOSHADER_astExpressionTernary
{
    MOJOSHADER_astNodeInfo ast;
    const MOJOSHADER_astDataType *datatype;
    MOJOSHADER_astExpression *left;
    MOJOSHADER_astExpression *center;
    MOJOSHADER_astExpression *right;
} MOJOSHADER_astExpressionTernary;

/* Identifier indexes aren't available until semantic analysis phase completes.
 *  It provides a unique id for this identifier's variable.
 *  It will be negative for global scope, positive for function scope
 *  (global values are globally unique, function values are only
 *  unique within the scope of the given function). There's a different
 *  set of indices if this identifier is a function (positive for
 *  user-defined functions, negative for intrinsics).
 *  May be zero for various reasons (unknown identifier, etc).
 */
typedef struct MOJOSHADER_astExpressionIdentifier
{
    MOJOSHADER_astNodeInfo ast;  /* Always MOJOSHADER_AST_OP_IDENTIFIER */
    const MOJOSHADER_astDataType *datatype;
    const char *identifier;
    int index;
} MOJOSHADER_astExpressionIdentifier;

typedef struct MOJOSHADER_astExpressionIntLiteral
{
    MOJOSHADER_astNodeInfo ast;  /* Always MOJOSHADER_AST_OP_INT_LITERAL */
    const MOJOSHADER_astDataType *datatype;  /* always AST_DATATYPE_INT */
    int value;
} MOJOSHADER_astExpressionIntLiteral;

typedef struct MOJOSHADER_astExpressionFloatLiteral
{
    MOJOSHADER_astNodeInfo ast;  /* Always MOJOSHADER_AST_OP_FLOAT_LITERAL */
    const MOJOSHADER_astDataType *datatype;  /* always AST_DATATYPE_FLOAT */
    double value;
} MOJOSHADER_astExpressionFloatLiteral;

typedef struct MOJOSHADER_astExpressionStringLiteral
{
    MOJOSHADER_astNodeInfo ast;  /* Always MOJOSHADER_AST_OP_STRING_LITERAL */
    const MOJOSHADER_astDataType *datatype;  /* always AST_DATATYPE_STRING */
    const char *string;
} MOJOSHADER_astExpressionStringLiteral;

typedef struct MOJOSHADER_astExpressionBooleanLiteral
{
    MOJOSHADER_astNodeInfo ast;  /* Always MOJOSHADER_AST_OP_BOOLEAN_LITERAL */
    const MOJOSHADER_astDataType *datatype;  /* always AST_DATATYPE_BOOL */
    int value;  /* Always 1 or 0. */
} MOJOSHADER_astExpressionBooleanLiteral;

typedef struct MOJOSHADER_astExpressionConstructor
{
    MOJOSHADER_astNodeInfo ast;  /* Always MOJOSHADER_AST_OP_CONSTRUCTOR */
    const MOJOSHADER_astDataType *datatype;
    MOJOSHADER_astArguments *args;
} MOJOSHADER_astExpressionConstructor;

typedef struct MOJOSHADER_astExpressionDerefStruct
{
    MOJOSHADER_astNodeInfo ast;  /* Always MOJOSHADER_AST_OP_DEREF_STRUCT */
    const MOJOSHADER_astDataType *datatype;
    /* !!! FIXME:
     *  "identifier" is misnamed; this might not be an identifier at all:
     *    x = FunctionThatReturnsAStruct().SomeMember;
     */
    MOJOSHADER_astExpression *identifier;
    const char *member;
    int isswizzle;  /* Always 1 or 0. Never set by parseAst()! */
    int member_index;  /* Never set by parseAst()! */
} MOJOSHADER_astExpressionDerefStruct;

typedef struct MOJOSHADER_astExpressionCallFunction
{
    MOJOSHADER_astNodeInfo ast;  /* Always MOJOSHADER_AST_OP_CALLFUNC */
    const MOJOSHADER_astDataType *datatype;
    MOJOSHADER_astExpressionIdentifier *identifier;
    MOJOSHADER_astArguments *args;
} MOJOSHADER_astExpressionCallFunction;

typedef struct MOJOSHADER_astExpressionCast
{
    MOJOSHADER_astNodeInfo ast;  /* Always MOJOSHADER_AST_OP_CAST */
    const MOJOSHADER_astDataType *datatype;
    MOJOSHADER_astExpression *operand;
} MOJOSHADER_astExpressionCast;

typedef struct MOJOSHADER_astCompilationUnit
{
    MOJOSHADER_astNodeInfo ast;
    struct MOJOSHADER_astCompilationUnit *next;
} MOJOSHADER_astCompilationUnit;

typedef enum MOJOSHADER_astFunctionStorageClass
{
    MOJOSHADER_AST_FNSTORECLS_NONE,
    MOJOSHADER_AST_FNSTORECLS_INLINE
} MOJOSHADER_astFunctionStorageClass;

typedef enum MOJOSHADER_astInputModifier
{
    MOJOSHADER_AST_INPUTMOD_NONE,
    MOJOSHADER_AST_INPUTMOD_IN,
    MOJOSHADER_AST_INPUTMOD_OUT,
    MOJOSHADER_AST_INPUTMOD_INOUT,
    MOJOSHADER_AST_INPUTMOD_UNIFORM
} MOJOSHADER_astInputModifier;

typedef enum MOJOSHADER_astInterpolationModifier
{
    MOJOSHADER_AST_INTERPMOD_NONE,
    MOJOSHADER_AST_INTERPMOD_LINEAR,
    MOJOSHADER_AST_INTERPMOD_CENTROID,
    MOJOSHADER_AST_INTERPMOD_NOINTERPOLATION,
    MOJOSHADER_AST_INTERPMOD_NOPERSPECTIVE,
    MOJOSHADER_AST_INTERPMOD_SAMPLE
} MOJOSHADER_astInterpolationModifier;

typedef struct MOJOSHADER_astFunctionParameters
{
    MOJOSHADER_astNodeInfo ast;
    const MOJOSHADER_astDataType *datatype;
    MOJOSHADER_astInputModifier input_modifier;
    const char *identifier;
    const char *semantic;
    MOJOSHADER_astInterpolationModifier interpolation_modifier;
    MOJOSHADER_astExpression *initializer;
    struct MOJOSHADER_astFunctionParameters *next;
} MOJOSHADER_astFunctionParameters;

typedef struct MOJOSHADER_astFunctionSignature
{
    MOJOSHADER_astNodeInfo ast;
    const MOJOSHADER_astDataType *datatype;
    const char *identifier;
    MOJOSHADER_astFunctionParameters *params;
    MOJOSHADER_astFunctionStorageClass storage_class;
    const char *semantic;
} MOJOSHADER_astFunctionSignature;

typedef struct MOJOSHADER_astScalarOrArray
{
    MOJOSHADER_astNodeInfo ast;
    const char *identifier;
    int isarray;  /* boolean: 1 or 0 */
    MOJOSHADER_astExpression *dimension;
} MOJOSHADER_astScalarOrArray;

typedef struct MOJOSHADER_astAnnotations
{
    MOJOSHADER_astNodeInfo ast;
    const MOJOSHADER_astDataType *datatype;
    MOJOSHADER_astExpression *initializer;
    struct MOJOSHADER_astAnnotations *next;
} MOJOSHADER_astAnnotations;

typedef struct MOJOSHADER_astPackOffset
{
    MOJOSHADER_astNodeInfo ast;
    const char *ident1;   /* !!! FIXME: rename this. */
    const char *ident2;
} MOJOSHADER_astPackOffset;

typedef struct MOJOSHADER_astVariableLowLevel
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astPackOffset *packoffset;
    const char *register_name;
} MOJOSHADER_astVariableLowLevel;

typedef struct MOJOSHADER_astStructMembers
{
    MOJOSHADER_astNodeInfo ast;
    const MOJOSHADER_astDataType *datatype;
    const char *semantic;
    MOJOSHADER_astScalarOrArray *details;
    MOJOSHADER_astInterpolationModifier interpolation_mod;
    struct MOJOSHADER_astStructMembers *next;
} MOJOSHADER_astStructMembers;

typedef struct MOJOSHADER_astStructDeclaration
{
    MOJOSHADER_astNodeInfo ast;
    const MOJOSHADER_astDataType *datatype;
    const char *name;
    MOJOSHADER_astStructMembers *members;
} MOJOSHADER_astStructDeclaration;

typedef struct MOJOSHADER_astVariableDeclaration
{
    MOJOSHADER_astNodeInfo ast;
    int attributes;
    const MOJOSHADER_astDataType *datatype;
    MOJOSHADER_astStructDeclaration *anonymous_datatype;
    MOJOSHADER_astScalarOrArray *details;
    const char *semantic;
    MOJOSHADER_astAnnotations *annotations;
    MOJOSHADER_astExpression *initializer;
    MOJOSHADER_astVariableLowLevel *lowlevel;
    struct MOJOSHADER_astVariableDeclaration *next;
} MOJOSHADER_astVariableDeclaration;

typedef struct MOJOSHADER_astStatement
{
    MOJOSHADER_astNodeInfo ast;
    struct MOJOSHADER_astStatement *next;
} MOJOSHADER_astStatement;

typedef MOJOSHADER_astStatement MOJOSHADER_astEmptyStatement;
typedef MOJOSHADER_astStatement MOJOSHADER_astBreakStatement;
typedef MOJOSHADER_astStatement MOJOSHADER_astContinueStatement;
typedef MOJOSHADER_astStatement MOJOSHADER_astDiscardStatement;

/* something enclosed in "{}" braces. */
typedef struct MOJOSHADER_astBlockStatement
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astStatement *next;
    MOJOSHADER_astStatement *statements;  /* list of child statements. */
} MOJOSHADER_astBlockStatement;

typedef struct MOJOSHADER_astReturnStatement
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astStatement *next;
    MOJOSHADER_astExpression *expr;
} MOJOSHADER_astReturnStatement;

typedef struct MOJOSHADER_astExpressionStatement
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astStatement *next;
    MOJOSHADER_astExpression *expr;
} MOJOSHADER_astExpressionStatement;

typedef struct MOJOSHADER_astIfStatement
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astStatement *next;
    int attributes;
    MOJOSHADER_astExpression *expr;
    MOJOSHADER_astStatement *statement;
    MOJOSHADER_astStatement *else_statement;
} MOJOSHADER_astIfStatement;

typedef struct MOJOSHADER_astSwitchCases
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astExpression *expr;
    MOJOSHADER_astStatement *statement;
    struct MOJOSHADER_astSwitchCases *next;
} MOJOSHADER_astSwitchCases;

typedef struct MOJOSHADER_astSwitchStatement
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astStatement *next;
    int attributes;
    MOJOSHADER_astExpression *expr;
    MOJOSHADER_astSwitchCases *cases;
} MOJOSHADER_astSwitchStatement;

typedef struct MOJOSHADER_astWhileStatement
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astStatement *next;
    int unroll;  /* # times to unroll, 0 to loop, < 0 == compiler's choice. */
    MOJOSHADER_astExpression *expr;
    MOJOSHADER_astStatement *statement;
} MOJOSHADER_astWhileStatement;

typedef MOJOSHADER_astWhileStatement MOJOSHADER_astDoStatement;

typedef struct MOJOSHADER_astForStatement
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astStatement *next;
    int unroll;  /* # times to unroll, 0 to loop, < 0 == compiler's choice. */
    MOJOSHADER_astVariableDeclaration *var_decl;  /* either this ... */
    MOJOSHADER_astExpression *initializer;        /*  ... or this will used. */
    MOJOSHADER_astExpression *looptest;
    MOJOSHADER_astExpression *counter;
    MOJOSHADER_astStatement *statement;
} MOJOSHADER_astForStatement;

typedef struct MOJOSHADER_astTypedef
{
    MOJOSHADER_astNodeInfo ast;
    const MOJOSHADER_astDataType *datatype;
    int isconst;  /* boolean: 1 or 0 */
    MOJOSHADER_astScalarOrArray *details;
} MOJOSHADER_astTypedef;

typedef struct MOJOSHADER_astTypedefStatement
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astStatement *next;
    MOJOSHADER_astTypedef *type_info;
} MOJOSHADER_astTypedefStatement;

typedef struct MOJOSHADER_astVarDeclStatement
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astStatement *next;
    MOJOSHADER_astVariableDeclaration *declaration;
} MOJOSHADER_astVarDeclStatement;

typedef struct MOJOSHADER_astStructStatement
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astStatement *next;
    MOJOSHADER_astStructDeclaration *struct_info;
} MOJOSHADER_astStructStatement;

typedef struct MOJOSHADER_astCompilationUnitFunction
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astCompilationUnit *next;
    MOJOSHADER_astFunctionSignature *declaration;
    MOJOSHADER_astStatement *definition;
    int index;  /* unique id. Will be 0 until semantic analysis runs. */
} MOJOSHADER_astCompilationUnitFunction;

typedef struct MOJOSHADER_astCompilationUnitTypedef
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astCompilationUnit *next;
    MOJOSHADER_astTypedef *type_info;
} MOJOSHADER_astCompilationUnitTypedef;

typedef struct MOJOSHADER_astCompilationUnitStruct
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astCompilationUnit *next;
    MOJOSHADER_astStructDeclaration *struct_info;
} MOJOSHADER_astCompilationUnitStruct;

typedef struct MOJOSHADER_astCompilationUnitVariable
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astCompilationUnit *next;
    MOJOSHADER_astVariableDeclaration *declaration;
} MOJOSHADER_astCompilationUnitVariable;


/* this is way cleaner than all the nasty typecasting. */
typedef union MOJOSHADER_astNode
{
    MOJOSHADER_astNodeInfo ast;
    MOJOSHADER_astGeneric generic;
    MOJOSHADER_astExpression expression;
    MOJOSHADER_astArguments arguments;
    MOJOSHADER_astExpressionUnary unary;
    MOJOSHADER_astExpressionBinary binary;
    MOJOSHADER_astExpressionTernary ternary;
    MOJOSHADER_astExpressionIdentifier identifier;
    MOJOSHADER_astExpressionIntLiteral intliteral;
    MOJOSHADER_astExpressionFloatLiteral floatliteral;
    MOJOSHADER_astExpressionStringLiteral stringliteral;
    MOJOSHADER_astExpressionBooleanLiteral boolliteral;
    MOJOSHADER_astExpressionConstructor constructor;
    MOJOSHADER_astExpressionDerefStruct derefstruct;
    MOJOSHADER_astExpressionCallFunction callfunc;
    MOJOSHADER_astExpressionCast cast;
    MOJOSHADER_astCompilationUnit compunit;
    MOJOSHADER_astFunctionParameters params;
    MOJOSHADER_astFunctionSignature funcsig;
    MOJOSHADER_astScalarOrArray soa;
    MOJOSHADER_astAnnotations annotations;
    MOJOSHADER_astPackOffset packoffset;
    MOJOSHADER_astVariableLowLevel varlowlevel;
    MOJOSHADER_astStructMembers structmembers;
    MOJOSHADER_astStructDeclaration structdecl;
    MOJOSHADER_astVariableDeclaration vardecl;
    MOJOSHADER_astStatement stmt;
    MOJOSHADER_astEmptyStatement emptystmt;
    MOJOSHADER_astBreakStatement breakstmt;
    MOJOSHADER_astContinueStatement contstmt;
    MOJOSHADER_astDiscardStatement discardstmt;
    MOJOSHADER_astBlockStatement blockstmt;
    MOJOSHADER_astReturnStatement returnstmt;
    MOJOSHADER_astExpressionStatement exprstmt;
    MOJOSHADER_astIfStatement ifstmt;
    MOJOSHADER_astSwitchCases cases;
    MOJOSHADER_astSwitchStatement switchstmt;
    MOJOSHADER_astWhileStatement whilestmt;
    MOJOSHADER_astDoStatement dostmt;
    MOJOSHADER_astForStatement forstmt;
    MOJOSHADER_astTypedef typdef;
    MOJOSHADER_astTypedefStatement typedefstmt;
    MOJOSHADER_astVarDeclStatement vardeclstmt;
    MOJOSHADER_astStructStatement structstmt;
    MOJOSHADER_astCompilationUnitFunction funcunit;
    MOJOSHADER_astCompilationUnitTypedef typedefunit;
    MOJOSHADER_astCompilationUnitStruct structunit;
    MOJOSHADER_astCompilationUnitVariable varunit;
} MOJOSHADER_astNode;


/*
 * Structure used to return data from parsing of a shader into an AST...
 */
/* !!! FIXME: most of these ints should be unsigned. */
typedef struct MOJOSHADER_astData
{
    /*
     * The number of elements pointed to by (errors).
     */
    int error_count;

    /*
     * (error_count) elements of data that specify errors that were generated
     *  by parsing this shader.
     * This can be NULL if there were no errors or if (error_count) is zero.
     *  Note that this will only produce errors for syntax problems. Most of
     *  the things we expect a compiler to produce errors for--incompatible
     *  types, unknown identifiers, etc--are not checked at all during
     *  initial generation of the syntax tree...bogus programs that would
     *  fail to compile will pass here without error, if they are syntactically
     *  correct!
     */
    MOJOSHADER_error *errors;

    /*
     * The name of the source profile used to parse the shader. Will be NULL
     *  on error.
     */
    const char *source_profile;

    /*
     * The actual syntax tree. You are responsible for walking it yourself.
     *  CompilationUnits are always the top of the tree (functions, typedefs,
     *  global variables, etc). Will be NULL on error.
     */
    const MOJOSHADER_astNode *ast;

    /*
     * This is the malloc implementation you passed to MOJOSHADER_parse().
     */
    MOJOSHADER_malloc malloc;

    /*
     * This is the free implementation you passed to MOJOSHADER_parse().
     */
    MOJOSHADER_free free;

    /*
     * This is the pointer you passed as opaque data for your allocator.
     */
    void *malloc_data;

    /*
     * This is internal data, and not for the application to touch.
     */
    void *opaque;
} MOJOSHADER_astData;


/*
 * You almost certainly don't need this function, unless you absolutely know
 *  why you need it without hesitation. This is almost certainly only good for
 *  building code analysis tools on top of.
 *
 * This is intended to parse HLSL source code, turning it into an abstract
 *  syntax tree.
 *
 * (srcprofile) specifies the source language of the shader. You can specify
 *  a shader model with this, too. See MOJOSHADER_SRC_PROFILE_* constants.
 *
 * (filename) is a NULL-terminated UTF-8 filename. It can be NULL. We do not
 *  actually access this file, as we obtain our data from (source). This
 *  string is copied when we need to report errors while processing (source),
 *  as opposed to errors in a file referenced via the #include directive in
 *  (source). If this is NULL, then errors will report the filename as NULL,
 *  too.
 *
 * (source) is an UTF-8 string of valid high-level shader source code.
 *  It does not need to be NULL-terminated.
 *
 * (sourcelen) is the length of the string pointed to by (source), in bytes.
 *
 * (defines) points to (define_count) preprocessor definitions, and can be
 *  NULL. These are treated by the preprocessor as if the source code started
 *  with one #define for each entry you pass in here.
 *
 * (include_open) and (include_close) let the app control the preprocessor's
 *  behaviour for #include statements. Both are optional and can be NULL, but
 *  both must be specified if either is specified.
 *
 * This will return a MOJOSHADER_astData. The data supplied here gives the
 *  application a tree-like structure they can walk to see the layout of
 *  a given program. When you are done with this data, pass it to
 *  MOJOSHADER_freeCompileData() to deallocate resources.
 *
 * This function will never return NULL, even if the system is completely
 *  out of memory upon entry (in which case, this function returns a static
 *  MOJOSHADER_astData object, which is still safe to pass to
 *  MOJOSHADER_freeAstData()).
 *
 * As parsing requires some memory to be allocated, you may provide a
 *  custom allocator to this function, which will be used to allocate/free
 *  memory. They function just like malloc() and free(). We do not use
 *  realloc(). If you don't care, pass NULL in for the allocator functions.
 *  If your allocator needs instance-specific data, you may supply it with the
 *  (d) parameter. This pointer is passed as-is to your (m) and (f) functions.
 *
 * This function is thread safe, so long as the various callback functions
 *  are, too, and that the parameters remains intact for the duration of the
 *  call. This allows you to parse several shaders on separate CPU cores
 *  at the same time.
 */
const MOJOSHADER_astData *MOJOSHADER_parseAst(const char *srcprofile,
                                    const char *filename, const char *source,
                                    unsigned int sourcelen,
                                    const MOJOSHADER_preprocessorDefine *defs,
                                    unsigned int define_count,
                                    MOJOSHADER_includeOpen include_open,
                                    MOJOSHADER_includeClose include_close,
                                    MOJOSHADER_malloc m, MOJOSHADER_free f,
                                    void *d);


/* !!! FIXME: expose semantic analysis to the public API? */


/*
 * Call this to dispose of AST parsing results when you are done with them.
 *  This will call the MOJOSHADER_free function you provided to
 *  MOJOSHADER_parseAst() multiple times, if you provided one.
 *  Passing a NULL here is a safe no-op.
 *
 * This function is thread safe, so long as any allocator you passed into
 *  MOJOSHADER_parseAst() is, too.
 */
void MOJOSHADER_freeAstData(const MOJOSHADER_astData *data);


/* Intermediate Representation interface... */
/* !!! FIXME: there is currently no way to access the IR via the public API. */
typedef enum MOJOSHADER_irNodeType
{
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

    MOJOSHADER_IR_END_RANGE
} MOJOSHADER_irNodeType;

typedef struct MOJOSHADER_irNodeInfo
{
    MOJOSHADER_irNodeType type;
    const char *filename;
    unsigned int line;
} MOJOSHADER_irNodeInfo;

typedef struct MOJOSHADER_irExprList MOJOSHADER_irExprList;

/*
 * IR nodes are categorized into Expressions, Statements, and Everything Else.
 *  You can cast any of them to MOJOSHADER_irGeneric, but this split is
 *  useful for slightly better type-checking (you can't cleanly assign
 *  something that doesn't return a value to something that wants one, etc).
 * These broader categories are just unions of the simpler types, so the
 *  real definitions are below all the things they contain (but these
 *  predeclarations are because the simpler types refer to the broader
 *  categories).
 */
typedef union MOJOSHADER_irExpression MOJOSHADER_irExpression;  /* returns a value. */
typedef union MOJOSHADER_irStatement MOJOSHADER_irStatement;   /* no returned value. */
typedef union MOJOSHADER_irMisc MOJOSHADER_irMisc;        /* Everything Else. */
typedef union MOJOSHADER_irNode MOJOSHADER_irNode;        /* Generic uber-wrapper. */

/* You can cast any IR node pointer to this. */
typedef struct MOJOSHADER_irGeneric
{
    MOJOSHADER_irNodeInfo ir;
} MOJOSHADER_irGeneric;


/* These are used for MOJOSHADER_irBinOp */
typedef enum MOJOSHADER_irBinOpType
{
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
    MOJOSHADER_IR_BINOP_UNKNOWN
} MOJOSHADER_irBinOpType;

typedef enum MOJOSHADER_irConditionType
{
    MOJOSHADER_IR_COND_EQL,
    MOJOSHADER_IR_COND_NEQ,
    MOJOSHADER_IR_COND_LT,
    MOJOSHADER_IR_COND_GT,
    MOJOSHADER_IR_COND_LEQ,
    MOJOSHADER_IR_COND_GEQ,
    MOJOSHADER_IR_COND_UNKNOWN
} MOJOSHADER_irConditionType;


/* MOJOSHADER_irExpression types... */

typedef struct MOJOSHADER_irExprInfo
{
    MOJOSHADER_irNodeInfo ir;
    MOJOSHADER_astDataTypeType type;
    int elements;
} MOJOSHADER_irExprInfo;

typedef struct MOJOSHADER_irConstant    /* Constant value */
{
    MOJOSHADER_irExprInfo info;  /* Always MOJOSHADER_IR_CONSTANT */
    union
    {
        int ival[16];
        float fval[16];
    } value;
} MOJOSHADER_irConstant;

typedef struct MOJOSHADER_irTemp /* temp value (not necessarily a register). */
{
    MOJOSHADER_irExprInfo info;  /* Always MOJOSHADER_IR_TEMP */
    int index;
} MOJOSHADER_irTemp;

typedef struct MOJOSHADER_irBinOp  /* binary operator (+, -, etc) */
{
    MOJOSHADER_irExprInfo info;  /* Always MOJOSHADER_IR_BINOP */
    MOJOSHADER_irBinOpType op;
    MOJOSHADER_irExpression *left;
    MOJOSHADER_irExpression *right;
} MOJOSHADER_irBinOp;

typedef struct MOJOSHADER_irMemory
{
    MOJOSHADER_irExprInfo info;  /* Always MOJOSHADER_IR_MEMORY */
    int index;  /* not final addresses, just a unique identifier. */
} MOJOSHADER_irMemory;

typedef struct MOJOSHADER_irCall
{
    MOJOSHADER_irExprInfo info;  /* Always MOJOSHADER_IR_CALL */
    int index;
    MOJOSHADER_irExprList *args;
} MOJOSHADER_irCall;

typedef struct MOJOSHADER_irESeq  /* statement with result */
{
    MOJOSHADER_irExprInfo info;  /* Always MOJOSHADER_IR_ESEQ */
    MOJOSHADER_irStatement *stmt;  /* execute this for side-effects, then... */
    MOJOSHADER_irExpression *expr; /* ...use this for the result. */
} MOJOSHADER_irESeq;

typedef struct MOJOSHADER_irArray  /* Array dereference. */
{
    MOJOSHADER_irExprInfo info;  /* Always MOJOSHADER_IR_ARRAY */
    MOJOSHADER_irExpression *array;
    MOJOSHADER_irExpression *element;
} MOJOSHADER_irArray;

typedef struct MOJOSHADER_irConvert  /* casting between datatypes */
{
    MOJOSHADER_irExprInfo info;  /* Always MOJOSHADER_IR_CONVERT */
    MOJOSHADER_irExpression *expr;
} MOJOSHADER_irConvert;

typedef struct MOJOSHADER_irSwizzle  /* vector swizzle */
{
    MOJOSHADER_irExprInfo info;  /* Always MOJOSHADER_IR_SWIZZLE */
    MOJOSHADER_irExpression *expr;
    char channels[4];
} MOJOSHADER_irSwizzle;

typedef struct MOJOSHADER_irConstruct  /* vector construct from discrete items */
{
    MOJOSHADER_irExprInfo info;  /* Always MOJOSHADER_IR_CONTSTRUCT */
    MOJOSHADER_irExprList *args;
} MOJOSHADER_irConstruct;

/* Wrap the whole category in a union for type "safety." */
union MOJOSHADER_irExpression
{
    MOJOSHADER_irNodeInfo ir;
    MOJOSHADER_irExprInfo info;
    MOJOSHADER_irConstant constant;
    MOJOSHADER_irTemp temp;
    MOJOSHADER_irBinOp binop;
    MOJOSHADER_irMemory memory;
    MOJOSHADER_irCall call;
    MOJOSHADER_irESeq eseq;
    MOJOSHADER_irArray array;
    MOJOSHADER_irConvert convert;
    MOJOSHADER_irSwizzle swizzle;
    MOJOSHADER_irConstruct construct;
};

/* MOJOSHADER_irStatement types. */

typedef struct MOJOSHADER_irMove  /* load/store. */
{
    MOJOSHADER_irNodeInfo ir;  /* Always MOJOSHADER_IR_MOVE */
    MOJOSHADER_irExpression *dst; /* must result in a temp or mem! */
    MOJOSHADER_irExpression *src;
    int writemask;  // for write-masking vector channels.
} MOJOSHADER_irMove;

typedef struct MOJOSHADER_irExprStmt  /* evaluate expression, throw it away. */
{
    MOJOSHADER_irNodeInfo ir;  /* Always MOJOSHADER_IR_EXPR_STMT */
    MOJOSHADER_irExpression *expr;
} MOJOSHADER_irExprStmt;

typedef struct MOJOSHADER_irJump  /* unconditional jump */
{
    MOJOSHADER_irNodeInfo ir;  /* Always MOJOSHADER_IR_JUMP */
    int label;
    // !!! FIXME: possible label list, for further optimization passes.
} MOJOSHADER_irJump;

typedef struct MOJOSHADER_irCJump  /* conditional jump */
{
    MOJOSHADER_irNodeInfo ir;  /* Always MOJOSHADER_IR_CJUMP */
    MOJOSHADER_irConditionType cond;
    MOJOSHADER_irExpression *left;  /* if (left cond right) */
    MOJOSHADER_irExpression *right;
    int iftrue;  /* label id for true case. */
    int iffalse; /* label id for false case. */
} MOJOSHADER_irCJump;

typedef struct MOJOSHADER_irSeq  /* statement without side effects */
{
    MOJOSHADER_irNodeInfo ir;  /* Always MOJOSHADER_IR_SEQ */
    MOJOSHADER_irStatement *first;
    MOJOSHADER_irStatement *next;
} MOJOSHADER_irSeq;

typedef struct MOJOSHADER_irLabel  /* like a label in assembly language. */
{
    MOJOSHADER_irNodeInfo ir;  /* Always MOJOSHADER_IR_LABEL */
    int index;
} MOJOSHADER_irLabel;

typedef MOJOSHADER_irGeneric MOJOSHADER_irDiscard;  /* discard statement. */


/* Wrap the whole category in a union for type "safety." */
union MOJOSHADER_irStatement
{
    MOJOSHADER_irNodeInfo ir;
    MOJOSHADER_irGeneric generic;
    MOJOSHADER_irMove move;
    MOJOSHADER_irExprStmt expr;
    MOJOSHADER_irJump jump;
    MOJOSHADER_irCJump cjump;
    MOJOSHADER_irSeq seq;
    MOJOSHADER_irLabel label;
    MOJOSHADER_irDiscard discard;
};

/* MOJOSHADER_irMisc types. */

struct MOJOSHADER_irExprList
{
    MOJOSHADER_irNodeInfo ir;  /* Always MOJOSHADER_IR_EXPRLIST */
    MOJOSHADER_irExpression *expr;
    MOJOSHADER_irExprList *next;
};

/* Wrap the whole category in a union for type "safety." */
union MOJOSHADER_irMisc
{
    MOJOSHADER_irNodeInfo ir;
    MOJOSHADER_irGeneric generic;
    MOJOSHADER_irExprList exprlist;
};

/* This is a catchall for all your needs. :) */
union MOJOSHADER_irNode
{
    MOJOSHADER_irNodeInfo ir;
    MOJOSHADER_irGeneric generic;
    MOJOSHADER_irExpression expr;
    MOJOSHADER_irStatement stmt;
    MOJOSHADER_irMisc misc;
};


/* Compiler interface... */

/*
 * Structure used to return data from parsing of a shader...
 */
/* !!! FIXME: most of these ints should be unsigned. */
typedef struct MOJOSHADER_compileData
{
    /*
     * The number of elements pointed to by (errors).
     */
    int error_count;

    /*
     * (error_count) elements of data that specify errors that were generated
     *  by compiling this shader.
     * This can be NULL if there were no errors or if (error_count) is zero.
     */
    MOJOSHADER_error *errors;

    /*
     * The number of elements pointed to by (warnings).
     */
    int warning_count;

    /*
     * (warning_count) elements of data that specify errors that were
     *  generated by compiling this shader.
     * This can be NULL if there were no errors or if (warning_count) is zero.
     */
    MOJOSHADER_error *warnings;

    /*
     * The name of the source profile used to compile the shader. Will be NULL
     *  on error.
     */
    const char *source_profile;

    /*
     * Bytes of output from compiling. This will be a null-terminated ASCII
     *  string of D3D assembly source code.
     */
    const char *output;

    /*
     * Byte count for output, not counting any null terminator.
     *  Will be 0 on error.
     */
    int output_len;

    /*
     * The number of elements pointed to by (symbols).
     */
    int symbol_count;

    /*
     * (symbol_count) elements of data that specify high-level symbol data
     *  for the shader. This can be used by MOJOSHADER_assemble() to
     *  generate a CTAB section in bytecode, which is needed by
     *  MOJOSHADER_parseData() to handle some shaders. This can be NULL on
     *  error or if (symbol_count) is zero.
     */
    MOJOSHADER_symbol *symbols;

    /*
     * This is the malloc implementation you passed to MOJOSHADER_parse().
     */
    MOJOSHADER_malloc malloc;

    /*
     * This is the free implementation you passed to MOJOSHADER_parse().
     */
    MOJOSHADER_free free;

    /*
     * This is the pointer you passed as opaque data for your allocator.
     */
    void *malloc_data;
} MOJOSHADER_compileData;


/*
 * This function is optional. Use this to compile high-level shader programs.
 *
 * This is intended to turn HLSL source code into D3D assembly code, which
 *  can then be passed to MOJOSHADER_assemble() to convert it to D3D bytecode
 *  (which can then be used with MOJOSHADER_parseData() to support other
 *  shading targets).
 *
 * (srcprofile) specifies the source language of the shader. You can specify
 *  a shader model with this, too. See MOJOSHADER_SRC_PROFILE_* constants.
 *
 * (filename) is a NULL-terminated UTF-8 filename. It can be NULL. We do not
 *  actually access this file, as we obtain our data from (source). This
 *  string is copied when we need to report errors while processing (source),
 *  as opposed to errors in a file referenced via the #include directive in
 *  (source). If this is NULL, then errors will report the filename as NULL,
 *  too.
 *
 * (source) is an UTF-8 string of valid high-level shader source code.
 *  It does not need to be NULL-terminated.
 *
 * (sourcelen) is the length of the string pointed to by (source), in bytes.
 *
 * (defines) points to (define_count) preprocessor definitions, and can be
 *  NULL. These are treated by the preprocessor as if the source code started
 *  with one #define for each entry you pass in here.
 *
 * (include_open) and (include_close) let the app control the preprocessor's
 *  behaviour for #include statements. Both are optional and can be NULL, but
 *  both must be specified if either is specified.
 *
 * This will return a MOJOSHADER_compileData. The data supplied here is
 *  sufficient to supply to MOJOSHADER_assemble() for further processing.
 *  When you are done with this data, pass it to MOJOSHADER_freeCompileData()
 *  to deallocate resources.
 *
 * This function will never return NULL, even if the system is completely
 *  out of memory upon entry (in which case, this function returns a static
 *  MOJOSHADER_compileData object, which is still safe to pass to
 *  MOJOSHADER_freeCompileData()).
 *
 * As compiling requires some memory to be allocated, you may provide a
 *  custom allocator to this function, which will be used to allocate/free
 *  memory. They function just like malloc() and free(). We do not use
 *  realloc(). If you don't care, pass NULL in for the allocator functions.
 *  If your allocator needs instance-specific data, you may supply it with the
 *  (d) parameter. This pointer is passed as-is to your (m) and (f) functions.
 *
 * This function is thread safe, so long as the various callback functions
 *  are, too, and that the parameters remains intact for the duration of the
 *  call. This allows you to compile several shaders on separate CPU cores
 *  at the same time.
 */
const MOJOSHADER_compileData *MOJOSHADER_compile(const char *srcprofile,
                                    const char *filename, const char *source,
                                    unsigned int sourcelen,
                                    const MOJOSHADER_preprocessorDefine *defs,
                                    unsigned int define_count,
                                    MOJOSHADER_includeOpen include_open,
                                    MOJOSHADER_includeClose include_close,
                                    MOJOSHADER_malloc m, MOJOSHADER_free f,
                                    void *d);


/*
 * Call this to dispose of compile results when you are done with them.
 *  This will call the MOJOSHADER_free function you provided to
 *  MOJOSHADER_compile() multiple times, if you provided one.
 *  Passing a NULL here is a safe no-op.
 *
 * This function is thread safe, so long as any allocator you passed into
 *  MOJOSHADER_compile() is, too.
 */
void MOJOSHADER_freeCompileData(const MOJOSHADER_compileData *data);


/* OpenGL interface... */

/*
 * Signature for function lookup callbacks. MojoShader will call a function
 *  you provide to get OpenGL entry points (both standard functions and
 *  extensions). Through this, MojoShader never links directly to OpenGL,
 *  but relies on you to provide the implementation. This means you can
 *  swap in different drivers, or hook functions (log every GL call MojoShader
 *  makes, etc).
 *
 * (fnname) is the function name we want the address for ("glBegin" or
 *  whatever. (data) is a void pointer you provide, if this callback needs
 *  extra information. If you don't need it, you may specify NULL.
 *
 * Return the entry point on success, NULL if it couldn't be found.
 *  Note that this could ask for standard entry points like glEnable(), or
 *  extensions like glProgramLocalParameterI4ivNV(), so you might need
 *  to check two places to find the desired entry point, depending on your
 *  platform (Windows might need to look in OpenGL32.dll and use WGL, etc).
 */
typedef void *(*MOJOSHADER_glGetProcAddress)(const char *fnname, void *data);


/*
 * "Contexts" map to OpenGL contexts...you need one per window, or whatever,
 *  and need to inform MojoShader when you make a new one current.
 *
 * "Shaders" refer to individual vertex or pixel programs, and are created
 *  by "compiling" Direct3D shader bytecode. A vertex and pixel shader are
 *  "linked" into a "Program" before you can use them to render.
 *
 * To the calling application, these are all opaque handles.
 */
typedef struct MOJOSHADER_glContext MOJOSHADER_glContext;
typedef struct MOJOSHADER_glShader MOJOSHADER_glShader;
typedef struct MOJOSHADER_glProgram MOJOSHADER_glProgram;


/*
 * Get a list of available profiles. This will fill in the array (profs)
 *  with up to (size) pointers of profiles that the current system can handle;
 *  that is, the profiles are built into MojoShader and the OpenGL extensions
 *  required for them exist at runtime. This function returns the number of
 *  available profiles, which may be more, less, or equal to (size).
 *
 * If there are more than (size) profiles, the (profs) buffer will not
 *  overflow. You can check the return value for the total number of
 *  available profiles, allocate more space, and try again if necessary.
 *  Calling this function with (size) == 0 is legal.
 *
 * You can only call this AFTER you have successfully built your GL context
 *  and made it current. This function will lookup the GL functions it needs
 *  through the callback you supply, via (lookup) and (d). The lookup function
 *  is neither stored nor used by MojoShader after this function returns, nor
 *  are the functions it might look up.
 *
 * You should not free any strings returned from this function; they are
 *  pointers to internal, probably static, memory.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 */
int MOJOSHADER_glAvailableProfiles(MOJOSHADER_glGetProcAddress lookup, void *d,
                                   const char **profs, const int size);


/*
 * Determine the best profile to use for the current system.
 *
 * You can only call this AFTER you have successfully built your GL context
 *  and made it current. This function will lookup the GL functions it needs
 *  through the callback you supply via (lookup) and (d). The lookup function
 *  is neither stored nor used by MojoShader after this function returns, nor
 *  are the functions it might look up.
 *
 * Returns the name of the "best" profile on success, NULL if none of the
 *  available profiles will work on this system. "Best" is a relative term,
 *  but it generally means the best trade off between feature set and
 *  performance. The selection algorithm may be arbitrary and complex.
 *
 * The returned value is an internal static string, and should not be free()'d
 *  by the caller. If you get a NULL, calling MOJOSHADER_glGetError() might
 *  shed some light on why.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 */
const char *MOJOSHADER_glBestProfile(MOJOSHADER_glGetProcAddress lookup, void *d);


/*
 * Prepare MojoShader to manage OpenGL shaders.
 *
 * You do not need to call this if all you want is MOJOSHADER_parse().
 *
 * You must call this once AFTER you have successfully built your GL context
 *  and made it current. This function will lookup the GL functions it needs
 *  through the callback you supply via (lookup) and (lookup_d), after which
 *  it may call them at any time up until you call
 *  MOJOSHADER_glDestroyContext(). The lookup function is neither stored nor
 *  used by MojoShader after this function returns.
 *
 * (profile) is an OpenGL-specific MojoShader profile, which decides how
 *  Direct3D bytecode shaders get turned into OpenGL programs, and how they
 *  are fed to the GL.
 *
 * (lookup) is a callback that is used to load GL entry points. This callback
 *  has to look up base GL functions and extension entry points. The pointer
 *  you supply in (lookup_d) is passed as-is to the callback.
 *
 * As MojoShader requires some memory to be allocated, you may provide a
 *  custom allocator to this function, which will be used to allocate/free
 *  memory. They function just like malloc() and free(). We do not use
 *  realloc(). If you don't care, pass NULL in for the allocator functions.
 *  If your allocator needs instance-specific data, you may supply it with the
 *  (malloc_d) parameter. This pointer is passed as-is to your (m) and (f)
 *  functions.
 *
 * Returns a new context on success, NULL on error. If you get a new context,
 *  you need to make it current before using it with
 *  MOJOSHADER_glMakeContextCurrent().
 *
 * This call is NOT thread safe! It must return success before you may call
 *  any other MOJOSHADER_gl* function. Also, as most OpenGL implementations
 *  are not thread safe, you should probably only call this from the same
 *  thread that created the GL context.
 */
MOJOSHADER_glContext *MOJOSHADER_glCreateContext(const char *profile,
                                        MOJOSHADER_glGetProcAddress lookup,
                                        void *lookup_d,
                                        MOJOSHADER_malloc m, MOJOSHADER_free f,
                                        void *malloc_d);

/*
 * You must call this before using the context that you got from
 *  MOJOSHADER_glCreateContext(), and must use it when you switch to a new GL
 *  context.
 *
 * You can only have one MOJOSHADER_glContext per actual GL context, or
 *  undefined behaviour will result.
 *
 * It is legal to call this with a NULL pointer to make no context current,
 *  but you need a valid context to be current to use most of MojoShader.
 */
void MOJOSHADER_glMakeContextCurrent(MOJOSHADER_glContext *ctx);

/*
 * Get any error state we might have picked up. MojoShader will NOT call
 *  glGetError() internally, but there are other errors we can pick up,
 *  such as failed shader compilation, etc.
 *
 * Returns a human-readable string. This string is for debugging purposes, and
 *  not guaranteed to be localized, coherent, or user-friendly in any way.
 *  It's for programmers!
 *
 * The latest error may remain between calls. New errors replace any existing
 *  error. Don't check this string for a sign that an error happened, check
 *  return codes instead and use this for explanation when debugging.
 *
 * Do not free the returned string: it's a pointer to a static internal
 *  buffer. Do not keep the pointer around, either, as it's likely to become
 *  invalid as soon as you call into MojoShader again.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call does NOT require a valid MOJOSHADER_glContext to have been made
 *  current. The error buffer is shared between contexts, so you can get
 *  error results from a failed MOJOSHADER_glCreateContext().
 */
const char *MOJOSHADER_glGetError(void);

/*
 * Get the maximum uniforms a shader can support for the current GL context,
 *  MojoShader profile, and shader type. You can use this to make decisions
 *  about what shaders you want to use (for example, a less complicated
 *  shader may be swapped in for lower-end systems).
 *
 * Returns the number, or -1 on error.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 */
int MOJOSHADER_glMaxUniforms(MOJOSHADER_shaderType shader_type);

/*
 * Compile a buffer of Direct3D shader bytecode into an OpenGL shader.
 *  You still need to link the shader before you may render with it.
 *
 *   (tokenbuf) is a buffer of Direct3D shader bytecode.
 *   (bufsize) is the size, in bytes, of the bytecode buffer.
 *   (swiz) and (swizcount) are passed to MOJOSHADER_parse() unmolested.
 *
 * Returns NULL on error, or a shader handle on success.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Compiled shaders from this function may not be shared between contexts.
 */
MOJOSHADER_glShader *MOJOSHADER_glCompileShader(const unsigned char *tokenbuf,
                                                const unsigned int bufsize,
                                                const MOJOSHADER_swizzle *swiz,
                                                const unsigned int swizcount);


/*
 * Get the MOJOSHADER_parseData structure that was produced from the
 *  call to MOJOSHADER_glCompileShader().
 *
 * This data is read-only, and you should NOT attempt to free it. This
 *  pointer remains valid until the shader is deleted.
 */
const MOJOSHADER_parseData *MOJOSHADER_glGetShaderParseData(
                                                MOJOSHADER_glShader *shader);
/*
 * Link a vertex and pixel shader into an OpenGL program.
 *  (vshader) or (pshader) can be NULL, to specify that the GL should use the
 *  fixed-function pipeline instead of the programmable pipeline for that
 *  portion of the work. You can reuse shaders in various combinations across
 *  multiple programs, by relinking different pairs.
 *
 * It is illegal to give a vertex shader for (pshader) or a pixel shader
 *  for (vshader).
 *
 * Once you have successfully linked a program, you may render with it.
 *
 * Returns NULL on error, or a program handle on success.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Linked programs from this function may not be shared between contexts.
 */
MOJOSHADER_glProgram *MOJOSHADER_glLinkProgram(MOJOSHADER_glShader *vshader,
                                               MOJOSHADER_glShader *pshader);

/*
 * This binds the program (using, for example, glUseProgramObjectARB()), and
 *  disables all the client-side arrays so we can reset them with new values
 *  if appropriate.
 *
 * Call with NULL to disable the programmable pipeline and all enabled
 *  client-side arrays.
 *
 * After binding a program, you should update any uniforms you care about
 *  with MOJOSHADER_glSetVertexShaderUniformF() (etc), set any vertex arrays
 *  you want to use with MOJOSHADER_glSetVertexAttribute(), and finally call
 *  MOJOSHADER_glProgramReady() to commit everything to the GL. Then you may
 *  begin drawing through standard GL entry points.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 */
void MOJOSHADER_glBindProgram(MOJOSHADER_glProgram *program);

/*
 * This binds individual shaders as if you had linked them with
 *  MOJOSHADER_glLinkProgram(), and used MOJOSHADER_glBindProgram() on the
 *  linked result.
 *
 * MojoShader will handle linking behind the scenes, and keep a cache of
 *  programs linked here. Programs are removed from this cache when one of the
 *  invidual shaders in it is deleted, otherwise they remain cached so future
 *  calls to this function don't need to relink a previously-used shader
 *  grouping.
 *
 * This function is for convenience, as the API is closer to how Direct3D
 *  works, and retrofitting linking into your app can be difficult;
 *  frequently, you just end up building your own cache, anyhow.
 *
 * Calling with all shaders set to NULL is equivalent to calling
 *  MOJOSHADER_glBindProgram(NULL).
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 */
void MOJOSHADER_glBindShaders(MOJOSHADER_glShader *vshader,
                              MOJOSHADER_glShader *pshader);

/*
 * Set a floating-point uniform value (what Direct3D calls a "constant").
 *
 * There is a single array of 4-float "registers" shared by all vertex shaders.
 *  This is the "c" register file in Direct3D (c0, c1, c2, etc...)
 *  MojoShader will take care of synchronizing this internal array with the
 *  appropriate variables in the GL shaders.
 *
 * (idx) is the index into the internal array: 0 is the first four floats,
 *  1 is the next four, etc.
 * (data) is a pointer to (vec4count*4) floats.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glSetVertexShaderUniformF(unsigned int idx, const float *data,
                                          unsigned int vec4count);

/*
 * Retrieve a floating-point uniform value (what Direct3D calls a "constant").
 *
 * There is a single array of 4-float "registers" shared by all vertex shaders.
 *  This is the "c" register file in Direct3D (c0, c1, c2, etc...)
 *  MojoShader will take care of synchronizing this internal array with the
 *  appropriate variables in the GL shaders.
 *
 * (idx) is the index into the internal array: 0 is the first four floats,
 *  1 is the next four, etc.
 * (data) is a pointer to space for (vec4count*4) floats.
 *  (data) will be filled will current values in the register file. Results
 *  are undefined if you request data past the end of the register file or
 *  previously uninitialized registers.
 *
 * This is a "fast" call; we're just reading memory from internal memory. We
 *  do not query the GPU or the GL for this information.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glGetVertexShaderUniformF(unsigned int idx, float *data,
                                          unsigned int vec4count);


/*
 * Set an integer uniform value (what Direct3D calls a "constant").
 *
 * There is a single array of 4-int "registers" shared by all vertex shaders.
 *  This is the "i" register file in Direct3D (i0, i1, i2, etc...)
 *  MojoShader will take care of synchronizing this internal array with the
 *  appropriate variables in the GL shaders.
 *
 * (idx) is the index into the internal array: 0 is the first four ints,
 *  1 is the next four, etc.
 * (data) is a pointer to (ivec4count*4) ints.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glSetVertexShaderUniformI(unsigned int idx, const int *data,
                                          unsigned int ivec4count);

/*
 * Retrieve an integer uniform value (what Direct3D calls a "constant").
 *
 * There is a single array of 4-int "registers" shared by all vertex shaders.
 *  This is the "i" register file in Direct3D (i0, i1, i2, etc...)
 *  MojoShader will take care of synchronizing this internal array with the
 *  appropriate variables in the GL shaders.
 *
 * (idx) is the index into the internal array: 0 is the first four ints,
 *  1 is the next four, etc.
 * (data) is a pointer to space for (ivec4count*4) ints.
 *  (data) will be filled will current values in the register file. Results
 *  are undefined if you request data past the end of the register file or
 *  previously uninitialized registers.
 *
 * This is a "fast" call; we're just reading memory from internal memory. We
 *  do not query the GPU or the GL for this information.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glGetVertexShaderUniformI(unsigned int idx, int *data,
                                          unsigned int ivec4count);

/*
 * Set a boolean uniform value (what Direct3D calls a "constant").
 *
 * There is a single array of "registers" shared by all vertex shaders.
 *  This is the "b" register file in Direct3D (b0, b1, b2, etc...)
 *  MojoShader will take care of synchronizing this internal array with the
 *  appropriate variables in the GL shaders.
 *
 * Unlike the float and int counterparts, booleans are single values, not
 *  four-element vectors...so idx==1 is the second boolean in the internal
 *  array, not the fifth.
 *
 * Non-zero values are considered "true" and zero is considered "false".
 *
 * (idx) is the index into the internal array.
 * (data) is a pointer to (bcount) ints.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glSetVertexShaderUniformB(unsigned int idx, const int *data,
                                          unsigned int bcount);

/*
 * Retrieve a boolean uniform value (what Direct3D calls a "constant").
 *
 * There is a single array of "registers" shared by all vertex shaders.
 *  This is the "b" register file in Direct3D (b0, b1, b2, etc...)
 *  MojoShader will take care of synchronizing this internal array with the
 *  appropriate variables in the GL shaders.
 *
 * Unlike the float and int counterparts, booleans are single values, not
 *  four-element vectors...so idx==1 is the second boolean in the internal
 *  array, not the fifth.
 *
 * Non-zero values are considered "true" and zero is considered "false".
 *  This function will always return true values as 1, regardless of what
 *  non-zero integer you originally used to set the registers.
 *
 * (idx) is the index into the internal array.
 * (data) is a pointer to space for (bcount) ints.
 *  (data) will be filled will current values in the register file. Results
 *  are undefined if you request data past the end of the register file or
 *  previously uninitialized registers.
 *
 * This is a "fast" call; we're just reading memory from internal memory. We
 *  do not query the GPU or the GL for this information.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glGetVertexShaderUniformB(unsigned int idx, int *data,
                                          unsigned int bcount);

/*
 * The equivalent of MOJOSHADER_glSetVertexShaderUniformF() for pixel
 *  shaders. Other than using a different internal array that is specific
 *  to pixel shaders, this functions just like its vertex array equivalent.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glSetPixelShaderUniformF(unsigned int idx, const float *data,
                                         unsigned int vec4count);


/*
 * The equivalent of MOJOSHADER_glGetVertexShaderUniformF() for pixel
 *  shaders. Other than using a different internal array that is specific
 *  to pixel shaders, this functions just like its vertex array equivalent.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glGetPixelShaderUniformF(unsigned int idx, float *data,
                                         unsigned int vec4count);


/*
 * The equivalent of MOJOSHADER_glSetVertexShaderUniformI() for pixel
 *  shaders. Other than using a different internal array that is specific
 *  to pixel shaders, this functions just like its vertex array equivalent.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glSetPixelShaderUniformI(unsigned int idx, const int *data,
                                         unsigned int ivec4count);


/*
 * The equivalent of MOJOSHADER_glGetVertexShaderUniformI() for pixel
 *  shaders. Other than using a different internal array that is specific
 *  to pixel shaders, this functions just like its vertex array equivalent.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glGetPixelShaderUniformI(unsigned int idx, int *data,
                                         unsigned int ivec4count);

/*
 * The equivalent of MOJOSHADER_glSetVertexShaderUniformB() for pixel
 *  shaders. Other than using a different internal array that is specific
 *  to pixel shaders, this functions just like its vertex array equivalent.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glSetPixelShaderUniformB(unsigned int idx, const int *data,
                                         unsigned int bcount);

/*
 * The equivalent of MOJOSHADER_glGetVertexShaderUniformB() for pixel
 *  shaders. Other than using a different internal array that is specific
 *  to pixel shaders, this functions just like its vertex array equivalent.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Uniforms are not shared between contexts.
 */
void MOJOSHADER_glGetPixelShaderUniformB(unsigned int idx, int *data,
                                         unsigned int bcount);

/*
 * Connect a client-side array to the currently-bound program.
 *
 * (usage) and (index) map to Direct3D vertex declaration values: COLOR1 would
 *  be MOJOSHADER_USAGE_COLOR and 1.
 *
 * The caller should bind VBOs before this call and treat (ptr) as an offset,
 *  if appropriate.
 *
 * MojoShader will figure out where to plug this stream into the
 *  currently-bound program, and enable the appropriate client-side array.
 *
 * (size), (type), (normalized), (stride), and (ptr) correspond to
 *  glVertexAttribPointer()'s parameters (in most cases, these get passed
 *  unmolested to that very entry point during this function).
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 *
 * Vertex attributes are not shared between contexts.
 */
 /* !!! FIXME: this should probably be "input" and not "attribute" */
 /* !!! FIXME: or maybe "vertex array" or something. */
void MOJOSHADER_glSetVertexAttribute(MOJOSHADER_usage usage,
                                     int index, unsigned int size,
                                     MOJOSHADER_attributeType type,
                                     int normalized, unsigned int stride,
                                     const void *ptr);




/* These below functions are temporary and will be removed from the API once
    the real Effects API is written. Do not use! */
void MOJOSHADER_glSetVertexPreshaderUniformF(unsigned int idx, const float *data,
                                             unsigned int vec4n);
void MOJOSHADER_glGetVertexPreshaderUniformF(unsigned int idx, float *data,
                                             unsigned int vec4n);
void MOJOSHADER_glSetPixelPreshaderUniformF(unsigned int idx, const float *data,
                                            unsigned int vec4n);
void MOJOSHADER_glGetPixelPreshaderUniformF(unsigned int idx, float *data,
                                            unsigned int vec4n);
/* These above functions are temporary and will be removed from the API once
    the real Effects API is written. Do not use! */




/*
 * Inform MojoShader that it should commit any pending state to the GL. This
 *  must be called after you bind a program and update any inputs, right
 *  before you start drawing, so any outstanding changes made to the shared
 *  constants array (etc) can propagate to the shader during this call.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 */
void MOJOSHADER_glProgramReady(void);

/*
 * Free the resources of a linked program. This will delete the GL object
 *  and free memory.
 *
 * If the program is currently bound by MOJOSHADER_glBindProgram(), it will
 *  be deleted as soon as it becomes unbound.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 */
void MOJOSHADER_glDeleteProgram(MOJOSHADER_glProgram *program);

/*
 * Free the resources of a compiled shader. This will delete the GL object
 *  and free memory.
 *
 * If the shader is currently referenced by a linked program (or is currently
 *  bound with MOJOSHADER_glBindShaders()), it will be deleted as soon as all
 *  referencing programs are deleted and it is no longer bound, too.
 *
 * This call is NOT thread safe! As most OpenGL implementations are not thread
 *  safe, you should probably only call this from the same thread that created
 *  the GL context.
 *
 * This call requires a valid MOJOSHADER_glContext to have been made current,
 *  or it will crash your program. See MOJOSHADER_glMakeContextCurrent().
 */
void MOJOSHADER_glDeleteShader(MOJOSHADER_glShader *shader);

/*
 * Deinitialize MojoShader's OpenGL shader management.
 *
 * You must call this once, while your GL context (not MojoShader context) is
 *  still current, if you previously had a successful call to
 *  MOJOSHADER_glCreateContext(). This should be the last MOJOSHADER_gl*
 *  function you call until you've prepared a context again.
 *
 * This will clean up resources previously allocated, and may call into the GL.
 *
 * This will not clean up shaders and programs you created! Please call
 *  MOJOSHADER_glDeleteShader() and MOJOSHADER_glDeleteProgram() to clean
 *  those up before calling this function!
 *
 * This function destroys the MOJOSHADER_glContext you pass it. If it's the
 *  current context, then no context will be current upon return.
 *
 * This call is NOT thread safe! There must not be any other MOJOSHADER_gl*
 *  functions running when this is called. Also, as most OpenGL implementations
 *  are not thread safe, you should probably only call this from the same
 *  thread that created the GL context.
 */
void MOJOSHADER_glDestroyContext(MOJOSHADER_glContext *ctx);

#ifdef __cplusplus
}
#endif

#endif  /* include-once blocker. */

/* end of mojoshader.h ... */

