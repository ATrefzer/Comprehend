//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once

#include <crtdbg.h>
#include "Exception.h"
#include "DataTypes.h"
#include <cstddef>


namespace CppEssentials
{
	class IOutputStream;
	class IInputStream;

	/// Contains encoding information for the text reader / writer classes.
	///
	class IEncoder
	{
	public:
		virtual ~IEncoder()
		{
		};

		virtual IEncoder* Clone() const = 0;

		virtual UInt32 ReadWideChars(wchar_t* targetBuffer, UInt32 targetBufferCharCount, IInputStream* reader) = 0;

		/// Writes the given number of characters. No terminating 0 character is written.
		///
		virtual void WriteWideChars(const wchar_t* sourceBuffer, UInt32 sourceBufferCharCount,
		                            IOutputStream* writer) = 0;

		/// Returns the maximum number of bytes used to encode a character.
		/// For single byte character encodings like WIndows-1252 this is 1.
		/// For UTF-8 it is 4, and for UTF-16 2
		///
		virtual byte GetMaxBytesPerCharacter() = 0;

		virtual void ReadByteOrderMark(IInputStream* reader) = 0;

		virtual void WriteByteOrderMark(IOutputStream* writer) = 0;
	};

	/// Structure of a (little endian) Unicode file
	/// 0xff 0xfe - 0xx 0x00 - 0xx 0x00 - 0x0a 0x00 - 0x0d etc.
	///
	class UTF16LittleEndianEncoder : public IEncoder
	{
	public:

		byte GetMaxBytesPerCharacter() override;

		IEncoder* Clone() const override;

		UInt32 ReadWideChars(wchar_t* targetBuffer, UInt32 targetBufferByteCount, IInputStream* reader) override;

		void WriteWideChars(const wchar_t* sourceBuffer, UInt32 sourceBufferCharCount, IOutputStream* writer) override;

		void ReadByteOrderMark(IInputStream* reader) override;

		void WriteByteOrderMark(IOutputStream* writer) override;
	};
}
