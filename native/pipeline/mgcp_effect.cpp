// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include <stdlib.h>
#include <string.h>
#include <float.h>

#include <vector>
#include <string>
#include <queue>

#include <assert.h>

#include <slang.h>
#include <slang-com-ptr.h>
#include <slang-com-helper.h>

#include "shader_types.h"
#include "memstream.h"
#include "shader_profiles.h"
#include "pipeline_logger.h"
#include "std_helpers.h"

#include "api_MGCP.h"

#include <simplecpp.h>
#include <simplecpp.cpp>


// TODO: How do we share this code between the runtime and pipeline?
mguint MG_ComputeHash(const mgbyte* value, mgint length);


class EffectCompilerState
{
private:

	bool DeSlangShader(
		Slang::ComPtr<slang::IGlobalSession> gsession,
		const std::string& filename,
		Shader& shader);

	bool TryParseProperty(simplecpp::Token*& tok, std::string& name, std::string& value);
	bool TryParseSampler(simplecpp::Token*& tok, SamplerInfo& sampler);

	bool TryParseTechnique(simplecpp::Token*& tok, Technique& technique);
	bool ParseEffectPass(simplecpp::Token*& tok, Pass& pass);
	bool ParseShaderState(simplecpp::Token*& tok, ShaderState& shader);

	size_t AddConstantBuffer(ConstantBuffer& cb);

	void WriteConstantBuffer(memstream& stream, ConstantBuffer& cb);
	void WriteAnnotations(memstream& writer);
	void WriteShader(memstream& stream, Shader& shader);
	void WriteParameters(memstream& writer, const std::vector<EffectParameter>& parameters);
	void WriteParameter(memstream& writer, const EffectParameter& param);

public:

	const char* Header = "MGFX"; 
	const uint8_t Version = 11;

	EffectCompilerState() {}
	~EffectCompilerState()
	{
	}

	PipelineLogger log;

	std::string debug_symbol_path;

	std::string effect_filename;
	ShaderProfile* profile = nullptr;
	std::vector<std::string> preprocessor_defines;

	std::vector<Technique> techniques;
	std::vector<SamplerInfo> samplers;
	std::vector<Shader> shaders;
	std::vector<ConstantBuffer> cbuffers;
	std::vector<EffectParameter> parameters;

	std::vector<uint8_t> bytecode;
	std::string source;

	bool Initialize(const MGCP_EffectOptions& options);
	bool PreProcessEffect(const std::string& filename);
	bool DeSlangEffect(const std::string& filename);
	bool Compile(const std::string& filename);
	bool GenerateBytecode();
};


static void ClearTokens(simplecpp::Token* start, simplecpp::Token* end)
{
	while (start != end)
	{
		start->setstr("");
		start = start->next;
	}
}

static bool ParseTextureFilter(const std::string& value, MGTextureFilter& result)
{
	if (iequals(value, "None"))
	{
		result = MGTextureFilter::None;
		return true;
	}
	if (iequals(value, "Linear"))
	{
		result = MGTextureFilter::Linear;
		return true;
	}
	if (iequals(value, "Point"))
	{
		result = MGTextureFilter::Point;
		return true;
	}
	if (iequals(value, "Anisotropic"))
	{
		result = MGTextureFilter::Anisotropic;
		return true;
	}

	return false;
}

bool EffectCompilerState::TryParseProperty(simplecpp::Token*& tok, std::string& name, std::string& value)
{
	if (!isidentifier(tok->str()))
		return false;

	name = tok->str();

	tok = tok->nextSkipComments();
	if (tok->op != '=')
	{
		log.error("Expected equals after property name.");
		return false;
	}

	tok = tok->nextSkipComments();
	value = tok->str();

	return true;
}

