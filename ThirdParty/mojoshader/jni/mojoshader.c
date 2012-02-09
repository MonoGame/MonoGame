/**
 * MojoShader; generate shader programs from bytecode of compiled
 *  Direct3D shaders.
 *
 * Please see the file LICENSE.txt in the source's root directory.
 *
 *  This file written by Ryan C. Gordon.
 */

// !!! FIXME: this file really needs to be split up.
// !!! FIXME: I keep changing coding styles for symbols and typedefs.

#define __MOJOSHADER_INTERNAL__ 1
#include "mojoshader_internal.h"

typedef struct ConstantsList
{
    MOJOSHADER_constant constant;
    struct ConstantsList *next;
} ConstantsList;

typedef struct VariableList
{
    MOJOSHADER_uniformType type;
    int index;
    int count;
    ConstantsList *constant;
    int used;
    int emit_position;  // used in some profiles.
    struct VariableList *next;
} VariableList;

typedef struct RegisterList
{
    RegisterType regtype;
    int regnum;
    MOJOSHADER_usage usage;
    unsigned int index;
    int writemask;
    int misc;
    const VariableList *array;
    struct RegisterList *next;
} RegisterList;

typedef struct
{
    const uint32 *token;   // this is the unmolested token in the stream.
    int regnum;
    int swizzle;  // xyzw (all four, not split out).
    int swizzle_x;
    int swizzle_y;
    int swizzle_z;
    int swizzle_w;
    SourceMod src_mod;
    RegisterType regtype;
    int relative;
    RegisterType relative_regtype;
    int relative_regnum;
    int relative_component;
    const VariableList *relative_array;
} SourceArgInfo;

struct Profile;  // predeclare.

typedef struct CtabData
{
    int have_ctab;
    int symbol_count;
    MOJOSHADER_symbol *symbols;
} CtabData;

// Context...this is state that changes as we parse through a shader...
typedef struct Context
{
    int isfail;
    int out_of_memory;
    MOJOSHADER_malloc malloc;
    MOJOSHADER_free free;
    void *malloc_data;
    int current_position;
    const uint32 *orig_tokens;
    const uint32 *tokens;
    uint32 tokencount;
    const MOJOSHADER_swizzle *swizzles;
    unsigned int swizzles_count;
    Buffer *output;
    Buffer *preflight;
    Buffer *globals;
    Buffer *helpers;
    Buffer *subroutines;
    Buffer *mainline_intro;
    Buffer *mainline;
    Buffer *ignore;
    Buffer *output_stack[2];
    int indent_stack[2];
    int output_stack_len;
    int indent;
    const char *shader_type_str;
    const char *endline;
    int endline_len;
    int profileid;
    const struct Profile *profile;
    MOJOSHADER_shaderType shader_type;
    uint8 major_ver;
    uint8 minor_ver;
    DestArgInfo dest_arg;
    SourceArgInfo source_args[5];
    SourceArgInfo predicate_arg;  // for predicated instructions.
    uint32 dwords[4];
    uint32 version_token;
    int instruction_count;
    uint32 instruction_controls;
    uint32 previous_opcode;
    int loops;
    int reps;
    int max_reps;
    int cmps;
    int scratch_registers;
    int max_scratch_registers;
    int branch_labels_stack_index;
    int branch_labels_stack[32];
    int assigned_branch_labels;
    int assigned_vertex_attributes;
    int last_address_reg_component;
    RegisterList used_registers;
    RegisterList defined_registers;
    ErrorList *errors;
    int constant_count;
    ConstantsList *constants;
    int uniform_count;
    int uniform_float4_count;
    int uniform_int4_count;
    int uniform_bool_count;
    RegisterList uniforms;
    int attribute_count;
    RegisterList attributes;
    int sampler_count;
    RegisterList samplers;
    VariableList *variables;  // variables to register mapping.
    int centroid_allowed;
    CtabData ctab;
    int have_relative_input_registers;
    int have_multi_color_outputs;
    int determined_constants_arrays;
    int predicated;
    int uses_pointsize;
    int glsl_generated_lit_opcode;
    int glsl_generated_texldd_setup;
    int arb1_wrote_position;
    int have_preshader;
    int ignores_ctab;
    MOJOSHADER_preshader *preshader;

#if SUPPORT_PROFILE_ARB1_NV
    int profile_supports_nv2;
    int profile_supports_nv3;
    int profile_supports_nv4;
#endif
#if SUPPORT_PROFILE_GLSL120
    int profile_supports_glsl120;
#endif
} Context;


// Use these macros so we can remove all bits of these profiles from the build.
#if SUPPORT_PROFILE_ARB1_NV
#define support_nv2(ctx) ((ctx)->profile_supports_nv2)
#define support_nv3(ctx) ((ctx)->profile_supports_nv3)
#define support_nv4(ctx) ((ctx)->profile_supports_nv4)
#else
#define support_nv2(ctx) (0)
#define support_nv3(ctx) (0)
#define support_nv4(ctx) (0)
#endif

#if SUPPORT_PROFILE_GLSL120
#define support_glsl120(ctx) ((ctx)->profile_supports_glsl120)
#else
#define support_glsl120(ctx) (0)
#endif


// Profile entry points...

// one emit function for each opcode in each profile.
typedef void (*emit_function)(Context *ctx);

// one emit function for starting output in each profile.
typedef void (*emit_start)(Context *ctx, const char *profilestr);

// one emit function for ending output in each profile.
typedef void (*emit_end)(Context *ctx);

// one emit function for phase opcode output in each profile.
typedef void (*emit_phase)(Context *ctx);

// one emit function for finalizing output in each profile.
typedef void (*emit_finalize)(Context *ctx);

// one emit function for global definitions in each profile.
typedef void (*emit_global)(Context *ctx, RegisterType regtype, int regnum);

// one emit function for relative uniform arrays in each profile.
typedef void (*emit_array)(Context *ctx, VariableList *var);

// one emit function for relative constants arrays in each profile.
typedef void (*emit_const_array)(Context *ctx,
                                 const struct ConstantsList *constslist,
                                 int base, int size);

// one emit function for uniforms in each profile.
typedef void (*emit_uniform)(Context *ctx, RegisterType regtype, int regnum,
                             const VariableList *var);

// one emit function for samplers in each profile.
typedef void (*emit_sampler)(Context *ctx, int stage, TextureType ttype);

// one emit function for attributes in each profile.
typedef void (*emit_attribute)(Context *ctx, RegisterType regtype, int regnum,
                               MOJOSHADER_usage usage, int index, int wmask,
                               int flags);

// one args function for each possible sequence of opcode arguments.
typedef int (*args_function)(Context *ctx);

// one state function for each opcode where we have state machine updates.
typedef void (*state_function)(Context *ctx);

// one function for varnames in each profile.
typedef const char *(*varname_function)(Context *c, RegisterType t, int num);

// one function for const var array in each profile.
typedef const char *(*const_array_varname_function)(Context *c, int base, int size);

typedef struct Profile
{
    const char *name;
    emit_start start_emitter;
    emit_end end_emitter;
    emit_phase phase_emitter;
    emit_global global_emitter;
    emit_array array_emitter;
    emit_const_array const_array_emitter;
    emit_uniform uniform_emitter;
    emit_sampler sampler_emitter;
    emit_attribute attribute_emitter;
    emit_finalize finalize_emitter;
    varname_function get_varname;
    const_array_varname_function get_const_array_varname;
} Profile;


// Convenience functions for allocators...
#if !MOJOSHADER_FORCE_ALLOCATOR
void *MOJOSHADER_internal_malloc(int bytes, void *d) { return malloc(bytes); }
void MOJOSHADER_internal_free(void *ptr, void *d) { free(ptr); }
#endif

MOJOSHADER_error MOJOSHADER_out_of_mem_error = {
    "Out of memory", NULL, MOJOSHADER_POSITION_NONE
};

MOJOSHADER_parseData MOJOSHADER_out_of_mem_data = {
    1, &MOJOSHADER_out_of_mem_error, 0, 0, 0, 0,
    MOJOSHADER_TYPE_UNKNOWN, 0, 0, 0, 0
};


// !!! FIXME: cut and paste between every damned source file follows...
// !!! FIXME: We need to make some sort of ContextBase that applies to all
// !!! FIXME:  files and move this stuff to mojoshader_common.c ...

static inline void out_of_memory(Context *ctx)
{
    ctx->isfail = ctx->out_of_memory = 1;
} // out_of_memory

static inline void *Malloc(Context *ctx, const size_t len)
{
    void *retval = ctx->malloc((int) len, ctx->malloc_data);
    if (retval == NULL)
        out_of_memory(ctx);
    return retval;
} // Malloc

static inline char *StrDup(Context *ctx, const char *str)
{
    char *retval = (char *) Malloc(ctx, strlen(str) + 1);
    if (retval != NULL)
        strcpy(retval, str);
    return retval;
} // StrDup

static inline void Free(Context *ctx, void *ptr)
{
    ctx->free(ptr, ctx->malloc_data);
} // Free

static void *MallocBridge(int bytes, void *data)
{
    return Malloc((Context *) data, (size_t) bytes);
} // MallocBridge

static void FreeBridge(void *ptr, void *data)
{
    Free((Context *) data, ptr);
} // FreeBridge


// jump between output sections in the context...

static int set_output(Context *ctx, Buffer **section)
{
    // only create output sections on first use.
    if (*section == NULL)
    {
        *section = buffer_create(256, MallocBridge, FreeBridge, ctx);
        if (*section == NULL)
            return 0;
    } // if

    ctx->output = *section;
    return 1;
} // set_output

static void push_output(Context *ctx, Buffer **section)
{
    assert(ctx->output_stack_len < (int) (STATICARRAYLEN(ctx->output_stack)));
    ctx->output_stack[ctx->output_stack_len] = ctx->output;
    ctx->indent_stack[ctx->output_stack_len] = ctx->indent;
    ctx->output_stack_len++;
    if (!set_output(ctx, section))
        return;
    ctx->indent = 0;
} // push_output

static inline void pop_output(Context *ctx)
{
    assert(ctx->output_stack_len > 0);
    ctx->output_stack_len--;
    ctx->output = ctx->output_stack[ctx->output_stack_len];
    ctx->indent = ctx->indent_stack[ctx->output_stack_len];
} // pop_output



// Shader model version magic...

static inline uint32 ver_ui32(const uint8 major, const uint8 minor)
{
    return ( (((uint32) major) << 16) | (((minor) == 0xFF) ? 1 : (minor)) );
} // version_ui32

static inline int shader_version_supported(const uint8 maj, const uint8 min)
{
    return (ver_ui32(maj,min) <= ver_ui32(MAX_SHADER_MAJOR, MAX_SHADER_MINOR));
} // shader_version_supported

static inline int shader_version_atleast(const Context *ctx, const uint8 maj,
                                         const uint8 min)
{
    return (ver_ui32(ctx->major_ver, ctx->minor_ver) >= ver_ui32(maj, min));
} // shader_version_atleast

static inline int shader_version_exactly(const Context *ctx, const uint8 maj,
                                         const uint8 min)
{
    return ((ctx->major_ver == maj) && (ctx->minor_ver == min));
} // shader_version_exactly

static inline int shader_is_pixel(const Context *ctx)
{
    return (ctx->shader_type == MOJOSHADER_TYPE_PIXEL);
} // shader_is_pixel

static inline int shader_is_vertex(const Context *ctx)
{
    return (ctx->shader_type == MOJOSHADER_TYPE_VERTEX);
} // shader_is_vertex


static inline int isfail(const Context *ctx)
{
    return ctx->isfail;
} // isfail


static void failf(Context *ctx, const char *fmt, ...) ISPRINTF(2,3);
static void failf(Context *ctx, const char *fmt, ...)
{
    ctx->isfail = 1;
    if (ctx->out_of_memory)
        return;

    // no filename at this level (we pass a NULL to errorlist_add_va()...)
    va_list ap;
    va_start(ap, fmt);
    errorlist_add_va(ctx->errors, NULL, ctx->current_position, fmt, ap);
    va_end(ap);
} // failf


static inline void fail(Context *ctx, const char *reason)
{
    failf(ctx, "%s", reason);
} // fail


static void output_line(Context *ctx, const char *fmt, ...) ISPRINTF(2,3);
static void output_line(Context *ctx, const char *fmt, ...)
{
    assert(ctx->output != NULL);
    if (isfail(ctx))
        return;  // we failed previously, don't go on...

    const int indent = ctx->indent;
    if (indent > 0)
    {
        char *indentbuf = (char *) alloca(indent);
        memset(indentbuf, '\t', indent);
        buffer_append(ctx->output, indentbuf, indent);
    } // if

    va_list ap;
    va_start(ap, fmt);
    buffer_append_va(ctx->output, fmt, ap);
    va_end(ap);

    buffer_append(ctx->output, ctx->endline, ctx->endline_len);
} // output_line


static inline void output_blank_line(Context *ctx)
{
    assert(ctx->output != NULL);
    if (!isfail(ctx))
        buffer_append(ctx->output, ctx->endline, ctx->endline_len);
} // output_blank_line


// !!! FIXME: this is sort of nasty.
static void floatstr(Context *ctx, char *buf, size_t bufsize, float f,
                     int leavedecimal)
{
    const size_t len = snprintf(buf, bufsize, "%f", f);
    if ((len+2) >= bufsize)
        fail(ctx, "BUG: internal buffer is too small");
    else
    {
        char *end = buf + len;
        char *ptr = strchr(buf, '.');
        if (ptr == NULL)
        {
            if (leavedecimal)
                strcat(buf, ".0");
            return;  // done.
        } // if

        while (--end != ptr)
        {
            if (*end != '0')
            {
                end++;
                break;
            } // if
        } // while
        if ((leavedecimal) && (end == ptr))
            end += 2;
        *end = '\0';  // chop extra '0' or all decimal places off.
    } // else
} // floatstr


// Deal with register lists...  !!! FIXME: I sort of hate this.

static void free_reglist(MOJOSHADER_free f, void *d, RegisterList *item)
{
    while (item != NULL)
    {
        RegisterList *next = item->next;
        f(item, d);
        item = next;
    } // while
} // free_reglist

static inline uint32 reg_to_ui32(const RegisterType regtype, const int regnum)
{
    return ( ((uint32) regtype) | (((uint32) regnum) << 16) );
} // reg_to_uint32

// !!! FIXME: ditch this for a hash table.
static RegisterList *reglist_insert(Context *ctx, RegisterList *prev,
                                    const RegisterType regtype,
                                    const int regnum)
{
    const uint32 newval = reg_to_ui32(regtype, regnum);
    RegisterList *item = prev->next;
    while (item != NULL)
    {
        const uint32 val = reg_to_ui32(item->regtype, item->regnum);
        if (newval == val)
            return item;  // already set, so we're done.
        else if (newval < val)  // insert it here.
            break;
        else // if (newval > val)
        {
            // keep going, we're not to the insertion point yet.
            prev = item;
            item = item->next;
        } // else
    } // while

    // we need to insert an entry after (prev).
    item = (RegisterList *) Malloc(ctx, sizeof (RegisterList));
    if (item != NULL)
    {
        item->regtype = regtype;
        item->regnum = regnum;
        item->usage = MOJOSHADER_USAGE_UNKNOWN;
        item->index = 0;
        item->writemask = 0;
        item->misc = 0;
        item->array = NULL;
        item->next = prev->next;
        prev->next = item;
    } // if

    return item;
} // reglist_insert

static RegisterList *reglist_find(const RegisterList *prev,
                                  const RegisterType rtype, const int regnum)
{
    const uint32 newval = reg_to_ui32(rtype, regnum);
    RegisterList *item = prev->next;
    while (item != NULL)
    {
        const uint32 val = reg_to_ui32(item->regtype, item->regnum);
        if (newval == val)
            return item;  // here it is.
        else if (newval < val)  // should have been here if it existed.
            return NULL;
        else // if (newval > val)
            item = item->next;
    } // while

    return NULL;  // wasn't in the list.
} // reglist_find

static inline const RegisterList *reglist_exists(RegisterList *prev,
                                                 const RegisterType regtype,
                                                 const int regnum)
{
    return (reglist_find(prev, regtype, regnum));
} // reglist_exists

static inline void set_used_register(Context *ctx, const RegisterType regtype,
                                     const int regnum)
{
    if ((regtype == REG_TYPE_COLOROUT) && (regnum > 0))
        ctx->have_multi_color_outputs = 1;
    reglist_insert(ctx, &ctx->used_registers, regtype, regnum);
} // set_used_register

static inline int get_used_register(Context *ctx, const RegisterType regtype,
                                    const int regnum)
{
    return (reglist_exists(&ctx->used_registers, regtype, regnum) != NULL);
} // get_used_register

static inline void set_defined_register(Context *ctx, const RegisterType rtype,
                                        const int regnum)
{
    reglist_insert(ctx, &ctx->defined_registers, rtype, regnum);
} // set_defined_register

static inline int get_defined_register(Context *ctx, const RegisterType rtype,
                                       const int regnum)
{
    return (reglist_exists(&ctx->defined_registers, rtype, regnum) != NULL);
} // get_defined_register

static void add_attribute_register(Context *ctx, const RegisterType rtype,
                                const int regnum, const MOJOSHADER_usage usage,
                                const int index, const int writemask, int flags)
{
    RegisterList *item = reglist_insert(ctx, &ctx->attributes, rtype, regnum);
    item->usage = usage;
    item->index = index;
    item->writemask = writemask;
    item->misc = flags;

    if ((rtype == REG_TYPE_OUTPUT) && (usage == MOJOSHADER_USAGE_POINTSIZE))
        ctx->uses_pointsize = 1;  // note that we have to check this later.
} // add_attribute_register

static inline void add_sampler(Context *ctx, const RegisterType rtype,
                               const int regnum, const TextureType ttype)
{
    // !!! FIXME: make sure it doesn't exist?
    RegisterList *item = reglist_insert(ctx, &ctx->samplers, rtype, regnum);
    item->index = (int) ttype;
} // add_sampler


static inline int writemask_xyzw(const int writemask)
{
    return (writemask == 0xF);  // 0xF == 1111. No explicit mask (full!).
} // writemask_xyzw


static inline int writemask_xyz(const int writemask)
{
    return (writemask == 0x7);  // 0x7 == 0111. (that is: xyz)
} // writemask_xyz


static inline int writemask_xy(const int writemask)
{
    return (writemask == 0x3);  // 0x3 == 0011. (that is: xy)
} // writemask_xy


static inline int writemask_x(const int writemask)
{
    return (writemask == 0x1);  // 0x1 == 0001. (that is: x)
} // writemask_x


static inline int writemask_y(const int writemask)
{
    return (writemask == 0x2);  // 0x1 == 0010. (that is: y)
} // writemask_y


static inline int replicate_swizzle(const int swizzle)
{
    return ( (((swizzle >> 0) & 0x3) == ((swizzle >> 2) & 0x3)) &&
             (((swizzle >> 2) & 0x3) == ((swizzle >> 4) & 0x3)) &&
             (((swizzle >> 4) & 0x3) == ((swizzle >> 6) & 0x3)) );
} // replicate_swizzle


static inline int no_swizzle(const int swizzle)
{
    return (swizzle == 0xE4);  // 0xE4 == 11100100 ... 0 1 2 3. No swizzle.
} // no_swizzle


static inline int vecsize_from_writemask(const int m)
{
    return (m & 1) + ((m >> 1) & 1) + ((m >> 2) & 1) + ((m >> 3) & 1);
} // vecsize_from_writemask

static int allocate_scratch_register(Context *ctx)
{
    const int retval = ctx->scratch_registers++;
    if (retval >= ctx->max_scratch_registers)
        ctx->max_scratch_registers = retval + 1;
    return retval;
} // allocate_scratch_register

static int allocate_branch_label(Context *ctx)
{
    return ctx->assigned_branch_labels++;
} // allocate_branch_label

static inline void adjust_token_position(Context *ctx, const int incr)
{
    ctx->tokens += incr;
    ctx->tokencount -= incr;
    ctx->current_position += incr * sizeof (uint32);
} // adjust_token_position


// D3D stuff that's used in more than just the d3d profile...

static int isscalar(Context *ctx, const MOJOSHADER_shaderType shader_type,
                    const RegisterType rtype, const int rnum)
{
    if ((rtype == REG_TYPE_OUTPUT) && (ctx->uses_pointsize))
    {
        const RegisterList *reg = reglist_find(&ctx->attributes, rtype, rnum);
        if (reg != NULL)
            return (reg->usage == MOJOSHADER_USAGE_POINTSIZE);
    } // if

    return scalar_register(shader_type, rtype, rnum);
} // isscalar

static const char swizzle_channels[] = { 'x', 'y', 'z', 'w' };


static const char *usagestrs[] = {
    "_position", "_blendweight", "_blendindices", "_normal", "_psize",
    "_texcoord", "_tangent", "_binormal", "_tessfactor", "_positiont",
    "_color", "_fog", "_depth", "_sample"
};

static const char *get_D3D_register_string(Context *ctx,
                                           RegisterType regtype,
                                           int regnum, char *regnum_str,
                                           size_t regnum_size)
{
    const char *retval = NULL;
    int has_number = 1;

    switch (regtype)
    {
        case REG_TYPE_TEMP:
            retval = "r";
            break;

        case REG_TYPE_INPUT:
            retval = "v";
            break;

        case REG_TYPE_CONST:
            retval = "c";
            break;

        case REG_TYPE_ADDRESS:  // (or REG_TYPE_TEXTURE, same value.)
            retval = shader_is_vertex(ctx) ? "a" : "t";
            break;

        case REG_TYPE_RASTOUT:
            switch ((RastOutType) regnum)
            {
                case RASTOUT_TYPE_POSITION: retval = "oPos"; break;
                case RASTOUT_TYPE_FOG: retval = "oFog"; break;
                case RASTOUT_TYPE_POINT_SIZE: retval = "oPts"; break;
            } // switch
            has_number = 0;
            break;

        case REG_TYPE_ATTROUT:
            retval = "oD";
            break;

        case REG_TYPE_OUTPUT: // (or REG_TYPE_TEXCRDOUT, same value.)
            if (shader_is_vertex(ctx) && shader_version_atleast(ctx, 3, 0))
                retval = "o";
            else
                retval = "oT";
            break;

        case REG_TYPE_CONSTINT:
            retval = "i";
            break;

        case REG_TYPE_COLOROUT:
            retval = "oC";
            break;

        case REG_TYPE_DEPTHOUT:
            retval = "oDepth";
            has_number = 0;
            break;

        case REG_TYPE_SAMPLER:
            retval = "s";
            break;

        case REG_TYPE_CONSTBOOL:
            retval = "b";
            break;

        case REG_TYPE_LOOP:
            retval = "aL";
            has_number = 0;
            break;

        case REG_TYPE_MISCTYPE:
            switch ((const MiscTypeType) regnum)
            {
                case MISCTYPE_TYPE_POSITION: retval = "vPos"; break;
                case MISCTYPE_TYPE_FACE: retval = "vFace"; break;
            } // switch
            has_number = 0;
            break;

        case REG_TYPE_LABEL:
            retval = "l";
            break;

        case REG_TYPE_PREDICATE:
            retval = "p";
            break;

        //case REG_TYPE_TEMPFLOAT16:  // !!! FIXME: don't know this asm string
        default:
            fail(ctx, "unknown register type");
            retval = "???";
            has_number = 0;
            break;
    } // switch

    if (has_number)
        snprintf(regnum_str, regnum_size, "%u", (uint) regnum);
    else
        regnum_str[0] = '\0';

    return retval;
} // get_D3D_register_string


// !!! FIXME: can we split the profile code out to separate source files?

#define AT_LEAST_ONE_PROFILE 0

#if !SUPPORT_PROFILE_D3D
#define PROFILE_EMITTER_D3D(op)
#else
#undef AT_LEAST_ONE_PROFILE
#define AT_LEAST_ONE_PROFILE 1
#define PROFILE_EMITTER_D3D(op) emit_D3D_##op,

static const char *make_D3D_srcarg_string_in_buf(Context *ctx,
                                                 const SourceArgInfo *arg,
                                                 char *buf, size_t buflen)
{
    const char *premod_str = "";
    const char *postmod_str = "";
    switch (arg->src_mod)
    {
        case SRCMOD_NEGATE:
            premod_str = "-";
            break;

        case SRCMOD_BIASNEGATE:
            premod_str = "-";
            // fall through.
        case SRCMOD_BIAS:
            postmod_str = "_bias";
            break;

        case SRCMOD_SIGNNEGATE:
            premod_str = "-";
            // fall through.
        case SRCMOD_SIGN:
            postmod_str = "_bx2";
            break;

        case SRCMOD_COMPLEMENT:
            premod_str = "1-";
            break;

        case SRCMOD_X2NEGATE:
            premod_str = "-";
            // fall through.
        case SRCMOD_X2:
            postmod_str = "_x2";
            break;

        case SRCMOD_DZ:
            postmod_str = "_dz";
            break;

        case SRCMOD_DW:
            postmod_str = "_dw";
            break;

        case SRCMOD_ABSNEGATE:
            premod_str = "-";
            // fall through.
        case SRCMOD_ABS:
            postmod_str = "_abs";
            break;

        case SRCMOD_NOT:
            premod_str = "!";
            break;

        case SRCMOD_NONE:
        case SRCMOD_TOTAL:
             break;  // stop compiler whining.
    } // switch


    char regnum_str[16];
    const char *regtype_str = get_D3D_register_string(ctx, arg->regtype,
                                                      arg->regnum, regnum_str,
                                                      sizeof (regnum_str));

    if (regtype_str == NULL)
    {
        fail(ctx, "Unknown source register type.");
        *buf = '\0';
        return buf;
    } // if

    const char *rel_lbracket = "";
    const char *rel_rbracket = "";
    char rel_swizzle[4] = { '\0' };
    char rel_regnum_str[16] = { '\0' };
    const char *rel_regtype_str = "";
    if (arg->relative)
    {
        rel_swizzle[0] = '.';
        rel_swizzle[1] = swizzle_channels[arg->relative_component];
        rel_swizzle[2] = '\0';
        rel_lbracket = "[";
        rel_rbracket = "]";
        rel_regtype_str = get_D3D_register_string(ctx, arg->relative_regtype,
                                                  arg->relative_regnum,
                                                  rel_regnum_str,
                                                  sizeof (rel_regnum_str));

        if (regtype_str == NULL)
        {
            fail(ctx, "Unknown relative source register type.");
            *buf = '\0';
            return buf;
        } // if
    } // if

    char swizzle_str[6];
    size_t i = 0;
    const int scalar = isscalar(ctx, ctx->shader_type, arg->regtype, arg->regnum);
    if (!scalar && !no_swizzle(arg->swizzle))
    {
        swizzle_str[i++] = '.';
        swizzle_str[i++] = swizzle_channels[arg->swizzle_x];
        swizzle_str[i++] = swizzle_channels[arg->swizzle_y];
        swizzle_str[i++] = swizzle_channels[arg->swizzle_z];
        swizzle_str[i++] = swizzle_channels[arg->swizzle_w];

        // .xyzz is the same as .xyz, .z is the same as .zzzz, etc.
        while (swizzle_str[i-1] == swizzle_str[i-2])
            i--;
    } // if
    swizzle_str[i] = '\0';
    assert(i < sizeof (swizzle_str));

    // !!! FIXME: c12[a0.x] actually needs to be c[a0.x + 12]
    snprintf(buf, buflen, "%s%s%s%s%s%s%s%s%s%s",
             premod_str, regtype_str, regnum_str, postmod_str,
             rel_lbracket, rel_regtype_str, rel_regnum_str, rel_swizzle,
             rel_rbracket, swizzle_str);
    // !!! FIXME: make sure the scratch buffer was large enough.
    return buf;
} // make_D3D_srcarg_string_in_buf


static const char *make_D3D_destarg_string(Context *ctx, char *buf,
                                           const size_t buflen)
{
    const DestArgInfo *arg = &ctx->dest_arg;

    const char *result_shift_str = "";
    switch (arg->result_shift)
    {
        case 0x1: result_shift_str = "_x2"; break;
        case 0x2: result_shift_str = "_x4"; break;
        case 0x3: result_shift_str = "_x8"; break;
        case 0xD: result_shift_str = "_d8"; break;
        case 0xE: result_shift_str = "_d4"; break;
        case 0xF: result_shift_str = "_d2"; break;
    } // switch

    const char *sat_str = (arg->result_mod & MOD_SATURATE) ? "_sat" : "";
    const char *pp_str = (arg->result_mod & MOD_PP) ? "_pp" : "";
    const char *cent_str = (arg->result_mod & MOD_CENTROID) ? "_centroid" : "";

    char regnum_str[16];
    const char *regtype_str = get_D3D_register_string(ctx, arg->regtype,
                                                      arg->regnum, regnum_str,
                                                      sizeof (regnum_str));
    if (regtype_str == NULL)
    {
        fail(ctx, "Unknown destination register type.");
        *buf = '\0';
        return buf;
    } // if

    char writemask_str[6];
    size_t i = 0;
    const int scalar = isscalar(ctx, ctx->shader_type, arg->regtype, arg->regnum);
    if (!scalar && !writemask_xyzw(arg->writemask))
    {
        writemask_str[i++] = '.';
        if (arg->writemask0) writemask_str[i++] = 'x';
        if (arg->writemask1) writemask_str[i++] = 'y';
        if (arg->writemask2) writemask_str[i++] = 'z';
        if (arg->writemask3) writemask_str[i++] = 'w';
    } // if
    writemask_str[i] = '\0';
    assert(i < sizeof (writemask_str));

    const char *pred_left = "";
    const char *pred_right = "";
    char pred[32] = { '\0' };
    if (ctx->predicated)
    {
        pred_left = "(";
        pred_right = ") ";
        make_D3D_srcarg_string_in_buf(ctx, &ctx->predicate_arg,
                                      pred, sizeof (pred));
    } // if

    // may turn out something like "_x2_sat_pp_centroid (!p0.x) r0.xyzw" ...
    snprintf(buf, buflen, "%s%s%s%s %s%s%s%s%s%s",
             result_shift_str, sat_str, pp_str, cent_str,
             pred_left, pred, pred_right,
             regtype_str, regnum_str, writemask_str);
    // !!! FIXME: make sure the scratch buffer was large enough.
    return buf;
} // make_D3D_destarg_string


static const char *make_D3D_srcarg_string(Context *ctx, const size_t idx,
                                          char *buf, size_t buflen)
{
    if (idx >= STATICARRAYLEN(ctx->source_args))
    {
        fail(ctx, "Too many source args");
        *buf = '\0';
        return buf;
    } // if

    const SourceArgInfo *arg = &ctx->source_args[idx];
    return make_D3D_srcarg_string_in_buf(ctx, arg, buf, buflen);
} // make_D3D_srcarg_string

static const char *get_D3D_varname_in_buf(Context *ctx, RegisterType rt,
                                           int regnum, char *buf,
                                           const size_t len)
{
    char regnum_str[16];
    const char *regtype_str = get_D3D_register_string(ctx, rt, regnum,
                                              regnum_str, sizeof (regnum_str));
    snprintf(buf,len,"%s%s", regtype_str, regnum_str);
    return buf;
} // get_D3D_varname_in_buf


static const char *get_D3D_varname(Context *ctx, RegisterType rt, int regnum)
{
    char buf[64];
    get_D3D_varname_in_buf(ctx, rt, regnum, buf, sizeof (buf));
    return StrDup(ctx, buf);
} // get_D3D_varname


static const char *get_D3D_const_array_varname(Context *ctx, int base, int size)
{
    char buf[64];
    snprintf(buf, sizeof (buf), "c_array_%d_%d", base, size);
    return StrDup(ctx, buf);
} // get_D3D_const_array_varname


static void emit_D3D_start(Context *ctx, const char *profilestr)
{
    const uint major = (uint) ctx->major_ver;
    const uint minor = (uint) ctx->minor_ver;
    char minor_str[16];

    ctx->ignores_ctab = 1;

    if (minor == 0xFF)
        strcpy(minor_str, "sw");
    else if ((major > 1) && (minor == 1))
        strcpy(minor_str, "x");  // for >= SM2, apparently this is "x". Weird.
    else
        snprintf(minor_str, sizeof (minor_str), "%u", (uint) minor);

    output_line(ctx, "%s_%u_%s", ctx->shader_type_str, major, minor_str);
} // emit_D3D_start


static void emit_D3D_end(Context *ctx)
{
    output_line(ctx, "end");
} // emit_D3D_end


static void emit_D3D_phase(Context *ctx)
{
    output_line(ctx, "phase");
} // emit_D3D_phase


static void emit_D3D_finalize(Context *ctx)
{
    // no-op.
} // emit_D3D_finalize


static void emit_D3D_global(Context *ctx, RegisterType regtype, int regnum)
{
    // no-op.
} // emit_D3D_global


static void emit_D3D_array(Context *ctx, VariableList *var)
{
    // no-op.
} // emit_D3D_array


static void emit_D3D_const_array(Context *ctx, const ConstantsList *clist,
                                 int base, int size)
{
    // no-op.
} // emit_D3D_const_array


static void emit_D3D_uniform(Context *ctx, RegisterType regtype, int regnum,
                             const VariableList *var)
{
    // no-op.
} // emit_D3D_uniform


static void emit_D3D_sampler(Context *ctx, int stage, TextureType ttype)
{
    // no-op.
} // emit_D3D_sampler


static void emit_D3D_attribute(Context *ctx, RegisterType regtype, int regnum,
                               MOJOSHADER_usage usage, int index, int wmask,
                               int flags)
{
    // no-op.
} // emit_D3D_attribute


static void emit_D3D_RESERVED(Context *ctx)
{
    // do nothing; fails in the state machine.
} // emit_D3D_RESERVED


// Generic D3D opcode emitters. A list of macros generate all the entry points
//  that call into these...

static char *lowercase(char *dst, const char *src)
{
    int i = 0;
    do
    {
        const char ch = src[i];
        dst[i] = (((ch >= 'A') && (ch <= 'Z')) ? (ch - ('A' - 'a')) : ch);
    } while (src[i++]);
    return dst;
} // lowercase


static void emit_D3D_opcode_d(Context *ctx, const char *opcode)
{
    char dst[64]; make_D3D_destarg_string(ctx, dst, sizeof (dst));
    opcode = lowercase((char *) alloca(strlen(opcode) + 1), opcode);
    output_line(ctx, "%s%s", opcode, dst);
} // emit_D3D_opcode_d


static void emit_D3D_opcode_s(Context *ctx, const char *opcode)
{
    char src0[64]; make_D3D_srcarg_string(ctx, 0, src0, sizeof (src0));
    opcode = lowercase((char *) alloca(strlen(opcode) + 1), opcode);
    output_line(ctx, "%s %s", opcode, src0);
} // emit_D3D_opcode_s


