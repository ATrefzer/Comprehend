#include "pch.h"
#include "ProfileWriter.h"
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

FunctionInfo* ProfileWriter::AddFunctionInfo(FunctionID funcId)
{
	auto info = _api->CreateFunctionInfo(funcId);

	// TOD
	// No duplicates found.

	EnterCriticalSection(&_cs);
	auto result = _funcInfos.emplace(info->_id, info);
	assert(result.second == true);
	LeaveCriticalSection(&_cs);

	return info;
}

FunctionInfo* ProfileWriter::GetFunctionInfo(FunctionID funcId)
{
	EnterCriticalSection(&_cs);
	assert(_funcInfos.find(funcId) != _funcInfos.end());
	auto info = _funcInfos[funcId];
	LeaveCriticalSection(&_cs);
	return info;
}

ProfileWriter::ProfileWriter(IProfilerApi* api, BinaryWriter* writer)
{
	_api = api;
	_writer = writer;
	InitializeCriticalSection(&_cs);
}

void ProfileWriter::Release()
{
	DeleteCriticalSection(&_cs);
}


void ProfileWriter::OnEnter(FunctionID funcId)
{
	if (!_isEnabled)
	{
		return;
	}
	
	auto tid = _api->GetThreadId();

	EnterCriticalSection(&_cs);
	_writer->WriteUInt16(TokenEnter);
	_writer->WriteUInt64(tid);
	_writer->WriteUInt64(funcId);
	LeaveCriticalSection(&_cs);
}

void ProfileWriter::OnLeave(FunctionID funcId)
{
	// It is possible that we find the same function name with different ids(!)

	if (!_isEnabled)
	{
		return;
	}
	

	auto tid = _api->GetThreadId();

	EnterCriticalSection(&_cs);
	_writer->WriteUInt16(TokenLeave);
	_writer->WriteUInt64(tid);
	_writer->WriteUInt64(funcId);
	LeaveCriticalSection(&_cs);
}

void ProfileWriter::OnTailCall(FunctionID funcId)
{
	if (!_isEnabled)
	{
		return;
	}
	
	auto tid = _api->GetThreadId();

	EnterCriticalSection(&_cs);
	_writer->WriteUInt16(TokenTailCall);
	_writer->WriteUInt64(tid);
	_writer->WriteUInt64(funcId);
	LeaveCriticalSection(&_cs);
}

void ProfileWriter::OnThreadCreated(ThreadID tid)
{
	if (!_isEnabled)
	{
		return;
	}
	
	EnterCriticalSection(&_cs);
	_writer->WriteUInt16(TokenCreateThread);
	_writer->WriteUInt64(tid);
	LeaveCriticalSection(&_cs);
}

void ProfileWriter::OnThreadDestroyed(ThreadID tid)
{
	if (!_isEnabled)
	{
		return;
	}
	
	// Note that the same ThreadID may be reused later.
	EnterCriticalSection(&_cs);
	_writer->WriteUInt16(TokenDestroyThread);
	_writer->WriteUInt64(tid);
	LeaveCriticalSection(&_cs);
}

void ProfileWriter::WriteIndexFile(TextFileWriter& writer)
{
	// Independent of enable state!
	for (auto iter = _funcInfos.begin(); iter != _funcInfos.end(); ++iter)
	{
		if (iter != _funcInfos.begin())
		{
			writer.WriteString(L"\r\n");
		}

		auto func = iter->second;

		writer.WriteString(std::to_wstring(func->_id));
		writer.WriteString(L"\t");
		writer.WriteString(func->GetFullName());
		writer.WriteString(L"\t");
		writer.WriteString(func->IsPublic() == true ? L"+" : L"-");
	}
}
