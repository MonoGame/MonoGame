// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGG.h"

#include "mg_common.h"


#define VULKAN_HPP_DISPATCH_LOADER_DYNAMIC 1
#define VULKAN_HPP_NO_EXCEPTIONS
#define VULKAN_HPP_TYPESAFE_CONVERSION 1
#define VK_NO_PROTOTYPES
#include <vulkan/vulkan.h>

#define VOLK_IMPLEMENTATION
#include <volk.h>

#define VMA_IMPLEMENTATION
#define VMA_STATIC_VULKAN_FUNCTIONS 1
#include <vk_mem_alloc.h>

#if defined(MG_SDL2)
#include <SDL_vulkan.h>
#endif

#ifdef _WIN32
#include <Windows.h>
#include <vulkan/vulkan_win32.h>
#include "vulkan.resources.h"
#endif

#define VK_CHECK_RESULT(vkr)															\
{																						\
	VkResult _vkr = vkr;																\
	if (_vkr < VK_SUCCESS)																\
	{																					\
		fprintf(stderr, "Fatal : VkResult is %x in %s:%d", _vkr, __FILE__, __LINE__);	\
		assert(_vkr == VK_SUCCESS);														\
	}																					\
}

template<class T>
T MG_AlignUp(T value, const T alignment)
{
	return (value + alignment - 1) & ~(alignment - 1);
}

struct MGVK_Program;

typedef uint32_t FrameCounter;

const FrameCounter kFreeFrames = 2;
const FrameCounter kConcurrentFrameCount = 2;


struct MGVK_CmdBuffer
{
	VkSemaphore     imageAcquiredSemaphore;
	VkSemaphore     renderCompleteSemaphore;
	VkCommandBuffer buffer;
	VkFence         completedFence;
};

struct MGVK_TargetSet
{
	MGG_Texture* targets[4] = { 0 };
	int numTargets = 0;
};

struct MGVK_TargetSetCache
{
	MGVK_TargetSet set;

	int width = 0;
	int height = 0;
	VkFramebuffer framebuffer = VK_NULL_HANDLE;
	VkRenderPass renderPass = VK_NULL_HANDLE;
};

struct MGVK_PipelineState
{
	VkPrimitiveTopology topology = VK_PRIMITIVE_TOPOLOGY_POINT_LIST;

	MGVK_Program* program = nullptr;
	MGG_InputLayout* layout = nullptr;

	MGVK_TargetSetCache* targets = nullptr;

	MGG_BlendState* blendState = nullptr;
	MGG_RasterizerState* rasterizerState = nullptr;
	MGG_DepthStencilState* depthStencilState = nullptr;
};

struct MGVK_PipelineCache
{
	MGVK_PipelineState state;
	VkPipeline cache = VK_NULL_HANDLE;
};

struct MGVK_Program
{
	MGG_Shader* vertex;
	MGG_Shader* pixel;

	//std::vector<MGG_BindingInfo> bindings;

	VkPipelineLayout layout;
};

struct MGVK_FrameState
{
	bool is_recording = false;

	MGG_Texture* swapchainTexture = nullptr;
	MGVK_CmdBuffer commandBuffer;

	uint32_t uniformOffset = 0;
	MGG_Buffer* uniforms = nullptr;
};


struct MGG_GraphicsAdapter
{
	VkInstance instance = nullptr;
	VkPhysicalDevice device = nullptr;
	VkPhysicalDeviceProperties properties = { 0 };
	VkPhysicalDeviceFeatures features = { 0 };
	VkPhysicalDeviceMemoryProperties memory = { 0 };

	MGG_DisplayMode current = { MGSurfaceFormat::Color, 0, 0 };

	std::vector<MGG_DisplayMode> modes;
};

const int MAX_TEXTURE_SLOTS = 16;

struct MGG_GraphicsDevice
{
	VkInstance instance = VK_NULL_HANDLE;

	VkPhysicalDevice physicalDevice = VK_NULL_HANDLE;
	VkPhysicalDeviceProperties deviceProperties;
	VkPhysicalDeviceFeatures deviceFeatures;
	VkPhysicalDeviceMemoryProperties deviceMemoryProperties;

	VkDevice device = VK_NULL_HANDLE;
	VkQueue queue = VK_NULL_HANDLE;
	VkCommandPool cmdPool = VK_NULL_HANDLE;

	VmaAllocator allocator = VK_NULL_HANDLE;

	MGVK_FrameState* frames = nullptr;
	FrameCounter frame = 0;

	uint32_t swapchainWidth = 0;
	uint32_t swapchainHeight = 0;
	VkFormat colorFormat = VK_FORMAT_UNDEFINED;
	VkFormat depthFormat = VK_FORMAT_UNDEFINED;

	bool inRenderPass = false;
	bool renderTargetDirty = false;

	VkViewport viewport = { 0 };
	VkRect2D scissor = { 0 };
	bool scissorDirty = false;

#if defined(MG_SDL2)
	SDL_Window* window = nullptr;
#else
#error Not Implemented
#endif
	VkSurfaceKHR surface = VK_NULL_HANDLE;
	VkSwapchainKHR swapchain = VK_NULL_HANDLE;
	uint32_t swapchain_image_index = 0;

	uint64_t vertexBuffersDirty = 0;
	MGG_Buffer* vertexBuffers[8] = { 0 };
	uint32_t vertexOffsets[8] = { 0 };

	MGG_Buffer* indexBuffer = nullptr;
	MGIndexElementSize indexBufferSize = MGIndexElementSize::SixteenBits;

	uint64_t currentTextureId = 1;
	uint64_t currentSamplerId = 1;
	MGG_Texture* textures[MAX_TEXTURE_SLOTS] = { 0 };
	MGG_SamplerState* samplers[MAX_TEXTURE_SLOTS] = { 0 };
	uint32_t textureSamplerDirty = 0;

	bool blendFactorDirty = false;
	float blendFactor[4] = { 0 };

	uint32_t currentShaderId = 0;
	MGG_Shader* shaders[(mgint)MGShaderStage::Count] = { 0 };
	bool shaderDirty = false;
	std::map<uint64_t, MGVK_Program*> shader_programs;
	std::vector<MGG_Shader*> all_shaders;

	VkPipelineCache pipelineCache = VK_NULL_HANDLE;
	std::map<uint32_t, MGVK_PipelineCache> pipelines;

	std::map<uint32_t, MGG_BlendState*> blendStates;
	std::map<uint32_t, MGG_RasterizerState*> rasterizerStates;
	std::map<uint32_t, MGG_DepthStencilState*> depthStencilStates;

	MGVK_TargetSet targets;
	std::map<uint32_t, MGVK_TargetSetCache*> targetCache;


	//
	bool pipelineStateDirty = false;
	MGVK_PipelineState pipelineState;

	MGG_Buffer* uniforms[2] = { nullptr, nullptr };
	uint32_t uniformsDirty = 0;

	VkDescriptorSet descriptorSets[2] = { 0 };
	uint32_t dynamicOffsets[2] = { 0 };

	// Some needed limits.
	VkDeviceSize minUniformBufferOffsetAlignment = 0;

	std::queue<MGG_Buffer*> destroyBuffers;
	std::queue<MGG_Texture*> destroyTextures;
	std::queue<MGG_BlendState*> destroyBlendStates;
	std::queue<MGG_RasterizerState*> destroyRasterizerStates;
	std::queue<MGG_DepthStencilState*> destroyDepthStencilStates;

	MGG_Buffer* discarded = nullptr;
	MGG_Buffer* pending = nullptr;
	MGG_Buffer* free = nullptr;

	std::vector<MGG_Buffer*> all_buffers;
	std::vector<MGG_Texture*> all_textures;
};

struct MGG_Buffer
{
	FrameCounter frame = 0;

	MGG_Buffer* next = nullptr;

	MGBufferType type = MGBufferType::Vertex;

	int dataSize = 0;
	int actualSize = 0;
	bool dirty = false;

	uint8_t* push = nullptr;

	VkBuffer buffer = VK_NULL_HANDLE;
	VmaAllocation allocation = VK_NULL_HANDLE;

	uint8_t* mapped = nullptr;
};

struct MGG_Texture
{
	FrameCounter frame;

	MGTextureType type;
	MGSurfaceFormat format;

	MGRenderTargetUsage usage = MGRenderTargetUsage::PlatformContents;

	mgbool isTarget = false;
	mgbool isSwapchain = false;

	mgint multiSampleCount = 0;

	uint64_t id;
	VkImageCreateInfo info;
	VkImage image;

	VkImageLayout layout = VK_IMAGE_LAYOUT_GENERAL;
	VkImageLayout optimal_layout = VK_IMAGE_LAYOUT_GENERAL;

	VmaAllocation allocation;

	void* mappedAddr;

	VkImageView view = VK_NULL_HANDLE;
	VkImageView target_view = VK_NULL_HANDLE;

	MGDepthFormat depthFormat = MGDepthFormat::None;
	MGG_Texture* depthTexture;
};

struct MGG_InputLayout
{
	VkVertexInputAttributeDescription* attributes = nullptr;
	VkVertexInputBindingDescription* bindings = nullptr;
	mgint streamCount = 0;
	mgint attributeCount = 0;
};

struct MGVK_DescriptorInfo
{
	FrameCounter frame;
	VkDescriptorSet set;
};


struct MGG_Shader
{
	uint32_t id;

	MGShaderStage stage;

	VkShaderModule module;
	VkDescriptorSetLayout setLayout;

	std::vector<VkDescriptorSetLayoutBinding> bindings;

	VkWriteDescriptorSet* writes;

	mguint uniformSlots;
	mguint textureSlots;
	mguint samplerSlots;

	VkDescriptorPoolCreateInfo* poolInfo;
	VkDescriptorPool pool;

	std::queue<MGVK_DescriptorInfo*> freeSets;
	std::map<uint32_t, MGVK_DescriptorInfo*> usedSets;
};

struct MGVK_BindingInfo
{
	VkDescriptorType type;
	mguint slot;
	mguint binding;
	MGG_Shader* shader;
};

struct MGG_BlendState
{
	FrameCounter frame;
	mguint hash;
	mgint refs;
	VkPipelineColorBlendStateCreateInfo info;
	VkPipelineColorBlendAttachmentState attachments[4];
};

struct MGG_DepthStencilState
{
	FrameCounter frame;
	mguint hash;
	mgint refs;
	VkPipelineDepthStencilStateCreateInfo info;
};

struct MGG_RasterizerState
{
	FrameCounter frame;
	mguint hash;
	mgint refs;
	VkPipelineRasterizationStateCreateInfo info;
	mgbool scissorTestEnable;
	mgbool multiSampleAntiAlias;
};

struct MGG_SamplerState
{
	uint64_t id;
	VkSampler sampler;
	MGG_SamplerState_Info info;
};

struct MGG_OcclusionQuery
{
	// TODO!
};

struct MGG_GraphicsSystem
{
	VkInstance instance;

	std::vector<MGG_GraphicsAdapter*> adapters;
};


static void MGVK_BufferCopyAndFlush(MGG_GraphicsDevice* device, MGG_Buffer* buffer, int destOffset, mgbyte* data, int dataBytes);
static MGG_Buffer* MGVK_Buffer_Create(MGG_GraphicsDevice* device, MGBufferType type, mgint sizeInBytes, bool no_push);
static void MGVK_DestroyFrameResources(MGG_GraphicsDevice* device, mgint currentFrame, mgbool free_all);
static void MGVK_UpdateRenderPass(MGG_GraphicsDevice* device, FrameCounter currentFrame, MGVK_CmdBuffer& cmd);
static void MGVK_TransitionImageLayout(MGG_GraphicsDevice* device, MGG_Texture* texture, int32_t level, VkImageLayout newLayout);


static VkFormat ToVkFormat(MGSurfaceFormat format)
{
	switch (format)
	{
	case MGSurfaceFormat::Color:
		return VK_FORMAT_R8G8B8A8_UNORM;
	case MGSurfaceFormat::Bgr565:
		return VK_FORMAT_B5G6R5_UNORM_PACK16;
	case MGSurfaceFormat::Bgra5551:
		return VK_FORMAT_B5G5R5A1_UNORM_PACK16;
	case MGSurfaceFormat::Bgra4444:
		return VK_FORMAT_B4G4R4A4_UNORM_PACK16;
	case MGSurfaceFormat::Dxt1:
		return VK_FORMAT_BC1_RGB_UNORM_BLOCK;
	case MGSurfaceFormat::Dxt3:
		return VK_FORMAT_BC2_UNORM_BLOCK;
	case MGSurfaceFormat::Dxt5:
		return VK_FORMAT_BC3_UNORM_BLOCK;
	case MGSurfaceFormat::NormalizedByte2:
		return VK_FORMAT_R8G8_UNORM;
	case MGSurfaceFormat::NormalizedByte4:
		return VK_FORMAT_R8G8B8A8_UNORM;
	case MGSurfaceFormat::Rgba1010102:
		return VK_FORMAT_A2R10G10B10_UNORM_PACK32;
	case MGSurfaceFormat::Rg32:
		return VK_FORMAT_R16G16_UNORM;
	case MGSurfaceFormat::Rgba64:
		return VK_FORMAT_R16G16B16A16_UNORM;
	case MGSurfaceFormat::Alpha8:
		return VK_FORMAT_R8_UNORM;
	case MGSurfaceFormat::Single:
		return VK_FORMAT_R32_SFLOAT;
	case MGSurfaceFormat::Vector2:
		return VK_FORMAT_R32G32_SFLOAT;
	case MGSurfaceFormat::Vector4:
		return VK_FORMAT_R32G32B32A32_SFLOAT;
	case MGSurfaceFormat::HalfSingle:
		return VK_FORMAT_R32G32B32A32_SFLOAT;
	case MGSurfaceFormat::HalfVector2:
		return VK_FORMAT_R16G16_SFLOAT;
	case MGSurfaceFormat::HalfVector4:
		return VK_FORMAT_R16G16B16A16_SFLOAT;
	case MGSurfaceFormat::HdrBlendable:
		return VK_FORMAT_R16G16B16A16_SFLOAT;
	case MGSurfaceFormat::Bgr32:
		return VK_FORMAT_B8G8R8A8_UNORM;
	case MGSurfaceFormat::Bgra32:
		return VK_FORMAT_B8G8R8A8_UNORM;
	case MGSurfaceFormat::ColorSRgb:
		return VK_FORMAT_R8G8B8A8_SRGB;
	case MGSurfaceFormat::Bgr32SRgb:
		return VK_FORMAT_B8G8R8A8_SRGB;
	case MGSurfaceFormat::Bgra32SRgb:
		return VK_FORMAT_B8G8R8A8_SRGB;
	case MGSurfaceFormat::Dxt1SRgb:
		return VK_FORMAT_BC1_RGB_SRGB_BLOCK;
	case MGSurfaceFormat::Dxt3SRgb:
		return VK_FORMAT_BC2_SRGB_BLOCK;
	case MGSurfaceFormat::Dxt5SRgb:
		return VK_FORMAT_BC3_SRGB_BLOCK;
	case MGSurfaceFormat::Dxt1a:
		return VK_FORMAT_BC1_RGBA_UNORM_BLOCK;
	default:
		assert(0);
	}
}

static VkFormat ToVkFormat(MGDepthFormat format)
{
	switch (format)
	{
	case MGDepthFormat::Depth16:
		return VkFormat::VK_FORMAT_D16_UNORM;
	case MGDepthFormat::Depth24:
	case MGDepthFormat::Depth24Stencil8:
		return VkFormat::VK_FORMAT_D24_UNORM_S8_UINT;
	default:
		return VkFormat::VK_FORMAT_UNDEFINED;
	}
}

static VkFormat ToVkFormat(MGVertexElementFormat format)
{
	switch (format)
	{
	case MGVertexElementFormat::Single:
		return VK_FORMAT_R32_SFLOAT;
	case MGVertexElementFormat::Vector2:
		return VK_FORMAT_R32G32_SFLOAT;
	case MGVertexElementFormat::Vector3:
		return VK_FORMAT_R32G32B32_SFLOAT;
	case MGVertexElementFormat::Vector4:
		return VK_FORMAT_R32G32B32A32_SFLOAT;
	case MGVertexElementFormat::Color:
		return VK_FORMAT_R8G8B8A8_UNORM;
	case MGVertexElementFormat::Byte4:
		return VK_FORMAT_R8G8B8A8_UINT;
	case MGVertexElementFormat::Short2:
		return VK_FORMAT_R16G16_SINT;
	case MGVertexElementFormat::Short4:
		return VK_FORMAT_R16G16B16A16_SINT;
	case MGVertexElementFormat::NormalizedShort2:
		return VK_FORMAT_R16G16_SSCALED;
	case MGVertexElementFormat::NormalizedShort4:
		return VK_FORMAT_R16G16B16A16_SSCALED;
	case MGVertexElementFormat::HalfVector2:
		return VK_FORMAT_R16G16_SFLOAT;
	case MGVertexElementFormat::HalfVector4:
		return VK_FORMAT_R16G16B16A16_SFLOAT;
	default:
		assert(0);
	}
}

