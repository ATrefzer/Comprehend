//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#include "BinaryWriter.h"
#include "BinaryTools.h"
#include <crtdbg.h>

namespace CppEssentials
{
    BinaryWriter::BinaryWriter(IOutputStream * outputStream, bool takeOwnership /*= false*/)
    {
        _outputStream = outputStream;
        _hasOwnership = takeOwnership;
        _eByteOrder = LittleEndian;
    }
    BinaryWriter::~BinaryWriter(void)
    {
        Clear();

    }

    void BinaryWriter::SetByteOrder(ByteOrder eByteOrder)
    {
        _eByteOrder = eByteOrder;
    }

    void BinaryWriter::WriteInt8(Int8 value)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&value), sizeof(Int8));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&value), sizeof(Int8));
    }

    void BinaryWriter::WriteInt16(Int16 value)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&value), sizeof(Int16));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&value), sizeof(Int16));
    }

    void BinaryWriter::WriteInt32(Int32 value)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&value), sizeof(Int32));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&value), sizeof(Int32));
    }

    void BinaryWriter::WriteInt64(Int64 value)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&value), sizeof(Int64));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&value), sizeof(Int64));
    }

    void BinaryWriter::WriteUInt8(UInt8 value)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&value), sizeof(UInt8));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&value), sizeof(UInt8));
    }

    void BinaryWriter::WriteUInt16(UInt16 value)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&value), sizeof(UInt16));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&value), sizeof(UInt16));
    }

    void BinaryWriter::WriteUInt32(UInt32 value)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&value), sizeof(UInt32));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&value), sizeof(UInt32));
    }

    void BinaryWriter::WriteUInt64(UInt64 value)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&value), sizeof(UInt64));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&value), sizeof(UInt64));
    }

    void BinaryWriter::WriteFloat(float fValue)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&fValue), sizeof(float));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&fValue), sizeof(float));
    }

    void BinaryWriter::WriteDouble(double dValue)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&dValue), sizeof(double));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&dValue), sizeof(double));
    }

    void BinaryWriter::WriteBool(bool value)
    {
        if (_eByteOrder == BigEndian)
        {
            BinaryTools::SwapBuffer(reinterpret_cast<byte*>(&value), sizeof(value));
        }

        _outputStream->Write(reinterpret_cast<byte*>(&value), sizeof(value));
    }

    void BinaryWriter::WriteString(const string & value)
    {
        _ASSERTE(sizeof(UInt32) >= sizeof(size_t));
        UInt32 length = (UInt32)value.length();

        WriteUInt32(length);

        if (length > 0)
        {
            // Terminating zero is not written!
            _outputStream->Write(reinterpret_cast<const byte*>(value.c_str()), length * sizeof(char));
        }
    }

    void BinaryWriter::WriteWideString(const wstring & value)
    {
        _ASSERTE(sizeof(UInt32) >= sizeof(size_t));
        UInt32 length = (UInt32)value.length() * sizeof(wchar_t);
        WriteUInt32(length);

        if (length > 0)
        {
            // Terminating zero is not written!
            _outputStream->Write(reinterpret_cast<const byte*>(value.c_str()), length);
        }
    }

    void BinaryWriter::WriteComString(const _bstr_t & value)
    {
        // get the _bstr_t's size in bytes!
        UInt32 length = SysStringByteLen(value);

        WriteUInt32(length);

        if (length > 0)
        {
            _outputStream->Write(reinterpret_cast<byte*>((wchar_t*)value), length);
        }
    }

    void BinaryWriter::Write(byte value)
    {
        _outputStream->Write(value);
    }

    void BinaryWriter::Write(const byte * data, UInt32 length)
    {
        _outputStream->Write(data, length);
    }

    void BinaryWriter::Flush()
    {
        _outputStream->Flush();
    }

    void BinaryWriter::Clear()
    {
        if (_hasOwnership && _outputStream != NULL)
        {
            delete _outputStream;
            _outputStream = NULL;
        }
    }

}