#include "pch.h"
#include "CallGraphExporter.h"
#include "Common/BinaryWriter.h"
#include <cassert>

// TODO
// 1. How fast ist it?
// 2. Trigger function
// 3. launcher => Select executable, enter trigger function, specify output file.


enum Tokens
{
	TokenCreateThread,
	TokenDestroyThread,
	TokenEnter,
	TokenLeave,
	TokenTailCall,

	// After this token the function dictionary starts
	Terminate
};

FunctionInfo* CallGraphExporter::AddFunctionInfo(FunctionID funcId)
{
	auto info = _api->CreateFunctionInfo(funcId);

	/*bool isHidden = info->_moduleName.find(L"mscorlib.dll") != std::wstring::npos;
	if (isHidden)
	{
		info->SetHidden();
	}*/

	_funcInfos.emplace(info->_id, info);
	return info;
}

FunctionInfo* CallGraphExporter::GetFunctionInfo(FunctionID funcId)
{
	assert(_funcInfos.find(funcId) != _funcInfos.end());
	return _funcInfos[funcId];
}

CallGraphExporter::CallGraphExporter(IProfilerApi* api, CppEssentials::BinaryWriter* writer)
{
	_api = api;
	_writer = writer;
}

void CallGraphExporter::Release()
{
	if (_writer != nullptr)
	{
		_writer->Flush();

		// Closes the inner file
		delete _writer;
		_writer = nullptr;
	}

	if (_api != nullptr)
	{
		_api->Release();
		delete _api;
		_api = nullptr;
	}
}


void CallGraphExporter::OnEnter(FunctionID funcId)
{
	auto tid = _api->GetThreadId();

	_writer->WriteUInt16(TokenEnter);
	_writer->WriteUInt64(tid);
	_writer->WriteUInt64(funcId);
}

void CallGraphExporter::OnLeave(FunctionID funcId)
{
	// It is possible that we find the same function name with different ids(!)

	auto info = GetFunctionInfo(funcId);
	auto tid = _api->GetThreadId();


	_writer->WriteUInt16(TokenLeave);
	_writer->WriteUInt64(tid);
	_writer->WriteUInt64(funcId);
}

void CallGraphExporter::OnTailCall(FunctionID funcId)
{
	auto tid = _api->GetThreadId();

	_writer->WriteUInt16(TokenTailCall);
	_writer->WriteUInt64(tid);
	_writer->WriteUInt64(funcId);
}

void CallGraphExporter::OnThreadCreated(ThreadID tid)
{
	_writer->WriteUInt16(TokenCreateThread);
	_writer->WriteUInt64(tid);
}

void CallGraphExporter::OnThreadDestroyed(ThreadID tid)
{
	// Note that the same ThreadID may be reused later.

	_writer->WriteUInt16(TokenDestroyThread);
	_writer->WriteUInt64(tid);
}
