/**
 * MojoShader; generate shader programs from bytecode of compiled
 *  Direct3D shaders.
 *
 * Please see the file LICENSE.txt in the source's root directory.
 *
 *  This file written by Ryan C. Gordon.
 */

// !!! FIXME: preshaders shouldn't be handled in here at all. This should
// !!! FIXME:  be in the Effects API, once that's actually written.

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <stdarg.h>
#include <assert.h>

// !!! FIXME: most of these _MSC_VER should probably be _WINDOWS?
#ifdef _MSC_VER
#define WIN32_LEAN_AND_MEAN 1
#include <windows.h>  // GL headers need this for WINGDIAPI definition.
#endif

#if (defined(__APPLE__) && defined(__MACH__))
//#define PLATFORM_MACOSX 1
#endif

#if PLATFORM_MACOSX
#include <Carbon/Carbon.h>
#endif

#define __MOJOSHADER_INTERNAL__ 1
#include "mojoshader_internal.h"

#define GL_GLEXT_LEGACY 1
#include "GL/gl.h"
#include "GL/glext.h"

#ifndef GL_HALF_FLOAT_NV
#define GL_HALF_FLOAT_NV 0x140B
#endif

#ifndef GL_HALF_FLOAT_ARB
#define GL_HALF_FLOAT_ARB 0x140B
#endif

#ifndef GL_HALF_FLOAT_OES
#define GL_HALF_FLOAT_OES 0x8D61
#endif

// this happens to be the same value for ARB1 and GLSL.
#ifndef GL_PROGRAM_POINT_SIZE
#define GL_PROGRAM_POINT_SIZE 0x8642
#endif

struct MOJOSHADER_glShader
{
    const MOJOSHADER_parseData *parseData;
    GLuint handle;
    uint32 refcount;
};

typedef struct
{
    MOJOSHADER_shaderType shader_type;
    const MOJOSHADER_uniform *uniform;
    GLint location;
} UniformMap;

typedef struct
{
    const MOJOSHADER_attribute *attribute;
    GLint location;
} AttributeMap;

struct MOJOSHADER_glProgram
{
    MOJOSHADER_glShader *vertex;
    MOJOSHADER_glShader *fragment;
    GLuint handle;
    uint32 generation;
    uint32 uniform_count;
    UniformMap *uniforms;
    uint32 attribute_count;
    AttributeMap *attributes;
    size_t vs_uniforms_float4_count;
    GLfloat *vs_uniforms_float4;
    size_t vs_uniforms_int4_count;
    GLint *vs_uniforms_int4;
    size_t vs_uniforms_bool_count;
    GLint *vs_uniforms_bool;
    size_t ps_uniforms_float4_count;
    GLfloat *ps_uniforms_float4;
    size_t ps_uniforms_int4_count;
    GLint *ps_uniforms_int4;
    size_t ps_uniforms_bool_count;
    GLint *ps_uniforms_bool;

    size_t vs_preshader_reg_count;
    GLfloat *vs_preshader_regs;
    size_t ps_preshader_reg_count;
    GLfloat *ps_preshader_regs;

    uint32 refcount;

    int uses_pointsize;

    // GLSL uses these...location of uniform arrays.
    GLint vs_float4_loc;
    GLint vs_int4_loc;
    GLint vs_bool_loc;
    GLint ps_float4_loc;
    GLint ps_int4_loc;
    GLint ps_bool_loc;
};

#ifndef WINGDIAPI
#define WINGDIAPI
#endif

// Entry points in base OpenGL that lack function pointer prototypes...
typedef WINGDIAPI void (APIENTRYP PFNGLGETINTEGERVPROC) (GLenum pname, GLint *params);
typedef WINGDIAPI const GLubyte * (APIENTRYP PFNGLGETSTRINGPROC) (GLenum name);
typedef WINGDIAPI GLenum (APIENTRYP PFNGLGETERRORPROC) (void);
typedef WINGDIAPI void (APIENTRYP PFNGLENABLEPROC) (GLenum cap);
typedef WINGDIAPI void (APIENTRYP PFNGLDISABLEPROC) (GLenum cap);

// Max entries for each register file type...
#define MAX_REG_FILE_F 8192
#define MAX_REG_FILE_I 2047
#define MAX_REG_FILE_B 2047

struct MOJOSHADER_glContext
{
    // Allocators...
    MOJOSHADER_malloc malloc_fn;
    MOJOSHADER_free free_fn;
    void *malloc_data;

    // The constant register files...
    // !!! FIXME: Man, it kills me how much memory this takes...
    // !!! FIXME:  ... make this dynamically allocated on demand.
    GLfloat vs_reg_file_f[MAX_REG_FILE_F * 4];
    GLint vs_reg_file_i[MAX_REG_FILE_I * 4];
    uint8 vs_reg_file_b[MAX_REG_FILE_B];
    GLfloat ps_reg_file_f[MAX_REG_FILE_F * 4];
    GLint ps_reg_file_i[MAX_REG_FILE_I * 4];
    uint8 ps_reg_file_b[MAX_REG_FILE_B];
    GLuint sampler_reg_file[16];

    // This increments every time we change the register files.
    uint32 generation;

    // This keeps track of implicitly linked programs.
    HashTable *linker_cache;

    // This tells us which vertex attribute arrays we have enabled.
    GLint max_attrs;
    uint8 want_attr[32];
    uint8 have_attr[32];

    // rarely used, so we don't touch when we don't have to.
    int pointsize_enabled;

    // GL stuff...
    int opengl_major;
    int opengl_minor;
    int glsl_major;
    int glsl_minor;
    MOJOSHADER_glProgram *bound_program;
    char profile[16];

    // Extensions...
    int have_core_opengl;
    int have_opengl_2;  // different entry points than ARB extensions.
    int have_GL_ARB_vertex_program;
    int have_GL_ARB_fragment_program;
    int have_GL_NV_vertex_program2_option;
    int have_GL_NV_fragment_program2;
    int have_GL_NV_vertex_program3;
    int have_GL_NV_gpu_program4;
    int have_GL_ARB_shader_objects;
    int have_GL_ARB_vertex_shader;
    int have_GL_ARB_fragment_shader;
    int have_GL_ARB_shading_language_100;
    int have_GL_NV_half_float;
    int have_GL_ARB_half_float_vertex;
    int have_GL_OES_vertex_half_float;

    // Entry points...
    PFNGLGETSTRINGPROC glGetString;
    PFNGLGETERRORPROC glGetError;
    PFNGLGETINTEGERVPROC glGetIntegerv;
    PFNGLENABLEPROC glEnable;
    PFNGLDISABLEPROC glDisable;
    PFNGLDELETESHADERPROC glDeleteShader;
    PFNGLDELETEPROGRAMPROC glDeleteProgram;
    PFNGLATTACHSHADERPROC glAttachShader;
    PFNGLCOMPILESHADERPROC glCompileShader;
    PFNGLCREATESHADERPROC glCreateShader;
    PFNGLCREATEPROGRAMPROC glCreateProgram;
    PFNGLDISABLEVERTEXATTRIBARRAYPROC glDisableVertexAttribArray;
    PFNGLENABLEVERTEXATTRIBARRAYPROC glEnableVertexAttribArray;
    PFNGLGETATTRIBLOCATIONPROC glGetAttribLocation;
    PFNGLGETPROGRAMINFOLOGPROC glGetProgramInfoLog;
    PFNGLGETSHADERIVPROC glGetShaderiv;
    PFNGLGETPROGRAMIVPROC glGetProgramiv;
    PFNGLGETUNIFORMLOCATIONPROC glGetUniformLocation;
    PFNGLLINKPROGRAMPROC glLinkProgram;
    PFNGLSHADERSOURCEPROC glShaderSource;
    PFNGLUNIFORM1IPROC glUniform1i;
    PFNGLUNIFORM1IVPROC glUniform1iv;
    PFNGLUNIFORM4FVPROC glUniform4fv;
    PFNGLUNIFORM4IVPROC glUniform4iv;
    PFNGLUSEPROGRAMPROC glUseProgram;
    PFNGLVERTEXATTRIBPOINTERPROC glVertexAttribPointer;
    PFNGLDELETEOBJECTARBPROC glDeleteObjectARB;
    PFNGLATTACHOBJECTARBPROC glAttachObjectARB;
    PFNGLCOMPILESHADERARBPROC glCompileShaderARB;
    PFNGLCREATEPROGRAMOBJECTARBPROC glCreateProgramObjectARB;
    PFNGLCREATESHADEROBJECTARBPROC glCreateShaderObjectARB;
    PFNGLDISABLEVERTEXATTRIBARRAYARBPROC glDisableVertexAttribArrayARB;
    PFNGLENABLEVERTEXATTRIBARRAYARBPROC glEnableVertexAttribArrayARB;
    PFNGLGETATTRIBLOCATIONARBPROC glGetAttribLocationARB;
    PFNGLGETINFOLOGARBPROC glGetInfoLogARB;
    PFNGLGETOBJECTPARAMETERIVARBPROC glGetObjectParameterivARB;
    PFNGLGETUNIFORMLOCATIONARBPROC glGetUniformLocationARB;
    PFNGLLINKPROGRAMARBPROC glLinkProgramARB;
    PFNGLSHADERSOURCEARBPROC glShaderSourceARB;
    PFNGLUNIFORM1IARBPROC glUniform1iARB;
    PFNGLUNIFORM1IVARBPROC glUniform1ivARB;
    PFNGLUNIFORM4FVARBPROC glUniform4fvARB;
    PFNGLUNIFORM4IVARBPROC glUniform4ivARB;
    PFNGLUSEPROGRAMOBJECTARBPROC glUseProgramObjectARB;
    PFNGLVERTEXATTRIBPOINTERARBPROC glVertexAttribPointerARB;
    PFNGLGETPROGRAMIVARBPROC glGetProgramivARB;
    PFNGLPROGRAMLOCALPARAMETER4FVARBPROC glProgramLocalParameter4fvARB;
    PFNGLPROGRAMLOCALPARAMETERI4IVNVPROC glProgramLocalParameterI4ivNV;
    PFNGLDELETEPROGRAMSARBPROC glDeleteProgramsARB;
    PFNGLGENPROGRAMSARBPROC glGenProgramsARB;
    PFNGLBINDPROGRAMARBPROC glBindProgramARB;
    PFNGLPROGRAMSTRINGARBPROC glProgramStringARB;

    // interface for profile-specific things.
    int (*profileMaxUniforms)(MOJOSHADER_shaderType shader_type);
    int (*profileCompileShader)(const MOJOSHADER_parseData *pd, GLuint *s);
    void (*profileDeleteShader)(const GLuint shader);
    void (*profileDeleteProgram)(const GLuint program);
    GLint (*profileGetAttribLocation)(MOJOSHADER_glProgram *program, int idx);
    GLint (*profileGetUniformLocation)(MOJOSHADER_glProgram *program, MOJOSHADER_glShader *shader, int idx);
    GLint (*profileGetSamplerLocation)(MOJOSHADER_glProgram *, MOJOSHADER_glShader *, int);
    GLuint (*profileLinkProgram)(MOJOSHADER_glShader *, MOJOSHADER_glShader *);
    void (*profileFinalInitProgram)(MOJOSHADER_glProgram *program);
    void (*profileUseProgram)(MOJOSHADER_glProgram *program);
    void (*profilePushConstantArray)(MOJOSHADER_glProgram *, const MOJOSHADER_uniform *, const GLfloat *);
    void (*profilePushUniforms)(void);
    void (*profilePushSampler)(GLint loc, GLuint sampler);
    int (*profileMustPushConstantArrays)(void);
    int (*profileMustPushSamplers)(void);
};


static MOJOSHADER_glContext *ctx = NULL;

// Error state...
static char error_buffer[1024] = { '\0' };

