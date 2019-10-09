#pragma once


#include <string>
#include <corhlpr.h>
#include <corprof.h>
#include <strstream>

class FunctionInfo;

class IProfilerApi
{
public:

	virtual void Release() = 0;
	virtual FunctionInfo* CreateFunctionInfo(FunctionID funcId) = 0;
	virtual ThreadID GetThreadId() = 0;
	
	//virtual std::wstring GetModuleName(FunctionID functionId) = 0;*/
};

class FunctionInfo
{
public:
	FunctionID _id;
	std::wstring _moduleName;
	std::wstring _funcName;
	DWORD _attributes;
	std::wstring _typeName;

public:

	bool IsPublic()
	{
		return (_attributes & 6) == 6; // See MethodAttributes in reflection
	}

	std::wstring GetFullName()
	{
		return _moduleName + L"!" + _typeName + L"." + _funcName;
	}

	std::wstring ToString();

	FunctionInfo(FunctionID id, const std::wstring& moduleName, const std::wstring& typeName,
	             const std::wstring& funcName, DWORD attributes)
	{
		_id = id;
		_typeName = typeName;
		_moduleName = moduleName;
		_funcName = funcName;
		_attributes = attributes;
	}
};

class ProfilerApi : public IProfilerApi
{
public:
	ICorProfilerInfo8* _corProfilerInfo;

	void Release() override;

	ProfilerApi(ICorProfilerInfo8* profilerInfo);

	FunctionInfo* CreateFunctionInfo(FunctionID funcId) override;

	ThreadID GetThreadId() override;

	std::wstring GetModuleName(FunctionID functionId);

};