static void emit_D3D_opcode_ss(Context *ctx, const char *opcode)
{
    char src0[64]; make_D3D_srcarg_string(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_D3D_srcarg_string(ctx, 1, src1, sizeof (src1));
    opcode = lowercase((char *) alloca(strlen(opcode) + 1), opcode);
    output_line(ctx, "%s %s, %s", opcode, src0, src1);
} // emit_D3D_opcode_ss


static void emit_D3D_opcode_ds(Context *ctx, const char *opcode)
{
    char dst[64]; make_D3D_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_D3D_srcarg_string(ctx, 0, src0, sizeof (src0));
    opcode = lowercase((char *) alloca(strlen(opcode) + 1), opcode);
    output_line(ctx, "%s%s, %s", opcode, dst, src0);
} // emit_D3D_opcode_ds


static void emit_D3D_opcode_dss(Context *ctx, const char *opcode)
{
    char dst[64]; make_D3D_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_D3D_srcarg_string(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_D3D_srcarg_string(ctx, 1, src1, sizeof (src1));
    opcode = lowercase((char *) alloca(strlen(opcode) + 1), opcode);
    output_line(ctx, "%s%s, %s, %s", opcode, dst, src0, src1);
} // emit_D3D_opcode_dss


static void emit_D3D_opcode_dsss(Context *ctx, const char *opcode)
{
    char dst[64]; make_D3D_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_D3D_srcarg_string(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_D3D_srcarg_string(ctx, 1, src1, sizeof (src1));
    char src2[64]; make_D3D_srcarg_string(ctx, 2, src2, sizeof (src2));
    opcode = lowercase((char *) alloca(strlen(opcode) + 1), opcode);
    output_line(ctx, "%s%s, %s, %s, %s", opcode, dst, src0, src1, src2);
} // emit_D3D_opcode_dsss


static void emit_D3D_opcode_dssss(Context *ctx, const char *opcode)
{
    char dst[64]; make_D3D_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_D3D_srcarg_string(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_D3D_srcarg_string(ctx, 1, src1, sizeof (src1));
    char src2[64]; make_D3D_srcarg_string(ctx, 2, src2, sizeof (src2));
    char src3[64]; make_D3D_srcarg_string(ctx, 3, src3, sizeof (src3));
    opcode = lowercase((char *) alloca(strlen(opcode) + 1), opcode);
    output_line(ctx,"%s%s, %s, %s, %s, %s",opcode,dst,src0,src1,src2,src3);
} // emit_D3D_opcode_dssss


static void emit_D3D_opcode(Context *ctx, const char *opcode)
{
    opcode = lowercase((char *) alloca(strlen(opcode) + 1), opcode);
    output_line(ctx, "%s", opcode);
} // emit_D3D_opcode_dssss


#define EMIT_D3D_OPCODE_FUNC(op) \
    static void emit_D3D_##op(Context *ctx) { \
        emit_D3D_opcode(ctx, #op); \
    }
#define EMIT_D3D_OPCODE_D_FUNC(op) \
    static void emit_D3D_##op(Context *ctx) { \
        emit_D3D_opcode_d(ctx, #op); \
    }
#define EMIT_D3D_OPCODE_S_FUNC(op) \
    static void emit_D3D_##op(Context *ctx) { \
        emit_D3D_opcode_s(ctx, #op); \
    }
#define EMIT_D3D_OPCODE_SS_FUNC(op) \
    static void emit_D3D_##op(Context *ctx) { \
        emit_D3D_opcode_ss(ctx, #op); \
    }
#define EMIT_D3D_OPCODE_DS_FUNC(op) \
    static void emit_D3D_##op(Context *ctx) { \
        emit_D3D_opcode_ds(ctx, #op); \
    }
#define EMIT_D3D_OPCODE_DSS_FUNC(op) \
    static void emit_D3D_##op(Context *ctx) { \
        emit_D3D_opcode_dss(ctx, #op); \
    }
#define EMIT_D3D_OPCODE_DSSS_FUNC(op) \
    static void emit_D3D_##op(Context *ctx) { \
        emit_D3D_opcode_dsss(ctx, #op); \
    }
#define EMIT_D3D_OPCODE_DSSSS_FUNC(op) \
    static void emit_D3D_##op(Context *ctx) { \
        emit_D3D_opcode_dssss(ctx, #op); \
    }

EMIT_D3D_OPCODE_FUNC(NOP)
EMIT_D3D_OPCODE_DS_FUNC(MOV)
EMIT_D3D_OPCODE_DSS_FUNC(ADD)
EMIT_D3D_OPCODE_DSS_FUNC(SUB)
EMIT_D3D_OPCODE_DSSS_FUNC(MAD)
EMIT_D3D_OPCODE_DSS_FUNC(MUL)
EMIT_D3D_OPCODE_DS_FUNC(RCP)
EMIT_D3D_OPCODE_DS_FUNC(RSQ)
EMIT_D3D_OPCODE_DSS_FUNC(DP3)
EMIT_D3D_OPCODE_DSS_FUNC(DP4)
EMIT_D3D_OPCODE_DSS_FUNC(MIN)
EMIT_D3D_OPCODE_DSS_FUNC(MAX)
EMIT_D3D_OPCODE_DSS_FUNC(SLT)
EMIT_D3D_OPCODE_DSS_FUNC(SGE)
EMIT_D3D_OPCODE_DS_FUNC(EXP)
EMIT_D3D_OPCODE_DS_FUNC(LOG)
EMIT_D3D_OPCODE_DS_FUNC(LIT)
EMIT_D3D_OPCODE_DSS_FUNC(DST)
EMIT_D3D_OPCODE_DSSS_FUNC(LRP)
EMIT_D3D_OPCODE_DS_FUNC(FRC)
EMIT_D3D_OPCODE_DSS_FUNC(M4X4)
EMIT_D3D_OPCODE_DSS_FUNC(M4X3)
EMIT_D3D_OPCODE_DSS_FUNC(M3X4)
EMIT_D3D_OPCODE_DSS_FUNC(M3X3)
EMIT_D3D_OPCODE_DSS_FUNC(M3X2)
EMIT_D3D_OPCODE_S_FUNC(CALL)
EMIT_D3D_OPCODE_SS_FUNC(CALLNZ)
EMIT_D3D_OPCODE_SS_FUNC(LOOP)
EMIT_D3D_OPCODE_FUNC(RET)
EMIT_D3D_OPCODE_FUNC(ENDLOOP)
EMIT_D3D_OPCODE_S_FUNC(LABEL)
EMIT_D3D_OPCODE_DSS_FUNC(POW)
EMIT_D3D_OPCODE_DSS_FUNC(CRS)
EMIT_D3D_OPCODE_DSSS_FUNC(SGN)
EMIT_D3D_OPCODE_DS_FUNC(ABS)
EMIT_D3D_OPCODE_DS_FUNC(NRM)
EMIT_D3D_OPCODE_S_FUNC(REP)
EMIT_D3D_OPCODE_FUNC(ENDREP)
EMIT_D3D_OPCODE_S_FUNC(IF)
EMIT_D3D_OPCODE_FUNC(ELSE)
EMIT_D3D_OPCODE_FUNC(ENDIF)
EMIT_D3D_OPCODE_FUNC(BREAK)
EMIT_D3D_OPCODE_DS_FUNC(MOVA)
EMIT_D3D_OPCODE_D_FUNC(TEXKILL)
EMIT_D3D_OPCODE_DS_FUNC(TEXBEM)
EMIT_D3D_OPCODE_DS_FUNC(TEXBEML)
EMIT_D3D_OPCODE_DS_FUNC(TEXREG2AR)
EMIT_D3D_OPCODE_DS_FUNC(TEXREG2GB)
EMIT_D3D_OPCODE_DS_FUNC(TEXM3X2PAD)
EMIT_D3D_OPCODE_DS_FUNC(TEXM3X2TEX)
EMIT_D3D_OPCODE_DS_FUNC(TEXM3X3PAD)
EMIT_D3D_OPCODE_DS_FUNC(TEXM3X3TEX)
EMIT_D3D_OPCODE_DSS_FUNC(TEXM3X3SPEC)
EMIT_D3D_OPCODE_DS_FUNC(TEXM3X3VSPEC)
EMIT_D3D_OPCODE_DS_FUNC(EXPP)
EMIT_D3D_OPCODE_DS_FUNC(LOGP)
EMIT_D3D_OPCODE_DSSS_FUNC(CND)
EMIT_D3D_OPCODE_DS_FUNC(TEXREG2RGB)
EMIT_D3D_OPCODE_DS_FUNC(TEXDP3TEX)
EMIT_D3D_OPCODE_DS_FUNC(TEXM3X2DEPTH)
EMIT_D3D_OPCODE_DS_FUNC(TEXDP3)
EMIT_D3D_OPCODE_DS_FUNC(TEXM3X3)
EMIT_D3D_OPCODE_D_FUNC(TEXDEPTH)
EMIT_D3D_OPCODE_DSSS_FUNC(CMP)
EMIT_D3D_OPCODE_DSS_FUNC(BEM)
EMIT_D3D_OPCODE_DSSS_FUNC(DP2ADD)
EMIT_D3D_OPCODE_DS_FUNC(DSX)
EMIT_D3D_OPCODE_DS_FUNC(DSY)
EMIT_D3D_OPCODE_DSSSS_FUNC(TEXLDD)
EMIT_D3D_OPCODE_DSS_FUNC(TEXLDL)
EMIT_D3D_OPCODE_S_FUNC(BREAKP)

// special cases for comparison opcodes...
static const char *get_D3D_comparison_string(Context *ctx)
{
    static const char *comps[] = {
        "", "_gt", "_eq", "_ge", "_lt", "_ne", "_le"
    };

    if (ctx->instruction_controls >= STATICARRAYLEN(comps))
    {
        fail(ctx, "unknown comparison control");
        return "";
    } // if

    return comps[ctx->instruction_controls];
} // get_D3D_comparison_string

static void emit_D3D_BREAKC(Context *ctx)
{
    char op[16];
    snprintf(op, sizeof (op), "break%s", get_D3D_comparison_string(ctx));
    emit_D3D_opcode_ss(ctx, op);
} // emit_D3D_BREAKC

static void emit_D3D_IFC(Context *ctx)
{
    char op[16];
    snprintf(op, sizeof (op), "if%s", get_D3D_comparison_string(ctx));
    emit_D3D_opcode_ss(ctx, op);
} // emit_D3D_IFC

static void emit_D3D_SETP(Context *ctx)
{
    char op[16];
    snprintf(op, sizeof (op), "setp%s", get_D3D_comparison_string(ctx));
    emit_D3D_opcode_dss(ctx, op);
} // emit_D3D_SETP

static void emit_D3D_DEF(Context *ctx)
{
    char dst[64];
    make_D3D_destarg_string(ctx, dst, sizeof (dst));
    const float *val = (const float *) ctx->dwords; // !!! FIXME: could be int?
    char val0[32];
    char val1[32];
    char val2[32];
    char val3[32];
    floatstr(ctx, val0, sizeof (val0), val[0], 0);
    floatstr(ctx, val1, sizeof (val1), val[1], 0);
    floatstr(ctx, val2, sizeof (val2), val[2], 0);
    floatstr(ctx, val3, sizeof (val3), val[3], 0);
    output_line(ctx, "def%s, %s, %s, %s, %s", dst, val0, val1, val2, val3);
} // emit_D3D_DEF

static void emit_D3D_DEFI(Context *ctx)
{
    char dst[64];
    make_D3D_destarg_string(ctx, dst, sizeof (dst));
    const int32 *x = (const int32 *) ctx->dwords;
    output_line(ctx, "defi%s, %d, %d, %d, %d", dst,
                (int) x[0], (int) x[1], (int) x[2], (int) x[3]);
} // emit_D3D_DEFI

static void emit_D3D_DEFB(Context *ctx)
{
    char dst[64];
    make_D3D_destarg_string(ctx, dst, sizeof (dst));
    output_line(ctx, "defb%s, %s", dst, ctx->dwords[0] ? "true" : "false");
} // emit_D3D_DEFB


static void emit_D3D_DCL(Context *ctx)
{
    char dst[64];
    make_D3D_destarg_string(ctx, dst, sizeof (dst));
    const DestArgInfo *arg = &ctx->dest_arg;
    const char *usage_str = "";
    char index_str[16] = { '\0' };

    if (arg->regtype == REG_TYPE_SAMPLER)
    {
        switch ((const TextureType) ctx->dwords[0])
        {
            case TEXTURE_TYPE_2D: usage_str = "_2d"; break;
            case TEXTURE_TYPE_CUBE: usage_str = "_cube"; break;
            case TEXTURE_TYPE_VOLUME: usage_str = "_volume"; break;
            default: fail(ctx, "unknown sampler texture type"); return;
        } // switch
    } // if

    else if (arg->regtype == REG_TYPE_MISCTYPE)
    {
        switch ((const MiscTypeType) arg->regnum)
        {
            case MISCTYPE_TYPE_POSITION:
            case MISCTYPE_TYPE_FACE:
                usage_str = "";  // just become "dcl vFace" or whatever.
                break;
            default: fail(ctx, "unknown misc register type"); return;
        } // switch
    } // else if

    else
    {
        const uint32 usage = ctx->dwords[0];
        const uint32 index = ctx->dwords[1];
        usage_str = usagestrs[usage];
        if (index != 0)
            snprintf(index_str, sizeof (index_str), "%u", (uint) index);
    } // else

    output_line(ctx, "dcl%s%s%s", usage_str, index_str, dst);
} // emit_D3D_DCL


static void emit_D3D_TEXCRD(Context *ctx)
{
    // this opcode looks and acts differently depending on the shader model.
    if (shader_version_atleast(ctx, 1, 4))
        emit_D3D_opcode_ds(ctx, "texcrd");
    else
        emit_D3D_opcode_d(ctx, "texcoord");
} // emit_D3D_TEXCOORD

static void emit_D3D_TEXLD(Context *ctx)
{
    // this opcode looks and acts differently depending on the shader model.
    if (shader_version_atleast(ctx, 2, 0))
    {
        if (ctx->instruction_controls == CONTROL_TEXLD)
           emit_D3D_opcode_dss(ctx, "texld");
        else if (ctx->instruction_controls == CONTROL_TEXLDP)
           emit_D3D_opcode_dss(ctx, "texldp");
        else if (ctx->instruction_controls == CONTROL_TEXLDB)
           emit_D3D_opcode_dss(ctx, "texldb");
    } // if

    else if (shader_version_atleast(ctx, 1, 4))
    {
        emit_D3D_opcode_ds(ctx, "texld");
    } // else if

    else
    {
        emit_D3D_opcode_d(ctx, "tex");
    } // else
} // emit_D3D_TEXLD

static void emit_D3D_SINCOS(Context *ctx)
{
    // this opcode needs extra registers for sm2 and lower.
    if (!shader_version_atleast(ctx, 3, 0))
        emit_D3D_opcode_dsss(ctx, "sincos");
    else
        emit_D3D_opcode_ds(ctx, "sincos");
} // emit_D3D_SINCOS


#undef EMIT_D3D_OPCODE_FUNC
#undef EMIT_D3D_OPCODE_D_FUNC
#undef EMIT_D3D_OPCODE_S_FUNC
#undef EMIT_D3D_OPCODE_SS_FUNC
#undef EMIT_D3D_OPCODE_DS_FUNC
#undef EMIT_D3D_OPCODE_DSS_FUNC
#undef EMIT_D3D_OPCODE_DSSS_FUNC
#undef EMIT_D3D_OPCODE_DSSSS_FUNC

#endif  // SUPPORT_PROFILE_D3D


#if !SUPPORT_PROFILE_BYTECODE
#define PROFILE_EMITTER_BYTECODE(op)
#else
#undef AT_LEAST_ONE_PROFILE
#define AT_LEAST_ONE_PROFILE 1
#define PROFILE_EMITTER_BYTECODE(op) emit_BYTECODE_##op,

static void emit_BYTECODE_start(Context *ctx, const char *profilestr)
{
    ctx->ignores_ctab = 1;

    // just copy the whole token stream and make all other emitters no-ops.
    if (set_output(ctx, &ctx->mainline))
    {
        const size_t len = ctx->tokencount * sizeof (uint32);
        buffer_append(ctx->mainline, (const char *) ctx->tokens, len);
    } // if
} // emit_BYTECODE_start

static void emit_BYTECODE_end(Context *ctx) {}
static void emit_BYTECODE_phase(Context *ctx) {}
static void emit_BYTECODE_finalize(Context *ctx) {}
static void emit_BYTECODE_global(Context *ctx, RegisterType t, int n) {}
static void emit_BYTECODE_array(Context *ctx, VariableList *var) {}
static void emit_BYTECODE_sampler(Context *ctx, int s, TextureType ttype) {}
static void emit_BYTECODE_const_array(Context *ctx, const ConstantsList *c,
                                         int base, int size) {}
static void emit_BYTECODE_uniform(Context *ctx, RegisterType t, int n,
                                  const VariableList *var) {}
static void emit_BYTECODE_attribute(Context *ctx, RegisterType t, int n,
                                       MOJOSHADER_usage u, int i, int w,
                                       int f) {}

static const char *get_BYTECODE_varname(Context *ctx, RegisterType rt, int regnum)
{
    char regnum_str[16];
    const char *regtype_str = get_D3D_register_string(ctx, rt, regnum,
                                              regnum_str, sizeof (regnum_str));
    char buf[64];
    snprintf(buf, sizeof (buf), "%s%s", regtype_str, regnum_str);
    return StrDup(ctx, buf);
} // get_BYTECODE_varname

static const char *get_BYTECODE_const_array_varname(Context *ctx, int base, int size)
{
    char buf[64];
    snprintf(buf, sizeof (buf), "c_array_%d_%d", base, size);
    return StrDup(ctx, buf);
} // get_BYTECODE_const_array_varname

#define EMIT_BYTECODE_OPCODE_FUNC(op) \
    static void emit_BYTECODE_##op(Context *ctx) {}

EMIT_BYTECODE_OPCODE_FUNC(RESERVED)
EMIT_BYTECODE_OPCODE_FUNC(NOP)
EMIT_BYTECODE_OPCODE_FUNC(MOV)
EMIT_BYTECODE_OPCODE_FUNC(ADD)
EMIT_BYTECODE_OPCODE_FUNC(SUB)
EMIT_BYTECODE_OPCODE_FUNC(MAD)
EMIT_BYTECODE_OPCODE_FUNC(MUL)
EMIT_BYTECODE_OPCODE_FUNC(RCP)
EMIT_BYTECODE_OPCODE_FUNC(RSQ)
EMIT_BYTECODE_OPCODE_FUNC(DP3)
EMIT_BYTECODE_OPCODE_FUNC(DP4)
EMIT_BYTECODE_OPCODE_FUNC(MIN)
EMIT_BYTECODE_OPCODE_FUNC(MAX)
EMIT_BYTECODE_OPCODE_FUNC(SLT)
EMIT_BYTECODE_OPCODE_FUNC(SGE)
EMIT_BYTECODE_OPCODE_FUNC(EXP)
EMIT_BYTECODE_OPCODE_FUNC(LOG)
EMIT_BYTECODE_OPCODE_FUNC(LIT)
EMIT_BYTECODE_OPCODE_FUNC(DST)
EMIT_BYTECODE_OPCODE_FUNC(LRP)
EMIT_BYTECODE_OPCODE_FUNC(FRC)
EMIT_BYTECODE_OPCODE_FUNC(M4X4)
EMIT_BYTECODE_OPCODE_FUNC(M4X3)
EMIT_BYTECODE_OPCODE_FUNC(M3X4)
EMIT_BYTECODE_OPCODE_FUNC(M3X3)
EMIT_BYTECODE_OPCODE_FUNC(M3X2)
EMIT_BYTECODE_OPCODE_FUNC(CALL)
EMIT_BYTECODE_OPCODE_FUNC(CALLNZ)
EMIT_BYTECODE_OPCODE_FUNC(LOOP)
EMIT_BYTECODE_OPCODE_FUNC(RET)
EMIT_BYTECODE_OPCODE_FUNC(ENDLOOP)
EMIT_BYTECODE_OPCODE_FUNC(LABEL)
EMIT_BYTECODE_OPCODE_FUNC(POW)
EMIT_BYTECODE_OPCODE_FUNC(CRS)
EMIT_BYTECODE_OPCODE_FUNC(SGN)
EMIT_BYTECODE_OPCODE_FUNC(ABS)
EMIT_BYTECODE_OPCODE_FUNC(NRM)
EMIT_BYTECODE_OPCODE_FUNC(SINCOS)
EMIT_BYTECODE_OPCODE_FUNC(REP)
EMIT_BYTECODE_OPCODE_FUNC(ENDREP)
EMIT_BYTECODE_OPCODE_FUNC(IF)
EMIT_BYTECODE_OPCODE_FUNC(ELSE)
EMIT_BYTECODE_OPCODE_FUNC(ENDIF)
EMIT_BYTECODE_OPCODE_FUNC(BREAK)
EMIT_BYTECODE_OPCODE_FUNC(MOVA)
EMIT_BYTECODE_OPCODE_FUNC(TEXKILL)
EMIT_BYTECODE_OPCODE_FUNC(TEXBEM)
EMIT_BYTECODE_OPCODE_FUNC(TEXBEML)
EMIT_BYTECODE_OPCODE_FUNC(TEXREG2AR)
EMIT_BYTECODE_OPCODE_FUNC(TEXREG2GB)
EMIT_BYTECODE_OPCODE_FUNC(TEXM3X2PAD)
EMIT_BYTECODE_OPCODE_FUNC(TEXM3X2TEX)
EMIT_BYTECODE_OPCODE_FUNC(TEXM3X3PAD)
EMIT_BYTECODE_OPCODE_FUNC(TEXM3X3TEX)
EMIT_BYTECODE_OPCODE_FUNC(TEXM3X3SPEC)
EMIT_BYTECODE_OPCODE_FUNC(TEXM3X3VSPEC)
EMIT_BYTECODE_OPCODE_FUNC(EXPP)
EMIT_BYTECODE_OPCODE_FUNC(LOGP)
EMIT_BYTECODE_OPCODE_FUNC(CND)
EMIT_BYTECODE_OPCODE_FUNC(TEXREG2RGB)
EMIT_BYTECODE_OPCODE_FUNC(TEXDP3TEX)
EMIT_BYTECODE_OPCODE_FUNC(TEXM3X2DEPTH)
EMIT_BYTECODE_OPCODE_FUNC(TEXDP3)
EMIT_BYTECODE_OPCODE_FUNC(TEXM3X3)
EMIT_BYTECODE_OPCODE_FUNC(TEXDEPTH)
EMIT_BYTECODE_OPCODE_FUNC(CMP)
EMIT_BYTECODE_OPCODE_FUNC(BEM)
EMIT_BYTECODE_OPCODE_FUNC(DP2ADD)
EMIT_BYTECODE_OPCODE_FUNC(DSX)
EMIT_BYTECODE_OPCODE_FUNC(DSY)
EMIT_BYTECODE_OPCODE_FUNC(TEXLDD)
EMIT_BYTECODE_OPCODE_FUNC(TEXLDL)
EMIT_BYTECODE_OPCODE_FUNC(BREAKP)
EMIT_BYTECODE_OPCODE_FUNC(BREAKC)
EMIT_BYTECODE_OPCODE_FUNC(IFC)
EMIT_BYTECODE_OPCODE_FUNC(SETP)
EMIT_BYTECODE_OPCODE_FUNC(DEF)
EMIT_BYTECODE_OPCODE_FUNC(DEFI)
EMIT_BYTECODE_OPCODE_FUNC(DEFB)
EMIT_BYTECODE_OPCODE_FUNC(DCL)
EMIT_BYTECODE_OPCODE_FUNC(TEXCRD)
EMIT_BYTECODE_OPCODE_FUNC(TEXLD)

#undef EMIT_BYTECODE_OPCODE_FUNC

#endif  // SUPPORT_PROFILE_BYTECODE


#if !SUPPORT_PROFILE_GLSL
#define PROFILE_EMITTER_GLSL(op)
#else
#undef AT_LEAST_ONE_PROFILE
#define AT_LEAST_ONE_PROFILE 1
#define PROFILE_EMITTER_GLSL(op) emit_GLSL_##op,

#define EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(op) \
    static void emit_GLSL_##op(Context *ctx) { \
        fail(ctx, #op " unimplemented in glsl profile"); \
    }

static inline const char *get_GLSL_register_string(Context *ctx,
                        const RegisterType regtype, const int regnum,
                        char *regnum_str, const size_t regnum_size)
{
    // turns out these are identical at the moment.
    return get_D3D_register_string(ctx,regtype,regnum,regnum_str,regnum_size);
} // get_GLSL_register_string

static const char *get_GLSL_uniform_type(Context *ctx, const RegisterType rtype)
{
    switch (rtype)
    {
        case REG_TYPE_CONST: return "vec4";
        case REG_TYPE_CONSTINT: return "ivec4";
        case REG_TYPE_CONSTBOOL: return "bool";
        default: fail(ctx, "BUG: used a uniform we don't know how to define.");
    } // switch

    return NULL;
} // get_GLSL_uniform_type

static const char *get_GLSL_varname_in_buf(Context *ctx, RegisterType rt,
                                           int regnum, char *buf,
                                           const size_t len)
{
    char regnum_str[16];
    const char *regtype_str = get_GLSL_register_string(ctx, rt, regnum,
                                              regnum_str, sizeof (regnum_str));
    snprintf(buf,len,"%s_%s%s", ctx->shader_type_str, regtype_str, regnum_str);
    return buf;
} // get_GLSL_varname_in_buf


static const char *get_GLSL_varname(Context *ctx, RegisterType rt, int regnum)
{
    char buf[64];
    get_GLSL_varname_in_buf(ctx, rt, regnum, buf, sizeof (buf));
    return StrDup(ctx, buf);
} // get_GLSL_varname


static inline const char *get_GLSL_const_array_varname_in_buf(Context *ctx,
                                                const int base, const int size,
                                                char *buf, const size_t buflen)
{
    const char *type = ctx->shader_type_str;
    snprintf(buf, buflen, "%s_const_array_%d_%d", type, base, size);
    return buf;
} // get_GLSL_const_array_varname_in_buf

static const char *get_GLSL_const_array_varname(Context *ctx, int base, int size)
{
    char buf[64];
    get_GLSL_const_array_varname_in_buf(ctx, base, size, buf, sizeof (buf));
    return StrDup(ctx, buf);
} // get_GLSL_const_array_varname


static inline const char *get_GLSL_input_array_varname(Context *ctx,
                                                char *buf, const size_t buflen)
{
    snprintf(buf, buflen, "%s", "vertex_input_array");
    return buf;
} // get_GLSL_input_array_varname


static const char *get_GLSL_uniform_array_varname(Context *ctx,
                                                  const RegisterType regtype,
                                                  char *buf, const size_t len)
{
    const char *shadertype = ctx->shader_type_str;
    const char *type = get_GLSL_uniform_type(ctx, regtype);
    snprintf(buf, len, "%s_uniforms_%s", shadertype, type);
    return buf;
} // get_GLSL_uniform_array_varname

static const char *get_GLSL_destarg_varname(Context *ctx, char *buf, size_t len)
{
    const DestArgInfo *arg = &ctx->dest_arg;
    return get_GLSL_varname_in_buf(ctx, arg->regtype, arg->regnum, buf, len);
} // get_GLSL_destarg_varname

static const char *get_GLSL_srcarg_varname(Context *ctx, const size_t idx,
                                           char *buf, size_t len)
{
    if (idx >= STATICARRAYLEN(ctx->source_args))
    {
        fail(ctx, "Too many source args");
        *buf = '\0';
        return buf;
    } // if

    const SourceArgInfo *arg = &ctx->source_args[idx];
    return get_GLSL_varname_in_buf(ctx, arg->regtype, arg->regnum, buf, len);
} // get_GLSL_srcarg_varname


static const char *make_GLSL_destarg_assign(Context *, char *, const size_t,
                                            const char *, ...) ISPRINTF(4,5);

static const char *make_GLSL_destarg_assign(Context *ctx, char *buf,
                                            const size_t buflen,
                                            const char *fmt, ...)
{
    int need_parens = 0;
    const DestArgInfo *arg = &ctx->dest_arg;

    if (arg->writemask == 0)
    {
        *buf = '\0';
        return buf;  // no writemask? It's a no-op.
    } // if

    char clampbuf[32] = { '\0' };
    const char *clampleft = "";
    const char *clampright = "";
    if (arg->result_mod & MOD_SATURATE)
    {
        const int vecsize = vecsize_from_writemask(arg->writemask);
        clampleft = "clamp(";
        if (vecsize == 1)
            clampright = ", 0.0, 1.0)";
        else
        {
            snprintf(clampbuf, sizeof (clampbuf),
                     ", vec%d(0.0), vec%d(1.0))", vecsize, vecsize);
            clampright = clampbuf;
        } // else
    } // if

    // MSDN says MOD_PP is a hint and many implementations ignore it. So do we.

    // CENTROID only allowed in DCL opcodes, which shouldn't come through here.
    assert((arg->result_mod & MOD_CENTROID) == 0);

    if (ctx->predicated)
    {
        fail(ctx, "predicated destinations unsupported");  // !!! FIXME
        *buf = '\0';
        return buf;
    } // if

    char operation[128];
    va_list ap;
    va_start(ap, fmt);
    const int len = vsnprintf(operation, sizeof (operation), fmt, ap);
    va_end(ap);
    if (len >= sizeof (operation))
    {
        fail(ctx, "operation string too large");  // I'm lazy.  :P
        *buf = '\0';
        return buf;
    } // if

    const char *result_shift_str = "";
    switch (arg->result_shift)
    {
        case 0x1: result_shift_str = " * 2.0"; break;
        case 0x2: result_shift_str = " * 4.0"; break;
        case 0x3: result_shift_str = " * 8.0"; break;
        case 0xD: result_shift_str = " / 8.0"; break;
        case 0xE: result_shift_str = " / 4.0"; break;
        case 0xF: result_shift_str = " / 2.0"; break;
    } // switch
    need_parens |= (result_shift_str[0] != '\0');

    char regnum_str[16];
    const char *regtype_str = get_GLSL_register_string(ctx, arg->regtype,
                                                       arg->regnum, regnum_str,
                                                       sizeof (regnum_str));
    char writemask_str[6];
    size_t i = 0;
    const int scalar = isscalar(ctx, ctx->shader_type, arg->regtype, arg->regnum);
    if (!scalar && !writemask_xyzw(arg->writemask))
    {
        writemask_str[i++] = '.';
        if (arg->writemask0) writemask_str[i++] = 'x';
        if (arg->writemask1) writemask_str[i++] = 'y';
        if (arg->writemask2) writemask_str[i++] = 'z';
        if (arg->writemask3) writemask_str[i++] = 'w';
    } // if
    writemask_str[i] = '\0';
    assert(i < sizeof (writemask_str));

    const char *leftparen = (need_parens) ? "(" : "";
    const char *rightparen = (need_parens) ? ")" : "";

    snprintf(buf, buflen, "%s_%s%s%s = %s%s%s%s%s%s;",
             ctx->shader_type_str, regtype_str, regnum_str, writemask_str,
             clampleft, leftparen, operation, rightparen, result_shift_str,
             clampright);
    // !!! FIXME: make sure the scratch buffer was large enough.
    return buf;
} // make_GLSL_destarg_assign


static char *make_GLSL_swizzle_string(char *swiz_str, const size_t strsize,
                                      const int swizzle, const int writemask)
{
    size_t i = 0;
    if ( (!no_swizzle(swizzle)) || (!writemask_xyzw(writemask)) )
    {
        const int writemask0 = (writemask >> 0) & 0x1;
        const int writemask1 = (writemask >> 1) & 0x1;
        const int writemask2 = (writemask >> 2) & 0x1;
        const int writemask3 = (writemask >> 3) & 0x1;

        const int swizzle_x = (swizzle >> 0) & 0x3;
        const int swizzle_y = (swizzle >> 2) & 0x3;
        const int swizzle_z = (swizzle >> 4) & 0x3;
        const int swizzle_w = (swizzle >> 6) & 0x3;

        swiz_str[i++] = '.';
        if (writemask0) swiz_str[i++] = swizzle_channels[swizzle_x];
        if (writemask1) swiz_str[i++] = swizzle_channels[swizzle_y];
        if (writemask2) swiz_str[i++] = swizzle_channels[swizzle_z];
        if (writemask3) swiz_str[i++] = swizzle_channels[swizzle_w];
    } // if
    assert(i < strsize);
    swiz_str[i] = '\0';
    return swiz_str;
} // make_GLSL_swizzle_string


static const char *make_GLSL_srcarg_string(Context *ctx, const size_t idx,
                                           const int writemask, char *buf,
                                           const size_t buflen)
{
    *buf = '\0';

    if (idx >= STATICARRAYLEN(ctx->source_args))
    {
        fail(ctx, "Too many source args");
        return buf;
    } // if

// !!! FIXME: not right.
    const SourceArgInfo *arg = &ctx->source_args[idx];

    const char *premod_str = "";
    const char *postmod_str = "";
    switch (arg->src_mod)
    {
        case SRCMOD_NEGATE:
            premod_str = "-";
            break;

        case SRCMOD_BIASNEGATE:
            premod_str = "-";
            // fall through.
        case SRCMOD_BIAS:
            fail(ctx, "SRCMOD_BIAS unsupported"); return buf; // !!! FIXME
            postmod_str = "_bias";
            break;

        case SRCMOD_SIGNNEGATE:
            premod_str = "-";
            // fall through.
        case SRCMOD_SIGN:
            fail(ctx, "SRCMOD_SIGN unsupported"); return buf; // !!! FIXME
            postmod_str = "_bx2";
            break;

        case SRCMOD_COMPLEMENT:
            fail(ctx, "SRCMOD_COMPLEMENT unsupported"); return buf; // !!! FIXME  (need to handle vecsize)
            premod_str = "(1.0 - (";
            postmod_str = "))";
            break;

        case SRCMOD_X2NEGATE:
            fail(ctx, "SRCMOD_X2NEGATE unsupported"); return buf; // !!! FIXME  (need to handle vecsize)
            premod_str = "-(";
            postmod_str = " * 2.0)";
            break;

        case SRCMOD_X2:
            fail(ctx, "SRCMOD_X2 unsupported"); return buf; // !!! FIXME  (need to handle vecsize)
            premod_str = "(";
            postmod_str = " * 2.0)";
            break;

        case SRCMOD_DZ:
            fail(ctx, "SRCMOD_DZ unsupported"); return buf; // !!! FIXME
            postmod_str = "_dz";
            break;

        case SRCMOD_DW:
            fail(ctx, "SRCMOD_DW unsupported"); return buf; // !!! FIXME
            postmod_str = "_dw";
            break;

        case SRCMOD_ABSNEGATE:
            premod_str = "-abs(";
            postmod_str = ")";
            break;

        case SRCMOD_ABS:
            premod_str = "abs(";
            postmod_str = ")";
            break;

        case SRCMOD_NOT:
            premod_str = "!";
            break;

        case SRCMOD_NONE:
        case SRCMOD_TOTAL:
             break;  // stop compiler whining.
    } // switch

    const char *regtype_str = NULL;

    if (!arg->relative)
    {
        regtype_str = get_GLSL_varname_in_buf(ctx, arg->regtype, arg->regnum,
                                              (char *) alloca(64), 64);
    } // if

    const char *rel_lbracket = "";
    char rel_offset[32] = { '\0' };
    const char *rel_rbracket = "";
    char rel_swizzle[4] = { '\0' };
    const char *rel_regtype_str = "";
    if (arg->relative)
    {
        if (arg->regtype == REG_TYPE_INPUT)
            regtype_str=get_GLSL_input_array_varname(ctx,(char*)alloca(64),64);
        else
        {
            assert(arg->regtype == REG_TYPE_CONST);
            const int arrayidx = arg->relative_array->index;
            const int offset = arg->regnum - arrayidx;
            assert(offset >= 0);
            if (arg->relative_array->constant)
            {
                const int arraysize = arg->relative_array->count;
                regtype_str = get_GLSL_const_array_varname_in_buf(ctx,
                                arrayidx, arraysize, (char *) alloca(64), 64);
                if (offset != 0)
                    snprintf(rel_offset, sizeof (rel_offset), "%d + ", offset);
            } // if
            else
            {
                regtype_str = get_GLSL_uniform_array_varname(ctx, arg->regtype,
                                                      (char *) alloca(64), 64);
                if (offset == 0)
                {
                    snprintf(rel_offset, sizeof (rel_offset),
                             "ARRAYBASE_%d + ", arrayidx);
                } // if
                else
                {
                    snprintf(rel_offset, sizeof (rel_offset),
                             "(ARRAYBASE_%d + %d) + ", arrayidx, offset);
                } // else
            } // else
        } // else

        rel_lbracket = "[";

        rel_regtype_str = get_GLSL_varname_in_buf(ctx, arg->relative_regtype,
                                                  arg->relative_regnum,
                                                  (char *) alloca(64), 64);
        rel_swizzle[0] = '.';
        rel_swizzle[1] = swizzle_channels[arg->relative_component];
        rel_swizzle[2] = '\0';
        rel_rbracket = "]";
    } // if

    char swiz_str[6] = { '\0' };
    if (!isscalar(ctx, ctx->shader_type, arg->regtype, arg->regnum))
    {
        make_GLSL_swizzle_string(swiz_str, sizeof (swiz_str),
                                 arg->swizzle, writemask);
    } // if

    if (regtype_str == NULL)
    {
        fail(ctx, "Unknown source register type.");
        return buf;
    } // if

    snprintf(buf, buflen, "%s%s%s%s%s%s%s%s%s",
             premod_str, regtype_str, rel_lbracket, rel_offset,
             rel_regtype_str, rel_swizzle, rel_rbracket, swiz_str,
             postmod_str);
    // !!! FIXME: make sure the scratch buffer was large enough.
    return buf;
} // make_GLSL_srcarg_string

// generate some convenience functions.
#define MAKE_GLSL_SRCARG_STRING_(mask, bitmask) \
    static inline const char *make_GLSL_srcarg_string_##mask(Context *ctx, \
                                                const size_t idx, char *buf, \
                                                const size_t buflen) { \
        return make_GLSL_srcarg_string(ctx, idx, bitmask, buf, buflen); \
    }
MAKE_GLSL_SRCARG_STRING_(x, (1 << 0))
MAKE_GLSL_SRCARG_STRING_(y, (1 << 1))
MAKE_GLSL_SRCARG_STRING_(z, (1 << 2))
MAKE_GLSL_SRCARG_STRING_(w, (1 << 3))
MAKE_GLSL_SRCARG_STRING_(scalar, (1 << 0))
MAKE_GLSL_SRCARG_STRING_(full, 0xF)
MAKE_GLSL_SRCARG_STRING_(masked, ctx->dest_arg.writemask)
MAKE_GLSL_SRCARG_STRING_(vec3, 0x7)
MAKE_GLSL_SRCARG_STRING_(vec2, 0x3)
#undef MAKE_GLSL_SRCARG_STRING_

// special cases for comparison opcodes...

static const char *get_GLSL_comparison_string_scalar(Context *ctx)
{
    static const char *comps[] = { "", ">", "==", ">=", "<", "!=", "<=" };
    if (ctx->instruction_controls >= STATICARRAYLEN(comps))
    {
        fail(ctx, "unknown comparison control");
        return "";
    } // if

    return comps[ctx->instruction_controls];
} // get_GLSL_comparison_string_scalar

static const char *get_GLSL_comparison_string_vector(Context *ctx)
{
    static const char *comps[] = {
        "", "greaterThan", "equal", "greaterThanEqual", "lessThan",
        "notEqual", "lessThanEqual"
    };

    if (ctx->instruction_controls >= STATICARRAYLEN(comps))
    {
        fail(ctx, "unknown comparison control");
        return "";
    } // if

    return comps[ctx->instruction_controls];
} // get_GLSL_comparison_string_vector


static void emit_GLSL_start(Context *ctx, const char *profilestr)
{
    if (!shader_is_vertex(ctx) && !shader_is_pixel(ctx))
    {
        failf(ctx, "Shader type %u unsupported in this profile.",
              (uint) ctx->shader_type);
        return;
    } // if

    else if (strcmp(profilestr, MOJOSHADER_PROFILE_GLSL) == 0)
    {
        // No gl_FragData[] before GLSL 1.10, so we have to force the version.
        push_output(ctx, &ctx->preflight);
        output_line(ctx, "#version 110");
        pop_output(ctx);
    } // else if

    #if SUPPORT_PROFILE_GLSL120
    else if (strcmp(profilestr, MOJOSHADER_PROFILE_GLSL120) == 0)
    {
        ctx->profile_supports_glsl120 = 1;
        push_output(ctx, &ctx->preflight);
        output_line(ctx, "#version 120");
        pop_output(ctx);
    } // else if
    #endif

    else
    {
        failf(ctx, "Profile '%s' unsupported or unknown.", profilestr);
        return;
    } // else

    push_output(ctx, &ctx->mainline_intro);
    output_line(ctx, "void main()");
    output_line(ctx, "{");
    pop_output(ctx);

    set_output(ctx, &ctx->mainline);
    ctx->indent++;
} // emit_GLSL_start

static void emit_GLSL_RET(Context *ctx);
static void emit_GLSL_end(Context *ctx)
{
    
    if (ctx->shader_type == MOJOSHADER_TYPE_VERTEX) {
        //fix the opengl/directx coord mapping
        //see wine glsl_shader.c for details
        //TODO: fix if we've already seen a ret
        //output_line(ctx, "gl_Position.y = gl_Position.y * posFixup.y;");
        //output_line(ctx, "gl_Position.xy += posFixup.zw * gl_Position.ww;");
        //output_line(ctx, "gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;");
    }
    
    // force a RET opcode if we're at the end of the stream without one.
    if (ctx->previous_opcode != OPCODE_RET)
        emit_GLSL_RET(ctx);
} // emit_GLSL_end

static void emit_GLSL_phase(Context *ctx)
{
    // no-op in GLSL.
} // emit_GLSL_phase

static void output_GLSL_uniform_array(Context *ctx, const RegisterType regtype,
                                      const int size)
{
    if (size > 0)
    {
        char buf[64];
        get_GLSL_uniform_array_varname(ctx, regtype, buf, sizeof (buf));
        output_line(ctx, "uniform vec4 %s[%d];", buf, size);
    } // if
} // output_GLSL_uniform_array

static void emit_GLSL_finalize(Context *ctx)
{
    
    // throw some blank lines around to make source more readable.
    push_output(ctx, &ctx->globals);
    output_blank_line(ctx);
    pop_output(ctx);

    // If we had a relative addressing of REG_TYPE_INPUT, we need to build
    //  an array for it at the start of main(). GLSL doesn't let you specify
    //  arrays of attributes.
    //vec4 blah_array[BIGGEST_ARRAY];
    if (ctx->have_relative_input_registers) // !!! FIXME
        fail(ctx, "Relative addressing of input registers not supported.");

    push_output(ctx, &ctx->preflight);
    output_GLSL_uniform_array(ctx, REG_TYPE_CONST, ctx->uniform_float4_count);
    output_GLSL_uniform_array(ctx, REG_TYPE_CONSTINT, ctx->uniform_int4_count);
    output_GLSL_uniform_array(ctx, REG_TYPE_CONSTBOOL, ctx->uniform_bool_count);
    
    if (ctx->shader_type == MOJOSHADER_TYPE_VERTEX) {
        //output_line(ctx, "uniform vec4 posFixup;");
    }
    
    pop_output(ctx);
} // emit_GLSL_finalize

static void emit_GLSL_global(Context *ctx, RegisterType regtype, int regnum)
{
    char varname[64];
    get_GLSL_varname_in_buf(ctx, regtype, regnum, varname, sizeof (varname));

    push_output(ctx, &ctx->globals);
    switch (regtype)
    {
        case REG_TYPE_ADDRESS:
            output_line(ctx, "ivec4 %s;", varname);
            break;
        case REG_TYPE_PREDICATE:
            output_line(ctx, "bvec4 %s;", varname);
            break;
        case REG_TYPE_TEMP:
            output_line(ctx, "vec4 %s;", varname);
            break;
        case REG_TYPE_LOOP:
            break; // no-op. We declare these in for loops at the moment.
        case REG_TYPE_LABEL:
            break; // no-op. If we see it here, it means we optimized it out.
        default:
            fail(ctx, "BUG: we used a register we don't know how to define.");
            break;
    } // switch
    pop_output(ctx);
} // emit_GLSL_global

static void emit_GLSL_array(Context *ctx, VariableList *var)
{
    // All uniforms (except constant arrays, which only get pushed once at
    //  compile time) are now packed into a single array, so we can batch
    //  the uniform transfers. So this is doesn't actually define an array
    //  here; the one, big array is emitted during finalization instead.
    // However, we need to #define the offset into the one, big array here,
    //  and let dereferences use that #define.
    const int base = var->index;
    const int glslbase = ctx->uniform_float4_count;
    push_output(ctx, &ctx->globals);
    output_line(ctx, "#define ARRAYBASE_%d %d", base, glslbase);
    pop_output(ctx);
    var->emit_position = glslbase;
} // emit_GLSL_array

static void emit_GLSL_const_array(Context *ctx, const ConstantsList *clist,
                                  int base, int size)
{
    char varname[64];
    get_GLSL_const_array_varname_in_buf(ctx,base,size,varname,sizeof(varname));

#if 0
    // !!! FIXME: fails on Nvidia's and Apple's GL, even with #version 120.
    // !!! FIXME:  (the 1.20 spec says it should work, though, I think...)
    if (support_glsl120(ctx))
    {
        // GLSL 1.20 can do constant arrays.
        const char *cstr = NULL;
        push_output(ctx, &ctx->globals);
        output_line(ctx, "const vec4 %s[%d] = vec4[%d](", varname, size, size);
        ctx->indent++;

        int i;
        for (i = 0; i < size; i++)
        {
            while (clist->constant.type != MOJOSHADER_UNIFORM_FLOAT)
                clist = clist->next;
            assert(clist->constant.index == (base + i));

            char val0[32];
            char val1[32];
            char val2[32];
            char val3[32];
            floatstr(ctx, val0, sizeof (val0), clist->constant.value.f[0], 1);
            floatstr(ctx, val1, sizeof (val1), clist->constant.value.f[1], 1);
            floatstr(ctx, val2, sizeof (val2), clist->constant.value.f[2], 1);
            floatstr(ctx, val3, sizeof (val3), clist->constant.value.f[3], 1);

            output_line(ctx, "vec4(%s, %s, %s, %s)%s", val0, val1, val2, val3,
                        (i < (size-1)) ? "," : "");

            clist = clist->next;
        } // for

        ctx->indent--;
        output_line(ctx, ");");
        pop_output(ctx);
    } // if

    else
#endif
    {
        // stock GLSL 1.0 can't do constant arrays, so make a uniform array
        //  and have the OpenGL glue assign it at link time. Lame!
        push_output(ctx, &ctx->globals);
        output_line(ctx, "uniform vec4 %s[%d];", varname, size);
        pop_output(ctx);
    } // else
} // emit_GLSL_const_array

static void emit_GLSL_uniform(Context *ctx, RegisterType regtype, int regnum,
                              const VariableList *var)
{
    // Now that we're pushing all the uniforms as one big array, pack these
    //  down, so if we only use register c439, it'll actually map to
    //  glsl_uniforms_vec4[0]. As we push one big array, this will prevent
    //  uploading unused data.

    char varname[64];
    char name[64];
    int index = 0;

    get_GLSL_varname_in_buf(ctx, regtype, regnum, varname, sizeof (varname));

    push_output(ctx, &ctx->globals);

    if (var == NULL)
    {
        get_GLSL_uniform_array_varname(ctx, regtype, name, sizeof (name));

        if (regtype == REG_TYPE_CONST)
            index = ctx->uniform_float4_count;
        else if (regtype == REG_TYPE_CONSTINT)
            index = ctx->uniform_int4_count;
        else if (regtype == REG_TYPE_CONSTBOOL)
            index = ctx->uniform_bool_count;
        else  // get_GLSL_uniform_array_varname() would have called fail().
            assert(isfail(ctx));

        output_line(ctx, "#define %s %s[%d]", varname, name, index);
    } // if

    else
    {
        const int arraybase = var->index;
        if (var->constant)
        {
            get_GLSL_const_array_varname_in_buf(ctx, arraybase, var->count,
                                                name, sizeof (name));
            index = (regnum - arraybase);
        } // if
        else
        {
            assert(var->emit_position != -1);
            get_GLSL_uniform_array_varname(ctx, regtype, name, sizeof (name));
            index = (regnum - arraybase) + var->emit_position;
        } // else

        output_line(ctx, "#define %s %s[%d]", varname, name, index);
    } // else

    pop_output(ctx);
} // emit_GLSL_uniform

static void emit_GLSL_sampler(Context *ctx, int stage, TextureType ttype)
{
    const char *type = "";
    switch (ttype)
    {
        case TEXTURE_TYPE_2D: type = "sampler2D"; break;
        case TEXTURE_TYPE_CUBE: type = "samplerCube"; break;
        case TEXTURE_TYPE_VOLUME: type = "sampler3D"; break;
        default: fail(ctx, "BUG: used a sampler we don't know how to define.");
    } // switch

    char var[64];
    get_GLSL_varname_in_buf(ctx, REG_TYPE_SAMPLER, stage, var, sizeof (var));

    push_output(ctx, &ctx->globals);
    output_line(ctx, "uniform %s %s;", type, var);
    pop_output(ctx);
} // emit_GLSL_sampler

static void emit_GLSL_attribute(Context *ctx, RegisterType regtype, int regnum,
                                MOJOSHADER_usage usage, int index, int wmask,
                                int flags)
{
    // !!! FIXME: this function doesn't deal with write masks at all yet!
    const char *usage_str = NULL;
    const char *arrayleft = "";
    const char *arrayright = "";
    char index_str[16] = { '\0' };
    char var[64];

    get_GLSL_varname_in_buf(ctx, regtype, regnum, var, sizeof (var));

    //assert((flags & MOD_PP) == 0);  // !!! FIXME: is PP allowed?

    if (index != 0)  // !!! FIXME: a lot of these MUST be zero.
        snprintf(index_str, sizeof (index_str), "%u", (uint) index);

    if (shader_is_vertex(ctx))
    {
        // pre-vs3 output registers.
        // these don't ever happen in DCL opcodes, I think. Map to vs_3_*
        //  output registers.
        if (!shader_version_atleast(ctx, 3, 0))
        {
            if (regtype == REG_TYPE_RASTOUT)
            {
                regtype = REG_TYPE_OUTPUT;
                index = regnum;
                switch ((const RastOutType) regnum)
                {
                    case RASTOUT_TYPE_POSITION:
                        usage = MOJOSHADER_USAGE_POSITION;
                        break;
                    case RASTOUT_TYPE_FOG:
                        usage = MOJOSHADER_USAGE_FOG;
                        break;
                    case RASTOUT_TYPE_POINT_SIZE:
                        usage = MOJOSHADER_USAGE_POINTSIZE;
                        break;
                } // switch
            } // if

            else if (regtype == REG_TYPE_ATTROUT)
            {
                regtype = REG_TYPE_OUTPUT;
                usage = MOJOSHADER_USAGE_COLOR;
                index = regnum;
            } // else if

            else if (regtype == REG_TYPE_TEXCRDOUT)
            {
                regtype = REG_TYPE_OUTPUT;
                usage = MOJOSHADER_USAGE_TEXCOORD;
                index = regnum;
            } // else if
        } // if

        // to avoid limitations of various GL entry points for input
        // attributes (glSecondaryColorPointer() can only take 3 component
        // items, glVertexPointer() can't do GL_UNSIGNED_BYTE, many other
        // issues), we set up all inputs as generic vertex attributes, so we
        // can pass data in just about any form, and ignore the built-in GLSL
        // attributes like gl_SecondaryColor. Output needs to use the the
        // built-ins, though, but we don't have to worry about the GL entry
        // point limitations there.

        if (regtype == REG_TYPE_INPUT)
        {
            push_output(ctx, &ctx->globals);
            output_line(ctx, "attribute vec4 %s;", var);
            pop_output(ctx);
        } // if

        else if (regtype == REG_TYPE_OUTPUT)
        {
            push_output(ctx, &ctx->globals);
            
            switch (usage)
            {
                case MOJOSHADER_USAGE_POSITION:
                    usage_str = "gl_Position";
                    break;
                case MOJOSHADER_USAGE_POINTSIZE:
                    usage_str = "gl_PointSize";
                    break;
                case MOJOSHADER_USAGE_COLOR:
                    index_str[0] = '\0';  // no explicit number.
                    
                    if (index == 0) {
                        output_line(ctx, "varying vec4 vFrontColor;");
                        //usage_str = "gl_FrontColor";
                        usage_str = "vFrontColor";
                    } else if (index == 1) {
                        output_line(ctx, "varying vec4 vFrontSecondaryColor;");
                        //usage_str = "gl_FrontSecondaryColor";
                        usage_str = "vFrontSecondaryColor";
                    }
                    
                    break;
                case MOJOSHADER_USAGE_FOG:
                    output_line(ctx, "varying float vFogFragCoord;");
                    //usage_str = "gl_FogFragCoord";
                    usage_str = "vFogFragCoord";
                    break;
                case MOJOSHADER_USAGE_TEXCOORD:
                    snprintf(index_str, sizeof (index_str), "%u", (uint) index);
                    output_line(ctx, "varying vec4 vTexCoord%s;", index_str);
                    usage_str = "vTexCoord";
                    //usage_str = "gl_TexCoord";
                    //arrayleft = "[";
                    //arrayright = "]";
                    break;
                default:
                    // !!! FIXME: we need to deal with some more built-in varyings here.
                    break;
            } // switch
            
            // !!! FIXME: the #define is a little hacky, but it means we don't
            // !!! FIXME:  have to track these separately if this works.
            // no mapping to built-in var? Just make it a regular global, pray.
            if (usage_str == NULL)
                output_line(ctx, "vec4 %s;", var);
            else
            {
                //output_line(ctx, "#define %s %s%s%s%s", var, usage_str,
                //            arrayleft, index_str, arrayright);
                output_line(ctx, "#define %s %s%s", var, usage_str, index_str);
            } // else
            pop_output(ctx);
        } // else if

        else
        {
            fail(ctx, "unknown vertex shader attribute register");
        } // else
    } // if

    else if (shader_is_pixel(ctx))
    {
        // samplers DCLs get handled in emit_GLSL_sampler().

        if (flags & MOD_CENTROID)  // !!! FIXME
        {
            failf(ctx, "centroid unsupported in %s profile", ctx->profile->name);
            return;
        } // if

        if (regtype == REG_TYPE_COLOROUT)
        {
            if (!ctx->have_multi_color_outputs)
                usage_str = "gl_FragColor";  // maybe faster?
            else
            {
                snprintf(index_str, sizeof (index_str), "%u", (uint) regnum);
                usage_str = "gl_FragData";
                arrayleft = "[";
                arrayright = "]";
            } // else
        } // if

        else if (regtype == REG_TYPE_DEPTHOUT)
            usage_str = "gl_FragDepth";

        // !!! FIXME: can you actualy have a texture register with COLOR usage?
        else if ((regtype == REG_TYPE_TEXTURE) || (regtype == REG_TYPE_INPUT))
        {
            push_output(ctx, &ctx->globals);
            if (usage == MOJOSHADER_USAGE_TEXCOORD)
            {
                snprintf(index_str, sizeof (index_str), "%u", (uint) index);
                output_line(ctx, "varying vec4 vTexCoord%s;", index_str);
                output_line(ctx, "#define %s vTexCoord%s", var, index_str);
                //usage_str = "gl_TexCoord";
                //arrayleft = "[";
                //arrayright = "]";
            } // if

            else if (usage == MOJOSHADER_USAGE_COLOR)
            {
                index_str[0] = '\0';  // no explicit number.
                if (index == 0) {
                    output_line(ctx, "varying vec4 vFrontColor;");
                    usage_str = "vFrontColor";
                    //usage_str = "gl_Color";
                } else if (index == 1) {
                    output_line(ctx, "varying vec4 vFrontSecondaryColor;");
                    usage_str = "vFrontSecondaryColor";
                    //usage_str = "gl_SecondaryColor";
                } else {
                    fail(ctx, "unsupported color index");
                }
            } // else if
            pop_output(ctx);
        } // else if

        else if (regtype == REG_TYPE_MISCTYPE)
        {
            const MiscTypeType mt = (MiscTypeType) regnum;
            if (mt == MISCTYPE_TYPE_FACE)
            {
                push_output(ctx, &ctx->globals);
                output_line(ctx, "float %s = gl_FrontFacing ? 1.0 : -1.0;", var);
                pop_output(ctx);
            } // if
            else if (mt == MISCTYPE_TYPE_POSITION)
            {
                index_str[0] = '\0';  // no explicit number.
                usage_str = "gl_FragCoord";  // !!! FIXME: is this the same coord space as D3D?
            } // else if
            else
            {
                fail(ctx, "BUG: unhandled misc register");
            } // else
        } // else if

        else
        {
            fail(ctx, "unknown pixel shader attribute register");
        } // else

        if (usage_str != NULL)
        {
            push_output(ctx, &ctx->globals);
            output_line(ctx, "#define %s %s%s%s%s", var, usage_str,
                        arrayleft, index_str, arrayright);
            pop_output(ctx);
        } // if
    } // else if

    else
    {
        fail(ctx, "Unknown shader type");  // state machine should catch this.
    } // else
} // emit_GLSL_attribute

static void emit_GLSL_NOP(Context *ctx)
{
    // no-op is a no-op.  :)
} // emit_GLSL_NOP

static void emit_GLSL_MOV(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "%s", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_MOV

static void emit_GLSL_ADD(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "%s + %s", src0, src1);
    output_line(ctx, "%s", code);
} // emit_GLSL_ADD

static void emit_GLSL_SUB(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "%s - %s", src0, src1);
    output_line(ctx, "%s", code);
} // emit_GLSL_SUB

static void emit_GLSL_MAD(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char src2[64]; make_GLSL_srcarg_string_masked(ctx, 2, src2, sizeof (src2));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "(%s * %s) + %s", src0, src1, src2);
    output_line(ctx, "%s", code);
} // emit_GLSL_MAD

static void emit_GLSL_MUL(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "%s * %s", src0, src1);
    output_line(ctx, "%s", code);
} // emit_GLSL_MUL

