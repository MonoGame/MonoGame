// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include <vector>


class memstream
{
public:

	std::vector<uint8_t> buffer;

	const uint8_t* ptr() const
	{
		return buffer.data();
	}

	size_t size() const
	{
		return buffer.size();
	}

	size_t offset() const
	{
		return buffer.size();
	}

	void write(const char* data, size_t len)
	{
		size_t index = buffer.size();
		buffer.resize(index + len);
		memcpy(buffer.data() + index, data, len);
	}

	void write(const uint8_t* data, size_t len)
	{
		size_t index = buffer.size();
		buffer.resize(index + len);
		memcpy(buffer.data() + index, data, len);
	}

	void write(const bool& data)
	{
		buffer.push_back(data ? 1 : 0);
	}

	void write(const uint8_t& data)
	{
		buffer.push_back(data);
	}

	void write(const uint16_t& data)
	{
		size_t index = buffer.size();
		buffer.resize(index + 2);
		memcpy(buffer.data() + index, &data, 2);
	}

	void write(const int32_t& data)
	{
		size_t index = buffer.size();
		buffer.resize(index + 4);
		memcpy(buffer.data() + index, &data, 4);
	}

	void write(const uint32_t& data)
	{
		size_t index = buffer.size();
		buffer.resize(index + 4);
		memcpy(buffer.data() + index, &data, 4);
	}

	void write(const float& data)
	{
		size_t index = buffer.size();
		buffer.resize(index + 4);
		memcpy(buffer.data() + index, &data, 4);
	}

	void write(const double& data)
	{
		size_t index = buffer.size();
		buffer.resize(index + 8);
		memcpy(buffer.data() + index, &data, 8);
	}

	void write_at(size_t index, const uint32_t& data)
	{
		memcpy(buffer.data() + index, &data, 4);
	}

	void write7bitInt(const int32_t& data)
	{
		uint32_t v = (uint32_t)data;
		while (v > 0x80)
		{
			buffer.push_back((uint8_t)(v | 0x80));
			v >>= 7;
		}
		buffer.push_back((uint8_t)v);
	}

	void write(const std::string& data)
	{
		write7bitInt(data.length());
		auto iter = data.cbegin();
		for ( ; iter != data.end(); iter++)
			buffer.push_back(*iter);
	}
};