static VkPrimitiveTopology ToVkPrimitiveTopology(MGPrimitiveType type)
{
	switch (type)
	{
	case MGPrimitiveType::TriangleList:
		return VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
	case MGPrimitiveType::TriangleStrip:
		return VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP;
	case MGPrimitiveType::LineList:
		return VK_PRIMITIVE_TOPOLOGY_LINE_LIST;
	case MGPrimitiveType::LineStrip:
		return VK_PRIMITIVE_TOPOLOGY_LINE_STRIP;
	default:
		assert(0);
	}
}

static VkImageAspectFlags DetermineAspectMask(VkFormat format)
{
	VkImageAspectFlags result = (VkImageAspectFlags)0;
	switch (format) {
		// Depth
	case VK_FORMAT_D16_UNORM:
	case VK_FORMAT_X8_D24_UNORM_PACK32:
	case VK_FORMAT_D32_SFLOAT:
		result = VK_IMAGE_ASPECT_DEPTH_BIT;
		break;
		// Stencil
	case VK_FORMAT_S8_UINT:
		result = VK_IMAGE_ASPECT_STENCIL_BIT;
		break;
		// Depth/Stencil
	case VK_FORMAT_D16_UNORM_S8_UINT:
	case VK_FORMAT_D24_UNORM_S8_UINT:
	case VK_FORMAT_D32_SFLOAT_S8_UINT:
		result = VK_IMAGE_ASPECT_DEPTH_BIT | VK_IMAGE_ASPECT_STENCIL_BIT;
		break;
		// Undefined
	case VK_FORMAT_UNDEFINED:
		break;
		// Assume everything else is Color
	default:
		result = VK_IMAGE_ASPECT_COLOR_BIT;
		break;
	}
	return result;
}

void MGG_EffectResource_GetBytecode(const char* name, mgbyte*& bytecode, mgint& size)
{
	bytecode = nullptr;
	size = 0;

	// Get the handle of this DLL.
	HMODULE module;
	::GetModuleHandleExA(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, (LPCSTR)&MGG_EffectResource_GetBytecode, &module);

	LPCSTR id = "";

	if (strcmp(name, "AlphaTestEffect") == 0)
		id = MAKEINTRESOURCEA(C_AlphaTestEffect);
	else if (strcmp(name, "BasicEffect") == 0)
		id = MAKEINTRESOURCEA(C_BasicEffect);
	else if (strcmp(name, "DualTextureEffect") == 0)
		id = MAKEINTRESOURCEA(C_DualTextureEffect);
	else if (strcmp(name, "EnvironmentMapEffect") == 0)
		id = MAKEINTRESOURCEA(C_EnvironmentMapEffect);
	else if (strcmp(name, "SkinnedEffect") == 0)
		id = MAKEINTRESOURCEA(C_SkinnedEffect);
	else if (strcmp(name, "SpriteEffect") == 0)
		id = MAKEINTRESOURCEA(C_SpriteEffect);

	auto handle = ::FindResourceA(module, id, "BIN");
	if (handle == nullptr)
		return;

	size = ::SizeofResource(module, handle);
	if (size == 0)
		return;

	HGLOBAL global = ::LoadResource(module, handle);
	if (global == nullptr)
		return;

	bytecode = (mgbyte*)LockResource(global);
}

uint64_t CheckValidationLayerSupport(const std::vector<const char*>& validationLayers)
{
	uint32_t layerCount;
	vkEnumerateInstanceLayerProperties(&layerCount, nullptr);

	std::vector<VkLayerProperties> availableLayers(layerCount);
	vkEnumerateInstanceLayerProperties(&layerCount, availableLayers.data());

	uint64_t found = 0;

	for (int i = 0; i < validationLayers.size(); i++)
	{
		const char* layerName = validationLayers[i];

		for (const auto& layerProperties : availableLayers)
		{
			if (strcmp(layerName, layerProperties.layerName) == 0)
			{
				found |= ((uint64_t)1) << i;
				break;
			}
		}
	}

	return found;
}
MGG_GraphicsSystem* MGG_GraphicsSystem_Create()
{
	auto err = volkInitialize();
	if (err != VK_SUCCESS)
	{
		printf("Failed to initialize volk!");
		return nullptr;
	}

	VkApplicationInfo app_info = { VK_STRUCTURE_TYPE_APPLICATION_INFO };
	app_info.pNext = nullptr;
	app_info.apiVersion = VK_API_VERSION_1_0;

	// TODO: pass these thru from C# is more flexible!
	app_info.applicationVersion = VK_MAKE_VERSION(1, 0, 0);
	app_info.engineVersion = VK_MAKE_VERSION(1, 0, 0);
	app_info.pApplicationName = "Unknown";
	app_info.pEngineName = "MonoGame";

	std::vector<const char*> instanceExtensions;
#if defined(MG_SDL2)
	{
		uint32_t count;
		SDL_Vulkan_GetInstanceExtensions(nullptr, &count, nullptr);
		instanceExtensions.resize(count);

		SDL_Vulkan_GetInstanceExtensions(nullptr, &count, instanceExtensions.data());
	}
#else
#error Not Implemented!
#endif
	instanceExtensions.push_back(VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME);

	std::vector<const char*> enabledLayers;
	enabledLayers.push_back("VK_LAYER_KHRONOS_validation");
	//enabledLayers.push_back(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
	CheckValidationLayerSupport(enabledLayers);

	VkInstanceCreateInfo instance_create_info = { VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO };
	instance_create_info.pApplicationInfo = &app_info;
	instance_create_info.enabledExtensionCount = instanceExtensions.size();
	instance_create_info.ppEnabledExtensionNames = instanceExtensions.data();
	instance_create_info.enabledLayerCount = enabledLayers.size();
	instance_create_info.ppEnabledLayerNames = enabledLayers.data();
	instance_create_info.pNext = nullptr;

	VkInstance instance = VK_NULL_HANDLE;

	err = vkCreateInstance(&instance_create_info, nullptr, &instance);
	if (err != VK_SUCCESS)
	{
		printf("Failed to create Vulkan instance!");
		return nullptr;
	}

	volkLoadInstance(instance);

	auto system = new MGG_GraphicsSystem();
	system->instance = instance;

	// Gather the physical devices.
	{
		uint32_t count = 0;
		VkResult res = vkEnumeratePhysicalDevices(system->instance, &count, NULL);
		if (res == VK_SUCCESS)
		{
			VkPhysicalDevice* gpus = (VkPhysicalDevice*)calloc(count, sizeof(*gpus));

			res = vkEnumeratePhysicalDevices(system->instance, &count, gpus);
			if (res == VK_SUCCESS)
			{
				for (uint32_t i = 0; i < count; ++i)
				{
					auto adapter = new MGG_GraphicsAdapter();
					adapter->device = gpus[i];

					vkGetPhysicalDeviceProperties(adapter->device, &adapter->properties);
					vkGetPhysicalDeviceFeatures(adapter->device, &adapter->features);
					vkGetPhysicalDeviceMemoryProperties(adapter->device, &adapter->memory);

					system->adapters.push_back(adapter);
				}
			}

			free((void*)gpus);
		}
	}

	return system;
}

void MGG_GraphicsSystem_Destroy(MGG_GraphicsSystem* system)
{
	assert(system != nullptr);

	MG_NOT_IMPLEMEMTED;
}

MGG_GraphicsAdapter* MGG_GraphicsAdapter_Get(MGG_GraphicsSystem* system, mgint index)
{
	if (index < 0 || index >= system->adapters.size())
		return nullptr;

	return system->adapters[index];
}

void MGG_GraphicsAdapter_GetInfo(MGG_GraphicsAdapter* adapter, MGG_GraphicsAdaptor_Info& info)
{
	assert(adapter != nullptr);

	info.DeviceName = adapter->properties.deviceName;
	info.DeviceId = adapter->properties.deviceID;
	info.Revision = adapter->properties.driverVersion;
	info.VendorId = adapter->properties.vendorID;

	// TODO: Should we be generating a description?
	// Is there a description somewhere for us to get?
	static const char* description = "";
	info.Description = (void*)description;

	info.SubSystemId = 0;
	info.MonitorHandle = 0;

#ifdef _WIN32

	HMONITOR primaryMonitor = MonitorFromPoint(POINT{ 0, 0 }, MONITOR_DEFAULTTOPRIMARY);
	info.MonitorHandle = primaryMonitor;

	MONITORINFOEX monitorInfo;
	monitorInfo.cbSize = sizeof(MONITORINFOEX);
	GetMonitorInfo(primaryMonitor, &monitorInfo);

	if (adapter->modes.size() == 0)
	{
		// TODO: There is probably a better way to do all this
		// but this works for our current case.
		//
		// Like shouldn't this be per-graphics device/adapter?
		//
		// What about the color format?  Does it matter in 2024?
		//

		DEVMODE devMode;
		devMode.dmSize = sizeof(DEVMODE);
		
		int count = 0;

		while (EnumDisplaySettings(monitorInfo.szDevice, count, &devMode))
		{
			MGG_DisplayMode mode;
			mode.width = devMode.dmPelsWidth;
			mode.height = devMode.dmPelsHeight;
			mode.format = MGSurfaceFormat::Color;

			bool found = false;
			for (auto m : adapter->modes)
			{
				if (m.width == mode.width &&
					m.height == mode.height)
				{
					found = true;
					break;
				}
			}

			if (!found)
				adapter->modes.push_back(mode);

			count++;
		}
	}

	info.DisplayModeCount = adapter->modes.size();
	info.DisplayModes = adapter->modes.data();

	info.CurrentDisplayMode.width = monitorInfo.rcMonitor.right - monitorInfo.rcMonitor.left;
	info.CurrentDisplayMode.height = monitorInfo.rcMonitor.bottom - monitorInfo.rcMonitor.top;
	info.CurrentDisplayMode.format = MGSurfaceFormat::Color;

#else
#error NOT IMPLEMENTED!
#endif
}

static void mggCreateImage(MGG_GraphicsDevice* device, VkImageCreateInfo* info, MGG_Texture* texture)
{
	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;
	VkResult res = vmaCreateImage(device->allocator, info, &allocInfo, &texture->image, &texture->allocation, nullptr);
	VK_CHECK_RESULT(res);
}

static MGG_Texture* CreateDepthTexture(MGG_GraphicsDevice* device, VkFormat format, uint32_t width, uint32_t height)
{
	// TODO: Could convert this into a
	// general image creation method.

	MGG_Texture* texture = new MGG_Texture;
	memset(texture, 0, sizeof(MGG_Texture));

	VkImageCreateInfo& create_info = texture->info;
	create_info.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
	create_info.imageType = VK_IMAGE_TYPE_2D;
	create_info.format = format;
	create_info.extent = { width, height, 1 };
	create_info.mipLevels = 1;
	create_info.arrayLayers = 1;
	create_info.samples = VK_SAMPLE_COUNT_1_BIT;
	create_info.tiling = VK_IMAGE_TILING_OPTIMAL;
	create_info.usage = VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT;
	create_info.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
	create_info.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;

	texture->layout = VK_IMAGE_LAYOUT_UNDEFINED;
	texture->optimal_layout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

	mggCreateImage(device, &create_info, texture);

	MGVK_TransitionImageLayout(device, texture, 0, texture->optimal_layout);

	return texture;
}

static VkImageView CreateImageView(MGG_GraphicsDevice* device, MGG_Texture* texture, uint32_t level_count)
{
	VkFormat format = texture->info.format;
	uint32_t layer_count = texture->info.arrayLayers;

	VkImageAspectFlags aspect_mask = DetermineAspectMask(format);

	VkImageViewCreateInfo image_view_create_info = { VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO };
	image_view_create_info.image = texture->image;
	image_view_create_info.viewType = VK_IMAGE_VIEW_TYPE_2D;
	image_view_create_info.format = format;
	image_view_create_info.subresourceRange.aspectMask = aspect_mask;
	image_view_create_info.subresourceRange.baseMipLevel = 0;
	image_view_create_info.subresourceRange.levelCount = level_count;
	image_view_create_info.subresourceRange.baseArrayLayer = 0;
	image_view_create_info.subresourceRange.layerCount = layer_count;

	VkImageView view;
	VkResult res = vkCreateImageView(device->device, &image_view_create_info, NULL, &view);
	VK_CHECK_RESULT(res);

	return view;
}

static void MGVK_BufferCreate(MGG_GraphicsDevice* device, int sizeInBytes, VkBufferUsageFlags usage, VmaMemoryUsage flags, MGG_Buffer* buffer)
{
	VkBufferCreateInfo bufferInfo = { VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO };
	bufferInfo.size = sizeInBytes;
	bufferInfo.usage = usage;
	bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = flags;

	VkResult res = vmaCreateBuffer(device->allocator, &bufferInfo, &allocInfo, &buffer->buffer, &buffer->allocation, nullptr);
	VK_CHECK_RESULT(res);
}

static VkBufferUsageFlags ToUsage(MGBufferType type)
{
	VkBufferUsageFlags usage;
	switch (type)
	{
	case MGBufferType::Index:
		return VK_BUFFER_USAGE_INDEX_BUFFER_BIT;
		break;
	case MGBufferType::Vertex:
		return VK_BUFFER_USAGE_VERTEX_BUFFER_BIT;
		break;
	case MGBufferType::Constant:
		return VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;
		break;
	default:
		assert(0);
	}
}

static VkCommandBuffer MGVK_BeginNewCommandBuffer(MGG_GraphicsDevice* device)
{
	VkCommandBufferAllocateInfo allocInfo = {};
	allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
	allocInfo.commandPool = device->cmdPool;
	allocInfo.commandBufferCount = 1;

	VkCommandBuffer commandBuffer;
	vkAllocateCommandBuffers(device->device, &allocInfo, &commandBuffer);

	VkCommandBufferBeginInfo beginInfo = {};
	beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;

	vkBeginCommandBuffer(commandBuffer, &beginInfo);

	return commandBuffer;
}

static void MGVK_ExecuteAndFreeCommandBuffer(MGG_GraphicsDevice* device, VkCommandBuffer commandBuffer)
{
	vkEndCommandBuffer(commandBuffer);

	VkFence renderFence;
	{
		VkFenceCreateInfo fenceCreateInfo = {};
		fenceCreateInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
		fenceCreateInfo.flags = 0;
		vkCreateFence(device->device, &fenceCreateInfo, nullptr, &renderFence);
	}

	VkSubmitInfo submitInfo = {};
	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submitInfo.commandBufferCount = 1;
	submitInfo.pCommandBuffers = &commandBuffer;

	vkQueueSubmit(device->queue, 1, &submitInfo, renderFence);
	//vkQueueWaitIdle(device->queue);

	vkWaitForFences(device->device, 1, &renderFence, VK_TRUE, UINT64_MAX);

	vkDestroyFence(device->device, renderFence, nullptr);

	vkFreeCommandBuffers(device->device, device->cmdPool, 1, &commandBuffer);
}

static void MGVK_CopyBufferToImage(MGG_GraphicsDevice* device, VkBuffer buffer, VkImage image, int32_t x, int32_t y, int32_t level, uint32_t width, uint32_t height)
{
	VkCommandBuffer cmds = MGVK_BeginNewCommandBuffer(device);

	VkBufferImageCopy region = {};
	region.bufferOffset = 0;
	region.bufferRowLength = 0;
	region.bufferImageHeight = 0;

	region.imageSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
	region.imageSubresource.mipLevel = level;
	region.imageSubresource.baseArrayLayer = 0;
	region.imageSubresource.layerCount = 1;

	region.imageOffset = { x, y, 0 };
	region.imageExtent = { width, height, 1 };

	vkCmdCopyBufferToImage(cmds, buffer, image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &region);

	MGVK_ExecuteAndFreeCommandBuffer(device, cmds);
}

static void MGVK_CopyImageToBuffer(MGG_GraphicsDevice* device, VkImage image, VkBuffer buffer, int32_t x, int32_t y, int32_t level, uint32_t width, uint32_t height)
{
	VkCommandBuffer cmds = MGVK_BeginNewCommandBuffer(device);

	VkBufferImageCopy region = {};
	region.bufferOffset = 0;
	region.bufferRowLength = 0;
	region.bufferImageHeight = 0;

	region.imageSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
	region.imageSubresource.mipLevel = level;
	region.imageSubresource.baseArrayLayer = 0;
	region.imageSubresource.layerCount = 1;

	region.imageOffset = { x, y, level };
	region.imageExtent = { width, height, 1 };

	vkCmdCopyImageToBuffer(cmds, image, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, buffer, 1, &region);

	MGVK_ExecuteAndFreeCommandBuffer(device, cmds);
}

