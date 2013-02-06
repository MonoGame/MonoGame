// PVRTexLibC.h

#pragma once

extern "C" {
    __declspec(dllexport) void* __cdecl compressPVRTC(unsigned char* data, int height, int width, int mipLevels, bool preMultiplied, bool fourbppCompression, int** dataSizes);
}
