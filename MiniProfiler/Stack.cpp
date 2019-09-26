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
    _level++;

	if (_entryFunc == nullptr)
	{
		// First call on this stack
		_entryFunc = call;
	}
	
	_activeFunc = call;

	
	return call;
}

FunctionCall* Stack::Pop()
{
	// Parent function is active again

	const auto removed = _stack.top();
	_stack.pop();
    _level--;

	if (_stack.empty())
	{
		_activeFunc = nullptr;
		_entryFunc = nullptr;
	}
	else
	{
		_activeFunc = _stack.top();
	}



	return removed;
}
