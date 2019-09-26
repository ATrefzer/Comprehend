#include "pch.h"
#include "CallGraphBuilder.h"
#include "TextWriterAdapter.h"
#include <cassert>
#include "CallStackTests.h"




class ProfilerApiMock : public IProfilerApi
{
    // Inherited via IProfilerApi
    virtual void Release() override
    {
    }
    virtual FunctionInfo* CreateFunctionInfo(FunctionID funcId) override
    {
        return new FunctionInfo(funcId, L"", L"");
    }
    virtual ThreadID GetThreadId() override
    {
        return 0;
    }
    virtual std::wstring GetModuleName(FunctionID functionId) override
    {
        return std::wstring(L"ModuleName_") + std::to_wstring(functionId);
    }
    virtual std::wstring GetFunctionName(FunctionID functionId) override
    {
        return std::wstring(L"FunctionName_") + std::to_wstring(functionId);
    }
};

class TextWriterAdapterMock : public ITextWriter
{
public:

    std::wstringstream _stream;
    // Inherited via ITextWriter
    virtual void WriteString(const wstring& stringToWrite) override
    {
        _stream << stringToWrite;
    }
    virtual void Open() override
    {
        _stream.clear();
    }
    virtual void Close() override
    {
    }

    std::wstring GetAll()
    {
        return _stream.str();
    }
};

// Excluded from release build
extern "C" void __stdcall RunTests()
{
    SingleVisibleCall();
}

void SingleVisibleCall()
{
    // Thread is always 0
    auto api = new ProfilerApiMock();
    auto writer = new TextWriterAdapterMock();
    writer->Open();

    CallGraphBuilder builder(api, writer);

    auto func_1 = builder.AddFunctionInfo(1);

    builder.OnThreadCreated(0);
    builder.OnEnter(1);
    builder.OnLeave(1);

   assert(builder.IsEmpty(0));
    auto all = writer->GetAll();

    builder.Release();
}
