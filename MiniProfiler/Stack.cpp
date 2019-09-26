#include "pch.h"
#include "Stack.h"

FunctionCall* Stack::ActiveFunction() const
{
	
	return _activeFunc;
}

FunctionCall* Stack::Push(FunctionInfo* info)
{
	const auto call = new FunctionCall(info);
	_stack.push(call);

	if (_entryFunc == nullptr)
	{
		// First call on this stack
		_entryFunc = call;
	}
	
	_activeFunc = call;

	/*if (_activeFunc == nullptr)
	{
		OutputDebugString(L"\nStack::Push We have no active func");
	}
	else
	{
		OutputDebugString(L"\nStack::Push We have an active func");
	}*/
	
	return call;
}

FunctionCall* Stack::Pop()
{
	// Parent function is active again

	const auto removed = _stack.top();
	_stack.pop();

	if (_stack.empty())
	{
		_activeFunc = nullptr;
		_entryFunc = nullptr;
	}
	else
	{
		_activeFunc = _stack.top();
	}

	/*if (_activeFunc == nullptr)
	{
		OutputDebugString(L"\nStack::Pop We have no active func");
	}
	else
	{
		OutputDebugString(L"\nStack::Pop We have an active func");
	}*/

	return removed;
}