static void emit_GLSL_RCP(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "1.0 / %s", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_RCP

static void emit_GLSL_RSQ(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "inversesqrt(%s)", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_RSQ

static void emit_GLSL_dotprod(Context *ctx, const char *src0, const char *src1,
                              const char *extra)
{
    const int vecsize = vecsize_from_writemask(ctx->dest_arg.writemask);
    char castleft[16] = { '\0' };
    const char *castright = "";
    if (vecsize != 1)
    {
        snprintf(castleft, sizeof (castleft), "vec%d(", vecsize);
        castright = ")";
    } // if

    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "%sdot(%s, %s)%s%s",
                             castleft, src0, src1, extra, castright);
    output_line(ctx, "%s", code);
} // emit_GLSL_dotprod

static void emit_GLSL_DP3(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_vec3(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_vec3(ctx, 1, src1, sizeof (src1));
    emit_GLSL_dotprod(ctx, src0, src1, "");
} // emit_GLSL_DP3

static void emit_GLSL_DP4(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_full(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_full(ctx, 1, src1, sizeof (src1));
    emit_GLSL_dotprod(ctx, src0, src1, "");
} // emit_GLSL_DP4

static void emit_GLSL_MIN(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "min(%s, %s)", src0, src1);
    output_line(ctx, "%s", code);
} // emit_GLSL_MIN

static void emit_GLSL_MAX(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "max(%s, %s)", src0, src1);
    output_line(ctx, "%s", code);
} // emit_GLSL_MAX

static void emit_GLSL_SLT(Context *ctx)
{
    const int vecsize = vecsize_from_writemask(ctx->dest_arg.writemask);
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char code[128];

    // float(bool) or vec(bvec) results in 0.0 or 1.0, like SLT wants.
    if (vecsize == 1)
        make_GLSL_destarg_assign(ctx, code, sizeof (code), "float(%s < %s)", src0, src1);
    else
    {
        make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                 "vec%d(lessThan(%s, %s))",
                                 vecsize, src0, src1);
    } // else
    output_line(ctx, "%s", code);
} // emit_GLSL_SLT

static void emit_GLSL_SGE(Context *ctx)
{
    const int vecsize = vecsize_from_writemask(ctx->dest_arg.writemask);
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char code[128];

    // float(bool) or vec(bvec) results in 0.0 or 1.0, like SGE wants.
    if (vecsize == 1)
    {
        make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                 "float(%s >= %s)", src0, src1);
    } // if
    else
    {
        make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                 "vec%d(greaterThanEqual(%s, %s))",
                                 vecsize, src0, src1);
    } // else
    output_line(ctx, "%s", code);
} // emit_GLSL_SGE

static void emit_GLSL_EXP(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "exp2(%s)", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_EXP

static void emit_GLSL_LOG(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "log2(%s)", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_LOG

static void emit_GLSL_LIT_helper(Context *ctx)
{
    const char *maxp = "127.9961f"; // value from the dx9 reference.

    if (ctx->glsl_generated_lit_opcode)
        return;

    ctx->glsl_generated_lit_opcode = 1;

    push_output(ctx, &ctx->helpers);
    output_line(ctx, "const vec4 LIT(const vec4 src)");
    output_line(ctx, "{"); ctx->indent++;
    output_line(ctx,   "const float power = clamp(src.w, -%s, %s);",maxp,maxp);
    output_line(ctx,   "vec4 retval(1.0, 0.0, 0.0, 1.0)");
    output_line(ctx,   "if (src.x > 0.0) {"); ctx->indent++;
    output_line(ctx,     "retval.y = src.x;");
    output_line(ctx,     "if (src.y > 0.0) {"); ctx->indent++;
    output_line(ctx,       "retval.z = pow(src.y, power);"); ctx->indent--;
    output_line(ctx,     "}"); ctx->indent--;
    output_line(ctx,   "}");
    output_line(ctx,   "return retval;"); ctx->indent--;
    output_line(ctx, "}");
    output_blank_line(ctx);
    pop_output(ctx);
} // emit_GLSL_LIT_helper

static void emit_GLSL_LIT(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_full(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "LIT(%s)", src0);
    output_line(ctx, "%s", code);
    emit_GLSL_LIT_helper(ctx);
} // emit_GLSL_LIT

static void emit_GLSL_DST(Context *ctx)
{
    // !!! FIXME: needs to take ctx->dst_arg.writemask into account.
    char src0_y[64]; make_GLSL_srcarg_string_y(ctx, 0, src0_y, sizeof (src0_y));
    char src1_y[64]; make_GLSL_srcarg_string_y(ctx, 1, src1_y, sizeof (src1_y));
    char src0_z[64]; make_GLSL_srcarg_string_z(ctx, 0, src0_z, sizeof (src0_z));
    char src1_w[64]; make_GLSL_srcarg_string_w(ctx, 1, src1_w, sizeof (src1_w));

    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code),
                             "vec4(1.0, %s * %s, %s, %s)",
                             src0_y, src1_y, src0_z, src1_w);
    output_line(ctx, "%s", code);
} // emit_GLSL_DST

static void emit_GLSL_LRP(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char src2[64]; make_GLSL_srcarg_string_masked(ctx, 2, src2, sizeof (src2));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "mix(%s, %s, %s)",
                             src2, src1, src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_LRP

static void emit_GLSL_FRC(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "fract(%s)", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_FRC

static void emit_GLSL_M4X4(Context *ctx)
{
    // !!! FIXME: d3d is row-major, glsl is column-major, I think.
    char src0[64]; make_GLSL_srcarg_string_full(ctx, 0, src0, sizeof (src0));
    char row0[64]; make_GLSL_srcarg_string_full(ctx, 1, row0, sizeof (row0));
    char row1[64]; make_GLSL_srcarg_string_full(ctx, 2, row1, sizeof (row1));
    char row2[64]; make_GLSL_srcarg_string_full(ctx, 3, row2, sizeof (row2));
    char row3[64]; make_GLSL_srcarg_string_full(ctx, 4, row3, sizeof (row3));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code),
                    "vec4(dot(%s, %s), dot(%s, %s), dot(%s, %s), dot(%s, %s))",
                    src0, row0, src0, row1, src0, row2, src0, row3);
    output_line(ctx, "%s", code);
} // emit_GLSL_M4X4

static void emit_GLSL_M4X3(Context *ctx)
{
    // !!! FIXME: d3d is row-major, glsl is column-major, I think.
    char src0[64]; make_GLSL_srcarg_string_full(ctx, 0, src0, sizeof (src0));
    char row0[64]; make_GLSL_srcarg_string_full(ctx, 1, row0, sizeof (row0));
    char row1[64]; make_GLSL_srcarg_string_full(ctx, 2, row1, sizeof (row1));
    char row2[64]; make_GLSL_srcarg_string_full(ctx, 3, row2, sizeof (row2));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                "vec3(dot(%s, %s), dot(%s, %s), dot(%s, %s))",
                                src0, row0, src0, row1, src0, row2);
    output_line(ctx, "%s", code);
} // emit_GLSL_M4X3

static void emit_GLSL_M3X4(Context *ctx)
{
    // !!! FIXME: d3d is row-major, glsl is column-major, I think.
    char src0[64]; make_GLSL_srcarg_string_vec3(ctx, 0, src0, sizeof (src0));
    char row0[64]; make_GLSL_srcarg_string_vec3(ctx, 1, row0, sizeof (row0));
    char row1[64]; make_GLSL_srcarg_string_vec3(ctx, 2, row1, sizeof (row1));
    char row2[64]; make_GLSL_srcarg_string_vec3(ctx, 3, row2, sizeof (row2));
    char row3[64]; make_GLSL_srcarg_string_vec3(ctx, 4, row3, sizeof (row3));

    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                "vec4(dot(%s, %s), dot(%s, %s), "
                                     "dot(%s, %s), dot(%s, %s))",
                                src0, row0, src0, row1,
                                src0, row2, src0, row3);
    output_line(ctx, "%s", code);
} // emit_GLSL_M3X4

static void emit_GLSL_M3X3(Context *ctx)
{
    // !!! FIXME: d3d is row-major, glsl is column-major, I think.
    char src0[64]; make_GLSL_srcarg_string_vec3(ctx, 0, src0, sizeof (src0));
    char row0[64]; make_GLSL_srcarg_string_vec3(ctx, 1, row0, sizeof (row0));
    char row1[64]; make_GLSL_srcarg_string_vec3(ctx, 2, row1, sizeof (row1));
    char row2[64]; make_GLSL_srcarg_string_vec3(ctx, 3, row2, sizeof (row2));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                "vec3(dot(%s, %s), dot(%s, %s), dot(%s, %s))",
                                src0, row0, src0, row1, src0, row2);
    output_line(ctx, "%s", code);
} // emit_GLSL_M3X3

static void emit_GLSL_M3X2(Context *ctx)
{
    // !!! FIXME: d3d is row-major, glsl is column-major, I think.
    char src0[64]; make_GLSL_srcarg_string_vec3(ctx, 0, src0, sizeof (src0));
    char row0[64]; make_GLSL_srcarg_string_vec3(ctx, 1, row0, sizeof (row0));
    char row1[64]; make_GLSL_srcarg_string_vec3(ctx, 2, row1, sizeof (row1));

    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                "vec2(dot(%s, %s), dot(%s, %s))",
                                src0, row0, src0, row1);
    output_line(ctx, "%s", code);
} // emit_GLSL_M3X2

static void emit_GLSL_CALL(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    if (ctx->loops > 0)
        output_line(ctx, "%s(aL);", src0);
    else
        output_line(ctx, "%s();", src0);
} // emit_GLSL_CALL

static void emit_GLSL_CALLNZ(Context *ctx)
{
    // !!! FIXME: if src1 is a constbool that's true, we can remove the
    // !!! FIXME:  if. If it's false, we can make this a no-op.
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));

    if (ctx->loops > 0)
        output_line(ctx, "if (%s) { %s(aL); }", src1, src0);
    else
        output_line(ctx, "if (%s) { %s(); }", src1, src0);
} // emit_GLSL_CALLNZ

static void emit_GLSL_LOOP(Context *ctx)
{
    // !!! FIXME: swizzle?
    char var[64]; get_GLSL_srcarg_varname(ctx, 1, var, sizeof (var));
    assert(ctx->source_args[0].regnum == 0);  // in case they add aL1 someday.
    output_line(ctx, "{");
    ctx->indent++;
    output_line(ctx, "const int aLend = %s.x + %s.y;", var, var);
    output_line(ctx, "for (int aL = %s.y; aL < aLend; aL += %s.z) {", var, var);
    ctx->indent++;
} // emit_GLSL_LOOP

static void emit_GLSL_RET(Context *ctx)
{
    // thankfully, the MSDN specs say a RET _has_ to end a function...no
    //  early returns. So if you hit one, you know you can safely close
    //  a high-level function.
    ctx->indent--;
    output_line(ctx, "}");
    output_blank_line(ctx);
    set_output(ctx, &ctx->subroutines);
} // emit_GLSL_RET

static void emit_GLSL_ENDLOOP(Context *ctx)
{
    ctx->indent--;
    output_line(ctx, "}");
    ctx->indent--;
    output_line(ctx, "}");
} // emit_GLSL_ENDLOOP

static void emit_GLSL_LABEL(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    const int label = ctx->source_args[0].regnum;
    RegisterList *reg = reglist_find(&ctx->used_registers, REG_TYPE_LABEL, label);
    assert(ctx->output == ctx->subroutines);  // not mainline, etc.
    assert(ctx->indent == 0);  // we shouldn't be in the middle of a function.

    // MSDN specs say CALL* has to come before the LABEL, so we know if we
    //  can ditch the entire function here as unused.
    if (reg == NULL)
        set_output(ctx, &ctx->ignore);  // Func not used. Parse, but don't output.

    // !!! FIXME: it would be nice if we could determine if a function is
    // !!! FIXME:  only called once and, if so, forcibly inline it.

    const char *uses_loopreg = ((reg) && (reg->misc == 1)) ? "int aL" : "";
    output_line(ctx, "void %s(%s)", src0, uses_loopreg);
    output_line(ctx, "{");
    ctx->indent++;
} // emit_GLSL_LABEL

static void emit_GLSL_DCL(Context *ctx)
{
    // no-op. We do this in our emit_attribute() and emit_uniform().
} // emit_GLSL_DCL

static void emit_GLSL_POW(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code),
                             "pow(abs(%s), %s)", src0, src1);
    output_line(ctx, "%s", code);
} // emit_GLSL_POW

static void emit_GLSL_CRS(Context *ctx)
{
    // !!! FIXME: needs to take ctx->dst_arg.writemask into account.
    char src0[64]; make_GLSL_srcarg_string_vec3(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_vec3(ctx, 1, src1, sizeof (src1));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code),
                             "cross(%s, %s)", src0, src1);
    output_line(ctx, "%s", code);
} // emit_GLSL_CRS

static void emit_GLSL_SGN(Context *ctx)
{
    // (we don't need the temporary registers specified for the D3D opcode.)
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "sign(%s)", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_SGN

static void emit_GLSL_ABS(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "abs(%s)", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_ABS

static void emit_GLSL_NRM(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "normalize(%s)", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_NRM

static void emit_GLSL_SINCOS(Context *ctx)
{
    // we don't care about the temp registers that <= sm2 demands; ignore them.
    //  sm2 also talks about what components are left untouched vs. undefined,
    //  but we just leave those all untouched with GLSL write masks (which
    //  would fulfill the "undefined" requirement, too).
    const int mask = ctx->dest_arg.writemask;
    char src0[64]; make_GLSL_srcarg_string_scalar(ctx, 0, src0, sizeof (src0));
    char code[128] = { '\0' };

    if (writemask_x(mask))
        make_GLSL_destarg_assign(ctx, code, sizeof (code), "cos(%s)", src0);
    else if (writemask_y(mask))
        make_GLSL_destarg_assign(ctx, code, sizeof (code), "sin(%s)", src0);
    else if (writemask_xy(mask))
    {
        make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                 "vec2(cos(%s), sin(%s))", src0, src0);
    } // else if

    output_line(ctx, "%s", code);
} // emit_GLSL_SINCOS

static void emit_GLSL_REP(Context *ctx)
{
    // !!! FIXME:
    // msdn docs say legal loop values are 0 to 255. We can check DEFI values
    //  at parse time, but if they are pulling a value from a uniform, do
    //  we clamp here?
    // !!! FIXME: swizzle is legal here, right?
    char src0[64]; make_GLSL_srcarg_string_x(ctx, 0, src0, sizeof (src0));
    const uint rep = (uint) ctx->reps;
    output_line(ctx, "for (int rep%u = 0; rep%u < %s; rep%u++) {",
                rep, rep, src0, rep);
    ctx->indent++;
} // emit_GLSL_REP

static void emit_GLSL_ENDREP(Context *ctx)
{
    ctx->indent--;
    output_line(ctx, "}");
} // emit_GLSL_ENDREP

static void emit_GLSL_IF(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_scalar(ctx, 0, src0, sizeof (src0));
    output_line(ctx, "if (%s) {", src0);
    ctx->indent++;
} // emit_GLSL_IF

static void emit_GLSL_IFC(Context *ctx)
{
    const char *comp = get_GLSL_comparison_string_scalar(ctx);
    char src0[64]; make_GLSL_srcarg_string_scalar(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_scalar(ctx, 1, src1, sizeof (src1));
    output_line(ctx, "if (%s %s %s) {", src0, comp, src1);
    ctx->indent++;
} // emit_GLSL_IFC

static void emit_GLSL_ELSE(Context *ctx)
{
    ctx->indent--;
    output_line(ctx, "} else {");
    ctx->indent++;
} // emit_GLSL_ELSE

static void emit_GLSL_ENDIF(Context *ctx)
{
    ctx->indent--;
    output_line(ctx, "}");
} // emit_GLSL_ENDIF

static void emit_GLSL_BREAK(Context *ctx)
{
    output_line(ctx, "break;");
} // emit_GLSL_BREAK

static void emit_GLSL_BREAKC(Context *ctx)
{
    const char *comp = get_GLSL_comparison_string_scalar(ctx);
    char src0[64]; make_GLSL_srcarg_string_scalar(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_scalar(ctx, 1, src1, sizeof (src1));
    output_line(ctx, "if (%s %s %s) { break; }", src0, comp, src1);
} // emit_GLSL_BREAKC

static void emit_GLSL_MOVA(Context *ctx)
{
    const int vecsize = vecsize_from_writemask(ctx->dest_arg.writemask);
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];

    if (vecsize == 1)
    {
        make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                 "int(floor(abs(%s) + 0.5) * sign(%s))",
                                 src0, src0);
    } // if

    else
    {
        make_GLSL_destarg_assign(ctx, code, sizeof (code),
                            "ivec%d(floor(abs(%s) + vec%d(0.5)) * sign(%s))",
                            vecsize, src0, vecsize, src0);
    } // else

    output_line(ctx, "%s", code);
} // emit_GLSL_MOVA

static void emit_GLSL_DEFB(Context *ctx)
{
    char varname[64]; get_GLSL_destarg_varname(ctx, varname, sizeof (varname));
    push_output(ctx, &ctx->globals);
    output_line(ctx, "const bool %s = %s;",
                varname, ctx->dwords[0] ? "true" : "false");
    pop_output(ctx);
} // emit_GLSL_DEFB

static void emit_GLSL_DEFI(Context *ctx)
{
    char varname[64]; get_GLSL_destarg_varname(ctx, varname, sizeof (varname));
    const int32 *x = (const int32 *) ctx->dwords;
    push_output(ctx, &ctx->globals);
    output_line(ctx, "const ivec4 %s = ivec4(%d, %d, %d, %d);",
                varname, (int) x[0], (int) x[1], (int) x[2], (int) x[3]);
    pop_output(ctx);
} // emit_GLSL_DEFI

EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXCRD)

static void emit_GLSL_TEXKILL(Context *ctx)
{
    char dst[64]; get_GLSL_destarg_varname(ctx, dst, sizeof (dst));
    output_line(ctx, "if (any(lessThan(%s.xyz, vec3(0.0)))) discard;", dst);
} // emit_GLSL_TEXKILL

static void glsl_texld(Context *ctx, const int texldd)
{
    // !!! FIXME: do non-RGBA textures map to same default values as D3D?

    if (!shader_version_atleast(ctx, 2, 0))
    {
        // ps_1_0 and ps_1_4 are both different, too!
        fail(ctx, "TEXLD <= Shader Model 2.0 unimplemented.");  // !!! FIXME
        return;
    } // if
    else
    {
        const SourceArgInfo *samp_arg = &ctx->source_args[1];
        RegisterList *sreg = reglist_find(&ctx->samplers, REG_TYPE_SAMPLER,
                                          samp_arg->regnum);
        const char *funcname = NULL;
        char src0[64] = { '\0' };
        char src1[64]; get_GLSL_srcarg_varname(ctx, 1, src1, sizeof (src1)); // !!! FIXME: SRC_MOD?
        char src2[64] = { '\0' };
        char src3[64] = { '\0' };

        if (sreg == NULL)
        {
            fail(ctx, "TEXLD using undeclared sampler");
            return;
        } // if

        if (texldd)
        {
            make_GLSL_srcarg_string_vec2(ctx, 2, src2, sizeof (src2));
            make_GLSL_srcarg_string_vec2(ctx, 3, src3, sizeof (src3));
        } // if

        // !!! FIXME: can TEXLDD set instruction_controls?
        // !!! FIXME: does the d3d bias value map directly to GLSL?
        const char *biassep = "";
        char bias[64] = { '\0' };
        if (ctx->instruction_controls == CONTROL_TEXLDB)
        {
            biassep = ", ";
            make_GLSL_srcarg_string_w(ctx, 0, bias, sizeof (bias));
        } // if

        switch ((const TextureType) sreg->index)
        {
            case TEXTURE_TYPE_2D:
                if (ctx->instruction_controls == CONTROL_TEXLDP)
                {
                    funcname = "texture2DProj";
                    make_GLSL_srcarg_string_full(ctx, 0, src0, sizeof (src0));
                } // if
                else  // texld/texldb
                {
                    funcname = "texture2D";
                    make_GLSL_srcarg_string_vec2(ctx, 0, src0, sizeof (src0));
                } // else
                break;
            case TEXTURE_TYPE_CUBE:
                if (ctx->instruction_controls == CONTROL_TEXLDP)
                    fail(ctx, "TEXLDP on a cubemap");  // !!! FIXME: is this legal?
                funcname = "textureCube";
                make_GLSL_srcarg_string_vec3(ctx, 0, src0, sizeof (src0));
                break;
            case TEXTURE_TYPE_VOLUME:
                if (ctx->instruction_controls == CONTROL_TEXLDP)
                {
                    funcname = "texture3DProj";
                    make_GLSL_srcarg_string_full(ctx, 0, src0, sizeof (src0));
                } // if
                else  // texld/texldb
                {
                    funcname = "texture3D";
                    make_GLSL_srcarg_string_vec3(ctx, 0, src0, sizeof (src0));
                } // else
                break;
            default:
                fail(ctx, "unknown texture type");
                return;
        } // switch

        assert(!isscalar(ctx, ctx->shader_type, samp_arg->regtype, samp_arg->regnum));
        char swiz_str[6] = { '\0' };
        make_GLSL_swizzle_string(swiz_str, sizeof (swiz_str),
                                 samp_arg->swizzle, ctx->dest_arg.writemask);

        char code[128];
        if (texldd)
        {
            make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                     "%sGrad(%s, %s, %s, %s)%s", funcname,
                                     src1, src0, src2, src3, swiz_str);
        } // if
        else
        {
            make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                     "%s(%s, %s%s%s)%s", funcname,
                                     src1, src0, biassep, bias, swiz_str);
        } // else

        output_line(ctx, "%s", code);
    } // else
} // glsl_texld

