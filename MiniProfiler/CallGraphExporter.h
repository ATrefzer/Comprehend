#pragma once
#include "Common/TextFileWriter.h"
#include <corprof.h>
#include "ProfilerApi.h"
#include <unordered_map>

namespace CppEssentials {
	class BinaryWriter;
}

class TraceExporter
{
	CRITICAL_SECTION _cs;
  
public:

    // Takes ownership
	TraceExporter(IProfilerApi* api, CppEssentials::BinaryWriter * writer);
	void Release();

	virtual ~TraceExporter();

	void OnEnter(FunctionID funcId);
	void OnLeave(FunctionID funcId);
	void OnTailCall(FunctionID funcId);
	void OnThreadCreated(ThreadID tid);
	void OnThreadDestroyed(ThreadID tid);

    void WriteIndexFile(CppEssentials::TextFileWriter& writer);

    // Ownership stays within this class.
    FunctionInfo* AddFunctionInfo(FunctionID funcId);

   // bool IsEmpty(ThreadID tid);

private:

	FunctionInfo* GetFunctionInfo(FunctionID funcId);

	// 
    //std::unordered_map<ThreadID, Stack*> _threadIdToStack;
	
private:

	IProfilerApi* _api;
	std::unordered_map<UINT_PTR, FunctionInfo*> _funcInfos;
    CppEssentials::BinaryWriter* _writer;
};
