#include "mg_common.h"

inline void MG_OnLoad();
inline void MG_OnUnload();

#if defined(_WIN32)
#include <windows.h>
#include <crtdbg.h>
#include <stdexcept>

static int MG_ReportHook(int reportType, char* message, int* returnValue)
{
    if (reportType != _CRT_ASSERT && reportType != _CRT_ERROR) {
        return FALSE;
    }

    if (IsDebuggerPresent()) {
        if (returnValue)
		*returnValue = 1;
        return TRUE;
    }
    else {
		throw std::runtime_error(message);
    }
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
        MG_OnLoad();
        break;
    case DLL_PROCESS_DETACH:
        MG_OnUnload();
        break;
    }
    return TRUE;
}

#elif defined(__GNUC__)
__attribute__((constructor)) static void mg_library_on_load() {
    MG_OnLoad();
}
__attribute__((destructor)) static void mg_library_on_unload() {
    MG_OnUnload();
}
#endif

inline void MG_OnLoad() {
    #if defined(DEBUG) && defined(_WIN32)
        _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
        _set_error_mode(_OUT_TO_STDERR);
        _CrtSetReportHook(MG_ReportHook);
    #endif
}

inline void MG_OnUnload() {
    
}
