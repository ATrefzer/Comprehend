//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once

#include "DataTypes.h"

namespace CppEssentials
{
	/// Interface for a general binary input stream
	///
	class IInputStream
	{
	public:
		virtual ~IInputStream()
		{
		};

		/// Reads the next byte from input stream
		/// If there are no more bytes an Exception is thrown.
		///
		virtual byte Read() = 0;

		/// Read a chunk of bytes from the input stream.
		/// The method returns the number of bytes that were read.
		/// An exception is thrown if you try to read behind the stream.
		///
		/// @param data     The buffer to fill
		/// @param length   Size of the buffer in bytes. The method tries to fill the whole buffer.
		/// @return         Count of actual read bytes.
		///
		virtual UInt32 Read(byte* data, UInt32 length) = 0;

		/// Returns the next byte without affecting the input stream.
		/// If there are no more bytes an Exception is thrown.
		///
		virtual byte Peek() = 0;

		/// Returns true if the stream contains no more bytes.
		/// The EOF flag is always up-to-date. (Unlike the STL file stream)
		///
		virtual bool IsEof() = 0;
	};
}