static void emit_GLSL_TEXLD(Context *ctx)
{
     glsl_texld(ctx, 0);
} // emit_GLSL_TEXLD
    

EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXBEM)  // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXBEML) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXREG2AR) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXREG2GB) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X2PAD) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X2TEX) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X3PAD) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X3TEX) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X3SPEC) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X3VSPEC) // !!! FIXME

static void emit_GLSL_EXPP(Context *ctx)
{
    // !!! FIXME: msdn's asm docs don't list this opcode, I'll have to check the driver documentation.
    emit_GLSL_EXP(ctx);  // I guess this is just partial precision EXP?
} // emit_GLSL_EXPP

static void emit_GLSL_LOGP(Context *ctx)
{
    // LOGP is just low-precision LOG, but we'll take the higher precision.
    emit_GLSL_LOG(ctx);
} // emit_GLSL_LOGP

// common code between CMP and CND.
static void emit_GLSL_comparison_operations(Context *ctx, const char *cmp)
{
    int i, j;
    DestArgInfo *dst = &ctx->dest_arg;
    const SourceArgInfo *srcarg0 = &ctx->source_args[0];
    const int origmask = dst->writemask;
    int used_swiz[4] = { 0, 0, 0, 0 };
    const int writemask[4] = { dst->writemask0, dst->writemask1,
                               dst->writemask2, dst->writemask3 };
    const int src0swiz[4] = { srcarg0->swizzle_x, srcarg0->swizzle_y,
                              srcarg0->swizzle_z, srcarg0->swizzle_w };

    for (i = 0; i < 4; i++)
    {
        int mask = (1 << i);

        if (!writemask[i]) continue;
        if (used_swiz[i]) continue;

        // This is a swizzle we haven't checked yet.
        used_swiz[i] = 1;

        // see if there are any other elements swizzled to match (.yyyy)
        for (j = i + 1; j < 4; j++)
        {
            if (!writemask[j]) continue;
            if (src0swiz[i] != src0swiz[j]) continue;
            mask |= (1 << j);
            used_swiz[j] = 1;
        } // for

        // okay, (mask) should be the writemask of swizzles we like.

        //return make_GLSL_srcarg_string(ctx, idx, (1 << 0));

        char src0[64];
        char src1[64];
        char src2[64];
        make_GLSL_srcarg_string(ctx, 0, (1 << i), src0, sizeof (src0));
        make_GLSL_srcarg_string(ctx, 1, mask, src1, sizeof (src1));
        make_GLSL_srcarg_string(ctx, 2, mask, src2, sizeof (src2));

        dst->writemask = mask;
        dst->writemask0 = ((mask >> 0) & 1);
        dst->writemask1 = ((mask >> 1) & 1);
        dst->writemask2 = ((mask >> 2) & 1);
        dst->writemask3 = ((mask >> 3) & 1);

        char code[128];
        make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                 "((%s %s) ? %s : %s)",
                                 src0, cmp, src1, src2);
        dst->writemask = origmask;
        dst->writemask0 = ((origmask >> 0) & 1);
        dst->writemask1 = ((origmask >> 1) & 1);
        dst->writemask2 = ((origmask >> 2) & 1);
        dst->writemask3 = ((origmask >> 3) & 1);
        output_line(ctx, "%s", code);
    } // for
} // emit_GLSL_comparison_operations

static void emit_GLSL_CND(Context *ctx)
{
    emit_GLSL_comparison_operations(ctx, "> 0.5");
} // emit_GLSL_CND

static void emit_GLSL_DEF(Context *ctx)
{
    const float *val = (const float *) ctx->dwords; // !!! FIXME: could be int?
    char varname[64]; get_GLSL_destarg_varname(ctx, varname, sizeof (varname));
    char val0[32]; floatstr(ctx, val0, sizeof (val0), val[0], 1);
    char val1[32]; floatstr(ctx, val1, sizeof (val1), val[1], 1);
    char val2[32]; floatstr(ctx, val2, sizeof (val2), val[2], 1);
    char val3[32]; floatstr(ctx, val3, sizeof (val3), val[3], 1);

    push_output(ctx, &ctx->globals);
    output_line(ctx, "const vec4 %s = vec4(%s, %s, %s, %s);",
                varname, val0, val1, val2, val3);
    pop_output(ctx);
} // emit_GLSL_DEF

EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXREG2RGB) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXDP3TEX) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X2DEPTH) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXDP3) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X3) // !!! FIXME
EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(TEXDEPTH) // !!! FIXME

static void emit_GLSL_CMP(Context *ctx)
{
    emit_GLSL_comparison_operations(ctx, ">= 0.0");
} // emit_GLSL_CMP

EMIT_GLSL_OPCODE_UNIMPLEMENTED_FUNC(BEM) // !!! FIXME

static void emit_GLSL_DP2ADD(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_vec2(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_vec2(ctx, 1, src1, sizeof (src1));
    char src2[64]; make_GLSL_srcarg_string_scalar(ctx, 2, src2, sizeof (src2));
    char extra[64]; snprintf(extra, sizeof (extra), " + %s", src2);
    emit_GLSL_dotprod(ctx, src0, src1, extra);
} // emit_GLSL_DP2ADD

static void emit_GLSL_DSX(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "dFdx(%s)", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_DSX

static void emit_GLSL_DSY(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char code[128];
    make_GLSL_destarg_assign(ctx, code, sizeof (code), "dFdy(%s)", src0);
    output_line(ctx, "%s", code);
} // emit_GLSL_DSY

static void emit_GLSL_TEXLDD(Context *ctx)
{
    // !!! FIXME:
    // GLSL 1.30 introduced textureGrad() for this, but it looks like the
    //  functions are overloaded instead of texture2DGrad() (etc).

    // GL_shader_texture_lod and GL_EXT_gpu_shader4 added texture2DGrad*(),
    //  so we'll use them if available. Failing that, we'll just fallback
    //  to a regular texture2D call and hope the mipmap it chooses is close
    //  enough.
    if (!ctx->glsl_generated_texldd_setup)
    {
        ctx->glsl_generated_texldd_setup = 1;
        push_output(ctx, &ctx->preflight);
        output_line(ctx, "#if GL_ARB_shader_texture_lod");
        output_line(ctx, "#extension GL_ARB_shader_texture_lod : enable");
        output_line(ctx, "#define texture2DGrad texture2DGradARB");
        output_line(ctx, "#define texture2DProjGrad texture2DProjARB");
        output_line(ctx, "#elif GL_EXT_gpu_shader4");
        output_line(ctx, "#extension GL_EXT_gpu_shader4 : enable");
        output_line(ctx, "#else");
        output_line(ctx, "#define texture2DGrad(a,b,c,d) texture2D(a,b)");
        output_line(ctx, "#define texture2DProjGrad(a,b,c,d) texture2DProj(a,b)");
        output_line(ctx, "#endif");
        output_blank_line(ctx);
        pop_output(ctx);
    } // if

    glsl_texld(ctx, 1);
} // emit_GLSL_TEXLDD

static void emit_GLSL_SETP(Context *ctx)
{
    const int vecsize = vecsize_from_writemask(ctx->dest_arg.writemask);
    char src0[64]; make_GLSL_srcarg_string_masked(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_GLSL_srcarg_string_masked(ctx, 1, src1, sizeof (src1));
    char code[128];

    // destination is always predicate register (which is type bvec4).
    if (vecsize == 1)
    {
        const char *comp = get_GLSL_comparison_string_scalar(ctx);
        make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                 "(%s %s %s)", src0, comp, src1);
    } // if
    else
    {
        const char *comp = get_GLSL_comparison_string_vector(ctx);
        make_GLSL_destarg_assign(ctx, code, sizeof (code),
                                 "%s(%s, %s)", comp, src0, src1);
    } // else

    output_line(ctx, "%s", code);
} // emit_GLSL_SETP

static void emit_GLSL_TEXLDL(Context *ctx)
{
    // !!! FIXME: The spec says we can't use GLSL's texture*Lod() built-ins
    // !!! FIXME:  from fragment shaders for some inexplicable reason.
    // !!! FIXME:  For now, you'll just have to suffer with the potentially
    // !!! FIXME:  wrong mipmap until I can figure something out.
    emit_GLSL_TEXLD(ctx);
} // emit_GLSL_TEXLDL

static void emit_GLSL_BREAKP(Context *ctx)
{
    char src0[64]; make_GLSL_srcarg_string_scalar(ctx, 0, src0, sizeof (src0));
    output_line(ctx, "if (%s) { break; }", src0);
} // emit_GLSL_BREAKP

static void emit_GLSL_RESERVED(Context *ctx)
{
    // do nothing; fails in the state machine.
} // emit_GLSL_RESERVED

#endif  // SUPPORT_PROFILE_GLSL



#if !SUPPORT_PROFILE_ARB1
#define PROFILE_EMITTER_ARB1(op)
#else
#undef AT_LEAST_ONE_PROFILE
#define AT_LEAST_ONE_PROFILE 1
#define PROFILE_EMITTER_ARB1(op) emit_ARB1_##op,

static inline const char *get_ARB1_register_string(Context *ctx,
                        const RegisterType regtype, const int regnum,
                        char *regnum_str, const size_t regnum_size)
{
    // turns out these are identical at the moment.
    return get_D3D_register_string(ctx,regtype,regnum,regnum_str,regnum_size);
} // get_ARB1_register_string

static const char *allocate_ARB1_scratch_reg_name(Context *ctx, char *buf,
                                                  const size_t buflen)
{
    const int scratch = allocate_scratch_register(ctx);
    snprintf(buf, buflen, "scratch%d", scratch);
    return buf;
} // allocate_ARB1_scratch_reg_name

static inline const char *get_ARB1_branch_label_name(Context *ctx, const int id,
                                                char *buf, const size_t buflen)
{
    snprintf(buf, buflen, "branch_label%d", id);
    return buf;
} // get_ARB1_branch_label_name

static const char *get_ARB1_varname_in_buf(Context *ctx, const RegisterType rt,
                                           const int regnum, char *buf,
                                           const size_t buflen)
{
    // turns out these are identical at the moment.
    return get_D3D_varname_in_buf(ctx, rt, regnum, buf, buflen);
} // get_ARB1_varname_in_buf

static const char *get_ARB1_varname(Context *ctx, const RegisterType rt,
                                    const int regnum)
{
    // turns out these are identical at the moment.
    return get_D3D_varname(ctx, rt, regnum);
} // get_ARB1_varname


static inline const char *get_ARB1_const_array_varname_in_buf(Context *ctx,
                                                const int base, const int size,
                                                char *buf, const size_t buflen)
{
    snprintf(buf, buflen, "c_array_%d_%d", base, size);
    return buf;
} // get_ARB1_const_array_varname_in_buf


static const char *get_ARB1_const_array_varname(Context *ctx, int base, int size)
{
    char buf[64];
    get_ARB1_const_array_varname_in_buf(ctx, base, size, buf, sizeof (buf));
    return StrDup(ctx, buf);
} // get_ARB1_const_array_varname


static const char *make_ARB1_srcarg_string_in_buf(Context *ctx,
                                                  const SourceArgInfo *arg,
                                                  char *buf, size_t buflen)
{
    char regnum_str[16] = { '\0' };

    // !!! FIXME: use get_ARB1_varname_in_buf() instead?
    const char *regtype_str = NULL;
    if (!arg->relative)
    {
        regtype_str = get_ARB1_register_string(ctx, arg->regtype,
                                               arg->regnum, regnum_str,
                                               sizeof (regnum_str));
    } // if

    const char *rel_lbracket = "";
    char rel_offset[32] = { '\0' };
    const char *rel_rbracket = "";
    char rel_swizzle[4] = { '\0' };
    const char *rel_regtype_str = "";
    if (arg->relative)
    {
        rel_regtype_str = get_ARB1_varname_in_buf(ctx, arg->relative_regtype,
                                                  arg->relative_regnum,
                                                  (char *) alloca(64), 64);

        rel_swizzle[0] = '.';
        rel_swizzle[1] = swizzle_channels[arg->relative_component];
        rel_swizzle[2] = '\0';

        if (!support_nv2(ctx))
        {
            // The address register in ARB1 only allows the '.x' component, so
            //  we need to load the component we need from a temp vector
            //  register into .x as needed.
            assert(arg->relative_regtype == REG_TYPE_ADDRESS);
            assert(arg->relative_regnum == 0);
            if (ctx->last_address_reg_component != arg->relative_component)
            {
                output_line(ctx, "ARL %s.x, addr%d.%c;", rel_regtype_str,
                            arg->relative_regnum,
                            swizzle_channels[arg->relative_component]);
                ctx->last_address_reg_component = arg->relative_component;
            } // if

            rel_swizzle[1] = 'x';
        } // if

        if (arg->regtype == REG_TYPE_INPUT)
            regtype_str = "vertex.attrib";
        else
        {
            assert(arg->regtype == REG_TYPE_CONST);
            const int arrayidx = arg->relative_array->index;
            const int arraysize = arg->relative_array->count;
            const int offset = arg->regnum - arrayidx;
            assert(offset >= 0);
            regtype_str = get_ARB1_const_array_varname_in_buf(ctx, arrayidx,
                                           arraysize, (char *) alloca(64), 64);
            if (offset != 0)
                snprintf(rel_offset, sizeof (rel_offset), " + %d", offset);
        } // else

        rel_lbracket = "[";
        rel_rbracket = "]";
    } // if

    // This is the source register with everything but swizzle and source mods.
    snprintf(buf, buflen, "%s%s%s%s%s%s%s", regtype_str, regnum_str,
             rel_lbracket, rel_regtype_str, rel_swizzle, rel_offset,
             rel_rbracket);

    // Some of the source mods need to generate instructions to a temp
    //  register, in which case we'll replace the register name.

    const char *premod_str = "";
    const char *postmod_str = "";
    switch (arg->src_mod)
    {
        case SRCMOD_NEGATE:
            premod_str = "-";
            break;

        case SRCMOD_BIASNEGATE:
            premod_str = "-";
            // fall through.
        case SRCMOD_BIAS:
            fail(ctx, "SRCMOD_BIAS currently unsupported in arb1");
            postmod_str = "_bias";
            break;

        case SRCMOD_SIGNNEGATE:
            premod_str = "-";
            // fall through.
        case SRCMOD_SIGN:
            fail(ctx, "SRCMOD_SIGN currently unsupported in arb1");
            postmod_str = "_bx2";
            break;

        case SRCMOD_COMPLEMENT:
            fail(ctx, "SRCMOD_COMPLEMENT currently unsupported in arb1");
            premod_str = "1-";
            break;

        case SRCMOD_X2NEGATE:
            premod_str = "-";
            // fall through.
        case SRCMOD_X2:
            fail(ctx, "SRCMOD_X2 currently unsupported in arb1");
            postmod_str = "_x2";
            break;

        case SRCMOD_DZ:
            fail(ctx, "SRCMOD_DZ currently unsupported in arb1");
            postmod_str = "_dz";
            break;

        case SRCMOD_DW:
            fail(ctx, "SRCMOD_DW currently unsupported in arb1");
            postmod_str = "_dw";
            break;

        case SRCMOD_ABSNEGATE:
            premod_str = "-";
            // fall through.
        case SRCMOD_ABS:
            if (support_nv2(ctx))  // GL_NV_vertex_program2_option adds this.
            {
                premod_str = (arg->src_mod == SRCMOD_ABSNEGATE) ? "-|" : "|";
                postmod_str = "|";
            } // if
            else
            {
                regtype_str = allocate_ARB1_scratch_reg_name(ctx,
                                                    (char *) alloca(64), 64);
                regnum_str[0] = '\0'; // move value to scratch register.
                rel_lbracket = "";   // scratch register won't use array.
                rel_rbracket = "";
                rel_offset[0] = '\0';
                rel_swizzle[0] = '\0';
                rel_regtype_str = "";
                output_line(ctx, "ABS %s, %s;", regtype_str, buf);
            } // else
            break;

        case SRCMOD_NOT:
            fail(ctx, "SRCMOD_NOT currently unsupported in arb1");
            premod_str = "!";
            break;

        case SRCMOD_NONE:
        case SRCMOD_TOTAL:
             break;  // stop compiler whining.
    } // switch

    char swizzle_str[6];
    size_t i = 0;

    if (support_nv4(ctx))  // vFace must be output as "vFace.x" in nv4.
    {
        if (arg->regtype == REG_TYPE_MISCTYPE)
        {
            if ( ((const MiscTypeType) arg->regnum) == MISCTYPE_TYPE_FACE )
            {
                swizzle_str[i++] = '.';
                swizzle_str[i++] = 'x';
            } // if
        } // if
    } // if

    const int scalar = isscalar(ctx, ctx->shader_type, arg->regtype, arg->regnum);
    if (!scalar && !no_swizzle(arg->swizzle))
    {
        swizzle_str[i++] = '.';

        // .xxxx is the same as .x, but .xx is illegal...scalar or full!
        if (replicate_swizzle(arg->swizzle))
            swizzle_str[i++] = swizzle_channels[arg->swizzle_x];
        else
        {
            swizzle_str[i++] = swizzle_channels[arg->swizzle_x];
            swizzle_str[i++] = swizzle_channels[arg->swizzle_y];
            swizzle_str[i++] = swizzle_channels[arg->swizzle_z];
            swizzle_str[i++] = swizzle_channels[arg->swizzle_w];
        } // else
    } // if
    swizzle_str[i] = '\0';
    assert(i < sizeof (swizzle_str));

    snprintf(buf, buflen, "%s%s%s%s%s%s%s%s%s%s", premod_str,
             regtype_str, regnum_str, rel_lbracket,
             rel_regtype_str, rel_swizzle, rel_offset, rel_rbracket,
             swizzle_str, postmod_str);
    // !!! FIXME: make sure the scratch buffer was large enough.
    return buf;
} // make_ARB1_srcarg_string_in_buf

static const char *get_ARB1_destarg_varname(Context *ctx, char *buf,
                                            const size_t buflen)
{
    const DestArgInfo *arg = &ctx->dest_arg;
    return get_ARB1_varname_in_buf(ctx, arg->regtype, arg->regnum, buf, buflen);
} // get_ARB1_destarg_varname

static const char *get_ARB1_srcarg_varname(Context *ctx, const size_t idx,
                                           char *buf, const size_t buflen)
{
    if (idx >= STATICARRAYLEN(ctx->source_args))
    {
        fail(ctx, "Too many source args");
        *buf = '\0';
        return buf;
    } // if

    const SourceArgInfo *arg = &ctx->source_args[idx];
    return get_ARB1_varname_in_buf(ctx, arg->regtype, arg->regnum, buf, buflen);
} // get_ARB1_srcarg_varname


static const char *make_ARB1_destarg_string(Context *ctx, char *buf,
                                            const size_t buflen)
{
    const DestArgInfo *arg = &ctx->dest_arg;

    *buf = '\0';

    const char *sat_str = "";
    if (arg->result_mod & MOD_SATURATE)
    {
        // nv4 can use ".SAT" in all program types.
        // For less than nv4, the "_SAT" modifier is only available in
        //  fragment shaders. Every thing else will fake it later in
        //  emit_ARB1_dest_modifiers() ...
        if (support_nv4(ctx))
            sat_str = ".SAT";
        else if (shader_is_pixel(ctx))
            sat_str = "_SAT";
    } // if

    const char *pp_str = "";
    if (arg->result_mod & MOD_PP)
    {
        // Most ARB1 profiles can't do partial precision (MOD_PP), but that's
        //  okay. The spec says lots of Direct3D implementations ignore the
        //  flag anyhow.
        if (support_nv4(ctx))
            pp_str = "H";
    } // if

    // CENTROID only allowed in DCL opcodes, which shouldn't come through here.
    assert((arg->result_mod & MOD_CENTROID) == 0);

    char regnum_str[16];
    const char *regtype_str = get_ARB1_register_string(ctx, arg->regtype,
                                                       arg->regnum, regnum_str,
                                                       sizeof (regnum_str));
    if (regtype_str == NULL)
    {
        fail(ctx, "Unknown destination register type.");
        return buf;
    } // if

    char writemask_str[6];
    size_t i = 0;
    const int scalar = isscalar(ctx, ctx->shader_type, arg->regtype, arg->regnum);
    if (!scalar && !writemask_xyzw(arg->writemask))
    {
        writemask_str[i++] = '.';
        if (arg->writemask0) writemask_str[i++] = 'x';
        if (arg->writemask1) writemask_str[i++] = 'y';
        if (arg->writemask2) writemask_str[i++] = 'z';
        if (arg->writemask3) writemask_str[i++] = 'w';
    } // if
    writemask_str[i] = '\0';
    assert(i < sizeof (writemask_str));

    const char *pred_left = "";
    const char *pred_right = "";
    char pred[32] = { '\0' };
    if (ctx->predicated)
    {
        fail(ctx, "dest register predication currently unsupported in arb1");
        return buf;
        pred_left = "(";
        pred_right = ") ";
        make_ARB1_srcarg_string_in_buf(ctx, &ctx->predicate_arg,
                                       pred, sizeof (pred));
    } // if

    snprintf(buf, buflen, "%s%s %s%s%s", pp_str, sat_str,
             regtype_str, regnum_str, writemask_str);
    // !!! FIXME: make sure the scratch buffer was large enough.
    return buf;
} // make_ARB1_destarg_string


static void emit_ARB1_dest_modifiers(Context *ctx)
{
    const DestArgInfo *arg = &ctx->dest_arg;

    if (arg->result_shift != 0x0)
    {
        char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
        const char *multiplier = NULL;

        switch (arg->result_shift)
        {
            case 0x1: multiplier = "2.0"; break;
            case 0x2: multiplier = "4.0"; break;
            case 0x3: multiplier = "8.0"; break;
            case 0xD: multiplier = "0.125"; break;
            case 0xE: multiplier = "0.25"; break;
            case 0xF: multiplier = "0.5"; break;
        } // switch

        if (multiplier != NULL)
        {
            char var[64]; get_ARB1_destarg_varname(ctx, var, sizeof (var));
            output_line(ctx, "MUL%s, %s, %s;", dst, var, multiplier);
        } // if
    } // if

    if (arg->result_mod & MOD_SATURATE)
    {
        // nv4 and/or pixel shaders just used the "SAT" modifier, instead.
        if ( (!support_nv4(ctx)) && (!shader_is_pixel(ctx)) )
        {
            char var[64]; get_ARB1_destarg_varname(ctx, var, sizeof (var));
            char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
            output_line(ctx, "MIN%s, %s, 1.0;", dst, var);
            output_line(ctx, "MAX%s, %s, 0.0;", dst, var);
        } // if
    } // if
} // emit_ARB1_dest_modifiers


static const char *make_ARB1_srcarg_string(Context *ctx, const size_t idx,
                                           char *buf, const size_t buflen)
{
    if (idx >= STATICARRAYLEN(ctx->source_args))
    {
        fail(ctx, "Too many source args");
        *buf = '\0';
        return buf;
    } // if

    const SourceArgInfo *arg = &ctx->source_args[idx];
    return make_ARB1_srcarg_string_in_buf(ctx, arg, buf, buflen);
} // make_ARB1_srcarg_string

static void emit_ARB1_opcode_ds(Context *ctx, const char *opcode)
{
    char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
    output_line(ctx, "%s%s, %s;", opcode, dst, src0);
    emit_ARB1_dest_modifiers(ctx);
} // emit_ARB1_opcode_ds

static void emit_ARB1_opcode_dss(Context *ctx, const char *opcode)
{
    char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_ARB1_srcarg_string(ctx, 1, src1, sizeof (src1));
    output_line(ctx, "%s%s, %s, %s;", opcode, dst, src0, src1);
    emit_ARB1_dest_modifiers(ctx);
} // emit_ARB1_opcode_dss

static void emit_ARB1_opcode_dsss(Context *ctx, const char *opcode)
{
    char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_ARB1_srcarg_string(ctx, 1, src1, sizeof (src1));
    char src2[64]; make_ARB1_srcarg_string(ctx, 2, src2, sizeof (src2));
    output_line(ctx, "%s%s, %s, %s, %s;", opcode, dst, src0, src1, src2);
    emit_ARB1_dest_modifiers(ctx);
} // emit_ARB1_opcode_dsss


#define EMIT_ARB1_OPCODE_FUNC(op) \
    static void emit_ARB1_##op(Context *ctx) { \
        emit_ARB1_opcode(ctx, #op); \
    }
#define EMIT_ARB1_OPCODE_D_FUNC(op) \
    static void emit_ARB1_##op(Context *ctx) { \
        emit_ARB1_opcode_d(ctx, #op); \
    }
#define EMIT_ARB1_OPCODE_S_FUNC(op) \
    static void emit_ARB1_##op(Context *ctx) { \
        emit_ARB1_opcode_s(ctx, #op); \
    }
#define EMIT_ARB1_OPCODE_SS_FUNC(op) \
    static void emit_ARB1_##op(Context *ctx) { \
        emit_ARB1_opcode_ss(ctx, #op); \
    }
#define EMIT_ARB1_OPCODE_DS_FUNC(op) \
    static void emit_ARB1_##op(Context *ctx) { \
        emit_ARB1_opcode_ds(ctx, #op); \
    }
#define EMIT_ARB1_OPCODE_DSS_FUNC(op) \
    static void emit_ARB1_##op(Context *ctx) { \
        emit_ARB1_opcode_dss(ctx, #op); \
    }
#define EMIT_ARB1_OPCODE_DSSS_FUNC(op) \
    static void emit_ARB1_##op(Context *ctx) { \
        emit_ARB1_opcode_dsss(ctx, #op); \
    }
#define EMIT_ARB1_OPCODE_DSSSS_FUNC(op) \
    static void emit_ARB1_##op(Context *ctx) { \
        emit_ARB1_opcode_dssss(ctx, #op); \
    }
#define EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(op) \
    static void emit_ARB1_##op(Context *ctx) { \
        failf(ctx, #op " unimplemented in %s profile", ctx->profile->name); \
    }


static void emit_ARB1_start(Context *ctx, const char *profilestr)
{
    const char *shader_str = NULL;
    const char *shader_full_str = NULL;
    if (shader_is_vertex(ctx))
    {
        shader_str = "vp";
        shader_full_str = "vertex";
    } // if
    else if (shader_is_pixel(ctx))
    {
        shader_str = "fp";
        shader_full_str = "fragment";
    } // else if
    else
    {
        failf(ctx, "Shader type %u unsupported in this profile.",
              (uint) ctx->shader_type);
        return;
    } // if

    set_output(ctx, &ctx->preflight);

    if (strcmp(profilestr, MOJOSHADER_PROFILE_ARB1) == 0)
        output_line(ctx, "!!ARB%s1.0", shader_str);

    #if SUPPORT_PROFILE_ARB1_NV
    else if (strcmp(profilestr, MOJOSHADER_PROFILE_NV2) == 0)
    {
        ctx->profile_supports_nv2 = 1;
        output_line(ctx, "!!ARB%s1.0", shader_str);
        output_line(ctx, "OPTION NV_%s_program2;", shader_full_str);
    } // else if

    else if (strcmp(profilestr, MOJOSHADER_PROFILE_NV3) == 0)
    {
        // there's no NV_fragment_program3, so just use 2.
        const int ver = shader_is_pixel(ctx) ? 2 : 3;
        ctx->profile_supports_nv2 = 1;
        ctx->profile_supports_nv3 = 1;
        output_line(ctx, "!!ARB%s1.0", shader_str);
        output_line(ctx, "OPTION NV_%s_program%d;", shader_full_str, ver);
    } // else if

    else if (strcmp(profilestr, MOJOSHADER_PROFILE_NV4) == 0)
    {
        ctx->profile_supports_nv2 = 1;
        ctx->profile_supports_nv3 = 1;
        ctx->profile_supports_nv4 = 1;
        output_line(ctx, "!!NV%s4.0", shader_str);
    } // else if
    #endif

    else
    {
        failf(ctx, "Profile '%s' unsupported or unknown.", profilestr);
    } // else

    set_output(ctx, &ctx->mainline);
} // emit_ARB1_start

static void emit_ARB1_end(Context *ctx)
{
    output_line(ctx, "END");
} // emit_ARB1_end

static void emit_ARB1_phase(Context *ctx)
{
    // no-op in arb1.
} // emit_ARB1_phase

static inline const char *arb1_float_temp(const Context *ctx)
{
    // nv4 lets you specify data type.
    return (support_nv4(ctx)) ? "FLOAT TEMP" : "TEMP";
} // arb1_float_temp

static void emit_ARB1_finalize(Context *ctx)
{
    push_output(ctx, &ctx->preflight);

    if (shader_is_vertex(ctx) && !ctx->arb1_wrote_position)
        output_line(ctx, "OPTION ARB_position_invariant;");

    if (shader_is_pixel(ctx) && ctx->have_multi_color_outputs)
        output_line(ctx, "OPTION ARB_draw_buffers;");

    pop_output(ctx);

    const char *tmpstr = arb1_float_temp(ctx);
    int i;
    push_output(ctx, &ctx->globals);
    for (i = 0; i < ctx->max_scratch_registers; i++)
    {
        char buf[64];
        allocate_ARB1_scratch_reg_name(ctx, buf, sizeof (buf));
        output_line(ctx, "%s %s;", tmpstr, buf);
    } // for

    // nv2 fragment programs (and anything nv4) have a real REP/ENDREP.
    if ( (support_nv2(ctx)) && (!shader_is_pixel(ctx)) && (!support_nv4(ctx)) )
    {
        // set up temps for nv2 REP/ENDREP emulation through branching.
        for (i = 0; i < ctx->max_reps; i++)
            output_line(ctx, "TEMP rep%d;", i);
    } // if

    pop_output(ctx);
    assert(ctx->scratch_registers == ctx->max_scratch_registers);
} // emit_ARB1_finalize

static void emit_ARB1_global(Context *ctx, RegisterType regtype, int regnum)
{
    // !!! FIXME: dependency on ARB1 profile.  // !!! FIXME about FIXME: huh?
    char varname[64];
    get_ARB1_varname_in_buf(ctx, regtype, regnum, varname, sizeof (varname));

    push_output(ctx, &ctx->globals);
    switch (regtype)
    {
        case REG_TYPE_ADDRESS:
            // nv4 replaced address registers with generic int registers.
            if (support_nv4(ctx))
                output_line(ctx, "INT TEMP %s;", varname);
            else
            {
                // nv2 has four-component address already, but stock arb1 has
                //  to emulate it in a temporary, and move components to the
                //  scalar ADDRESS register on demand.
                output_line(ctx, "ADDRESS %s;", varname);
                if (!support_nv2(ctx))
                    output_line(ctx, "TEMP addr%d;", regnum);
            } // else
            break;

        //case REG_TYPE_PREDICATE:
        //    output_line(ctx, "bvec4 %s;", varname);
        //    break;
        case REG_TYPE_TEMP:
            output_line(ctx, "%s %s;", arb1_float_temp(ctx), varname);
            break;
        //case REG_TYPE_LOOP:
        //    break; // no-op. We declare these in for loops at the moment.
        //case REG_TYPE_LABEL:
        //    break; // no-op. If we see it here, it means we optimized it out.
        default:
            fail(ctx, "BUG: we used a register we don't know how to define.");
            break;
    } // switch
    pop_output(ctx);
} // emit_ARB1_global

static void emit_ARB1_array(Context *ctx, VariableList *var)
{
    // All uniforms are now packed tightly into the program.local array,
    //  instead of trying to map them to the d3d registers. So this needs to
    //  map to the next piece of the array we haven't used yet. Thankfully,
    //  arb1 lets you make a PARAM array that maps to a subset of another
    //  array; we don't need to do offsets, since myarray[0] can map to
    //  program.local[5] without any extra math from us.
    const int base = var->index;
    const int size = var->count;
    const int arb1base = ctx->uniform_float4_count +
                         ctx->uniform_int4_count +
                         ctx->uniform_bool_count;
    char varname[64];
    get_ARB1_const_array_varname_in_buf(ctx, base, size, varname, sizeof (varname));
    push_output(ctx, &ctx->globals);
    output_line(ctx, "PARAM %s[%d] = { program.local[%d..%d] };", varname,
                size, arb1base, (arb1base + size) - 1);
    pop_output(ctx);
    var->emit_position = arb1base;
} // emit_ARB1_array

static void emit_ARB1_const_array(Context *ctx, const ConstantsList *clist,
                                  int base, int size)
{
    char varname[64];
    get_ARB1_const_array_varname_in_buf(ctx, base, size, varname, sizeof (varname));
    int i;

    push_output(ctx, &ctx->globals);
    output_line(ctx, "PARAM %s[%d] = {", varname, size);
    ctx->indent++;

    for (i = 0; i < size; i++)
    {
        while (clist->constant.type != MOJOSHADER_UNIFORM_FLOAT)
            clist = clist->next;
        assert(clist->constant.index == (base + i));

        char val0[32];
        char val1[32];
        char val2[32];
        char val3[32];
        floatstr(ctx, val0, sizeof (val0), clist->constant.value.f[0], 1);
        floatstr(ctx, val1, sizeof (val1), clist->constant.value.f[1], 1);
        floatstr(ctx, val2, sizeof (val2), clist->constant.value.f[2], 1);
        floatstr(ctx, val3, sizeof (val3), clist->constant.value.f[3], 1);

        output_line(ctx, "{ %s, %s, %s, %s }%s", val0, val1, val2, val3,
                    (i < (size-1)) ? "," : "");

        clist = clist->next;
    } // for

    ctx->indent--;
    output_line(ctx, "};");
    pop_output(ctx);
} // emit_ARB1_const_array

static void emit_ARB1_uniform(Context *ctx, RegisterType regtype, int regnum,
                              const VariableList *var)
{
    // We pack these down into the program.local array, so if we only use
    //  register c439, it'll actually map to program.local[0]. This will
    //  prevent overflows when we actually have enough resources to run.

    const char *arrayname = "program.local";
    int index = 0;

    char varname[64];
    get_ARB1_varname_in_buf(ctx, regtype, regnum, varname, sizeof (varname));

    push_output(ctx, &ctx->globals);

    if (var == NULL)
    {
        // all types share one array (rather, all types convert to float4).
        index = ctx->uniform_float4_count + ctx->uniform_int4_count +
                ctx->uniform_bool_count;
    } // if

    else
    {
        const int arraybase = var->index;
        if (var->constant)
        {
            const int arraysize = var->count;
            arrayname = get_ARB1_const_array_varname_in_buf(ctx, arraybase,
                                        arraysize, (char *) alloca(64), 64);
            index = (regnum - arraybase);
        } // if
        else
        {
            assert(var->emit_position != -1);
            index = (regnum - arraybase) + var->emit_position;
        } // else
    } // else

    output_line(ctx, "PARAM %s = %s[%d];", varname, arrayname, index);
    pop_output(ctx);
} // emit_ARB1_uniform

static void emit_ARB1_sampler(Context *ctx, int stage, TextureType ttype)
{
    // this is a no-op...you don't predeclare samplers in arb1.
} // emit_ARB1_sampler

// !!! FIXME: a lot of cut-and-paste here from emit_GLSL_attribute().
static void emit_ARB1_attribute(Context *ctx, RegisterType regtype, int regnum,
                                MOJOSHADER_usage usage, int index, int wmask,
                                int flags)
{
    // !!! FIXME: this function doesn't deal with write masks at all yet!
    const char *usage_str = NULL;
    const char *arrayleft = "";
    const char *arrayright = "";
    char index_str[16] = { '\0' };

    char varname[64];
    get_ARB1_varname_in_buf(ctx, regtype, regnum, varname, sizeof (varname));

    //assert((flags & MOD_PP) == 0);  // !!! FIXME: is PP allowed?

    if (index != 0)  // !!! FIXME: a lot of these MUST be zero.
        snprintf(index_str, sizeof (index_str), "%u", (uint) index);

    if (shader_is_vertex(ctx))
    {
        // pre-vs3 output registers.
        // these don't ever happen in DCL opcodes, I think. Map to vs_3_*
        //  output registers.
        if (!shader_version_atleast(ctx, 3, 0))
        {
            if (regtype == REG_TYPE_RASTOUT)
            {
                regtype = REG_TYPE_OUTPUT;
                index = regnum;
                switch ((const RastOutType) regnum)
                {
                    case RASTOUT_TYPE_POSITION:
                        usage = MOJOSHADER_USAGE_POSITION;
                        break;
                    case RASTOUT_TYPE_FOG:
                        usage = MOJOSHADER_USAGE_FOG;
                        break;
                    case RASTOUT_TYPE_POINT_SIZE:
                        usage = MOJOSHADER_USAGE_POINTSIZE;
                        break;
                } // switch
            } // if

            else if (regtype == REG_TYPE_ATTROUT)
            {
                regtype = REG_TYPE_OUTPUT;
                usage = MOJOSHADER_USAGE_COLOR;
                index = regnum;
            } // else if

            else if (regtype == REG_TYPE_TEXCRDOUT)
            {
                regtype = REG_TYPE_OUTPUT;
                usage = MOJOSHADER_USAGE_TEXCOORD;
                index = regnum;
            } // else if
        } // if

        // to avoid limitations of various GL entry points for input
        // attributes (glSecondaryColorPointer() can only take 3 component
        // items, glVertexPointer() can't do GL_UNSIGNED_BYTE, many other
        // issues), we set up all inputs as generic vertex attributes, so we
        // can pass data in just about any form, and ignore the built-in GLSL
        // attributes like gl_SecondaryColor. Output needs to use the the
        // built-ins, though, but we don't have to worry about the GL entry
        // point limitations there.

        if (regtype == REG_TYPE_INPUT)
        {
            const int attr = ctx->assigned_vertex_attributes++;
            push_output(ctx, &ctx->globals);
            output_line(ctx, "ATTRIB %s = vertex.attrib[%d];", varname, attr);
            pop_output(ctx);
        } // if

        else if (regtype == REG_TYPE_OUTPUT)
        {
            switch (usage)
            {
                case MOJOSHADER_USAGE_POSITION:
                    ctx->arb1_wrote_position = 1;
                    usage_str = "result.position";
                    break;
                case MOJOSHADER_USAGE_POINTSIZE:
                    usage_str = "result.pointsize";
                    break;
                case MOJOSHADER_USAGE_COLOR:
                    index_str[0] = '\0';  // no explicit number.
                    if (index == 0)
                        usage_str = "result.color.primary";
                    else if (index == 1)
                        usage_str = "result.color.secondary";
                    break;
                case MOJOSHADER_USAGE_FOG:
                    usage_str = "result.fogcoord";
                    break;
                case MOJOSHADER_USAGE_TEXCOORD:
                    snprintf(index_str, sizeof (index_str), "%u", (uint) index);
                    usage_str = "result.texcoord";
                    arrayleft = "[";
                    arrayright = "]";
                    break;
                default:
                    // !!! FIXME: we need to deal with some more built-in varyings here.
                    break;
            } // switch

            // !!! FIXME: the #define is a little hacky, but it means we don't
            // !!! FIXME:  have to track these separately if this works.
            push_output(ctx, &ctx->globals);
            // no mapping to built-in var? Just make it a regular global, pray.
            if (usage_str == NULL)
                output_line(ctx, "%s %s;", arb1_float_temp(ctx), varname);
            else
            {
                output_line(ctx, "OUTPUT %s = %s%s%s%s;", varname, usage_str,
                            arrayleft, index_str, arrayright);
            } // else
            pop_output(ctx);
        } // else if

        else
        {
            fail(ctx, "unknown vertex shader attribute register");
        } // else
    } // if

    else if (shader_is_pixel(ctx))
    {
        const char *paramtype_str = "ATTRIB";

        // samplers DCLs get handled in emit_ARB1_sampler().

        if (flags & MOD_CENTROID)
        {
            if (!support_nv4(ctx))  // GL_NV_fragment_program4 adds centroid.
            {
                // !!! FIXME: should we just wing it without centroid here?
                failf(ctx, "centroid unsupported in %s profile",
                      ctx->profile->name);
                return;
            } // if

            paramtype_str = "CENTROID ATTRIB";
        } // if

        if (regtype == REG_TYPE_COLOROUT)
        {
            paramtype_str = "OUTPUT";
            usage_str = "result.color";
            if (ctx->have_multi_color_outputs)
            {
                // We have to gamble that you have GL_ARB_draw_buffers.
                // You probably do at this point if you have a sane setup.
                snprintf(index_str, sizeof (index_str), "%u", (uint) regnum);
                arrayleft = "[";
                arrayright = "]";
            } // if
        } // if

        else if (regtype == REG_TYPE_DEPTHOUT)
        {
            paramtype_str = "OUTPUT";
            usage_str = "result.depth";
        } // else if

        // !!! FIXME: can you actualy have a texture register with COLOR usage?
        else if ((regtype == REG_TYPE_TEXTURE) || (regtype == REG_TYPE_INPUT))
        {
            if (usage == MOJOSHADER_USAGE_TEXCOORD)
            {
                snprintf(index_str, sizeof (index_str), "%u", (uint) index);
                usage_str = "fragment.texcoord";
                arrayleft = "[";
                arrayright = "]";
            } // if

            else if (usage == MOJOSHADER_USAGE_COLOR)
            {
                index_str[0] = '\0';  // no explicit number.
                if (index == 0)
                    usage_str = "fragment.color.primary";
                else if (index == 1)
                    usage_str = "fragment.color.secondary";
                else
                    fail(ctx, "unsupported color index");
            } // else if
        } // else if

        else if (regtype == REG_TYPE_MISCTYPE)
        {
            const MiscTypeType mt = (MiscTypeType) regnum;
            if (mt == MISCTYPE_TYPE_FACE)
            {
                if (support_nv4(ctx))  // FINALLY, a vFace equivalent in nv4!
                {
                    index_str[0] = '\0';  // no explicit number.
                    usage_str = "fragment.facing";
                } // if
                else
                {
                    failf(ctx, "vFace unsupported in %s profile",
                          ctx->profile->name);
                } // else
            } // if
            else if (mt == MISCTYPE_TYPE_POSITION)
            {
                index_str[0] = '\0';  // no explicit number.
                usage_str = "fragment.position";  // !!! FIXME: is this the same coord space as D3D?
            } // else if
            else
            {
                fail(ctx, "BUG: unhandled misc register");
            } // else
        } // else if

        else
        {
            fail(ctx, "unknown pixel shader attribute register");
        } // else

        if (usage_str != NULL)
        {
            push_output(ctx, &ctx->globals);
            output_line(ctx, "%s %s = %s%s%s%s;", paramtype_str, varname,
                        usage_str, arrayleft, index_str, arrayright);
            pop_output(ctx);
        } // if
    } // else if

    else
    {
        fail(ctx, "Unknown shader type");  // state machine should catch this.
    } // else
} // emit_ARB1_attribute

static void emit_ARB1_RESERVED(Context *ctx) { /* no-op. */ }

static void emit_ARB1_NOP(Context *ctx)
{
    // There is no NOP in arb1. Just don't output anything here.
} // emit_ARB1_NOP

EMIT_ARB1_OPCODE_DS_FUNC(MOV)
EMIT_ARB1_OPCODE_DSS_FUNC(ADD)
EMIT_ARB1_OPCODE_DSS_FUNC(SUB)
EMIT_ARB1_OPCODE_DSSS_FUNC(MAD)
EMIT_ARB1_OPCODE_DSS_FUNC(MUL)
EMIT_ARB1_OPCODE_DS_FUNC(RCP)

static void emit_ARB1_RSQ(Context *ctx)
{
    // nv4 doesn't force abs() on this, so negative values will generate NaN.
    // The spec says you should force the abs() yourself.
    if (!support_nv4(ctx))
    {
        emit_ARB1_opcode_ds(ctx, "RSQ");  // pre-nv4 implies ABS.
        return;
    } // if

    // we can optimize this to use nv2's |abs| construct in some cases.
    if ( (ctx->source_args[0].src_mod == SRCMOD_NONE) ||
         (ctx->source_args[0].src_mod == SRCMOD_NEGATE) ||
         (ctx->source_args[0].src_mod == SRCMOD_ABSNEGATE) )
        ctx->source_args[0].src_mod = SRCMOD_ABS;

    char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));

    if (ctx->source_args[0].src_mod == SRCMOD_ABS)
        output_line(ctx, "RSQ%s, %s;", dst, src0);
    else
    {
        char buf[64]; allocate_ARB1_scratch_reg_name(ctx, buf, sizeof (buf));
        output_line(ctx, "ABS %s, %s;", buf, src0);
        output_line(ctx, "RSQ%s, %s.x;", dst, buf);
    } // else

    emit_ARB1_dest_modifiers(ctx);
} // emit_ARB1_RSQ