static void set_error(const char *str)
{
    snprintf(error_buffer, sizeof (error_buffer), "%s", str);
} // set_error

#if PLATFORM_MACOSX
static inline int macosx_version_atleast(int x, int y, int z)
{
    static int checked = 0;
    static int combined = 0;

    if (!checked)
    {
        SInt32 ver, major, minor, patch;
        int convert = 0;

        if (Gestalt(gestaltSystemVersion, &ver) != noErr)
            ver = 0x1000;  // oh well.
        else if (ver < 0x1030)
            convert = 1;  // split (ver) into (major),(minor),(patch).
        else
        {
            // presumably this won't fail. But if it does, we'll just use the
            //  original version value. This might cut the value--10.12.11 will
            //  come out to 10.9.9, for example--but it's better than nothing.
            if (Gestalt(gestaltSystemVersionMajor, &major) != noErr)
                convert = 1;
            else if (Gestalt(gestaltSystemVersionMinor, &minor) != noErr)
                convert = 1;
            else if (Gestalt(gestaltSystemVersionBugFix, &patch) != noErr)
                convert = 1;
        } // else

        if (convert)
        {
            major = ((ver & 0xFF00) >> 8);
            major = (((major / 16) * 10) + (major % 16));
            minor = ((ver & 0xF0) >> 4);
            patch = (ver & 0xF);
        } // if

        combined = (major << 16) | (minor << 8) | patch;
        checked = 1;
    } // if

    return (combined >= ((x << 16) | (y << 8) | z));
} // macosx_version_atleast
#endif

static inline void out_of_memory(void)
{
    set_error("out of memory");
} // out_of_memory

static inline void *Malloc(const size_t len)
{
    void *retval = ctx->malloc_fn((int) len, ctx->malloc_data);
    if (retval == NULL)
        out_of_memory();
    return retval;
} // Malloc

static inline void Free(void *ptr)
{
    if (ptr != NULL)
        ctx->free_fn(ptr, ctx->malloc_data);
} // Free


static inline void toggle_gl_state(GLenum state, int val)
{
    if (val)
        ctx->glEnable(state);
    else
        ctx->glDisable(state);
} // toggle_gl_state


// profile-specific implementations...

#if SUPPORT_PROFILE_GLSL
static inline GLenum glsl_shader_type(const MOJOSHADER_shaderType t)
{
    // these enums match between core 2.0 and the ARB extensions.
    if (t == MOJOSHADER_TYPE_VERTEX)
        return GL_VERTEX_SHADER;
    else if (t == MOJOSHADER_TYPE_PIXEL)
        return GL_FRAGMENT_SHADER;

    // !!! FIXME: geometry shaders?
    return GL_NONE;
} // glsl_shader_type


static int impl_GLSL_MustPushConstantArrays(void) { return 1; }
static int impl_GLSL_MustPushSamplers(void) { return 1; }

static int impl_GLSL_MaxUniforms(MOJOSHADER_shaderType shader_type)
{
    // these enums match between core 2.0 and the ARB extensions.
    GLenum pname = GL_NONE;
    GLint val = 0;
    if (shader_type == MOJOSHADER_TYPE_VERTEX)
        pname = GL_MAX_VERTEX_UNIFORM_COMPONENTS;
    else if (shader_type == MOJOSHADER_TYPE_PIXEL)
        pname = GL_MAX_FRAGMENT_UNIFORM_COMPONENTS;
    else
        return -1;

    ctx->glGetIntegerv(pname, &val);
    return (int) val;
} // impl_GLSL_MaxUniforms


static int impl_GLSL_CompileShader(const MOJOSHADER_parseData *pd, GLuint *s)
{
    GLint ok = 0;
    const GLint codelen = (GLint) pd->output_len;
    const GLenum shader_type = glsl_shader_type(pd->shader_type);

    if (ctx->have_opengl_2)
    {
        const GLuint shader = ctx->glCreateShader(shader_type);
        ctx->glShaderSource(shader, 1, (const GLchar**) &pd->output, &codelen);
        ctx->glCompileShader(shader);
        ctx->glGetShaderiv(shader, GL_COMPILE_STATUS, &ok);
        if (!ok)
        {
            GLsizei len = 0;
            ctx->glGetInfoLogARB(shader, sizeof (error_buffer), &len,
                                 (GLchar *) error_buffer);
            *s = 0;
            return 0;
        } // if

        *s = shader;
    } // if
    else
    {
        const GLhandleARB shader = ctx->glCreateShaderObjectARB(shader_type);
        assert(sizeof (shader) == sizeof (*s));  // not always true on OS X!
        ctx->glShaderSourceARB(shader, 1,
                              (const GLcharARB **) &pd->output, &codelen);
        ctx->glCompileShaderARB(shader);
        ctx->glGetObjectParameterivARB(shader,GL_OBJECT_COMPILE_STATUS_ARB,&ok);
        if (!ok)
        {
            GLsizei len = 0;
            ctx->glGetInfoLogARB(shader, sizeof (error_buffer), &len,
                                 (GLcharARB *) error_buffer);
            *s = 0;
            return 0;
        } // if

        *s = (GLuint) shader;
    } // else

    return 1;
} // impl_GLSL_CompileShader


static void impl_GLSL_DeleteShader(const GLuint shader)
{
    if (ctx->have_opengl_2)
        ctx->glDeleteShader(shader);
    else
        ctx->glDeleteObjectARB((GLhandleARB) shader);
} // impl_GLSL_DeleteShader


static void impl_GLSL_DeleteProgram(const GLuint program)
{
    if (ctx->have_opengl_2)
        ctx->glDeleteProgram(program);
    else
        ctx->glDeleteObjectARB((GLhandleARB) program);
} // impl_GLSL_DeleteProgram


static GLint impl_GLSL_GetUniformLocation(MOJOSHADER_glProgram *program,
                                          MOJOSHADER_glShader *shader, int idx)
{
    return 0;  // no-op, we push this as one big-ass array now.
} // impl_GLSL_GetUniformLocation


static inline GLint glsl_uniform_loc(MOJOSHADER_glProgram *program,
                                          const char *name)
{
    return ctx->have_opengl_2 ?
        ctx->glGetUniformLocation(program->handle, name) :
        ctx->glGetUniformLocationARB((GLhandleARB) program->handle, name);
} // glsl_uniform_loc


static GLint impl_GLSL_GetSamplerLocation(MOJOSHADER_glProgram *program,
                                          MOJOSHADER_glShader *shader, int idx)
{
    return glsl_uniform_loc(program, shader->parseData->samplers[idx].name);
} // impl_GLSL_GetSamplerLocation


static GLint impl_GLSL_GetAttribLocation(MOJOSHADER_glProgram *program, int idx)
{
    const MOJOSHADER_parseData *pd = program->vertex->parseData;
    const MOJOSHADER_attribute *a = pd->attributes;

    if (ctx->have_opengl_2)
    {
        return ctx->glGetAttribLocation(program->handle,
                                        (const GLchar *) a[idx].name);
    } // if

    return ctx->glGetAttribLocationARB((GLhandleARB) program->handle,
                                        (const GLcharARB *) a[idx].name);
} // impl_GLSL_GetAttribLocation


static GLuint impl_GLSL_LinkProgram(MOJOSHADER_glShader *vshader,
                                    MOJOSHADER_glShader *pshader)
{
    GLint ok = 0;

    if (ctx->have_opengl_2)
    {
        const GLuint program = ctx->glCreateProgram();

        if (vshader != NULL) ctx->glAttachShader(program, vshader->handle);
        if (pshader != NULL) ctx->glAttachShader(program, pshader->handle);

        ctx->glLinkProgram(program);

        ctx->glGetProgramiv(program, GL_LINK_STATUS, &ok);
        if (!ok)
        {
            GLsizei len = 0;
            ctx->glGetProgramInfoLog(program, sizeof (error_buffer),
                                     &len, (GLchar *) error_buffer);
            ctx->glDeleteProgram(program);
            return 0;
        } // if

        return program;
    } // if
    else
    {
        const GLhandleARB program = ctx->glCreateProgramObjectARB();
        assert(sizeof(program) == sizeof(GLuint));  // not always true on OS X!

        if (vshader != NULL)
            ctx->glAttachObjectARB(program, (GLhandleARB) vshader->handle);

        if (pshader != NULL)
            ctx->glAttachObjectARB(program, (GLhandleARB) pshader->handle);

        ctx->glLinkProgramARB(program);

        ctx->glGetObjectParameterivARB(program, GL_OBJECT_LINK_STATUS_ARB, &ok);
        if (!ok)
        {
            GLsizei len = 0;
            ctx->glGetInfoLogARB(program, sizeof (error_buffer),
                                 &len, (GLcharARB *) error_buffer);
            ctx->glDeleteObjectARB(program);
            return 0;
        } // if

        return (GLuint) program;
    } // else
} // impl_GLSL_LinkProgram

static void impl_GLSL_FinalInitProgram(MOJOSHADER_glProgram *program)
{
    
    program->vs_float4_loc = glsl_uniform_loc(program, "vs_uniforms_vec4");
    program->vs_int4_loc = glsl_uniform_loc(program, "vs_uniforms_ivec4");
    program->vs_bool_loc = glsl_uniform_loc(program, "vs_uniforms_bool");
    program->ps_float4_loc = glsl_uniform_loc(program, "ps_uniforms_vec4");
    program->ps_int4_loc = glsl_uniform_loc(program, "ps_uniforms_ivec4");
    program->ps_bool_loc = glsl_uniform_loc(program, "ps_uniforms_bool");
} // impl_GLSL_FinalInitProgram


static void impl_GLSL_UseProgram(MOJOSHADER_glProgram *program)
{
    if (ctx->have_opengl_2)
        ctx->glUseProgram(program ? program->handle : 0);
    else
        ctx->glUseProgramObjectARB((GLhandleARB) (program ? program->handle : 0));
} // impl_GLSL_UseProgram


static void impl_GLSL_PushConstantArray(MOJOSHADER_glProgram *program,
                                        const MOJOSHADER_uniform *u,
                                        const GLfloat *f)
{
    const GLint loc = glsl_uniform_loc(program, u->name);
    if (loc >= 0)   // not optimized out?
        ctx->glUniform4fv(loc, u->array_count, f);
} // impl_GLSL_PushConstantArray


static void impl_GLSL_PushUniforms(void)
{
    const MOJOSHADER_glProgram *program = ctx->bound_program;

    assert(program->uniform_count > 0);  // don't call with nothing to do!

    if (program->vs_float4_loc != -1)
    {
        ctx->glUniform4fv(program->vs_float4_loc,
                          program->vs_uniforms_float4_count,
                          program->vs_uniforms_float4);
    } // if

    if (program->vs_int4_loc != -1)
    {
        ctx->glUniform4iv(program->vs_int4_loc,
                          program->vs_uniforms_int4_count,
                          program->vs_uniforms_int4);
    } // if

    if (program->vs_bool_loc != -1)
    {
        ctx->glUniform1iv(program->vs_bool_loc,
                          program->vs_uniforms_bool_count,
                          program->vs_uniforms_bool);
    } // if

    if (program->ps_float4_loc != -1)
    {
        ctx->glUniform4fv(program->ps_float4_loc,
                          program->ps_uniforms_float4_count,
                          program->ps_uniforms_float4);
    } // if

    if (program->ps_int4_loc != -1)
    {
        ctx->glUniform4iv(program->ps_int4_loc,
                          program->ps_uniforms_int4_count,
                          program->ps_uniforms_int4);
    } // if

    if (program->ps_bool_loc != -1)
    {
        ctx->glUniform1iv(program->ps_bool_loc,
                          program->ps_uniforms_bool_count,
                          program->ps_uniforms_bool);
    } // if
} // impl_GLSL_PushUniforms