MGG_GraphicsDevice* MGG_GraphicsDevice_Create(MGG_GraphicsSystem* system, MGG_GraphicsAdapter* adapter)
{
	assert(system != nullptr);
	assert(adapter != nullptr);

	auto device = new MGG_GraphicsDevice();

	device->instance = system->instance;
	device->physicalDevice = adapter->device;

	// Capture some needed limits.
	device->minUniformBufferOffsetAlignment = adapter->properties.limits.minUniformBufferOffsetAlignment;

	uint32_t queueFamilyCount;
	vkGetPhysicalDeviceQueueFamilyProperties(device->physicalDevice, &queueFamilyCount, nullptr);
	assert(queueFamilyCount > 0);

	VkQueueFamilyProperties* queueFamilyProps = new VkQueueFamilyProperties[queueFamilyCount];
	uint32_t queueFamilyIndex = 0;
	{
		vkGetPhysicalDeviceQueueFamilyProperties(device->physicalDevice, &queueFamilyCount, queueFamilyProps);

		for (int i = 0; i < queueFamilyCount; i++)
		{
			if ((queueFamilyProps[i].queueFlags & VK_QUEUE_GRAPHICS_BIT) != 0)
			{
				queueFamilyIndex = i;
				break;
			}
		}
	}

	int queueCreateInfoCount = 0;
	VkDeviceQueueCreateInfo* queueCreateInfos = new VkDeviceQueueCreateInfo[queueFamilyCount];
	memset(queueCreateInfos, 0, sizeof(VkDeviceQueueCreateInfo) * queueFamilyCount);

	for (int i = 0; i < queueFamilyCount; i++)
	{
		const VkQueueFamilyProperties* properties = queueFamilyProps + i;
		float* queuePriorities = new float[properties->queueCount];

		for (int j = 0; j < properties->queueCount; ++j)
			queuePriorities[j] = 1.0f;

		queueCreateInfos[i].sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
		queueCreateInfos[i].queueFamilyIndex = i;
		queueCreateInfos[i].queueCount = 1;
		queueCreateInfos[i].pQueuePriorities = queuePriorities;
		++queueCreateInfoCount;
	}
	assert(queueFamilyCount == queueCreateInfoCount);

	std::vector<const char*> extensions;
	extensions.push_back(VK_KHR_SWAPCHAIN_EXTENSION_NAME);
	extensions.push_back(VK_EXT_CUSTOM_BORDER_COLOR_EXTENSION_NAME);

	VkPhysicalDeviceCustomBorderColorFeaturesEXT customBorderColorFeatures = {};
	customBorderColorFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_CUSTOM_BORDER_COLOR_FEATURES_EXT;
	customBorderColorFeatures.customBorderColors = VK_TRUE;
	customBorderColorFeatures.customBorderColorWithoutFormat = VK_TRUE;

	VkPhysicalDeviceFeatures2 deviceFeatures2 = {};
	deviceFeatures2.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_FEATURES_2;
	deviceFeatures2.pNext = &customBorderColorFeatures;
	deviceFeatures2.features = device->deviceFeatures;

	VkDeviceCreateInfo deviceCreateInfo = { VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO };
	deviceCreateInfo.queueCreateInfoCount = queueCreateInfoCount;
	deviceCreateInfo.pQueueCreateInfos = queueCreateInfos;
	deviceCreateInfo.enabledExtensionCount = extensions.size();
	deviceCreateInfo.ppEnabledExtensionNames = extensions.data();
	deviceCreateInfo.pEnabledFeatures = nullptr;
	deviceCreateInfo.pNext = &deviceFeatures2;

	auto res = vkCreateDevice(device->physicalDevice, &deviceCreateInfo, NULL, &device->device);
	VK_CHECK_RESULT(res);

	VmaAllocatorCreateInfo allocatorInfo = {};
	allocatorInfo.instance = system->instance;
	allocatorInfo.physicalDevice = device->physicalDevice;
	allocatorInfo.device = device->device;
	vmaCreateAllocator(&allocatorInfo, &device->allocator);

	vkGetDeviceQueue(device->device, queueFamilyIndex, 0, &device->queue);

	VkCommandPoolCreateInfo cmdPoolInfo = { VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO };
	cmdPoolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;
	cmdPoolInfo.queueFamilyIndex = queueFamilyIndex;
	res = vkCreateCommandPool(device->device, &cmdPoolInfo, nullptr, &device->cmdPool);
	VK_CHECK_RESULT(res);

	VkCommandBufferAllocateInfo comBufferInfo =
	{
		VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,
		NULL,
		device->cmdPool,
		VK_COMMAND_BUFFER_LEVEL_PRIMARY,
		1
	};

	device->frames = new MGVK_FrameState[kConcurrentFrameCount];
	memset(device->frames, 0, sizeof(MGVK_FrameState) * kConcurrentFrameCount);

	for (int i = 0; i < kConcurrentFrameCount; i++)
	{
		MGVK_CmdBuffer& cmd = device->frames[i].commandBuffer;

		res = vkAllocateCommandBuffers(device->device, &comBufferInfo, &cmd.buffer);
		VK_CHECK_RESULT(res);

		VkSemaphoreCreateInfo semaphore_create_info = { VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO };
		res = vkCreateSemaphore(device->device, &semaphore_create_info, NULL, &cmd.imageAcquiredSemaphore);
		VK_CHECK_RESULT(res);
		res = vkCreateSemaphore(device->device, &semaphore_create_info, NULL, &cmd.renderCompleteSemaphore);
		VK_CHECK_RESULT(res);

		VkFenceCreateInfo fence_create_info = { VK_STRUCTURE_TYPE_FENCE_CREATE_INFO };
		fence_create_info.flags = VK_FENCE_CREATE_SIGNALED_BIT;
		res = vkCreateFence(device->device, &fence_create_info, NULL, &cmd.completedFence);
		VK_CHECK_RESULT(res);
	}

	// Create the pipeline cache which is used at runtime
	// to speed up pipeline creation.
	VkPipelineCacheCreateInfo pipelineCache{ VK_STRUCTURE_TYPE_PIPELINE_CACHE_CREATE_INFO };
	pipelineCache.pNext = NULL;
	pipelineCache.initialDataSize = 0;
	pipelineCache.pInitialData = NULL;
	pipelineCache.flags = 0;
	res = vkCreatePipelineCache(device->device, &pipelineCache, nullptr, &device->pipelineCache);
	VK_CHECK_RESULT(res);

	//res = vkDeviceWaitIdle(device->device);
	//VK_CHECK_RESULT(res);
	// 
	// Initialize all the device state.
	device->scissorDirty = true;
	device->uniformsDirty = 0xFFFFFFFF;
	device->textureSamplerDirty = 0xFFFFFFFF;
	device->vertexBuffersDirty = 0xFFFFFFFF;
	device->pipelineStateDirty = true;
	device->currentTextureId = 1;
	device->currentSamplerId = 1;
	device->blendFactorDirty = true;
	device->renderTargetDirty = true;
	device->inRenderPass = false;
	device->blendFactor[0] = device->blendFactor[1] = device->blendFactor[2] = device->blendFactor[3] = 1;
	memset(device->descriptorSets, 0, sizeof(device->descriptorSets));
	memset(device->dynamicOffsets, 0, sizeof(device->dynamicOffsets));
	memset(device->vertexBuffers, 0, sizeof(device->vertexBuffers));
	memset(device->vertexOffsets, 0, sizeof(device->vertexOffsets));	
	memset(device->uniforms, 0, sizeof(device->uniforms));
	memset(device->textures, 0, sizeof(device->textures));
	memset(device->samplers, 0, sizeof(device->samplers));

	return device;
}

static void cleanupSwapChain(MGG_GraphicsDevice* device)
{
	vkQueueWaitIdle(device->queue);
	 
	// Destroy all the frame resources.


	// Destroy all frame buffers.
	for (auto pair : device->targetCache)
	{
		vkDestroyRenderPass(device->device, pair.second->renderPass, nullptr);
		vkDestroyFramebuffer(device->device, pair.second->framebuffer, nullptr);
		delete pair.second;
	}
	device->targetCache.clear();

	// Cleanup the swap chain images.
	for (size_t i = 0; i < kConcurrentFrameCount; i++)
	{
		auto chain = device->frames[i].swapchainTexture;
		if (chain == nullptr)
			continue;

		vkDestroyImageView(device->device, chain->target_view, nullptr);
		//vkDestroyImage(device->device, chain->image, nullptr);
		//vmaFreeMemory(device->allocator, chain->allocation);

		chain->target_view = VK_NULL_HANDLE;
		chain->image = VK_NULL_HANDLE;
		chain->allocation = nullptr;

		if (chain->depthTexture != nullptr)
		{
			vkDestroyImageView(device->device, chain->depthTexture->target_view, nullptr);
			vmaDestroyImage(device->allocator, chain->depthTexture->image, chain->depthTexture->allocation);
			delete chain->depthTexture;

			chain->depthTexture = nullptr;
		}

		delete chain;
		device->frames[i].swapchainTexture = nullptr;
	}

	if (device->swapchain != VK_NULL_HANDLE)
	{
		vkDestroySwapchainKHR(device->device, device->swapchain, nullptr);
		device->swapchain = VK_NULL_HANDLE;
	}
}

void MGG_GraphicsDevice_Destroy(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);

	cleanupSwapChain(device);

	for (auto pair : device->shader_programs)
	{
		vkDestroyPipelineLayout(device->device, pair.second->layout, nullptr);
		delete pair.second;
	}
	device->shader_programs.clear();

	vkDestroyPipelineCache(device->device, device->pipelineCache, nullptr);

	while (device->all_buffers.size() > 0)
		MGG_Buffer_Destroy(device, device->all_buffers[0]);

	for (size_t i = 0; i < kConcurrentFrameCount; i++)
	{
		auto cmd = device->frames[i].commandBuffer;

		vkDestroySemaphore(device->device, cmd.imageAcquiredSemaphore, nullptr);
		vkDestroySemaphore(device->device, cmd.renderCompleteSemaphore, nullptr);
		vkDestroyFence(device->device, cmd.completedFence, nullptr);
	}

	vkDestroyCommandPool(device->device, device->cmdPool, nullptr);

	for (size_t i = 0; i < kConcurrentFrameCount; i++)
		MGVK_DestroyFrameResources(device, i, true);

	vmaDestroyAllocator(device->allocator);

	vkDestroyDevice(device->device, nullptr);

	delete device;
}

void MGG_GraphicsDevice_GetCaps(MGG_GraphicsDevice* device, MGG_GraphicsDevice_Caps& caps)
{
	assert(device != nullptr);

	// TODO: Get actual stats from the device!

	caps.MaxTextureSlots = 16;
	caps.MaxVertexBufferSlots = 8;
	caps.MaxVertexTextureSlots = 8;

	// Vulkan shader profile from pipeline.
	caps.ShaderProfile = 80;
}

void MGVK_RecreateSwapChain(
	MGG_GraphicsDevice* device,
	void* nativeWindowHandle,
	mgint width,
	mgint height,
	VkFormat vkColor,
	VkFormat vkDepth)
{
	assert(device != nullptr);
	assert(nativeWindowHandle != nullptr);

	// Be sure we're done drawing.
	vkDeviceWaitIdle(device->device);

	VkResult res;

	// Create the surface.
#if defined(MG_SDL2)
	auto sdl_window = (SDL_Window*)nativeWindowHandle;
	if (sdl_window != device->window)
	{
		cleanupSwapChain(device);

		if (device->surface != nullptr)
			vkDestroySurfaceKHR(device->instance, device->surface, nullptr);

		SDL_Vulkan_CreateSurface(sdl_window, device->instance, &device->surface);

		device->window = sdl_window;
	}
#else
#error Not Implemented
#endif

	if (width == device->swapchainWidth &&
		height == device->swapchainHeight &&
		vkColor == device->colorFormat &&
		vkDepth == device->depthFormat &&
		device->swapchain != VK_NULL_HANDLE)
		return;

	cleanupSwapChain(device);

	device->swapchainWidth = width;
	device->swapchainHeight = height;
	device->colorFormat = vkColor;
	device->depthFormat = vkDepth;

	{
		VkSurfaceCapabilitiesKHR surface_capabilities;
		res = vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device->physicalDevice, device->surface, &surface_capabilities);
		VK_CHECK_RESULT(res);

		VkFormat surface_format = VK_FORMAT_UNDEFINED;
		uint32_t format_count = 0;
		res = vkGetPhysicalDeviceSurfaceFormatsKHR(device->physicalDevice, device->surface, &format_count, nullptr);
		VK_CHECK_RESULT(res);

		VkSurfaceFormatKHR* surfFormats = new VkSurfaceFormatKHR[format_count];
		res = vkGetPhysicalDeviceSurfaceFormatsKHR(device->physicalDevice, device->surface, &format_count, surfFormats);
		VK_CHECK_RESULT(res);

		surface_format = surfFormats[0].format;
		if ((1 == format_count) && (VK_FORMAT_UNDEFINED == surfFormats[0].format))
			surface_format = VK_FORMAT_B8G8R8A8_UNORM;

		VkSurfaceCapabilitiesKHR surface_caps;
		res = vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device->physicalDevice, device->surface, &surface_caps);
		VK_CHECK_RESULT(res);

		device->colorFormat = surface_format;

		delete[] surfFormats;
	}

	// Requested swapchain extent will be clamped based on the surface's min/max extent.
	// In most cases these will match the current extent, as specified at window/surface creation time.
	VkExtent2D extent;
	extent.width = device->swapchainWidth;
	extent.height = device->swapchainHeight;

	/*
	VkSwapchainPresentScalingCreateInfoEXT scalingCreateInfo = {};
	scalingCreateInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_PRESENT_SCALING_CREATE_INFO_EXT;
	scalingCreateInfo.scalingBehavior = VK_PRESENT_SCALING_ASPECT_RATIO_STRETCH_BIT_EXT | VK_PRESENT_SCALING_STRETCH_BIT_EXT;
	scalingCreateInfo.presentGravityX = VK_PRESENT_GRAVITY_CENTERED_BIT_EXT;
	scalingCreateInfo.presentGravityY = VK_PRESENT_GRAVITY_CENTERED_BIT_EXT;
	*/

	VkSwapchainCreateInfoKHR create_info = { VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR };
	create_info.surface = device->surface;
	create_info.minImageCount = kConcurrentFrameCount;
	create_info.imageFormat = device->colorFormat;
	create_info.imageColorSpace = VK_COLORSPACE_SRGB_NONLINEAR_KHR;
	create_info.imageExtent = extent;
	create_info.imageArrayLayers = 1;
	create_info.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT | VK_IMAGE_USAGE_TRANSFER_DST_BIT;
	create_info.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
	create_info.preTransform = VK_SURFACE_TRANSFORM_IDENTITY_BIT_KHR;
	create_info.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
	create_info.presentMode = VK_PRESENT_MODE_MAILBOX_KHR;
	create_info.clipped = true;
	//create_info.pNext = &scalingCreateInfo;
	res = vkCreateSwapchainKHR(device->device, &create_info, nullptr, &device->swapchain);
	VK_CHECK_RESULT(res);

	uint32_t swapchainCount = 0;
	res = vkGetSwapchainImagesKHR(device->device, device->swapchain, &swapchainCount, NULL);
	VK_CHECK_RESULT(res);

	VkImage* swapchainImages = new VkImage[swapchainCount];
	res = vkGetSwapchainImagesKHR(device->device, device->swapchain, &swapchainCount, swapchainImages);
	VK_CHECK_RESULT(res);

	for (uint32_t i = 0; i < swapchainCount; ++i)
	{
		VkImageCreateInfo image_create_info = { VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO };
		image_create_info.imageType = VK_IMAGE_TYPE_2D;
		image_create_info.format = device->colorFormat;
		image_create_info.extent = { device->swapchainWidth, device->swapchainHeight, 1 };
		image_create_info.mipLevels = 1;
		image_create_info.arrayLayers = 1;
		image_create_info.samples = VK_SAMPLE_COUNT_1_BIT;
		image_create_info.tiling = VK_IMAGE_TILING_OPTIMAL;
		image_create_info.usage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
		image_create_info.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
		image_create_info.initialLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

		MGG_Texture* texture = new MGG_Texture;
		memset(texture, 0, sizeof(MGG_Texture));

		VkMemoryRequirements memory_requirements = { 0 };
		vkGetImageMemoryRequirements(device->device, swapchainImages[i], &memory_requirements);

		texture->info = image_create_info;
		texture->image = swapchainImages[i];
		texture->isSwapchain = texture->isTarget = true;

		texture->target_view = CreateImageView(device, texture, 1);

		if (device->depthFormat != VK_FORMAT_UNDEFINED)
		{
			texture->depthTexture = CreateDepthTexture(device, device->depthFormat, texture->info.extent.width, texture->info.extent.height);
			texture->depthTexture->target_view = CreateImageView(device, texture->depthTexture, 1);
		}

		device->frames[i].swapchainTexture = texture;
	}

	delete[] swapchainImages;
}

void MGVK_RecreateSwapChain(MGG_GraphicsDevice* device)
{
	cleanupSwapChain(device);

	MGVK_RecreateSwapChain(
		device,
		device->window,
		device->swapchainWidth,
		device->swapchainHeight,
		device->colorFormat,
		device->depthFormat);

	MGG_GraphicsDevice_SetRenderTargets(device, nullptr, 0);
}

void MGG_GraphicsDevice_ResizeSwapchain(
	MGG_GraphicsDevice* device,
	void* nativeWindowHandle,
	mgint width,
	mgint height,
	MGSurfaceFormat color,
	MGDepthFormat depth)
{
	auto vkColor = ToVkFormat(color);
	auto vkDepth = ToVkFormat(depth);

	MGVK_RecreateSwapChain(device, nativeWindowHandle, width, height, vkColor, vkDepth);
}


