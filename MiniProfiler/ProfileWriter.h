#pragma once
#include "Common/TextFileWriter.h"
#include <corprof.h>
#include "ProfilerApi.h"
#include <unordered_map>

namespace CppEssentials
{
	class BinaryWriter;
}

class ProfileWriter
{
public:

	// Takes ownership
	ProfileWriter(IProfilerApi* api, CppEssentials::BinaryWriter* writer);
	void Release();

	void OnEnter(FunctionID funcId);
	void OnLeave(FunctionID funcId);
	void OnTailCall(FunctionID funcId);
	void OnThreadCreated(ThreadID tid);
	void OnThreadDestroyed(ThreadID tid);

	void WriteIndexFile(CppEssentials::TextFileWriter& writer);

	// Ownership stays within this class.
	FunctionInfo* AddFunctionInfo(FunctionID funcId);
	void Enable()
	{
		_isEnabled = true;
	}
	void Disable()
	{
		_isEnabled = false;
	}

private:

	FunctionInfo* GetFunctionInfo(FunctionID funcId);

private:

	bool _isEnabled = true;
	IProfilerApi* _api;
	std::unordered_map<UINT_PTR, FunctionInfo*> _funcInfos;
	CppEssentials::BinaryWriter* _writer;

	CRITICAL_SECTION _cs;
};