bool EffectCompilerState::TryParseSampler(simplecpp::Token*& tok, SamplerInfo& sampler)
{
	bool isSampler = iequals(tok->str(), "sampler");
	bool isSamplerState = iequals(tok->str(), "SamplerState");
	bool isSamplerComparisonState = iequals(tok->str(), "SamplerComparisonState");

	if (!isSampler && !isSamplerState && !isSamplerComparisonState)
		return false;

	// To be compatible with old MonoGame HLSL we need to replace
	// the `sampler` identifier with `SamplerState` to be valid Slang.
	if (isSampler)
	{
		tok->setstr("SamplerState");

		// TODO: Should we bump the character offset
		// on other tokens after this one on the same line?
	}

	sampler = SamplerInfo();

	tok = tok->nextSkipComments();
	if (!tok || !isidentifier(tok->str()))
	{
		log.error("Expected sampler name.");
		return false;
	}

	sampler.name = tok->str();

	// Loop for the starting bracket.
	while (true)
	{
		tok = tok->nextSkipComments();
		if (!tok)
		{
			log.error("Unexpected end of file found parsing sampler state.");
			return false;
		}

		// TODO: Should we parse the register?

		if (tok->op == ';')
		{
			// Reached the end of the statement without
			// a starting sampler bracket.
			return false;
		}

		if (tok->op == '{')
			break;
	}

	simplecpp::Token* start = tok;

	// We have a sampler state... so read properties until
	// we get a closing bracket.
	MGTextureFilter minFilter = MGTextureFilter::Linear;
	MGTextureFilter magFilter = MGTextureFilter::Linear;
	MGTextureFilter mipFilter = MGTextureFilter::Linear;
	while (true)
	{
		tok = tok->nextSkipComments();
		if (!tok)
		{
			log.error("Unexpected end of file found parsing sampler state.");
			return false;
		}

		std::string property, value;
		if (TryParseProperty(tok, property, value))
		{
			if (iequals(property, "MipFilter"))
			{
				sampler.state = sampler.state ? sampler.state : new SamplerState();
				if (!ParseTextureFilter(value, mipFilter))
					log.error("Unexpected texture filter mode: ", value);
			}
			else if (iequals(property, "MagFilter"))
			{
				sampler.state = sampler.state ? sampler.state : new SamplerState();
				if (!ParseTextureFilter(value, magFilter))
					log.error("Unexpected texture filter mode: ", value);
			}
			else if (iequals(property, "MinFilter"))
			{
				sampler.state = sampler.state ? sampler.state : new SamplerState();
				if (!ParseTextureFilter(value, minFilter))
					log.error("Unexpected texture filter mode: ", value);
			}
			else if (iequals(property, "AddressU"))
			{
				sampler.state = sampler.state ? sampler.state : new SamplerState();
				//if (!ParseTextureFilter(value, minFilter))
					//log.error("Unexpected texture filter mode: ", value);
			}
			else if (iequals(property, "AddressV"))
			{
				sampler.state = sampler.state ? sampler.state : new SamplerState();
				//if (!ParseTextureFilter(value, minFilter))
					//log.error("Unexpected texture filter mode: ", value);
			}
			else if (iequals(property, "AddressW"))
			{
				sampler.state = sampler.state ? sampler.state : new SamplerState();
				//if (!ParseTextureFilter(value, minFilter))
					//log.error("Unexpected texture filter mode: ", value);
			}

			continue;
		}

		if (tok->op == '}')
			break;
	}

	if (sampler.state)
	{

		// Set up the sampler state.
		if (minFilter == MGTextureFilter::Anisotropic)
			sampler.state->Filter = MGTextureFilter::Anisotropic;
		else if (minFilter == MGTextureFilter::Linear && magFilter == MGTextureFilter::Linear && mipFilter == MGTextureFilter::Linear)
			sampler.state->Filter = MGTextureFilter::Linear;
		else if (minFilter == MGTextureFilter::Linear && magFilter == MGTextureFilter::Linear && mipFilter <= MGTextureFilter::Point)
			sampler.state->Filter = MGTextureFilter::LinearMipPoint;
		else if (minFilter == MGTextureFilter::Linear && magFilter <= MGTextureFilter::Point && mipFilter == MGTextureFilter::Linear)
			sampler.state->Filter = MGTextureFilter::MinLinearMagPointMipLinear;
		else if (minFilter == MGTextureFilter::Linear && magFilter <= MGTextureFilter::Point && mipFilter <= MGTextureFilter::Point)
			sampler.state->Filter = MGTextureFilter::MinLinearMagPointMipPoint;
		else if (minFilter <= MGTextureFilter::Point && magFilter == MGTextureFilter::Linear && mipFilter == MGTextureFilter::Linear)
			sampler.state->Filter = MGTextureFilter::MinPointMagLinearMipLinear;
		else if (minFilter <= MGTextureFilter::Point && magFilter == MGTextureFilter::Linear && mipFilter <= MGTextureFilter::Point)
			sampler.state->Filter = MGTextureFilter::MinPointMagLinearMipPoint;
		else if (minFilter <= MGTextureFilter::Point && magFilter <= MGTextureFilter::Point && mipFilter <= MGTextureFilter::Point)
			sampler.state->Filter = MGTextureFilter::Point;
		else if (minFilter <= MGTextureFilter::Point && magFilter <= MGTextureFilter::Point && mipFilter == MGTextureFilter::Linear)
			sampler.state->Filter = MGTextureFilter::PointMipLinear;

		// Do we need to disable mipmapping?
		if (mipFilter == MGTextureFilter::None)
		{
			// TODO: This is the only option we have right now for
			// disabling mipmapping.  We should add support for MinLod
			// and MaxLod which potentially does a better job at this.
			sampler.state->MipMapLevelOfDetailBias = -16.0f;
			sampler.state->MaxMipLevel = 0;
		}
	}
	simplecpp::Token* end = tok->next;

	// The sampler must end with a semicolon.
	tok = tok->nextSkipComments();
	if (!tok || tok->op != ';')
	{
		log.error("Expected ';' to end sampler state.");
		return false;
	}

	// Clear out the sampler state so Slang doesn't see it.
	ClearTokens(start, end);

	return true;
}