static void MGVK_ProcessDescriptorCaches(MGG_GraphicsDevice* device, FrameCounter currentFrame)
{
	// Here we go thru the shaders and move any
	// descriptors that haven't been used in a few
	// frames to the correct free list.

	// TODO: This could be a ton of iteration.
	// Maybe keep a list of recently used shaders
	// or descriptors and only process those?

	auto shader = device->all_shaders.begin();
	for (; shader != device->all_shaders.end(); shader++)
	{
		auto& usedSets = (*shader)->usedSets;
		auto& freeSets = (*shader)->freeSets;

		auto pair = usedSets.begin();
		for (; pair != usedSets.end();)
		{
			auto diff = currentFrame - pair->second->frame;
			if (diff < kFreeFrames || (0xFFFF - diff) < kFreeFrames)
			{
				pair++;
				continue;
			}

			freeSets.push(pair->second);
			pair = usedSets.erase(pair);
		}
	}
}

void MGVK_BeginFrame(MGVK_CmdBuffer& cmd)
{
	VkCommandBufferBeginInfo beginInfo =
	{
		VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO,
		NULL,
		VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT,
		NULL
	};

	auto res = vkResetCommandBuffer(cmd.buffer, VK_COMMAND_BUFFER_RESET_RELEASE_RESOURCES_BIT);
	VK_CHECK_RESULT(res);
	res = vkBeginCommandBuffer(cmd.buffer, &beginInfo);
	VK_CHECK_RESULT(res);

	//vkCmdSetDepthClampEnableEXT(cmd.buffer, VK_TRUE);
}

mgint MGG_GraphicsDevice_BeginFrame(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);

	VkResult res;

	const FrameCounter currentFrame = device->frame;
	const FrameCounter frameIndex = currentFrame % kConcurrentFrameCount;
	MGVK_FrameState& frame = device->frames[frameIndex];
	MGVK_CmdBuffer& cmd = frame.commandBuffer;

	MGVK_ProcessDescriptorCaches(device, currentFrame);

	res = vkWaitForFences(device->device, 1, &cmd.completedFence, VK_TRUE, UINT64_MAX);
	VK_CHECK_RESULT(res);
	res = vkResetFences(device->device, 1, &cmd.completedFence);
	VK_CHECK_RESULT(res);

	device->swapchain_image_index = 0;
	res = vkAcquireNextImageKHR(device->device, device->swapchain, UINT64_MAX,
		cmd.imageAcquiredSemaphore, VK_NULL_HANDLE, &device->swapchain_image_index);
	VK_CHECK_RESULT(res);

	frame.uniformOffset = 0;
	if (frame.uniforms == NULL)
		frame.uniforms = MGVK_Buffer_Create(device, MGBufferType::Constant, 4 * 1024 * 1024, true);

	MGVK_BeginFrame(cmd);

	//device->dynamicOffsets[0] = 0;
	//device->dynamicOffsets[1] = 0;

	frame.is_recording = true;

	return frameIndex;
}

void MGG_GraphicsDevice_Clear(MGG_GraphicsDevice* device, MGClearOptions options, Vector4& color, mgfloat depth, mgint stencil)
{
	assert(device != nullptr);

	// Make sure something was set to be cleared.
	if ((mgint)options == 0)
		return;

	auto currentFrame = device->frame;
	auto frameIndex = currentFrame % kConcurrentFrameCount;
	auto& frame = device->frames[frameIndex];
	auto& cmd = frame.commandBuffer;
	assert(frame.is_recording);

	MGVK_UpdateRenderPass(device, currentFrame, cmd);

	int num_attachments = 0;

	VkClearAttachment attachments[5];
	memset(attachments, 0, sizeof(attachments));

	auto targets = device->pipelineState.targets;

	if (((int)options & (int)MGClearOptions::Target) != 0)
	{
		for (int i = 0; i < targets->set.numTargets; i++)
		{
			VkClearAttachment* attachment = &attachments[num_attachments];
			attachment->aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
			attachment->colorAttachment = num_attachments;
			attachment->clearValue.color.float32[0] = color.X;
			attachment->clearValue.color.float32[1] = color.Y;
			attachment->clearValue.color.float32[2] = color.Z;
			attachment->clearValue.color.float32[3] = color.W;
			num_attachments++;
		}
	}

	bool clearDepth = ((int)options & (int)MGClearOptions::DepthBuffer) != 0;
	bool clearStencil = ((int)options & (int)MGClearOptions::Stencil) != 0;
	if (clearDepth || clearStencil)
	{
		VkClearAttachment* attachment = &attachments[num_attachments++];

		if (clearDepth)
			attachment->aspectMask |= VK_IMAGE_ASPECT_DEPTH_BIT;
		if (clearStencil)
			attachment->aspectMask |= VK_IMAGE_ASPECT_STENCIL_BIT;

		attachment->clearValue.depthStencil.depth = depth;
		attachment->clearValue.depthStencil.stencil = stencil;
	}

	VkClearRect rect;
	rect.rect.offset.x = 0;
	rect.rect.offset.y = 0;
	rect.baseArrayLayer = 0;
	rect.layerCount = 1;

	// Clear always clears the entire target.
	rect.rect.extent.width = targets->width;
	rect.rect.extent.height = targets->height;

	vkCmdClearAttachments(cmd.buffer, num_attachments, attachments, 1, &rect);
}

static void MGVK_DestroyTargetSets(MGG_GraphicsDevice* device, std::function<bool(const MGVK_TargetSetCache*)> compare)
{
	auto& cache = device->targetCache;
	auto itr = cache.cbegin();

	while (itr != cache.cend())
	{
		if (!compare(itr->second))
			itr++;
		else
		{
			vkDestroyFramebuffer(device->device, itr->second->framebuffer, nullptr);
			vkDestroyRenderPass(device->device, itr->second->renderPass, nullptr);
			delete itr->second;
			cache.erase(itr++);
		}
	}
}

static void MGVK_DestroyPipelines(MGG_GraphicsDevice* device, std::function<bool(const MGVK_PipelineState&)> compare)
{
	auto& pipelines = device->pipelines;
	auto itr = pipelines.cbegin();

	while (itr != pipelines.cend())
	{
		if (!compare(itr->second.state))
			itr++;
		else
		{
			vkDestroyPipeline(device->device, itr->second.cache, nullptr);
			pipelines.erase(itr++);
		}
	}
}

static void MGVK_DestroyFrameResources(MGG_GraphicsDevice* device, mgint currentFrame, mgbool free_all)
{
	assert(device != nullptr);
	assert(currentFrame >= 0);

	auto frameIndex = currentFrame % kConcurrentFrameCount;
	auto& frame = device->frames[frameIndex];

	// Delete resources that haven't been used in a few frames 
	{
		while (device->destroyBuffers.size() > 0)
		{
			auto buffer = device->destroyBuffers.front();
			auto diff = currentFrame - buffer->frame;
			if (!free_all && diff < kFreeFrames || (0xFFFF - diff) < kFreeFrames)
				break;

			device->destroyBuffers.pop();
			vmaDestroyBuffer(device->allocator, buffer->buffer, buffer->allocation);
			delete buffer;
		}

		while (device->destroyTextures.size() > 0)
		{
			auto texture = device->destroyTextures.front();
			auto diff = currentFrame - texture->frame;
			if (!free_all && diff < kFreeFrames || (0xFFFF - diff) < kFreeFrames)
				break;

			device->destroyTextures.pop();

			if (texture->isTarget)
			{
				MGVK_DestroyPipelines(device, [texture](const MGVK_PipelineState& s)
				{
					return	s.targets->set.targets[0] == texture ||
						s.targets->set.targets[1] == texture ||
						s.targets->set.targets[2] == texture ||
						s.targets->set.targets[3] == texture;
				});

				MGVK_DestroyTargetSets(device, [texture](const MGVK_TargetSetCache* s)
				{
					return	s->set.targets[0] == texture ||
						s->set.targets[1] == texture ||
						s->set.targets[2] == texture ||
						s->set.targets[3] == texture;
				});
			}

			if (texture->target_view != VK_NULL_HANDLE)
				vkDestroyImageView(device->device, texture->target_view, nullptr);
			if (texture->view != VK_NULL_HANDLE)
				vkDestroyImageView(device->device, texture->view, nullptr);

			vmaDestroyImage(device->allocator, texture->image, texture->allocation);
			delete texture;
		}

		while (device->destroyBlendStates.size() > 0)
		{
			auto state = device->destroyBlendStates.front();
			auto diff = currentFrame - state->frame;
			if (!free_all && diff < kFreeFrames || (0xFFFF - diff) < kFreeFrames)
				break;

			device->destroyBlendStates.pop();

			MGVK_DestroyPipelines(device, [state](const MGVK_PipelineState& s) { return s.blendState == state; });
			delete state;
		}

		while (device->destroyRasterizerStates.size() > 0)
		{
			auto state = device->destroyRasterizerStates.front();
			auto diff = currentFrame - state->frame;
			if (!free_all && diff < kFreeFrames || (0xFFFF - diff) < kFreeFrames)
				break;

			device->destroyRasterizerStates.pop();

			MGVK_DestroyPipelines(device, [state](const MGVK_PipelineState& s) { return s.rasterizerState == state; });
			delete state;
		}

		while (device->destroyDepthStencilStates.size() > 0)
		{
			auto state = device->destroyDepthStencilStates.front();
			auto diff = currentFrame - state->frame;
			if (!free_all && diff < kFreeFrames || (0xFFFF - diff) < kFreeFrames)
				break;

			device->destroyDepthStencilStates.pop();

			MGVK_DestroyPipelines(device, [state](const MGVK_PipelineState& s) { return s.depthStencilState == state; });
			delete state;
		}
	}
}

void MGG_GraphicsDevice_Present(MGG_GraphicsDevice* device, mgint currentFrame, mgint syncInterval)
{
	assert(device != nullptr);
	assert(syncInterval >= 0);
	assert(currentFrame >= 0);
	assert((device->frame % kConcurrentFrameCount) == currentFrame);

	auto frameIndex = currentFrame % kConcurrentFrameCount;
	auto& frame = device->frames[frameIndex];
	assert(frame.is_recording);

	auto& cmd = frame.commandBuffer;

	if (device->inRenderPass)
	{
		vkCmdEndRenderPass(cmd.buffer);
		device->inRenderPass = false;
	}

	VkResult res = vkEndCommandBuffer(cmd.buffer);
	VK_CHECK_RESULT(res);

	VkPipelineStageFlags wait_dst_stage_mask = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
	VkSubmitInfo submitInfo = { VK_STRUCTURE_TYPE_SUBMIT_INFO };
	submitInfo.waitSemaphoreCount = 1;
	submitInfo.pWaitSemaphores = &cmd.imageAcquiredSemaphore;
	submitInfo.pWaitDstStageMask = &wait_dst_stage_mask;
	submitInfo.commandBufferCount = 1;
	submitInfo.pCommandBuffers = &cmd.buffer;
	submitInfo.signalSemaphoreCount = 1;
	submitInfo.pSignalSemaphores = &cmd.renderCompleteSemaphore;

	res = vkQueueSubmit(device->queue, 1, &submitInfo, cmd.completedFence);
	VK_CHECK_RESULT(res);

	VkPresentInfoKHR presentInfo = { VK_STRUCTURE_TYPE_PRESENT_INFO_KHR };
	presentInfo.waitSemaphoreCount = 1;
	presentInfo.pWaitSemaphores = &cmd.renderCompleteSemaphore;
	presentInfo.swapchainCount = 1;
	presentInfo.pSwapchains = &device->swapchain;
	presentInfo.pImageIndices = &device->swapchain_image_index;

	res = vkQueuePresentKHR(device->queue, &presentInfo);
	if (res == VK_ERROR_OUT_OF_DATE_KHR)
		MGVK_RecreateSwapChain(device);
	else
	{
		VK_CHECK_RESULT(res);
	}

	++device->frame;

	// Move the pending buffers to the free list 
	// for reuse on the next frame.
	if (device->pending != nullptr)
	{
		auto last = device->pending;
		while (true)
		{
			if (last->next == nullptr)
				break;
			last = last->next;
		}

		last->next = device->free;
		device->free = device->pending;
		device->pending = nullptr;
	}

	// Buffers discarded this frame can be moved
	// into the pending list for a future frame.
	device->pending = device->discarded;
	device->discarded = nullptr;

	// Cleanup resources for the next frame.
	MGVK_DestroyFrameResources(device, currentFrame, false);
}

void MGG_GraphicsDevice_SetBlendState(MGG_GraphicsDevice* device, MGG_BlendState* state, mgfloat factorR, mgfloat factorG, mgfloat factorB, mgfloat factorA)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (device->pipelineState.blendState != state)
	{
		device->pipelineStateDirty = true;
		device->pipelineState.blendState = state;
	}

	if (device->blendFactor[0] != factorR ||
		device->blendFactor[1] != factorG ||
		device->blendFactor[2] != factorB ||
		device->blendFactor[3] != factorA)
	{
		device->blendFactor[0] = factorR;
		device->blendFactor[1] = factorG;
		device->blendFactor[2] = factorB;
		device->blendFactor[3] = factorA;
		device->blendFactorDirty = true;
	}
}

void MGG_GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (device->pipelineState.depthStencilState != state)
	{
		device->pipelineStateDirty = true;
		device->pipelineState.depthStencilState = state;
	}
}

void MGG_GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (device->pipelineState.rasterizerState != state)
	{
		device->pipelineStateDirty = true;
		device->pipelineState.rasterizerState = state;
	}
}

void MGG_GraphicsDevice_GetTitleSafeArea(mgint& x, mgint& y, mgint& width, mgint& height)
{
	// Nothing for PC here unless we want to support
	// things like Steam TV modes and we need platform
	// specific calls for that.
}

void MGG_GraphicsDevice_SetViewport(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height, mgfloat minDepth, mgfloat maxDepth)
{
	assert(device != nullptr);

	VkViewport& viewport = device->viewport;
	viewport.x = x;
	viewport.y = y;
	viewport.width = width;
	viewport.height = height;
	viewport.minDepth = minDepth;
	viewport.maxDepth = maxDepth;

	auto frameIndex = device->frame % kConcurrentFrameCount;
	auto& frame = device->frames[frameIndex];
	assert(frame.is_recording);

	vkCmdSetViewport(frame.commandBuffer.buffer, 0, 1, &viewport);
}

void MGG_GraphicsDevice_SetScissorRectangle(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height)
{
	assert(device != nullptr);

	VkRect2D& scissor = device->scissor;
	scissor.offset.x = x;
	scissor.offset.y = y;
	scissor.extent.width = width;
	scissor.extent.height = height;
	device->scissorDirty = true;
}

void MGG_GraphicsDevice_SetRenderTargets(MGG_GraphicsDevice* device, MGG_Texture** targets, mgint count)
{
	assert(device != nullptr);

	if (targets == nullptr || count == 0)
	{
		auto currentFrame = device->frame;
		auto frameIndex = currentFrame % kConcurrentFrameCount;
		auto& frame = device->frames[frameIndex];

		device->targets.targets[0] = frame.swapchainTexture;
		memset(device->targets.targets + 1, 0, 3 * sizeof(MGG_Texture*));
		device->targets.numTargets = 1;
	}
	else
	{
		memcpy(device->targets.targets, targets, count * sizeof(MGG_Texture*));
		memset(device->targets.targets + count, 0, (4 - count) * sizeof(MGG_Texture*));
		device->targets.numTargets = count;
	}

	device->pipelineStateDirty = true;
	device->renderTargetDirty = true;
}

void MGG_GraphicsDevice_SetConstantBuffer(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Buffer* buffer)
{
	assert(device != nullptr);
	assert(buffer != nullptr);

	// TODO: slot ??

	MGG_Buffer** uniforms = device->uniforms;

	if (uniforms[(int)stage] != buffer)
	{
		uniforms[(int)stage] = buffer;
		device->uniformsDirty |= 1 << (int)stage;
	}
	else
	{
		if (buffer->dirty)		
			device->uniformsDirty |= 1 << (int)stage;
	}
}

void MGG_GraphicsDevice_SetTexture(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Texture* texture)
{
	assert(device != nullptr);
	assert(slot >= 0);
	assert(slot < MAX_TEXTURE_SLOTS);

	device->textures[slot] = texture;
	device->textureSamplerDirty |= 1 << slot;
}

void MGG_GraphicsDevice_SetSamplerState(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_SamplerState* state)
{
	assert(device != nullptr);
	assert(slot >= 0);
	assert(slot < MAX_TEXTURE_SLOTS);

	device->samplers[slot] = state;
	device->textureSamplerDirty |= 1 << slot;
}

void MGG_GraphicsDevice_SetIndexBuffer(MGG_GraphicsDevice* device, MGIndexElementSize size, MGG_Buffer* buffer)
{
	assert(device != nullptr);
	assert(buffer != nullptr);

	device->indexBuffer = buffer;
	device->indexBufferSize = size;
}

void MGG_GraphicsDevice_SetVertexBuffer(MGG_GraphicsDevice* device, mgint slot, MGG_Buffer* buffer, mgint vertexOffset)
{
	assert(device != nullptr);
	assert(buffer != nullptr);

	// TODO: Support multiple VB streams!
	assert(slot == 0);
	assert(vertexOffset == 0);

	device->vertexBuffers[slot] = buffer;
	device->vertexOffsets[slot] = vertexOffset;
	device->vertexBuffersDirty |= 1 << slot;
}

