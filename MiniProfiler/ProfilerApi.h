#pragma once



#include <string>
#include <corhlpr.h>
#include <corprof.h>



class FunctionInfo
{
public:
    FunctionID _id;
    std::wstring _moduleName;
    std::wstring _funcName;

public:
    bool Hide()
    {
        return _moduleName.find(L"mscorlib.dll") != std::wstring::npos;
    }

    FunctionInfo(FunctionID id, std::wstring moduleName, std::wstring funcName)
    {
        _id = id;
        _moduleName = moduleName;
        _funcName = funcName;

        //OutputDebugString((std::wstring(L"\r\nName:") + funcName).c_str());
        OutputDebugString((std::wstring(L"\r\nModuleName:") + moduleName).c_str());
    }
};

class ProfilerApi
{
public:
    ICorProfilerInfo8* _corProfilerInfo;

    void Release();

    ProfilerApi(ICorProfilerInfo8* profilerInfo);

    FunctionInfo* CreateFunctionInfo(FunctionID funcId);

    ThreadID GetThreadId();

    std::wstring GetModuleName(FunctionID functionId);

    std::wstring GetFunctionName(FunctionID functionId);
};
