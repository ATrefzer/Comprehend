//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once

#include "DataTypes.h"
#include <crtdbg.h>

namespace CppEssentials
{
	class BinaryTools
	{
	public:

		/// Flips bytes in buffer. (inplace)
		///
		static void SwapBuffer(byte* buffer, UInt32 length);

		/// Sets bit at given bit index starting with zero
		template <class T>
		static void SetBitAt(T& value, byte bitIndex, bool bitValue)
		{
			_ASSERTE(bitIndex < std::numeric_limits<T>::digits);

			if (bitValue == true)
			{
				value |= (1 << bitIndex);
			}
			else
			{
				value &= ~(1 << bitIndex);
			}
		}

		/// Reads bit from specified position
		template <class T>
		static bool GetBitAt(T value, byte bitIndex)
		{
			_ASSERTE(bitIndex < std::numeric_limits<T>::digits);
			return ((value & (1 << bitIndex)) != 0);
		}

	private:

		BinaryTools(void);
		~BinaryTools(void);
	};
}
