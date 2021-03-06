#include "pch.h"
#include "TraceExporter.h"
#include "Common/BinaryWriter.h"
#include <cassert>
using namespace CppEssentials;

enum Tokens
{
	TokenCreateThread,
	TokenDestroyThread,
	TokenEnter,
	TokenLeave,
	TokenTailCall,
};

FunctionInfo* TraceExporter::AddFunctionInfo(FunctionID funcId)
{
	auto info = _api->CreateFunctionInfo(funcId);

	// No duplicates found.

	::EnterCriticalSection(&_cs);
	auto result = _funcInfos.emplace(info->_id, info);
	assert(result.second == true);
	::LeaveCriticalSection(&_cs);
	
	return info;
}

FunctionInfo* TraceExporter::GetFunctionInfo(FunctionID funcId)
{
	::EnterCriticalSection(&_cs);
	assert(_funcInfos.find(funcId) != _funcInfos.end());	
	auto info =  _funcInfos[funcId];
	::LeaveCriticalSection(&_cs);
	return info;
}

TraceExporter::TraceExporter(IProfilerApi* api, CppEssentials::BinaryWriter* writer)
{
	_api = api;
	_writer = writer;
	::InitializeCriticalSection(&_cs);
}

void TraceExporter::Release()
{
	::DeleteCriticalSection(&_cs);
}

TraceExporter::~TraceExporter()
{
	
}

void TraceExporter::OnEnter(FunctionID funcId)
{
	auto tid = _api->GetThreadId();

	::EnterCriticalSection(&_cs);
	_writer->WriteUInt16(TokenEnter);
	_writer->WriteUInt64(tid);
	_writer->WriteUInt64(funcId);
	::LeaveCriticalSection(&_cs);
}

void TraceExporter::OnLeave(FunctionID funcId)
{
	// It is possible that we find the same function name with different ids(!)

	auto tid = _api->GetThreadId();

	::EnterCriticalSection(&_cs);
	_writer->WriteUInt16(TokenLeave);
	_writer->WriteUInt64(tid);
	_writer->WriteUInt64(funcId);
	::LeaveCriticalSection(&_cs);
}

void TraceExporter::OnTailCall(FunctionID funcId)
{
	auto tid = _api->GetThreadId();

	::EnterCriticalSection(&_cs);
	_writer->WriteUInt16(TokenTailCall);
	_writer->WriteUInt64(tid);
	_writer->WriteUInt64(funcId);
	::LeaveCriticalSection(&_cs);
}

void TraceExporter::OnThreadCreated(ThreadID tid)
{
	::EnterCriticalSection(&_cs);
	_writer->WriteUInt16(TokenCreateThread);
	_writer->WriteUInt64(tid);
	::LeaveCriticalSection(&_cs);
}

void TraceExporter::OnThreadDestroyed(ThreadID tid)
{
	// Note that the same ThreadID may be reused later.
	::EnterCriticalSection(&_cs);
	_writer->WriteUInt16(TokenDestroyThread);
	_writer->WriteUInt64(tid);
	::LeaveCriticalSection(&_cs);
}

void TraceExporter::WriteIndexFile(CppEssentials::TextFileWriter& writer)
{
    for (auto iter = _funcInfos.begin(); iter != _funcInfos.end(); ++iter)
    {
        if (iter != _funcInfos.begin())
        {
            writer.WriteString(L"\r\n");
        }

        auto func = iter->second;
        writer.WriteString(std::to_wstring(func->_id));
        writer.WriteString(L" ");
    	writer.WriteString(func->_moduleName);
    	writer.WriteString(L"!");
        writer.WriteString(func->_funcName);
    }
}