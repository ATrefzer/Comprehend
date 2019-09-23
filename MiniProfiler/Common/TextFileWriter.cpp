//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#include "TextFileWriter.h"
#include "IOutputStream.h"
#include "Encodings.h"

namespace CppEssentials
{
TextFileWriter::TextFileWriter()
{
    Clear();
}

void TextFileWriter::Open(const wstring & filePath, FileOpenMode eMode, const IEncoder & encoder)
{
    Close();

    _writer.Open(filePath, eMode);
    _encoder = encoder.Clone();

    WriteByteOrderMark();
}

void TextFileWriter::Close()
{
    _writer.Close();

    if (_encoder)
    {
        delete _encoder;
    }

    Clear();
}

void TextFileWriter::WriteByteOrderMark()
{
    _encoder->WriteByteOrderMark(&_writer);
}

void TextFileWriter::WriteString(const wstring & stringToWrite)
{
    _encoder->WriteWideChars(stringToWrite.c_str(), (UInt32)stringToWrite.length(), &_writer);
}

void TextFileWriter::WriteWideChar(wchar_t wideChar)
{
    _encoder->WriteWideChars(&wideChar, 1, &_writer);
}

void TextFileWriter::Clear()
{
    _encoder = NULL;
}

};