static void impl_GLSL_PushSampler(GLint loc, GLuint sampler)
{
    ctx->glUniform1i(loc, sampler);
} // impl_GLSL_PushSampler

#endif  // SUPPORT_PROFILE_GLSL


#if SUPPORT_PROFILE_ARB1
static inline GLenum arb1_shader_type(const MOJOSHADER_shaderType t)
{
    if (t == MOJOSHADER_TYPE_VERTEX)
        return GL_VERTEX_PROGRAM_ARB;
    else if (t == MOJOSHADER_TYPE_PIXEL)
        return GL_FRAGMENT_PROGRAM_ARB;

    // !!! FIXME: geometry shaders?
    return GL_NONE;
} // arb1_shader_type

static int impl_ARB1_MustPushConstantArrays(void) { return 0; }
static int impl_ARB1_MustPushSamplers(void) { return 0; }

static int impl_ARB1_MaxUniforms(MOJOSHADER_shaderType shader_type)
{
    GLint retval = 0;
    const GLenum program_type = arb1_shader_type(shader_type);
    if (program_type == GL_NONE)
        return -1;

    ctx->glGetProgramivARB(program_type, GL_MAX_PROGRAM_PARAMETERS_ARB, &retval);
    return (int) retval;  // !!! FIXME: times four?
} // impl_ARB1_MaxUniforms


static int impl_ARB1_CompileShader(const MOJOSHADER_parseData *pd, GLuint *s)
{
    GLint shaderlen = (GLint) pd->output_len;
    const GLenum shader_type = arb1_shader_type(pd->shader_type);
    GLuint shader = 0;
    ctx->glGenProgramsARB(1, &shader);

    ctx->glGetError();  // flush any existing error state.
    ctx->glBindProgramARB(shader_type, shader);
    ctx->glProgramStringARB(shader_type, GL_PROGRAM_FORMAT_ASCII_ARB,
                                shaderlen, pd->output);

    if (ctx->glGetError() == GL_INVALID_OPERATION)
    { 
        GLint pos = 0;
        ctx->glGetIntegerv(GL_PROGRAM_ERROR_POSITION_ARB, &pos);
        const GLubyte *errstr = ctx->glGetString(GL_PROGRAM_ERROR_STRING_ARB);
        snprintf(error_buffer, sizeof (error_buffer),
                  "ARB1 compile error at position %d: %s",
                  (int) pos, (const char *) errstr);
        ctx->glBindProgramARB(shader_type, 0);
        ctx->glDeleteProgramsARB(1, &shader);
        *s = 0;
        return 0;
    } // if

    *s = shader;
    return 1;
} // impl_ARB1_CompileShader


static void impl_ARB1_DeleteShader(const GLuint _shader)
{
    GLuint shader = _shader;  // const removal.
    ctx->glDeleteProgramsARB(1, &shader);
} // impl_ARB1_DeleteShader


static void impl_ARB1_DeleteProgram(const GLuint program)
{
    // no-op. ARB1 doesn't have real linked programs.
} // impl_GLSL_DeleteProgram

static GLint impl_ARB1_GetUniformLocation(MOJOSHADER_glProgram *program,
                                          MOJOSHADER_glShader *shader, int idx)
{
    return 0;  // no-op, we push this as one big-ass array now.
} // impl_ARB1_GetUniformLocation

static GLint impl_ARB1_GetSamplerLocation(MOJOSHADER_glProgram *program,
                                          MOJOSHADER_glShader *shader, int idx)
{
    return shader->parseData->samplers[idx].index;
} // impl_ARB1_GetSamplerLocation


static GLint impl_ARB1_GetAttribLocation(MOJOSHADER_glProgram *program, int idx)
{
    return idx;  // map to vertex arrays in the same order as the parseData.
} // impl_ARB1_GetAttribLocation


static GLuint impl_ARB1_LinkProgram(MOJOSHADER_glShader *vshader,
                                    MOJOSHADER_glShader *pshader)
{
    // there is no formal linking in ARB1...just return a unique value.
    static GLuint retval = 1;
    return retval++;
} // impl_ARB1_LinkProgram


static void impl_ARB1_FinalInitProgram(MOJOSHADER_glProgram *program)
{
    // no-op.
} // impl_ARB1_FinalInitProgram


static void impl_ARB1_UseProgram(MOJOSHADER_glProgram *program)
{
    GLuint vhandle = 0;
    GLuint phandle = 0;
    if (program != NULL)
    {
        if (program->vertex != NULL)
            vhandle = program->vertex->handle;
        if (program->fragment != NULL)
            phandle = program->fragment->handle;
    } // if

    toggle_gl_state(GL_VERTEX_PROGRAM_ARB, vhandle != 0);
    toggle_gl_state(GL_FRAGMENT_PROGRAM_ARB, phandle != 0);

    ctx->glBindProgramARB(GL_VERTEX_PROGRAM_ARB, vhandle);
    ctx->glBindProgramARB(GL_FRAGMENT_PROGRAM_ARB, phandle);
} // impl_ARB1_UseProgram


static void impl_ARB1_PushConstantArray(MOJOSHADER_glProgram *program,
                                        const MOJOSHADER_uniform *u,
                                        const GLfloat *f)
{
    // no-op. Constant arrays are defined in source code for arb1.
} // impl_ARB1_PushConstantArray


static void impl_ARB1_PushUniforms(void)
{
    // vertex shader uniforms come first in program->uniforms array.
    MOJOSHADER_shaderType shader_type = MOJOSHADER_TYPE_VERTEX;
    GLenum arb_shader_type = arb1_shader_type(shader_type);
    const MOJOSHADER_glProgram *program = ctx->bound_program;
    const uint32 count = program->uniform_count;
    const GLfloat *srcf = program->vs_uniforms_float4;
    const GLint *srci = program->vs_uniforms_int4;
    const GLint *srcb = program->vs_uniforms_bool;
    GLint loc = 0;
    uint32 i;

    assert(count > 0);  // shouldn't call this with nothing to do!

    for (i = 0; i < count; i++)
    {
        UniformMap *map = &program->uniforms[i];
        const MOJOSHADER_shaderType uniform_shader_type = map->shader_type;
        const MOJOSHADER_uniform *u = map->uniform;
        const MOJOSHADER_uniformType type = u->type;
        const int size = u->array_count ? u->array_count : 1;

        assert(!u->constant);

        // Did we switch from vertex to pixel (to geometry, etc)?
        if (shader_type != uniform_shader_type)
        {
            // we start with vertex, move to pixel, then to geometry, etc.
            //  The array should always be sorted as such.
            if (uniform_shader_type == MOJOSHADER_TYPE_PIXEL)
            {
                assert(shader_type == MOJOSHADER_TYPE_VERTEX);
                srcf = program->ps_uniforms_float4;
                srci = program->ps_uniforms_int4;
                srcb = program->ps_uniforms_bool;
                loc = 0;
            } // if
            else
            {
                // These should be ordered vertex, then pixel, then geometry.
                assert(0 && "Unexpected shader type");
            } // else

            shader_type = uniform_shader_type;
            arb_shader_type = arb1_shader_type(uniform_shader_type);
        } // if

        if (type == MOJOSHADER_UNIFORM_FLOAT)
        {
            int i;
            for (i = 0; i < size; i++, srcf += 4, loc++)
                ctx->glProgramLocalParameter4fvARB(arb_shader_type, loc, srcf);
        } // if
        else if (type == MOJOSHADER_UNIFORM_INT)
        {
            int i;
            if (ctx->have_GL_NV_gpu_program4)
            {
                // GL_NV_gpu_program4 has integer uniform loading support.
                for (i = 0; i < size; i++, srci += 4, loc++)
                    ctx->glProgramLocalParameterI4ivNV(arb_shader_type, loc, srci);
            } // if
            else
            {
                for (i = 0; i < size; i++, srci += 4, loc++)
                {
                    const GLfloat fv[4] = {
                        (GLfloat) srci[0], (GLfloat) srci[1],
                        (GLfloat) srci[2], (GLfloat) srci[3]
                    };
                    ctx->glProgramLocalParameter4fvARB(arb_shader_type, loc, fv);
                } // for
            } // else
        } // else if
        else if (type == MOJOSHADER_UNIFORM_BOOL)
        {
            int i;
            if (ctx->have_GL_NV_gpu_program4)
            {
                // GL_NV_gpu_program4 has integer uniform loading support.
                for (i = 0; i < size; i++, srcb++, loc++)
                {
                    const GLint ib = (GLint) ((*srcb) ? 1 : 0);
                    const GLint iv[4] = { ib, ib, ib, ib };
                    ctx->glProgramLocalParameterI4ivNV(arb_shader_type, loc, iv);
                } // for
            } // if
            else
            {
                for (i = 0; i < size; i++, srcb++, loc++)
                {
                    const GLfloat fb = (GLfloat) ((*srcb) ? 1.0f : 0.0f);
                    const GLfloat fv[4] = { fb, fb, fb, fb };
                    ctx->glProgramLocalParameter4fvARB(arb_shader_type, loc, fv);
                } // for
            } // else
        } // else if
    } // for
} // impl_ARB1_PushUniforms

static void impl_ARB1_PushSampler(GLint loc, GLuint sampler)
{
    // no-op in this profile...arb1 uses the texture units as-is.
    assert(loc == (GLint) sampler);
} // impl_ARB1_PushSampler

#endif  // SUPPORT_PROFILE_ARB1


const char *MOJOSHADER_glGetError(void)
{
    return error_buffer;
} // MOJOSHADER_glGetError


static void *loadsym(MOJOSHADER_glGetProcAddress lookup, void *d,
                     const char *fn, int *ext)
{
    void *retval = NULL;
    if (lookup != NULL)
        retval = lookup(fn, d);

    if (retval == NULL)
        *ext = 0;

    return retval;
} // loadsym

