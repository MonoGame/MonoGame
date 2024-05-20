// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "csharp_MGP.h"

#include "stl_common.h"


struct MGP_Platform
{
	std::vector<MGP_Window*> windows;
};


struct MGP_Window
{
	MGP_Platform* platform;
	mgbool allowResizing = true;
	mgbool borderless = false;
};

MGP_Platform* MGP_Platform_Create(MGGameRunBehavior* behavior)
{
	assert(behavior != nullptr);

	*behavior = MGGameRunBehavior::Asynchronous;

	auto platform = new MGP_Platform();
	return platform;
}

void MGP_Platform_Destroy(MGP_Platform* platform)
{
	assert(platform != nullptr);
	delete platform;
}

void MGP_Platform_BeforeInitialize(MGP_Platform* platform)
{
	assert(platform != nullptr);
}

void MGP_Platform_RunLoop(MGP_Platform* platform)
{
	assert(platform != nullptr);
}

mgbool MGP_Platform_BeforeRun(MGP_Platform* platform)
{
	assert(platform != nullptr);
	return true;
}

mgbool MGP_Platform_BeforeUpdate(MGP_Platform* platform)
{
	assert(platform != nullptr);
	return true;
}

mgbool MGP_Platform_BeforeDraw(MGP_Platform* platform)
{
	assert(platform != nullptr);
	return true;
}

void MGP_Platform_EnterFullScreen(MGP_Platform* platform)
{
	assert(platform != nullptr);
}

void MGP_Platform_ExitFullScreen(MGP_Platform* platform)
{
	assert(platform != nullptr);
}

void MGP_Platform_Exit(MGP_Platform* platform)
{
	assert(platform != nullptr);
}

MGP_Window* MGP_Window_Create(MGP_Platform* platform, const char* identifier)
{
	assert(platform != nullptr);
	assert(identifier != nullptr);

	auto window = new MGP_Window();
	window->platform = platform;
	window->identifier = identifier;

	platform->windows.push_back(window);

	return window;
}

void MGP_Window_Destroy(MGP_Window* window)
{
	assert(window != nullptr);
	assert(window->platform != nullptr);

	remove_by_value(window->platform->windows, window);
	delete window;
}

void* MGP_Window_GetNativeHandle(MGP_Window* window)
{
	assert(window != nullptr);
	return window;
}

mgbool MGP_Window_GetAllowUserResizing(MGP_Window* window)
{
	assert(window != nullptr);
	return window->allowResizing;
}

void MGP_Window_SetAllowUserResizing(MGP_Window* window, mgbool allow)
{
	assert(window != nullptr);
	window->allowResizing = allow;
}

MG_EXPORT mgbool MGP_Window_GetIsBoderless(MGP_Window* window)
{
	assert(window != nullptr);
	return window->borderless;
}

MG_EXPORT void MGP_Window_SetIsBoderless(MGP_Window* window, mgbool borderless)
{
	assert(window != nullptr);
	window->borderless = borderless;
}