EMIT_ARB1_OPCODE_DSS_FUNC(DP3)
EMIT_ARB1_OPCODE_DSS_FUNC(DP4)
EMIT_ARB1_OPCODE_DSS_FUNC(MIN)
EMIT_ARB1_OPCODE_DSS_FUNC(MAX)
EMIT_ARB1_OPCODE_DSS_FUNC(SLT)
EMIT_ARB1_OPCODE_DSS_FUNC(SGE)

static void emit_ARB1_EXP(Context *ctx) { emit_ARB1_opcode_ds(ctx, "EX2"); }

static void arb1_log(Context *ctx, const char *opcode)
{
    // !!! FIXME: SRCMOD_NEGATE can be made into SRCMOD_ABS here, too
    // we can optimize this to use nv2's |abs| construct in some cases.
    if ( (ctx->source_args[0].src_mod == SRCMOD_NONE) ||
         (ctx->source_args[0].src_mod == SRCMOD_ABSNEGATE) )
        ctx->source_args[0].src_mod = SRCMOD_ABS;

    char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));

    if (ctx->source_args[0].src_mod == SRCMOD_ABS)
        output_line(ctx, "%s%s, %s;", opcode, dst, src0);
    else
    {
        char buf[64]; allocate_ARB1_scratch_reg_name(ctx, buf, sizeof (buf));
        output_line(ctx, "ABS %s, %s;", buf, src0);
        output_line(ctx, "%s%s, %s.x;", opcode, dst, buf);
    } // else

    emit_ARB1_dest_modifiers(ctx);
} // arb1_log


static void emit_ARB1_LOG(Context *ctx)
{
    arb1_log(ctx, "LG2");
} // emit_ARB1_LOG


EMIT_ARB1_OPCODE_DS_FUNC(LIT)
EMIT_ARB1_OPCODE_DSS_FUNC(DST)

static void emit_ARB1_LRP(Context *ctx)
{
    if (shader_is_pixel(ctx))  // fragment shaders have a matching LRP opcode.
        emit_ARB1_opcode_dsss(ctx, "LRP");
    else
    {
        char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
        char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
        char src1[64]; make_ARB1_srcarg_string(ctx, 1, src1, sizeof (src1));
        char src2[64]; make_ARB1_srcarg_string(ctx, 2, src2, sizeof (src2));
        char buf[64]; allocate_ARB1_scratch_reg_name(ctx, buf, sizeof (buf));

        // LRP is: dest = src2 + src0 * (src1 - src2)
        output_line(ctx, "SUB %s, %s, %s;", buf, src1, src2);
        output_line(ctx, "MAD%s, %s, %s, %s;", dst, buf, src0, src2);
        emit_ARB1_dest_modifiers(ctx);
    } // else
} // emit_ARB1_LRP

EMIT_ARB1_OPCODE_DS_FUNC(FRC)

// !!! FIXME: these could be implemented with vector opcodes, but it looks
// !!! FIXME:  like the Microsoft HLSL compiler never generates matrix
// !!! FIXME:  operations for some reason.
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(M4X4)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(M4X3)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(M3X4)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(M3X3)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(M3X2)

static void emit_ARB1_CALL(Context *ctx)
{
    if (!support_nv2(ctx))  // no branching in stock ARB1.
    {
        failf(ctx, "branching unsupported in %s profile", ctx->profile->name);
        return;
    } // if

    char labelstr[64];
    get_ARB1_srcarg_varname(ctx, 0, labelstr, sizeof (labelstr));
    output_line(ctx, "CAL %s;", labelstr);
} // emit_ARB1_CALL

static void emit_ARB1_CALLNZ(Context *ctx)
{
    // !!! FIXME: if src1 is a constbool that's true, we can remove the
    // !!! FIXME:  if. If it's false, we can make this a no-op.

    if (!support_nv2(ctx))  // no branching in stock ARB1.
        failf(ctx, "branching unsupported in %s profile", ctx->profile->name);
    else
    {
        // !!! FIXME: double-check this.
        char labelstr[64];
        char scratch[64];
        char src1[64];
        get_ARB1_srcarg_varname(ctx, 0, labelstr, sizeof (labelstr));
        get_ARB1_srcarg_varname(ctx, 1, src1, sizeof (src1));
        allocate_ARB1_scratch_reg_name(ctx, scratch, sizeof (scratch));
        output_line(ctx, "MOVC %s, %s;", scratch, src1);
        output_line(ctx, "CAL %s (NE.x);", labelstr);
    } // else
} // emit_ARB1_CALLNZ

// !!! FIXME: needs BRA in nv2, LOOP in nv2 fragment progs, and REP in nv4.
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(LOOP)

static void emit_ARB1_RET(Context *ctx)
{
    // don't fail() if no nv2...maybe we're just ending the mainline?
    //  if we're ending a LABEL that had no CALL, this would all be written
    //  to ctx->ignore anyhow, so this should be "safe" ... arb1 profile will
    //  just end up throwing all this code out.
    if (support_nv2(ctx))  // no branching in stock ARB1.
        output_line(ctx, "RET;");
    set_output(ctx, &ctx->mainline); // in case we were ignoring this function.
} // emit_ARB1_RET


EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(ENDLOOP)

static void emit_ARB1_LABEL(Context *ctx)
{
    if (!support_nv2(ctx))  // no branching in stock ARB1.
        return;  // don't fail()...maybe we never use it, but do fail in CALL.

    const int label = ctx->source_args[0].regnum;
    RegisterList *reg = reglist_find(&ctx->used_registers, REG_TYPE_LABEL, label);

    // MSDN specs say CALL* has to come before the LABEL, so we know if we
    //  can ditch the entire function here as unused.
    if (reg == NULL)
        set_output(ctx, &ctx->ignore);  // Func not used. Parse, but don't output.

    // !!! FIXME: it would be nice if we could determine if a function is
    // !!! FIXME:  only called once and, if so, forcibly inline it.

    //const char *uses_loopreg = ((reg) && (reg->misc == 1)) ? "int aL" : "";
    char labelstr[64];
    get_ARB1_srcarg_varname(ctx, 0, labelstr, sizeof (labelstr));
    output_line(ctx, "%s:", labelstr);
} // emit_ARB1_LABEL


static void emit_ARB1_POW(Context *ctx)
{
    // we can optimize this to use nv2's |abs| construct in some cases.
    if ( (ctx->source_args[0].src_mod == SRCMOD_NONE) ||
         (ctx->source_args[0].src_mod == SRCMOD_ABSNEGATE) )
        ctx->source_args[0].src_mod = SRCMOD_ABS;

    char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_ARB1_srcarg_string(ctx, 1, src1, sizeof (src1));

    if (ctx->source_args[0].src_mod == SRCMOD_ABS)
        output_line(ctx, "POW%s, %s, %s;", dst, src0, src1);
    else
    {
        char buf[64]; allocate_ARB1_scratch_reg_name(ctx, buf, sizeof (buf));
        output_line(ctx, "ABS %s, %s;", buf, src0);
        output_line(ctx, "POW%s, %s.x, %s;", dst, buf, src1);
    } // else

    emit_ARB1_dest_modifiers(ctx);
} // emit_ARB1_POW

static void emit_ARB1_CRS(Context *ctx) { emit_ARB1_opcode_dss(ctx, "XPD"); }

static void emit_ARB1_SGN(Context *ctx)
{
    if (support_nv2(ctx))
        emit_ARB1_opcode_ds(ctx, "SSG");
    else
    {
        char dst[64];
        char src0[64];
        char scratch1[64];
        char scratch2[64];
        make_ARB1_destarg_string(ctx, dst, sizeof (dst));
        make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
        allocate_ARB1_scratch_reg_name(ctx, scratch1, sizeof (scratch1));
        allocate_ARB1_scratch_reg_name(ctx, scratch2, sizeof (scratch2));
        output_line(ctx, "SLT %s, %s, 0.0;", scratch1, src0);
        output_line(ctx, "SLT %s, -%s, 0.0;", scratch2, src0);
        output_line(ctx, "ADD%s -%s, %s;", dst, scratch1, scratch2);
        emit_ARB1_dest_modifiers(ctx);
    } // else
} // emit_ARB1_SGN

EMIT_ARB1_OPCODE_DS_FUNC(ABS)

static void emit_ARB1_NRM(Context *ctx)
{
    // nv2 fragment programs (and anything nv4) have a real NRM.
    if ( (support_nv4(ctx)) || ((support_nv2(ctx)) && (shader_is_pixel(ctx))) )
        emit_ARB1_opcode_ds(ctx, "NRM");
    else
    {
        char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
        char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
        char buf[64]; allocate_ARB1_scratch_reg_name(ctx, buf, sizeof (buf));
        output_line(ctx, "DP3 %s.w, %s, %s;", buf, src0, src0);
        output_line(ctx, "RSQ %s.w, %s.w;", buf, buf);
        output_line(ctx, "MUL%s, %s.w, %s;", dst, buf, src0);
        emit_ARB1_dest_modifiers(ctx);
    } // else
} // emit_ARB1_NRM


static void emit_ARB1_SINCOS(Context *ctx)
{
    // we don't care about the temp registers that <= sm2 demands; ignore them.
    const int mask = ctx->dest_arg.writemask;

    // arb1 fragment programs and everything nv4 have sin/cos/sincos opcodes.
    if ((shader_is_pixel(ctx)) || (support_nv4(ctx)))
    {
        char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
        char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
        if (writemask_x(mask))
            output_line(ctx, "COS%s, %s;", dst, src0);
        else if (writemask_y(mask))
            output_line(ctx, "SIN%s, %s;", dst, src0);
        else if (writemask_xy(mask))
            output_line(ctx, "SCS%s, %s;", dst, src0);
    } // if

    // nv2+ profiles have sin and cos opcodes.
    else if (support_nv2(ctx))
    {
        char dst[64]; get_ARB1_destarg_varname(ctx, dst, sizeof (dst));
        char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
        if (writemask_x(mask))
            output_line(ctx, "COS %s.x, %s;", dst, src0);
        else if (writemask_y(mask))
            output_line(ctx, "SIN %s.y, %s;", dst, src0);
        else if (writemask_xy(mask))
        {
            output_line(ctx, "SIN %s.x, %s;", dst, src0);
            output_line(ctx, "COS %s.y, %s;", dst, src0);
        } // else if
    } // if

    else  // big nasty.
    {
        char dst[64]; get_ARB1_destarg_varname(ctx, dst, sizeof (dst));
        char src0[64]; get_ARB1_srcarg_varname(ctx, 0, src0, sizeof (src0));
        const int need_sin = (writemask_x(mask) || writemask_xy(mask));
        const int need_cos = (writemask_y(mask) || writemask_xy(mask));
        char scratch[64];

        if (need_sin || need_cos)
            allocate_ARB1_scratch_reg_name(ctx, scratch, sizeof (scratch));

        // These sin() and cos() approximations originally found here:
        //    http://www.devmaster.net/forums/showthread.php?t=5784
        //
        // const float B = 4.0f / M_PI;
        // const float C = -4.0f / (M_PI * M_PI);
        // float y = B * x + C * x * fabs(x);
        //
        // // optional better precision...
        // const float P = 0.225f;
        // y = P * (y * fabs(y) - y) + y;
        //
        //
        // That first thing can be reduced to:
        // const float y = ((1.2732395447351626861510701069801f * x) +
        //             ((-0.40528473456935108577551785283891f * x) * fabs(x)));

        if (need_sin)
        {
            // !!! FIXME: use SRCMOD_ABS here?
            output_line(ctx, "ABS %s.x, %s.x;", dst, src0);
            output_line(ctx, "MUL %s.x, %s.x, -0.40528473456935108577551785283891;", dst, dst);
            output_line(ctx, "MUL %s.x, %s.x, 1.2732395447351626861510701069801;", scratch, src0);
            output_line(ctx, "MAD %s.x, %s.x, %s.x, %s.x;", dst, dst, src0, scratch);
        } // if

        // cosine is sin(x + M_PI/2), but you have to wrap x to pi:
        //  if (x+(M_PI/2) > M_PI)
        //      x -= 2 * M_PI;
        //
        // which is...
        //  if (x+(1.57079637050628662109375) > 3.1415927410125732421875)
        //      x += -6.283185482025146484375;

        if (need_cos)
        {
            output_line(ctx, "ADD %s.x, %s.x, 1.57079637050628662109375;", scratch, src0);
            output_line(ctx, "SGE %s.y, %s.x, 3.1415927410125732421875;", scratch, scratch);
            output_line(ctx, "MAD %s.x, %s.y, -6.283185482025146484375, %s.x;", scratch, scratch, scratch);
            output_line(ctx, "ABS %s.x, %s.x;", dst, src0);
            output_line(ctx, "MUL %s.x, %s.x, -0.40528473456935108577551785283891;", dst, dst);
            output_line(ctx, "MUL %s.x, %s.x, 1.2732395447351626861510701069801;", scratch, src0);
            output_line(ctx, "MAD %s.y, %s.x, %s.x, %s.x;", dst, dst, src0, scratch);
        } // if
    } // else

    // !!! FIXME: might not have done anything. Don't emit if we didn't.
    if (!isfail(ctx))
        emit_ARB1_dest_modifiers(ctx);
} // emit_ARB1_SINCOS


static void emit_ARB1_REP(Context *ctx)
{
    char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));

    // nv2 fragment programs (and everything nv4) have a real REP.
    if ( (support_nv4(ctx)) || ((support_nv2(ctx)) && (shader_is_pixel(ctx))) )
        output_line(ctx, "REP %s;", src0);

    else if (support_nv2(ctx))
    {
        // no REP, but we can use branches.
        char failbranch[32];
        char topbranch[32];
        const int toplabel = allocate_branch_label(ctx);
        const int faillabel = allocate_branch_label(ctx);
        get_ARB1_branch_label_name(ctx,faillabel,failbranch,sizeof(failbranch));
        get_ARB1_branch_label_name(ctx,toplabel,topbranch,sizeof(topbranch));

        assert(((size_t) ctx->branch_labels_stack_index) <
                STATICARRAYLEN(ctx->branch_labels_stack)-1);

        ctx->branch_labels_stack[ctx->branch_labels_stack_index++] = toplabel;
        ctx->branch_labels_stack[ctx->branch_labels_stack_index++] = faillabel;

        char scratch[32];
        snprintf(scratch, sizeof (scratch), "rep%d", ctx->reps);
        output_line(ctx, "MOVC %s.x, %s;", scratch, src0);
        output_line(ctx, "BRA %s (LE.x);", failbranch);
        output_line(ctx, "%s:", topbranch);
    } // else if

    else  // stock ARB1 has no branching.
    {
        fail(ctx, "branching unsupported in this profile");
    } // else
} // emit_ARB1_REP


static void emit_ARB1_ENDREP(Context *ctx)
{
    // nv2 fragment programs (and everything nv4) have a real ENDREP.
    if ( (support_nv4(ctx)) || ((support_nv2(ctx)) && (shader_is_pixel(ctx))) )
        output_line(ctx, "ENDREP;");

    else if (support_nv2(ctx))
    {
        // no ENDREP, but we can use branches.
        assert(ctx->branch_labels_stack_index >= 2);

        char failbranch[32];
        char topbranch[32];
        const int faillabel = ctx->branch_labels_stack[--ctx->branch_labels_stack_index];
        const int toplabel = ctx->branch_labels_stack[--ctx->branch_labels_stack_index];
        get_ARB1_branch_label_name(ctx,faillabel,failbranch,sizeof(failbranch));
        get_ARB1_branch_label_name(ctx,toplabel,topbranch,sizeof(topbranch));

        char scratch[32];
        snprintf(scratch, sizeof (scratch), "rep%d", ctx->reps);
        output_line(ctx, "SUBC %s.x, %s.x, 1.0;", scratch, scratch);
        output_line(ctx, "BRA %s (GT.x);", topbranch);
        output_line(ctx, "%s:", failbranch);
    } // else if

    else  // stock ARB1 has no branching.
    {
        fail(ctx, "branching unsupported in this profile");
    } // else
} // emit_ARB1_ENDREP


static void nv2_if(Context *ctx)
{
    // The condition code register MUST be set up before this!
    // nv2 fragment programs (and everything nv4) have a real IF.
    if ( (support_nv4(ctx)) || (shader_is_pixel(ctx)) )
        output_line(ctx, "IF EQ.x;");
    else
    {
        // there's no IF construct, but we can use a branch to a label.
        char failbranch[32];
        const int label = allocate_branch_label(ctx);
        get_ARB1_branch_label_name(ctx, label, failbranch, sizeof (failbranch));

        assert(((size_t) ctx->branch_labels_stack_index)
                 < STATICARRAYLEN(ctx->branch_labels_stack));

        ctx->branch_labels_stack[ctx->branch_labels_stack_index++] = label;

        // !!! FIXME: should this be NE? (EQ would jump to the ELSE for the IF condition, right?).
        output_line(ctx, "BRA %s (EQ.x);", failbranch);
    } // else
} // nv2_if


static void emit_ARB1_IF(Context *ctx)
{
    if (support_nv2(ctx))
    {
        char buf[64]; allocate_ARB1_scratch_reg_name(ctx, buf, sizeof (buf));
        char src0[64]; get_ARB1_srcarg_varname(ctx, 0, src0, sizeof (src0));
        output_line(ctx, "MOVC %s.x, %s;", buf, src0);
        nv2_if(ctx);
    } // if

    else  // stock ARB1 has no branching.
    {
        failf(ctx, "branching unsupported in %s profile", ctx->profile->name);
    } // else
} // emit_ARB1_IF


static void emit_ARB1_ELSE(Context *ctx)
{
    // nv2 fragment programs (and everything nv4) have a real ELSE.
    if ( (support_nv4(ctx)) || ((support_nv2(ctx)) && (shader_is_pixel(ctx))) )
        output_line(ctx, "ELSE;");

    else if (support_nv2(ctx))
    {
        // there's no ELSE construct, but we can use a branch to a label.
        assert(ctx->branch_labels_stack_index > 0);

        // At the end of the IF block, unconditionally jump to the ENDIF.
        const int endlabel = allocate_branch_label(ctx);
        char endbranch[32];
        get_ARB1_branch_label_name(ctx,endlabel,endbranch,sizeof (endbranch));
        output_line(ctx, "BRA %s;", endbranch);

        // Now mark the ELSE section with a lable.
        const int elselabel = ctx->branch_labels_stack[ctx->branch_labels_stack_index-1];
        char elsebranch[32];
        get_ARB1_branch_label_name(ctx,elselabel,elsebranch,sizeof(elsebranch));
        output_line(ctx, "%s:", elsebranch);

        // Replace the ELSE label with the ENDIF on the label stack.
        ctx->branch_labels_stack[ctx->branch_labels_stack_index-1] = endlabel;
    } // else if

    else  // stock ARB1 has no branching.
    {
        failf(ctx, "branching unsupported in %s profile", ctx->profile->name);
    } // else
} // emit_ARB1_ELSE


static void emit_ARB1_ENDIF(Context *ctx)
{
    // nv2 fragment programs (and everything nv4) have a real ENDIF.
    if ( (support_nv4(ctx)) || ((support_nv2(ctx)) && (shader_is_pixel(ctx))) )
        output_line(ctx, "ENDIF;");

    else if (support_nv2(ctx))
    {
        // there's no ENDIF construct, but we can use a branch to a label.
        assert(ctx->branch_labels_stack_index > 0);
        const int endlabel = ctx->branch_labels_stack[--ctx->branch_labels_stack_index];
        char endbranch[32];
        get_ARB1_branch_label_name(ctx,endlabel,endbranch,sizeof (endbranch));
        output_line(ctx, "%s:", endbranch);
    } // if

    else  // stock ARB1 has no branching.
    {
        failf(ctx, "branching unsupported in %s profile", ctx->profile->name);
    } // else
} // emit_ARB1_ENDIF


static void emit_ARB1_BREAK(Context *ctx)
{
    // nv2 fragment programs (and everything nv4) have a real BREAK.
    if ( (support_nv4(ctx)) || ((support_nv2(ctx)) && (shader_is_pixel(ctx))) )
        output_line(ctx, "BRK;");

    else if (support_nv2(ctx))
    {
        // no BREAK, but we can use branches.
        assert(ctx->branch_labels_stack_index >= 2);
        const int faillabel = ctx->branch_labels_stack[ctx->branch_labels_stack_index];
        char failbranch[32];
        get_ARB1_branch_label_name(ctx,faillabel,failbranch,sizeof(failbranch));
        output_line(ctx, "BRA %s;", failbranch);
    } // else if

    else  // stock ARB1 has no branching.
    {
        failf(ctx, "branching unsupported in %s profile", ctx->profile->name);
    } // else
} // emit_ARB1_BREAK


static void emit_ARB1_MOVA(Context *ctx)
{
    // nv2 and nv3 can use the ARR opcode.
    // But nv4 removed ARR (and ADDRESS registers!). Just ROUND to an INT.
    if (support_nv4(ctx))
        emit_ARB1_opcode_ds(ctx, "ROUND.S");  // !!! FIXME: don't use a modifier here.
    else if ((support_nv2(ctx)) || (support_nv3(ctx)))
        emit_ARB1_opcode_ds(ctx, "ARR");
    else
    {
        char src0[64];
        char scratch[64];
        char addr[32];

        make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
        allocate_ARB1_scratch_reg_name(ctx, scratch, sizeof (scratch));
        snprintf(addr, sizeof (addr), "addr%d", ctx->dest_arg.regnum);

        // !!! FIXME: we can optimize this if src_mod is ABS or ABSNEGATE.

        // ARL uses floor(), but D3D expects round-to-nearest.
        // There is probably a more efficient way to do this.
        if (shader_is_pixel(ctx))  // CMP only exists in fragment programs.  :/
            output_line(ctx, "CMP %s, %s, -1.0, 1.0;", scratch, src0);
        else
        {
            output_line(ctx, "SLT %s, %s, 0.0;", scratch, src0);
            output_line(ctx, "MAD %s, %s, -2.0, 1.0;", scratch, scratch);
        } // else

        output_line(ctx, "ABS %s, %s;", addr, src0);
        output_line(ctx, "ADD %s, %s, 0.5;", addr, addr);
        output_line(ctx, "FLR %s, %s;", addr, addr);
        output_line(ctx, "MUL %s, %s, %s;", addr, addr, scratch);

        // we don't handle these right now, since emit_ARB1_dest_modifiers(ctx)
        //  wants to look at dest_arg, not our temp register.
        assert(ctx->dest_arg.result_mod == 0);
        assert(ctx->dest_arg.result_shift == 0);

        // we assign to the actual address register as needed.
        ctx->last_address_reg_component = -1;
    } // else
} // emit_ARB1_MOVA


static void emit_ARB1_TEXKILL(Context *ctx)
{
    // d3d kills on xyz, arb1 kills on xyzw. Fix the swizzle.
    //  We just map the x component to w. If it's negative, the fragment
    //  would discard anyhow, otherwise, it'll pass through okay. This saves
    //  us a temp register.
    char dst[64];
    get_ARB1_destarg_varname(ctx, dst, sizeof (dst));
    output_line(ctx, "KIL %s.xyzx;", dst);
} // emit_ARB1_TEXKILL

EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXBEM)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXBEML)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXREG2AR)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXREG2GB)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X2PAD)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X2TEX)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X3PAD)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X3TEX)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X3SPEC)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X3VSPEC)

static void emit_ARB1_EXPP(Context *ctx) { emit_ARB1_opcode_ds(ctx, "EX2"); }
static void emit_ARB1_LOGP(Context *ctx) { arb1_log(ctx, "LG2"); }

EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(CND)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXREG2RGB)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXDP3TEX)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X2DEPTH)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXDP3)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXM3X3)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXDEPTH)

static void emit_ARB1_CMP(Context *ctx)
{
    char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
    char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
    char src1[64]; make_ARB1_srcarg_string(ctx, 1, src1, sizeof (src1));
    char src2[64]; make_ARB1_srcarg_string(ctx, 2, src2, sizeof (src2));
    // D3D tests (src0 >= 0.0), but ARB1 tests (src0 < 0.0) ... so just
    //  switch src1 and src2 to get the same results.
    output_line(ctx, "CMP%s, %s, %s, %s;", dst, src0, src2, src1);
    emit_ARB1_dest_modifiers(ctx);
} // emit_ARB1_CMP

EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(BEM)


static void emit_ARB1_DP2ADD(Context *ctx)
{
    if (support_nv4(ctx))  // nv4 has a built-in equivalent to DP2ADD.
        emit_ARB1_opcode_dsss(ctx, "DP2A");
    else
    {
        char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
        char src0[64]; make_ARB1_srcarg_string(ctx, 0, src0, sizeof (src0));
        char src1[64]; make_ARB1_srcarg_string(ctx, 1, src1, sizeof (src1));
        char src2[64]; make_ARB1_srcarg_string(ctx, 2, src2, sizeof (src2));
        char scratch[64];

        // DP2ADD is:
        //  dst = (src0.r * src1.r) + (src0.g * src1.g) + src2.replicate_swiz
        allocate_ARB1_scratch_reg_name(ctx, scratch, sizeof (scratch));
        output_line(ctx, "MUL %s, %s, %s;", scratch, src0, src1);
        output_line(ctx, "ADD %s, %s.x, %s.y;", scratch, scratch, scratch);
        output_line(ctx, "ADD%s, %s.x, %s;", dst, scratch, src2);
        emit_ARB1_dest_modifiers(ctx);
    } // else
} // emit_ARB1_DP2ADD


static void emit_ARB1_DSX(Context *ctx)
{
    if (support_nv2(ctx))  // nv2 has a built-in equivalent to DSX.
        emit_ARB1_opcode_ds(ctx, "DDX");
    else
        failf(ctx, "DSX unsupported in %s profile", ctx->profile->name);
} // emit_ARB1_DSX


static void emit_ARB1_DSY(Context *ctx)
{
    if (support_nv2(ctx))  // nv2 has a built-in equivalent to DSY.
        emit_ARB1_opcode_ds(ctx, "DDY");
    else
        failf(ctx, "DSY unsupported in %s profile", ctx->profile->name);
} // emit_ARB1_DSY

static void arb1_texld(Context *ctx, const char *opcode, const int texldd)
{
    // !!! FIXME: Hack: "TEXH" is invalid in nv4. Fix this more cleanly.
    if ((ctx->dest_arg.result_mod & MOD_PP) && (support_nv4(ctx)))
        ctx->dest_arg.result_mod &= ~MOD_PP;

    // !!! FIXME: do non-RGBA textures map to same default values as D3D?
    char dst[64]; make_ARB1_destarg_string(ctx, dst, sizeof (dst));
    const SourceArgInfo *samp_arg = &ctx->source_args[1];
    RegisterList *sreg = reglist_find(&ctx->samplers, REG_TYPE_SAMPLER,
                                      samp_arg->regnum);
    const char *ttype = NULL;
    char src0[64]; get_ARB1_srcarg_varname(ctx, 0, src0, sizeof (src0));
    //char src1[64]; get_ARB1_srcarg_varname(ctx, 1, src1, sizeof (src1));  // !!! FIXME: SRC_MOD?

    char src2[64] = { 0 };
    char src3[64] = { 0 };

    if (texldd)
    {
        make_ARB1_srcarg_string(ctx, 2, src2, sizeof (src2));
        make_ARB1_srcarg_string(ctx, 3, src3, sizeof (src3));
    } // if

    // !!! FIXME: this should be in state_TEXLD, not in the arb1/glsl emitters.
    if (sreg == NULL)
    {
        fail(ctx, "TEXLD using undeclared sampler");
        return;
    } // if

    if (!no_swizzle(samp_arg->swizzle))
    {
        // !!! FIXME: does this ever actually happen?
        fail(ctx, "BUG: can't handle TEXLD with sampler swizzle at the moment");
    } // if

    switch ((const TextureType) sreg->index)
    {
        case TEXTURE_TYPE_2D: ttype = "2D"; break; // !!! FIXME: "RECT"?
        case TEXTURE_TYPE_CUBE: ttype = "CUBE"; break;
        case TEXTURE_TYPE_VOLUME: ttype = "3D"; break;
        default: fail(ctx, "unknown texture type"); return;
    } // switch

    if (texldd)
    {
        output_line(ctx, "%s%s, %s, %s, %s, texture[%d], %s;", opcode, dst,
                    src0, src2, src3, samp_arg->regnum, ttype);
    } // if
    else
    {
        output_line(ctx, "%s%s, %s, texture[%d], %s;", opcode, dst, src0,
                    samp_arg->regnum, ttype);
    } // else
} // arb1_texld


static void emit_ARB1_TEXLDD(Context *ctx)
{
    // With GL_NV_fragment_program2, we can use the TXD opcode.
    //  In stock arb1, we can settle for a standard texld, which isn't
    //  perfect, but oh well.
    if (support_nv2(ctx))
        arb1_texld(ctx, "TXD", 1);
    else
        arb1_texld(ctx, "TEX", 0);
} // emit_ARB1_TEXLDD


static void emit_ARB1_TEXLDL(Context *ctx)
{
    if ((shader_is_vertex(ctx)) && (!support_nv3(ctx)))
    {
        failf(ctx, "Vertex shader TEXLDL unsupported in %s profile",
              ctx->profile->name);
        return;
    } // if

    else if ((shader_is_pixel(ctx)) && (!support_nv2(ctx)))
    {
        failf(ctx, "Pixel shader TEXLDL unsupported in %s profile",
              ctx->profile->name);
        return;
    } // if

    // !!! FIXME: this doesn't map exactly to TEXLDL. Review this.
    arb1_texld(ctx, "TXL", 0);
} // emit_ARB1_TEXLDL


EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(BREAKP)
EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(BREAKC)

static void emit_ARB1_IFC(Context *ctx)
{
    if (support_nv2(ctx))
    {
        static const char *comps[] = {
            "", "SGTC", "SEQC", "SGEC", "SGTC", "SNEC", "SLEC"
        };

        if (ctx->instruction_controls >= STATICARRAYLEN(comps))
        {
            fail(ctx, "unknown comparison control");
            return;
        } // if

        char src0[64];
        char src1[64];
        char scratch[64];

        const char *comp = comps[ctx->instruction_controls];
        get_ARB1_srcarg_varname(ctx, 0, src0, sizeof (src0));
        get_ARB1_srcarg_varname(ctx, 1, src1, sizeof (src1));
        allocate_ARB1_scratch_reg_name(ctx, scratch, sizeof (scratch));
        output_line(ctx, "%s %s.x, %s, %s;", comp, scratch, src0, src1);
        nv2_if(ctx);
    } // if

    else  // stock ARB1 has no branching.
    {
        failf(ctx, "branching unsupported in %s profile", ctx->profile->name);
    } // else
} // emit_ARB1_IFC


EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(SETP)