static void lookup_entry_points(MOJOSHADER_glGetProcAddress lookup, void *d)
{
    #define DO_LOOKUP(ext, typ, fn) { \
        ctx->fn = (typ) loadsym(lookup, d, #fn, &ctx->have_##ext); \
    }

    DO_LOOKUP(core_opengl, PFNGLGETSTRINGPROC, glGetString);
    DO_LOOKUP(core_opengl, PFNGLGETERRORPROC, glGetError);
    DO_LOOKUP(core_opengl, PFNGLGETINTEGERVPROC, glGetIntegerv);
    DO_LOOKUP(core_opengl, PFNGLENABLEPROC, glEnable);
    DO_LOOKUP(core_opengl, PFNGLDISABLEPROC, glDisable);
    DO_LOOKUP(opengl_2, PFNGLDELETESHADERPROC, glDeleteShader);
    DO_LOOKUP(opengl_2, PFNGLDELETEPROGRAMPROC, glDeleteProgram);
    DO_LOOKUP(opengl_2, PFNGLATTACHSHADERPROC, glAttachShader);
    DO_LOOKUP(opengl_2, PFNGLCOMPILESHADERPROC, glCompileShader);
    DO_LOOKUP(opengl_2, PFNGLCREATESHADERPROC, glCreateShader);
    DO_LOOKUP(opengl_2, PFNGLCREATEPROGRAMPROC, glCreateProgram);
    DO_LOOKUP(opengl_2, PFNGLDISABLEVERTEXATTRIBARRAYPROC, glDisableVertexAttribArray);
    DO_LOOKUP(opengl_2, PFNGLENABLEVERTEXATTRIBARRAYPROC, glEnableVertexAttribArray);
    DO_LOOKUP(opengl_2, PFNGLGETATTRIBLOCATIONPROC, glGetAttribLocation);
    DO_LOOKUP(opengl_2, PFNGLGETPROGRAMINFOLOGPROC, glGetProgramInfoLog);
    DO_LOOKUP(opengl_2, PFNGLGETSHADERIVPROC, glGetShaderiv);
    DO_LOOKUP(opengl_2, PFNGLGETPROGRAMIVPROC, glGetProgramiv);
    DO_LOOKUP(opengl_2, PFNGLGETUNIFORMLOCATIONPROC, glGetUniformLocation);
    DO_LOOKUP(opengl_2, PFNGLLINKPROGRAMPROC, glLinkProgram);
    DO_LOOKUP(opengl_2, PFNGLSHADERSOURCEPROC, glShaderSource);
    DO_LOOKUP(opengl_2, PFNGLUNIFORM1IPROC, glUniform1i);
    DO_LOOKUP(opengl_2, PFNGLUNIFORM1IVPROC, glUniform1iv);
    DO_LOOKUP(opengl_2, PFNGLUNIFORM4FVPROC, glUniform4fv);
    DO_LOOKUP(opengl_2, PFNGLUNIFORM4IVPROC, glUniform4iv);
    DO_LOOKUP(opengl_2, PFNGLUSEPROGRAMPROC, glUseProgram);
    DO_LOOKUP(opengl_2, PFNGLVERTEXATTRIBPOINTERPROC, glVertexAttribPointer);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLDELETEOBJECTARBPROC, glDeleteObjectARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLATTACHOBJECTARBPROC, glAttachObjectARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLCOMPILESHADERARBPROC, glCompileShaderARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLCREATEPROGRAMOBJECTARBPROC, glCreateProgramObjectARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLCREATESHADEROBJECTARBPROC, glCreateShaderObjectARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLGETINFOLOGARBPROC, glGetInfoLogARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLGETOBJECTPARAMETERIVARBPROC, glGetObjectParameterivARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLGETUNIFORMLOCATIONARBPROC, glGetUniformLocationARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLLINKPROGRAMARBPROC, glLinkProgramARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLSHADERSOURCEARBPROC, glShaderSourceARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLUNIFORM1IARBPROC, glUniform1iARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLUNIFORM1IVARBPROC, glUniform1ivARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLUNIFORM4FVARBPROC, glUniform4fvARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLUNIFORM4IVARBPROC, glUniform4ivARB);
    DO_LOOKUP(GL_ARB_shader_objects, PFNGLUSEPROGRAMOBJECTARBPROC, glUseProgramObjectARB);
    DO_LOOKUP(GL_ARB_vertex_shader, PFNGLDISABLEVERTEXATTRIBARRAYARBPROC, glDisableVertexAttribArrayARB);
    DO_LOOKUP(GL_ARB_vertex_shader, PFNGLENABLEVERTEXATTRIBARRAYARBPROC, glEnableVertexAttribArrayARB);
    DO_LOOKUP(GL_ARB_vertex_shader, PFNGLGETATTRIBLOCATIONARBPROC, glGetAttribLocationARB);
    DO_LOOKUP(GL_ARB_vertex_shader, PFNGLVERTEXATTRIBPOINTERARBPROC, glVertexAttribPointerARB);
    DO_LOOKUP(GL_ARB_vertex_program, PFNGLVERTEXATTRIBPOINTERARBPROC, glVertexAttribPointerARB);
    DO_LOOKUP(GL_ARB_vertex_program, PFNGLGETPROGRAMIVARBPROC, glGetProgramivARB);
    DO_LOOKUP(GL_ARB_vertex_program, PFNGLPROGRAMLOCALPARAMETER4FVARBPROC, glProgramLocalParameter4fvARB);
    DO_LOOKUP(GL_ARB_vertex_program, PFNGLDELETEPROGRAMSARBPROC, glDeleteProgramsARB);
    DO_LOOKUP(GL_ARB_vertex_program, PFNGLGENPROGRAMSARBPROC, glGenProgramsARB);
    DO_LOOKUP(GL_ARB_vertex_program, PFNGLBINDPROGRAMARBPROC, glBindProgramARB);
    DO_LOOKUP(GL_ARB_vertex_program, PFNGLPROGRAMSTRINGARBPROC, glProgramStringARB);
    DO_LOOKUP(GL_NV_gpu_program4, PFNGLPROGRAMLOCALPARAMETERI4IVNVPROC, glProgramLocalParameterI4ivNV);

    #undef DO_LOOKUP
} // lookup_entry_points

static inline int opengl_version_atleast(const int major, const int minor)
{
    return ( ((ctx->opengl_major << 16) | (ctx->opengl_minor & 0xFFFF)) >=
             ((major << 16) | (minor & 0xFFFF)) );
} // opengl_version_atleast

static int verify_extension(const char *ext, int have, const char *extlist,
                            int major, int minor)
{
    if (have == 0)
        return 0;  // don't bother checking, we're missing an entry point.

    else if (!ctx->have_core_opengl)
        return 0;  // don't bother checking, we're missing basic functionality.

    // See if it's in the spec for this GL implementation's version.
    if ((major > 0) && (opengl_version_atleast(major, minor)))
        return 1;

    // Not available in the GL version, check the extension list.
    const char *ptr = strstr(extlist, ext);
    if (ptr == NULL)
        return 0;

    const char endchar = ptr[strlen(ext)];
    if ((endchar == '\0') || (endchar == ' '))
        return 1;  // extension is in the list.

    return 0;  // just not supported, fail.
} // verify_extension


static void parse_opengl_version_str(const char *verstr, int *maj, int *min)
{
    if (verstr == NULL)
        *maj = *min = 0;
    else
        sscanf(verstr, "%d.%d", maj, min);
} // parse_opengl_version_str


#if SUPPORT_PROFILE_GLSL
static inline int glsl_version_atleast(const int major, const int minor)
{
    return ( ((ctx->glsl_major << 16) | (ctx->glsl_minor & 0xFFFF)) >=
             ((major << 16) | (minor & 0xFFFF)) );
} // glsl_version_atleast
#endif

static void detect_glsl_version(void)
{
    ctx->glsl_major = ctx->glsl_minor = 0;

#if SUPPORT_PROFILE_GLSL
    if (!ctx->have_core_opengl)
        return;  // everything's busted, give up.

    #if PLATFORM_MACOSX
    // If running on Mac OS X <= 10.4, don't ever use GLSL, even if
    //  the system claims it is available.
    if (!macosx_version_atleast(10, 5, 0))
        return;
    #endif

    if ( ctx->have_opengl_2 ||
         ( ctx->have_GL_ARB_shader_objects &&
           ctx->have_GL_ARB_vertex_shader &&
           ctx->have_GL_ARB_fragment_shader &&
           ctx->have_GL_ARB_shading_language_100 ) )
    {
        // the GL2.0 and ARB enum is the same value.
        const GLenum enumval = GL_SHADING_LANGUAGE_VERSION;
        ctx->glGetError();  // flush any existing error state.
        const char *str = (const char *) ctx->glGetString(enumval);
        if (ctx->glGetError() == GL_INVALID_ENUM)
            str = NULL;
        parse_opengl_version_str(str, &ctx->glsl_major, &ctx->glsl_minor);
    } // if
#endif
} // detect_glsl_version


static void load_extensions(MOJOSHADER_glGetProcAddress lookup, void *d)
{
    const char *extlist = NULL;

    ctx->have_core_opengl = 1;
    ctx->have_opengl_2 = 1;
    ctx->have_GL_ARB_vertex_program = 1;
    ctx->have_GL_ARB_fragment_program = 1;
    ctx->have_GL_NV_vertex_program2_option = 1;
    ctx->have_GL_NV_fragment_program2 = 1;
    ctx->have_GL_NV_vertex_program3 = 1;
    ctx->have_GL_NV_gpu_program4 = 1;
    ctx->have_GL_ARB_shader_objects = 1;
    ctx->have_GL_ARB_vertex_shader = 1;
    ctx->have_GL_ARB_fragment_shader = 1;
    ctx->have_GL_ARB_shading_language_100 = 1;
    ctx->have_GL_NV_half_float = 1;
    ctx->have_GL_ARB_half_float_vertex = 1;
    ctx->have_GL_OES_vertex_half_float = 1;

    lookup_entry_points(lookup, d);

    if (!ctx->have_core_opengl)
        set_error("missing basic OpenGL entry points");
    else
    {
        const char *str = (const char *) ctx->glGetString(GL_VERSION);
        parse_opengl_version_str(str, &ctx->opengl_major, &ctx->opengl_minor);
        extlist = (const char *) ctx->glGetString(GL_EXTENSIONS);
    } // else

    if (extlist == NULL)
        extlist = "";  // just in case.

    if ((ctx->have_opengl_2) && (!opengl_version_atleast(2, 0)))
    {
        ctx->have_opengl_2 = 0;  // Not GL2! must have the ARB extensions!

        // Force compatible ARB function pointers in...this keeps the code
        //  cleaner when they are identical, so we don't have to if/else
        //  every function call, but we definitely have the right entry
        //  point. Be careful what you add here.
        // These may be NULL, btw.
        ctx->glUniform1i = ctx->glUniform1iARB;
        ctx->glUniform4fv = ctx->glUniform4fvARB;
        ctx->glUniform4iv = ctx->glUniform4ivARB;
        ctx->glDisableVertexAttribArray = ctx->glDisableVertexAttribArrayARB;
        ctx->glEnableVertexAttribArray = ctx->glEnableVertexAttribArrayARB;
        ctx->glVertexAttribPointer = ctx->glVertexAttribPointerARB;
    } // if

    #define VERIFY_EXT(ext, major, minor) \
        ctx->have_##ext = verify_extension(#ext, ctx->have_##ext, extlist, major, minor)

    VERIFY_EXT(GL_ARB_vertex_program, -1, -1);
    VERIFY_EXT(GL_ARB_fragment_program, -1, -1);
    VERIFY_EXT(GL_ARB_shader_objects, -1, -1);
    VERIFY_EXT(GL_ARB_vertex_shader, -1, -1);
    VERIFY_EXT(GL_ARB_fragment_shader, -1, -1);
    VERIFY_EXT(GL_ARB_shading_language_100, -1, -1);
    VERIFY_EXT(GL_NV_vertex_program2_option, -1, -1);
    VERIFY_EXT(GL_NV_fragment_program2, -1, -1);
    VERIFY_EXT(GL_NV_vertex_program3, -1, -1);
    VERIFY_EXT(GL_NV_half_float, -1, -1);
    VERIFY_EXT(GL_ARB_half_float_vertex, 3, 0);
    VERIFY_EXT(GL_OES_vertex_half_float, -1, -1);

    #undef VERIFY_EXT

    detect_glsl_version();
} // load_extensions


