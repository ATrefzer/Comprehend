#include "pch.h"
#include "CallGraphExporter.h"
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

FunctionInfo* CallGraphExporter::AddFunctionInfo(FunctionID funcId)
{
	auto info = _api->CreateFunctionInfo(funcId);

	// No duplicates found.
	
	auto result = _funcInfos.emplace(info->_id, info);
	assert(result.second == true);
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

void CallGraphExporter::WriteIndexFile(CppEssentials::TextFileWriter& writer)
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