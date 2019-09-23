//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once

#include <crtdbg.h>
#include "Exception.h"

#include "OutputFileStream.h"

namespace CppEssentials
{
class IInputStream;
class IEncoder;

/// Writer class for text files
class TextFileWriter
{
public:

    TextFileWriter();

    void Open(const wstring & filePath, FileOpenMode eMode, const IEncoder & encoder);

    void Close();

    void WriteByteOrderMark();

    void WriteString(const wstring & stringToWrite);

    void WriteWideChar(wchar_t wideChar);

private:

    void Clear();

    OutputFileStream _writer;
    IEncoder * _encoder;

    void operator=(const TextFileWriter &);

};
};