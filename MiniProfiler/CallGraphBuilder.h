#pragma once
#include <string>
#include "Common/TextFileWriter.h"
#include <corprof.h>
#include "ProfilerApi.h"
#include <unordered_map>

class CallGraphBuilder
{
public:

	CallGraphBuilder(const std::wstring& file, ProfilerApi* api);
	void Release();

	void OnEnter(FunctionID funcId);
	void OnLeave(FunctionID funcId);
	void OnTailCall(FunctionID funcId);
	void OnThreadCreated(ThreadID tid);
	void OnThreadDestroyed(ThreadID tid);
	void AddFunctionInfo(FunctionID funcId);

private:

	FunctionInfo* GetFunctionInfo(FunctionID funcId);


	std::wstring Format(const wstring& prefix, ThreadID tid, FunctionInfo* info = nullptr, int numSpaces = 1);

	std::wstring FormatCreateThread(ThreadID tid);
	std::wstring FormatDestroyThread(ThreadID tid);
private:

	ProfilerApi* _api;
	std::unordered_map<UINT_PTR, FunctionInfo*> _funcInfos;
	shared_ptr<CppEssentials::TextFileWriter> _writer;
};
