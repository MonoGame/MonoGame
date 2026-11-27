// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include <vector>
#include <string>
#include <sstream>


class PipelineLogger
{
private:
	std::ostringstream _output;
	std::string _string;
	int _errors = 0;
	int _warnings = 0;
	int _info = 0;


	template <typename... Args>
	void append(Args&&... args)
	{
		(_output << ... << std::forward<Args>(args));
	}

public:

	template <typename... Args>
	void warn(Args&&... args) {
		append("WARNING: ", std::forward<Args>(args) ..., '\n');
		++_warnings;
	}

	template <typename... Args>
	void error(Args&&... args)
	{
		append("ERROR: ", std::forward<Args>(args) ..., '\n');
		++_errors;
	}

	template <typename... Args>
	void info(Args&&... args)
	{
		append("INFO: ", std::forward<Args>(args) ..., '\n');
		++_info;
	}

	const char* str()
	{
		_string = _output.str();
		return _string.c_str();
	}

	bool hasErrors() const { return _errors > 0; }
	int  errors() const { return _errors; }
	int  warnings() const { return _warnings; }
	int  info() const { return _info; }
};
