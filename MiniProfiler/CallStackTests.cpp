#include "pch.h"
#include "ProfileWriter.h"
#include <cassert>
#include "CallStackTests.h"
#include "ProfilerApi.h"


class ProfilerApiMock : public IProfilerApi
{
	// Inherited via IProfilerApi
	void Release() override
	{
	}

	FunctionInfo* CreateFunctionInfo(FunctionID funcId) override
	{
		return new FunctionInfo(funcId, L"", L"");
	}

	ThreadID GetThreadId() override
	{
		return 0;
	}

	std::wstring GetModuleName(FunctionID functionId) override
	{
		return std::wstring(L"ModuleName_") + std::to_wstring(functionId);
	}

	std::wstring GetFunctionName(FunctionID functionId) override
	{
		return std::wstring(L"FunctionName_") + std::to_wstring(functionId);
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
}
