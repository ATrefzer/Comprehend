#include "pch.h"
#include "CallGraphBuilder.h"
#include "Common\Encodings.h"

// TODO (Algorithm)
// - Write functions at the end and keep the ids in the output. Let file parser resolve the methods.
// - Write @hidded tag only together with @enter / @leave tags if - and only if - there is hidded element on the top of the stack.
//   Nowhere else @hidden is written.
// - Introduce a stack for all calls (hidded / visible). The stack can anser the question: Am I embedded in a hidded call.

void CallGraphBuilder::AddFunctionInfo(FunctionID funcId)
{
    // TODO read filter file! Remove filtering from FunctionInfo
    // IsHidden(FunctionInfo)
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
    _isHiding = false;
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

    //<tid><space><prefix><space><funcName>
    std::wstringstream msg;
    msg << L"\r\n" << tid;
    msg << spaces;
    msg << prefix << L" ";   
    msg << info->_funcName;
    return msg.str();
}

std::wstring CallGraphBuilder::FormatHidden(ThreadID tid)
{
    std::wstringstream msg;
    msg << L"\r\n" << tid << L" ";
    msg << L"@hidden";
    return msg.str();
}

std::wstring CallGraphBuilder::FormatCreateThread(ThreadID tid)
{
    std::wstringstream msg;
    msg << L"\r\n" << tid << L" ";
    msg << L"@create_thread";
    return msg.str();
}

std::wstring CallGraphBuilder::FormatDestroyThread(ThreadID tid)
{
    std::wstringstream msg;
    msg << L"\r\n" << tid << L" ";
    msg << L"@destroy_thread";
    return msg.str();
}


void CallGraphBuilder::OnEnter(FunctionID funcId)
{
    auto info = GetFunctionInfo(funcId);
   
    if (!info->Hide())
    {
        _isHiding = false;
        auto tid = _api->GetThreadId();
        auto msg = Format(L"Enter", tid, info);
        _writer->WriteString(msg);
    }
    else
    {
        HideCall();
    }

    // TODO else mark all other calls as indirect
}

void CallGraphBuilder::HideCall()
{
    if (!_isHiding)
    {
        _isHiding = true;
        auto tid = _api->GetThreadId();
        _writer->WriteString(FormatHidden(tid));
    }
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
    auto tid = _api->GetThreadId();

    if (!info->Hide())
    {
        auto msg = Format(L"TailCall", tid, info);
        _writer->WriteString(msg);
    }
   
}

void CallGraphBuilder::OnThreadCreated(ThreadID tid)
{
    _writer->WriteString(FormatCreateThread(tid));
}

void CallGraphBuilder::OnThreadDestroyed(ThreadID tid)
{
    _writer->WriteString(FormatDestroyThread(tid));
    // TODO Stop tracking (reset) calls on this thread
    // Note that the same ThreadID may be reused later.
}