void MGG_GraphicsDevice_SetShader(MGG_GraphicsDevice* device, MGShaderStage stage, MGG_Shader* shader)
{
	assert(device != nullptr);
	assert(shader != nullptr);
	assert(shader->stage == stage);

	device->shaders[(mgint)stage] = shader;
	device->shaderDirty = true;
}

void MGG_GraphicsDevice_SetInputLayout(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
	assert(device != nullptr);
	assert(layout != nullptr);

	if (layout != device->pipelineState.layout)
	{
		device->pipelineStateDirty = true;
		device->pipelineState.layout = layout;
	}
}

static void MGVK_UpdateRenderPass(MGG_GraphicsDevice* device, FrameCounter currentFrame, MGVK_CmdBuffer& cmd)
{
	if (!device->renderTargetDirty)
		return;

	if (device->inRenderPass)
	{
		vkCmdEndRenderPass(cmd.buffer);
		device->inRenderPass = false;
	}

	// Lookup the texture set in the cache.
	uint32_t hash = MG_ComputeHash((mgbyte*)&device->targets, sizeof(MGVK_TargetSet));
	MGVK_TargetSetCache* cached = device->targetCache[hash];
	if (!cached)
	{
		cached = new MGVK_TargetSetCache();
		cached->set = device->targets;

		auto first = cached->set.targets[0];
		assert(first);
		cached->width = first->info.extent.width;
		cached->height = first->info.extent.height;

		VkImageView attachments[5];
		VkAttachmentReference color_attachments[5];
		VkAttachmentReference depth_stencil_attachment;
		VkAttachmentDescription attachment_descs[5];
		memset(attachment_descs, 0, sizeof(attachment_descs));
		int num_attachments = 0;
		int num_color_attachments = 0;

		for (int i = 0; i < cached->set.numTargets; i++)
		{
			auto target = cached->set.targets[i];
			assert(target);

			attachments[num_attachments] = target->target_view;

			color_attachments[num_attachments].attachment = num_attachments;
			color_attachments[num_attachments].layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

			attachment_descs[num_attachments].format = target->info.format;
			attachment_descs[num_attachments].samples = VK_SAMPLE_COUNT_1_BIT;
			attachment_descs[num_attachments].loadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
			attachment_descs[num_attachments].storeOp = VK_ATTACHMENT_STORE_OP_STORE;
			attachment_descs[num_attachments].stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
			attachment_descs[num_attachments].stencilStoreOp = VK_ATTACHMENT_STORE_OP_STORE;
			attachment_descs[num_attachments].initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
			attachment_descs[num_attachments].finalLayout = target->isSwapchain ? VK_IMAGE_LAYOUT_PRESENT_SRC_KHR : VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
			num_attachments++;
			num_color_attachments++;
		}

		auto depth = cached->set.targets[0]->depthTexture;
		if (depth)
		{
			depth_stencil_attachment.attachment = num_attachments;
			depth_stencil_attachment.layout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

			attachments[num_attachments] = depth->target_view;

			attachment_descs[num_attachments].format = depth->info.format;
			attachment_descs[num_attachments].samples = VK_SAMPLE_COUNT_1_BIT;
			attachment_descs[num_attachments].loadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
			attachment_descs[num_attachments].storeOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
			attachment_descs[num_attachments].stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
			attachment_descs[num_attachments].stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
			attachment_descs[num_attachments].initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
			attachment_descs[num_attachments].finalLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
			num_attachments++;
		}

		{
			VkSubpassDescription subpass_desc = {};
			subpass_desc.pipelineBindPoint = VK_PIPELINE_BIND_POINT_GRAPHICS;
			subpass_desc.colorAttachmentCount = num_color_attachments;
			subpass_desc.pColorAttachments = color_attachments;
			if (depth)
				subpass_desc.pDepthStencilAttachment = &depth_stencil_attachment;

			VkRenderPassCreateInfo create_info = { VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO };
			create_info.attachmentCount = num_attachments;
			create_info.pAttachments = attachment_descs;
			create_info.subpassCount = 1;
			create_info.pSubpasses = &subpass_desc;

			VkResult res = vkCreateRenderPass(device->device, &create_info, nullptr, &cached->renderPass);
			VK_CHECK_RESULT(res);
		}

		{
			VkFramebufferCreateInfo create_info = { VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO };
			create_info.renderPass = cached->renderPass;
			create_info.attachmentCount = num_attachments;
			create_info.pAttachments = attachments;
			create_info.width = cached->width;
			create_info.height = cached->height;
			create_info.layers = 1;

			VkResult res = vkCreateFramebuffer(device->device, &create_info, nullptr, &cached->framebuffer);
			VK_CHECK_RESULT(res);
		}

		device->targetCache[hash] = cached;
	}

	// Set the cache for the changed pipeline state.
	device->pipelineState.targets = cached;

	// Set default viewport and scissor.
	MGG_GraphicsDevice_SetViewport(device, 0, 0, cached->width, cached->height, 0, 1);
	MGG_GraphicsDevice_SetScissorRectangle(device, 0, 0, cached->width, cached->height);

	// Setup the render pass and pipeline.
	VkRect2D render_area;
	render_area.offset.x = 0;
	render_area.offset.y = 0;
	render_area.extent.width = cached->width;
	render_area.extent.height = cached->height;

	VkRenderPassBeginInfo render_pass_begin_info = { VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO };
	render_pass_begin_info.renderPass = cached->renderPass;
	render_pass_begin_info.framebuffer = cached->framebuffer;
	render_pass_begin_info.renderArea = render_area;
	render_pass_begin_info.clearValueCount = 0;
	render_pass_begin_info.pClearValues = NULL;
	vkCmdBeginRenderPass(cmd.buffer, &render_pass_begin_info, VK_SUBPASS_CONTENTS_INLINE);

	device->inRenderPass = true;
	device->renderTargetDirty = false;
	device->pipelineStateDirty = true;
}

static void MGVK_UpdateDescriptors(MGG_GraphicsDevice* device, FrameCounter currentFrame, MGG_Shader* shader, VkDescriptorSet* current, uint32_t* dynamicOffset)
{
	// If nothing is dirty then skip the update.
	if ((device->uniformsDirty & shader->uniformSlots) == 0 &&
		(device->textureSamplerDirty & shader->textureSlots) == 0 &&
		(device->textureSamplerDirty & shader->samplerSlots) == 0)
		return;

	// If we got here we must have some sort of bindings!
	assert(!shader->bindings.empty());

	// Apply the bindings to the new descriptor set.
	const FrameCounter frameIndex = currentFrame % kConcurrentFrameCount;
	auto& offset = device->frames[frameIndex].uniformOffset;
	auto buffer = device->frames[frameIndex].uniforms;

	// We have some dirty state... so the descriptor
	// needs to be updated before we draw.

	MGVK_DescriptorInfo* info;

	// First generate a hash of the new state.
	uint32_t hash = MG_ComputeHash(shader->uniformSlots);
	hash = MG_ComputeHash(shader->textureSlots, hash);
	uint32_t dirty = shader->textureSlots;
	for (int i = 0; i < 16; i++)
	{
		uint32_t mask = 1 << i;
		if ((dirty & mask) == 0)
			continue;

		device->textures[i]->frame = currentFrame;

		hash = MG_ComputeHash(device->textures[i]->id, hash);
		hash = MG_ComputeHash(device->samplers[i]->id, hash);

		// Early out if there are no more used slots.
		dirty &= ~mask;
		if (!dirty)
			break;
	}
	//hash = MG_ComputeHash(frame_index);

	// Do we have this same descriptor cached?
	info = shader->usedSets[hash];
	if (!info)
	{
		// The descriptor wasn't cached... so we need to
		// create a new one from the free sets.
		if (shader->freeSets.size() > 0)
		{
			info = shader->freeSets.front();
			shader->freeSets.pop();
		}
		else
		{
			// TODO: We're out of free sets... allocate more?
			assert(0);
		}

		// Cache the new or recycled set for later use.
		shader->usedSets[hash] = info;

		for (int i = 0; i < shader->bindings.size(); i++)
		{
			auto& w = shader->writes[i];
			w.dstSet = info->set;

			if (w.descriptorType == VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC)
			{
				auto src = device->uniforms[(int)shader->stage + w.dstBinding];
				((VkDescriptorBufferInfo*)w.pBufferInfo)->buffer = buffer->buffer;
				((VkDescriptorBufferInfo*)w.pBufferInfo)->range = src->dataSize;
			}
			else if (w.descriptorType == VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER)
			{
				int slot = w.dstBinding - 32;
				((VkDescriptorImageInfo*)w.pImageInfo)->imageView = device->textures[slot]->view;
				((VkDescriptorImageInfo*)w.pImageInfo)->sampler = device->samplers[slot]->sampler;
			}
			else
			{
				// This should never happen!
				assert(0);
			}
		}

		vkUpdateDescriptorSets(device->device, shader->bindings.size(), shader->writes, 0, nullptr);
	}

	// Update the uniform data into the buffer.
	if (shader->uniformSlots)
	{
		for (int i = 0; i < shader->bindings.size(); i++)
		{
			auto& w = shader->writes[i];
			if (w.descriptorType == VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC)
			{
				auto src = device->uniforms[(int)shader->stage + w.dstBinding];

				*dynamicOffset = offset;

				if ((offset + src->dataSize) < buffer->dataSize)
					MGVK_BufferCopyAndFlush(device, buffer, offset, src->push, src->dataSize);
				else
				{
					// TODO: Discard or wrap around?
					assert(0);
				}

				// Must meet the alignment limit for the device.
				auto alignedSize = MG_AlignUp(src->dataSize, (int)device->minUniformBufferOffsetAlignment);
				offset += alignedSize;
			}
		}
	}

	// Mark when we used this set last so we
	// can move it back the free list later.
	info->frame = currentFrame;
	*current = info->set;
}

static MGVK_Program* MGVK_ProgramGetOrCreate(MGG_GraphicsDevice* device, MGG_Shader* vertexShaderData, MGG_Shader* pixelShaderData)
{
	assert(device);
	assert(vertexShaderData);
	assert(pixelShaderData);

	uint64_t programId = ((uint64_t)vertexShaderData->id) | (((uint64_t)pixelShaderData->id) << 32);
	MGVK_Program* program = device->shader_programs[programId];
	if (program != nullptr)
		return program;

	program = new MGVK_Program();
	program->vertex = vertexShaderData;
	program->pixel = pixelShaderData;

	VkResult res;

	int count = 0;
	VkDescriptorSetLayout layouts[2];
	if (program->vertex->setLayout)
		layouts[count++] = program->vertex->setLayout;
	if (program->pixel->setLayout)
		layouts[count++] = program->pixel->setLayout;

	VkPipelineLayoutCreateInfo pipelineLayoutInfo;
	memset(&pipelineLayoutInfo, 0, sizeof(pipelineLayoutInfo));
	pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
	pipelineLayoutInfo.setLayoutCount = count;
	pipelineLayoutInfo.pSetLayouts = layouts;
	pipelineLayoutInfo.pushConstantRangeCount = 0;
	pipelineLayoutInfo.pPushConstantRanges = nullptr;
	res = vkCreatePipelineLayout(device->device, &pipelineLayoutInfo, nullptr, &program->layout);
	VK_CHECK_RESULT(res);

	device->shader_programs[programId] = program;

	return program;
}

static VkPipeline MGVK_CreatePipeline(MGG_GraphicsDevice* device)
{
	// TODO: We could hit hash collisions here, so we 
	// eventually need to implement some fast hash
	// collision detection and resolution.

	// First see if we've cached this state before.
	uint32_t hash = MG_ComputeHash((mgbyte*)&device->pipelineState, sizeof(MGVK_PipelineState));
	auto itr = device->pipelines.find(hash);
	if (itr != device->pipelines.end())
		return itr->second.cache;

	VkGraphicsPipelineCreateInfo pipelineInfo;
	memset(&pipelineInfo, 0, sizeof(pipelineInfo));
	pipelineInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;

	// We set the viewport and scissor dynamically.
	static const VkDynamicState dynamicStates[] = {
		VK_DYNAMIC_STATE_VIEWPORT,
		VK_DYNAMIC_STATE_SCISSOR,
		VK_DYNAMIC_STATE_BLEND_CONSTANTS,
		//VK_DYNAMIC_STATE_DEPTH_BIAS,
		//VK_DYNAMIC_STATE_DEPTH_BOUNDS,
	};
	VkPipelineDynamicStateCreateInfo dynamicState;
	memset(&dynamicState, 0, sizeof(dynamicState));
	dynamicState.sType = VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO;
	dynamicState.dynamicStateCount = 3;
	dynamicState.pDynamicStates = dynamicStates;
	pipelineInfo.pDynamicState = &dynamicState;


	const auto& pstate = device->pipelineState;

	// NOTE: These don't matter as we set them dynamically.
	VkPipelineViewportStateCreateInfo viewport;
	memset(&viewport, 0, sizeof(viewport));
	viewport.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;
	viewport.viewportCount = 1;
	viewport.pViewports = &device->viewport;
	viewport.scissorCount = 1;
	viewport.pScissors = &device->scissor;
	pipelineInfo.pViewportState = &viewport;

	VkPipelineShaderStageCreateInfo stages[2];
	memset(stages, 0, sizeof(stages));
	stages[0].sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
	stages[0].stage = VK_SHADER_STAGE_VERTEX_BIT;
	stages[0].module = pstate.program->vertex->module;
	stages[0].pName = "main";
	stages[1].sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
	stages[1].stage = VK_SHADER_STAGE_FRAGMENT_BIT;
	stages[1].module = pstate.program->pixel->module;
	stages[1].pName = "main";
	pipelineInfo.stageCount = 2;
	pipelineInfo.pStages = stages;
	pipelineInfo.layout = pstate.program->layout;

	VkPipelineVertexInputStateCreateInfo vertexInputInfo;
	memset(&vertexInputInfo, 0, sizeof(vertexInputInfo));
	vertexInputInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
	vertexInputInfo.vertexBindingDescriptionCount = pstate.layout->streamCount;
	vertexInputInfo.pVertexBindingDescriptions = pstate.layout->bindings;
	vertexInputInfo.vertexAttributeDescriptionCount = pstate.layout->attributeCount;
	vertexInputInfo.pVertexAttributeDescriptions = pstate.layout->attributes;
	pipelineInfo.pVertexInputState = &vertexInputInfo;


	VkPipelineInputAssemblyStateCreateInfo inputAssembly;
	memset(&inputAssembly, 0, sizeof(inputAssembly));
	inputAssembly.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
	inputAssembly.topology = pstate.topology;
	pipelineInfo.pInputAssemblyState = &inputAssembly;

	// Setup the blend, rasterizer, and depth/stencil states.
	VkPipelineColorBlendStateCreateInfo colorBlending = pstate.blendState->info;
	colorBlending.attachmentCount = pstate.targets->set.numTargets;
	pipelineInfo.pColorBlendState = &colorBlending;
	pipelineInfo.pRasterizationState = &pstate.rasterizerState->info;
	pipelineInfo.pDepthStencilState = &pstate.depthStencilState->info;

	// TODO: What state is this part of?
	VkPipelineMultisampleStateCreateInfo multisampling;
	memset(&multisampling, 0, sizeof(multisampling));
	multisampling.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
	multisampling.sampleShadingEnable = VK_FALSE;
	multisampling.rasterizationSamples = pstate.rasterizerState->multiSampleAntiAlias ? VK_SAMPLE_COUNT_1_BIT : VK_SAMPLE_COUNT_1_BIT;
	multisampling.minSampleShading = 1.0f; // Optional
	multisampling.pSampleMask = nullptr; // Optional
	multisampling.alphaToCoverageEnable = VK_FALSE; // Optional
	multisampling.alphaToOneEnable = VK_FALSE; // Optional
	pipelineInfo.pMultisampleState = &multisampling;
	pipelineInfo.renderPass = pstate.targets->renderPass;

	pipelineInfo.subpass = 0;

	MGVK_PipelineCache pipeline;
	pipeline.state = device->pipelineState;

	VkResult res = vkCreateGraphicsPipelines(device->device, device->pipelineCache, 1, &pipelineInfo, nullptr, &pipeline.cache);
	VK_CHECK_RESULT(res);

	device->pipelines[hash] = pipeline;

	return pipeline.cache;
}

