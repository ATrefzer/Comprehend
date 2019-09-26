//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once

#include <string>

namespace CppEssentials
{
	class FilePath
	{
	public:

		struct Parts
		{
			std::wstring _parent;
			std::wstring _name;
			std::wstring _extension;
		};

		/// Convert relative to absolute path. The file part can be a search pattern.
		/// It is not necessary that the path exists!
		///
		static std::wstring Resolve(const std::wstring& path);

		/// Combines the two given parts to a file path.
		///
		static std::wstring Combine(const std::wstring& part1, const std::wstring& part2);

		static Parts Split(const std::wstring& path);
	};
};
