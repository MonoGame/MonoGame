// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "include.fxh"

#define TEST 1

#if TEST == 0
Foo
#elif TEST == 1
Bar
#else
Baz
#endif

#if defined(TEST2)
FOO
#elif defined(TEST3)
BAR
#endif