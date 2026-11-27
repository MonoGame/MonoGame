// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

class Shader;
class PipelineLogger;

#include <string>
#include <vector>



/// <summary>
/// Used to register platform profiles that guide building of platform specific shader code.
/// </summary>
class ShaderProfile
{
protected:

    ShaderProfile() {};
    virtual ~ShaderProfile() {};

public:

    /// <summary>
    /// The shader profile name that must match the C# side of the pipeline.
    /// </summary>
    virtual bool Supports(const char* profileOrPlatform) const = 0;

    /// <summary>
    /// The double null terminated defines to force in the preprocessor.
    /// </summary>
    virtual const char* Defines() const = 0;

    /// <summary>
    /// The platform specific identifier for this shader profile.
    /// </summary>
    virtual uint8_t GetProfileId() const = 0;

    /// <summary>
    /// The Slang code generation target for generating final source.
    /// </summary>
    virtual int SlangCompileTarget() const = 0;

    /// <summary>
    /// The target profile Slang should use for generating final source.
    /// </summary>
    virtual const char* SlangTargetProfile() const = 0;

    /// <summary>
    /// Compile the source code to a shader compatible for this platform.
    /// </summary>
    virtual bool Compile(
        const std::string& filename,
        Shader& shader,
        PipelineLogger& log) = 0;
};


/// <summary>
/// Helper for registration and access to platform specific shader profiles.
/// </summary>
class ShaderProfileManager
{
private:
    static std::vector<ShaderProfile*>& profiles();

public:

    explicit ShaderProfileManager(ShaderProfile* profile);

    /// <summary>
    /// Returns the shader profile by profile or platform name.
    /// </summary>
    static ShaderProfile* Get(const char* profileOrPlatform);
};

#define REGISTER_SHADER_PROFILE(profile_type)       \
    static profile_type profile_type##_var;   \
    static ShaderProfileManager profile_var##_register( (ShaderProfile*)&profile_type##_var )