static int valid_profile(const char *profile)
{
    if (!ctx->have_core_opengl)
        return 0;

    #define MUST_HAVE(p, x) \
        if (!ctx->have_##x) { set_error(#p " profile needs " #x); return 0; }

    // we might actually _have_ maj.min, but forcibly disabled GLSL elsewhere.
    #define MUST_HAVE_GLSL(p, maj, min) \
        if (!glsl_version_atleast(maj, min)) { \
            set_error(#p " profile needs missing GLSL support"); return 0; \
        }

    if (profile == NULL)
    {
        set_error("NULL profile");
        return 0;
    } // if

    #if SUPPORT_PROFILE_ARB1
    else if (strcmp(profile, MOJOSHADER_PROFILE_ARB1) == 0)
    {
        MUST_HAVE(MOJOSHADER_PROFILE_ARB1, GL_ARB_vertex_program);
        MUST_HAVE(MOJOSHADER_PROFILE_ARB1, GL_ARB_fragment_program);
    } // else if
    #endif

    #if SUPPORT_PROFILE_ARB1_NV
    else if (strcmp(profile, MOJOSHADER_PROFILE_NV2) == 0)
    {
        MUST_HAVE(MOJOSHADER_PROFILE_NV2, GL_ARB_vertex_program);
        MUST_HAVE(MOJOSHADER_PROFILE_NV2, GL_ARB_fragment_program);
        MUST_HAVE(MOJOSHADER_PROFILE_NV2, GL_NV_vertex_program2_option);
        MUST_HAVE(MOJOSHADER_PROFILE_NV2, GL_NV_fragment_program2);
    } // else if

    else if (strcmp(profile, MOJOSHADER_PROFILE_NV3) == 0)
    {
        MUST_HAVE(MOJOSHADER_PROFILE_NV3, GL_ARB_vertex_program);
        MUST_HAVE(MOJOSHADER_PROFILE_NV3, GL_ARB_fragment_program);
        MUST_HAVE(MOJOSHADER_PROFILE_NV3, GL_NV_vertex_program3);
        MUST_HAVE(MOJOSHADER_PROFILE_NV3, GL_NV_fragment_program2);
    } // else if

    else if (strcmp(profile, MOJOSHADER_PROFILE_NV4) == 0)
    {
        MUST_HAVE(MOJOSHADER_PROFILE_NV4, GL_NV_gpu_program4);
    } // else if
    #endif

    #if SUPPORT_PROFILE_GLSL120
    else if (strcmp(profile, MOJOSHADER_PROFILE_GLSL120) == 0)
    {
        MUST_HAVE_GLSL(MOJOSHADER_PROFILE_GLSL120, 1, 20);
    } // else if
    #endif

    #if SUPPORT_PROFILE_GLSL
    else if (strcmp(profile, MOJOSHADER_PROFILE_GLSL) == 0)
    {
        MUST_HAVE_GLSL(MOJOSHADER_PROFILE_GLSL, 1, 10);
    } // else if
    #endif

    else
    {
        set_error("unknown or unsupported profile");
        return 0;
    } // else

    #undef MUST_HAVE

    return 1;
} // valid_profile


static const char *profile_priorities[] = {
#if SUPPORT_PROFILE_GLSL120
    MOJOSHADER_PROFILE_GLSL120,
#endif
#if SUPPORT_PROFILE_GLSL
    MOJOSHADER_PROFILE_GLSL,
#endif
#if SUPPORT_PROFILE_ARB1_NV
    MOJOSHADER_PROFILE_NV4,
    MOJOSHADER_PROFILE_NV3,
    MOJOSHADER_PROFILE_NV2,
#endif
#if SUPPORT_PROFILE_ARB1
    MOJOSHADER_PROFILE_ARB1,
#endif
};

int MOJOSHADER_glAvailableProfiles(MOJOSHADER_glGetProcAddress lookup, void *d,
                                   const char **profs, const int size)
{
    int retval = 0;
    MOJOSHADER_glContext _ctx;
    MOJOSHADER_glContext *current_ctx = ctx;

    ctx = &_ctx;
    memset(ctx, '\0', sizeof (MOJOSHADER_glContext));
    load_extensions(lookup, d);

    if (ctx->have_core_opengl)
    {
        size_t i;
        for (i = 0; i < STATICARRAYLEN(profile_priorities); i++)
        {
            const char *profile = profile_priorities[i];
            if (valid_profile(profile))
            {
                if (retval < size)
                    profs[retval] = profile;
                retval++;
            } // if
        } // for
    } // if

    ctx = current_ctx;
    return retval;
} // MOJOSHADER_glAvailableProfiles


const char *MOJOSHADER_glBestProfile(MOJOSHADER_glGetProcAddress gpa, void *d)
{
    const char *prof[STATICARRAYLEN(profile_priorities)];
    if (MOJOSHADER_glAvailableProfiles(gpa, d, prof, STATICARRAYLEN(prof)) <= 0)
    {
        set_error("no profiles available");
        return NULL;
    } // if

    return prof[0];  // profiles are sorted "best" to "worst."
} // MOJOSHADER_glBestProfile


MOJOSHADER_glContext *MOJOSHADER_glCreateContext(const char *profile,
                                        MOJOSHADER_glGetProcAddress lookup,
                                        void *lookup_d,
                                        MOJOSHADER_malloc m, MOJOSHADER_free f,
                                        void *malloc_d)
{
    MOJOSHADER_glContext *retval = NULL;
    MOJOSHADER_glContext *current_ctx = ctx;

    ctx = NULL;

    if (m == NULL) m = MOJOSHADER_internal_malloc;
    if (f == NULL) f = MOJOSHADER_internal_free;

    ctx = (MOJOSHADER_glContext *) m(sizeof (MOJOSHADER_glContext), malloc_d);
    if (ctx == NULL)
    {
        out_of_memory();
        goto init_fail;
    } // if

    memset(ctx, '\0', sizeof (MOJOSHADER_glContext));
    ctx->malloc_fn = m;
    ctx->free_fn = f;
    ctx->malloc_data = malloc_d;
    snprintf(ctx->profile, sizeof (ctx->profile), "%s", profile);

    load_extensions(lookup, lookup_d);
    if (!valid_profile(profile))
        goto init_fail;

    MOJOSHADER_glBindProgram(NULL);

    // !!! FIXME: generalize this part.
    if (profile == NULL) {}

    // We don't check SUPPORT_PROFILE_GLSL120 here, since valid_profile() does.
#if SUPPORT_PROFILE_GLSL
    else if ( (strcmp(profile, MOJOSHADER_PROFILE_GLSL) == 0) ||
              (strcmp(profile, MOJOSHADER_PROFILE_GLSL120) == 0) )
    {
        ctx->profileMaxUniforms = impl_GLSL_MaxUniforms;
        ctx->profileCompileShader = impl_GLSL_CompileShader;
        ctx->profileDeleteShader = impl_GLSL_DeleteShader;
        ctx->profileDeleteProgram = impl_GLSL_DeleteProgram;
        ctx->profileGetAttribLocation = impl_GLSL_GetAttribLocation;
        ctx->profileGetUniformLocation = impl_GLSL_GetUniformLocation;
        ctx->profileGetSamplerLocation = impl_GLSL_GetSamplerLocation;
        ctx->profileLinkProgram = impl_GLSL_LinkProgram;
        ctx->profileFinalInitProgram = impl_GLSL_FinalInitProgram;
        ctx->profileUseProgram = impl_GLSL_UseProgram;
        ctx->profilePushConstantArray = impl_GLSL_PushConstantArray;
        ctx->profilePushUniforms = impl_GLSL_PushUniforms;
        ctx->profilePushSampler = impl_GLSL_PushSampler;
        ctx->profileMustPushConstantArrays = impl_GLSL_MustPushConstantArrays;
        ctx->profileMustPushSamplers = impl_GLSL_MustPushSamplers;
    } // if
#endif

    // We don't check SUPPORT_PROFILE_ARB1_NV here, since valid_profile() does.
#if SUPPORT_PROFILE_ARB1
    else if ( (strcmp(profile, MOJOSHADER_PROFILE_ARB1) == 0) ||
              (strcmp(profile, MOJOSHADER_PROFILE_NV2) == 0) ||
              (strcmp(profile, MOJOSHADER_PROFILE_NV3) == 0) ||
              (strcmp(profile, MOJOSHADER_PROFILE_NV4) == 0) )
    {
        ctx->profileMaxUniforms = impl_ARB1_MaxUniforms;
        ctx->profileCompileShader = impl_ARB1_CompileShader;
        ctx->profileDeleteShader = impl_ARB1_DeleteShader;
        ctx->profileDeleteProgram = impl_ARB1_DeleteProgram;
        ctx->profileGetAttribLocation = impl_ARB1_GetAttribLocation;
        ctx->profileGetUniformLocation = impl_ARB1_GetUniformLocation;
        ctx->profileGetSamplerLocation = impl_ARB1_GetSamplerLocation;
        ctx->profileLinkProgram = impl_ARB1_LinkProgram;
        ctx->profileFinalInitProgram = impl_ARB1_FinalInitProgram;
        ctx->profileUseProgram = impl_ARB1_UseProgram;
        ctx->profilePushConstantArray = impl_ARB1_PushConstantArray;
        ctx->profilePushUniforms = impl_ARB1_PushUniforms;
        ctx->profilePushSampler = impl_ARB1_PushSampler;
        ctx->profileMustPushConstantArrays = impl_ARB1_MustPushConstantArrays;
        ctx->profileMustPushSamplers = impl_ARB1_MustPushSamplers;
    } // if
#endif

    assert(ctx->profileMaxUniforms != NULL);
    assert(ctx->profileCompileShader != NULL);
    assert(ctx->profileDeleteShader != NULL);
    assert(ctx->profileDeleteProgram != NULL);
    assert(ctx->profileMaxUniforms != NULL);
    assert(ctx->profileGetAttribLocation != NULL);
    assert(ctx->profileGetUniformLocation != NULL);
    assert(ctx->profileGetSamplerLocation != NULL);
    assert(ctx->profileLinkProgram != NULL);
    assert(ctx->profileFinalInitProgram != NULL);
    assert(ctx->profileUseProgram != NULL);
    assert(ctx->profilePushConstantArray != NULL);
    assert(ctx->profilePushUniforms != NULL);
    assert(ctx->profilePushSampler != NULL);
    assert(ctx->profileMustPushConstantArrays != NULL);
    assert(ctx->profileMustPushSamplers != NULL);

    retval = ctx;
    ctx = current_ctx;
    return retval;

init_fail:
    if (ctx != NULL)
        f(ctx, malloc_d);
    ctx = current_ctx;
    return NULL;
} // MOJOSHADER_glCreateContext


void MOJOSHADER_glMakeContextCurrent(MOJOSHADER_glContext *_ctx)
{
    ctx = _ctx;
} // MOJOSHADER_glMakeContextCurrent


int MOJOSHADER_glMaxUniforms(MOJOSHADER_shaderType shader_type)
{
    return ctx->profileMaxUniforms(shader_type);
} // MOJOSHADER_glMaxUniforms


MOJOSHADER_glShader *MOJOSHADER_glCompileShader(const unsigned char *tokenbuf,
                                                const unsigned int bufsize,
                                                const MOJOSHADER_swizzle *swiz,
                                                const unsigned int swizcount)
{
    MOJOSHADER_glShader *retval = NULL;
    GLuint shader = 0;
    const MOJOSHADER_parseData *pd = MOJOSHADER_parse(ctx->profile, tokenbuf,
                                                      bufsize, swiz, swizcount,
                                                      ctx->malloc_fn,
                                                      ctx->free_fn,
                                                      ctx->malloc_data);
    if (pd->error_count > 0)
    {
        // !!! FIXME: put multiple errors in the buffer? Don't use
        // !!! FIXME:  MOJOSHADER_glGetError() for this?
        set_error(pd->errors[0].error);
        goto compile_shader_fail;
    } // if

    retval = (MOJOSHADER_glShader *) Malloc(sizeof (MOJOSHADER_glShader));
    if (retval == NULL)
        goto compile_shader_fail;

    if (!ctx->profileCompileShader(pd, &shader))
        goto compile_shader_fail;

    retval->parseData = pd;
    retval->handle = shader;
    retval->refcount = 1;
    return retval;

compile_shader_fail:
    MOJOSHADER_freeParseData(pd);
    Free(retval);
    if (shader != 0)
        ctx->profileDeleteShader(shader);
    return NULL;
} // MOJOSHADER_glCompileShader


const MOJOSHADER_parseData *MOJOSHADER_glGetShaderParseData(
                                                MOJOSHADER_glShader *shader)
{
    return (shader != NULL) ? shader->parseData : NULL;
} // MOJOSHADER_glGetShaderParseData


static void shader_unref(MOJOSHADER_glShader *shader)
{
    if (shader != NULL)
    {
        const uint32 refcount = shader->refcount;
        if (refcount > 1)
            shader->refcount--;
        else
        {
            ctx->profileDeleteShader(shader->handle);
            MOJOSHADER_freeParseData(shader->parseData);
            Free(shader);
        } // else
    } // if
} // shader_unref


static void program_unref(MOJOSHADER_glProgram *program)
{
    if (program != NULL)
    {
        const uint32 refcount = program->refcount;
        if (refcount > 1)
            program->refcount--;
        else
        {
            ctx->profileDeleteProgram(program->handle);
            shader_unref(program->vertex);
            shader_unref(program->fragment);
            Free(program->vs_preshader_regs);
            Free(program->ps_preshader_regs);
            Free(program->vs_uniforms_float4);
            Free(program->vs_uniforms_int4);
            Free(program->vs_uniforms_bool);
            Free(program->ps_uniforms_float4);
            Free(program->ps_uniforms_int4);
            Free(program->ps_uniforms_bool);
            Free(program->uniforms);
            Free(program->attributes);
            Free(program);
        } // else
    } // if
} // program_unref


static void fill_constant_array(GLfloat *f, const int base, const int size,
                                const MOJOSHADER_parseData *pd)
{
    int i;
    int filled = 0;
    for (i = 0; i < pd->constant_count; i++)
    {
        const MOJOSHADER_constant *c = &pd->constants[i];
        if (c->type != MOJOSHADER_UNIFORM_FLOAT)
            continue;
        else if (c->index < base)
            continue;
        else if (c->index >= (base+size))
            continue;
        memcpy(&f[(c->index-base) * 4], &c->value.f, sizeof (c->value.f));
        filled++;
    } // for

    assert(filled == size);
} // fill_constant_array


static int lookup_uniforms(MOJOSHADER_glProgram *program,
                           MOJOSHADER_glShader *shader, int *bound)
{
    const MOJOSHADER_parseData *pd = shader->parseData;
    const MOJOSHADER_shaderType shader_type = pd->shader_type;
    uint32 float4_count = 0;
    uint32 int4_count = 0;
    uint32 bool_count = 0;
    int i;

    for (i = 0; i < pd->uniform_count; i++)
    {
        const MOJOSHADER_uniform *u = &pd->uniforms[i];

        if (u->constant)
        {
            // only do constants once, at link time. These aren't changed ever.
            if (ctx->profileMustPushConstantArrays())
            {
                const int base = u->index;
                const int size = u->array_count;
                GLfloat *f = (GLfloat *) alloca(sizeof (GLfloat) * (size * 4));
                fill_constant_array(f, base, size, pd);
                if (!(*bound))
                {
                    ctx->profileUseProgram(program);
                    *bound = 1;
                } // if
                ctx->profilePushConstantArray(program, u, f);
            } // if
        } // if

        else
        {
            const GLint loc = ctx->profileGetUniformLocation(program, shader, i);
            if (loc != -1)  // -1 means it was optimized out, or failure.
            {
                const int regcount = u->array_count;
                UniformMap *map = &program->uniforms[program->uniform_count];
                map->shader_type = shader_type;
                map->uniform = u;
                map->location = (GLuint) loc;
                program->uniform_count++;

                if (u->type == MOJOSHADER_UNIFORM_FLOAT)
                    float4_count += regcount ? regcount : 1;
                else if (u->type == MOJOSHADER_UNIFORM_INT)
                    int4_count += regcount ? regcount : 1;
                else if (u->type == MOJOSHADER_UNIFORM_BOOL)
                    bool_count += regcount ? regcount : 1;
                else
                    assert(0 && "Unexpected register type");
            } // if
        } // else
    } // for

    #define MAKE_ARRAY(typ, gltyp, siz, count) \
        if (count) { \
            const size_t buflen = sizeof (gltyp) * siz * count; \
            gltyp *ptr = (gltyp *) Malloc(buflen); \
            if (ptr == NULL) { \
                return 0; \
            } else if (shader_type == MOJOSHADER_TYPE_VERTEX) { \
                program->vs_uniforms_##typ = ptr; \
                program->vs_uniforms_##typ##_count = count; \
            } else if (shader_type == MOJOSHADER_TYPE_PIXEL) { \
                program->ps_uniforms_##typ = ptr; \
                program->ps_uniforms_##typ##_count = count; \
            } else { \
                assert(0 && "unsupported shader type"); \
            } \
            memset(ptr, '\0', buflen); \
        }

    MAKE_ARRAY(float4, GLfloat, 4, float4_count);
    MAKE_ARRAY(int4, GLint, 4, int4_count);
    MAKE_ARRAY(bool, GLint, 1, bool_count);

    #undef MAKE_ARRAY

    if (pd->preshader)
    {
        unsigned int largest = 0;
        const MOJOSHADER_symbol *sym = pd->preshader->symbols;
        for (i = 0; i < pd->preshader->symbol_count; i++, sym++)
        {
            const unsigned int val = sym->register_index + sym->register_count;
            if (val > largest)
                largest = val;
        } // for

        if (largest > 0)
        {
            const size_t len = largest * sizeof (GLfloat) * 4;
            GLfloat *buf = (GLfloat *) Malloc(len);
            if (buf == NULL)
                return 0;
            memset(buf, '\0', len);

            if (shader_type == MOJOSHADER_TYPE_VERTEX)
            {
                program->vs_preshader_reg_count = largest;
                program->vs_preshader_regs = buf;
            } // if
            else if (shader_type == MOJOSHADER_TYPE_PIXEL)
            {
                program->ps_preshader_reg_count = largest;
                program->ps_preshader_regs = buf;
            } // else if
        } // if
    } // if

    return 1;
} // lookup_uniforms


static void lookup_samplers(MOJOSHADER_glProgram *program,
                            MOJOSHADER_glShader *shader, int *bound)
{
    const MOJOSHADER_parseData *pd = shader->parseData;
    const MOJOSHADER_sampler *s = pd->samplers;
    int i;

    if ((pd->sampler_count == 0) || (!ctx->profileMustPushSamplers()))
        return;   // nothing to do here, so don't bother binding, etc.

    // Link up the Samplers. These never change after link time, since they
    //  are meant to be constant texture unit ids and not textures.

    if (!(*bound))
    {
        ctx->profileUseProgram(program);
        *bound = 1;
    } // if

    for (i = 0; i < pd->sampler_count; i++)
    {
        const GLint loc = ctx->profileGetSamplerLocation(program, shader, i);
        if (loc >= 0)  // maybe the Sampler was optimized out?
            ctx->profilePushSampler(loc, s[i].index);
    } // for
} // lookup_samplers


// Right now, this just decides if we have to toggle pointsize support later.
static void lookup_outputs(MOJOSHADER_glProgram *program,
                           MOJOSHADER_glShader *shader)
{
    const MOJOSHADER_parseData *pd = shader->parseData;
    int i;

    for (i = 0; i < pd->output_count; i++)
    {
        if (pd->outputs[i].usage == MOJOSHADER_USAGE_POINTSIZE)
        {
            program->uses_pointsize = 1;
            break;
        } // if
    } // for
} // lookup_outputs


static int lookup_attributes(MOJOSHADER_glProgram *program)
{
    int i;
    const MOJOSHADER_parseData *pd = program->vertex->parseData;
    const MOJOSHADER_attribute *a = pd->attributes;

    for (i = 0; i < pd->attribute_count; i++)
    {
        const GLint loc = ctx->profileGetAttribLocation(program, i);
        if (loc >= 0)  // maybe the Attribute was optimized out?
        {
            AttributeMap *map = &program->attributes[program->attribute_count];
            map->attribute = &a[i];
            map->location = loc;
            program->attribute_count++;

            if (((size_t)loc) > STATICARRAYLEN(ctx->want_attr))
            {
                assert(0 && "Static array is too small.");  // laziness fail.
                return 0;
            } // if
        } // if
    } // for

    return 1;
} // lookup_attributes


// !!! FIXME: misnamed
// build a list of indexes that need to be overwritten with constant values
//  when pushing a uniform array to the GL.
static int build_constants_lists(MOJOSHADER_glProgram *program)
{
    int i;
    const int count = program->uniform_count;
    for (i = 0; i < count; i++)
    {
        UniformMap *map = &program->uniforms[i];
        const MOJOSHADER_uniform *u = map->uniform;
        const int size = u->array_count;

        assert(!u->constant);

        if (size == 0)
            continue;  // nothing to see here.

        // only use arrays for 'c' registers.
        assert(u->type == MOJOSHADER_UNIFORM_FLOAT);

        // !!! FIXME: deal with this.
    } // for

    return 1;
} // build_constants_lists


MOJOSHADER_glProgram *MOJOSHADER_glLinkProgram(MOJOSHADER_glShader *vshader,
                                               MOJOSHADER_glShader *pshader)
{
    int bound = 0;

    if ((vshader == NULL) && (pshader == NULL))
        return NULL;

    int numregs = 0;
    MOJOSHADER_glProgram *retval = NULL;
    const GLuint program = ctx->profileLinkProgram(vshader, pshader);
    if (program == 0)
        goto link_program_fail;

    retval = (MOJOSHADER_glProgram *) Malloc(sizeof (MOJOSHADER_glProgram));
    if (retval == NULL)
        goto link_program_fail;
    memset(retval, '\0', sizeof (MOJOSHADER_glProgram));

    numregs = 0;
    if (vshader != NULL) numregs += vshader->parseData->uniform_count;
    if (pshader != NULL) numregs += pshader->parseData->uniform_count;
    if (numregs > 0)
    {
        const size_t len = sizeof (UniformMap) * numregs;
        retval->uniforms = (UniformMap *) Malloc(len);
        if (retval->uniforms == NULL)
            goto link_program_fail;
        memset(retval->uniforms, '\0', len);
    } // if

    retval->handle = program;
    retval->vertex = vshader;
    retval->fragment = pshader;
    retval->generation = ctx->generation - 1;
    retval->refcount = 1;

    if (vshader != NULL)
    {
        if (vshader->parseData->attribute_count > 0)
        {
            const int count = vshader->parseData->attribute_count;
            const size_t len = sizeof (AttributeMap) * count;
            retval->attributes = (AttributeMap *) Malloc(len);
            if (retval->attributes == NULL)
                goto link_program_fail;

            memset(retval->attributes, '\0', len);
            if (!lookup_attributes(retval))
                goto link_program_fail;
        } // if

        if (!lookup_uniforms(retval, vshader, &bound))
            goto link_program_fail;
        lookup_samplers(retval, vshader, &bound);
        lookup_outputs(retval, pshader);
        vshader->refcount++;
    } // if

    if (pshader != NULL)
    {
        if (!lookup_uniforms(retval, pshader, &bound))
            goto link_program_fail;
        lookup_samplers(retval, pshader, &bound);
        lookup_outputs(retval, pshader);
        pshader->refcount++;
    } // if

    if (!build_constants_lists(retval))
        goto link_program_fail;

    if (bound)  // reset the old binding.
        ctx->profileUseProgram(ctx->bound_program);

    ctx->profileFinalInitProgram(retval);

    return retval;

link_program_fail:
    if (retval != NULL)
    {
        Free(retval->vs_uniforms_float4);
        Free(retval->vs_uniforms_int4);
        Free(retval->vs_uniforms_bool);
        Free(retval->ps_uniforms_float4);
        Free(retval->ps_uniforms_int4);
        Free(retval->ps_uniforms_bool);
        Free(retval->uniforms);
        Free(retval->attributes);
        Free(retval);
    } // if

    if (program != 0)
        ctx->profileDeleteProgram(program);

    if (bound)
        ctx->profileUseProgram(ctx->bound_program);

    return NULL;
} // MOJOSHADER_glLinkProgram


static void update_enabled_arrays(void)
{
    int highest_enabled = 0;
    int i;

    // Enable/disable vertex arrays to match our needs.
    // this happens to work in both ARB1 and GLSL, but if something alien
    //  shows up, we'll have to split these into profile*() functions.
    for (i = 0; i < ctx->max_attrs; i++)
    {
        const int want = (const int) ctx->want_attr[i];
        const int have = (const int) ctx->have_attr[i];
        if (want != have)
        {
            if (want)
                ctx->glEnableVertexAttribArray(i);
            else
                ctx->glDisableVertexAttribArray(i);
            ctx->have_attr[i] = want;
        } // if

        if (want)
            highest_enabled = i + 1;
    } // for

    ctx->max_attrs = highest_enabled;  // trim unneeded iterations next time.
} // update_enabled_arrays


void MOJOSHADER_glBindProgram(MOJOSHADER_glProgram *program)
{
    if (program == ctx->bound_program)
        return;  // nothing to do.

    if (program != NULL)
        program->refcount++;

    memset(ctx->want_attr, '\0', sizeof (ctx->want_attr[0]) * ctx->max_attrs);

    // If no program bound, disable all arrays, in case we're switching to
    //  fixed function pipeline. Otherwise, we try to minimize state changes
    //  by toggling just the changed set of needed arrays in ProgramReady().
    if (program == NULL)
        update_enabled_arrays();

    ctx->profileUseProgram(program);
    program_unref(ctx->bound_program);
    ctx->bound_program = program;
} // MOJOSHADER_glBindProgram


typedef struct
{
    MOJOSHADER_glShader *vertex;
    MOJOSHADER_glShader *fragment;
} BoundShaders;

static uint32 hash_shaders(const void *sym, void *data)
{
    (void) data;
    const BoundShaders *s = (const BoundShaders *) sym;
    const uint32 v = (s->vertex) ? (uint32) s->vertex->handle : 0;
    const uint32 f = (s->fragment) ? (uint32) s->fragment->handle : 0;
    return ((v & 0xFFFF) << 16) | (f & 0xFFFF);
} // hash_shaders

static int match_shaders(const void *_a, const void *_b, void *data)
{
    (void) data;
    const BoundShaders *a = (const BoundShaders *) _a;
    const BoundShaders *b = (const BoundShaders *) _b;

    const GLuint av = (a->vertex) ? a->vertex->handle : 0;
    const GLuint bv = (b->vertex) ? b->vertex->handle : 0;
    if (av != bv)
        return 0;

    const GLuint af = (a->fragment) ? a->fragment->handle : 0;
    const GLuint bf = (b->fragment) ? b->fragment->handle : 0;
    if (af != bf)
        return 0;

    return 1;
} // match_shaders

static void nuke_shaders(const void *key, const void *value, void *data)
{
    (void) data;
    Free((void *) key);  // this was a BoundShaders struct.
    MOJOSHADER_glDeleteProgram((MOJOSHADER_glProgram *) value);
} // nuke_shaders

void MOJOSHADER_glBindShaders(MOJOSHADER_glShader *v, MOJOSHADER_glShader *p)
{
    if ((v == NULL) && (p == NULL))
    {
        MOJOSHADER_glBindProgram(NULL);
        return;
    } // if

    // !!! FIXME: eventually support GL_EXT_separate_shader_objects.
    if (ctx->linker_cache == NULL)
    {
        ctx->linker_cache = hash_create(NULL, hash_shaders, match_shaders,
                                        nuke_shaders, 0, ctx->malloc_fn,
                                        ctx->free_fn, ctx->malloc_data);

        if (ctx->linker_cache == NULL)
        {
            out_of_memory();
            return;
        } // if
    } // if

    MOJOSHADER_glProgram *program = NULL;
    BoundShaders shaders;
    shaders.vertex = v;
    shaders.fragment = p;

    const void *val = NULL;
    if (hash_find(ctx->linker_cache, &shaders, &val))
        program = (MOJOSHADER_glProgram *) val;
    else
    {
        program = MOJOSHADER_glLinkProgram(v, p);
        if (program == NULL)
            return;

        BoundShaders *item = (BoundShaders *) Malloc(sizeof (BoundShaders));
        if (item == NULL)
        {
            MOJOSHADER_glDeleteProgram(program);
            return;
        } // if

        memcpy(item, &shaders, sizeof (BoundShaders));
        if (hash_insert(ctx->linker_cache, item, program) != 1)
        {
            Free(item);
            MOJOSHADER_glDeleteProgram(program);
            out_of_memory();
            return;
        } // if
    } // else

    assert(program != NULL);
    MOJOSHADER_glBindProgram(program);
} // MOJOSHADER_glBindShaders


static inline uint minuint(const uint a, const uint b)
{
    return ((a < b) ? a : b);
} // minuint


void MOJOSHADER_glSetVertexShaderUniformF(unsigned int idx, const float *data,
                                          unsigned int vec4n)
{
    const uint maxregs = STATICARRAYLEN(ctx->vs_reg_file_f) / 4;
    if (idx < maxregs)
    {
        assert(sizeof (GLfloat) == sizeof (float));
        const uint cpy = (minuint(maxregs - idx, vec4n) * sizeof (*data)) * 4;
        memcpy(ctx->vs_reg_file_f + (idx * 4), data, cpy);
        ctx->generation++;
    } // if
} // MOJOSHADER_glSetVertexShaderUniformF


void MOJOSHADER_glGetVertexShaderUniformF(unsigned int idx, float *data,
                                          unsigned int vec4n)
{
    const uint maxregs = STATICARRAYLEN(ctx->vs_reg_file_f) / 4;
    if (idx < maxregs)
    {
        assert(sizeof (GLfloat) == sizeof (float));
        const uint cpy = (minuint(maxregs - idx, vec4n) * sizeof (*data)) * 4;
        memcpy(data, ctx->vs_reg_file_f + (idx * 4), cpy);
    } // if
} // MOJOSHADER_glGetVertexShaderUniformF


void MOJOSHADER_glSetVertexShaderUniformI(unsigned int idx, const int *data,
                                          unsigned int ivec4n)
{
    const uint maxregs = STATICARRAYLEN(ctx->vs_reg_file_i) / 4;
    if (idx < maxregs)
    {
        assert(sizeof (GLint) == sizeof (int));
        const uint cpy = (minuint(maxregs - idx, ivec4n) * sizeof (*data)) * 4;
        memcpy(ctx->vs_reg_file_i + (idx * 4), data, cpy);
        ctx->generation++;
    } // if
} // MOJOSHADER_glSetVertexShaderUniformI


void MOJOSHADER_glGetVertexShaderUniformI(unsigned int idx, int *data,
                                          unsigned int ivec4n)
{
    const uint maxregs = STATICARRAYLEN(ctx->vs_reg_file_i) / 4;
    if (idx < maxregs)
    {
        assert(sizeof (GLint) == sizeof (int));
        const uint cpy = (minuint(maxregs - idx, ivec4n) * sizeof (*data)) * 4;
        memcpy(data, ctx->vs_reg_file_i + (idx * 4), cpy);
    } // if
} // MOJOSHADER_glGetVertexShaderUniformI


void MOJOSHADER_glSetVertexShaderUniformB(unsigned int idx, const int *data,
                                          unsigned int bcount)
{
    const uint maxregs = STATICARRAYLEN(ctx->vs_reg_file_f) / 4;
    if (idx < maxregs)
    {
        uint8 *wptr = ctx->vs_reg_file_b + idx;
        uint8 *endptr = wptr + minuint(maxregs - idx, bcount);
        while (wptr != endptr)
            *(wptr++) = *(data++) ? 1 : 0;
        ctx->generation++;
    } // if
} // MOJOSHADER_glSetVertexShaderUniformB


void MOJOSHADER_glGetVertexShaderUniformB(unsigned int idx, int *data,
                                          unsigned int bcount)
{
    const uint maxregs = STATICARRAYLEN(ctx->vs_reg_file_f) / 4;
    if (idx < maxregs)
    {
        uint8 *rptr = ctx->vs_reg_file_b + idx;
        uint8 *endptr = rptr + minuint(maxregs - idx, bcount);
        while (rptr != endptr)
            *(data++) = (int) *(rptr++);
    } // if
} // MOJOSHADER_glGetVertexShaderUniformB


void MOJOSHADER_glSetPixelShaderUniformF(unsigned int idx, const float *data,
                                         unsigned int vec4n)
{
    const uint maxregs = STATICARRAYLEN(ctx->ps_reg_file_f) / 4;
    if (idx < maxregs)
    {
        assert(sizeof (GLfloat) == sizeof (float));
        const uint cpy = (minuint(maxregs - idx, vec4n) * sizeof (*data)) * 4;
        memcpy(ctx->ps_reg_file_f + (idx * 4), data, cpy);
        ctx->generation++;
    } // if
} // MOJOSHADER_glSetPixelShaderUniformF


void MOJOSHADER_glGetPixelShaderUniformF(unsigned int idx, float *data,
                                         unsigned int vec4n)
{
    const uint maxregs = STATICARRAYLEN(ctx->ps_reg_file_f) / 4;
    if (idx < maxregs)
    {
        assert(sizeof (GLfloat) == sizeof (float));
        const uint cpy = (minuint(maxregs - idx, vec4n) * sizeof (*data)) * 4;
        memcpy(data, ctx->ps_reg_file_f + (idx * 4), cpy);
    } // if
} // MOJOSHADER_glGetPixelShaderUniformF


void MOJOSHADER_glSetPixelShaderUniformI(unsigned int idx, const int *data,
                                         unsigned int ivec4n)
{
    const uint maxregs = STATICARRAYLEN(ctx->ps_reg_file_i) / 4;
    if (idx < maxregs)
    {
        assert(sizeof (GLint) == sizeof (int));
        const uint cpy = (minuint(maxregs - idx, ivec4n) * sizeof (*data)) * 4;
        memcpy(ctx->ps_reg_file_i + (idx * 4), data, cpy);
        ctx->generation++;
    } // if
} // MOJOSHADER_glSetPixelShaderUniformI


void MOJOSHADER_glGetPixelShaderUniformI(unsigned int idx, int *data,
                                         unsigned int ivec4n)
{
    const uint maxregs = STATICARRAYLEN(ctx->ps_reg_file_i) / 4;
    if (idx < maxregs)
    {
        assert(sizeof (GLint) == sizeof (int));
        const uint cpy = (minuint(maxregs - idx, ivec4n) * sizeof (*data)) * 4;
        memcpy(data, ctx->ps_reg_file_i + (idx * 4), cpy);
    } // if
} // MOJOSHADER_glGetPixelShaderUniformI


void MOJOSHADER_glSetPixelShaderUniformB(unsigned int idx, const int *data,
                                         unsigned int bcount)
{
    const uint maxregs = STATICARRAYLEN(ctx->ps_reg_file_f) / 4;
    if (idx < maxregs)
    {
        uint8 *wptr = ctx->ps_reg_file_b + idx;
        uint8 *endptr = wptr + minuint(maxregs - idx, bcount);
        while (wptr != endptr)
            *(wptr++) = *(data++) ? 1 : 0;
        ctx->generation++;
    } // if
} // MOJOSHADER_glSetPixelShaderUniformB


void MOJOSHADER_glGetPixelShaderUniformB(unsigned int idx, int *data,
                                         unsigned int bcount)
{
    const uint maxregs = STATICARRAYLEN(ctx->ps_reg_file_f) / 4;
    if (idx < maxregs)
    {
        uint8 *rptr = ctx->ps_reg_file_b + idx;
        uint8 *endptr = rptr + minuint(maxregs - idx, bcount);
        while (rptr != endptr)
            *(data++) = (int) *(rptr++);
    } // if
} // MOJOSHADER_glGetPixelShaderUniformB


static inline GLenum opengl_attr_type(const MOJOSHADER_attributeType type)
{
    switch (type)
    {
        case MOJOSHADER_ATTRIBUTE_UNKNOWN: return GL_NONE; // oh well.
        case MOJOSHADER_ATTRIBUTE_BYTE: return GL_BYTE;
        case MOJOSHADER_ATTRIBUTE_UBYTE: return GL_UNSIGNED_BYTE;
        case MOJOSHADER_ATTRIBUTE_SHORT: return GL_SHORT;
        case MOJOSHADER_ATTRIBUTE_USHORT: return GL_UNSIGNED_SHORT;
        case MOJOSHADER_ATTRIBUTE_INT: return GL_INT;
        case MOJOSHADER_ATTRIBUTE_UINT: return GL_UNSIGNED_INT;
        case MOJOSHADER_ATTRIBUTE_FLOAT: return GL_FLOAT;
        case MOJOSHADER_ATTRIBUTE_DOUBLE: return GL_DOUBLE;

        case MOJOSHADER_ATTRIBUTE_HALF_FLOAT:
            if (ctx->have_GL_NV_half_float)
                return GL_HALF_FLOAT_NV;
            else if (ctx->have_GL_ARB_half_float_vertex)
                return GL_HALF_FLOAT_ARB;
            else if (ctx->have_GL_OES_vertex_half_float)
                return GL_HALF_FLOAT_OES;
            break;
    } // switch

    return GL_NONE;  // oh well. Raises a GL error later.
} // opengl_attr_type


// !!! FIXME: shouldn't (index) be unsigned?
void MOJOSHADER_glSetVertexAttribute(MOJOSHADER_usage usage,
                                     int index, unsigned int size,
                                     MOJOSHADER_attributeType type,
                                     int normalized, unsigned int stride,
                                     const void *ptr)
{
    if ((ctx->bound_program == NULL) || (ctx->bound_program->vertex == NULL))
        return;

    const GLenum gl_type = opengl_attr_type(type);
    const GLboolean norm = (normalized) ? GL_TRUE : GL_FALSE;
    const int count = ctx->bound_program->attribute_count;
    GLint gl_index = 0;
    int i;

    for (i = 0; i < count; i++)
    {
        const AttributeMap *map = &ctx->bound_program->attributes[i];
        const MOJOSHADER_attribute *a = map->attribute;

        // !!! FIXME: is this array guaranteed to be sorted by usage?
        // !!! FIXME:  if so, we can break if a->usage > usage.

        if ((a->usage == usage) && (a->index == index))
        {
            gl_index = map->location;
            break;
        } // if
    } // for

    if (i == count)
        return;  // nothing to do, this shader doesn't use this stream.

    // this happens to work in both ARB1 and GLSL, but if something alien
    //  shows up, we'll have to split these into profile*() functions.
    ctx->glVertexAttribPointer(gl_index, size, gl_type, norm, stride, ptr);

    // flag this array as in use, so we can enable it later.
    ctx->want_attr[gl_index] = 1;
    if (ctx->max_attrs < (gl_index + 1))
        ctx->max_attrs = gl_index + 1;
} // MOJOSHADER_glSetVertexAttribute


void MOJOSHADER_glSetVertexPreshaderUniformF(unsigned int idx,
                                             const float *data,
                                             unsigned int vec4n)
{
    MOJOSHADER_glProgram *program = ctx->bound_program;
    if (program == NULL)
        return;  // nothing to do.

    const uint maxregs = program->vs_preshader_reg_count;
    if (idx < maxregs)
    {
        assert(sizeof (GLfloat) == sizeof (float));
        const uint cpy = (minuint(maxregs - idx, vec4n) * sizeof (*data)) * 4;
        memcpy(program->vs_preshader_regs + (idx * 4), data, cpy);
        program->generation = ctx->generation-1;
    } // if
} // MOJOSHADER_glSetVertexPreshaderUniformF


void MOJOSHADER_glGetVertexPreshaderUniformF(unsigned int idx, float *data,
                                             unsigned int vec4n)
{
    MOJOSHADER_glProgram *program = ctx->bound_program;
    if (program == NULL)
        return;  // nothing to do.

    const uint maxregs = program->vs_preshader_reg_count;
    if (idx < maxregs)
    {
        assert(sizeof (GLfloat) == sizeof (float));
        const uint cpy = (minuint(maxregs - idx, vec4n) * sizeof (*data)) * 4;
        memcpy(data, program->vs_preshader_regs + (idx * 4), cpy);
    } // if
} // MOJOSHADER_glGetVertexPreshaderUniformF


void MOJOSHADER_glSetPixelPreshaderUniformF(unsigned int idx,
                                             const float *data,
                                             unsigned int vec4n)
{
    MOJOSHADER_glProgram *program = ctx->bound_program;
    if (program == NULL)
        return;  // nothing to do.

    const uint maxregs = program->ps_preshader_reg_count;
    if (idx < maxregs)
    {
        assert(sizeof (GLfloat) == sizeof (float));
        const uint cpy = (minuint(maxregs - idx, vec4n) * sizeof (*data)) * 4;
        memcpy(program->ps_preshader_regs + (idx * 4), data, cpy);
        program->generation = ctx->generation-1;
    } // if
} // MOJOSHADER_glSetPixelPreshaderUniformF


void MOJOSHADER_glGetPixelPreshaderUniformF(unsigned int idx, float *data,
                                             unsigned int vec4n)
{
    MOJOSHADER_glProgram *program = ctx->bound_program;
    if (program == NULL)
        return;  // nothing to do.

    const uint maxregs = program->ps_preshader_reg_count;
    if (idx < maxregs)
    {
        assert(sizeof (GLfloat) == sizeof (float));
        const uint cpy = (minuint(maxregs - idx, vec4n) * sizeof (*data)) * 4;
        memcpy(data, program->ps_preshader_regs + (idx * 4), cpy);
    } // if
} // MOJOSHADER_glGetPixelPreshaderUniformF


void MOJOSHADER_glProgramReady(void)
{
    MOJOSHADER_glProgram *program = ctx->bound_program;

    if (program == NULL)
        return;  // nothing to do.

    // Toggle vertex attribute arrays on/off, based on our needs.
    update_enabled_arrays();

    if (program->uses_pointsize != ctx->pointsize_enabled)
    {
        if (program->uses_pointsize)
            ctx->glEnable(GL_PROGRAM_POINT_SIZE);
        else
            ctx->glDisable(GL_PROGRAM_POINT_SIZE);
        ctx->pointsize_enabled = program->uses_pointsize;
    } // if

    // push Uniforms to the program from our register files...
    if ((program->uniform_count) && (program->generation != ctx->generation))
    {
        // vertex shader uniforms come first in program->uniforms array.
        const uint32 count = program->uniform_count;
        const GLfloat *srcf = ctx->vs_reg_file_f;
        const GLint *srci = ctx->vs_reg_file_i;
        const uint8 *srcb = ctx->vs_reg_file_b;
        MOJOSHADER_shaderType shader_type = MOJOSHADER_TYPE_VERTEX;
        GLfloat *dstf = program->vs_uniforms_float4;
        GLint *dsti = program->vs_uniforms_int4;
        GLint *dstb = program->vs_uniforms_bool;
        const MOJOSHADER_preshader *preshader = NULL;
        uint32 i;

        int ran_preshader = 0;
        if (program->vertex)
        {
            preshader = program->vertex->parseData->preshader;
            if (preshader)
            {
                MOJOSHADER_runPreshader(preshader, program->vs_preshader_regs,
                                        ctx->vs_reg_file_f);
                ran_preshader = 1;
            } // if
        } // if

        if (program->fragment)
        {
            preshader = program->fragment->parseData->preshader;
            if (preshader)
            {
                MOJOSHADER_runPreshader(preshader, program->ps_preshader_regs,
                                        ctx->ps_reg_file_f);
                ran_preshader = 1;
            } // if
        } // if

        if (ran_preshader)
            ctx->generation++;

        for (i = 0; i < count; i++)
        {
            UniformMap *map = &program->uniforms[i];
            const MOJOSHADER_shaderType uniform_shader_type = map->shader_type;
            const MOJOSHADER_uniform *u = map->uniform;
            const MOJOSHADER_uniformType type = u->type;
            const int index = u->index;
            const int size = u->array_count ? u->array_count : 1;

            assert(!u->constant);

            // Did we switch from vertex to pixel (to geometry, etc)?
            if (shader_type != uniform_shader_type)
            {
                // we start with vertex, move to pixel, then to geometry, etc.
                //  The array should always be sorted as such.
                if (uniform_shader_type == MOJOSHADER_TYPE_PIXEL)
                {
                    assert(shader_type == MOJOSHADER_TYPE_VERTEX);
                    srcf = ctx->ps_reg_file_f;
                    srci = ctx->ps_reg_file_i;
                    srcb = ctx->ps_reg_file_b;
                    dstf = program->ps_uniforms_float4;
                    dsti = program->ps_uniforms_int4;
                    dstb = program->ps_uniforms_bool;
                } // if
                else
                {
                    // Should be ordered vertex, then pixel, then geometry.
                    assert(0 && "Unexpected shader type");
                } // else

                shader_type = uniform_shader_type;
            } // if

            if (type == MOJOSHADER_UNIFORM_FLOAT)
            {
                const size_t count = 4 * size;
                const GLfloat *f = &srcf[index * 4];
                memcpy(dstf, f, sizeof (GLfloat) * count);
                dstf += count;
            } // if
            else if (type == MOJOSHADER_UNIFORM_INT)
            {
                const size_t count = 4 * size;
                const GLint *i = &srci[index * 4];
                memcpy(dsti, i, sizeof (GLint) * count);
                dsti += count;
            } // else if
            else if (type == MOJOSHADER_UNIFORM_BOOL)
            {
                const size_t count = size;
                const uint8 *b = &srcb[index];
                size_t i;
                for (i = 0; i < count; i++)
                    dstb[i] = (GLint) b[i];
                dstb += count;
            } // else if

            // !!! FIXME: set constants that overlap the array.
        } // for

        program->generation = ctx->generation;

        ctx->profilePushUniforms();
    } // if
} // MOJOSHADER_glProgramReady


void MOJOSHADER_glDeleteProgram(MOJOSHADER_glProgram *program)
{
    program_unref(program);
} // MOJOSHADER_glDeleteProgram


void MOJOSHADER_glDeleteShader(MOJOSHADER_glShader *shader)
{
    // See if this was bound as an unlinked program anywhere...
    if (ctx->linker_cache)
    {
        const void *key = NULL;
        void *iter = NULL;
        int morekeys = hash_iter_keys(ctx->linker_cache, &key, &iter);
        while (morekeys)
        {
            const BoundShaders *shaders = (const BoundShaders *) key;
            // Do this here so we don't confuse the iteration by removing...
            morekeys = hash_iter_keys(ctx->linker_cache, &key, &iter);
            if ((shaders->vertex == shader) || (shaders->fragment == shader))
            {
                // Deletes the linked program, which will unref the shader.
                hash_remove(ctx->linker_cache, shaders);
            } // if
        } // while
    } // if

    shader_unref(shader);
} // MOJOSHADER_glDeleteShader


void MOJOSHADER_glDestroyContext(MOJOSHADER_glContext *_ctx)
{
    MOJOSHADER_glContext *current_ctx = ctx;
    ctx = _ctx;
    MOJOSHADER_glBindProgram(NULL);
    lookup_entry_points(NULL, NULL);   // !!! FIXME: is there a value to this?
    if (ctx->linker_cache)
        hash_destroy(ctx->linker_cache);
    Free(ctx);
    ctx = ((current_ctx == _ctx) ? NULL : current_ctx);
} // MOJOSHADER_glDestroyContext

// end of mojoshader_opengl.c ...

