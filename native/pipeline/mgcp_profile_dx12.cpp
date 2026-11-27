// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "shader_profiles.h"
#include "pipeline_logger.h"
#include "shader_types.h"
#include "std_helpers.h"
#include "memstream.h"

#include <slang.h>
#include <combaseapi.h>
#include <memory>
#include <string>
#include <map>

// Stuff for COM light DXC API.
#ifndef STDMETHODCALLTYPE
#define STDMETHODCALLTYPE
#endif
#ifndef STDMETHODVCALLTYPE
#define STDMETHODVCALLTYPE
#endif
#ifndef STDAPICALLTYPE
#define STDAPICALLTYPE
#endif
#ifndef Interface
#define Interface struct
#endif

#include <dxcapi.h>
#include <d3d12shader.h>

//#pragma comment(lib, "dxcompiler.lib")


template <typename T>
class DxcPtr
{
private:
	T* _ptr;

public:
	DxcPtr() noexcept
	{
		_ptr = nullptr;
	}

	DxcPtr(T* ptr) noexcept
	{
		_ptr = ptr;
	}

	DxcPtr(const DxcPtr& p) = delete;
	DxcPtr& operator=(const DxcPtr&) = delete;

	~DxcPtr()
	{
		if (_ptr)
			_ptr->Release();
	}

	operator T*() noexcept
	{
		return _ptr;
	}

	T* operator ->() noexcept
	{
		return _ptr;
	}

	operator T**() noexcept
	{
		return &_ptr;
	}

	operator bool () noexcept
	{
		return _ptr != nullptr;
	}


	// Called from COM calls that will assign a new ptr.
	T** ppv()
	{
		// Release first.
		if (_ptr)
			_ptr->Release();

		return &_ptr;
	}

	DxcPtr& operator=(T* ptr)
	{
		if (_ptr)
			_ptr->Release();
		_ptr = ptr;
	}

	bool operator == (std::nullptr_t) noexcept
	{
		return _ptr == nullptr;
	}

	bool operator != (std::nullptr_t) noexcept
	{
		return _ptr != nullptr;
	}
};

class DirectX12ShaderProfile : public ShaderProfile
{
protected:
	bool _initialized = false;
	DxcPtr<IDxcUtils> _utils;
	DxcPtr<IDxcCompiler3> _compiler;
	DxcPtr<IDxcContainerBuilder> _builder;

	bool Initialize();

public:

	DirectX12ShaderProfile() {}
	virtual ~DirectX12ShaderProfile();

	bool Supports(const char* profileOrPlatform) const
	{
		if (iequals(profileOrPlatform, "DirectX_12"))
			return true;

		if (iequals(profileOrPlatform, "WindowsDX12") ||
			iequals(profileOrPlatform, "XboxSeries") ||
			iequals(profileOrPlatform, "XboxOne"))
			return true;

		return false;
	}

	const char* Defines() const { return "SM6\0HLSL\0DX12\0\0"; }
	uint8_t GetProfileId() const { return 2; }
	int SlangCompileTarget() const { return SLANG_HLSL; }
	const char* SlangTargetProfile() const { return "sm_6_0"; }

	bool Compile(
		const std::string& filename,
		Shader& shader,
		PipelineLogger& log);
};


REGISTER_SHADER_PROFILE(DirectX12ShaderProfile);


bool DirectX12ShaderProfile::Initialize()
{
	if (_initialized)
		return true;

	// Initialze the shared parts of DXC once.

	HRESULT hr;

	hr = DxcCreateInstance(CLSID_DxcUtils, IID_PPV_ARGS(_utils.ppv()));
	if (FAILED(hr))
		return false;

	hr = DxcCreateInstance(CLSID_DxcCompiler, IID_PPV_ARGS(_compiler.ppv()));
	if (FAILED(hr))
		return false;

	DxcCreateInstance(CLSID_DxcContainerBuilder, IID_PPV_ARGS(_builder.ppv()));
	if (FAILED(hr))
		return false;
	
	_initialized = true;
	return true;
}