static void emit_ARB1_DEF(Context *ctx)
{
    const float *val = (const float *) ctx->dwords; // !!! FIXME: could be int?
    char dst[64]; get_ARB1_destarg_varname(ctx, dst, sizeof (dst));
    char val0[32]; floatstr(ctx, val0, sizeof (val0), val[0], 1);
    char val1[32]; floatstr(ctx, val1, sizeof (val1), val[1], 1);
    char val2[32]; floatstr(ctx, val2, sizeof (val2), val[2], 1);
    char val3[32]; floatstr(ctx, val3, sizeof (val3), val[3], 1);

    push_output(ctx, &ctx->globals);
    output_line(ctx, "PARAM %s = { %s, %s, %s, %s };",
                dst, val0, val1, val2, val3);
    pop_output(ctx);
} // emit_ARB1_DEF

static void emit_ARB1_DEFI(Context *ctx)
{
    char dst[64]; get_ARB1_destarg_varname(ctx, dst, sizeof (dst));
    const int32 *x = (const int32 *) ctx->dwords;
    push_output(ctx, &ctx->globals);
    output_line(ctx, "PARAM %s = { %d, %d, %d, %d };",
                dst, (int) x[0], (int) x[1], (int) x[2], (int) x[3]);
    pop_output(ctx);
} // emit_ARB1_DEFI

static void emit_ARB1_DEFB(Context *ctx)
{
    char dst[64]; get_ARB1_destarg_varname(ctx, dst, sizeof (dst));
    push_output(ctx, &ctx->globals);
    output_line(ctx, "PARAM %s = %d;", dst, ctx->dwords[0] ? 1 : 0);
    pop_output(ctx);
} // emit_ARB1_DEFB

static void emit_ARB1_DCL(Context *ctx)
{
    // no-op. We do this in our emit_attribute() and emit_uniform().
} // emit_ARB1_DCL

EMIT_ARB1_OPCODE_UNIMPLEMENTED_FUNC(TEXCRD)

static void emit_ARB1_TEXLD(Context *ctx)
{
    if (!shader_version_atleast(ctx, 2, 0))
    {
        // ps_1_0 and ps_1_4 are both different, too!
        fail(ctx, "TEXLD <= Shader Model 2.0 unimplemented.");  // !!! FIXME
        return;
    } // if

    // !!! FIXME: do texldb and texldp map between OpenGL and D3D correctly?
    if (ctx->instruction_controls == CONTROL_TEXLD)
        arb1_texld(ctx, "TEX", 0);
    else if (ctx->instruction_controls == CONTROL_TEXLDP)
        arb1_texld(ctx, "TXP", 0);
    else if (ctx->instruction_controls == CONTROL_TEXLDB)
        arb1_texld(ctx, "TXB", 0);
} // emit_ARB1_TEXLD

#endif  // SUPPORT_PROFILE_ARB1


#if !AT_LEAST_ONE_PROFILE
#error No profiles are supported. Fix your build.
#endif

#define DEFINE_PROFILE(prof) { \
    MOJOSHADER_PROFILE_##prof, \
    emit_##prof##_start, \
    emit_##prof##_end, \
    emit_##prof##_phase, \
    emit_##prof##_global, \
    emit_##prof##_array, \
    emit_##prof##_const_array, \
    emit_##prof##_uniform, \
    emit_##prof##_sampler, \
    emit_##prof##_attribute, \
    emit_##prof##_finalize, \
    get_##prof##_varname, \
    get_##prof##_const_array_varname, \
},

static const Profile profiles[] =
{
#if SUPPORT_PROFILE_D3D
    DEFINE_PROFILE(D3D)
#endif
#if SUPPORT_PROFILE_BYTECODE
    DEFINE_PROFILE(BYTECODE)
#endif
#if SUPPORT_PROFILE_GLSL
    DEFINE_PROFILE(GLSL)
#endif
#if SUPPORT_PROFILE_ARB1
    DEFINE_PROFILE(ARB1)
#endif
};

#undef DEFINE_PROFILE

// This is for profiles that extend other profiles...
static const struct { const char *from; const char *to; } profileMap[] =
{
    { MOJOSHADER_PROFILE_GLSL120, MOJOSHADER_PROFILE_GLSL },
    { MOJOSHADER_PROFILE_NV2, MOJOSHADER_PROFILE_ARB1 },
    { MOJOSHADER_PROFILE_NV3, MOJOSHADER_PROFILE_ARB1 },
    { MOJOSHADER_PROFILE_NV4, MOJOSHADER_PROFILE_ARB1 },
};


// The PROFILE_EMITTER_* items MUST be in the same order as profiles[]!
#define PROFILE_EMITTERS(op) { \
     PROFILE_EMITTER_D3D(op) \
     PROFILE_EMITTER_BYTECODE(op) \
     PROFILE_EMITTER_GLSL(op) \
     PROFILE_EMITTER_ARB1(op) \
}

static int parse_destination_token(Context *ctx, DestArgInfo *info)
{
    // !!! FIXME: recheck against the spec for ranges (like RASTOUT values, etc).
    if (ctx->tokencount == 0)
    {
        fail(ctx, "Out of tokens in destination parameter");
        return 0;
    } // if

    const uint32 token = SWAP32(*(ctx->tokens));
    const int reserved1 = (int) ((token >> 14) & 0x3); // bits 14 through 15
    const int reserved2 = (int) ((token >> 31) & 0x1); // bit 31

    info->token = ctx->tokens;
    info->regnum = (int) (token & 0x7ff);  // bits 0 through 10
    info->relative = (int) ((token >> 13) & 0x1); // bit 13
    info->orig_writemask = (int) ((token >> 16) & 0xF); // bits 16 through 19
    info->result_mod = (int) ((token >> 20) & 0xF); // bits 20 through 23
    info->result_shift = (int) ((token >> 24) & 0xF); // bits 24 through 27      abc
    info->regtype = (RegisterType) (((token >> 28) & 0x7) | ((token >> 8) & 0x18));  // bits 28-30, 11-12

    int writemask;
    if (isscalar(ctx, ctx->shader_type, info->regtype, info->regnum))
        writemask = 0x1;  // just x.
    else
        writemask = info->orig_writemask;

    info->writemask = writemask;
    info->writemask0 = (int) ((writemask >> 0) & 0x1); // bit 16
    info->writemask1 = (int) ((writemask >> 1) & 0x1); // bit 17
    info->writemask2 = (int) ((writemask >> 2) & 0x1); // bit 18
    info->writemask3 = (int) ((writemask >> 3) & 0x1); // bit 19

    // all the REG_TYPE_CONSTx types are the same register type, it's just
    //  split up so its regnum can be > 2047 in the bytecode. Clean it up.
    if (info->regtype == REG_TYPE_CONST2)
    {
        info->regtype = REG_TYPE_CONST;
        info->regnum += 2048;
    } // else if
    else if (info->regtype == REG_TYPE_CONST3)
    {
        info->regtype = REG_TYPE_CONST;
        info->regnum += 4096;
    } // else if
    else if (info->regtype == REG_TYPE_CONST4)
    {
        info->regtype = REG_TYPE_CONST;
        info->regnum += 6144;
    } // else if

    // swallow token for now, for multiple calls in a row.
    adjust_token_position(ctx, 1);

    if (reserved1 != 0x0)
        fail(ctx, "Reserved bit #1 in destination token must be zero");

    if (reserved2 != 0x1)
        fail(ctx, "Reserved bit #2 in destination token must be one");

    if (info->relative)
    {
        if (!shader_is_vertex(ctx))
            fail(ctx, "Relative addressing in non-vertex shader");
        if (!shader_version_atleast(ctx, 3, 0))
            fail(ctx, "Relative addressing in vertex shader version < 3.0");
        if ((!ctx->ctab.have_ctab) && (!ctx->ignores_ctab))
        {
            // it's hard to do this efficiently without!
            fail(ctx, "relative addressing unsupported without a CTAB");
        } // if

        // !!! FIXME: I don't have a shader that has a relative dest currently.
        fail(ctx, "Relative addressing of dest tokens is unsupported");
        return 2;
    } // if

    const int s = info->result_shift;
    if (s != 0)
    {
        if (!shader_is_pixel(ctx))
            fail(ctx, "Result shift scale in non-pixel shader");
        if (shader_version_atleast(ctx, 2, 0))
            fail(ctx, "Result shift scale in pixel shader version >= 2.0");
        if ( ! (((s >= 1) && (s <= 3)) || ((s >= 0xD) && (s <= 0xF))) )
            fail(ctx, "Result shift scale isn't 1 to 3, or 13 to 15.");
    } // if

    if (info->result_mod & MOD_PP)  // Partial precision (pixel shaders only)
    {
        if (!shader_is_pixel(ctx))
            fail(ctx, "Partial precision result mod in non-pixel shader");
    } // if

    if (info->result_mod & MOD_CENTROID)  // Centroid (pixel shaders only)
    {
        if (!shader_is_pixel(ctx))
            fail(ctx, "Centroid result mod in non-pixel shader");
        else if (!ctx->centroid_allowed)  // only on DCL opcodes!
            fail(ctx, "Centroid modifier not allowed here");
    } // if

    if ((info->regtype < 0) || (info->regtype > REG_TYPE_MAX))
        fail(ctx, "Register type is out of range");

    if (!isfail(ctx))
        set_used_register(ctx, info->regtype, info->regnum);

    return 1;
} // parse_destination_token


static void determine_constants_arrays(Context *ctx)
{
    // Only process this stuff once. This is called after all DEF* opcodes
    //  could have been parsed.
    if (ctx->determined_constants_arrays)
        return;

    ctx->determined_constants_arrays = 1;

    if (ctx->constant_count <= 1)
        return;  // nothing to sort or group.

    // Sort the linked list into an array for easier tapdancing...
    ConstantsList **array = (ConstantsList **) alloca(sizeof (ConstantsList *) * (ctx->constant_count + 1));
    ConstantsList *item = ctx->constants;
    int i;

    for (i = 0; i < ctx->constant_count; i++)
    {
        if (item == NULL)
        {
            fail(ctx, "BUG: mismatched constant list and count");
            return;
        } // if

        array[i] = item;
        item = item->next;
    } // for

    array[ctx->constant_count] = NULL;

    // bubble sort ftw.
    int sorted;
    do
    {
        sorted = 1;
        for (i = 0; i < ctx->constant_count-1; i++)
        {
            if (array[i]->constant.index > array[i+1]->constant.index)
            {
                ConstantsList *tmp = array[i];
                array[i] = array[i+1];
                array[i+1] = tmp;
                sorted = 0;
            } // if
        } // for
    } while (!sorted);

    // okay, sorted. While we're here, let's redo the linked list in order...
    for (i = 0; i < ctx->constant_count; i++)
        array[i]->next = array[i+1];
    ctx->constants = array[0];

    // now figure out the groupings of constants and add to ctx->variables...
    int start = -1;
    int prev = -1;
    int count = 0;
    const int hi = ctx->constant_count;
    for (i = 0; i <= hi; i++)
    {
        if (array[i] && (array[i]->constant.type != MOJOSHADER_UNIFORM_FLOAT))
            continue;  // we only care about REG_TYPE_CONST for array groups.

        if (start == -1)
        {
            prev = start = i;  // first REG_TYPE_CONST we've seen. Mark it!
            continue;
        } // if

        // not a match (or last item in the array)...see if we had a
        //  contiguous set before this point...
        if ( (array[i]) && (array[i]->constant.index == (array[prev]->constant.index + 1)) )
            count++;
        else
        {
            if (count > 0)  // multiple constants in the set?
            {
                VariableList *var;
                var = (VariableList *) Malloc(ctx, sizeof (VariableList));
                if (var == NULL)
                    break;

                var->type = MOJOSHADER_UNIFORM_FLOAT;
                var->index = array[start]->constant.index;
                var->count = (array[prev]->constant.index - var->index) + 1;
                var->constant = array[start];
                var->used = 0;
                var->emit_position = -1;
                var->next = ctx->variables;
                ctx->variables = var;
            } // else

            start = i;   // set this as new start of sequence.
        } // if

        prev = i;
    } // for
} // determine_constants_arrays


static int adjust_swizzle(const Context *ctx, const RegisterType regtype,
                          const int regnum, const int swizzle)
{
    if (regtype != REG_TYPE_INPUT)  // !!! FIXME: maybe lift this later?
        return swizzle;
    else if (ctx->swizzles_count == 0)
        return swizzle;

    const RegisterList *reg = reglist_find(&ctx->attributes, regtype, regnum);
    if (reg == NULL)
        return swizzle;

    size_t i;
    for (i = 0; i < ctx->swizzles_count; i++)
    {
        const MOJOSHADER_swizzle *swiz = &ctx->swizzles[i];
        if ((swiz->usage == reg->usage) && (swiz->index == reg->index))
        {
            return ( (((int)(swiz->swizzles[((swizzle >> 0) & 0x3)])) << 0) |
                     (((int)(swiz->swizzles[((swizzle >> 2) & 0x3)])) << 2) |
                     (((int)(swiz->swizzles[((swizzle >> 4) & 0x3)])) << 4) |
                     (((int)(swiz->swizzles[((swizzle >> 6) & 0x3)])) << 6) );
        } // if
    } // for

    return swizzle;
} // adjust_swizzle


static int parse_source_token(Context *ctx, SourceArgInfo *info)
{
    int retval = 1;

    if (ctx->tokencount == 0)
    {
        fail(ctx, "Out of tokens in source parameter");
        return 0;
    } // if

    const uint32 token = SWAP32(*(ctx->tokens));
    const int reserved1 = (int) ((token >> 14) & 0x3); // bits 14 through 15
    const int reserved2 = (int) ((token >> 31) & 0x1); // bit 31

    info->token = ctx->tokens;
    info->regnum = (int) (token & 0x7ff);  // bits 0 through 10
    info->relative = (int) ((token >> 13) & 0x1); // bit 13
    const int swizzle = (int) ((token >> 16) & 0xFF); // bits 16 through 23
    info->src_mod = (SourceMod) ((token >> 24) & 0xF); // bits 24 through 27
    info->regtype = (RegisterType) (((token >> 28) & 0x7) | ((token >> 8) & 0x18));  // bits 28-30, 11-12

    // all the REG_TYPE_CONSTx types are the same register type, it's just
    //  split up so its regnum can be > 2047 in the bytecode. Clean it up.
    if (info->regtype == REG_TYPE_CONST2)
    {
        info->regtype = REG_TYPE_CONST;
        info->regnum += 2048;
    } // else if
    else if (info->regtype == REG_TYPE_CONST3)
    {
        info->regtype = REG_TYPE_CONST;
        info->regnum += 4096;
    } // else if
    else if (info->regtype == REG_TYPE_CONST4)
    {
        info->regtype = REG_TYPE_CONST;
        info->regnum += 6144;
    } // else if

    info->swizzle = adjust_swizzle(ctx, info->regtype, info->regnum, swizzle);
    info->swizzle_x = ((info->swizzle >> 0) & 0x3);
    info->swizzle_y = ((info->swizzle >> 2) & 0x3);
    info->swizzle_z = ((info->swizzle >> 4) & 0x3);
    info->swizzle_w = ((info->swizzle >> 6) & 0x3);

    // swallow token for now, for multiple calls in a row.
    adjust_token_position(ctx, 1);

    if (reserved1 != 0x0)
        fail(ctx, "Reserved bits #1 in source token must be zero");

    if (reserved2 != 0x1)
        fail(ctx, "Reserved bit #2 in source token must be one");

    if ((info->relative) && (ctx->tokencount == 0))
    {
        fail(ctx, "Out of tokens in relative source parameter");
        info->relative = 0;  // don't try to process it.
    } // if

    if (info->relative)
    {
        if ( (shader_is_pixel(ctx)) && (!shader_version_atleast(ctx, 3, 0)) )
            fail(ctx, "Relative addressing in pixel shader version < 3.0");

        const uint32 reltoken = SWAP32(*(ctx->tokens));
        // swallow token for now, for multiple calls in a row.
        adjust_token_position(ctx, 1);

        const int relswiz = (int) ((reltoken >> 16) & 0xFF);
        info->relative_regnum = (int) (reltoken & 0x7ff);
        info->relative_regtype = (RegisterType)
                                    (((reltoken >> 28) & 0x7) |
                                    ((reltoken >> 8) & 0x18));

        if (((reltoken >> 31) & 0x1) == 0)
            fail(ctx, "bit #31 in relative address must be set");

        if ((reltoken & 0xF00E000) != 0)  // usused bits.
            fail(ctx, "relative address reserved bit must be zero");

        switch (info->relative_regtype)
        {
            case REG_TYPE_LOOP:
            case REG_TYPE_ADDRESS:
                break;
            default:
                fail(ctx, "invalid register for relative address");
                break;
        } // switch

        if (info->relative_regnum != 0)  // true for now.
            fail(ctx, "invalid register for relative address");

        if (!replicate_swizzle(relswiz))
            fail(ctx, "relative address needs replicate swizzle");

        info->relative_component = (relswiz & 0x3);

        if (info->regtype == REG_TYPE_INPUT)
        {
            if ( (shader_is_pixel(ctx)) || (!shader_version_atleast(ctx, 3, 0)) )
                fail(ctx, "relative addressing of input registers not supported in this shader model");
            ctx->have_relative_input_registers = 1;
        } // if
        else if (info->regtype == REG_TYPE_CONST)
        {
            // figure out what array we're in...
            if (!ctx->ignores_ctab)
            {
                if (!ctx->ctab.have_ctab)  // hard to do efficiently without!
                    fail(ctx, "relative addressing unsupported without a CTAB");
                else
                {
                    determine_constants_arrays(ctx);

                    VariableList *var;
                    const int reltarget = info->regnum;
                    for (var = ctx->variables; var != NULL; var = var->next)
                    {
                        const int lo = var->index;
                        if ( (reltarget >= lo) && (reltarget < (lo + var->count)) )
                            break;  // match!
                    } // for

                    if (var == NULL)
                        fail(ctx, "relative addressing of indeterminate array");
                    else
                    {
                        var->used = 1;
                        info->relative_array = var;
                        set_used_register(ctx, info->relative_regtype, info->relative_regnum);
                    } // else
                } // else
            } // if
        } // else if
        else
        {
            fail(ctx, "relative addressing of invalid register");
        } // else

        retval++;
    } // if

    switch (info->src_mod)
    {
        case SRCMOD_NONE:
        case SRCMOD_ABSNEGATE:
        case SRCMOD_ABS:
        case SRCMOD_NEGATE:
            break; // okay in any shader model.

        // apparently these are only legal in Shader Model 1.x ...
        case SRCMOD_BIASNEGATE:
        case SRCMOD_BIAS:
        case SRCMOD_SIGNNEGATE:
        case SRCMOD_SIGN:
        case SRCMOD_COMPLEMENT:
        case SRCMOD_X2NEGATE:
        case SRCMOD_X2:
        case SRCMOD_DZ:
        case SRCMOD_DW:
            if (shader_version_atleast(ctx, 2, 0))
                fail(ctx, "illegal source mod for this Shader Model.");
            break;

        case SRCMOD_NOT:  // !!! FIXME: I _think_ this is right...
            if (shader_version_atleast(ctx, 2, 0))
            {
                if (info->regtype != REG_TYPE_PREDICATE)
                    fail(ctx, "NOT only allowed on predicate register.");
            } // if
            break;

        default:
            fail(ctx, "Unknown source modifier");
    } // switch

    // !!! FIXME: docs say this for sm3 ... check these!
    //  "The negate modifier cannot be used on second source register of these
    //   instructions: m3x2 - ps, m3x3 - ps, m3x4 - ps, m4x3 - ps, and
    //   m4x4 - ps."
    //  "If any version 3 shader reads from one or more constant float
    //   registers (c#), one of the following must be true.
    //    All of the constant floating-point registers must use the abs modifier.
    //    None of the constant floating-point registers can use the abs modifier.

    if (!isfail(ctx))
        set_used_register(ctx, info->regtype, info->regnum);

    return retval;
} // parse_source_token


static int parse_predicated_token(Context *ctx)
{
    SourceArgInfo *arg = &ctx->predicate_arg;
    parse_source_token(ctx, arg);
    if (arg->regtype != REG_TYPE_PREDICATE)
        fail(ctx, "Predicated instruction but not predicate register!");
    if ((arg->src_mod != SRCMOD_NONE) && (arg->src_mod != SRCMOD_NOT))
        fail(ctx, "Predicated instruction register is not NONE or NOT");
    if ( !no_swizzle(arg->swizzle) && !replicate_swizzle(arg->swizzle) )
        fail(ctx, "Predicated instruction register has wrong swizzle");
    if (arg->relative)  // I'm pretty sure this is illegal...?
        fail(ctx, "relative addressing in predicated token");

    return 1;
} // parse_predicated_token


static int parse_args_NULL(Context *ctx)
{
    return 1;
} // parse_args_NULL


static int parse_args_DEF(Context *ctx)
{
    parse_destination_token(ctx, &ctx->dest_arg);
    if (ctx->dest_arg.regtype != REG_TYPE_CONST)
        fail(ctx, "DEF using non-CONST register");
    if (ctx->dest_arg.relative)  // I'm pretty sure this is illegal...?
        fail(ctx, "relative addressing in DEF");

    ctx->dwords[0] = SWAP32(ctx->tokens[0]);
    ctx->dwords[1] = SWAP32(ctx->tokens[1]);
    ctx->dwords[2] = SWAP32(ctx->tokens[2]);
    ctx->dwords[3] = SWAP32(ctx->tokens[3]);

    return 6;
} // parse_args_DEF


static int parse_args_DEFI(Context *ctx)
{
    parse_destination_token(ctx, &ctx->dest_arg);
    if (ctx->dest_arg.regtype != REG_TYPE_CONSTINT)
        fail(ctx, "DEFI using non-CONSTING register");
    if (ctx->dest_arg.relative)  // I'm pretty sure this is illegal...?
        fail(ctx, "relative addressing in DEFI");

    ctx->dwords[0] = SWAP32(ctx->tokens[0]);
    ctx->dwords[1] = SWAP32(ctx->tokens[1]);
    ctx->dwords[2] = SWAP32(ctx->tokens[2]);
    ctx->dwords[3] = SWAP32(ctx->tokens[3]);

    return 6;
} // parse_args_DEFI


static int parse_args_DEFB(Context *ctx)
{
    parse_destination_token(ctx, &ctx->dest_arg);
    if (ctx->dest_arg.regtype != REG_TYPE_CONSTBOOL)
        fail(ctx, "DEFB using non-CONSTBOOL register");
    if (ctx->dest_arg.relative)  // I'm pretty sure this is illegal...?
        fail(ctx, "relative addressing in DEFB");

    ctx->dwords[0] = *(ctx->tokens) ? 1 : 0;

    return 3;
} // parse_args_DEFB


static int valid_texture_type(const uint32 ttype)
{
    switch ((const TextureType) ttype)
    {
        case TEXTURE_TYPE_2D:
        case TEXTURE_TYPE_CUBE:
        case TEXTURE_TYPE_VOLUME:
            return 1;  // it's okay.
    } // switch

    return 0;
} // valid_texture_type


// !!! FIXME: this function is kind of a mess.
static int parse_args_DCL(Context *ctx)
{
    int unsupported = 0;
    const uint32 token = SWAP32(*(ctx->tokens));
    const int reserved1 = (int) ((token >> 31) & 0x1); // bit 31
    uint32 reserved_mask = 0x00000000;

    if (reserved1 != 0x1)
        fail(ctx, "Bit #31 in DCL token must be one");

    ctx->centroid_allowed = 1;
    adjust_token_position(ctx, 1);
    parse_destination_token(ctx, &ctx->dest_arg);
    ctx->centroid_allowed = 0;

    if (ctx->dest_arg.result_shift != 0)  // I'm pretty sure this is illegal...?
        fail(ctx, "shift scale in DCL");
    if (ctx->dest_arg.relative)  // I'm pretty sure this is illegal...?
        fail(ctx, "relative addressing in DCL");

    const RegisterType regtype = ctx->dest_arg.regtype;
    const int regnum = ctx->dest_arg.regnum;
    if ( (shader_is_pixel(ctx)) && (shader_version_atleast(ctx, 3, 0)) )
    {
        if (regtype == REG_TYPE_INPUT)
        {
            const uint32 usage = (token & 0xF);
            const uint32 index = ((token >> 16) & 0xF);
            reserved_mask = 0x7FF0FFE0;
            ctx->dwords[0] = usage;
            ctx->dwords[1] = index;
        } // if

        else if (regtype == REG_TYPE_MISCTYPE)
        {
            const MiscTypeType mt = (MiscTypeType) regnum;
            if (mt == MISCTYPE_TYPE_POSITION)
                reserved_mask = 0x7FFFFFFF;
            else if (mt == MISCTYPE_TYPE_FACE)
            {
                reserved_mask = 0x7FFFFFFF;
                if (!writemask_xyzw(ctx->dest_arg.orig_writemask))
                    fail(ctx, "DCL face writemask must be full");
                if (ctx->dest_arg.result_mod != 0)
                    fail(ctx, "DCL face result modifier must be zero");
                if (ctx->dest_arg.result_shift != 0)
                    fail(ctx, "DCL face shift scale must be zero");
            } // else if
            else
            {
                unsupported = 1;
            } // else

            ctx->dwords[0] = (uint32) MOJOSHADER_USAGE_UNKNOWN;
            ctx->dwords[1] = 0;
        } // else if

        else if (regtype == REG_TYPE_TEXTURE)
        {
            const uint32 usage = (token & 0xF);
            const uint32 index = ((token >> 16) & 0xF);
            if (usage == MOJOSHADER_USAGE_TEXCOORD)
            {
                if (index > 7)
                    fail(ctx, "DCL texcoord usage must have 0-7 index");
            } // if
            else if (usage == MOJOSHADER_USAGE_COLOR)
            {
                if (index != 0)
                    fail(ctx, "DCL color usage must have 0 index");
            } // else if
            else
            {
                fail(ctx, "Invalid DCL texture usage");
            } // else

            reserved_mask = 0x7FF0FFE0;
            ctx->dwords[0] = usage;
            ctx->dwords[1] = index;
        } // else if

        else if (regtype == REG_TYPE_SAMPLER)
        {
            const uint32 ttype = ((token >> 27) & 0xF);
            if (!valid_texture_type(ttype))
                fail(ctx, "unknown sampler texture type");
            reserved_mask = 0x7FFFFFF;
            ctx->dwords[0] = ttype;
        } // else if

        else
        {
            unsupported = 1;
        } // else
    } // if

    else if ( (shader_is_pixel(ctx)) && (shader_version_atleast(ctx, 2, 0)) )
    {
        if (regtype == REG_TYPE_INPUT)
        {
            ctx->dwords[0] = (uint32) MOJOSHADER_USAGE_COLOR;
            ctx->dwords[1] = regnum;
            reserved_mask = 0x7FFFFFFF;
        } // if
        else if (regtype == REG_TYPE_TEXTURE)
        {
            ctx->dwords[0] = (uint32) MOJOSHADER_USAGE_TEXCOORD;
            ctx->dwords[1] = regnum;
            reserved_mask = 0x7FFFFFFF;
        } // else if
        else if (regtype == REG_TYPE_SAMPLER)
        {
            const uint32 ttype = ((token >> 27) & 0xF);
            if (!valid_texture_type(ttype))
                fail(ctx, "unknown sampler texture type");
            reserved_mask = 0x7FFFFFF;
            ctx->dwords[0] = ttype;
        } // else if
        else
        {
            unsupported = 1;
        } // else
    } // if

    else if ( (shader_is_vertex(ctx)) && (shader_version_atleast(ctx, 3, 0)) )
    {
        if ((regtype == REG_TYPE_INPUT) || (regtype == REG_TYPE_OUTPUT))
        {
            const uint32 usage = (token & 0xF);
            const uint32 index = ((token >> 16) & 0xF);
            reserved_mask = 0x7FF0FFE0;
            ctx->dwords[0] = usage;
            ctx->dwords[1] = index;
        } // if
        else
        {
            unsupported = 1;
        } // else
    } // else if

    else if ( (shader_is_vertex(ctx)) && (shader_version_atleast(ctx, 1, 1)) )
    {
        if (regtype == REG_TYPE_INPUT)
        {
            const uint32 usage = (token & 0xF);
            const uint32 index = ((token >> 16) & 0xF);
            reserved_mask = 0x7FF0FFE0;
            ctx->dwords[0] = usage;
            ctx->dwords[1] = index;
        } // if
        else
        {
            unsupported = 1;
        } // else
    } // else if

    else
    {
        unsupported = 1;
    } // else

    if (unsupported)
        fail(ctx, "invalid DCL register type for this shader model");

    if ((token & reserved_mask) != 0)
        fail(ctx, "reserved bits in DCL dword aren't zero");

    return 3;
} // parse_args_DCL


static int parse_args_D(Context *ctx)
{
    int retval = 1;
    retval += parse_destination_token(ctx, &ctx->dest_arg);
    return retval;
} // parse_args_D


static int parse_args_S(Context *ctx)
{
    int retval = 1;
    retval += parse_source_token(ctx, &ctx->source_args[0]);
    return retval;
} // parse_args_S


static int parse_args_SS(Context *ctx)
{
    int retval = 1;
    retval += parse_source_token(ctx, &ctx->source_args[0]);
    retval += parse_source_token(ctx, &ctx->source_args[1]);
    return retval;
} // parse_args_SS


static int parse_args_DS(Context *ctx)
{
    int retval = 1;
    retval += parse_destination_token(ctx, &ctx->dest_arg);
    retval += parse_source_token(ctx, &ctx->source_args[0]);
    return retval;
} // parse_args_DS


static int parse_args_DSS(Context *ctx)
{
    int retval = 1;
    retval += parse_destination_token(ctx, &ctx->dest_arg);
    retval += parse_source_token(ctx, &ctx->source_args[0]);
    retval += parse_source_token(ctx, &ctx->source_args[1]);
    return retval;
} // parse_args_DSS


static int parse_args_DSSS(Context *ctx)
{
    int retval = 1;
    retval += parse_destination_token(ctx, &ctx->dest_arg);
    retval += parse_source_token(ctx, &ctx->source_args[0]);
    retval += parse_source_token(ctx, &ctx->source_args[1]);
    retval += parse_source_token(ctx, &ctx->source_args[2]);
    return retval;
} // parse_args_DSSS


static int parse_args_DSSSS(Context *ctx)
{
    int retval = 1;
    retval += parse_destination_token(ctx, &ctx->dest_arg);
    retval += parse_source_token(ctx, &ctx->source_args[0]);
    retval += parse_source_token(ctx, &ctx->source_args[1]);
    retval += parse_source_token(ctx, &ctx->source_args[2]);
    retval += parse_source_token(ctx, &ctx->source_args[3]);
    return retval;
} // parse_args_DSSSS


static int parse_args_SINCOS(Context *ctx)
{
    // this opcode needs extra registers for sm2 and lower.
    if (!shader_version_atleast(ctx, 3, 0))
        return parse_args_DSSS(ctx);
    return parse_args_DS(ctx);
} // parse_args_SINCOS


static int parse_args_TEXCRD(Context *ctx)
{
    // added extra register in ps_1_4.
    if (shader_version_atleast(ctx, 1, 4))
        return parse_args_DS(ctx);
    return parse_args_D(ctx);
} // parse_args_TEXCRD


static int parse_args_TEXLD(Context *ctx)
{
    // different registers in px_1_3, ps_1_4, and ps_2_0!
    if (shader_version_atleast(ctx, 2, 0))
        return parse_args_DSS(ctx);
    else if (shader_version_atleast(ctx, 1, 4))
        return parse_args_DS(ctx);
    return parse_args_D(ctx);
} // parse_args_TEXLD


// State machine functions...

static ConstantsList *alloc_constant_listitem(Context *ctx)
{
    ConstantsList *item = (ConstantsList *) Malloc(ctx, sizeof (ConstantsList));
    if (item == NULL)
        return NULL;

    memset(&item->constant, '\0', sizeof (MOJOSHADER_constant));
    item->next = ctx->constants;
    ctx->constants = item;
    ctx->constant_count++;

    return item;
} // alloc_constant_listitem


static void state_DEF(Context *ctx)
{
    const RegisterType regtype = ctx->dest_arg.regtype;
    const int regnum = ctx->dest_arg.regnum;

    // !!! FIXME: fail if same register is defined twice.

    if (ctx->instruction_count != 0)
        fail(ctx, "DEF token must come before any instructions");
    else if (regtype != REG_TYPE_CONST)
        fail(ctx, "DEF token using invalid register");
    else
    {
        ConstantsList *item = alloc_constant_listitem(ctx);
        item->constant.index = regnum;
        item->constant.type = MOJOSHADER_UNIFORM_FLOAT;
        memcpy(item->constant.value.f, ctx->dwords,
               sizeof (item->constant.value.f));
        set_defined_register(ctx, regtype, regnum);
    } // else
} // state_DEF

static void state_DEFI(Context *ctx)
{
    const RegisterType regtype = ctx->dest_arg.regtype;
    const int regnum = ctx->dest_arg.regnum;

    // !!! FIXME: fail if same register is defined twice.

    if (ctx->instruction_count != 0)
        fail(ctx, "DEFI token must come before any instructions");
    else if (regtype != REG_TYPE_CONSTINT)
        fail(ctx, "DEFI token using invalid register");
    else
    {
        ConstantsList *item = alloc_constant_listitem(ctx);
        item->constant.index = regnum;
        item->constant.type = MOJOSHADER_UNIFORM_INT;
        memcpy(item->constant.value.i, ctx->dwords,
               sizeof (item->constant.value.i));

        set_defined_register(ctx, regtype, regnum);
    } // else
} // state_DEFI

static void state_DEFB(Context *ctx)
{
    const RegisterType regtype = ctx->dest_arg.regtype;
    const int regnum = ctx->dest_arg.regnum;

    // !!! FIXME: fail if same register is defined twice.

    if (ctx->instruction_count != 0)
        fail(ctx, "DEFB token must come before any instructions");
    else if (regtype != REG_TYPE_CONSTBOOL)
        fail(ctx, "DEFB token using invalid register");
    else
    {
        ConstantsList *item = alloc_constant_listitem(ctx);
        item->constant.index = regnum;
        item->constant.type = MOJOSHADER_UNIFORM_BOOL;
        item->constant.value.b = ctx->dwords[0] ? 1 : 0;
        set_defined_register(ctx, regtype, regnum);
    } // else
} // state_DEFB

static void state_DCL(Context *ctx)
{
    const DestArgInfo *arg = &ctx->dest_arg;
    const RegisterType regtype = arg->regtype;
    const int regnum = arg->regnum;
    const int wmask = arg->writemask;
    const int mods = arg->result_mod;

    // parse_args_DCL() does a lot of state checking before we get here.

    // !!! FIXME: apparently vs_3_0 can use sampler registers now.
    // !!! FIXME:  (but only s0 through s3, not all 16 of them.)

    if (ctx->instruction_count != 0)
        fail(ctx, "DCL token must come before any instructions");

    else if (shader_is_vertex(ctx))
    {
        const MOJOSHADER_usage usage = (const MOJOSHADER_usage) ctx->dwords[0];
        const int index = ctx->dwords[1];
        if (usage >= MOJOSHADER_USAGE_TOTAL)
        {
            fail(ctx, "unknown DCL usage");
            return;
        } // if
        add_attribute_register(ctx, regtype, regnum, usage, index, wmask, mods);
    } // if

    else if (shader_is_pixel(ctx))
    {
        if (regtype == REG_TYPE_SAMPLER)
            add_sampler(ctx, regtype, regnum, (TextureType) ctx->dwords[0]);
        else
        {
            const MOJOSHADER_usage usage = (MOJOSHADER_usage) ctx->dwords[0];
            const int index = ctx->dwords[1];
            add_attribute_register(ctx, regtype, regnum, usage, index, wmask, mods);
        } // else
    } // else if

    else
    {
        fail(ctx, "unsupported shader type."); // should be caught elsewhere.
        return;
    } // else

    set_defined_register(ctx, regtype, regnum);
} // state_DCL

static void state_TEXCRD(Context *ctx)
{
    if (shader_version_atleast(ctx, 2, 0))
        fail(ctx, "TEXCRD in Shader Model >= 2.0");  // apparently removed.
} // state_TEXCRD

static void state_FRC(Context *ctx)
{
    const DestArgInfo *dst = &ctx->dest_arg;

    if (dst->result_mod & MOD_SATURATE)  // according to msdn...
        fail(ctx, "FRC destination can't use saturate modifier");

    else if (!shader_version_atleast(ctx, 2, 0))
    {
        if (!writemask_y(dst->writemask) && !writemask_xy(dst->writemask))
            fail(ctx, "FRC writemask must be .y or .xy for shader model 1.x");
    } // else if
} // state_FRC


// replicate the matrix registers to source args. The D3D profile will
//  only use the one legitimate argument, but this saves other profiles
//  from having to build this.
static void srcarg_matrix_replicate(Context *ctx, const int idx,
                                       const int rows)
{
    int i;
    SourceArgInfo *src = &ctx->source_args[idx];
    SourceArgInfo *dst = &ctx->source_args[idx+1];
    for (i = 0; i < (rows-1); i++, dst++)
    {
        memcpy(dst, src, sizeof (SourceArgInfo));
        dst->regnum += (i + 1);
        set_used_register(ctx, dst->regtype, dst->regnum);
    } // for
} // srcarg_matrix_replicate

static void state_M4X4(Context *ctx)
{
    const DestArgInfo *info = &ctx->dest_arg;
    if (!writemask_xyzw(info->writemask))
        fail(ctx, "M4X4 writemask must be full");

// !!! FIXME: MSDN:
//The xyzw (default) mask is required for the destination register. Negate and swizzle modifiers are allowed for src0, but not for src1.
//Swizzle and negate modifiers are invalid for the src0 register. The dest and src0 registers cannot be the same.

    srcarg_matrix_replicate(ctx, 1, 4);
} // state_M4X4

static void state_M4X3(Context *ctx)
{
    const DestArgInfo *info = &ctx->dest_arg;
    if (!writemask_xyz(info->writemask))
        fail(ctx, "M4X3 writemask must be .xyz");

// !!! FIXME: MSDN stuff

    srcarg_matrix_replicate(ctx, 1, 3);
} // state_M4X3