bool EffectCompilerState::ParseShaderState(simplecpp::Token*& tok, ShaderState& shader)
{
	tok = tok->nextSkipComments();
	if (!tok || tok->op != '=')
	{
		log.error("Expected assignment operator.");
		return false;
	}

	tok = tok->nextSkipComments();
	if (!tok || !iequals(tok->str(), "compile"))
	{
		log.error("Expected 'compile' identifier.");
		return false;
	}

	tok = tok->nextSkipComments();
	if (!tok || !isidentifier(tok->str()))
	{
		log.error("Expected shader model identifier.");
		return false;
	}
	shader.model = tok->str();

	tok = tok->nextSkipComments();
	if (!tok || !isidentifier(tok->str()))
	{
		log.error("Expected shader entrypoint identifier.");
		return false;
	}
	shader.entrypoint = tok->str();

	tok = tok->nextSkipComments();
	if (!tok || tok->op != '(')
	{
		log.error("Expected '('.");
		return false;
	}

	tok = tok->nextSkipComments();
	if (!tok || tok->op != ')')
	{
		log.error("Expected closing ')'.");
		return false;
	}

	tok = tok->nextSkipComments();
	if (!tok || tok->op != ';')
	{
		log.error("Expected ';'.");
		return false;
	}

	return true;
}

bool EffectCompilerState::ParseEffectPass(simplecpp::Token*& tok, Pass& pass)
{
	tok = tok->nextSkipComments();

	// Look for the optional pass name.
	if (tok && isidentifier(tok->str()))
	{
		pass.name = tok->str();
		tok = tok->nextSkipComments();
	}

	if (!tok || tok->op != '{')
	{
		log.error("Expected opening '{'.");
		return false;
	}

	while (true)
	{
		tok = tok->nextSkipComments();
		if (!tok)
		{
			log.error("Unexpected end of file found.");
			return false;
		}

		if (iequals(tok->str(), "VertexShader"))
		{
			ShaderState shader;
			shader.type = ShaderType::Vertex;
			if (ParseShaderState(tok, shader))
			{
				if (pass.vertex.isValid())
				{
					log.error("Cannot define more than one vertex shader in a pass.");
				}
				else
				{
					pass.vertex = shader;

					pass.vertex_index = find_index(shaders, [&](const Shader& s)
					{
						return s.entrypoint == shader.entrypoint;
					});
					
					if (pass.vertex_index == -1)
					{
						pass.vertex_index = shaders.size();
						shaders.push_back(Shader(shader));
					}
				}
			}
		}
		else if (iequals(tok->str(), "PixelShader"))
		{
			ShaderState shader;
			shader.type = ShaderType::Pixel;
			if (ParseShaderState(tok, shader))
			{
				if (pass.pixel.isValid())
				{
					log.error("Cannot define more than one pixel shader in a pass.");
				}
				else
				{
					pass.pixel = shader;

					pass.pixel_index = find_index(shaders, [&](const Shader& s)
					{
						return s.entrypoint == shader.entrypoint;
					});

					if (pass.pixel_index == -1)
					{
						pass.pixel_index = shaders.size();
						shaders.push_back(Shader(shader));
					}
				}
			}
		}
		else if (tok->op == '}')
		{
			simplecpp::Token* semicolon = tok->nextSkipComments();
			if (semicolon && semicolon->op == ';')
				tok = semicolon;
			break;
		}
		else
		{
			log.error("Found unexpected token!");
		}
	}

	return true;
}

