// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include <string>
#include <vector>
#include <functional>


/// <summary>
/// Compare two strings for caseless equality.
/// </summary>
inline bool iequals(const std::string& a, const std::string& b)
{
	if (a.size() != b.size())
		return false;

	return std::equal(a.begin(), a.end(), b.begin(),
		[](int a, int b) { return std::tolower(a) == std::tolower(b); });
}


/// <summary>
/// Returns true if the string is a valid C/C++ identifier name.
/// </summary>
inline bool isidentifier(const std::string& s)
{
	if (s.empty())
		return false;

	int first = s[0];
	if (!std::isalpha(first) && first != '_')
		return false;

	for (int c : s.substr(1))
	{
		if (!std::isalnum(c) && c != '_')
			return false;
	}

	return true;
}

template<class T, class Predicate>
inline size_t find_index(const std::vector<T>& v, Predicate pred)
{
	for (int i=0; i < v.size(); i++)
	{
		if (pred(v[i]))
			return i;
	}

	return -1;
}


inline void replace_all(std::string& string, char find, char replace)
{
	auto c = string.begin();
	for ( ; c != string.end(); c++)
	{
		if (*c == find)
			*c = replace;
	}
}


inline std::string get_filename_without_extension(std::string filepath)
{
	if (filepath.empty())
		return filepath;

	replace_all(filepath, '\\', '/');
	size_t index = filepath.find_last_of('/') + 1;		
	size_t end = filepath.find_first_of('.', index);
	if (end == -1)
		end = filepath.length() - 1;

	std::string filename = filepath.substr(index, end - index);
	return filename;
}
