/**
 * MojoShader; generate shader programs from bytecode of compiled
 *  Direct3D shaders.
 *
 * Please see the file LICENSE.txt in the source's root directory.
 *
 *  This file written by Ryan C. Gordon.
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <sys/types.h>
#include <errno.h>

#include "mojoshader.h"

#if FINDERRORS_COMPILE_SHADERS
#include "SDL.h"
static void *lookup(const char *fnname, void *unused)
{
    (void) unused;
    return SDL_GL_GetProcAddress(fnname);
} // lookup
#endif

#ifdef _MSC_VER
#define WIN32_LEAN_AND_MEAN 1
#include <windows.h>
#include <malloc.h>  // for alloca().
#define snprintf _snprintf
#else
#include <dirent.h>
#endif

#define report printf

static int do_file(const char *profile, const char *dname, const char *fn, int *total)
{
    int do_quit = 0;

    #if FINDERRORS_COMPILE_SHADERS
    SDL_Event e;  // pump event queue to keep OS happy.
    while (SDL_PollEvent(&e))
    {
        if (e.type == SDL_QUIT)
            do_quit = 1;
    } // while
    SDL_GL_SwapBuffers();
    #endif

    if (do_quit)
    {
        report("FAIL: user requested quit!\n");
        return 0;
    } // if

    int assembly = 0;
    if (strstr(fn, ".bytecode") != NULL)
        assembly = 0;
    else if (strstr(fn, ".disasm") != NULL)
        assembly = 1;
    else
        return 1;

    (*total)++;

    char *fname = (char *) alloca(strlen(fn) + strlen(dname) + 1);
    sprintf(fname, "%s/%s", dname, fn);
    FILE *io = fopen(fname, "rb");
    if (io == NULL)
    {
        report("FAIL: %s fopen() failed.\n", fname);
        return 1;
    } // if

    static unsigned char buf[1024 * 256];
    int rc = fread(buf, 1, sizeof (buf), io);
    fclose(io);
    if (rc == -1)
    {
        report("FAIL: %s %s\n", fname, strerror(errno));
        return 1;
    } // if

    if (assembly)
    {
        const MOJOSHADER_parseData *a;

        buf[rc] = '\0';  // make sure the source is null-terminated.
        a = MOJOSHADER_assemble(fname, (char *) buf, rc, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        if (a->error_count > 0)
        {
            report("FAIL: %s (line %d) %s\n",
                a->errors[0].filename ? a->errors[0].filename : "???",
                a->errors[0].error_position,
                a->errors[0].error);
            return 1;
        } // if

        else if (a->output_len > sizeof (buf))
        {
            report("FAIL: %s buffer overflow in finderrors.c\n", fname);
            return 1;
        } // if

        rc = a->output_len;
        memcpy(buf, a->output, rc);
        MOJOSHADER_freeParseData(a);
    } // if

    #if FINDERRORS_COMPILE_SHADERS
    MOJOSHADER_glShader *shader = MOJOSHADER_glCompileShader(buf, rc, NULL, 0);
    if (shader == NULL)
        report("FAIL: %s %s\n", fname, MOJOSHADER_glGetError());
    else
    {
        const MOJOSHADER_parseData *pd = MOJOSHADER_glGetShaderParseData(shader);
        MOJOSHADER_glShader *v = (pd->shader_type == MOJOSHADER_TYPE_VERTEX) ? shader : NULL;
        MOJOSHADER_glShader *p = (pd->shader_type == MOJOSHADER_TYPE_PIXEL) ? shader : NULL;
        MOJOSHADER_glProgram *program = MOJOSHADER_glLinkProgram(v, p);
        if (program == NULL)
            report("FAIL: %s %s\n", fname, MOJOSHADER_glGetError());
        else
        {
            report("PASS: %s\n", fname);
            MOJOSHADER_glDeleteProgram(program);
        } // else
        MOJOSHADER_glDeleteShader(shader);
    }
    #else
    const MOJOSHADER_parseData *pd = MOJOSHADER_parse(profile, buf, rc, NULL, 0, NULL, NULL, NULL);
    if (pd->error_count == 0)
        report("PASS: %s\n", fname);
	else
	{
		int i;
		for (i = 0; i < pd->error_count; i++)
		{
			report("FAIL: %s (position %d) %s\n", pd->errors[i].filename,
			       pd->errors[i].error_position, pd->errors[i].error);
		} // for
	} // else
    MOJOSHADER_freeParseData(pd);
    #endif

    return 1;
} // do_file


static int do_dir(const char *dname, const char *profile)
{
    int total = 0;

#ifdef _MSC_VER
    WIN32_FIND_DATA dent;
    HANDLE dirp = INVALID_HANDLE_VALUE;
    FindFirstFileA(dname, &dent);
    if (dirp != INVALID_HANDLE_VALUE)
    {
        do
        {
            if (!do_file(profile, dname, dent.cFileName, &total))
                break;
        } while (FindNextFileA(dirp, &dent) != 0);
        CloseHandle(dirp);
    } // if
#else
    struct dirent *dent = NULL;
    DIR *dirp = opendir(dname);
    if (dirp != NULL)
    {
        while ((dent = readdir(dirp)) != NULL)
        {
            if (!do_file(profile, dname, dent->d_name, &total))
                break;
        } // while
        closedir(dirp);
    } // if
#endif

    return total;
} // do_dir


int main(int argc, char **argv)
{
    //printf("MojoShader finderrors\n");
    //printf("Compiled against changeset %s\n", MOJOSHADER_CHANGESET);
    //printf("Linked against changeset %s\n", MOJOSHADER_changeset());
    //printf("\n");

    if (argc <= 2)
        printf("\n\nUSAGE: %s <profile> [dir1] ... [dirN]\n\n", argv[0]);
    else
    {
        int total = 0;
        int i;
        const char *profile = argv[1];

        #if FINDERRORS_COMPILE_SHADERS
        SDL_Init(SDL_INIT_VIDEO);
        SDL_GL_LoadLibrary(NULL);
        SDL_SetVideoMode(640, 480, 0, SDL_OPENGL);
        printf("Best profile is '%s'\n", MOJOSHADER_glBestProfile(lookup, 0));
        MOJOSHADER_glContext *ctx;
        ctx = MOJOSHADER_glCreateContext(profile, lookup, 0, 0, 0, 0);
        if (ctx == NULL)
        {
            printf("MOJOSHADER_glCreateContext() fail: %s\n", MOJOSHADER_glGetError());
            SDL_Quit();
            return 1;
        } // if
        MOJOSHADER_glMakeContextCurrent(ctx);
        #endif

        for (i = 2; i < argc; i++)
            total += do_dir(argv[i], profile);

        printf("Saw %d files.\n", total);

        #if FINDERRORS_COMPILE_SHADERS
        MOJOSHADER_glDestroyContext(ctx);
        SDL_Quit();
        #endif
    } // else

    return 0;
} // main

// end of finderrors.c ...