static void MGVK_UpdatePipeline(MGG_GraphicsDevice* device, MGVK_CmdBuffer& cmd, FrameCounter currentFrame)
{
	assert(device);

	// Make sure we know when we last used these states.
	device->pipelineState.blendState->frame = currentFrame;
	device->pipelineState.depthStencilState->frame = currentFrame;
	device->pipelineState.rasterizerState->frame = currentFrame;

	// Update the shader program.
	if (device->shaderDirty)
	{
		auto vertexShader = device->shaders[(mgint)MGShaderStage::Vertex];
		auto pixelShader = device->shaders[(mgint)MGShaderStage::Pixel];
		assert(vertexShader != nullptr);
		assert(pixelShader != nullptr);

		auto program = MGVK_ProgramGetOrCreate(device, vertexShader, pixelShader);

		if (program != device->pipelineState.program)
		{
			device->pipelineState.program = program;
			device->pipelineStateDirty = true;
		}

		device->shaderDirty = false;
	}

	// First update the pipeline if we need to.
	if (device->pipelineStateDirty)
	{
		device->pipelineStateDirty = false;

		auto pipeline = MGVK_CreatePipeline(device);
		vkCmdBindPipeline(cmd.buffer, VK_PIPELINE_BIND_POINT_GRAPHICS, pipeline);

		// Since the pipeline changed we need to update other states.
		device->scissorDirty = true;
		device->uniformsDirty = 0xFFFFFFFF;
		device->textureSamplerDirty = 0xFFFFFFFF;
		device->descriptorSets[0] = nullptr;
		device->descriptorSets[1] = nullptr;
		device->dynamicOffsets[0] = 0;
		device->dynamicOffsets[1] = 0;
		device->vertexBuffersDirty = 0xFFFFFFFF;
		device->blendFactorDirty = true;

		// We need to re-apply the viewport too.
		vkCmdSetViewport(cmd.buffer, 0, 1, &device->viewport);
	}

	if (device->scissorDirty)
	{
		device->scissorDirty = false;

		// If the scissor state is enabled then set the scissor rectable.
		auto rasterizerState = device->pipelineState.rasterizerState;
		if (rasterizerState->scissorTestEnable)
			vkCmdSetScissor(cmd.buffer, 0, 1, &device->scissor);
		else
		{
			VkRect2D scissor;
			scissor.offset.x = 0;
			scissor.offset.y = 0;

			// When the scissor test is disabled we need to set the
			// scissor rectangle to the full size of the current target.
			auto targets = device->pipelineState.targets;
			scissor.extent.width = targets->width;
			scissor.extent.height = targets->height;

			vkCmdSetScissor(cmd.buffer, 0, 1, &scissor);
		}
	}

	if (device->blendFactorDirty)
	{
		vkCmdSetBlendConstants(cmd.buffer, device->blendFactor);
		device->blendFactorDirty = false;
	}

	// Do we need to update the descriptors?
	if (device->uniformsDirty || device->textureSamplerDirty)
	{
		auto program = device->pipelineState.program;

		// Update the shader bindings.
		int setsCount = 0;
		int offsetCount = 0;
		if (program->vertex->setLayout)
		{
			uint32_t* dynamicOffset = nullptr;
			if (program->vertex->uniformSlots)
				dynamicOffset = &device->dynamicOffsets[offsetCount++];

			MGVK_UpdateDescriptors(device, currentFrame, program->vertex, &device->descriptorSets[setsCount++], dynamicOffset);
		}
		if (program->pixel->setLayout)
		{
			uint32_t* dynamicOffset = nullptr;
			if (program->pixel->uniformSlots)
				dynamicOffset = &device->dynamicOffsets[offsetCount++];

			MGVK_UpdateDescriptors(device, currentFrame, program->pixel, &device->descriptorSets[setsCount++], dynamicOffset);
		}

		// Bind the new descriptor sets.		
		vkCmdBindDescriptorSets(cmd.buffer, VK_PIPELINE_BIND_POINT_GRAPHICS,
			program->layout, 0,
			setsCount, device->descriptorSets,
			offsetCount, device->dynamicOffsets);

		// Clear all the dirty flags.
		device->uniformsDirty = 0;
		device->textureSamplerDirty = 0;
	}

	if (device->vertexBuffersDirty)
	{
		// TODO: Fix multiple vertex streams!

		int count = 0;
		VkDeviceSize offsets[8];
		VkBuffer buffers[8];
		for (int i = 0; i < 8; i++)
		{
			offsets[i] = device->vertexOffsets[i];
			buffers[i] = nullptr;

			auto buffer = device->vertexBuffers[i];
			if (buffer)
			{
				buffer->frame = currentFrame;
				buffers[i] = buffer->buffer;
				count++;
			}
		}

		vkCmdBindVertexBuffers(cmd.buffer, 0, count, buffers, offsets);

		device->vertexBuffersDirty = 0;
	}
}

static int MGVK_GetIndexCount(MGPrimitiveType primitiveType, mgint primitiveCount)
{
	switch (primitiveType)
	{
	case MGPrimitiveType::LineList:
		return primitiveCount * 2;
	case MGPrimitiveType::LineStrip:
		return primitiveCount + 1;
	case MGPrimitiveType::TriangleList:
		return primitiveCount * 3;
	case MGPrimitiveType::TriangleStrip:
		return primitiveCount + 2;
	default:
	case MGPrimitiveType::PointList:
		return primitiveCount;
	}
}

void MGG_GraphicsDevice_Draw(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint vertexStart, mgint vertexCount)
{
	assert(device != nullptr);
	assert(vertexStart >= 0);
	
	if (vertexCount <= 0)
		return;

	auto currentFrame = device->frame;
	auto frameIndex = currentFrame % kConcurrentFrameCount;
	auto& frame = device->frames[frameIndex];
	assert(frame.is_recording);

	MGVK_UpdateRenderPass(device, currentFrame, frame.commandBuffer);

	auto topology = ToVkPrimitiveTopology(primitiveType);
	if (device->pipelineState.topology != topology)
	{
		device->pipelineStateDirty = true;
		device->pipelineState.topology = topology;
	}

	MGVK_UpdatePipeline(device, frame.commandBuffer, currentFrame);

	vkCmdDraw(frame.commandBuffer.buffer, vertexCount, 1, vertexStart, 0);
}

void MGG_GraphicsDevice_DrawIndexed(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart)
{
	assert(device != nullptr);
	assert(primitiveCount >= 0);
	assert(indexStart >= 0);
	assert(vertexStart >= 0);

	if (primitiveCount <= 0)
		return;

	auto currentFrame = device->frame;
	auto frameIndex = currentFrame % kConcurrentFrameCount;
	auto& frame = device->frames[frameIndex];
	assert(frame.is_recording);

	MGVK_UpdateRenderPass(device, currentFrame, frame.commandBuffer);

	auto topology = ToVkPrimitiveTopology(primitiveType);
	if (device->pipelineState.topology != topology)
	{
		device->pipelineStateDirty = true;
		device->pipelineState.topology = topology;
	}

	MGVK_UpdatePipeline(device, frame.commandBuffer, currentFrame);

	auto indexBuffer = device->indexBuffer;
	assert(indexBuffer != nullptr);

	// TODO: Detect if we need to rebind the same index buffer?
	vkCmdBindIndexBuffer(frame.commandBuffer.buffer, indexBuffer->buffer, 0, device->indexBufferSize == MGIndexElementSize::SixteenBits ? VK_INDEX_TYPE_UINT16 : VK_INDEX_TYPE_UINT32);

	auto indexCount = MGVK_GetIndexCount(primitiveType, primitiveCount);

	vkCmdDrawIndexed(frame.commandBuffer.buffer, indexCount, 1, indexStart, vertexStart, 0);
}

void MGG_GraphicsDevice_DrawIndexedInstanced(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart, mgint instanceCount)
{
	assert(device != nullptr);
	assert(primitiveCount >= 0);
	assert(indexStart >= 0);
	assert(vertexStart >= 0);
	assert(instanceCount >= 0);

	if (primitiveCount <= 0)
		return;
	if (instanceCount <= 0)
		return;

	MG_NOT_IMPLEMEMTED;
}


static VkBlendFactor ToVkBlendFactor(MGBlend mode)
{
	switch (mode)
	{
	case MGBlend::One:
		return VK_BLEND_FACTOR_ONE;
	case MGBlend::Zero:
		return VK_BLEND_FACTOR_ZERO;
	case MGBlend::SourceColor:
		return VK_BLEND_FACTOR_SRC_COLOR;
	case MGBlend::InverseSourceColor:
		return VK_BLEND_FACTOR_ONE_MINUS_SRC_COLOR;
	case MGBlend::SourceAlpha:
		return VK_BLEND_FACTOR_SRC_ALPHA;
	case MGBlend::InverseSourceAlpha:
		return VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA;
	case MGBlend::DestinationColor:
		return VK_BLEND_FACTOR_DST_COLOR;
	case MGBlend::InverseDestinationColor:
		return VK_BLEND_FACTOR_ONE_MINUS_DST_COLOR;
	case MGBlend::DestinationAlpha:
		return VK_BLEND_FACTOR_DST_ALPHA;
	case MGBlend::InverseDestinationAlpha:
		return VK_BLEND_FACTOR_ONE_MINUS_DST_ALPHA;
	case MGBlend::BlendFactor:
		return VK_BLEND_FACTOR_CONSTANT_COLOR;
	case MGBlend::InverseBlendFactor:
		return VK_BLEND_FACTOR_ONE_MINUS_CONSTANT_COLOR;
	case MGBlend::SourceAlphaSaturation:
		return VK_BLEND_FACTOR_SRC_ALPHA_SATURATE;
	default:
		assert(0);
	}
}

static VkBlendOp ToVkBlendOp(MGBlendFunction func)
{
	switch (func)
	{
	case MGBlendFunction::Add:
		return VK_BLEND_OP_ADD;
	case MGBlendFunction::Subtract:
		return VK_BLEND_OP_SUBTRACT;
	case MGBlendFunction::ReverseSubtract:
		return VK_BLEND_OP_REVERSE_SUBTRACT;
	case MGBlendFunction::Min:
		return VK_BLEND_OP_MIN;
	case MGBlendFunction::Max:
		return VK_BLEND_OP_MAX;
	default:
		assert(0);
	}
}

static void MGVK_SetBlendState(MGG_BlendState_Info* info, VkPipelineColorBlendAttachmentState* blend)
{
	if ((int)info->colorWriteChannels & (int)MGColorWriteChannels::Red)
		blend->colorWriteMask |= VK_COLOR_COMPONENT_R_BIT;
	if ((int)info->colorWriteChannels & (int)MGColorWriteChannels::Green)
		blend->colorWriteMask |= VK_COLOR_COMPONENT_G_BIT;
	if ((int)info->colorWriteChannels & (int)MGColorWriteChannels::Blue)
		blend->colorWriteMask |= VK_COLOR_COMPONENT_B_BIT;
	if ((int)info->colorWriteChannels & (int)MGColorWriteChannels::Alpha)
		blend->colorWriteMask |= VK_COLOR_COMPONENT_A_BIT;

	blend->blendEnable = !(info->colorSourceBlend == MGBlend::One &&
		info->colorDestBlend == MGBlend::Zero &&
		info->alphaSourceBlend == MGBlend::One &&
		info->alphaDestBlend == MGBlend::Zero);

	blend->srcColorBlendFactor = ToVkBlendFactor(info->colorSourceBlend);
	blend->dstColorBlendFactor = ToVkBlendFactor(info->colorDestBlend);
	blend->colorBlendOp = ToVkBlendOp(info->colorBlendFunc);
	blend->srcAlphaBlendFactor = ToVkBlendFactor(info->alphaSourceBlend);
	blend->dstAlphaBlendFactor = ToVkBlendFactor(info->alphaDestBlend);
	blend->alphaBlendOp = ToVkBlendOp(info->alphaBlendFunc);
}

MGG_BlendState* MGG_BlendState_Create(MGG_GraphicsDevice* device, MGG_BlendState_Info* infos)
{
	assert(device != nullptr);
	assert(infos != nullptr);

	// First check the cache.
	uint32_t hash = MG_ComputeHash((mgbyte*)infos, sizeof(MGG_BlendState_Info) * 4);
	MGG_BlendState* cached = device->blendStates[hash];
	if (cached)
	{
		cached->refs++;
		return cached;
	}

	auto state = new MGG_BlendState();
	memset(state, 0, sizeof(MGG_BlendState));

	auto attachments = state->attachments;
	MGVK_SetBlendState(infos + 0, attachments + 0);
	MGVK_SetBlendState(infos + 1, attachments + 1);
	MGVK_SetBlendState(infos + 2, attachments + 2);
	MGVK_SetBlendState(infos + 3, attachments + 3);

	auto& blend = state->info;
	blend.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
	blend.logicOpEnable = VK_FALSE;
	blend.logicOp = VK_LOGIC_OP_COPY;
	blend.flags = 0; // Reserved for future use.
	blend.pAttachments = state->attachments;
	blend.attachmentCount = 4;

	state->refs = 1;
	state->hash = hash;

	device->blendStates[hash] = state;

	return state;

}

void MGG_BlendState_Destroy(MGG_GraphicsDevice* device, MGG_BlendState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;

	// Release the reference.
	state->refs--;
	if (state->refs > 0)
		return;

	// Queue the state for later destruction.
	device->destroyBlendStates.push(state);
	device->blendStates.erase(state->hash);
}

static VkCompareOp ToVkCompareOp(MGCompareFunction func)
{
	switch (func)
	{
	case MGCompareFunction::Always:
		return VK_COMPARE_OP_ALWAYS;
	case MGCompareFunction::Never:
		return VK_COMPARE_OP_NEVER;
	case MGCompareFunction::Less:
		return VK_COMPARE_OP_LESS;
	case MGCompareFunction::LessEqual:
		return VK_COMPARE_OP_LESS_OR_EQUAL;
	case MGCompareFunction::Equal:
		return VK_COMPARE_OP_EQUAL;
	case MGCompareFunction::GreaterEqual:
		return VK_COMPARE_OP_GREATER_OR_EQUAL;
	case MGCompareFunction::Greater:
		return VK_COMPARE_OP_GREATER;
	case MGCompareFunction::NotEqual:
		return VK_COMPARE_OP_NOT_EQUAL;
	default:
		assert(0);
	}
}

static VkStencilOp ToVkStencilOp(MGStencilOperation op)
{
	switch (op)
	{
	case MGStencilOperation::Keep:
		return VK_STENCIL_OP_KEEP;
	case MGStencilOperation::Zero:
		return VK_STENCIL_OP_ZERO;
	case MGStencilOperation::Replace:
		return VK_STENCIL_OP_REPLACE;
	case MGStencilOperation::Increment:
		return VK_STENCIL_OP_INCREMENT_AND_WRAP;
	case MGStencilOperation::Decrement:
		return VK_STENCIL_OP_DECREMENT_AND_WRAP;
	case MGStencilOperation::IncrementSaturation:
		return VK_STENCIL_OP_INCREMENT_AND_CLAMP;
	case MGStencilOperation::DecrementSaturation:
		return VK_STENCIL_OP_DECREMENT_AND_CLAMP;
	case MGStencilOperation::Invert:
		return VK_STENCIL_OP_INVERT;
	default:
		assert(0);
	}
}

MGG_DepthStencilState* MGG_DepthStencilState_Create(MGG_GraphicsDevice* device, MGG_DepthStencilState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

	// First check the cache.
	uint32_t hash = MG_ComputeHash((mgbyte*)info, sizeof(MGG_DepthStencilState_Info));
	MGG_DepthStencilState* cached = device->depthStencilStates[hash];
	if (cached)
	{
		cached->refs++;
		return cached;
	}

	auto state = new MGG_DepthStencilState();
	memset(state, 0, sizeof(MGG_DepthStencilState));

	auto& depth = state->info;
	depth.sType = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO;
	depth.flags = 0; // Reserved for future use.
	depth.depthTestEnable = info->depthBufferEnable;
	depth.depthWriteEnable = info->depthBufferWriteEnable;
	depth.depthCompareOp = ToVkCompareOp(info->depthBufferFunction);
	depth.depthBoundsTestEnable = VK_FALSE; // VK_TRUE;
	depth.stencilTestEnable = info->stencilEnable;
	depth.front.failOp = ToVkStencilOp(info->stencilFail);
	depth.front.passOp = ToVkStencilOp(info->stencilPass);
	depth.front.depthFailOp = ToVkStencilOp(info->stencilDepthBufferFail);
	depth.front.compareOp = ToVkCompareOp(info->stencilFunction);
	depth.front.compareMask = info->stencilMask;
	depth.front.writeMask = info->stencilWriteMask;
	depth.front.reference = info->referenceStencil;
	depth.back = depth.front;
	depth.minDepthBounds = 0.0f;
	depth.maxDepthBounds = 1.0f;

	state->refs = 1;
	state->hash = hash;
	device->depthStencilStates[hash] = state;

	return state;
}

void MGG_DepthStencilState_Destroy(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;

	// Release the reference.
	state->refs--;
	if (state->refs > 0)
		return;

	// Queue the state for later destruction.
	device->destroyDepthStencilStates.push(state);
	device->depthStencilStates.erase(state->hash);
}

static VkPolygonMode ToVkPolygonMode(MGFillMode mode)
{
	switch (mode)
	{
	case MGFillMode::Solid:
		return VK_POLYGON_MODE_FILL;
	case MGFillMode::WireFrame:
		return VK_POLYGON_MODE_LINE;
	default:
		assert(0);
	}
}

static VkCullModeFlags ToVkCullModeFlags(MGCullMode mode)
{
	switch (mode)
	{
	case MGCullMode::None:
		return VK_CULL_MODE_NONE;
	case MGCullMode::CullClockwiseFace:
		return VK_CULL_MODE_FRONT_BIT;
	case MGCullMode::CullCounterClockwiseFace:
		return VK_CULL_MODE_BACK_BIT;
	default:
		assert(0);
	}
}