bool EffectCompilerState::TryParseTechnique(simplecpp::Token*& tok, Technique& technique)
{
	if (!iequals(tok->str(), "technique"))
		return false;

	technique = Technique();

	simplecpp::Token* start = tok;

	tok = tok->nextSkipComments();
	if (!tok || !isidentifier(tok->str()))
	{
		log.error("Expected technique name.");
		return false;
	}

	technique.name = tok->str();

	tok = tok->nextSkipComments();
	if (!tok || tok->op != '{')
	{
		log.error("Expected opening '{'.");
		return false;				
	}

	while (true)
	{
		tok = tok->nextSkipComments();
		if (!tok)
		{
			log.error("Unexpected end of file found.");
			return false;
		}

		if (iequals(tok->str(), "pass"))
		{
			Pass pass;
			if (ParseEffectPass(tok, pass))
			{
				if (pass.hasShader())
					technique.passes.push_back(pass);
				else
				{
					log.error("A pass must have at least one shader.");
				}
			}
		}
		else if (tok->op == '}')
		{
			simplecpp::Token* semicolon = tok->nextSkipComments();
			if (semicolon && semicolon->op == ';')
				tok = semicolon;
			break;
		}
		else
		{
			return false;
		}
	}

	// Clear out the technique so Slang doesn't see it.
	ClearTokens(start, tok->next);

	return true;
}

bool EffectCompilerState::PreProcessEffect(
	const std::string& filename)
{
	simplecpp::DUI dui;
	dui.removeComments = false;
	dui.defines.assign(preprocessor_defines.begin(), preprocessor_defines.end());

	std::ifstream fstream(filename);
	if (!fstream.is_open())
	{
		log.error("The file '", filename, "' could not be opened.");
		return false;
	}

	std::vector<std::string> files;
	simplecpp::TokenList outputTokens(files);

	simplecpp::OutputList outputList;
	simplecpp::TokenList rawTokens(fstream, files, filename, dui, &outputList);

	simplecpp::FileDataCache cache;
	simplecpp::preprocess(outputTokens, rawTokens, files, cache, dui);

	// Now parse the samplers and techniques.
	for (simplecpp::Token* tok = outputTokens.front(); tok; tok = tok->next)
	{
		SamplerInfo sampler;
		if (TryParseSampler(tok, sampler))
		{
			samplers.push_back(sampler);
			continue;
		}

		Technique technique;
		if (TryParseTechnique(tok, technique))
		{
			techniques.push_back(technique);
			continue;
		}
	}

	// Gather the final source code.
	source = outputTokens.stringify();

	return true;
}