static void state_M3X4(Context *ctx)
{
    const DestArgInfo *info = &ctx->dest_arg;
    if (!writemask_xyzw(info->writemask))
        fail(ctx, "M3X4 writemask must be .xyzw");

// !!! FIXME: MSDN stuff

    srcarg_matrix_replicate(ctx, 1, 4);
} // state_M3X4

static void state_M3X3(Context *ctx)
{
    const DestArgInfo *info = &ctx->dest_arg;
    if (!writemask_xyz(info->writemask))
        fail(ctx, "M3X3 writemask must be .xyz");

// !!! FIXME: MSDN stuff

    srcarg_matrix_replicate(ctx, 1, 3);
} // state_M3X3

static void state_M3X2(Context *ctx)
{
    const DestArgInfo *info = &ctx->dest_arg;
    if (!writemask_xy(info->writemask))
        fail(ctx, "M3X2 writemask must be .xy");

// !!! FIXME: MSDN stuff

    srcarg_matrix_replicate(ctx, 1, 2);
} // state_M3X2

static void state_RET(Context *ctx)
{
    // MSDN all but says that assembly shaders are more or less serialized
    //  HLSL functions, and a RET means you're at the end of one, unlike how
    //  most CPUs would behave. This is actually really helpful,
    //  since we can use high-level constructs and not a mess of GOTOs,
    //  which is a godsend for GLSL...this also means we can consider things
    //  like a LOOP without a matching ENDLOOP within a label's section as
    //  an error.
    if (ctx->loops > 0)
        fail(ctx, "LOOP without ENDLOOP");
    if (ctx->reps > 0)
        fail(ctx, "REP without ENDREP");
} // state_RET

static void check_label_register(Context *ctx, int arg, const char *opcode)
{
    const SourceArgInfo *info = &ctx->source_args[arg];
    const RegisterType regtype = info->regtype;
    const int regnum = info->regnum;

    if (regtype != REG_TYPE_LABEL)
        failf(ctx, "%s with a non-label register specified", opcode);
    if (!shader_version_atleast(ctx, 2, 0))
        failf(ctx, "%s not supported in Shader Model 1", opcode);
    if ((shader_version_atleast(ctx, 2, 255)) && (regnum > 2047))
        fail(ctx, "label register number must be <= 2047");
    if (regnum > 15)
        fail(ctx, "label register number must be <= 15");
} // check_label_register

static void state_LABEL(Context *ctx)
{
    if (ctx->previous_opcode != OPCODE_RET)
        fail(ctx, "LABEL not followed by a RET");
    check_label_register(ctx, 0, "LABEL");
    set_defined_register(ctx, REG_TYPE_LABEL, ctx->source_args[0].regnum);
} // state_LABEL

static void check_call_loop_wrappage(Context *ctx, const int regnum)
{
    // msdn says subroutines inherit aL register if you're in a loop when
    //  you call, and further more _if you ever call this function in a loop,
    //  it must always be called in a loop_. So we'll just pass our loop
    //  variable as a function parameter in those cases.

    const int current_usage = (ctx->loops > 0) ? 1 : -1;
    RegisterList *reg = reglist_find(&ctx->used_registers, REG_TYPE_LABEL, regnum);
    assert(reg != NULL);

    if (reg->misc == 0)
        reg->misc = current_usage;
    else if (reg->misc != current_usage)
    {
        if (current_usage == 1)
            fail(ctx, "CALL to this label must be wrapped in LOOP/ENDLOOP");
        else
            fail(ctx, "CALL to this label must not be wrapped in LOOP/ENDLOOP");
    } // else if
} // check_call_loop_wrappage

static void state_CALL(Context *ctx)
{
    check_label_register(ctx, 0, "CALL");
    check_call_loop_wrappage(ctx, ctx->source_args[0].regnum);
} // state_CALL

static void state_CALLNZ(Context *ctx)
{
    const RegisterType regtype = ctx->source_args[1].regtype;
    if ((regtype != REG_TYPE_CONSTBOOL) && (regtype != REG_TYPE_PREDICATE))
        fail(ctx, "CALLNZ argument isn't constbool or predicate register");
    check_label_register(ctx, 0, "CALLNZ");
    check_call_loop_wrappage(ctx, ctx->source_args[0].regnum);
} // state_CALLNZ

static void state_MOVA(Context *ctx)
{
    if (ctx->dest_arg.regtype != REG_TYPE_ADDRESS)
        fail(ctx, "MOVA argument isn't address register");
} // state_MOVA

static void state_RCP(Context *ctx)
{
    if (!replicate_swizzle(ctx->source_args[0].swizzle))
        fail(ctx, "RCP without replicate swizzzle");
} // state_RCP

static void state_LOOP(Context *ctx)
{
    if (ctx->source_args[0].regtype != REG_TYPE_LOOP)
        fail(ctx, "LOOP argument isn't loop register");
    else if (ctx->source_args[1].regtype != REG_TYPE_CONSTINT)
        fail(ctx, "LOOP argument isn't constint register");
    else
        ctx->loops++;
} // state_LOOP

static void state_ENDLOOP(Context *ctx)
{
    // !!! FIXME: check that we aren't straddling an IF block.
    if (ctx->loops <= 0)
        fail(ctx, "ENDLOOP without LOOP");
    ctx->loops--;
} // state_ENDLOOP

static void state_BREAKP(Context *ctx)
{
    const RegisterType regtype = ctx->source_args[0].regtype;
    if (regtype != REG_TYPE_PREDICATE)
        fail(ctx, "BREAKP argument isn't predicate register");
    else if (!replicate_swizzle(ctx->source_args[0].swizzle))
        fail(ctx, "BREAKP without replicate swizzzle");
    else if ((ctx->loops == 0) && (ctx->reps == 0))
        fail(ctx, "BREAKP outside LOOP/ENDLOOP or REP/ENDREP");
} // state_BREAKP

static void state_BREAK(Context *ctx)
{
    if ((ctx->loops == 0) && (ctx->reps == 0))
        fail(ctx, "BREAK outside LOOP/ENDLOOP or REP/ENDREP");
} // state_BREAK

static void state_SETP(Context *ctx)
{
    const RegisterType regtype = ctx->dest_arg.regtype;
    if (regtype != REG_TYPE_PREDICATE)
        fail(ctx, "SETP argument isn't predicate register");
} // state_SETP

static void state_REP(Context *ctx)
{
    const RegisterType regtype = ctx->source_args[0].regtype;
    if (regtype != REG_TYPE_CONSTINT)
        fail(ctx, "REP argument isn't constint register");

    ctx->reps++;
    if (ctx->reps > ctx->max_reps)
        ctx->max_reps = ctx->reps;
} // state_REP

static void state_ENDREP(Context *ctx)
{
    // !!! FIXME: check that we aren't straddling an IF block.
    if (ctx->reps <= 0)
        fail(ctx, "ENDREP without REP");
    ctx->reps--;
} // state_ENDREP

static void state_CMP(Context *ctx)
{
    ctx->cmps++;

    // extra limitations for ps <= 1.4 ...
    if (!shader_version_atleast(ctx, 1, 4))
    {
        int i;
        const DestArgInfo *dst = &ctx->dest_arg;
        const RegisterType dregtype = dst->regtype;
        const int dregnum = dst->regnum;

        if (ctx->cmps > 3)
            fail(ctx, "only 3 CMP instructions allowed in this shader model");

        for (i = 0; i < 3; i++)
        {
            const SourceArgInfo *src = &ctx->source_args[i];
            const RegisterType sregtype = src->regtype;
            const int sregnum = src->regnum;
            if ((dregtype == sregtype) && (dregnum == sregnum))
                fail(ctx, "CMP dest can't match sources in this shader model");
        } // for

        ctx->instruction_count++;  // takes an extra slot in ps_1_2 and _3.
    } // if
} // state_CMP

static void state_DP4(Context *ctx)
{
    // extra limitations for ps <= 1.4 ...
    if (!shader_version_atleast(ctx, 1, 4))
        ctx->instruction_count++;  // takes an extra slot in ps_1_2 and _3.
} // state_DP4

static void state_CND(Context *ctx)
{
    // apparently it was removed...it's not in the docs past ps_1_4 ...
    if (shader_version_atleast(ctx, 2, 0))
        fail(ctx, "CND not allowed in this shader model");

    // extra limitations for ps <= 1.4 ...
    else if (!shader_version_atleast(ctx, 1, 4))
    {
        const SourceArgInfo *src = &ctx->source_args[0];
        if ((src->regtype != REG_TYPE_TEMP) || (src->regnum != 0) ||
            (src->swizzle != 0x0000))
        {
            fail(ctx, "CND src must be r0.a in this shader model");
        } // if
    } // if
} // state_CND

static void state_POW(Context *ctx)
{
    if (!replicate_swizzle(ctx->source_args[0].swizzle))
        fail(ctx, "POW src0 must have replicate swizzle");
    else if (!replicate_swizzle(ctx->source_args[1].swizzle))
        fail(ctx, "POW src1 must have replicate swizzle");
} // state_POW

static void state_LOG(Context *ctx)
{
    if (!replicate_swizzle(ctx->source_args[0].swizzle))
        fail(ctx, "LOG src0 must have replicate swizzle");
} // state_LOG

static void state_LOGP(Context *ctx)
{
    if (!replicate_swizzle(ctx->source_args[0].swizzle))
        fail(ctx, "LOGP src0 must have replicate swizzle");
} // state_LOGP

static void state_SINCOS(Context *ctx)
{
    const DestArgInfo *dst = &ctx->dest_arg;
    const int mask = dst->writemask;
    if (!writemask_x(mask) && !writemask_y(mask) && !writemask_xy(mask))
        fail(ctx, "SINCOS write mask must be .x or .y or .xy");

    else if (!replicate_swizzle(ctx->source_args[0].swizzle))
        fail(ctx, "SINCOS src0 must have replicate swizzle");

    else if (dst->result_mod & MOD_SATURATE)  // according to msdn...
        fail(ctx, "SINCOS destination can't use saturate modifier");

    // this opcode needs extra registers, with extra limitations, for <= sm2.
    else if (!shader_version_atleast(ctx, 3, 0))
    {
        int i;
        for (i = 1; i < 3; i++)
        {
            if (ctx->source_args[i].regtype != REG_TYPE_CONST)
            {
                failf(ctx, "SINCOS src%d must be constfloat", i);
                return;
            } // if
        } // for

        if (ctx->source_args[1].regnum == ctx->source_args[2].regnum)
            fail(ctx, "SINCOS src1 and src2 must be different registers");
    } // if
} // state_SINCOS

static void state_IF(Context *ctx)
{
    const RegisterType regtype = ctx->source_args[0].regtype;
    if ((regtype != REG_TYPE_PREDICATE) && (regtype != REG_TYPE_CONSTBOOL))
        fail(ctx, "IF src0 must be CONSTBOOL or PREDICATE");
    else if (!replicate_swizzle(ctx->source_args[0].swizzle))
        fail(ctx, "IF src0 must have replicate swizzle");
    // !!! FIXME: track if nesting depth.
} // state_IF

static void state_IFC(Context *ctx)
{
    if (!replicate_swizzle(ctx->source_args[0].swizzle))
        fail(ctx, "IFC src0 must have replicate swizzle");
    else if (!replicate_swizzle(ctx->source_args[1].swizzle))
        fail(ctx, "IFC src1 must have replicate swizzle");
    // !!! FIXME: track if nesting depth.
} // state_IFC

static void state_BREAKC(Context *ctx)
{
    if (!replicate_swizzle(ctx->source_args[0].swizzle))
        fail(ctx, "BREAKC src1 must have replicate swizzle");
    else if (!replicate_swizzle(ctx->source_args[1].swizzle))
        fail(ctx, "BREAKC src2 must have replicate swizzle");
    else if ((ctx->loops == 0) && (ctx->reps == 0))
        fail(ctx, "BREAKC outside LOOP/ENDLOOP or REP/ENDREP");
} // state_BREAKC

static void state_TEXKILL(Context *ctx)
{
    // The MSDN docs say this should be a source arg, but the driver docs
    //  say it's a dest arg. That's annoying.
    const DestArgInfo *info = &ctx->dest_arg;
    const RegisterType regtype = info->regtype;
    if (!writemask_xyzw(info->writemask))
        fail(ctx, "TEXKILL writemask must be .xyzw");
    else if ((regtype != REG_TYPE_TEMP) && (regtype != REG_TYPE_TEXTURE))
        fail(ctx, "TEXKILL must use a temp or texture register");

    // !!! FIXME: "If a temporary register is used, all components must have been previously written."
    // !!! FIXME: "If a texture register is used, all components that are read must have been declared."
    // !!! FIXME: there are further limitations in ps_1_3 and earlier.
} // state_TEXKILL

static void state_TEXLD(Context *ctx)
{
    if (shader_version_atleast(ctx, 2, 0))
    {
        const SourceArgInfo *src0 = &ctx->source_args[0];
        const SourceArgInfo *src1 = &ctx->source_args[1];

        // !!! FIXME: verify texldp restrictions:
        //http://msdn.microsoft.com/en-us/library/bb206221(VS.85).aspx
        // !!! FIXME: ...and texldb, too.
        //http://msdn.microsoft.com/en-us/library/bb206217(VS.85).aspx

        //const RegisterType rt0 = src0->regtype;

        // !!! FIXME: msdn says it has to be temp, but Microsoft's HLSL
        // !!! FIXME:  compiler is generating code that uses oC0 for a dest.
        //if (ctx->dest_arg.regtype != REG_TYPE_TEMP)
        //    fail(ctx, "TEXLD dest must be a temp register");

        // !!! FIXME: this can be an REG_TYPE_INPUT, DCL'd to TEXCOORD.
        //else if ((rt0 != REG_TYPE_TEXTURE) && (rt0 != REG_TYPE_TEMP))
        //    fail(ctx, "TEXLD src0 must be texture or temp register");
        //else

        if (src0->src_mod != SRCMOD_NONE)
            fail(ctx, "TEXLD src0 must have no modifiers");
        else if (src1->regtype != REG_TYPE_SAMPLER)
            fail(ctx, "TEXLD src1 must be sampler register");
        else if (src1->src_mod != SRCMOD_NONE)
            fail(ctx, "TEXLD src0 must have no modifiers");
        else if ( (ctx->instruction_controls != CONTROL_TEXLD) &&
                  (ctx->instruction_controls != CONTROL_TEXLDP) &&
                  (ctx->instruction_controls != CONTROL_TEXLDB) )
        {
            fail(ctx, "TEXLD has unknown control bits");
        } // else if

        // Shader Model 3 added swizzle support to this opcode.
        if (!shader_version_atleast(ctx, 3, 0))
        {
            if (!no_swizzle(src0->swizzle))
                fail(ctx, "TEXLD src0 must not swizzle");
            else if (!no_swizzle(src1->swizzle))
                fail(ctx, "TEXLD src1 must not swizzle");
        } // if

        if ( ((TextureType) ctx->source_args[1].regnum) == TEXTURE_TYPE_CUBE )
            ctx->instruction_count += 3;
    } // if

    // !!! FIXME: checks for ps_1_4 and ps_1_0 versions here...
} // state_TEXLD

static void state_TEXLDL(Context *ctx)
{
    if (!shader_version_atleast(ctx, 3, 0))
        fail(ctx, "TEXLDL in version < Shader Model 3.0");
    else if (ctx->source_args[1].regtype != REG_TYPE_SAMPLER)
        fail(ctx, "TEXLDL src1 must be sampler register");
    else
    {
        if ( ((TextureType) ctx->source_args[1].regnum) == TEXTURE_TYPE_CUBE )
            ctx->instruction_count += 3;
    } // else
} // state_TEXLDL

static void state_DP2ADD(Context *ctx)
{
    if (!replicate_swizzle(ctx->source_args[2].swizzle))
        fail(ctx, "DP2ADD src2 must have replicate swizzle");
} // state_DP2ADD


// Lookup table for instruction opcodes...
typedef struct
{
    const char *opcode_string;
    int slots;  // number of instruction slots this opcode eats.
    MOJOSHADER_shaderType shader_types;  // mask of types that can use opcode.
    args_function parse_args;
    state_function state;
    emit_function emitter[STATICARRAYLEN(profiles)];
} Instruction;

// These have to be in the right order! This array is indexed by the value
//  of the instruction token.
static const Instruction instructions[] =
{
    #define INSTRUCTION_STATE(op, opstr, slots, a, t) { \
        opstr, slots, t, parse_args_##a, state_##op, PROFILE_EMITTERS(op) \
    },

    #define INSTRUCTION(op, opstr, slots, a, t) { \
        opstr, slots, t, parse_args_##a, 0, PROFILE_EMITTERS(op) \
    },

    #define MOJOSHADER_DO_INSTRUCTION_TABLE 1
    #include "mojoshader_internal.h"
    #undef MOJOSHADER_DO_INSTRUCTION_TABLE

    #undef INSTRUCTION
    #undef INSTRUCTION_STATE
};


// parse various token types...

static int parse_instruction_token(Context *ctx)
{
    int retval = 0;
    const int start_position = ctx->current_position;
    const uint32 *start_tokens = ctx->tokens;
    const uint32 start_tokencount = ctx->tokencount;
    const uint32 token = SWAP32(*(ctx->tokens));
    const uint32 opcode = (token & 0xFFFF);
    const uint32 controls = ((token >> 16) & 0xFF);
    const uint32 insttoks = ((token >> 24) & 0x0F);
    const int coissue = (token & 0x40000000) ? 1 : 0;
    const int predicated = (token & 0x10000000) ? 1 : 0;

    if ( opcode >= (sizeof (instructions) / sizeof (instructions[0])) )
        return 0;  // not an instruction token, or just not handled here.

    const Instruction *instruction = &instructions[opcode];
    const emit_function emitter = instruction->emitter[ctx->profileid];

    if ((token & 0x80000000) != 0)
        fail(ctx, "instruction token high bit must be zero.");  // so says msdn.

    if (instruction->opcode_string == NULL)
    {
        fail(ctx, "Unknown opcode.");
        return insttoks + 1;  // pray that you resync later.
    } // if

    if (coissue)
    {
        if (!shader_is_pixel(ctx))
            fail(ctx, "coissue instruction on non-pixel shader");
        if (shader_version_atleast(ctx, 2, 0))
            fail(ctx, "coissue instruction in Shader Model >= 2.0");
        // !!! FIXME: I'm not sure what this actually means, yet.
        fail(ctx, "coissue instructions unsupported");
    } // if

    if ((ctx->shader_type & instruction->shader_types) == 0)
    {
        failf(ctx, "opcode '%s' not available in this shader type.",
                instruction->opcode_string);
    } // if

    memset(ctx->dwords, '\0', sizeof (ctx->dwords));
    ctx->instruction_controls = controls;
    ctx->predicated = predicated;

    // Update the context with instruction's arguments.
    adjust_token_position(ctx, 1);
    retval = instruction->parse_args(ctx);

    if (predicated)
        retval += parse_predicated_token(ctx);

    // parse_args() moves these forward for convenience...reset them.
    ctx->tokens = start_tokens;
    ctx->tokencount = start_tokencount;
    ctx->current_position = start_position;

    if (instruction->state != NULL)
        instruction->state(ctx);

    ctx->instruction_count += instruction->slots;

    if (!isfail(ctx))
        emitter(ctx);  // call the profile's emitter.

    ctx->previous_opcode = opcode;
    ctx->scratch_registers = 0;  // reset after every instruction.

    if (!shader_version_atleast(ctx, 2, 0))
    {
        if (insttoks != 0)  // reserved field in shaders < 2.0 ...
            fail(ctx, "instruction token count must be zero");
    } // if
    else
    {
        if (((uint32)retval) != (insttoks+1))
        {
            failf(ctx, "wrong token count (%u, not %u) for opcode '%s'.",
                    (uint) retval, (uint) (insttoks+1),
                    instruction->opcode_string);
            retval = insttoks + 1;  // try to keep sync.
        } // if
    } // else

    return retval;
} // parse_instruction_token


static int parse_version_token(Context *ctx, const char *profilestr)
{
    if (ctx->tokencount == 0)
    {
        fail(ctx, "Expected version token, got none at all.");
        return 0;
    } // if

    const uint32 token = SWAP32(*(ctx->tokens));
    const uint32 shadertype = ((token >> 16) & 0xFFFF);
    const uint8 major = (uint8) ((token >> 8) & 0xFF);
    const uint8 minor = (uint8) (token & 0xFF);

    ctx->version_token = token;

    // 0xFFFF == pixel shader, 0xFFFE == vertex shader
    if (shadertype == 0xFFFF)
    {
        ctx->shader_type = MOJOSHADER_TYPE_PIXEL;
        ctx->shader_type_str = "ps";
    } // if
    else if (shadertype == 0xFFFE)
    {
        ctx->shader_type = MOJOSHADER_TYPE_VERTEX;
        ctx->shader_type_str = "vs";
    } // else if
    else  // geometry shader? Bogus data?
    {
        fail(ctx, "Unsupported shader type or not a shader at all");
        return -1;
    } // else

    ctx->major_ver = major;
    ctx->minor_ver = minor;

    if (!shader_version_supported(major, minor))
    {
        failf(ctx, "Shader Model %u.%u is currently unsupported.",
                (uint) major, (uint) minor);
    } // if

    if (!isfail(ctx))
        ctx->profile->start_emitter(ctx, profilestr);

    return 1;  // ate one token.
} // parse_version_token


static int parse_ctab_string(const uint8 *start, const uint32 bytes,
                             const uint32 name)
{
    // Make sure strings don't overflow the CTAB buffer...
    if (name < bytes)
    {
        int i;
        const int slenmax = bytes - name;
        const char *namestr = (const char *) (start + name);
        for (i = 0; i < slenmax; i++)
        {
            if (namestr[i] == '\0')
                return 1;  // it's okay.
        } // for
    } // if

    return 0;  // overflowed.
} // parse_ctab_string


static int parse_ctab_typeinfo(Context *ctx, const uint8 *start,
                               const uint32 bytes, const uint32 pos,
                               MOJOSHADER_symbolTypeInfo *info)
{
    if ((pos + 16) >= bytes)
        return 0;  // corrupt CTAB.

    const uint16 *typeptr = (const uint16 *) (start + pos);

    info->parameter_class = (MOJOSHADER_symbolClass) SWAP16(typeptr[0]);
    info->parameter_type = (MOJOSHADER_symbolType) SWAP16(typeptr[1]);
    info->rows = (unsigned int) SWAP16(typeptr[2]);
    info->columns = (unsigned int) SWAP16(typeptr[3]);
    info->elements = (unsigned int) SWAP16(typeptr[4]);
    info->member_count = (unsigned int) SWAP16(typeptr[5]);

    if ((pos + 16 + (info->member_count * 8)) >= bytes)
        return 0;  // corrupt CTAB.

    if (info->member_count == 0)
        info->members = NULL;
    else
    {
        const size_t len = sizeof (MOJOSHADER_symbolStructMember) *
                            info->member_count;
        info->members = (MOJOSHADER_symbolStructMember *) Malloc(ctx, len);
        if (info->members == NULL)
            return 1;  // we'll check ctx->out_of_memory later.
        memset(info->members, '\0', len);
    } // else

    int i;
    const uint32 *member = (const uint32 *)((const uint8 *) (&typeptr[6]));
    for (i = 0; i < info->member_count; i++)
    {
        MOJOSHADER_symbolStructMember *mbr = &info->members[i];
        const uint32 name = SWAP32(member[0]);
        const uint32 memberinfopos = SWAP32(member[1]);
        member += 2;

        if (!parse_ctab_string(start, bytes, name))
            return 0;  // info->members will be free()'d elsewhere.

        mbr->name = StrDup(ctx, (const char *) (start + name));
        if (mbr->name == NULL)
            return 1;  // we'll check ctx->out_of_memory later.
        if (!parse_ctab_typeinfo(ctx, start, bytes, memberinfopos, &mbr->info))
            return 0;
        if (ctx->out_of_memory)
            return 1;  // drop out now.
    } // for

    return 1;
} // parse_ctab_typeinfo


// Microsoft's tools add a CTAB comment to all shaders. This is the
//  "constant table," or specifically: D3DXSHADER_CONSTANTTABLE:
//  http://msdn.microsoft.com/en-us/library/bb205440(VS.85).aspx
// This may tell us high-level truths about an otherwise generic low-level
//  registers, for instance, how large an array actually is, etc.
static void parse_constant_table(Context *ctx, const uint32 *tokens,
                                 const uint32 bytes, const uint32 okay_version,
                                 const int setvariables, CtabData *ctab)
{
    const uint32 id = SWAP32(tokens[1]);
    if (id != CTAB_ID)
        return;  // not the constant table.

    assert(ctab->have_ctab == 0);  // !!! FIXME: can you have more than one?
    ctab->have_ctab = 1;

    const uint8 *start = (uint8 *) &tokens[2];

    if (bytes < 32)
    {
        fail(ctx, "Truncated CTAB data");
        return;
    } // if

    const uint32 size = SWAP32(tokens[2]);
    const uint32 creator = SWAP32(tokens[3]);
    const uint32 version = SWAP32(tokens[4]);
    const uint32 constants = SWAP32(tokens[5]);
    const uint32 constantinfo = SWAP32(tokens[6]);
    const uint32 target = SWAP32(tokens[8]);

    if (size != CTAB_SIZE)
        goto corrupt_ctab;

    //if (version != okay_version) goto corrupt_ctab;
    if (creator >= bytes) goto corrupt_ctab;
    if ((constantinfo + (constants * CINFO_SIZE)) >= bytes) goto corrupt_ctab;
    if (target >= bytes) goto corrupt_ctab;
    if (!parse_ctab_string(start, bytes, target)) goto corrupt_ctab;
    // !!! FIXME: check that (start+target) points to "ps_3_0", etc.

    ctab->symbol_count = constants;
    ctab->symbols = Malloc(ctx, sizeof (MOJOSHADER_symbol) * constants);
    if (ctab->symbols == NULL)
        return;
    memset(ctab->symbols, '\0', sizeof (MOJOSHADER_symbol) * constants);

    uint32 i = 0;
    for (i = 0; i < constants; i++)
    {
        const uint8 *ptr = start + constantinfo + (i * CINFO_SIZE);
        const uint32 name = SWAP32(*((uint32 *) (ptr + 0)));
        const uint16 regset = SWAP16(*((uint16 *) (ptr + 4)));
        const uint16 regidx = SWAP16(*((uint16 *) (ptr + 6)));
        const uint16 regcnt = SWAP16(*((uint16 *) (ptr + 8)));
        const uint32 typeinf = SWAP32(*((uint32 *) (ptr + 12)));
        const uint32 defval = SWAP32(*((uint32 *) (ptr + 16)));
        MOJOSHADER_uniformType mojotype = MOJOSHADER_UNIFORM_UNKNOWN;

        if (!parse_ctab_string(start, bytes, name)) goto corrupt_ctab;
        if (defval >= bytes) goto corrupt_ctab;

        switch (regset)
        {
            case 0: mojotype = MOJOSHADER_UNIFORM_BOOL; break;
            case 1: mojotype = MOJOSHADER_UNIFORM_INT; break;
            case 2: mojotype = MOJOSHADER_UNIFORM_FLOAT; break;
            case 3: /* SAMPLER */ break;
            default: goto corrupt_ctab;
        } // switch

        if ((setvariables) && (mojotype != MOJOSHADER_UNIFORM_UNKNOWN))
        {
            VariableList *item;
            item = (VariableList *) Malloc(ctx, sizeof (VariableList));
            if (item != NULL)
            {
                item->type = mojotype;
                item->index = regidx;
                item->count = regcnt;
                item->constant = NULL;
                item->used = 0;
                item->emit_position = -1;
                item->next = ctx->variables;
                ctx->variables = item;
            } // if
        } // if

        // Add the symbol.
        const char *namecpy = StrDup(ctx, (const char *) (start + name));
        if (namecpy == NULL)
            return;

        MOJOSHADER_symbol *sym = &ctab->symbols[i];
        sym->name = namecpy;
        sym->register_set = (MOJOSHADER_symbolRegisterSet) regset;
        sym->register_index = (unsigned int) regidx;
        sym->register_count = (unsigned int) regcnt;
        if (!parse_ctab_typeinfo(ctx, start, bytes, typeinf, &sym->info))
            goto corrupt_ctab;  // sym->name will get free()'d later.
        else if (ctx->out_of_memory)
            return;  // just bail now.
    } // for

    return;

corrupt_ctab:
    fail(ctx, "Shader has corrupt CTAB data");
} // parse_constant_table


static void free_symbols(MOJOSHADER_free f, void *d, MOJOSHADER_symbol *syms,
                         const int symcount);


static int is_comment_token(Context *ctx, const uint32 tok, uint32 *tokcount)
{
    const uint32 token = SWAP32(tok);
    if ((token & 0xFFFF) == 0xFFFE)  // actually a comment token?
    {
        if ((token & 0x80000000) != 0)
            fail(ctx, "comment token high bit must be zero.");  // so says msdn.
        *tokcount = ((token >> 16) & 0xFFFF);
        return 1;
    } // if

    return 0;
} // is_comment_token


typedef struct PreshaderBlockInfo
{
    const uint32 *tokens;
    uint32 tokcount;
    int seen;
} PreshaderBlockInfo;

// Preshaders only show up in compiled Effect files. The format is
//  undocumented, and even the instructions aren't the same opcodes as you
//  would find in a regular shader. These things show up because the HLSL
//  compiler can detect work that sets up constant registers that could
//  be moved out of the shader itself. Preshaders run once, then the shader
//  itself runs many times, using the constant registers the preshader has set
//  up. There are cases where the preshaders are 3+ times as many instructions
//  as the shader itself, so this can be a big performance win.
// My presumption is that Microsoft's Effects framework runs the preshaders on
//  the CPU, then loads the constant register file appropriately before handing
//  off to the GPU. As such, we do the same.
static void parse_preshader(Context *ctx, uint32 tokcount)
{
    const uint32 *tokens = ctx->tokens;
    
    if (SWAP32(tokens[1]) == PRES_ID) {
        if (tokcount <= 2) return;
        //hax
        tokens += 2;
        tokcount -= 2;
    }
    
    assert(ctx->have_preshader == 0);  // !!! FIXME: can you have more than one?
    ctx->have_preshader = 1;

    // !!! FIXME: I don't know what specific versions signify, but we need to
    // !!! FIXME:  save this to test against the CTAB version field, if
    // !!! FIXME:  nothing else.
    // !!! FIXME: 0x02 0x01 is probably the version (fx_2_1),
    // !!! FIXME:  and 0x4658 is the magic, like a real shader's version token.
    const uint32 okay_versions[] = {0x46580201, 0x46580200};
    if (SWAP32(tokens[0]) != okay_versions[0] && SWAP32(tokens[0]) != okay_versions[1])
    {
        failf(ctx, "Unsupported preshader version %6x.", tokens[0]);
        return;  // fail because the shader will malfunction w/o this.
    } // if

    tokens += 1;
    tokcount -= 1;

    // All sections of a preshader are packed into separate comment tokens,
    //  inside the containing comment token block. Find them all before
    //  we start, so we don't care about the order they appear in the file.
    PreshaderBlockInfo ctab = { 0, 0, 0 };
    PreshaderBlockInfo prsi = { 0, 0, 0 };
    PreshaderBlockInfo fxlc = { 0, 0, 0 };
    PreshaderBlockInfo clit = { 0, 0, 0 };

    while (tokcount > 0)
    {
        uint32 subtokcount = 0;
        if ( (!is_comment_token(ctx, *tokens, &subtokcount)) ||
             (subtokcount > tokcount) )
        {
            fail(ctx, "Bogus preshader data.");
            return;
        } // if

        tokens++;
        tokcount--;

        const uint32 *nexttokens = tokens + subtokcount;
        const uint32 nexttokcount = tokcount - subtokcount;

        if (subtokcount > 0)
        {
            switch (SWAP32(*tokens))
            {
                #define PRESHADER_BLOCK_CASE(id, var) \
                    case id##_ID: { \
                        if (var.seen) { \
                            fail(ctx, "Multiple " #id " preshader blocks."); \
                            return; \
                        } \
                        var.tokens = tokens; \
                        var.tokcount = subtokcount; \
                        var.seen = 1; \
                        break; \
                    }
                PRESHADER_BLOCK_CASE(CTAB, ctab);
                PRESHADER_BLOCK_CASE(PRSI, prsi);
                PRESHADER_BLOCK_CASE(FXLC, fxlc);
                PRESHADER_BLOCK_CASE(CLIT, clit);
                default: fail(ctx, "Bogus preshader section."); return;
                #undef PRESHADER_BLOCK_CASE
            } // switch
        } // if

        tokens = nexttokens;
        tokcount = nexttokcount;
    } // while

    if (!ctab.seen) { fail(ctx, "No CTAB block in preshader."); return; }
    //if (!prsi.seen) { fail(ctx, "No PRSI block in preshader."); return; }
    if (!fxlc.seen) { fail(ctx, "No FXLC block in preshader."); return; }
    if (!clit.seen) { fail(ctx, "No CLIT block in preshader."); return; }

    MOJOSHADER_preshader *preshader = (MOJOSHADER_preshader *)
                                    Malloc(ctx, sizeof (MOJOSHADER_preshader));
    if (preshader == NULL)
        return;
    memset(preshader, '\0', sizeof (MOJOSHADER_preshader));
    ctx->preshader = preshader;

    // Let's set up the constant literals first...
    if (clit.tokcount == 0)
        fail(ctx, "Bogus CLIT block in preshader.");
    else
    {
        const uint32 lit_count = SWAP32(clit.tokens[1]);
        if (lit_count > ((clit.tokcount - 2) / 2))
        {
            fail(ctx, "Bogus CLIT block in preshader.");
            return;
        } // if
        else if (lit_count > 0)
        {
            preshader->literal_count = (unsigned int) lit_count;
            assert(sizeof (double) == 8);  // just in case.
            const size_t len = sizeof (double) * lit_count;
            preshader->literals = (double *) Malloc(ctx, len);
            if (preshader->literals == NULL)
                return;  // oh well.
            const double *litptr = (const double *) (clit.tokens + 2);
            int i;
            for (i = 0; i < lit_count; i++)
                preshader->literals[i] = SWAPDBL(litptr[i]);
        } // else if
    } // else



    //const uint32 first_output_reg = SWAP32(prsi.tokens[1]);
    // !!! FIXME: there are a lot of fields here I don't know about.
    // !!! FIXME:  maybe [2] and [3] are for int4 and bool registers?
    //const uint32 output_reg_count = SWAP32(prsi.tokens[4]);
    // !!! FIXME:  maybe [5] and [6] are for int4 and bool registers?
    uint32 output_map_count;
    const uint32 *output_map = NULL;
    if (prsi.seen) {
        // Parse out the PRSI block. This is used to map the output registers.
        if (prsi.tokcount < 8)
        {
            fail(ctx, "Bogus preshader PRSI data");
            return;
        } // if
        
        output_map_count = SWAP32(prsi.tokens[7]);
        
        prsi.tokcount -= 8;
        prsi.tokens += 8;

        if (prsi.tokcount < ((output_map_count + 1) * 2))
        {
            fail(ctx, "Bogus preshader PRSI data");
            return;
        } // if
        
        output_map = prsi.tokens;
    } else {
        output_map_count = 0;
    }

    // Now we'll figure out the CTAB...
    CtabData ctabdata = { 0, 0, 0 };
    parse_constant_table(ctx, ctab.tokens - 1, ctab.tokcount * 4,
                         okay_versions[1], 0, &ctabdata);

    // preshader owns this now. Don't free it in this function.
    preshader->symbol_count = ctabdata.symbol_count;
    preshader->symbols = ctabdata.symbols;

    if (!ctabdata.have_ctab)
    {
        fail(ctx, "Bogus preshader CTAB data");
        return;
    } // if

    // The FXLC block has the actual instructions...
    uint32 opcode_count = SWAP32(fxlc.tokens[1]);

    size_t len = sizeof (MOJOSHADER_preshaderInstruction) * opcode_count;
    preshader->instruction_count = (unsigned int) opcode_count;
    preshader->instructions = (MOJOSHADER_preshaderInstruction *)
                                Malloc(ctx, len);
    if (preshader->instructions == NULL)
        return;
    memset(preshader->instructions, '\0', len);

    fxlc.tokens += 2;
    fxlc.tokcount -= 2;
    if (opcode_count > (fxlc.tokcount / 2))
    {
        fail(ctx, "Bogus preshader FXLC block.");
        return;
    } // if

    MOJOSHADER_preshaderInstruction *inst = preshader->instructions;
    while (opcode_count--)
    {
        const uint32 opcodetok = SWAP32(fxlc.tokens[0]);
        
        MOJOSHADER_preshaderOpcode opcode = MOJOSHADER_PRESHADEROP_NOP;
        switch ((opcodetok >> 16) & 0x7FF0)
        {
            case 0x1000: opcode = MOJOSHADER_PRESHADEROP_MOV; break;
            case 0x1010: opcode = MOJOSHADER_PRESHADEROP_NEG; break;
            case 0x1030: opcode = MOJOSHADER_PRESHADEROP_RCP; break;
            case 0x1040: opcode = MOJOSHADER_PRESHADEROP_FRC; break;
            case 0x1050: opcode = MOJOSHADER_PRESHADEROP_EXP; break;
            case 0x1060: opcode = MOJOSHADER_PRESHADEROP_LOG; break;
            case 0x1070: opcode = MOJOSHADER_PRESHADEROP_RSQ; break;
            case 0x1080: opcode = MOJOSHADER_PRESHADEROP_SIN; break;
            case 0x1090: opcode = MOJOSHADER_PRESHADEROP_COS; break;
            case 0x10A0: opcode = MOJOSHADER_PRESHADEROP_ASIN; break;
            case 0x10B0: opcode = MOJOSHADER_PRESHADEROP_ACOS; break;
            case 0x10C0: opcode = MOJOSHADER_PRESHADEROP_ATAN; break;
            case 0x2000: opcode = MOJOSHADER_PRESHADEROP_MIN; break;
            case 0x2010: opcode = MOJOSHADER_PRESHADEROP_MAX; break;
            case 0x2020: opcode = MOJOSHADER_PRESHADEROP_LT; break;
            case 0x2030: opcode = MOJOSHADER_PRESHADEROP_GE; break;
            case 0x2040: opcode = MOJOSHADER_PRESHADEROP_ADD; break;
            case 0x2050: opcode = MOJOSHADER_PRESHADEROP_MUL; break;
            case 0x2060: opcode = MOJOSHADER_PRESHADEROP_ATAN2; break;
            case 0x2080: opcode = MOJOSHADER_PRESHADEROP_DIV; break;
            case 0x3000: opcode = MOJOSHADER_PRESHADEROP_CMP; break;
            case 0x3010: opcode = MOJOSHADER_PRESHADEROP_MOVC; break;
            case 0x5000: opcode = MOJOSHADER_PRESHADEROP_DOT; break;
            case 0x5020: opcode = MOJOSHADER_PRESHADEROP_NOISE; break;
            case 0xA000: opcode = MOJOSHADER_PRESHADEROP_MIN_SCALAR; break;
            case 0xA010: opcode = MOJOSHADER_PRESHADEROP_MAX_SCALAR; break;
            case 0xA020: opcode = MOJOSHADER_PRESHADEROP_LT_SCALAR; break;
            case 0xA030: opcode = MOJOSHADER_PRESHADEROP_GE_SCALAR; break;
            case 0xA040: opcode = MOJOSHADER_PRESHADEROP_ADD_SCALAR; break;
            case 0xA050: opcode = MOJOSHADER_PRESHADEROP_MUL_SCALAR; break;
            case 0xA060: opcode = MOJOSHADER_PRESHADEROP_ATAN2_SCALAR; break;
            case 0xA080: opcode = MOJOSHADER_PRESHADEROP_DIV_SCALAR; break;
            case 0xD000: opcode = MOJOSHADER_PRESHADEROP_DOT_SCALAR; break;
            case 0xD020: opcode = MOJOSHADER_PRESHADEROP_NOISE_SCALAR; break;
            default: fail(ctx, "Unknown preshader opcode."); break;
        } // switch

        uint32 operand_count = SWAP32(fxlc.tokens[1]) + 1;  // +1 for dest.

        inst->opcode = opcode;
        inst->element_count = (unsigned int) (opcodetok & 0xFF);
        inst->operand_count = (unsigned int) operand_count;

        fxlc.tokens += 2;
        fxlc.tokcount -= 2;
        if ((operand_count * 3) > fxlc.tokcount)
        {
            fail(ctx, "Bogus preshader FXLC block.");
            return;
        } // if

        MOJOSHADER_preshaderOperand *operand = inst->operands;
        while (operand_count--)
        {
            
            // !!! FIXME: don't quite know how this works
            unsigned int subOps = (unsigned int) SWAP32(fxlc.tokens[0]);
            operand->indexingType = 0;
            while (subOps--) {
                switch (fxlc.tokens[1]) {
                    case 2:
                    {
                        operand->indexingType = 2;
                        operand->indexingIndex = fxlc.tokens[2];
                        break;
                    }
                    default:
                    {
                        failf(ctx, "unkn indexer %d\n", fxlc.tokens[1]);
                        break;
                    }
                }
                //type info?
                fxlc.tokens += 2;
                fxlc.tokcount -= 2;
            }
            
            const unsigned int item = (unsigned int) SWAP32(fxlc.tokens[2]);
            
            operand->type = SWAP32(fxlc.tokens[1]);

            switch (operand->type)
            {
                case MOJOSHADER_PRESHADEROPERAND_LITERAL:  // literal from CLIT block.
                {
                    if (item >= preshader->literal_count)
                    {
                        fail(ctx, "Bogus preshader literal index.");
                        break;
                    } // if
                    break;
                } // case

                case MOJOSHADER_PRESHADEROPERAND_INPUT:  // item from ctabdata.
                {
                    int i;
                    MOJOSHADER_symbol *sym = ctabdata.symbols;
                    for (i = 0; i < ctabdata.symbol_count; i++, sym++)
                    {
                        const uint32 base = sym->register_index * 4;
                        const uint32 count = sym->register_count * 4;
                        assert(sym->register_set==MOJOSHADER_SYMREGSET_FLOAT4);
                        if ( (base <= item) && ((base + count) > item) )
                            break;
                    } // for
                    if (i == ctabdata.symbol_count)
                    {
                        fail(ctx, "Bogus preshader input index.");
                        break;
                    } // if
                    break;
                } // case

                case MOJOSHADER_PRESHADEROPERAND_OUTPUT:
                {
                    int i;
                    for (i = 0; i < output_map_count; i++)
                    {
                        const uint32 base = output_map[(i*2)] * 4;
                        const uint32 count = output_map[(i*2)+1] * 4;
                        if ( (base <= item) && ((base + count) > item) )
                            break;
                    } // for
                    if (output_map_count > 0 && i == output_map_count)
                    {
                        fail(ctx, "Bogus preshader output index.");
                        break;
                    } // if

                    break;
                } // case

                case MOJOSHADER_PRESHADEROPERAND_TEMP:
                {
                    if (item >= preshader->temp_count)
                        preshader->temp_count = item + 1;
                    break;
                } // case
                default:
                {
                    failf(ctx, "unknown operand type %d\n", operand->type);
                    break;
                }
            } // switch

            operand->index = item;

            fxlc.tokens += 3;
            fxlc.tokcount -= 3;
            operand++;
        } // while

        inst++;
    } // while
} // parse_preshader


