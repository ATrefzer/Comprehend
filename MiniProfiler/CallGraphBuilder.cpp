#include "pch.h"
#include "CallGraphExporter.h"
#include "Common/Encodings.h"
#include "Common/BinaryWriter.h"
#include <stack>
#include <cassert>
#include "Stack.h"

FunctionInfo* CallGraphExporter::AddFunctionInfo(FunctionID funcId)
{
	auto info = _api->CreateFunctionInfo(funcId);

	_funcInfos.emplace(info->_id, info);
    return info;
}

FunctionInfo* CallGraphExporter::GetFunctionInfo(FunctionID funcId)
{
	assert(_funcInfos.find(funcId) != _funcInfos.end());
	return _funcInfos[funcId];
}

CallGraphExporter::CallGraphExporter(IProfilerApi* api, CppEssentials::BinaryWriter * writer)
{
	_api = api;
    _writer = writer;

}

void CallGraphExporter::Release()
{
	_api = nullptr;
	_writer = nullptr;
	
}

std::wstring CallGraphExporter::Format(const wstring& prefix, ThreadID tid, FunctionInfo* info, int numSpaces)
{
	const auto spaces = std::wstring(numSpaces, ' ');

	//<tid><space><prefix><space><funcName>
	std::wstringstream msg;
	msg << L"\r\n" << tid;
	msg << spaces;
	msg << prefix;

	if (info != nullptr)
	{
		msg << L" ";
		msg << info->_funcName;
	}
	return msg.str();
}

std::wstring CallGraphExporter::FormatCompact(const wstring& prefix, ThreadID tid, FunctionInfo* info)
{
	//<tid><space><prefix><space><funcName>
	std::wstringstream msg;
	msg << L"\r\n" << tid;
	msg << L" ";
	msg << prefix;

	if (info != nullptr)
	{
		msg << L" ";
		msg << info->_funcName;
	}
	return msg.str();
}


std::wstring CallGraphExporter::FormatCreateThread(ThreadID tid)
{
	std::wstringstream msg;
	msg << L"\r\n" << tid << L" ";
	msg << L"@create_thread";
	return msg.str();
}

std::wstring CallGraphExporter::FormatDestroyThread(ThreadID tid)
{
	std::wstringstream msg;
	msg << L"\r\n" << tid << L" ";
	msg << L"@destroy_thread";
	return msg.str();
}


void CallGraphExporter::OnEnter(FunctionID funcId)
{
	auto tid = _api->GetThreadId();
	assert(_threadIdToStack.find(tid) != _threadIdToStack.end());

	auto info = GetFunctionInfo(funcId);
	assert(info != nullptr);

	/*auto stack = _threadIdToStack[tid];
	assert(stack != nullptr);*/

    const auto msg = FormatCompact(L"@enter", tid, info);
    _writer->WriteString(msg);

    //stack->Push(info);
}


void CallGraphExporter::OnLeave(FunctionID funcId)
{
	auto info = GetFunctionInfo(funcId);
	auto tid = _api->GetThreadId();

	/*assert(_threadIdToStack.find(tid) != _threadIdToStack.end());
	auto stack = _threadIdToStack[tid];
	assert(stack != nullptr);*/

	//const auto leaving = stack->Pop();

	// It is possible that we find the same function name with different ids.
	/*if (leaving->_info->_id == funcId)
	{
		OutputDebugString(L"\nError: Leaving: ");
		OutputDebugString(_funcInfos[funcId]->_funcName.c_str());
		OutputDebugString(L"\nbut should be");
		OutputDebugString(leaving->_info->_funcName.c_str());
	}*/

    const auto msg = FormatCompact(L"@leave", tid, info);
    _writer->WriteString(msg);
}

void CallGraphExporter::OnTailCall(FunctionID funcId)
{
	auto info = GetFunctionInfo(funcId);
	auto tid = _api->GetThreadId();

	if (!info->IsHidden())
	{
		auto msg = Format(L"TailCall", tid, info);
		_writer->WriteString(msg);
	}
}

void CallGraphExporter::OnThreadCreated(ThreadID tid)
{
	_writer->WriteString(FormatCreateThread(tid));
	//_threadIdToStack[tid] = new Stack();
}

void CallGraphExporter::OnThreadDestroyed(ThreadID tid)
{
	_writer->WriteString(FormatDestroyThread(tid));

	// Note that the same ThreadID may be reused later.
	/*auto stack = _threadIdToStack[tid];
	delete stack;

	_threadIdToStack[tid] = nullptr;
	*/
}

//
//bool CallGraphExporter::IsEmpty(ThreadID tid)
//{
//    auto stack = _threadIdToStack[tid];
//    return stack->ActiveFunction() == nullptr;
//}