#pragma once
#include <string>
#include "Common/TextFileWriter.h"
#include <corprof.h>
#include "ProfilerApi.h"
#include <unordered_map>
#include "TextWriterAdapter.h"

class Stack;
class CallGraphBuilder
{

  
public:

    // Takes ownership
	CallGraphBuilder(IProfilerApi* api, ITextWriter * writer);
	void Release();

	void OnEnter(FunctionID funcId);
	void OnLeave(FunctionID funcId);
	void OnTailCall(FunctionID funcId);
	void OnThreadCreated(ThreadID tid);
	void OnThreadDestroyed(ThreadID tid);

    // Ownership stays within this class.
    FunctionInfo* AddFunctionInfo(FunctionID funcId);

    bool IsEmpty(ThreadID tid);

private:

	FunctionInfo* GetFunctionInfo(FunctionID funcId);

    std::unordered_map<ThreadID, Stack*> _threadIdToStack;
	std::wstring Format(const wstring& prefix, ThreadID tid, FunctionInfo* info = nullptr, int numSpaces = 1);

	std::wstring FormatCreateThread(ThreadID tid);
	std::wstring FormatDestroyThread(ThreadID tid);
private:

	IProfilerApi* _api;
	std::unordered_map<UINT_PTR, FunctionInfo*> _funcInfos;
	ITextWriter* _writer;
};
