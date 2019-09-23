#include "pch.h"
#include "ProfilerApi.h"
#include "Common/FilePath.h"

void ProfilerApi::Release()
{
    if (_corProfilerInfo != nullptr)
    {
        _corProfilerInfo->Release();
        _corProfilerInfo = nullptr;
    }


}

ProfilerApi::ProfilerApi(ICorProfilerInfo8* profilerInfo) : _corProfilerInfo(nullptr)
{
    _corProfilerInfo = profilerInfo;


}

FunctionInfo* ProfilerApi::CreateFunctionInfo(FunctionID funcId)
{
    
    std::wstring moduleName = GetModuleName(funcId);
    std::wstring funcName = GetFunctionName(funcId);
    auto parts = CppEssentials::FilePath::Split(moduleName);
    return new FunctionInfo(funcId, parts._name, funcName);
}

ThreadID ProfilerApi::GetThreadId()
{
    ThreadID id;
    _corProfilerInfo->GetCurrentThreadID(&id);

    return id;
}

std::wstring ProfilerApi::GetModuleName(FunctionID functionId)
{
    ClassID classId;
    ModuleID moduleId;
    mdToken functionToken;
    _corProfilerInfo->GetFunctionInfo(functionId, &classId, &moduleId, &functionToken);

    wchar_t name[1000];
    auto numChars = sizeof(name) / sizeof(wchar_t);
    _corProfilerInfo->GetModuleInfo(moduleId, NULL, numChars, nullptr, name, NULL);
    return std::wstring(name);
}

std::wstring ProfilerApi::GetFunctionName(FunctionID functionId)
{


    wchar_t funcName[4000];
    wchar_t typeName[4000];


    mdToken functionToken = mdTypeDefNil;
    mdTypeDef classToken = mdTypeDefNil;
    IMetaDataImport* pMDImport = nullptr;
    _corProfilerInfo->GetTokenAndMetaDataFromFunction(functionId,
        IID_IMetaDataImport, reinterpret_cast<IUnknown * *>(&pMDImport),
        &functionToken);

    auto numChars = sizeof(funcName) / sizeof(wchar_t);
    pMDImport->GetMethodProps(functionToken,
        &classToken,
        funcName, numChars,
        nullptr, nullptr, nullptr, nullptr, nullptr, nullptr);

    numChars = sizeof(typeName) / sizeof(wchar_t);
    pMDImport->GetTypeDefProps(classToken, typeName, numChars, nullptr, nullptr, nullptr);


    return std::wstring(typeName) + std::wstring(L".") + std::wstring(funcName);
}