bool EffectCompilerState::DeSlangShader(
	Slang::ComPtr<slang::IGlobalSession> gsession,
	const std::string& filename,	
	Shader& shader)
{
	Slang::ComPtr<slang::ICompileRequest> req;
	gsession->createCompileRequest(req.writeRef());

	int ti = req->addCodeGenTarget((SlangCompileTarget)profile->SlangCompileTarget());

	SlangProfileID profileID = gsession->findProfile(profile->SlangTargetProfile());
	req->setTargetProfile(ti, profileID);

	int tu = req->addTranslationUnit(SLANG_SOURCE_LANGUAGE_SLANG, nullptr);
	req->addTranslationUnitSourceString(tu, filename.c_str(), source.c_str());

	SlangStage stage;
	switch (shader.type)
	{
	case ShaderType::Pixel:
		stage = SlangStage::SLANG_STAGE_PIXEL;
		break;
	default:
	case ShaderType::Vertex:
		stage = SlangStage::SLANG_STAGE_VERTEX;
		break;
	}

	// MonoGame CPU side matrix data is written in column major order.
	req->setMatrixLayoutMode(SLANG_MATRIX_LAYOUT_COLUMN_MAJOR);

	int ei = req->addEntryPoint(tu, shader.entrypoint.c_str(), stage);

	SlangResult result;
	result = req->compile();
	if (SLANG_FAILED(result))
	{
		log.error("Failed to slang compile stage: ", shader.entrypoint);

		const char* errors = req->getDiagnosticOutput();
		log.error(errors);

		return false;
	}

	slang::IBlob* blob = nullptr;
	req->getEntryPointCodeBlob(ei, ti, &blob);
	shader.source = (const char*)blob->getBufferPointer();

	// Some shader compilers cannot gather default
	// values for parameters... so get them here.
	{
		Slang::ComPtr<slang::IComponentType> program;
		req->getProgramWithEntryPoints(program.writeRef());
		slang::ProgramLayout* layout = program->getLayout(0);

		uint32_t pcount = layout->getParameterCount();
		for (int i = 0; i < pcount; ++i)
		{
			slang::VariableLayoutReflection* variable = layout->getParameterByIndex(i);
			slang::TypeLayoutReflection* typeLayout = variable->getTypeLayout();

			slang::TypeLayoutReflection* container_layout = typeLayout->getElementTypeLayout();
			if (!container_layout)
				continue;

			uint32_t fcount = container_layout->getFieldCount();
			for (int f = 0; f < fcount; ++f)
			{
				slang::VariableLayoutReflection* field = container_layout->getFieldByIndex(f);

				slang::VariableReflection* reflect = field->getVariable();
						
				const char* varName = field->getName();
			
				Slang::ComPtr<ISlangBlob> dblob;
				reflect->getDefaultValueBlob(dblob.writeRef());

				// 3. Query the default value blob
				if (dblob)
				{
					size_t blobSize = dblob->getBufferSize();
					uint8_t* defaultData = (uint8_t*)dblob->getBufferPointer();

					if (defaultData != nullptr && blobSize > 0)
					{
						// Success! You have the raw bytes of the default value.
						// You can copy these bytes directly into your host uniform buffer backup memory.
						printf("Variable '%s' has a default value of size %zu bytes\n", varName, blobSize);
					}
				}
			}
		}
	}

	return true;
}

bool EffectCompilerState::DeSlangEffect(const std::string& filename)
{	
	Slang::ComPtr<slang::IGlobalSession> gsession;
	createGlobalSession(gsession.writeRef());

	int errors = 0;

	for (auto& s : shaders)
	{
		if (!DeSlangShader(gsession, filename, s))
			errors++;
	}

	return errors == 0;
}

size_t EffectCompilerState::AddConstantBuffer(ConstantBuffer& cb)
{
	// Check if we already have this one.
	for (int i=0; i < cbuffers.size(); i++)
	{		
		if (cb.SameAs(cbuffers[i]))
		{
			return i;
		}
	}

	// If we got here we need to add it in.
	cbuffers.push_back(cb);
	return cbuffers.size() - 1;
}

bool EffectCompilerState::Compile(const std::string& filename)
{
	int errors = 0;

	// Process each unique shader.
	for (Shader& s : shaders)
	{
		// Are we building for debug?
		if (debug_symbol_path.size() > 0)
		{
			s.debug = true;
			s.debug_symbol_path = debug_symbol_path;
		}

		if (!profile->Compile(filename, s, log))
			errors++;
	}

	// Merge the stage states.
	for (Shader& s : shaders)
	{
		// Merge all the constant buffers.
		for (ConstantBuffer& cb : s.cbuffers)
		{
			size_t index = AddConstantBuffer(cb);
			s.cbuffer_index.push_back(index);

			for (EffectParameter& p : cb.parameters)
			{
				// This is very hackey!
				bool add = true;
				for (EffectParameter& o : parameters)
				{
					if (o.name == p.name)
					{
						add = false;
						break;
					}
				}

				if (add)
				{
					//s.parameters.push_back(p);
					parameters.push_back(p);
				}
			}
		}

		for (EffectParameter& p : s.parameters)
		{
			// This is very hackey!
			bool add = true;
			for (EffectParameter& o : parameters)
			{
				if (o.name == p.name)
				{
					add = false;
					break;
				}
			}

			if (add)
			{
				parameters.push_back(p);
			}
		}
	}

	return errors == 0;
}

