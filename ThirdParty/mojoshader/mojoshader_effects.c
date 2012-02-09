/**
 * MojoShader; generate shader programs from bytecode of compiled
 *  Direct3D shaders.
 *
 * Please see the file LICENSE.txt in the source's root directory.
 *
 *  This file written by Ryan C. Gordon.
 */

#define __MOJOSHADER_INTERNAL__ 1
#include "mojoshader_internal.h"

#include <math.h>

void MOJOSHADER_runPreshader(const MOJOSHADER_preshader *preshader,
                             const float *inregs, float *outregs)
{
    // this is fairly straightforward, as there aren't any branching
    //  opcodes in the preshader instruction set (at the moment, at least).
    const int scalarstart = (int) MOJOSHADER_PRESHADEROP_SCALAR_OPS;

    double *temps = NULL;
    if (preshader->temp_count > 0)
    {
        temps = (double *) alloca(sizeof (double) * preshader->temp_count);
        memset(temps, '\0', sizeof (double) * preshader->temp_count);
    } // if

    double dst[4] = { 0, 0, 0, 0 };
    double src[3][4] = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
    const double *src0 = &src[0][0];
    const double *src1 = &src[1][0];
    const double *src2 = &src[2][0];

    MOJOSHADER_preshaderInstruction *inst = preshader->instructions;
    int instit;

    for (instit = 0; instit < preshader->instruction_count; instit++, inst++)
    {
        const MOJOSHADER_preshaderOperand *operand = inst->operands;
        const int elems = inst->element_count;
        const int elemsbytes = sizeof (double) * elems;
        const int isscalarop = (inst->opcode >= scalarstart);

        assert(elems >= 0);
        assert(elems <= 4);

        // load up our operands...
        int opiter, elemiter;
        for (opiter = 0; opiter < inst->operand_count-1; opiter++, operand++)
        {
            const int isscalar = ((isscalarop) && (opiter == 0));
            unsigned int index = operand->index;
            switch (operand->type)
            {
                case MOJOSHADER_PRESHADEROPERAND_LITERAL:
                {
                    const double *lit = &preshader->literals[index];
                    assert((index + elems) <= preshader->literal_count);
                    if (!isscalar)
                        memcpy(&src[opiter][0], lit, elemsbytes);
                    else
                    {
                        const double val = *lit;
                        for (elemiter = 0; elemiter < elems; elemiter++)
                            src[opiter][elemiter] = val;
                    } // else
                    break;
                } // case

                case MOJOSHADER_PRESHADEROPERAND_INPUT:
                    if (operand->indexingType == 2) {
                        index = index+inregs[operand->indexingIndex]*4;
                    }
                    if (isscalar)
                        src[opiter][0] = inregs[index];
                    else
                    {
                        int cpy;
                        for (cpy = 0; cpy < elems; cpy++)
                            src[opiter][cpy] = inregs[index+cpy];
                    } // else
                    break;

                case MOJOSHADER_PRESHADEROPERAND_OUTPUT:
                    if (isscalar)
                        src[opiter][0] = outregs[index];
                    else
                    {
                        int cpy;
                        for (cpy = 0; cpy < elems; cpy++)
                            src[opiter][cpy] = outregs[index+cpy];
                    } // else
                    break;

                case MOJOSHADER_PRESHADEROPERAND_TEMP:
                    if (temps != NULL)
                    {
                        if (isscalar)
                            src[opiter][0] = temps[index];
                        else
                            memcpy(src[opiter], temps + index, elemsbytes);
                    } // if
                    break;

                default:
                    assert(0 && "unexpected preshader operand type.");
                    return;
            } // switch
        } // for

        // run the actual instruction, store result to dst.
        int i;
        switch (inst->opcode)
        {
            #define OPCODE_CASE(op, val) \
                case MOJOSHADER_PRESHADEROP_##op: \
                    for (i = 0; i < elems; i++) { dst[i] = val; } \
                    break;

            //OPCODE_CASE(NOP, 0.0)  // not a real instruction.
            OPCODE_CASE(MOV, src0[i])
            OPCODE_CASE(NEG, -src0[i])
            OPCODE_CASE(RCP, 1.0 / src0[i])
            OPCODE_CASE(FRC, src0[i] - floor(src0[i]))
            OPCODE_CASE(EXP, exp(src0[i]))
            OPCODE_CASE(LOG, log(src0[i]))
            OPCODE_CASE(RSQ, 1.0 / sqrt(src0[i]))
            OPCODE_CASE(SIN, sin(src0[i]))
            OPCODE_CASE(COS, cos(src0[i]))
            OPCODE_CASE(ASIN, asin(src0[i]))
            OPCODE_CASE(ACOS, acos(src0[i]))
            OPCODE_CASE(ATAN, atan(src0[i]))
            OPCODE_CASE(MIN, (src0[i] < src1[i]) ? src0[i] : src1[i])
            OPCODE_CASE(MAX, (src0[i] > src1[i]) ? src0[i] : src1[i])
            OPCODE_CASE(LT, (src0[i] < src1[i]) ? 1.0 : 0.0)
            OPCODE_CASE(GE, (src0[i] >= src1[i]) ? 1.0 : 0.0)
            OPCODE_CASE(ADD, src0[i] + src1[i])
            OPCODE_CASE(MUL,  src0[i] * src1[i])
            OPCODE_CASE(ATAN2, atan2(src0[i], src1[i]))
            OPCODE_CASE(DIV, src0[i] / src1[i])
            OPCODE_CASE(CMP, (src0[i] >= 0.0) ? src1[i] : src2[i])
            //OPCODE_CASE(NOISE, ???)  // !!! FIXME: don't know what this does
            //OPCODE_CASE(MOVC, ???)  // !!! FIXME: don't know what this does
            OPCODE_CASE(MIN_SCALAR, (src0[0] < src1[i]) ? src0[0] : src1[i])
            OPCODE_CASE(MAX_SCALAR, (src0[0] > src1[i]) ? src0[0] : src1[i])
            OPCODE_CASE(LT_SCALAR, (src0[0] < src1[i]) ? 1.0 : 0.0)
            OPCODE_CASE(GE_SCALAR, (src0[0] >= src1[i]) ? 1.0 : 0.0)
            OPCODE_CASE(ADD_SCALAR, src0[0] + src1[i])
            OPCODE_CASE(MUL_SCALAR, src0[0] * src1[i])
            OPCODE_CASE(ATAN2_SCALAR, atan2(src0[0], src1[i]))
            OPCODE_CASE(DIV_SCALAR, src0[0] / src1[i])
            //OPCODE_CASE(DOT_SCALAR)  // !!! FIXME: isn't this just a MUL?
            //OPCODE_CASE(NOISE_SCALAR, ???)  // !!! FIXME: ?
            #undef OPCODE_CASE

            case MOJOSHADER_PRESHADEROP_DOT:
            {
                double final = 0.0;
                for (i = 0; i < elems; i++)
                    final += src0[i] * src1[i];
                for (i = 0; i < elems; i++)
                    dst[i] = final;  // !!! FIXME: is this right?
            } // case

            default:
                assert(0 && "Unhandled preshader opcode!");
                break;
        } // switch

        // Figure out where dst wants to be stored.
        if (operand->type == MOJOSHADER_PRESHADEROPERAND_TEMP)
        {
            assert(preshader->temp_count >=
                    operand->index + (elemsbytes / sizeof (double)));
            memcpy(temps + operand->index, dst, elemsbytes);
        } // if
        else
        {
            assert(operand->type == MOJOSHADER_PRESHADEROPERAND_OUTPUT);
            for (i = 0; i < elems; i++)
                outregs[operand->index + i] = (float) dst[i];
        } // else
    } // for
} // MOJOSHADER_runPreshader