static int parse_comment_token(Context *ctx)
{
    uint32 commenttoks = 0;
    if (is_comment_token(ctx, *ctx->tokens, &commenttoks))
    {
        if ((commenttoks >= 1) && (commenttoks < ctx->tokencount))
        {
            const uint32 id = SWAP32(ctx->tokens[1]);
            if (id == PRES_ID)
                parse_preshader(ctx, commenttoks);
            else if (id == CTAB_ID)
            {
                parse_constant_table(ctx, ctx->tokens, commenttoks * 4,
                                     ctx->version_token, 1, &ctx->ctab);
            } // else if
        } // if
        return commenttoks + 1;  // comment data plus the initial token.
    } // if

    return 0;  // not a comment token.
} // parse_comment_token


static int parse_end_token(Context *ctx)
{
    if (SWAP32(*(ctx->tokens)) != 0x0000FFFF)   // end token always 0x0000FFFF.
        return 0;  // not us, eat no tokens.

    if (ctx->tokencount != 1)  // we _must_ be last. If not: fail.
        fail(ctx, "end token before end of stream");

    if (!isfail(ctx))
        ctx->profile->end_emitter(ctx);

    return 1;
} // parse_end_token


static int parse_phase_token(Context *ctx)
{
    // !!! FIXME: needs state; allow only one phase token per shader, I think?
    if (SWAP32(*(ctx->tokens)) != 0x0000FFFD) // phase token always 0x0000FFFD.
        return 0;  // not us, eat no tokens.

    if ( (!shader_is_pixel(ctx)) || (!shader_version_exactly(ctx, 1, 4)) )
        fail(ctx, "phase token only available in 1.4 pixel shaders");

    if (!isfail(ctx))
        ctx->profile->phase_emitter(ctx);

    return 1;
} // parse_phase_token


static int parse_token(Context *ctx)
{
    int rc = 0;

    assert(ctx->output_stack_len == 0);

    if (ctx->tokencount == 0)
        fail(ctx, "unexpected end of shader.");

    else if ((rc = parse_comment_token(ctx)) != 0)
        return rc;

    else if ((rc = parse_end_token(ctx)) != 0)
        return rc;

    else if ((rc = parse_phase_token(ctx)) != 0)
        return rc;

    else if ((rc = parse_instruction_token(ctx)) != 0)
        return rc;

    failf(ctx, "unknown token (0x%x)", (uint) *ctx->tokens);
    return 1;  // good luck!
} // parse_token


static int find_profile_id(const char *profile)
{
    size_t i;
    for (i = 0; i < STATICARRAYLEN(profileMap); i++)
    {
        const char *name = profileMap[i].from;
        if (strcmp(name, profile) == 0)
        {
            profile = profileMap[i].to;
            break;
        } // if
    } // for

    for (i = 0; i < STATICARRAYLEN(profiles); i++)
    {
        const char *name = profiles[i].name;
        if (strcmp(name, profile) == 0)
            return i;
    } // for

    return -1;  // no match.
} // find_profile_id


static Context *build_context(const char *profile,
                              const unsigned char *tokenbuf,
                              const unsigned int bufsize,
                              const MOJOSHADER_swizzle *swiz,
                              const unsigned int swizcount,
                              MOJOSHADER_malloc m, MOJOSHADER_free f, void *d)
{
    if (m == NULL) m = MOJOSHADER_internal_malloc;
    if (f == NULL) f = MOJOSHADER_internal_free;

    Context *ctx = (Context *) m(sizeof (Context), d);
    if (ctx == NULL)
        return NULL;

    memset(ctx, '\0', sizeof (Context));
    ctx->malloc = m;
    ctx->free = f;
    ctx->malloc_data = d;
    ctx->tokens = (const uint32 *) tokenbuf;
    ctx->orig_tokens = (const uint32 *) tokenbuf;
    ctx->tokencount = bufsize / sizeof (uint32);
    ctx->swizzles = swiz;
    ctx->swizzles_count = swizcount;
    ctx->endline = ENDLINE_STR;
    ctx->endline_len = strlen(ctx->endline);
    ctx->last_address_reg_component = -1;
    ctx->current_position = MOJOSHADER_POSITION_BEFORE;

    ctx->errors = errorlist_create(MallocBridge, FreeBridge, ctx);
    if (ctx->errors == NULL)
    {
        f(ctx, d);
        return NULL;
    } // if

    if (!set_output(ctx, &ctx->mainline))
    {
        errorlist_destroy(ctx->errors);
        f(ctx, d);
        return NULL;
    } // if

    const int profileid = find_profile_id(profile);
    ctx->profileid = profileid;
    if (profileid >= 0)
        ctx->profile = &profiles[profileid];
    else
        failf(ctx, "Profile '%s' is unknown or unsupported", profile);

    return ctx;
} // build_context


static void free_constants_list(MOJOSHADER_free f, void *d, ConstantsList *item)
{
    while (item != NULL)
    {
        ConstantsList *next = item->next;
        f(item, d);
        item = next;
    } // while
} // free_constants_list


static void free_variable_list(MOJOSHADER_free f, void *d, VariableList *item)
{
    while (item != NULL)
    {
        VariableList *next = item->next;
        f(item, d);
        item = next;
    } // while
} // free_variable_list


static void free_sym_typeinfo(MOJOSHADER_free f, void *d,
                              MOJOSHADER_symbolTypeInfo *typeinfo)
{
    int i;
    for (i = 0; i < typeinfo->member_count; i++)
    {
        f((void *) typeinfo->members[i].name, d);
        free_sym_typeinfo(f, d, &typeinfo->members[i].info);
    } // for
    f((void *) typeinfo->members, d);
} // free_sym_members


static void free_symbols(MOJOSHADER_free f, void *d, MOJOSHADER_symbol *syms,
                         const int symcount)
{
    int i;
    for (i = 0; i < symcount; i++)
    {
        f((void *) syms[i].name, d);
        free_sym_typeinfo(f, d, &syms[i].info);
    } // for
    f((void *) syms, d);
} // free_symbols


static void free_preshader(MOJOSHADER_free f, void *d,
                           MOJOSHADER_preshader *preshader)
{
    if (preshader != NULL)
    {
        f((void *) preshader->literals, d);
        f((void *) preshader->instructions, d);
        free_symbols(f, d, preshader->symbols, preshader->symbol_count);
        f((void *) preshader, d);
    } // if
} // free_preshader


static void destroy_context(Context *ctx)
{
    if (ctx != NULL)
    {
        MOJOSHADER_free f = ((ctx->free != NULL) ? ctx->free : MOJOSHADER_internal_free);
        void *d = ctx->malloc_data;
        buffer_destroy(ctx->preflight);
        buffer_destroy(ctx->globals);
        buffer_destroy(ctx->helpers);
        buffer_destroy(ctx->subroutines);
        buffer_destroy(ctx->mainline_intro);
        buffer_destroy(ctx->mainline);
        buffer_destroy(ctx->ignore);
        free_constants_list(f, d, ctx->constants);
        free_reglist(f, d, ctx->used_registers.next);
        free_reglist(f, d, ctx->defined_registers.next);
        free_reglist(f, d, ctx->uniforms.next);
        free_reglist(f, d, ctx->attributes.next);
        free_reglist(f, d, ctx->samplers.next);
        free_variable_list(f, d, ctx->variables);
        errorlist_destroy(ctx->errors);
        free_symbols(f, d, ctx->ctab.symbols, ctx->ctab.symbol_count);
        free_preshader(f, d, ctx->preshader);
        f(ctx, d);
    } // if
} // destroy_context


static char *build_output(Context *ctx, size_t *len)
{
    // add a byte for a null terminator.
    Buffer *buffers[] = {
        ctx->preflight, ctx->globals, ctx->helpers,
        ctx->subroutines, ctx->mainline_intro, ctx->mainline
        // don't append ctx->ignore ... that's why it's called "ignore"
    };
    char *retval = buffer_merge(buffers, STATICARRAYLEN(buffers), len);
    return retval;
} // build_output


static inline const char *alloc_varname(Context *ctx, const RegisterList *reg)
{
    return ctx->profile->get_varname(ctx, reg->regtype, reg->regnum);
} // alloc_varname


// !!! FIXME: this code is sort of hard to follow:
// !!! FIXME:  "var->used" only applies to arrays (at the moment, at least,
// !!! FIXME:  but this might be buggy at a later time?), and this code
// !!! FIXME:  relies on that.
// !!! FIXME: "variables" means "things we found in a CTAB" but it's not
// !!! FIXME:  all registers, etc.
// !!! FIXME: "const_array" means an array for d3d "const" registers (c0, c1,
// !!! FIXME:  etc), but not a constant array, although they _can_ be.
// !!! FIXME: It's just a mess.  :/
static MOJOSHADER_uniform *build_uniforms(Context *ctx)
{
    const size_t len = sizeof (MOJOSHADER_uniform) * ctx->uniform_count;
    MOJOSHADER_uniform *retval = (MOJOSHADER_uniform *) Malloc(ctx, len);

    if (retval != NULL)
    {
        MOJOSHADER_uniform *wptr = retval;
        memset(wptr, '\0', len);

        VariableList *var;
        int written = 0;
        for (var = ctx->variables; var != NULL; var = var->next)
        {
            if (var->used)
            {
                const char *name = ctx->profile->get_const_array_varname(ctx,
                                                      var->index, var->count);
                if (name != NULL)
                {
                    wptr->type = MOJOSHADER_UNIFORM_FLOAT;
                    wptr->index = var->index;
                    wptr->array_count = var->count;
                    wptr->constant = (var->constant != NULL) ? 1 : 0;
                    wptr->name = name;
                    wptr++;
                    written++;
                } // if
            } // if
        } // for

        RegisterList *item = ctx->uniforms.next;
        MOJOSHADER_uniformType type = MOJOSHADER_UNIFORM_FLOAT;
        while (written < ctx->uniform_count)
        {
            int skip = 0;

            // !!! FIXME: does this fail if written > ctx->uniform_count?
            if (item == NULL)
            {
                fail(ctx, "BUG: mismatched uniform list and count");
                break;
            } // if

            int index = item->regnum;
            switch (item->regtype)
            {
                case REG_TYPE_CONST:
                    skip = (item->array != NULL);
                    type = MOJOSHADER_UNIFORM_FLOAT;
                    break;

                case REG_TYPE_CONSTINT:
                    type = MOJOSHADER_UNIFORM_INT;
                    break;

                case REG_TYPE_CONSTBOOL:
                    type = MOJOSHADER_UNIFORM_BOOL;
                    break;

                default:
                    fail(ctx, "unknown uniform datatype");
                    break;
            } // switch

            if (!skip)
            {
                wptr->type = type;
                wptr->index = index;
                wptr->array_count = 0;
                wptr->name = alloc_varname(ctx, item);
                wptr++;
                written++;
            } // if

            item = item->next;
        } // for
    } // if

    return retval;
} // build_uniforms


static MOJOSHADER_constant *build_constants(Context *ctx)
{
    const size_t len = sizeof (MOJOSHADER_constant) * ctx->constant_count;
    MOJOSHADER_constant *retval = (MOJOSHADER_constant *) Malloc(ctx, len);

    if (retval != NULL)
    {
        ConstantsList *item = ctx->constants;
        int i;

        for (i = 0; i < ctx->constant_count; i++)
        {
            if (item == NULL)
            {
                fail(ctx, "BUG: mismatched constant list and count");
                break;
            } // if

            memcpy(&retval[i], &item->constant, sizeof (MOJOSHADER_constant));
            item = item->next;
        } // for
    } // if

    return retval;
} // build_constants


static MOJOSHADER_sampler *build_samplers(Context *ctx)
{
    const size_t len = sizeof (MOJOSHADER_sampler) * ctx->sampler_count;
    MOJOSHADER_sampler *retval = (MOJOSHADER_sampler *) Malloc(ctx, len);

    if (retval != NULL)
    {
        RegisterList *item = ctx->samplers.next;
        MOJOSHADER_samplerType type = MOJOSHADER_SAMPLER_2D;
        int i;

        memset(retval, '\0', len);

        for (i = 0; i < ctx->sampler_count; i++)
        {
            if (item == NULL)
            {
                fail(ctx, "BUG: mismatched sampler list and count");
                break;
            } // if

            assert(item->regtype == REG_TYPE_SAMPLER);
            switch ((const TextureType) item->index)
            {
                case TEXTURE_TYPE_2D:
                    type = MOJOSHADER_SAMPLER_2D;
                    break;

                case TEXTURE_TYPE_CUBE:
                    type = MOJOSHADER_SAMPLER_CUBE;
                    break;

                case TEXTURE_TYPE_VOLUME:
                    type = MOJOSHADER_SAMPLER_VOLUME;
                    break;

                default:
                    fail(ctx, "Unknown sampler type");
                    break;
            } // switch

            retval[i].type = type;
            retval[i].index = item->regnum;
            retval[i].name = alloc_varname(ctx, item);
            item = item->next;
        } // for
    } // if

    return retval;
} // build_samplers


static MOJOSHADER_attribute *build_attributes(Context *ctx, int *_count)
{
    int count = 0;

    if (ctx->attribute_count == 0)
    {
        *_count = 0;
        return NULL;  // nothing to do.
    } // if

    const size_t len = sizeof (MOJOSHADER_attribute) * ctx->attribute_count;
    MOJOSHADER_attribute *retval = (MOJOSHADER_attribute *) Malloc(ctx, len);

    if (retval != NULL)
    {
        RegisterList *item = ctx->attributes.next;
        MOJOSHADER_attribute *wptr = retval;
        int ignore = 0;
        int i;

        memset(retval, '\0', len);

        for (i = 0; i < ctx->attribute_count; i++)
        {
            if (item == NULL)
            {
                fail(ctx, "BUG: mismatched attribute list and count");
                break;
            } // if

            switch (item->regtype)
            {
                case REG_TYPE_RASTOUT:
                case REG_TYPE_ATTROUT:
                case REG_TYPE_TEXCRDOUT:
                case REG_TYPE_COLOROUT:
                case REG_TYPE_DEPTHOUT:
                    ignore = 1;
                    break;
                case REG_TYPE_TEXTURE:
                case REG_TYPE_MISCTYPE:
                case REG_TYPE_INPUT:
                    ignore = shader_is_pixel(ctx);
                    break;
                default:
                    ignore = 0;
                    break;
            } // switch

            if (!ignore)
            {
                if (shader_is_pixel(ctx))
                    fail(ctx, "BUG: pixel shader with vertex attributes");
                else
                {
                    wptr->usage = item->usage;
                    wptr->index = item->index;
                    wptr->name = alloc_varname(ctx, item);
                    wptr++;
                    count++;
                } // else
            } // if

            item = item->next;
        } // for
    } // if

    *_count = count;
    return retval;
} // build_attributes

static MOJOSHADER_attribute *build_outputs(Context *ctx, int *_count)
{
    int count = 0;

    if (ctx->attribute_count == 0)
    {
        *_count = 0;
        return NULL;  // nothing to do.
    } // if

    const size_t len = sizeof (MOJOSHADER_attribute) * ctx->attribute_count;
    MOJOSHADER_attribute *retval = (MOJOSHADER_attribute *) Malloc(ctx, len);

    if (retval != NULL)
    {
        RegisterList *item = ctx->attributes.next;
        MOJOSHADER_attribute *wptr = retval;
        int i;

        memset(retval, '\0', len);

        for (i = 0; i < ctx->attribute_count; i++)
        {
            if (item == NULL)
            {
                fail(ctx, "BUG: mismatched attribute list and count");
                break;
            } // if

            switch (item->regtype)
            {
                case REG_TYPE_RASTOUT:
                case REG_TYPE_ATTROUT:
                case REG_TYPE_TEXCRDOUT:
                case REG_TYPE_COLOROUT:
                case REG_TYPE_DEPTHOUT:
                    wptr->usage = item->usage;
                    wptr->index = item->index;
                    wptr->name = alloc_varname(ctx, item);
                    wptr++;
                    count++;
                    break;
                default:
                    break;
            } // switch


            item = item->next;
        } // for
    } // if

    *_count = count;
    return retval;
} // build_outputs


static MOJOSHADER_parseData *build_parsedata(Context *ctx)
{
    char *output = NULL;
    MOJOSHADER_constant *constants = NULL;
    MOJOSHADER_uniform *uniforms = NULL;
    MOJOSHADER_attribute *attributes = NULL;
    MOJOSHADER_attribute *outputs = NULL;
    MOJOSHADER_sampler *samplers = NULL;
    MOJOSHADER_swizzle *swizzles = NULL;
    MOJOSHADER_error *errors = NULL;
    MOJOSHADER_parseData *retval = NULL;
    size_t output_len = 0;
    int attribute_count = 0;
    int output_count = 0;

    if (ctx->out_of_memory)
        return &MOJOSHADER_out_of_mem_data;

    retval = (MOJOSHADER_parseData*) Malloc(ctx, sizeof(MOJOSHADER_parseData));
    if (retval == NULL)
        return &MOJOSHADER_out_of_mem_data;

    memset(retval, '\0', sizeof (MOJOSHADER_parseData));

    if (!isfail(ctx))
        output = build_output(ctx, &output_len);

    if (!isfail(ctx))
        constants = build_constants(ctx);

    if (!isfail(ctx))
        uniforms = build_uniforms(ctx);

    if (!isfail(ctx))
        attributes = build_attributes(ctx, &attribute_count);

    if (!isfail(ctx))
        outputs = build_outputs(ctx, &output_count);

    if (!isfail(ctx))
        samplers = build_samplers(ctx);

    const int error_count = errorlist_count(ctx->errors);
    errors = errorlist_flatten(ctx->errors);

    if (!isfail(ctx))
    {
        if (ctx->swizzles_count > 0)
        {
            const int len = ctx->swizzles_count * sizeof (MOJOSHADER_swizzle);
            swizzles = (MOJOSHADER_swizzle *) Malloc(ctx, len);
            if (swizzles != NULL)
                memcpy(swizzles, ctx->swizzles, len);
        } // if
    } // if

    // check again, in case build_output, etc, ran out of memory.
    if (isfail(ctx))
    {
        int i;

        Free(ctx, output);
        Free(ctx, constants);
        Free(ctx, swizzles);

        if (uniforms != NULL)
        {
            for (i = 0; i < ctx->uniform_count; i++)
                Free(ctx, (void *) uniforms[i].name);
            Free(ctx, uniforms);
        } // if

        if (attributes != NULL)
        {
            for (i = 0; i < attribute_count; i++)
                Free(ctx, (void *) attributes[i].name);
            Free(ctx, attributes);
        } // if

        if (outputs != NULL)
        {
            for (i = 0; i < output_count; i++)
                Free(ctx, (void *) outputs[i].name);
            Free(ctx, outputs);
        } // if

        if (samplers != NULL)
        {
            for (i = 0; i < ctx->sampler_count; i++)
                Free(ctx, (void *) samplers[i].name);
            Free(ctx, samplers);
        } // if

        if (ctx->out_of_memory)
        {
            for (i = 0; i < error_count; i++)
            {
                Free(ctx, (void *) errors[i].filename);
                Free(ctx, (void *) errors[i].error);
            } // for
            Free(ctx, errors);
            Free(ctx, retval);
            return &MOJOSHADER_out_of_mem_data;
        } // if
    } // if
    else
    {
        retval->profile = ctx->profile->name;
        retval->output = output;
        retval->output_len = (int) output_len;
        retval->instruction_count = ctx->instruction_count;
        retval->shader_type = ctx->shader_type;
        retval->major_ver = (int) ctx->major_ver;
        retval->minor_ver = (int) ctx->minor_ver;
        retval->uniform_count = ctx->uniform_count;
        retval->uniforms = uniforms;
        retval->constant_count = ctx->constant_count;
        retval->constants = constants;
        retval->sampler_count = ctx->sampler_count;
        retval->samplers = samplers;
        retval->attribute_count = attribute_count;
        retval->attributes = attributes;
        retval->output_count = output_count;
        retval->outputs = outputs;
        retval->swizzle_count = ctx->swizzles_count;
        retval->swizzles = swizzles;
        retval->symbol_count = ctx->ctab.symbol_count;
        retval->symbols = ctx->ctab.symbols;
        retval->preshader = ctx->preshader;

        // we don't own these now, retval does.
        ctx->ctab.symbols = NULL;
        ctx->preshader = NULL;
        ctx->ctab.symbol_count = 0;
    } // else

    retval->error_count = error_count;
    retval->errors = errors;
    retval->malloc = (ctx->malloc == MOJOSHADER_internal_malloc) ? NULL : ctx->malloc;
    retval->free = (ctx->free == MOJOSHADER_internal_free) ? NULL : ctx->free;
    retval->malloc_data = ctx->malloc_data;

    return retval;
} // build_parsedata


static void process_definitions(Context *ctx)
{
    // !!! FIXME: apparently, pre ps_3_0, sampler registers don't need to be
    // !!! FIXME:  DCL'd before use (default to 2d?). We aren't checking
    // !!! FIXME:  this at the moment, though.

    determine_constants_arrays(ctx);  // in case this hasn't been called yet.

    RegisterList *uitem = &ctx->uniforms;
    RegisterList *prev = &ctx->used_registers;
    RegisterList *item = prev->next;

    while (item != NULL)
    {
        RegisterList *next = item->next;
        const RegisterType regtype = item->regtype;
        const int regnum = item->regnum;

        if (!get_defined_register(ctx, regtype, regnum))
        {
            // haven't already dealt with this one.
            switch (regtype)
            {
                // !!! FIXME: I'm not entirely sure this is right...
                case REG_TYPE_RASTOUT:
                case REG_TYPE_ATTROUT:
                case REG_TYPE_TEXCRDOUT:
                case REG_TYPE_COLOROUT:
                case REG_TYPE_DEPTHOUT:
                    if (shader_is_vertex(ctx)&&shader_version_atleast(ctx,3,0))
                    {
                        fail(ctx, "vs_3 can't use output registers"
                                  " without declaring them first.");
                        return;
                    } // if

                    //find the usage
                    //hack copypastad from emit_GLSL_attribute
                    MOJOSHADER_usage usage = MOJOSHADER_USAGE_UNKNOWN;
                    if (regtype == REG_TYPE_RASTOUT)
                    {
                        switch ((const RastOutType) regnum)
                        {
                            case RASTOUT_TYPE_POSITION:
                                usage = MOJOSHADER_USAGE_POSITION;
                                break;
                            case RASTOUT_TYPE_FOG:
                                usage = MOJOSHADER_USAGE_FOG;
                                break;
                            case RASTOUT_TYPE_POINT_SIZE:
                                usage = MOJOSHADER_USAGE_POINTSIZE;
                                break;
                        } // switch
                    } // if

                    else if (regtype == REG_TYPE_ATTROUT)
                    {
                        usage = MOJOSHADER_USAGE_COLOR;
                    } // else if

                    else if (regtype == REG_TYPE_TEXCRDOUT)
                    {
                        usage = MOJOSHADER_USAGE_TEXCOORD;
                    } // else if

                    // Apparently this is an attribute that wasn't DCL'd.
                    //  Add it to the attribute list; deal with it later.
                    add_attribute_register(ctx, item->regtype, item->regnum,
                                           usage, 0, 0xF, 0);
                    break;

                case REG_TYPE_ADDRESS:
                case REG_TYPE_PREDICATE:
                case REG_TYPE_TEMP:
                case REG_TYPE_LOOP:
                case REG_TYPE_LABEL:
                    ctx->profile->global_emitter(ctx, regtype, regnum);
                    break;

                case REG_TYPE_CONST:
                case REG_TYPE_CONSTINT:
                case REG_TYPE_CONSTBOOL:
                    // separate uniforms into a different list for now.
                    prev->next = next;
                    item->next = NULL;
                    uitem->next = item;
                    uitem = item;
                    item = prev;
                    break;

                default:
                    fail(ctx, "BUG: we used a register we don't know how to define.");
            } // switch
        } // if

        prev = item;
        item = next;
    } // while

    // okay, now deal with uniform/constant arrays...
    VariableList *var;
    for (var = ctx->variables; var != NULL; var = var->next)
    {
        if (var->used)
        {
            if (var->constant)
            {
                ctx->profile->const_array_emitter(ctx, var->constant,
                                                  var->index, var->count);
            } // if
            else
            {
                ctx->profile->array_emitter(ctx, var);
                ctx->uniform_float4_count += var->count;
                ctx->uniform_count++;
            } // else
        } // if
    } // for

    // ...and uniforms...
    for (item = ctx->uniforms.next; item != NULL; item = item->next)
    {
        int arraysize = -1;

        // check if this is a register contained in an array...
        if (item->regtype == REG_TYPE_CONST)
        {
            for (var = ctx->variables; var != NULL; var = var->next)
            {
                if (!var->used)
                    continue;

                const int regnum = item->regnum;
                const int lo = var->index;
                if ( (regnum >= lo) && (regnum < (lo + var->count)) )
                {
                    assert(!var->constant);
                    item->array = var;  // used when building parseData.
                    arraysize = var->count;
                    break;
                } // if
            } // for
        } // if

        ctx->profile->uniform_emitter(ctx, item->regtype, item->regnum, var);

        if (arraysize < 0)  // not part of an array?
        {
            ctx->uniform_count++;
            switch (item->regtype)
            {
                case REG_TYPE_CONST: ctx->uniform_float4_count++; break;
                case REG_TYPE_CONSTINT: ctx->uniform_int4_count++; break;
                case REG_TYPE_CONSTBOOL: ctx->uniform_bool_count++; break;
                default: break;
            } // switch
        } // if
    } // for

    // ...and samplers...
    for (item = ctx->samplers.next; item != NULL; item = item->next)
    {
        ctx->sampler_count++;
        ctx->profile->sampler_emitter(ctx, item->regnum,
                                      (TextureType) item->index);
    } // for

    // ...and attributes...
    for (item = ctx->attributes.next; item != NULL; item = item->next)
    {
        ctx->attribute_count++;
        ctx->profile->attribute_emitter(ctx, item->regtype, item->regnum,
                                        item->usage, item->index,
                                        item->writemask, item->misc);
    } // for
} // process_definitions


static void verify_swizzles(Context *ctx)
{
    size_t i;
    const char *failmsg = "invalid swizzle";
    for (i = 0; i < ctx->swizzles_count; i++)
    {
        const MOJOSHADER_swizzle *swiz = &ctx->swizzles[i];
        if (swiz->swizzles[0] > 3) { fail(ctx, failmsg); return; }
        if (swiz->swizzles[1] > 3) { fail(ctx, failmsg); return; }
        if (swiz->swizzles[2] > 3) { fail(ctx, failmsg); return; }
        if (swiz->swizzles[3] > 3) { fail(ctx, failmsg); return; }
    } // for
} // verify_swizzles

const MOJOSHADER_parseData *MOJOSHADER_parseExpression(const unsigned char *tokenbuf,
                                      const unsigned int bufsize,
                                      MOJOSHADER_malloc m,
                                      MOJOSHADER_free f, void *d)
{
    MOJOSHADER_parseData *retval;
    Context *ctx = NULL;
    if ( ((m == NULL) && (f != NULL)) || ((m != NULL) && (f == NULL)) )
        return NULL;  // supply both or neither.

    ctx = build_context("glsl120", tokenbuf, bufsize, NULL, 0, m, f, d);
    if (ctx == NULL)
        return NULL;
    
    parse_preshader(ctx, bufsize/4-1);
    retval = build_parsedata(ctx);
    destroy_context(ctx);
    
    return retval;
}

// API entry point...

const MOJOSHADER_parseData *MOJOSHADER_parse(const char *profile,
                                             const unsigned char *tokenbuf,
                                             const unsigned int bufsize,
                                             const MOJOSHADER_swizzle *swiz,
                                             const unsigned int swizcount,
                                             MOJOSHADER_malloc m,
                                             MOJOSHADER_free f, void *d)
{
    MOJOSHADER_parseData *retval = NULL;
    Context *ctx = NULL;
    int rc = 0;
    int failed = 0;

    if ( ((m == NULL) && (f != NULL)) || ((m != NULL) && (f == NULL)) )
        return &MOJOSHADER_out_of_mem_data;  // supply both or neither.

    ctx = build_context(profile, tokenbuf, bufsize, swiz, swizcount, m, f, d);
    if (ctx == NULL)
        return &MOJOSHADER_out_of_mem_data;
	
    if (isfail(ctx))
    {
        retval = build_parsedata(ctx);
        destroy_context(ctx);
        return retval;
    } // if

    verify_swizzles(ctx);

    // Version token always comes first.
    ctx->current_position = 0;
    rc = parse_version_token(ctx, profile);

    // drop out now if this definitely isn't bytecode. Saves lots of
    //  meaningless errors flooding through.
    if (rc < 0)
    {
        retval = build_parsedata(ctx);
        destroy_context(ctx);
        return retval;
    } // if

    if ( ((uint32) rc) > ctx->tokencount )
    {
        fail(ctx, "Corrupted or truncated shader");
        ctx->tokencount = rc;
    } // if

    adjust_token_position(ctx, rc);

    // parse out the rest of the tokens after the version token...
    while (ctx->tokencount > 0)
    {
        // reset for each token.
        if (isfail(ctx))
        {
            failed = 1;
            ctx->isfail = 0;
        } // if

        rc = parse_token(ctx);
        if ( ((uint32) rc) > ctx->tokencount )
        {
            fail(ctx, "Corrupted or truncated shader");
            break;
        } // if

        adjust_token_position(ctx, rc);
    } // while

    ctx->current_position = MOJOSHADER_POSITION_AFTER;

    if (!failed)
    {
        process_definitions(ctx);
        failed = isfail(ctx);
    } // if

    if (!failed)
        ctx->profile->finalize_emitter(ctx);

    ctx->isfail = failed;
    retval = build_parsedata(ctx);
    destroy_context(ctx);
    return retval;
} // MOJOSHADER_parse


void MOJOSHADER_freeParseData(const MOJOSHADER_parseData *_data)
{
    MOJOSHADER_parseData *data = (MOJOSHADER_parseData *) _data;
    if ((data == NULL) || (data == &MOJOSHADER_out_of_mem_data))
        return;  // no-op.

    MOJOSHADER_free f = (data->free == NULL) ? MOJOSHADER_internal_free : data->free;
    void *d = data->malloc_data;
    int i;

    // we don't f(data->profile), because that's internal static data.

    f((void *) data->output, d);
    f((void *) data->constants, d);
    f((void *) data->swizzles, d);

    for (i = 0; i < data->error_count; i++)
    {
        f((void *) data->errors[i].error, d);
        f((void *) data->errors[i].filename, d);
    } // for
    f((void *) data->errors, d);

    for (i = 0; i < data->uniform_count; i++)
        f((void *) data->uniforms[i].name, d);
    f((void *) data->uniforms, d);

    for (i = 0; i < data->attribute_count; i++)
        f((void *) data->attributes[i].name, d);
    f((void *) data->attributes, d);

    for (i = 0; i < data->output_count; i++)
        f((void *) data->outputs[i].name, d);
    f((void *) data->outputs, d);

    for (i = 0; i < data->sampler_count; i++)
        f((void *) data->samplers[i].name, d);
    f((void *) data->samplers, d);

    free_symbols(f, d, data->symbols, data->symbol_count);
    free_preshader(f, d, data->preshader);

    f(data, d);
} // MOJOSHADER_freeParseData


int MOJOSHADER_version(void)
{
    return MOJOSHADER_VERSION;
} // MOJOSHADER_version


const char *MOJOSHADER_changeset(void)
{
    return MOJOSHADER_CHANGESET;
} // MOJOSHADER_changeset


int MOJOSHADER_maxShaderModel(const char *profile)
{
    #define PROFILE_SHADER_MODEL(p,v) if (strcmp(profile, p) == 0) return v;
    PROFILE_SHADER_MODEL(MOJOSHADER_PROFILE_D3D, 3);
    PROFILE_SHADER_MODEL(MOJOSHADER_PROFILE_BYTECODE, 3);
    PROFILE_SHADER_MODEL(MOJOSHADER_PROFILE_GLSL, 3);
    PROFILE_SHADER_MODEL(MOJOSHADER_PROFILE_GLSL120, 3);
    PROFILE_SHADER_MODEL(MOJOSHADER_PROFILE_ARB1, 2);
    PROFILE_SHADER_MODEL(MOJOSHADER_PROFILE_NV2, 2);
    PROFILE_SHADER_MODEL(MOJOSHADER_PROFILE_NV3, 2);
    PROFILE_SHADER_MODEL(MOJOSHADER_PROFILE_NV4, 3);
    #undef PROFILE_SHADER_MODEL
    return -1;  // unknown profile?
} // MOJOSHADER_maxShaderModel

// end of mojoshader.c ...

