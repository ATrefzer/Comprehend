#include "pch.h"
#include "CallGraphBuilder.h"
#include "Common\Encodings.h"

void CallGraphBuilder::AddFunctionInfo(FunctionID funcId)
{
    auto info = _api->CreateFunctionInfo(funcId);
    _funcInfos.emplace(info->_id, info);
}

FunctionInfo* CallGraphBuilder::GetFunctionInfo(FunctionID funcId)
{
    return _funcInfos[funcId];
}

CallGraphBuilder::CallGraphBuilder(const std::wstring& file, ProfilerApi * api)
{
    _api = api;
    _writer = std::make_shared<CppEssentials::TextFileWriter>();;
    _writer->Open(file, CppEssentials::FileOpenMode::CreateNew, CppEssentials::UTF16LittleEndianEncoder());


}

void CallGraphBuilder::Release()
{
    if (_writer != nullptr)
    {
        _writer->Close();
        _writer = nullptr;
    }

    if (_api != nullptr)
    {
        _api->Release();
        _api = nullptr;
    }
}

std::wstring CallGraphBuilder::Format(const wstring& prefix, ThreadID tid, FunctionInfo* info, int numSpaces)
{
    const auto spaces = std::wstring(numSpaces, ' ');

    std::wstringstream msg;
    msg << L"\r\n[" << tid << L"]";
    msg << spaces;
    msg << prefix << L" ";
    msg << info->_moduleName << L"!";
    msg << info->_funcName;
    return msg.str();
}

void CallGraphBuilder::OnEnter(FunctionID funcId)
{
    
    auto info = GetFunctionInfo(funcId);
    if (!info->Hide())
    {
        auto tid = _api->GetThreadId();
        auto msg = Format(L"Enter", tid, info);
        _writer->WriteString(msg);
    }

    // TODO else mark all other calls as indirect
}

void CallGraphBuilder::OnLeave(FunctionID funcId)
{
    auto info = GetFunctionInfo(funcId);
    if (!info->Hide())
    {
        auto tid = _api->GetThreadId();
        auto msg = Format(L"Leave", tid, info);
        _writer->WriteString(msg);
    }
}

void CallGraphBuilder::OnTailCall(FunctionID funcId)
{
    auto info = GetFunctionInfo(funcId);
    if (!info->Hide())
    {
        auto tid = _api->GetThreadId();
        auto msg = Format(L"TailCall", tid, info);
        _writer->WriteString(msg);
    }
}