void EffectCompilerState::WriteParameter(memstream& writer, const EffectParameter& param)
{
	writer.write((uint8_t)param.class_);
	writer.write((uint8_t)param.type);

	writer.write(param.name);
	writer.write(std::string()); // Currently unused!

	WriteAnnotations(writer /*, param.annotation_handles*/);

	writer.write(param.rows);
	writer.write(param.columns);

	// Write the elements or struct members.
	WriteParameters(writer, param.elements);
	WriteParameters(writer, param.members);

	// Write the data if this isn't an array or structure.
	if (param.elements.size() == 0 && param.members.size() == 0)
	{
		switch (param.type)
		{
		case EffectParameterType::Bool:
		case EffectParameterType::Int32:
		case EffectParameterType::Single:
			writer.write(param.data.data(), param.data.size());
			break;
		}
	}
}

void EffectCompilerState::WriteParameters(memstream& writer, const std::vector<EffectParameter>& parameters)
{
	writer.write((uint32_t)parameters.size());
	for (const EffectParameter& p : parameters)
		WriteParameter(writer, p);
}

void EffectCompilerState::WriteAnnotations(memstream& writer)
{
	// TODO: Support this fully and correctly!
	uint32_t count = 0;
	writer.write(count);

	/*
	var count = annotations == null ? 0 : annotations.Length;
	writer.Write(count);
	for (var i = 0; i < count; i++)
		WriteParameter(writer, annotations![i]);
	*/
}

void EffectCompilerState::WriteConstantBuffer(memstream& stream, ConstantBuffer& cb)
{
	stream.write(cb.name);
	stream.write((uint16_t)cb.size);

	stream.write((uint32_t)cb.params.size());
	for (auto& p : cb.params)
	{
		stream.write(p.index);
		stream.write(p.offset);
	}
}

void EffectCompilerState::WriteShader(memstream& stream, Shader& shader)
{
	// TODO: Has to break the format when we add stages.
	stream.write(shader.type == ShaderType::Vertex);

	stream.write(effect_filename);
	stream.write(shader.entrypoint);

	stream.write((uint32_t)shader.binary.size());
	stream.write(shader.binary.data(), shader.binary.size());

	stream.write((uint8_t)shader.samplers.size());
	for (SamplerInfo& sampler : shader.samplers)
	{
		stream.write((uint8_t)sampler.type);
		stream.write(sampler.texture_slot);
		stream.write(sampler.sampler_slot);

		if (sampler.state != nullptr)
		{
			stream.write(true);
			stream.write(sampler.state->AddressU);
			stream.write(sampler.state->AddressV);
			stream.write(sampler.state->AddressW);
			stream.write(sampler.state->BorderColorR);
			stream.write(sampler.state->BorderColorG);
			stream.write(sampler.state->BorderColorB);
			stream.write(sampler.state->BorderColorA);
			stream.write((uint8_t)sampler.state->Filter);
			stream.write(sampler.state->MaxAnisotropy);
			stream.write(sampler.state->MaxMipLevel);
			stream.write(sampler.state->MipMapLevelOfDetailBias);
		}
		else
			stream.write(false);

		stream.write(sampler.name);

		stream.write(sampler.param_index);
	}

	// The constant buffer index.
	stream.write((uint8_t)shader.cbuffer_index.size());
	for (uint8_t index : shader.cbuffer_index)
		stream.write(index);

	// Write the input attributes for the vertex shader or 0.
	stream.write((uint8_t)shader.attributes.size());
	for (InputAttribute& attrib : shader.attributes)
	{
		stream.write(attrib.name);
		stream.write((uint8_t)attrib.usage);
		stream.write(attrib.index);
		stream.write(attrib.location);
	}
}

