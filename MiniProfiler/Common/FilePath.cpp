//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#include "FilePath.h"
#include "Exception.h"

namespace CppEssentials
{
	std::wstring FilePath::Combine(const std::wstring& part1, const std::wstring& part2)
	{
		// Consider the situation when part2 starts with '\'
		std::wstring cleanedPart2 = part2;
		while (cleanedPart2.find(L"\\") == 0)
		{
			cleanedPart2 = cleanedPart2.substr(1);
		}

		std::wstring path = part1;

		if (path.length() > 0)
		{
			if (path[path.length() - 1] != L'\\')
			{
				path.append(L"\\");
			}
		}

		path.append(cleanedPart2);

		return path;
	}

	std::wstring FilePath::Resolve(const std::wstring& path)
	{
		if (*(path.rbegin()) == L':')
		{
			// GetFullPathNameW cannot handle d: It has to be d:\ .
			return path;
		}

		wchar_t szFullPath[_MAX_PATH];
		wchar_t* szFile;

		if (FALSE == GetFullPathNameW(path.c_str(), _MAX_PATH, szFullPath, &szFile))
		{
			// Most likely some allocation problem with the buffer
			throw Exception(L"Failed resolving path: " + path, GetLastError());
		}

		return szFullPath;
	}

	FilePath::Parts FilePath::Split(const std::wstring& path)
	{
		Parts parts;

		// remove tailing backslash
		std::wstring splitPath = path;
		while (splitPath.length() > 0 && *(splitPath.rbegin()) == L'\\')
		{
			splitPath.erase(splitPath.length() - 1);
		}

		std::wstring::size_type backslashPos = splitPath.rfind(L'\\');
		if (backslashPos != std::wstring::npos)
		{
			// Extract substring without the last \ .
			parts._parent = splitPath.substr(0, backslashPos);
			parts._name = splitPath.substr(backslashPos + 1);
		}
		else
		{
			// Also if path is a drive
			parts._name = splitPath;
		}

		std::wstring::size_type dotPos = parts._name.rfind(L'.');
		if (dotPos != std::wstring::npos)
		{
			parts._extension = parts._name.substr(dotPos + 1);
		}

		return parts;
	}
};