DirectX12ShaderProfile::~DirectX12ShaderProfile()
{
}

static std::string DeslangIdentifier(std::string identifier)
{
	// Slang adds a `_0` or other index to the end of
	// member names so we scrub them here.					
	identifier.resize(identifier.find_last_of('_'));
	return identifier;
}

bool DirectX12ShaderProfile::Compile(
	const std::string& filename,
	Shader& shader,
	PipelineLogger& log)
{
	if (!Initialize())
	{
		log.error("Failed to initialize DXC.");
		return false;
	}

	HRESULT hr;

	DxcBuffer source_buffer;
	source_buffer.Ptr = shader.source.c_str();
	source_buffer.Size = shader.source.length();
	source_buffer.Encoding = DXC_CP_UTF8;

	std::wstring name(filename.cbegin(), filename.cend());
	std::wstring entry(shader.entrypoint.cbegin(), shader.entrypoint.cend());
	std::wstring profile;
	std::wstring symbol_path;

	std::string type_string;

	switch(shader.type)
	{
	default:
		log.error("Unknown shader type: ", (int)shader.type);
		return false;
	case ShaderType::Vertex:
		profile = L"vs_6_0";
		type_string = "_vs";
		break;
	case ShaderType::Pixel:
		profile = L"ps_6_0";
		type_string = "_ps";
		break;
	}

	std::vector<LPCWSTR> arguments;
	arguments.push_back(name.c_str());
	arguments.push_back(L"-E"); arguments.push_back(entry.c_str());
	arguments.push_back(L"-T"); arguments.push_back(profile.c_str());

	// Pack matrix data in column-major order.
	arguments.push_back(L"-Zpc");

	// Strip reflection from the main blob but it
	// still provides reflection in the output stream.
	arguments.push_back(L"-Qstrip_reflect");

	// Enable debug vs release build features.
	if (shader.debug)
	{
		arguments.push_back(L"-Zi");

		std::string debug_path = "\"";
		debug_path += shader.debug_symbol_path;
		debug_path += "\\";
		debug_path += get_filename_without_extension(filename);
		debug_path += "_";
		debug_path += shader.entrypoint;
		debug_path += type_string;
		debug_path += ".pdb\"";

		symbol_path = std::wstring(debug_path.begin(), debug_path.end());

		arguments.push_back(L"-Fd"); arguments.push_back(symbol_path.c_str());
	} 
	else
	{
		// Full optimization.
		arguments.push_back(L"-O3");

		// Strip all debug info from the shader.
		arguments.push_back(L"-Qstrip_debug");
	}

	// Add in the MonoGame root signature.
	arguments.push_back(L"/force-rootsig-ver rootsig_1_0");
	arguments.push_back(L"/rootsig-define _MG_ROOT_SIGNATURE");
	arguments.push_back(L"/D _MG_ROOT_SIGNATURE=\"RootFlags(ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT | " \
		L"DENY_DOMAIN_SHADER_ROOT_ACCESS | " \
		L"DENY_GEOMETRY_SHADER_ROOT_ACCESS | " \
		L"DENY_HULL_SHADER_ROOT_ACCESS)," \
		L"CBV(b0, visibility = SHADER_VISIBILITY_VERTEX)," \
		L"CBV(b0, visibility = SHADER_VISIBILITY_PIXEL)," \
		L"DescriptorTable(SRV(t0, numDescriptors = unbounded), visibility = SHADER_VISIBILITY_VERTEX)," \
		L"DescriptorTable(SRV(t0, numDescriptors = unbounded), visibility = SHADER_VISIBILITY_PIXEL)," \
		L"DescriptorTable(Sampler(s0, numDescriptors = unbounded), visibility = SHADER_VISIBILITY_VERTEX)," \
		L"DescriptorTable(Sampler(s0, numDescriptors = unbounded), visibility = SHADER_VISIBILITY_PIXEL)\" "
	);

	// Compile the shader and gather relection info.
	DxcPtr<IDxcResult> result;
	hr = _compiler->Compile(
		&source_buffer,
		arguments.data(),
		static_cast<UINT32>(arguments.size()),
		nullptr,
		IID_PPV_ARGS(result.ppv())
	);

	if (FAILED(hr))
	{
		log.error("DXC Compile failed: ", hr);
		return false;
	}

	DxcPtr<IDxcBlobUtf8> errors;
	hr = result->GetOutput(DXC_OUT_ERRORS, IID_PPV_ARGS(errors.ppv()), nullptr);
	if (SUCCEEDED(hr) && errors)
	{
		if (errors->GetStringLength() > 0)
			log.info(errors->GetStringPointer());

	}

	// Be sure we didn't fail compiling.
	HRESULT status;
	hr = result->GetStatus(&status);
	if (FAILED(hr))
	{
		log.error("DXC GetStatus failed: ", hr);
		return false;
	}
	if (FAILED(status))
	{
		log.error("Failed to compile shader: ", status);
		return false;
	}

	// Gather the stripped shader binary blob.
	{
		DxcPtr<IDxcBlob> blob;
		hr = result->GetOutput(DXC_OUT_OBJECT, IID_PPV_ARGS(blob.ppv()), nullptr);
		if (FAILED(hr))
		{
			log.error("DXC GetOutput failed: ", hr);
			return false;
		}

		shader.binary.resize(blob->GetBufferSize());
		memcpy(shader.binary.data(), blob->GetBufferPointer(), shader.binary.size());
	}

	// Gather shader reflection information.
	int32_t textureMaxSlot = -1;
	int32_t samplerMaxSlot = -1;
	{
		DxcPtr<IDxcBlob> blob;
		DxcPtr<IDxcBlobUtf16> blob_path;
		hr = result->GetOutput(DXC_OUT_REFLECTION, IID_PPV_ARGS(blob.ppv()), blob_path.ppv());

		if (FAILED(hr))
		{
			log.error("DXC GetOutput failed: ", hr);
			return false;
		}

		DxcBuffer data;
		data.Ptr = blob->GetBufferPointer();
		data.Size = blob->GetBufferSize();
		data.Encoding = 0;

		DxcPtr<ID3D12ShaderReflection> reflection;
		hr = _utils->CreateReflection(&data, IID_PPV_ARGS(reflection.ppv()));
		if (FAILED(hr))
		{
			log.error("DXC CreateReflection failed: ", hr);
			return false;
		}

		D3D12_SHADER_DESC desc;
		hr = reflection->GetDesc(&desc);
		if (FAILED(hr))
		{
			log.error("DXC GetDesc failed: ", hr);
			return false;
		}

		// Process vertex shader input parameters into
		// input attribues for matching VBs to vertex shaders.
		if (shader.type == ShaderType::Vertex)
		{
			for (int i = 0; i < desc.InputParameters; i++)
			{
				D3D12_SIGNATURE_PARAMETER_DESC sdesc;
				hr = reflection->GetInputParameterDesc(i, &sdesc);
				if (FAILED(hr))
				{
					log.error("DXC GetInputParameterDesc failed: ", hr);
					continue;
				}


				InputAttribute input;
				input.index = sdesc.SemanticIndex;
				input.location = sdesc.Register;
				input.name = sdesc.SemanticName;

				// Get the vertex usage type from the name.
				if (iequals(input.name, "POSITION"))
					input.usage = VertexUsage::Position;
				else if (iequals(input.name, "NORMAL"))
					input.usage = VertexUsage::Normal;
				else if (iequals(input.name, "TANGENT"))
					input.usage = VertexUsage::Tangent;
				else if (iequals(input.name, "BINORMAL"))
					input.usage = VertexUsage::Binormal;
				else if (iequals(input.name, "TEXCOORD"))
					input.usage = VertexUsage::TextureCoordinate;
				else if (iequals(input.name, "COLOR"))
					input.usage = VertexUsage::Color;
				else if (iequals(input.name, "BLENDINDICES"))
					input.usage = VertexUsage::BlendIndices;
				else if (iequals(input.name, "BLENDWEIGHT"))
					input.usage = VertexUsage::BlendWeight;
				else if (iequals(input.name, "DEPTH"))
					input.usage = VertexUsage::Depth;
				else if (iequals(input.name, "FOG"))
					input.usage = VertexUsage::Fog;
				else if (iequals(input.name, "POINTSIZE"))
					input.usage = VertexUsage::PointSize;
				else if (iequals(input.name, "TESSELLATEFACTOR"))
					input.usage = VertexUsage::TessellateFactor;
				else
				{
					log.error("DXC Unknown SemanticName: ", sdesc.SemanticName);
					continue;
				}

				shader.attributes.push_back(input);
			}
		}

		for (int c=0; c < desc.ConstantBuffers; c++)
		{
			ID3D12ShaderReflectionConstantBuffer* cb = reflection->GetConstantBufferByIndex(c);

			D3D12_SHADER_BUFFER_DESC cdec;
			hr = cb->GetDesc(&cdec);
			if (FAILED(hr))
			{
				log.error("DXC ID3D12ShaderReflectionConstantBuffer::GetDesc failed: ", hr);
				continue;
			}

			shader.cbuffers.push_back(ConstantBuffer());
			ConstantBuffer& cbuff = *(shader.cbuffers.end() - 1);
			cbuff.name = cdec.Name;
			cbuff.size = cdec.Size;

			for (int v=0; v < cdec.Variables; v++)
			{
				ID3D12ShaderReflectionVariable* var = cb->GetVariableByIndex(v);
				ID3D12ShaderReflectionType* type = var->GetType();

				D3D12_SHADER_TYPE_DESC tdesc;
				hr = type->GetDesc(&tdesc);
				if (FAILED(hr))
				{
					log.error("DXC ID3D12ShaderReflectionType::GetDesc failed: ", hr);
					continue;
				}

				for (int m=0; m < tdesc.Members; m++)
				{
					ID3D12ShaderReflectionType* member = type->GetMemberTypeByIndex(m);

					LPCSTR mname = type->GetMemberTypeName(m);
					D3D12_SHADER_TYPE_DESC mdesc;
					hr = member->GetDesc(&mdesc);
					if (FAILED(hr))
					{
						log.error("DXC ID3D12ShaderReflectionType::GetDesc failed: ", hr);
						continue;
					}

					EffectParameter param;

					param.name = DeslangIdentifier(mname);
					param.rows = mdesc.Rows;
					param.columns = mdesc.Columns;
					param.offset = mdesc.Offset;

					switch (mdesc.Class)
					{
					case D3D_SVC_SCALAR:
						param.class_ = EffectParameterClass::Scalar;
						break;
					case D3D_SVC_VECTOR:
						param.class_ = EffectParameterClass::Vector;
						break;
					case D3D_SVC_MATRIX_ROWS:
					case D3D_SVC_MATRIX_COLUMNS:
						param.class_ = EffectParameterClass::Matrix;
						break;
					case D3D_SVC_OBJECT:
						param.class_ = EffectParameterClass::Object;
						break;
					case D3D_SVC_STRUCT:
						param.class_ = EffectParameterClass::Struct;
						break;
					case D3D_SVC_INTERFACE_CLASS:
						log.error("DXC Unsupported class D3D_SVC_INTERFACE_CLASS");
						continue;
					case D3D_SVC_INTERFACE_POINTER:
						log.error("DXC Unsupported class D3D_SVC_INTERFACE_POINTER");
						continue;
					};

					switch (mdesc.Type)
					{
					case D3D_SVT_VOID:
						param.type = EffectParameterType::Void;
						break;
					case D3D_SVT_BOOL:
						param.type = EffectParameterType::Bool;
						break;
					case D3D_SVT_INT:
						param.type = EffectParameterType::Int32;
						break;
					case D3D_SVT_FLOAT:
						param.type = EffectParameterType::Single;
						break;
					case D3D_SVT_STRING:
						param.type = EffectParameterType::String;
						break;
					case D3D_SVT_TEXTURE:
						param.type = EffectParameterType::Texture;
						break;
					case D3D_SVT_TEXTURE1D:
						param.type = EffectParameterType::Texture1D;
						break;
					case D3D_SVT_TEXTURE2D:
						param.type = EffectParameterType::Texture2D;
						break;
					case D3D_SVT_TEXTURE3D:
						param.type = EffectParameterType::Texture3D;
						break;
					case D3D_SVT_TEXTURECUBE:
						param.type = EffectParameterType::TextureCube;
						break;
					default:
						log.error("DXC Unsupported type: ", (uint32_t)mdesc.Type);
						break;
					};

					// Store the default value here.
					param.data.resize(param.rows * param.columns * 4, 0);

					// This is for runtime use in assigning effect parameter
					// values into the constant buffer.
					cbuff.params.push_back(ConstantBuffer::Param
					{
						(uint32_t)cbuff.parameters.size(),
						(uint16_t)param.offset,
					});

					// TODO: This is a mess... need to think out exactly how
					// EffectParameters are gatheed and shared between constant buffers.
					cbuff.parameters.push_back(param);
					shader.parameters.push_back(param);
				}
			}
		}

		// Gather the samplers and textures.
		std::map<UINT, D3D12_SHADER_INPUT_BIND_DESC> samplers;
		std::map<UINT, D3D12_SHADER_INPUT_BIND_DESC> textures;
		for (int r = 0; r < desc.BoundResources; r++)
		{
			D3D12_SHADER_INPUT_BIND_DESC rdesc;
			hr = reflection->GetResourceBindingDesc(r, &rdesc);
			if (FAILED(hr))
			{
				log.error("DXC GetResourceBindingDesc failed: ", hr);
				continue;
			}

			if (rdesc.Type == D3D_SIT_SAMPLER)
				samplers.emplace(rdesc.BindPoint, rdesc);
			else if (rdesc.Type == D3D_SIT_TEXTURE)
				textures.emplace(rdesc.BindPoint, rdesc);
        }

		for (auto& tex : textures)
		{
			SamplerInfo info;
			info.name = DeslangIdentifier(tex.second.Name);
			info.texture_slot = tex.second.BindPoint;
			info.sampler_slot = samplers[info.texture_slot].BindPoint;
			info.param_index = shader.parameters.size();

			EffectParameter param;
			param.name = info.name;
			param.class_ = EffectParameterClass::Object;
			param.rows = param.columns = param.offset = 0;

			switch (tex.second.Dimension)
			{
			case D3D_SRV_DIMENSION_TEXTURE1D:
				info.type = SamplerType::Sampler1D;
				param.type = EffectParameterType::Texture1D;
				break;
			case D3D_SRV_DIMENSION_TEXTURE2D:
				info.type = SamplerType::Sampler2D;
				param.type = EffectParameterType::Texture2D;
				break;
			case D3D_SRV_DIMENSION_TEXTURE3D:
			case D3D_SRV_DIMENSION_TEXTURE2DARRAY:
				info.type = SamplerType::SamplerVolume;
				param.type = EffectParameterType::Texture3D;
				break;
			case D3D_SRV_DIMENSION_TEXTURECUBE:
				info.type = SamplerType::SamplerCube;
				param.type = EffectParameterType::TextureCube;
				break;
			default:
				log.error("DXC unsupported texture type: ", tex.second.Dimension);
				continue;
			}
			
			// Track the max sampler/texture slots.
			if (info.texture_slot > textureMaxSlot)
				textureMaxSlot = info.texture_slot;
			if (info.sampler_slot > samplerMaxSlot)
				samplerMaxSlot = info.sampler_slot;

			shader.samplers.push_back(info);

			// Add the parameter to the shader.
			shader.parameters.push_back(param);
		}
    }

	// Generate the layout bindings from our cbuffers, samplers, and textures.
	{
		memstream mem;
		mem.write((uint32_t)0xB00B00);

		// Write the max texture and sampler slots.
		mem.write(samplerMaxSlot);
		mem.write(textureMaxSlot);

		// Finally write the shader bytecode.
		mem.write(shader.binary.data(), shader.binary.size());

		shader.binary = mem.buffer;
	}
	
	return true;
}