bool EffectCompilerState::GenerateBytecode()
{
	memstream writer;

	// Write a very simple header for identification and versioning.
	writer.write(Header, strlen(Header));
	writer.write(Version);

	// Write an simple identifier for DX11 vs GLSL
	// so we can easily detect the correct shader type.
	writer.write(profile->GetProfileId());

	// Hold space for the effect key which we fill in later.
	size_t effectKeyOffset = writer.offset();
	uint32_t effectKey = 0;
	writer.write(effectKey);

	// Write all the constant buffers.
	writer.write((uint32_t)cbuffers.size());
	for (ConstantBuffer& cb : cbuffers)
		WriteConstantBuffer(writer, cb);

	// Write all the shaders.
	writer.write((uint32_t)shaders.size());
	for (Shader& s : shaders)
		WriteShader(writer, s);

	// Write the parameters.
	WriteParameters(writer, parameters);

	// Write the techniques.
	writer.write((uint32_t)techniques.size());
	for (auto& technique : techniques)
	{
		writer.write(technique.name);
		WriteAnnotations(writer /*, technique.annotation_handles*/);

		// Write the passes.
		writer.write((uint32_t)technique.passes.size());
		for (Pass& pass : technique.passes)
		{
			writer.write(pass.name);
			WriteAnnotations(writer /*, pass.annotation_handles*/);

			// Write the index to the vertex and pixel shaders.
			writer.write(pass.vertex_index);
			writer.write(pass.pixel_index);

			// Write the state objects too!
			/*
			if (pass.blendState != null)
			{
				writer.write(true);
				writer.write((byte)pass.blendState.AlphaBlendFunction);
				writer.write((byte)pass.blendState.AlphaDestinationBlend);
				writer.write((byte)pass.blendState.AlphaSourceBlend);
				writer.write(pass.blendState.BlendFactor.R);
				writer.write(pass.blendState.BlendFactor.G);
				writer.write(pass.blendState.BlendFactor.B);
				writer.write(pass.blendState.BlendFactor.A);
				writer.write((byte)pass.blendState.ColorBlendFunction);
				writer.write((byte)pass.blendState.ColorDestinationBlend);
				writer.write((byte)pass.blendState.ColorSourceBlend);
				writer.write((byte)pass.blendState.ColorWriteChannels);
				writer.write((byte)pass.blendState.ColorWriteChannels1);
				writer.write((byte)pass.blendState.ColorWriteChannels2);
				writer.write((byte)pass.blendState.ColorWriteChannels3);
				writer.write(pass.blendState.MultiSampleMask);
			}
			else
			*/
				writer.write(false);

			/*
			if (pass.depthStencilState != null)
			{
				writer.write(true);
				writer.write((byte)pass.depthStencilState.CounterClockwiseStencilDepthBufferFail);
				writer.write((byte)pass.depthStencilState.CounterClockwiseStencilFail);
				writer.write((byte)pass.depthStencilState.CounterClockwiseStencilFunction);
				writer.write((byte)pass.depthStencilState.CounterClockwiseStencilPass);
				writer.write(pass.depthStencilState.DepthBufferEnable);
				writer.write((byte)pass.depthStencilState.DepthBufferFunction);
				writer.write(pass.depthStencilState.DepthBufferWriteEnable);
				writer.write(pass.depthStencilState.ReferenceStencil);
				writer.write((byte)pass.depthStencilState.StencilDepthBufferFail);
				writer.write(pass.depthStencilState.StencilEnable);
				writer.write((byte)pass.depthStencilState.StencilFail);
				writer.write((byte)pass.depthStencilState.StencilFunction);
				writer.write(pass.depthStencilState.StencilMask);
				writer.write((byte)pass.depthStencilState.StencilPass);
				writer.write(pass.depthStencilState.StencilWriteMask);
				writer.write(pass.depthStencilState.TwoSidedStencilMode);
			}
			else
			*/
				writer.write(false);

			/*
			if (pass.rasterizerState != null)
			{
				writer.write(true);
				writer.write((byte)pass.rasterizerState.CullMode);
				writer.write(pass.rasterizerState.DepthBias);
				writer.write((byte)pass.rasterizerState.FillMode);
				writer.write(pass.rasterizerState.MultiSampleAntiAlias);
				writer.write(pass.rasterizerState.ScissorTestEnable);
				writer.write(pass.rasterizerState.SlopeScaleDepthBias);
			}
			else
			*/
				writer.write(false);
		}
	}

	// Write a tail to be used by the reader for validation.
	writer.write(Header, sizeof(Header));

	// Generate a hash from the stream and insert it into
	effectKey = MG_ComputeHash(writer.ptr(), writer.size());
	writer.write_at(effectKeyOffset, effectKey);

	// We're done... set the final bytecode.
	bytecode = writer.buffer;
	return true;
}

