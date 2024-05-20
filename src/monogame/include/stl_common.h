// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once


#include <assert.h>
#include <vector>
#include <string>
#include <queue>
#include <map>
#include <functional>


template <class T>
void mg_remove(std::vector<T>& vector, const T& element)
{
	auto new_end = std::remove(vector.begin(), vector.end(), element);
	assert(new_end != vector.end());
	vector.erase(new_end, vector.end());
}

template <class T>
bool mg_contains(std::vector<T>& vector, const T& element)
{
	auto found = std::find(vector.begin(), vector.end(), element);
	return found != vector.end();
}