static MOJOSHADER_effect MOJOSHADER_out_of_mem_effect = {
    1, &MOJOSHADER_out_of_mem_error, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
};

static uint32 readui32(const uint8 **_ptr, uint32 *_len)
{
    uint32 retval = 0;
    if (*_len < sizeof (retval))
        *_len = 0;
    else
    {
        const uint32 *ptr = (const uint32 *) *_ptr;
        retval = SWAP32(*ptr);
        *_ptr += sizeof (retval);
        *_len -= sizeof (retval);
    } // else
    return retval;
} // readui32

// !!! FIXME: this is sort of a big, ugly function.
const MOJOSHADER_effect *MOJOSHADER_parseEffect(const char *profile,
                                                const unsigned char *buf,
                                                const unsigned int _len,
                                                const MOJOSHADER_swizzle *swiz,
                                                const unsigned int swizcount,
                                                MOJOSHADER_malloc m,
                                                MOJOSHADER_free f,
                                                void *d)
{
    if ( ((m == NULL) && (f != NULL)) || ((m != NULL) && (f == NULL)) )
        return &MOJOSHADER_out_of_mem_effect;  // supply both or neither.

    if (m == NULL) m = MOJOSHADER_internal_malloc;
    if (f == NULL) f = MOJOSHADER_internal_free;

    MOJOSHADER_effect *retval = m(sizeof (MOJOSHADER_effect), d);
    if (retval == NULL)
        return &MOJOSHADER_out_of_mem_effect;  // supply both or neither.
    memset(retval, '\0', sizeof (*retval));

    retval->malloc = m;
    retval->free = f;
    retval->malloc_data = d;

    const uint8 *ptr = (const uint8 *) buf;
    uint32 len = (uint32) _len;
    size_t siz = 0;
    int i, j, k;

    if (len < 8)
        goto parseEffect_unexpectedEOF;

    const uint8 *base = NULL;
    if (readui32(&ptr, &len) != 0xFEFF0901) // !!! FIXME: is this always magic?
        goto parseEffect_notAnEffectsFile;
    else
    {
        const uint32 offset = readui32(&ptr, &len);
        base = ptr;
//printf("base offset == %u\n", offset);
        if (offset > len)
            goto parseEffect_unexpectedEOF;
        ptr += offset;
        len -= offset;
    } // else

    // params...

    if (len < 16)
        goto parseEffect_unexpectedEOF;

    const uint32 numparams = readui32(&ptr, &len);
    const uint32 numtechniques = readui32(&ptr, &len);

    readui32(&ptr, &len); // !!! FIXME: there are 8 unknown bytes here. Annotations?
    /*const uint32 numobjects = */ readui32(&ptr, &len);

    if (numparams > 0)
    {
        siz = sizeof (MOJOSHADER_effectParam) * numparams;
        retval->params = (MOJOSHADER_effectParam *) m(siz, d);
        if (retval->params == NULL)
            goto parseEffect_outOfMemory;
        memset(retval->params, '\0', siz);

        retval->param_count = numparams;

        for (i = 0; i < numparams; i++)
        {
            if (len < 16)
                goto parseEffect_unexpectedEOF;

            const uint32 typeoffset = readui32(&ptr, &len);
            /*const uint32 valoffset =*/ readui32(&ptr, &len);
            /*const uint32 flags =*/ readui32(&ptr, &len);
            const uint32 numannos = readui32(&ptr, &len);
            for (j = 0; j < numannos; j++)
            {
                if (len < 8)
                    goto parseEffect_unexpectedEOF;
                // !!! FIXME: parse annotations.
                readui32(&ptr, &len);
                readui32(&ptr, &len);
            } // for

            const uint8 *typeptr = base + typeoffset;
            unsigned int typelen = 9999999;  // !!! FIXME
            /*const uint32 paramtype =*/ readui32(&typeptr, &typelen);
            /*const uint32 paramclass =*/ readui32(&typeptr, &typelen);
            const uint32 paramname = readui32(&typeptr, &typelen);
            const uint32 paramsemantic = readui32(&typeptr, &typelen);

            // !!! FIXME: sanity checks!
            const char *namestr = ((const char *) base) + paramname;
            const char *semstr = ((const char *) base) + paramsemantic;
            uint32 len;
            char *strptr;
            len = *((const uint32 *) namestr);
            strptr = (char *) m(len + 1, d);
            memcpy(strptr, namestr + 4, len);
            strptr[len] = '\0';
            retval->params[i].name = strptr;
            len = *((const uint32 *) semstr);
            strptr = (char *) m(len + 1, d);
            memcpy(strptr, semstr + 4, len);
            strptr[len] = '\0';
            retval->params[i].semantic = strptr;
        } // for
    } // if

    uint32 numshaders = 0;  // we'll calculate this later.

    // techniques...

    if (numtechniques > 0)
    {
        siz = sizeof (MOJOSHADER_effectTechnique) * numtechniques;
        retval->techniques = (MOJOSHADER_effectTechnique *) m(siz, d);
        if (retval->techniques == NULL)
            goto parseEffect_outOfMemory;
        memset(retval->techniques, '\0', siz);

        retval->technique_count = numtechniques;

        for (i = 0; i < numtechniques; i++)
        {
            if (len < 12)
                goto parseEffect_unexpectedEOF;
            
            MOJOSHADER_effectTechnique *technique = &retval->techniques[i];

            const uint32 nameoffset = readui32(&ptr, &len);
            const uint32 numannos = readui32(&ptr, &len);
            const uint32 numpasses = readui32(&ptr, &len);

            if (nameoffset >= _len)
                goto parseEffect_unexpectedEOF;

            if (numannos > 0)
            {
                // !!! FIXME: expose these to the caller?
                for (j = 0; j < numannos; j++)
                {
                    if (len < 8)
                        goto parseEffect_unexpectedEOF;
                    readui32(&ptr, &len);  // typedef offset
                    readui32(&ptr, &len);  // value offset
                } // for
            } // if

            // !!! FIXME: verify this doesn't go past EOF looking for a null.
            {
                const char *namestr = ((char *) base) + nameoffset;
                uint32 len = *((const uint32 *) namestr);
                char *strptr = (char *) m(len + 1, d);
                memcpy(strptr, namestr + 4, len);
                strptr[len] = '\0';
                technique->name = strptr;
            }

            if (numpasses > 0)
            {
                technique->pass_count = numpasses;

                siz = sizeof (MOJOSHADER_effectPass) * numpasses;
                technique->passes = (MOJOSHADER_effectPass *) m(siz, d);
                if (technique->passes == NULL)
                    goto parseEffect_outOfMemory;
                memset(technique->passes, '\0', siz);

                for (j = 0; j < numpasses; j++)
                {
                    if (len < 12)
                        goto parseEffect_unexpectedEOF;

                    MOJOSHADER_effectPass *pass = &technique->passes[j];

                    const uint32 passnameoffset = readui32(&ptr, &len);
                    const uint32 numannos = readui32(&ptr, &len);
                    const uint32 numstates = readui32(&ptr, &len);

                    if (passnameoffset >= _len)
                        goto parseEffect_unexpectedEOF;

                    // !!! FIXME: verify this doesn't go past EOF looking for a null.
                    {
                        const char *namestr = ((char *) base) + passnameoffset;
                        uint32 len = *((const uint32 *) namestr);
                        char *strptr = (char *) m(len + 1, d);
                        memcpy(strptr, namestr + 4, len);
                        strptr[len] = '\0';
                        pass->name = strptr;
                    }

                    if (numannos > 0)
                    {
                        for (k = 0; k < numannos; k++)
                        {
                            if (len < 8)
                                goto parseEffect_unexpectedEOF;
                            // !!! FIXME: do something with this.
                            readui32(&ptr, &len);
                            readui32(&ptr, &len);
                        } // for
                    } // if

                    if (numstates > 0)
                    {
                        pass->state_count = numstates;

                        siz = sizeof (MOJOSHADER_effectState) * numstates;
                        pass->states = (MOJOSHADER_effectState *) m(siz, d);
                        if (pass->states == NULL)
                            goto parseEffect_outOfMemory;
                        memset(pass->states, '\0', siz);

                        for (k = 0; k < numstates; k++)
                        {
                            if (len < 16)
                                goto parseEffect_unexpectedEOF;

                            MOJOSHADER_effectState *state = &pass->states[k];
                            const uint32 type = readui32(&ptr, &len);
                            readui32(&ptr, &len);  // !!! FIXME: don't know what this field does.
                            /*const uint32 offsetend = */ readui32(&ptr, &len);
                            /*const uint32 offsetstart = */ readui32(&ptr, &len);
                            state->type = type;

                            if ((type == 0x92) || (type == 0x93))
                                numshaders++;
                        } // for
                    } // if
                } // for
            } // if
        } // for
    } // if

    // textures...

    if (len < 8)
        goto parseEffect_unexpectedEOF;

    const int numtextures = readui32(&ptr, &len);
    const int numobjects = readui32(&ptr, &len);  // !!! FIXME: "objects" for lack of a better word.

    if (numtextures > 0)
    {
        siz = sizeof (MOJOSHADER_effectTexture) * numtextures;
        retval->textures = m(siz, d);
        if (retval->textures == NULL)
            goto parseEffect_outOfMemory;
        memset(retval->textures, '\0', siz);

        for (i = 0; i < numtextures; i++)
        {
            if (len < 8)
                goto parseEffect_unexpectedEOF;

            MOJOSHADER_effectTexture *texture = &retval->textures[i];
            const uint32 texparam = readui32(&ptr, &len);
            const uint32 texsize = readui32(&ptr, &len);
            // apparently texsize will pad out to 32 bits.
            const uint32 readsize = (((texsize + 3) / 4) * 4);
            if (len < readsize)
                goto parseEffect_unexpectedEOF;

            texture->param = texparam;
            char *str = m(texsize + 1, d);
            if (str == NULL)
                goto parseEffect_outOfMemory;
            memcpy(str, ptr, texsize);
            str[texsize] = '\0';
            texture->name = str;

            ptr += readsize;
            len -= readsize;
        } // for
    } // if

    // shaders...

    if (numshaders > 0)
    {
        siz = sizeof (MOJOSHADER_effectShader) * numshaders;
        retval->shaders = (MOJOSHADER_effectShader *) m(siz, d);
        if (retval->shaders == NULL)
            goto parseEffect_outOfMemory;
        memset(retval->shaders, '\0', siz);

        retval->shader_count = numshaders;

        // !!! FIXME: I wonder if we should pull these from offsets and not
        // !!! FIXME:  count on them all being in a line like this.
        for (i = 0; i < numshaders; i++)
        {
            if (len < 24)
                goto parseEffect_unexpectedEOF;

            MOJOSHADER_effectShader *shader = &retval->shaders[i];
            const uint32 technique = readui32(&ptr, &len);
            const uint32 pass = readui32(&ptr, &len);
            readui32(&ptr, &len);  // !!! FIXME: don't know what this does.
            readui32(&ptr, &len);  // !!! FIXME: don't know what this does (vertex/pixel/geometry?)
            readui32(&ptr, &len);  // !!! FIXME: don't know what this does.
            const uint32 shadersize = readui32(&ptr, &len);

            if (len < shadersize)
                goto parseEffect_unexpectedEOF;

            shader->technique = technique;
            shader->pass = pass;
            shader->shader = MOJOSHADER_parse(profile, ptr, shadersize,
                                              swiz, swizcount, m, f, d);

            // !!! FIXME: check for errors.

            ptr += shadersize;
            len -= shadersize;
        } // for
    } // if

    // !!! FIXME: we parse this, but don't expose the data, yet.
    // mappings ...
    assert(numshaders <= numobjects);
    const uint32 nummappings = numobjects - numshaders;
    if (nummappings > 0)
    {
        for (i = 0; i < nummappings; i++)
        {
            if (len < 24)
                goto parseEffect_unexpectedEOF;

            /*const uint32 magic = */ readui32(&ptr, &len);
            /*const uint32 index = */ readui32(&ptr, &len);
            readui32(&ptr, &len);  // !!! FIXME: what is this field?
            readui32(&ptr, &len);  // !!! FIXME: what is this field?
            /*const uint32 type = */ readui32(&ptr, &len);
            const uint32 mapsize = readui32(&ptr, &len);
            if (mapsize > 0)
            {
                const uint32 readsize = (((mapsize + 3) / 4) * 4);
                if (len < readsize)
                    goto parseEffect_unexpectedEOF;
            } // if
        } // for
    } // if

    retval->profile = (char *) m(strlen(profile) + 1, d);
    if (retval->profile == NULL)
        goto parseEffect_outOfMemory;
    strcpy((char *) retval->profile, profile);

    return retval;


// !!! FIXME: do something with this.
parseEffect_notAnEffectsFile:
parseEffect_unexpectedEOF:
parseEffect_outOfMemory:
    MOJOSHADER_freeEffect(retval);
    return &MOJOSHADER_out_of_mem_effect;
} // MOJOSHADER_parseEffect


