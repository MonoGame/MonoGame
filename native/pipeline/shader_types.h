// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include <string>
#include <vector>


// TODO: These are copies from the mgruntime api_enums.h.
// We should consider how we can include it directly here.

enum class MGTextureAddressMode : int
{
	Wrap = 0,
	Clamp = 1,
	Mirror = 2,
	Border = 3,
};

enum class MGTextureFilter : uint8_t
{
	None = -1,
	Linear = 0,
	Point = 1,
	Anisotropic = 2,
	LinearMipPoint = 3,
	PointMipLinear = 4,
	MinLinearMagPointMipLinear = 5,
	MinLinearMagPointMipPoint = 6,
	MinPointMagLinearMipLinear = 7,
	MinPointMagLinearMipPoint = 8,
};

/// <summary>
/// The shaders stages we support. Matches MG C# type.
/// </summary>
enum class ShaderType
{
	Vertex,
	Pixel,
};

/// <summary>
/// The types of sampler. Matches MG C# type.
/// </summary>
enum class SamplerType
{
	Sampler2D,
	SamplerCube,
	SamplerVolume,
	Sampler1D,
};

class SamplerState
{
public:

	uint8_t AddressU;
	uint8_t AddressV;
	uint8_t AddressW;
	uint8_t BorderColorR;
	uint8_t BorderColorG;
	uint8_t BorderColorB;
	uint8_t BorderColorA;
	MGTextureFilter Filter;
	int MaxAnisotropy;
	int MaxMipLevel;
	float MipMapLevelOfDetailBias;
};


class SamplerInfo
{
public:
	std::string name;

	SamplerType type = SamplerType::Sampler2D;
	uint8_t param_index = 0;
	uint8_t sampler_slot = 0;
	uint8_t texture_slot = 0;

	SamplerState* state = nullptr;
};


class ShaderState
{
public:
	ShaderType type;
	std::string model;
	std::string entrypoint;
	std::string deslag_source;

	bool isValid() const { return entrypoint.size() > 0; }
};

class Pass
{
public:

	std::string name;
	ShaderState pixel;
	ShaderState vertex;
	int32_t pixel_index = -1;
	int32_t vertex_index = -1;

	bool hasShader() const { return pixel.isValid() || vertex.isValid(); }
};

class Technique
{
public:
	std::string name;
	std::vector<Pass> passes;
};

/// <summary>
/// The input vertex element usage. Matches MG C# type.
/// </summary>
enum class VertexUsage : uint8_t
{
    Position,
    Color,
    TextureCoordinate,
    Normal,
    Binormal,
    Tangent,
    BlendIndices,
    BlendWeight,
    Depth,
    Fog,
    PointSize,
    Sample,
    TessellateFactor
};

class InputAttribute
{
public:
	std::string name;
	VertexUsage usage;
	uint8_t index;
	uint16_t location;
};


/// <summary>
/// The class of the effect parameter. Matches MG C# type.
/// </summary>
enum class EffectParameterClass : uint8_t
{
	Scalar,
	Vector,
	Matrix,
	Object,
	Struct
};

/// <summary>
/// The type of the effect parameter. Matches MG C# type.
/// </summary>
enum class EffectParameterType : uint8_t
{
	Void,
	Bool,
	Int32,
	Single,
	String,
	Texture,
	Texture1D,
	Texture2D,
	Texture3D,
	TextureCube
};


class EffectParameter
{
public:
	EffectParameter()
	{
	}

	EffectParameterClass class_;
	EffectParameterType type;
	std::string name;

	uint8_t rows;
	uint8_t columns;

	std::vector<EffectParameter> elements;
	std::vector<EffectParameter> members;

	std::vector<uint8_t> data;

	// TODO: This needs to store what constant
	// buffer this effect parameter belongs too!
	//
	// Currently it assume this is part of the single
	// global constant buffer 0.
		
	uint32_t offset;
};

class ConstantBuffer
{
public:
	std::string name;
	uint32_t reg;
	size_t size;

	struct Param
	{
		uint32_t index;
		uint16_t offset;
	};

	std::vector<Param> params;

	std::vector<EffectParameter> parameters;

	bool SameAs(const ConstantBuffer& other)
	{
		// If the names of the constant buffers don't
		// match then consider them different right off 
		// the bat... even if their parameters are the same.
		if (name != other.name)
			return false;

		// Do we have the same count of parameters and size?
		if (size != other.size ||
			parameters.size() != other.parameters.size())
			return false;

		// Compare the parameters themselves.
		for (int i = 0; i < parameters.size(); i++)
		{
			const EffectParameter& p1 = parameters[i];
			const EffectParameter& p2 = other.parameters[i];

			// Check the importaint bits.
			if (p1.name != p2.name ||
				p1.rows != p2.rows ||
				p1.columns != p2.columns ||
				p1.class_ != p2.class_ ||
				p1.type != p2.type ||
				p1.offset != p2.offset)
				return false;
		}

		// These are equal.
		return true;
	}
};


class Shader
{
public:
	Shader(ShaderState& state)
	{
		type = state.type;
		entrypoint = state.entrypoint;
		debug = false;
	}

	ShaderType type;
	std::string entrypoint;
	std::string source;
	bool debug;
	std::string debug_symbol_path;

	std::vector<ConstantBuffer> cbuffers;

	std::vector<uint8_t> binary;

	std::vector<uint8_t> cbuffer_index;

	std::vector<EffectParameter> parameters;
	std::vector<InputAttribute> attributes;
	std::vector<SamplerInfo> samplers;
};

