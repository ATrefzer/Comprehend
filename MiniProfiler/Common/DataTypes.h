//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once

#include <Windows.h>
#include <cstddef>
#include <string>
using namespace std;

typedef __int8 Int8;
typedef unsigned __int8 UInt8;
typedef __int16 Int16;
typedef unsigned __int16 UInt16;
typedef __int32 Int32;
typedef unsigned __int32 UInt32;
typedef __int64 Int64;
typedef unsigned __int64 UInt64;

typedef unsigned char byte;

typedef std::string String;

/// Encoding of the data inside the buffer.
enum ByteOrder
{
	/// The most significant byte is stored in the highest address.
	/// Intel format.
	LittleEndian,

	/// The most significant byte is stored in the lowest address.
	/// Network standard, Motorola format.
	BigEndian
};

enum CodePage
{
	CodePage_Current = 0,
	//CP_ACP (From WinNls.h)
	CodePage_Utf8 = 65001,
	//CP_UTF8 (From WinNls.h)
	CodePage_Windows_1252 = 1252
};
