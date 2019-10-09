//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once


#include "IOutputStream.h"
#include "DataTypes.h"
#include <string>
#include <comutil.h>
using namespace std;

namespace CppEssentials
{
	class IRandomAccess;

	class BinaryWriter : public IOutputStream
	{
	public:


		BinaryWriter(IOutputStream* outputStream, bool takeOwnership = false);


		~BinaryWriter(void);

		/// Set byte order stored in the internal buffer. Affects reading / writing methods.
		/// If the byte order is set to BigEndian the bytes are swapped
		/// when reading or writing a value.
		///
		void SetByteOrder(ByteOrder eByteOrder);

		void WriteInt8(Int8 value);
		void WriteInt16(Int16 value);
		void WriteInt32(Int32 value);
		void WriteInt64(Int64 value);

		void WriteUInt8(UInt8 value);
		void WriteUInt16(UInt16 value);
		void WriteUInt32(UInt32 value);
		void WriteUInt64(UInt64 value);

		void WriteFloat(float fValue);
		void WriteDouble(double dValue);

		// Terminating zero is not written to the buffer
		void WriteString(const string& value);

		/// Characters do not consider the byte order. But the byte counter does.
		void WriteWideString(const wstring& value);
		void WriteComString(const _bstr_t& value);

		void WriteBool(bool value);

		void Write(byte value) override;
		void Write(const byte* data, UInt32 length) override;
		void Flush() override;

	private:

		// Do not allow copy
		BinaryWriter(const BinaryWriter&);

		// Removes the attached source
		void Clear();

		IOutputStream* _outputStream;

		bool _hasOwnership;

		ByteOrder _eByteOrder;
	};
}
