// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "GraphicsEnums.h"

struct MGG_SamplerState_Info;

namespace Graphics {
class DeviceResources;

class Sampler {
public:
    Sampler(DeviceResources* device, MGG_SamplerState_Info* info);
    ~Sampler();

    struct InternalData {
        D3D12_CPU_DESCRIPTOR_HANDLE m_handle;
    };
    InternalData* impl;
};

}