void MOJOSHADER_freeEffect(const MOJOSHADER_effect *_effect)
{
    MOJOSHADER_effect *effect = (MOJOSHADER_effect *) _effect;
    if ((effect == NULL) || (effect == &MOJOSHADER_out_of_mem_effect))
        return;  // no-op.

    MOJOSHADER_free f = effect->free;
    void *d = effect->malloc_data;
    int i, j;

    for (i = 0; i < effect->error_count; i++)
    {
        f((void *) effect->errors[i].error, d);
        f((void *) effect->errors[i].filename, d);
    } // for
    f((void *) effect->errors, d);

    f((void *) effect->profile, d);

    for (i = 0; i < effect->param_count; i++)
    {
        f((void *) effect->params[i].name, d);
        f((void *) effect->params[i].semantic, d);
    } // for
    f(effect->params, d);

    for (i = 0; i < effect->technique_count; i++)
    {
        MOJOSHADER_effectTechnique *technique = &effect->techniques[i];
        f((void *) technique->name, d);
        for (j = 0; j < technique->pass_count; j++)
        {
            f((void *) technique->passes[j].name, d);
            f(technique->passes[j].states, d);
        } // for
        f(technique->passes, d);
    } // for

    f(effect->techniques, d);

    for (i = 0; i < effect->texture_count; i++)
        f((void *) effect->textures[i].name, d);
    f(effect->textures, d);

    for (i = 0; i < effect->shader_count; i++)
        MOJOSHADER_freeParseData(effect->shaders[i].shader);
    f(effect->shaders, d);

    f(effect, d);
} // MOJOSHADER_freeEffect

// end of mojoshader_effects.c ...

