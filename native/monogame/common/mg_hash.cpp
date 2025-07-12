// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "mg_common.h"


mguint MG_ComputeHash(const mgbyte* value, mgint length, mguint result)
{
    // This is a 32-bit FNV-1a hash based on public domain code from:
    // http://www.isthe.com/chongo/tech/comp/fnv

    while (length-- > 0)
    {
        result ^= (mguint)*value;
        result +=
            (result << 1) +
            (result << 4) +
            (result << 7) +
            (result << 8) +
            (result << 24);
        ++value;
    }

    return result;
}

mguint MG_ComputeHash(const mgbyte* value, mgint length)
{
    // This function now starts a new hash chain by providing the
    // standard FNV-1a offset basis as the initial result.
    return MG_ComputeHash(value, length, (mguint)0x811c9dc5);
}

mguint MG_ComputeHash(mguint value, mguint result)
{
    // This function now hashes a single integer by treating it
    // as a block of bytes.
    return MG_ComputeHash(reinterpret_cast<const mgbyte*>(&value), sizeof(mguint), result);
}