#pragma once
#include <string>
#include "Common/TextFileWriter.h"
#include "Common/Encodings.h"


class ITextWriter
{
public:
    virtual void WriteString(const wstring& stringToWrite) = 0;
    virtual void Open() = 0;
    virtual void Close() = 0;
    virtual ~ITextWriter(){}
};

class TextWriterAdapter : public ITextWriter
{
    std::wstring _file;
    CppEssentials::TextFileWriter* _writer;
public:
    TextWriterAdapter(const std::wstring& file)
    {
        _file = file;
    }

    virtual ~TextWriterAdapter()
    {
        Close();
    }

    void Open()
    {
        Close();
        _writer = new CppEssentials::TextFileWriter();
        _writer->Open(_file, CppEssentials::FileOpenMode::CreateNew, CppEssentials::UTF16LittleEndianEncoder());
    }

    void Close()
    {
        if (_writer != nullptr)
        {
            _writer->Close();
            _writer = nullptr;
        }
    }

    // Inherited via ITextWriter
    virtual void WriteString(const wstring& stringToWrite) override
    {
        _writer->WriteString(stringToWrite);
    }
};