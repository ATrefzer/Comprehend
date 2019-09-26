#pragma once



#include <string>
#include <corhlpr.h>
#include <corprof.h>
#include <strstream>



class FunctionInfo
{
public:
    FunctionID _id;
    std::wstring _moduleName;
    std::wstring _funcName;
    bool _isHidden;

public:

    bool IsHidden()
    {
        return _isHidden;
    }

    void SetHidden()
    {
        _isHidden = true;
    }
 
    std::wstring ToString();

    FunctionInfo(FunctionID id, std::wstring moduleName, std::wstring funcName)
    {
        _id = id;
        _moduleName = moduleName;
        _funcName = funcName;
        _isHidden = false;

        
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
