/**
 * MojoShader; generate shader programs from bytecode of compiled
 *  Direct3D shaders.
 *
 * Please see the file LICENSE.txt in the source's root directory.
 *
 *  This file written by Ryan C. Gordon.
 */

#include <stdio.h>
#include "mojoshader.h"
#include "SDL.h"

static void *lookup(const char *fnname, void *unused)
{
    (void) unused;
    return SDL_GL_GetProcAddress(fnname);
} // lookup

static int check_available(void)
{
    const char **avail = NULL;
    int total = MOJOSHADER_glAvailableProfiles(lookup, NULL, NULL, 0);
    if (total > 0)
    {
        avail = (const char **) alloca(sizeof (const char *) * total);
        total = MOJOSHADER_glAvailableProfiles(lookup, NULL, avail, total);
    } // if

    if (total <= 0)
        fprintf(stderr, "No profiles available.\n");
    else
    {
        int i;
        for (i = 0; i < total; i++)
        {
            printf("%s (Shader Model %d)\n", avail[i],
                   MOJOSHADER_maxShaderModel(avail[i]));
        } // for
    } // else

    return 0;
} // check_available


int main(int argc, char **argv)
{
    int retval = 1;

    #if 0
    printf("MojoShader availableprofile\n");
    printf("Compiled against changeset %s\n", MOJOSHADER_CHANGESET);
    printf("Linked against changeset %s\n", MOJOSHADER_changeset());
    printf("\n");
    #endif

    if (SDL_Init(SDL_INIT_VIDEO) == -1)
        fprintf(stderr, "SDL_Init(SDL_INIT_VIDEO) error: %s\n", SDL_GetError());
    else
    {
        SDL_GL_LoadLibrary(NULL);
        if (SDL_SetVideoMode(640, 480, 0, SDL_OPENGL) == NULL)
            fprintf(stderr, "SDL_SetVideoMode() error: %s\n", SDL_GetError());
        else
            retval = check_available();
        SDL_Quit();
    } // else

    return retval;
} // main

// end of availableprofiles.c ...

