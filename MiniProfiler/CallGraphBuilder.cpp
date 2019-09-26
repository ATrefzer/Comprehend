#include "pch.h"
#include "CallGraphBuilder.h"
#include "Common/Encodings.h"
#include <stack>
#include <cassert>
#include "Stack.h"

//#define GENERATE_REFERENCE_OUTPUT 0

// TODO (Algorithm)
// - delete new
// - Write functions at the end of the resulting file and keep the ids in the output. Let file parser resolve the methods.
// - Write @hidded tag only together with @enter / @leave tags if - and only if - there is hidded element on the top of the stack.
//   Nowhere else @hidden is written.
// - Introduce a stack for all calls (hidded / visible). The stack can anser the question: Am I embedded in a hidded call.

// CallFunctionPool!

std::unordered_map<ThreadID, Stack*> _threadIdToStack;

void CallGraphBuilder::AddFunctionInfo(FunctionID funcId)
{
	// TODO read filter file! Remove filtering from FunctionInfo
	// IsHidden(FunctionInfo)
	auto info = _api->CreateFunctionInfo(funcId);


	bool isHidden = info->_moduleName.find(L"mscorlib.dll") != std::wstring::npos;
	if (isHidden)
	{
		info->SetHidden();
	}

	_funcInfos.emplace(info->_id, info);
	//OutputDebugString((L"\r\nNew function: " + info->ToString()).c_str());
}

FunctionInfo* CallGraphBuilder::GetFunctionInfo(FunctionID funcId)
{
	assert(_funcInfos.find(funcId) != _funcInfos.end());
	return _funcInfos[funcId];
}

CallGraphBuilder::CallGraphBuilder(const std::wstring& file, ProfilerApi* api)
{
	_api = api;
	_writer = std::make_shared<CppEssentials::TextFileWriter>();
	_writer->Open(file, CppEssentials::FileOpenMode::CreateNew, CppEssentials::UTF16LittleEndianEncoder());
}

void CallGraphBuilder::Release()
{
	if (_writer != nullptr)
	{
		_writer->Close();
		_writer = nullptr;
	}

	if (_api != nullptr)
	{
		_api->Release();
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

#ifdef GENERATE_REFERENCE_OUTPUT
	_writer->WriteString(L"\r\n@enter " + info->ToString());
	return;
#endif

	auto stack = _threadIdToStack[tid];
	assert(stack != nullptr);

	auto active = stack->ActiveFunction();

	OutputDebugString(L"\n-------------------");
	if (active == nullptr)
	{
		OutputDebugString(L"\nNo active function!");
	}
	else
	{
		OutputDebugString(L"\nActive function: ");
		
		
		if (active->IsHidden())
		{
			OutputDebugString(L" -> hidden => ");
			OutputDebugString(active->_info->_funcName.c_str());
		}
		else
		{
			OutputDebugString(L" -> visible => ");
			OutputDebugString(active->_info->_funcName.c_str());
		}
	}

	if (info->IsHidden())
	{
			OutputDebugString(L"\nNew Func -> hidden => ");
			OutputDebugString(info->_funcName.c_str());
	}
		else
	{
			OutputDebugString(L"\nNew Func -> visible => ");
			OutputDebugString(info->_funcName.c_str());
	}
	
	
	if (!info->IsHidden())
	{
		if (active == nullptr || (active != nullptr && active->IsHidden()))
		{
			OutputDebugString(L"\nTransition from hidden to visible");
			// Write @enter_hidden when we transition from hidden to a visible element.
			// This is to compact multiple hidden function calls into a single one.
			_writer->WriteString(Format(L"@enter_hidden", tid));
		}

		_writer->WriteString(Format(L"@enter", tid, info));
	}

	stack->Push(info);
}


void CallGraphBuilder::OnLeave(FunctionID funcId)
{
	auto info = GetFunctionInfo(funcId);
	auto tid = _api->GetThreadId();

	assert(_threadIdToStack.find(tid) != _threadIdToStack.end());
	auto stack = _threadIdToStack[tid];
	assert(stack != nullptr);

#ifdef GENERATE_REFERENCE_OUTPUT
	_writer->WriteString(L"\r\n@leave " + info->ToString());
	return;
#endif

	// Back to caller
	const auto leaving = stack->Pop();
	assert(leaving->_info->_id == funcId);

	auto newActive = stack->ActiveFunction();

	if (!info->IsHidden())
	{
		const auto msg = Format(L"@leave", tid, info);
		_writer->WriteString(msg);
	}
	else
	{
		// Write @leave_hidden when we transition from hidden to non hidden function call.
		// This is to compact multiple hidden function calls into a single one.
		if (leaving->IsHidden() && ((newActive != nullptr && !newActive->IsHidden())) || newActive == nullptr)
		{
			auto msg = Format(L"@leave_hidden", tid);
			_writer->WriteString(msg);
		}
	}
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
