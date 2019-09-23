#pragma once
#include <string>
#include "Common/TextFileWriter.h"
#include <corprof.h>
#include "ProfilerApi.h"
#include <unordered_map>
class CallGraphBuilder
{
    // TODO delete info
public:
    ProfilerApi* _api;
    void AddFunctionInfo(FunctionID funcId);

    FunctionInfo* GetFunctionInfo(FunctionID funcId);

    std::unordered_map<UINT_PTR, FunctionInfo*> _funcInfos;

    shared_ptr<CppEssentials::TextFileWriter> _writer;

    CallGraphBuilder(const std::wstring& file, ProfilerApi* api);
    void Release();

    std::wstring Format(const wstring& prefix, ThreadID tid, FunctionInfo* info, int numSpaces = 0);


    void OnEnter(FunctionID funcId);

    void OnLeave(FunctionID funcId);

    void OnTailCall(FunctionID funcId);
};

