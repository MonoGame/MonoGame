// PVRTexLibC.h

#pragma once


extern "C" {
    __declspec(dllexport) void* __cdecl CompressTexture(unsigned char* data, int height, int width, int mipLevels, bool preMultiplied, int** dataSizes);
}