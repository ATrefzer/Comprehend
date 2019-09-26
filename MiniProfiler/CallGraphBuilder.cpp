#include "pch.h"
#include "CallGraphBuilder.h"
#include "Common/Encodings.h"
#include <stack>
#include <cassert>
#include "Stack.h"

// TODO
// 1. How fast ist it?
// 2. Trigger function
// 3. launcher => Select executable, enter trigger function, specify output file.

FunctionInfo* CallGraphBuilder::AddFunctionInfo(FunctionID funcId)
{

    // TODO If this is the trigger function: Mark it!
    // Enter trigger function starts monitoring. Leave exists it.
    // No second run. After it is done it is done.
	auto info = _api->CreateFunctionInfo(funcId);


	/*bool isHidden = info->_moduleName.find(L"mscorlib.dll") != std::wstring::npos;
	if (isHidden)
	{
		info->SetHidden();
	}*/

	_funcInfos.emplace(info->_id, info);
    return info;
}

FunctionInfo* CallGraphBuilder::GetFunctionInfo(FunctionID funcId)
{
	assert(_funcInfos.find(funcId) != _funcInfos.end());
	return _funcInfos[funcId];
}

CallGraphBuilder::CallGraphBuilder(IProfilerApi* api, ITextWriter * writer)
{
	_api = api;
    _writer = writer;

}

void CallGraphBuilder::Release()
{
	if (_writer != nullptr)
	{
		_writer->Close();
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

std::wstring CallGraphBuilder::Format(const wstring& prefix, ThreadID tid, FunctionInfo* info, int numSpaces)
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


std::wstring CallGraphBuilder::FormatCreateThread(ThreadID tid)
{
	std::wstringstream msg;
	msg << L"\r\n" << tid << L" ";
	msg << L"@create_thread";
	return msg.str();
}

std::wstring CallGraphBuilder::FormatDestroyThread(ThreadID tid)
{
	std::wstringstream msg;
	msg << L"\r\n" << tid << L" ";
	msg << L"@destroy_thread";
	return msg.str();
}


void CallGraphBuilder::OnEnter(FunctionID funcId)
{
	auto tid = _api->GetThreadId();
	assert(_threadIdToStack.find(tid) != _threadIdToStack.end());

	auto info = GetFunctionInfo(funcId);
	assert(info != nullptr);

	auto stack = _threadIdToStack[tid];
	assert(stack != nullptr);

    const auto msg = Format(L"@enter", tid, info, stack->Level());
    _writer->WriteString(msg);

    stack->Push(info);
}


void CallGraphBuilder::OnLeave(FunctionID funcId)
{
	auto info = GetFunctionInfo(funcId);
	auto tid = _api->GetThreadId();

	assert(_threadIdToStack.find(tid) != _threadIdToStack.end());
	auto stack = _threadIdToStack[tid];
	assert(stack != nullptr);

	const auto leaving = stack->Pop();
	assert(leaving->_info->_id == funcId);

    const auto msg = Format(L"@leave", tid, info, stack->Level());
    _writer->WriteString(msg);
}

void CallGraphBuilder::OnTailCall(FunctionID funcId)
{
	auto info = GetFunctionInfo(funcId);
	auto tid = _api->GetThreadId();

	if (!info->IsHidden())
	{
		auto msg = Format(L"TailCall", tid, info);
		_writer->WriteString(msg);
	}
}

void CallGraphBuilder::OnThreadCreated(ThreadID tid)
{
	_writer->WriteString(FormatCreateThread(tid));
	_threadIdToStack[tid] = new Stack();
}

void CallGraphBuilder::OnThreadDestroyed(ThreadID tid)
{
	_writer->WriteString(FormatDestroyThread(tid));

	// Note that the same ThreadID may be reused later.
	auto stack = _threadIdToStack[tid];
	delete stack;

	_threadIdToStack[tid] = nullptr;
}


bool CallGraphBuilder::IsEmpty(ThreadID tid)
{
    auto stack = _threadIdToStack[tid];
    return stack->ActiveFunction() == nullptr;
}