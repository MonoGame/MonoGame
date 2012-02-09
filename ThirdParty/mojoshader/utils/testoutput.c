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
#include "mojoshader.h"

static int do_parse(const unsigned char *buf, const int len, const char *prof)
{
    const MOJOSHADER_parseData *pd;
    int retval = 0;

    pd = MOJOSHADER_parse(prof, buf, len, NULL, 0, NULL, NULL, NULL);
    if (pd->error_count > 0)
    {
        int i;
        for (i = 0; i < pd->error_count; i++)
        {
            printf("%s:%d: ERROR: %s\n",
                    pd->errors[i].filename ? pd->errors[i].filename : "???",
                    pd->errors[i].error_position,
                    pd->errors[i].error);
        } // for
    } // if
    else
    {
        retval = 1;
        if (pd->output != NULL)
        {
            int i;
            for (i = 0; i < pd->output_len; i++)
                putchar((int) pd->output[i]);
            printf("\n");
        } // if
    } // else
    printf("\n\n");
    MOJOSHADER_freeParseData(pd);

    return retval;
} // do_parse


int main(int argc, char **argv)
{
    int retval = 0;

    if (argc <= 2)
        printf("\n\nUSAGE: %s <profile> [file1] ... [fileN]\n\n", argv[0]);
    else
    {
        const char *profile = argv[1];
        int i;

        for (i = 2; i < argc; i++)
        {
            FILE *io = fopen(argv[i], "rb");
            if (io == NULL)
                printf(" ... fopen('%s') failed.\n", argv[i]);
            else
            {
                unsigned char *buf = (unsigned char *) malloc(1000000);
                int rc = fread(buf, 1, 1000000, io);
                fclose(io);
                if (!do_parse(buf, rc, profile))
                    retval = 1;
                free(buf);
            } // else
        } // for
    } // else

    return retval;
} // main

// end of testoutput.c ...