MGG_RasterizerState* MGG_RasterizerState_Create(MGG_GraphicsDevice* device, MGG_RasterizerState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

	// First check the cache.
	uint32_t hash = MG_ComputeHash((mgbyte*)info, sizeof(MGG_RasterizerState_Info));
	MGG_RasterizerState* cached = device->rasterizerStates[hash];
	if (cached)
	{
		cached->refs++;
		return cached;
	}

	auto state = new MGG_RasterizerState();
	memset(state, 0, sizeof(MGG_RasterizerState));

	auto& rasterizer = state->info;
	rasterizer.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
	rasterizer.flags = 0; // Reserved for future use.
	rasterizer.depthClampEnable = VK_FALSE; // info->depthClipEnable;
	rasterizer.rasterizerDiscardEnable = VK_FALSE;
	rasterizer.polygonMode = ToVkPolygonMode(info->fillMode);
	rasterizer.cullMode = ToVkCullModeFlags(info->cullMode);
	rasterizer.frontFace = VK_FRONT_FACE_CLOCKWISE;
	rasterizer.depthBiasEnable = info->depthBias != 0;
	rasterizer.depthBiasConstantFactor = info->depthBias;
	rasterizer.depthBiasClamp = 0.0f;
	rasterizer.depthBiasSlopeFactor = info->slopeScaleDepthBias;
	rasterizer.lineWidth = 1.0f;

	// These don't fit this structure, so we
	// need to hold them for later.
	state->scissorTestEnable = info->scissorTestEnable;
	state->multiSampleAntiAlias = info->multiSampleAntiAlias;

	state->refs = 1;
	state->hash = hash;
	device->rasterizerStates[hash] = state;

	return state;
}

void MGG_RasterizerState_Destroy(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;

	// Release the reference.
	state->refs--;
	if (state->refs > 0)
		return;

	// Queue the state for later destruction.
	device->destroyRasterizerStates.push(state);
	device->rasterizerStates.erase(state->hash);
}

static VkSamplerAddressMode ToVkSamplerAddressMode(MGTextureAddressMode mode)
{
	switch (mode)
	{
	case MGTextureAddressMode::Wrap:
		return VK_SAMPLER_ADDRESS_MODE_REPEAT;
	case MGTextureAddressMode::Clamp:
		return VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_EDGE;
	case MGTextureAddressMode::Mirror:
		return VK_SAMPLER_ADDRESS_MODE_MIRRORED_REPEAT;
	case MGTextureAddressMode::Border:
		return VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_BORDER;
	default:
		assert(0);
	}
}

MGG_SamplerState* MGG_SamplerState_Create(MGG_GraphicsDevice* device, MGG_SamplerState_Info* info)
{
	assert(device != nullptr);
	assert(info != nullptr);

	// TODO: Why isn't this reference counted?

	auto state = new MGG_SamplerState();
	state->info = *info; // For debugging

	VkSamplerCreateInfo samplerInfo = { VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO };
	samplerInfo.addressModeU = ToVkSamplerAddressMode(info->AddressU);
	samplerInfo.addressModeV = ToVkSamplerAddressMode(info->AddressV);
	samplerInfo.addressModeW = ToVkSamplerAddressMode(info->AddressW);
	samplerInfo.anisotropyEnable = info->Filter == MGTextureFilter::Anisotropic;
	samplerInfo.maxAnisotropy = info->MaximumAnisotropy;
	samplerInfo.unnormalizedCoordinates = VK_FALSE;
	samplerInfo.compareEnable = VK_FALSE;
	samplerInfo.compareOp = VK_COMPARE_OP_NEVER;
	samplerInfo.mipLodBias = 0.0f; // ?? info->MipMapLevelOfDetailBias
	samplerInfo.minLod = 0; // ??? info->MaxMipLevel
	samplerInfo.maxLod = VK_LOD_CLAMP_NONE;

	VkSamplerCustomBorderColorCreateInfoEXT bcolor = {};

	// TODO: What about the FLOAT color cases?
	if (info->BorderColor == 0x00000000)
		samplerInfo.borderColor = VK_BORDER_COLOR_FLOAT_TRANSPARENT_BLACK;
	else if (info->BorderColor == 0xFF000000)
		samplerInfo.borderColor = VK_BORDER_COLOR_FLOAT_OPAQUE_BLACK;
	else if (info->BorderColor == 0xFFFFFFFF)
		samplerInfo.borderColor = VK_BORDER_COLOR_FLOAT_OPAQUE_WHITE;
	else
	{
		samplerInfo.borderColor = VK_BORDER_COLOR_FLOAT_CUSTOM_EXT;
		bcolor.sType = VK_STRUCTURE_TYPE_SAMPLER_CUSTOM_BORDER_COLOR_CREATE_INFO_EXT;

		// RGBA
		bcolor.format = VK_FORMAT_UNDEFINED;
		bcolor.customBorderColor.float32[0] = ((info->BorderColor >> 0) & 0xFF) / 255.0f;
		bcolor.customBorderColor.float32[1] = ((info->BorderColor >> 8) & 0xFF) / 255.0f;
		bcolor.customBorderColor.float32[2] = ((info->BorderColor >> 16) & 0xFF) / 255.0f;
		bcolor.customBorderColor.float32[3] = ((info->BorderColor >> 24) & 0xFF) / 255.0f;

		samplerInfo.pNext = &bcolor;
	}

	switch (info->Filter)
	{
	case MGTextureFilter::Anisotropic:
	case MGTextureFilter::Linear:
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		break;
	case MGTextureFilter::LinearMipPoint:
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		break;
	case MGTextureFilter::Point:
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		break;
	case MGTextureFilter::PointMipLinear:
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		break;
	case MGTextureFilter::MinLinearMagPointMipLinear:
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		break;
	case MGTextureFilter::MinLinearMagPointMipPoint:
		samplerInfo.minFilter = VK_FILTER_LINEAR;
		samplerInfo.magFilter = VK_FILTER_NEAREST;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		break;
	case MGTextureFilter::MinPointMagLinearMipLinear:
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
		break;
	case MGTextureFilter::MinPointMagLinearMipPoint:
		samplerInfo.minFilter = VK_FILTER_NEAREST;
		samplerInfo.magFilter = VK_FILTER_LINEAR;
		samplerInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		break;
	default:
		assert(0);
	}

	VkResult res = vkCreateSampler(device->device, &samplerInfo, nullptr, &state->sampler);
	VK_CHECK_RESULT(res);

	state->id = ++device->currentSamplerId;

	return state;
}

void MGG_SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state)
{
	assert(device != nullptr);
	assert(state != nullptr);

	if (!state)
		return;

	vkDestroySampler(device->device, state->sampler, nullptr);

	delete state;
}

static MGG_Buffer* MGVK_BufferDiscard(MGG_GraphicsDevice* device, MGG_Buffer* buffer)
{
	// Get the info we need to find/allocate a new buffer.
	auto dataSize = buffer->dataSize;
	auto type = buffer->type;

	// Discard the current buffer.
	assert(buffer->next == nullptr);
	buffer->next = device->discarded;
	device->discarded = buffer;
	buffer = nullptr;

	// Search for the best fit from the free list.	
	MGG_Buffer** curr = &device->free;
	MGG_Buffer** best = nullptr;
	while (*curr != nullptr)
	{
		if ((*curr)->type == type)
		{
			auto currSize = (*curr)->actualSize;

			if (currSize >= dataSize)
			{
				if (best == nullptr || (*best)->actualSize > currSize)
				{
					best = curr;
					if (currSize == dataSize)
						break;
				}
			}
		}

		curr = &(*curr)->next;
	}

	// Did we find a match?
	if (best != nullptr)
	{
		buffer = *best;
		*best = buffer->next;
		buffer->next = nullptr;
		buffer->dataSize = dataSize;
		//buffer->dirtyMin = 0;
		//buffer->dirtyMax = 0;
		return buffer;
	}

	// We didn't find a match, so allocate a new one.
	buffer = MGG_Buffer_Create(device, type, dataSize);

	return buffer;
}

static MGG_Buffer* MGVK_Buffer_Create(MGG_GraphicsDevice* device, MGBufferType type, mgint sizeInBytes, bool no_push)
{
	assert(device != nullptr);

	auto buffer = new MGG_Buffer();
	memset(buffer, 0, sizeof(MGG_Buffer));

	buffer->actualSize = buffer->dataSize = sizeInBytes;
	buffer->type = type;
	
	if (type == MGBufferType::Constant && !no_push)
	{
		// Uniform buffers never use real buffers.  We just
		// keep the data in system memory until it is time to render.
		buffer->push = new mgbyte[sizeInBytes];
	}
	else
	{
		VkBufferUsageFlags usage;
		switch (type)
		{
		case MGBufferType::Index:
			usage = VK_BUFFER_USAGE_INDEX_BUFFER_BIT;
			break;
		case MGBufferType::Vertex:
			usage = VK_BUFFER_USAGE_VERTEX_BUFFER_BIT;
			break;
		case MGBufferType::Constant:
			usage = VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;
			break;
		default:
			assert(0);
		}

		MGVK_BufferCreate(device, sizeInBytes, usage, VMA_MEMORY_USAGE_CPU_TO_GPU, buffer);

		// TODO: We could lazy map on first write/read... then
		// unmap it if we haven't accessed it in a while.  This
		// should reduce the CPU memory we have in use for
		// generally static buffers.  Maybe the dynamic buffer
		// hint would be useful for this?

		// Mapping buffers takes a good amount of time, so
		// just keep the buffer mapped the whole time and
		// use flush/invalidate to syncronize as needed.
		auto res = vmaMapMemory(device->allocator, buffer->allocation, (void**)&buffer->mapped);
		VK_CHECK_RESULT(res);
	}

	device->all_buffers.push_back(buffer);

	return buffer;
}

MGG_Buffer* MGG_Buffer_Create(MGG_GraphicsDevice* device, MGBufferType type, mgint sizeInBytes)
{
	return MGVK_Buffer_Create(device, type, sizeInBytes, false);
}

static void MGVK_BufferCopyAndFlush(MGG_GraphicsDevice* device, MGG_Buffer* buffer, int destOffset, mgbyte* data, int dataBytes)
{
	assert(device);
	assert(buffer);
	assert(destOffset < buffer->dataSize);
	assert(destOffset + dataBytes <= buffer->dataSize);

	memcpy(buffer->mapped + destOffset, data, dataBytes);

	buffer->dirty = false;

	/*
	// TODO: Store ranges and all buffers used in a frame
	// so we can flush them before vkSubmit.
	auto nonCoherentAtomSize = device->deviceProperties.limits.nonCoherentAtomSize;

	uint32_t alignedOffset = destOffset;
	if ((alignedOffset % nonCoherentAtomSize) != 0)
		alignedOffset -= alignedOffset % nonCoherentAtomSize;

	uint32_t alignedEnd = destOffset + dataBytes;
	if ((alignedEnd % nonCoherentAtomSize) != 0)
	{
		alignedEnd += nonCoherentAtomSize - (alignedEnd % nonCoherentAtomSize);
		if (alignedEnd >= buffer->dataSize)
			alignedEnd = buffer->dataSize;
	}

	assert(alignedOffset < buffer->dataSize);
	assert(alignedEnd <= buffer->dataSize);

	uint32_t alignedSize = alignedEnd - alignedOffset;

	VkMappedMemoryRange range = { VK_STRUCTURE_TYPE_MAPPED_MEMORY_RANGE };
	range.memory = buffer->memory;
	range.offset = alignedOffset;
	range.size = alignedSize;
	VkResult res = vkFlushMappedMemoryRanges(device->device, 1, &range);
	VK_CHECK_RESULT(res);
	*/
}

void MGG_Buffer_Destroy(MGG_GraphicsDevice* device, MGG_Buffer* buffer)
{
	assert(device != nullptr);
	assert(buffer != nullptr);

	if (!buffer)
		return;

	mg_remove(device->all_buffers, buffer);

	// Push buffers can be freed immediately.
	if (buffer->push)
	{
		delete[] buffer->push;
		delete buffer;
		return;
	}

	// Safe to unmap it now.
	if (buffer->mapped)
		vmaUnmapMemory(device->allocator, buffer->allocation);

	// Queue the buffer for later destruction.
	device->destroyBuffers.push(buffer);
}

void MGG_Buffer_SetData(MGG_GraphicsDevice* device, MGG_Buffer*& buffer, mgint offset, mgbyte* data, mgint length, mgbool discard)
{
	assert(device != nullptr);
	assert(buffer != nullptr);
	assert(data != nullptr);

	buffer->dirty = true;

	// If this is a push buffer we don't need to
	// do anything other than copy over the content.
	// We can safely ignore the discard.
	if (buffer->push)
	{
		memcpy(buffer->push + offset, data, length);
		return;
	}

	// TODO: Force discard here if we find we're
	// copying over data still in use.  See NX.

	if (discard)
	{
		auto last = buffer;

		buffer = MGVK_BufferDiscard(device, buffer);

		// Fix any active mapping of the buffer that
		// was just discarded for another.

		switch (buffer->type)
		{
		case MGBufferType::Constant:
			for (int i=0; i < (int)MGShaderStage::Count; i++)
			{
				if (device->uniforms[i] == last)
				{
					device->uniforms[i] = buffer;
					device->uniformsDirty |= 1 << (int)i;
				}		
			}
			break;

		case MGBufferType::Vertex:
			for (int i = 0; i < 8; i++)
			{
				if (device->vertexBuffers[i] == last)
				{
					device->vertexBuffers[i] = buffer;
					device->vertexBuffersDirty |= 1 << i;
				}
			}
			break;

		case MGBufferType::Index:
			if (device->indexBuffer == last)
				device->indexBuffer = buffer;
			break;
		}
	}

	// Do the copy and flush.
	MGVK_BufferCopyAndFlush(device, buffer, offset, data, length);
}

void MGG_Buffer_GetData(MGG_GraphicsDevice* device, MGG_Buffer* buffer, mgint offset, mgbyte* data, mgint dataCount, mgint dataBytes, mgint dataStride)
{
	assert(device != nullptr);
	assert(buffer != nullptr);
	assert(data != nullptr);

	//assert(offset > 0 && offset < buffer->length);

	assert(dataCount > 0);
	assert(dataBytes > 0);
	assert(dataStride > 0);

	if (buffer->push)
		memcpy(data, buffer->push + offset, dataCount * dataBytes);
	else
		memcpy(data, buffer->mapped + offset, dataCount * dataBytes);
}

MGG_Texture* MGG_Texture_Create(
	MGG_GraphicsDevice* device,
	MGTextureType type,
	MGSurfaceFormat format,
	mgint width,
	mgint height,
	mgint depth,
	mgint mipmaps,
	mgint slices)
{
	assert(device != nullptr);

	assert(width > 0);
	assert(height > 0);
	assert(depth > 0);
	assert(mipmaps > 0);
	assert(slices > 0);
	assert(type != MGTextureType::Cube || (slices % 6) == 0);

	auto texture = new MGG_Texture();
	texture->type = type;
	texture->format = format;

	VkImageCreateInfo& create_info = texture->info;
	create_info.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
	create_info.imageType = VK_IMAGE_TYPE_2D; // TODO: 3D textures
	create_info.format = ToVkFormat(format);
	create_info.extent.width = width;
	create_info.extent.height = height;
	create_info.extent.depth = depth;
	create_info.mipLevels = mipmaps;
	create_info.arrayLayers = slices;
	create_info.samples = VK_SAMPLE_COUNT_1_BIT;
	create_info.tiling = VK_IMAGE_TILING_OPTIMAL;
	create_info.usage = VK_IMAGE_USAGE_SAMPLED_BIT | VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK_IMAGE_USAGE_TRANSFER_SRC_BIT;
	create_info.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
	create_info.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;

	texture->layout = VK_IMAGE_LAYOUT_UNDEFINED;
	texture->optimal_layout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
	
	mggCreateImage(device, &create_info, texture);

	texture->view = CreateImageView(device, texture, mipmaps);

	texture->id = ++device->currentTextureId;

	return texture;
}

MGG_Texture* MGG_RenderTarget_Create(
	MGG_GraphicsDevice* device,
	MGTextureType type,
	MGSurfaceFormat format,
	mgint width,
	mgint height,
	mgint depth,
	mgint mipmaps,
	mgint slices,
	MGDepthFormat depthFormat,
	mgint multiSampleCount,
	MGRenderTargetUsage usage)
{
	assert(device != nullptr);

	assert(width > 0);
	assert(height > 0);
	assert(depth > 0);
	assert(mipmaps > 0);
	assert(slices > 0);
	assert(type != MGTextureType::Cube || (slices % 6) == 0);

	auto texture = new MGG_Texture();
	texture->isTarget = true;
	texture->type = type;
	texture->format = format;

	texture->depthFormat = depthFormat;
	texture->multiSampleCount = multiSampleCount;
	texture->usage = usage;

	{
		VkImageCreateInfo& create_info = texture->info;
		create_info.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
		create_info.imageType = VK_IMAGE_TYPE_2D; // TODO: Fix type!
		create_info.format = ToVkFormat(format);
		create_info.extent.width = width;
		create_info.extent.height = height;
		create_info.extent.depth = depth;
		create_info.mipLevels = mipmaps;
		create_info.arrayLayers = slices;
		create_info.samples = VK_SAMPLE_COUNT_1_BIT;
		create_info.tiling = VK_IMAGE_TILING_OPTIMAL;
		create_info.usage = VK_IMAGE_USAGE_SAMPLED_BIT | VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT | VK_IMAGE_USAGE_TRANSFER_SRC_BIT;
		create_info.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
		create_info.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;

		texture->layout = VK_IMAGE_LAYOUT_UNDEFINED;
		texture->optimal_layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;


		mggCreateImage(device, &create_info, texture);

		texture->view = CreateImageView(device, texture, mipmaps);
		texture->target_view = CreateImageView(device, texture, 1);

		MGVK_TransitionImageLayout(device, texture, 0, texture->optimal_layout);
	}

	if (depthFormat != MGDepthFormat::None)
	{
		texture->depthTexture = CreateDepthTexture(device, ToVkFormat(depthFormat), width, height);
		texture->depthTexture->target_view = CreateImageView(device, texture->depthTexture, 1);
	}

	texture->id = ++device->currentTextureId;

	//device->all_textures.push_back(texture);

	return texture;
}

