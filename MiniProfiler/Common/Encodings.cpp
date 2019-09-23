//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#include "Encodings.h"
#include "IOutputStream.h"
#include "IInputStream.h"

using namespace std;

namespace CppEssentials
{
    byte UTF16LittleEndianEncoder::GetMaxBytesPerCharacter()
    {
        // Does not apply
        return 2;
    }

    IEncoder * UTF16LittleEndianEncoder::Clone() const
    {
        return new UTF16LittleEndianEncoder();
    }

    UInt32 UTF16LittleEndianEncoder::ReadWideChars(wchar_t * targetBuffer, UInt32 targetBufferCharCount, IInputStream * reader)
    {
        UInt32 bytesRead = reader->Read(reinterpret_cast<byte*>(targetBuffer), targetBufferCharCount * sizeof(wchar_t));
        return bytesRead / 2;
    }

    void UTF16LittleEndianEncoder::WriteWideChars(const wchar_t * sourceBuffer, UInt32 sourceBufferCharCount, IOutputStream * writer)
    {
        writer->Write(reinterpret_cast<const byte*>(sourceBuffer), sourceBufferCharCount * sizeof(wchar_t));
    }

    void UTF16LittleEndianEncoder::ReadByteOrderMark(IInputStream * reader)
    {
        byte b0 = reader->Read();
        byte b1 = reader->Read();

        if (b0 != 0xff || b1 != 0xfe)
        {
            throw Exception(L"Unexpected byte order mark!");
        }
    }

    void UTF16LittleEndianEncoder::WriteByteOrderMark(IOutputStream * writer)
    {
        writer->Write(0xff);
        writer->Write(0xfe);
    }
};