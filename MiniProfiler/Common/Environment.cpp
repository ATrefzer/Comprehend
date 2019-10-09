//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//


#include "Environment.h"
#include "Exception.h"

// If you get an compiler error due to ambitious IXMLDOMDocument include this file before you import the msxml.
// I.e. in the stdafx.h
#include <codeanalysis/warnings.h>
#pragma warning (push)
#pragma warning (disable: ALL_CODE_ANALYSIS_WARNINGS)
#include "shlobj.h"
#pragma warning (pop)

namespace CppEssentials
{
	std::wstring Environment::GetVariableFromEnvironment(const std::wstring& name)
	{
		const auto size = _MAX_PATH;
		wchar_t buffer[_MAX_PATH];
		auto result = ::GetEnvironmentVariable(name.c_str(), buffer, _MAX_PATH);

		if (result == 0)
		{
			throw Exception(L"::GetEnvironmentVariable failed!", GetLastError());
		}

		return std::wstring(buffer);
	}

	std::wstring Environment::GetModuleDirectory(HMODULE hInst /* = NULL*/)
	{
		wchar_t srcBuffer[_MAX_PATH];
		wchar_t pathBuffer[_MAX_PATH];
		wchar_t* pFile = nullptr;

		::GetModuleFileName(hInst, srcBuffer, _MAX_PATH);

		::GetFullPathName(srcBuffer, _MAX_PATH, pathBuffer, &pFile);
		pathBuffer[wcslen(pathBuffer) - wcslen(pFile)] = L'\0';
		std::wstring path(pathBuffer);

		return path;
	}

	std::wstring Environment::GetModuleName(HINSTANCE hInst)
	{
		wchar_t srcBuffer[_MAX_PATH];
		wchar_t pathBuffer[_MAX_PATH];
		wchar_t* pFile = nullptr;

		GetModuleFileNameW(hInst, srcBuffer, _MAX_PATH);

		GetFullPathNameW(srcBuffer, _MAX_PATH, pathBuffer, &pFile);

		wchar_t* dotPos = wcsrchr(pFile, L'.');

		if (dotPos != nullptr)
		{
			*dotPos = L'\0';
		}

		return pFile;
	}

	std::wstring Environment::GetWorkingDirectory()
	{
		wchar_t* buffer = nullptr;

		// Let the function allocate the memory
		if ((buffer = _wgetcwd(nullptr, 0)) == nullptr)
		{
			throw Exception(L"Failed getting working directory!");
		}

		wstring workingDirectory(buffer);
		free(buffer);

		return workingDirectory;
	}

	std::wstring Environment::GetApplicationDataDirectory()
	{
		// Deprecated for Windows Vista!
		return GetSpecialDirectoryPath(CSIDL_APPDATA);
	}

	std::wstring Environment::GetCommonAppApplicationDataDirectory()
	{
		// Deprecated for Windows Vista!
		return GetSpecialDirectoryPath(CSIDL_COMMON_APPDATA);
	}

	void Environment::SetVariableToEnvironment(const std::wstring& name, const std::wstring& value)
	{
		auto result = ::SetEnvironmentVariable(name.c_str(), value.c_str());
		if (result == 0)
		{
			throw Exception(L"::SetEnvironmentVariable failed!", GetLastError());
		}
	}

	std::wstring Environment::GetSpecialDirectoryPath(int clsid)
	{
		wchar_t path[MAX_PATH];

		if (S_OK != (SHGetFolderPathW(nullptr, clsid, nullptr, 0, path)))
		{
			throw Exception(L"SHGetFolderPath failed!");
		}

		return wstring(path);
	}
};