bool EffectCompilerState::Initialize(const MGCP_EffectOptions& options)
{
	const char* profile_name = (const char*)options.Profile;

	ShaderProfile* profile = ShaderProfileManager::Get(profile_name);
	if (!profile)
	{
		log.error("The shader profile '", profile_name, "' was not found.");
		return false;
	}

	this->profile = profile;

	// The profile forces some defines.
	const char* defines = profile->Defines();
	while (defines && defines[0])
	{
		std::string define = defines;
		preprocessor_defines.push_back(define);
		defines += define.length() + 1;
	}

	// Now add in the user defines.
	defines = (const char*)options.Defines;
	while (defines && defines[0])
	{
		std::string define = defines;
		preprocessor_defines.push_back(define);
		defines += define.length() + 1;
	}

	// For output of shader debugging info.
	if (options.Debug && options.DebugSymbolPath)
		debug_symbol_path = (const char*)options.DebugSymbolPath;

	return true;
}

int MP_EffectCompiler_Compile(MGCP_EffectOptions& options, MGCP_Effect& out_effect)
{
	out_effect.Size = 0;
	out_effect.Version = 0;
	out_effect.bytecode = nullptr;

	EffectCompilerState* effect = new EffectCompilerState();
	out_effect.handle = effect;

	PipelineLogger& log = effect->log;

	if (!effect->Initialize(options))
	{
		out_effect.errorsAndWarnings = (void*)log.str();
		return log.errors();
	}

	std::string filename = (const char*)options.EffectFilePath;
	std::replace(filename.begin(), filename.end(), '\\', '/');

	// First preprocess the effect source removing #includes,
	// all macros, and Effect syntax for sampler states and techniques.
	if (!effect->PreProcessEffect(filename))
	{
		log.error("Failed to preprocess the effect.");
		out_effect.errorsAndWarnings = (void*)log.str();
		return log.errors();
	}

	// We assume all incoming HLSL could have Slang syntax in it
	// so we pass it thru Slang to convert it to the shader profile
	// target language: HLSL, SpirV, GLSL, Metal, etc.
	if (!effect->DeSlangEffect(filename))
	{
		log.error("Failed to de-slang the effect.");
		out_effect.errorsAndWarnings = (void*)log.str();
		return log.errors();
	}

	// Now we let the profile compile the shaders.
	if (!effect->Compile(filename))
	{
		log.error("Failed to compile the effect.");
		out_effect.errorsAndWarnings = (void*)log.str();
		return log.errors();
	}

	// Finally build the final effect binary in the format that
	// matches what Effect.ReadHeader and Effect.ReadEffect expect.
	if (!effect->GenerateBytecode())
	{
		log.error("Failed to generate the effect binary.");
		out_effect.errorsAndWarnings = (void*)log.str();
		return log.errors();
	}

	// Return the final effect bytecode to the caller.
	out_effect.bytecode = effect->bytecode.data();
	out_effect.Size = effect->bytecode.size();
	out_effect.Version = effect->Version;

	// We may have warnings, so return those too.
	out_effect.errorsAndWarnings = (void*)log.str();
	return log.errors();
}

void MP_EffectCompiler_Free(MGCP_Effect& result)
{
	EffectCompilerState* effect = (EffectCompilerState*)result.handle;
	delete effect;
	result.handle = nullptr;
}


