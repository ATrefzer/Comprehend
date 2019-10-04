//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once

#include <windows.h>
#include <string>

namespace CppEssentials
{
    class Environment
    {
    public:
     
    	static std::wstring GetVariableFromEnvironment(const std::wstring & name);

        /// Returns the directory for the given module. If the module is NULl the path of
        /// the executable is returned.
        ///
        static std::wstring GetModuleDirectory(HMODULE hInst = NULL);

		static std::wstring GetModuleName(HINSTANCE hInst = NULL);

        static std::wstring GetWorkingDirectory();

        /// A typical path is C:\Documents and Settings\username\Application Data.
        /// see http://msdn.microsoft.com/en-us/library/bb762494%28v=VS.85%29.aspx
        /// CSIDL_APPDATA or FOLDERID_RoamingAppData
        ///
        static std::wstring GetApplicationDataDirectory();

        /// A typical path is C:\Documents and Settings\All Users\Application Data
        /// CSIDL_COMMON_APPDATA or FOLDERID_ProgramData
        ///
        static std::wstring GetCommonAppApplicationDataDirectory();
        static void SetVariableToEnvironment(const std::wstring & name, const std::wstring & value);

    private:

        static std::wstring GetSpecialDirectoryPath(int clsid);
		

    };

};