void MGG_Texture_Destroy(MGG_GraphicsDevice* device, MGG_Texture* texture)
{
	assert(device != nullptr);
	assert(texture != nullptr);

	if (!texture)
		return;

	if (texture->depthTexture)
		MGG_Texture_Destroy(device, texture->depthTexture);

	// Queue the texture for later destruction.
	device->destroyTextures.push(texture);

	//remove_by_value(device->all_textures, texture);
	//delete texture;
}

static void MGVK_TransitionImageLayout(MGG_GraphicsDevice* device, MGG_Texture* texture, int32_t level, VkImageLayout newLayout)
{
	VkCommandBuffer cmd = MGVK_BeginNewCommandBuffer(device);

	VkImageLayout oldLayout = texture->layout;

	VkImageMemoryBarrier barrier = {};
	barrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
	barrier.oldLayout = oldLayout;
	barrier.newLayout = newLayout;
	barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	barrier.image = texture->image;
	barrier.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT; // Fetch this from MGG_Texture!
	barrier.subresourceRange.baseMipLevel = level;
	barrier.subresourceRange.levelCount = 1;
	barrier.subresourceRange.baseArrayLayer = 0;
	barrier.subresourceRange.layerCount = 1;

	VkPipelineStageFlags sourceStage;
	VkPipelineStageFlags destinationStage;

	if (newLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
	{
		barrier.srcAccessMask = 0;
		barrier.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;

		sourceStage = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
		destinationStage = VK_PIPELINE_STAGE_TRANSFER_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
	{
		barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
		barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;

		sourceStage = VK_PIPELINE_STAGE_TRANSFER_BIT;
		destinationStage = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
	}
	else if (newLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
	{
		barrier.srcAccessMask = 0;
		barrier.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;

		sourceStage = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
		destinationStage = VK_PIPELINE_STAGE_TRANSFER_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
	{
		barrier.srcAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
		barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;

		sourceStage = VK_PIPELINE_STAGE_TRANSFER_BIT;
		destinationStage = VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;

		if (newLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
			barrier.dstAccessMask |= VK_ACCESS_SHADER_WRITE_BIT;
	}
	else if (newLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
	{
		barrier.srcAccessMask = 0;
		barrier.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;

		sourceStage = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
		destinationStage = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
	}
	else if (newLayout == VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)
	{
		barrier.subresourceRange.aspectMask = VK_IMAGE_ASPECT_DEPTH_BIT | VK_IMAGE_ASPECT_STENCIL_BIT;

		barrier.srcAccessMask = 0;
		barrier.dstAccessMask = VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT | VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;

		sourceStage = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
		destinationStage = VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;
	}
	else
	{
		// unsupported layout transition!
		assert(0);
	}

	vkCmdPipelineBarrier(
		cmd,
		sourceStage, destinationStage,
		0,
		0, nullptr,
		0, nullptr,
		1, &barrier
	);

	MGVK_ExecuteAndFreeCommandBuffer(device, cmd);

	texture->layout = barrier.newLayout;
}

void MGG_Texture_SetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
	assert(device != nullptr);
	assert(texture != nullptr);

	if (x == 0 && y == 0 && width == 0 && height == 0)
	{
		width = texture->info.extent.width;
		height = texture->info.extent.height;
	}

	assert(level >= 0 && level < texture->info.mipLevels);
	assert(slice >= 0 && slice < texture->info.arrayLayers);
	assert(x >= 0 && x < texture->info.extent.width);
	assert(y >= 0 && y < texture->info.extent.height);
	assert(z >= 0 && z < texture->info.extent.depth);
	assert(x + width <= texture->info.extent.width);
	assert(y + height <= texture->info.extent.height);
	assert(z + depth <= texture->info.extent.depth);

	assert(data != nullptr);
	assert(dataBytes > 0);

	// TODO: Pool staging buffers maybe?

	MGG_Buffer buffer;
	MGVK_BufferCreate(device, dataBytes, VK_BUFFER_USAGE_TRANSFER_SRC_BIT, VMA_MEMORY_USAGE_CPU_ONLY, &buffer);

	void* dest;
	vmaMapMemory(device->allocator, buffer.allocation, &dest);
	memcpy(dest, data, dataBytes);
	vmaUnmapMemory(device->allocator, buffer.allocation);

	MGVK_TransitionImageLayout(device, texture, level, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);

	MGVK_CopyBufferToImage(device, buffer.buffer, texture->image, x, y, level, width, height);

	MGVK_TransitionImageLayout(device, texture, level, texture->optimal_layout);

	vmaDestroyBuffer(device->allocator, buffer.buffer, buffer.allocation);
}

void MGG_Texture_GetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
	assert(device != nullptr);
	assert(texture != nullptr);

	assert(level >= 0 && level < texture->info.mipLevels);
	assert(slice >= 0 && slice < texture->info.arrayLayers);
	assert(x >= 0 && x < texture->info.extent.width);
	assert(y >= 0 && y < texture->info.extent.height);
	assert(z >= 0 && z < texture->info.extent.depth);
	assert(x + width <= texture->info.extent.width);
	assert(y + height <= texture->info.extent.height);
	assert(z + depth <= texture->info.extent.depth);

	assert(data != nullptr);
	assert(dataBytes > 0);

	bool restart_frame = false;

	const FrameCounter currentFrame = device->frame;
	const FrameCounter frameIndex = currentFrame % kConcurrentFrameCount;
	MGVK_FrameState& frame = device->frames[frameIndex];
	MGVK_CmdBuffer& cmd = frame.commandBuffer;

	if (texture->isTarget)
	{
		if (texture->frame == device->frame)
		{
			if (device->inRenderPass)
			{
				vkCmdEndRenderPass(cmd.buffer);
				device->inRenderPass = false;
			}

			VkResult res = vkEndCommandBuffer(cmd.buffer);
			VK_CHECK_RESULT(res);

			VkFence renderFence;
			{
				VkFenceCreateInfo fenceCreateInfo = {};
				fenceCreateInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
				fenceCreateInfo.flags = 0;
				vkCreateFence(device->device, &fenceCreateInfo, nullptr, &renderFence);
			}

			VkSubmitInfo submitInfo = {};
			submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
			submitInfo.commandBufferCount = 1;
			submitInfo.pCommandBuffers = &cmd.buffer;

			vkQueueSubmit(device->queue, 1, &submitInfo, renderFence);

			vkWaitForFences(device->device, 1, &renderFence, VK_TRUE, UINT64_MAX);

			vkDestroyFence(device->device, renderFence, nullptr);

			restart_frame = true;
		}
	}

	MGG_Buffer buffer;
	MGVK_BufferCreate(device, dataBytes, VK_BUFFER_USAGE_TRANSFER_DST_BIT, VMA_MEMORY_USAGE_CPU_ONLY, &buffer);
	MGVK_TransitionImageLayout(device, texture, level, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL);

	MGVK_CopyImageToBuffer(device, texture->image, buffer.buffer, x, y, level, width, height);

	MGVK_TransitionImageLayout(device, texture, level, texture->layout);

	void* src;
	vmaMapMemory(device->allocator, buffer.allocation, &src);
	memcpy(data, src, dataBytes);
	vmaUnmapMemory(device->allocator, buffer.allocation);

	vmaDestroyBuffer(device->allocator, buffer.buffer, buffer.allocation);

	if (restart_frame)
		MGVK_BeginFrame(cmd);
}

MGG_InputLayout* MGG_InputLayout_Create(
	MGG_GraphicsDevice* device,
	mgint* strides,
	mgint streamCount,
	MGG_InputElement* elements,
	mgint elementCount
	)
{
	assert(device != nullptr);
	assert(streamCount >= 0);
	assert(strides != nullptr);
	assert(elements != nullptr);
	assert(elementCount >= 0);

	auto layout = new MGG_InputLayout();

	layout->streamCount = streamCount;
	auto bindings = layout->bindings = new VkVertexInputBindingDescription[streamCount];
	for (int i = 0; i < streamCount; i++)
	{
		bindings[i].binding = i;
		bindings[i].stride = strides[i];
		bindings[i].inputRate = VK_VERTEX_INPUT_RATE_VERTEX; // Support instance rates.
	}

	layout->attributeCount = elementCount;
	auto attrs = layout->attributes = new VkVertexInputAttributeDescription[elementCount];
	for (int i = 0; i < elementCount; i++)
	{
		attrs[i].location = i;
		attrs[i].binding = elements[i].VertexBufferSlot;
		attrs[i].format = ToVkFormat(elements[i].Format);
		attrs[i].offset = elements[i].AlignedByteOffset;
	}

	return layout;
}

void MGG_InputLayout_Destroy(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
	assert(device != nullptr);
	assert(layout != nullptr);

	if (layout == nullptr)
		return;

	//remove_by_value(device->layouts, layout);

	delete [] layout->bindings;
	delete [] layout->attributes;
	delete layout;
}

static const int DefaultPoolSize = 1024;

MGG_Shader* MGG_Shader_Create(MGG_GraphicsDevice* device, MGShaderStage stage, mgbyte* bytecode, mgint sizeInBytes)
{
	assert(device != nullptr);
	assert(bytecode != nullptr);
	assert(sizeInBytes > 0);

	auto shader = new MGG_Shader();
	shader->stage = stage;

	// We store the pre-generated bindings info at the top of the shader bytecode data.
	int uniformCount = 0;
	auto& layoutBindings = shader->bindings;
	{
		uniformCount = *(mgint*)bytecode; bytecode += sizeof(mgint); sizeInBytes -= sizeof(mgint);
		shader->uniformSlots = *(mguint*)bytecode; bytecode += sizeof(mguint); sizeInBytes -= sizeof(mguint);
		shader->textureSlots = *(mguint*)bytecode; bytecode += sizeof(mguint); sizeInBytes -= sizeof(mguint);
		shader->samplerSlots = *(mguint*)bytecode; bytecode += sizeof(mguint); sizeInBytes -= sizeof(mguint);

		int count = *(mgint*)bytecode; bytecode += sizeof(mgint); sizeInBytes -= sizeof(mgint);
		layoutBindings.resize(count);
		memcpy(layoutBindings.data(), bytecode, sizeof(VkDescriptorSetLayoutBinding) * count);

		// The shader bytecode follows.
		bytecode += sizeof(VkDescriptorSetLayoutBinding) * count;
		sizeInBytes -= sizeof(VkDescriptorSetLayoutBinding) * count;
	}

	VkShaderModuleCreateInfo create_info = { VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO };
	create_info.codeSize = sizeInBytes;
	create_info.pCode = (uint32_t*)bytecode;
	VkResult res = vkCreateShaderModule(device->device, &create_info, NULL, &shader->module);
	VK_CHECK_RESULT(res);

	shader->id = ++device->currentShaderId;

	device->all_shaders.push_back(shader);

	// If the shader has no bindings... then skip all the layout
	// and descriptor setup.
	if (layoutBindings.empty())
	{
		shader->setLayout = nullptr;
		shader->poolInfo = nullptr;
		shader->pool = nullptr;
		shader->writes = nullptr;
		return shader;
	}

	VkDescriptorSetLayoutCreateInfo layoutInfo;
	memset(&layoutInfo, 0, sizeof(layoutInfo));
	layoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
	layoutInfo.bindingCount = layoutBindings.size();
	layoutInfo.pBindings = layoutBindings.data();
	layoutInfo.flags = 0;

	res = vkCreateDescriptorSetLayout(device->device, &layoutInfo, nullptr, &shader->setLayout);
	VK_CHECK_RESULT(res);

	// Prepare the initial descriptor pool.
	{
		VkDescriptorPoolSize* pool_sizes = new VkDescriptorPoolSize[layoutBindings.size()];

		for (int i = 0; i < layoutBindings.size(); i++)
		{
			auto& b = layoutBindings[i];
			pool_sizes[i].type = b.descriptorType;
			pool_sizes[i].descriptorCount = DefaultPoolSize;
		}

		shader->poolInfo = new VkDescriptorPoolCreateInfo();
		shader->poolInfo->sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
		shader->poolInfo->flags = 0;
		shader->poolInfo->maxSets = DefaultPoolSize;
		shader->poolInfo->poolSizeCount = layoutBindings.size();
		shader->poolInfo->pPoolSizes = pool_sizes;

		res = vkCreateDescriptorPool(device->device, shader->poolInfo, nullptr, &shader->pool);
		VK_CHECK_RESULT(res);
	}

	// Pre-fill the free descriptor sets now.
	VkDescriptorSetAllocateInfo alloc_info = { VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO };
	alloc_info.descriptorPool = shader->pool;
	alloc_info.descriptorSetCount = 1;
	alloc_info.pSetLayouts = &shader->setLayout;
	for (int i = 0; i < DefaultPoolSize; i++)
	{
		MGVK_DescriptorInfo* info = new MGVK_DescriptorInfo;
		info->frame = 0;
		shader->freeSets.push(info);

		res = vkAllocateDescriptorSets(device->device, &alloc_info, &info->set);
		VK_CHECK_RESULT(res);
	}

	// Prepare the write descriptor set for updates at runtime.
	VkWriteDescriptorSet* write = shader->writes = new VkWriteDescriptorSet[layoutBindings.size()];
	VkDescriptorBufferInfo* bufferInfo = new VkDescriptorBufferInfo[uniformCount];
	VkDescriptorImageInfo* imageInfo = new VkDescriptorImageInfo[layoutBindings.size() - uniformCount];
	for (int i = 0; i < layoutBindings.size(); i++)
	{
		auto& b = layoutBindings[i];

		if (b.descriptorType == VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC)
		{
			bufferInfo->buffer = NULL;
			bufferInfo->offset = 0;
			bufferInfo->range = VK_WHOLE_SIZE;

			write->sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			write->pNext = nullptr;
			write->dstSet = nullptr;
			write->dstBinding = b.binding;
			write->dstArrayElement = 0;
			write->descriptorType = b.descriptorType;
			write->descriptorCount = 1;
			write->pImageInfo = nullptr;
			write->pBufferInfo = bufferInfo++;
			write->pTexelBufferView = nullptr;

			write++;
		}
		else if (b.descriptorType == VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER)
		{
			imageInfo->imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
			imageInfo->imageView = nullptr;
			imageInfo->sampler = nullptr;

			write->sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			write->pNext = nullptr;
			write->dstSet = nullptr;
			write->dstBinding = b.binding;
			write->dstArrayElement = 0;
			write->descriptorType = VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
			write->descriptorCount = 1;
			write->pImageInfo = imageInfo++;
			write->pBufferInfo = nullptr;
			write->pTexelBufferView = nullptr;

			write++;
		}
		else
		{
			assert(0);
		}
	}

	return shader;
}

void MGG_Shader_Destroy(MGG_GraphicsDevice* device, MGG_Shader* shader)
{
	assert(device != nullptr);
	assert(shader != nullptr);

	if (!shader)
		return;

	for (auto pair : shader->usedSets)
	{
		//vkFreeDescriptorSets(device->device, shader->pool, 1, &pair.second->set);
		delete pair.second;
	}

	while (shader->freeSets.size() > 0)
	{
		auto free = shader->freeSets.front();
		shader->freeSets.pop();

		//vkFreeDescriptorSets(device->device, shader->pool, 1, &free->set);
		delete free;
	}

	vkDestroyDescriptorSetLayout(device->device, shader->setLayout, nullptr);

	vkDestroyDescriptorPool(device->device, shader->pool, nullptr);
	delete shader->poolInfo;

	vkDestroyShaderModule(device->device, shader->module, nullptr);

	mg_remove(device->all_shaders, shader);
	delete shader;
}

MGG_OcclusionQuery* MGG_OcclusionQuery_Create(MGG_GraphicsDevice* device)
{
	assert(device != nullptr);

	auto query = new MGG_OcclusionQuery();

	// TODO: Implement!

	return query;
}

void MGG_OcclusionQuery_Destroy(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
	assert(device != nullptr);
	assert(query != nullptr);

	if (!query)
		return;

	// TODO: Implement!

	delete query;
}

void MGG_OcclusionQuery_Begin(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
	assert(device != nullptr);
	assert(query != nullptr);

	// TODO: Implement!
}

void MGG_OcclusionQuery_End(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
	assert(device != nullptr);
	assert(query != nullptr);

	// TODO: Implement!
}

mgbool MGG_OcclusionQuery_GetResult(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query, mgint& pixelCount)
{
	assert(device != nullptr);
	assert(query != nullptr);

	// TODO: Implement!

	pixelCount = 0;
	return true;
}
