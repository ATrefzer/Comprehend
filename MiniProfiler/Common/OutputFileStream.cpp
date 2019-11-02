//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#include "OutputFileStream.h"
#include "Exception.h"
#include <crtdbg.h>

namespace CppEssentials
{
	OutputFileStream::OutputFileStream(unsigned int cacheSize) : CACHE_SIZE(cacheSize)
	{
		Clear();
		if (cacheSize > 0)
		{
			_cache = new byte[cacheSize];
		}
		else
		{
			_cache = nullptr;
		}
	}

	OutputFileStream::~OutputFileStream()
	{
		Close();

		if (_cache)
		{
			delete [] _cache;
			_cache = nullptr;
		}
	}

	void OutputFileStream::Open(const wstring& filePath, FileOpenMode eMode)
	{
		Close();

		DWORD openMode = 0;
		if (eMode == CreateNew)
		{
			openMode = CREATE_ALWAYS;
		}
		else
		{
			// Append
			openMode = OPEN_ALWAYS;
		}

		_handle = CreateFileW(filePath.c_str(), GENERIC_WRITE, 0, nullptr, openMode, FILE_ATTRIBUTE_NORMAL, nullptr);

		if (_handle == INVALID_HANDLE_VALUE)
		{
			throw Exception(L"Failed opening file", filePath, GetLastError());
		}

		if (eMode == Append)
		{
			DWORD pos = SetFilePointer(_handle, 0, nullptr, FILE_END);

			if (pos == INVALID_SET_FILE_POINTER)
			{
				throw Exception(L"Failed seeking end of file!", GetLastError());
			}
		}
	}

	void OutputFileStream::Close()
	{
		Flush();

		if (_handle != INVALID_HANDLE_VALUE)
		{
			CloseHandle(_handle);
		}

		Clear();
	}

	void OutputFileStream::Write(byte value)
	{
		Write(&value, 1);
	}

	void OutputFileStream::_WriteChunk(const byte* data, UInt32 length)
	{
		DWORD bytesWritten = 0;
		const BOOL result = WriteFile(_handle, (LPVOID)data, length, &bytesWritten, static_cast<LPOVERLAPPED>(nullptr));

		_ASSERTE(length == bytesWritten);

		if (result == FALSE)
		{
			throw Exception(L"Failed writing to file!", GetLastError());
		}
	}

	void OutputFileStream::Write(const byte* data, UInt32 length)
	{
		if (_cache && _cachedBytes + length > CACHE_SIZE)
		{
			Flush();
		}

		if (length > CACHE_SIZE)
		{
			_WriteChunk(data, length);
			return;
		}

		memcpy_s(_cache + _cachedBytes, CACHE_SIZE, data, length);
		_cachedBytes += length;
	}

	void OutputFileStream::Flush()
	{
		if (_cache && _cachedBytes > 0)
		{
			_WriteChunk(_cache, _cachedBytes);

			_cachedBytes = 0;
		}
		
	}

	void OutputFileStream::Clear()
	{
		_handle = INVALID_HANDLE_VALUE;
		_cachedBytes = 0;
	}
}
