#include "../include/api_MGP.h"

// browser host owns frame loop
// native runtime only needs a no-op export for the shared interop surface.
MG_EXPORT void MGP_Platform_StartRunLoop(MGP_Platform* platform)
{
    (void)platform;
}
